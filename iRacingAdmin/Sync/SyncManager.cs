using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Alchemy;
using Alchemy.Classes;
using iRacingAdmin.Classes;
using iRacingAdmin.Models.Admins;
using iRacingAdmin.Models.Drivers;
using iRacingAdmin.Sync.Penalties;
using iRacingAdmin.Views;
using iRacingSimulator;
using Newtonsoft.Json;

namespace iRacingAdmin.Sync
{
    public class SyncManager
    {
        private WebSocketClient _client;
        private User _user;
        private User _offlineUser;
        private UserContainerCollection _users; 
        
        private string _name, _shortname, _password;
        private int _custid;
        private long _subSessionId;
        private string _address;
        
        private SyncManager(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;

            _offlineUser = new User();
            _offlineUser.IsRegistered = true;
            _offlineUser.CustId = 0;
            _offlineUser.Name = "Local";
            _offlineUser.ShortName = "L";
            _offlineUser.UserColor = UserColor.Offline;
            _offlineUser.IsHost = true;

            _status = ConnectionStatus.Disconnected;
            _state = new SyncState(0);
            _users = new UserContainerCollection();

            _users.Add(new UserContainer(_offlineUser));
        }

        private static SyncManager _instance;
        public static SyncManager Instance
        {
            get { return _instance; }
        }

        public static void Initialize(Dispatcher dispatcher)
        {
            _instance = new SyncManager(dispatcher);
        }

        private readonly Dispatcher _dispatcher;
        public Dispatcher Dispatcher { get { return _dispatcher; } }

        private ConnectionStatus _status;
        public ConnectionStatus Status { get { return _status; } }

        private SyncState _state;
        public SyncState State { get { return _state; } }

        public string Address { get { return _address; } }

        public User User
        {
            get
            {
                if (_user == null) return _offlineUser;
                return _user;
            }
        }

        public int UserId { get { return this.User.CustId; } }

        public UserContainerCollection Users
        {
            get { return _users; }
        }

        public UserContainer FindUser(int custid)
        {
            return _users.SingleOrDefault(u => u.User.CustId == custid);
        }

        #region Connection

        public Task<ConnectionResult> Connect(string ip, int port, int custid, string username, string shortname, string password)
        {
            _name = username;
            _shortname = shortname;
            _password = password;
            _custid = custid;
            _subSessionId = Connection.Instance.SubSessionId.GetValueOrDefault();

            _state = new SyncState(_subSessionId);

            _address = string.Format("{0}:{1}", ip, port);
            string path = string.Format("ws://{0}/channel", _address);

            var tcs = new TaskCompletionSource<ConnectionResult>();

            try
            {
                // Create new websocket
                _client = new WebSocketClient(path);
                _client.ConnectTimeout = new TimeSpan(0, 10, 0);

                // Handle connection events
                _client.OnConnected += OnConnected;
                _client.OnDisconnect += context => OnServerDisconnected();

                // Handle received event
                _client.OnReceive += context => OnReceive(context, tcs);
                
                // Connect and wait for connected event
                _client.Connect();

            }
            catch (Exception ex)
            {
                this.ResetStatus();

                tcs.SetResult(new ConnectionResult
                    {
                        Success = false,
                        Message = "Unknown error during connection: " + ex.Message
                    });
            }

            return tcs.Task;
        }

        private void OnServerDisconnected()
        {
            this.HandleDisconnect("Server connection lost.");
        }

        public void Disconnect()
        {
            try
            {
                if (_client != null) _client.Disconnect();
            }
            finally
            {
                _client = null;
            }
            
            this.ResetStatus();
            this.OnDisconnected();
        }

        private void OnConnected(UserContext context)
        {
            // Connected - send registration command
            var command = new SyncCommand(SyncCommand.CommandTypes.Register);
            command.Data = new
            {
                Id = _custid,
                Name = _name,
                ShortName = _shortname,
                Password = _password,
                Ssid = _subSessionId
            };

            _client.Send(command.ToString());
        }

        #endregion

        #region Server response
        
        private void OnReceive(UserContext context, TaskCompletionSource<ConnectionResult> tcs)
        {
            var value = context.DataFrame.ToString();
            var response = SyncMessage.FromString<SyncResponse>(value);

            Debug.WriteLine(">> Received server response: " + value);

            if (response != null)
            {
                // Handle response
                switch (response.Type)
                {
                    case SyncResponse.ResponseTypes.Connect:
                        HandleRegistration(context, response, tcs);
                        break;

                    case SyncResponse.ResponseTypes.Disconnect:
                        HandleServerDisconnect(response);
                        break;

                    case SyncResponse.ResponseTypes.Ping:
                        HandlePing();
                        break;

                    case SyncResponse.ResponseTypes.UpdateState:
                        HandleStateUpdate(response);
                        break;

                    case SyncResponse.ResponseTypes.UserList:
                        HandleUserlist(response);
                        break;

                    case SyncResponse.ResponseTypes.ProposeSyncCameras:
                        HandleCameraSyncProposal(response);
                        break;
                }
            }
            else
            {
                Debug.WriteLine(">> Failed to understand response: " + value);
                // Cannot understand response
                //this.ResetStatus();
                //if (!tcs.Task.IsCompleted) tcs.SetResult(new ConnectionResult("Unknown response"));
            }
        }

        private void HandleRegistration(UserContext context, SyncResponse response, TaskCompletionSource<ConnectionResult> tcs)
        {
            if (response.Data.Success.Value)
            {
                // Registered successfully
                _status = ConnectionStatus.Connected;

                this.RaiseConnected();

                _user = (User) JsonConvert.DeserializeObject<User>(response.Data.User.ToString());
                //_user.Context = context;

                tcs.SetResult(new ConnectionResult());
            }
            else
            {
                // Failed
                this.ResetStatus();
                tcs.SetResult(new ConnectionResult(response.Data.Message.Value));
            }
        }

        private void HandlePing()
        {
            var command = SyncCommandHelper.Pong();
            this.SendCommand(command);
        }

        private void HandleUserlist(SyncResponse response)
        {
            var users = (List<User>) JsonConvert.DeserializeObject<List<User>>(response.Data.Users.ToString());
            _dispatcher.Invoke(() => this.UpdateUsers(users));
        }

        private void HandleStateUpdate(SyncResponse response)
        {
            _state = (SyncState) JsonConvert.DeserializeObject<SyncState>(response.Data.State.ToString());
            Debug.WriteLine(">> Received state update: " + _state);

            // Save state
            SaveState(_state);

            // Copy settings that come from the server
            OfftrackHistory.OfftrackLimit = _state.OfftrackLimit;

            this.OnStateUpdated();
        }

        private void HandleCameraSyncProposal(SyncResponse response)
        {
            // Other admin proposed YOU to sync to HIS camera
            try
            {
                var camera =
                    (CameraDetails)
                        JsonConvert.DeserializeObject<CameraDetails>(response.Data.CameraDetails.ToString());
                var adminId = (int) response.Data.ProposingAdmin;
                var admin = SyncManager.Instance.Users.FromId(adminId);

                // Invoke on dispatcher to not block response handling thread
                App.Instance.Dispatcher.Invoke(() =>
                {
                    if ((bool) response.Data.Force 
                        || App.Instance.MainModel.ShowDialog(new ConfirmCameraSyncWindow(admin, camera)).GetValueOrDefault())
                    {
                        CameraControl.ChangeCamera(camera);
                    }
                });
            }
            catch (Exception ex)
            {
                App.Instance.LogError("Handling Camera Sync. Response: " + response, ex);
            }
        }

        private void HandleServerDisconnect(SyncResponse response)
        {
            string reason = "Unknown reason.";
            if (response.Data != null && response.Data.Reason != null)
            {
                reason = (string) response.Data.Reason;
            }
            this.HandleDisconnect(reason);
        }

        private void HandleDisconnect(string reason)
        {
            this.Disconnect();
            App.Instance.Dispatcher.Invoke(() =>
                  MessageWindow.Show("Disconnected",
                      "You have been disconnected from the server:\n\n" + reason,
                        MessageBoxButton.OK));
        }

        #endregion

        #region Server commands

        public void SendStateUpdate(SyncStateCommand command)
        {
            Debug.WriteLine(">> Sending state update: " + command);
            this.SendCommand(command);
            this.OnStateUpdated();
        }

        public void SendPenaltyUpdate(Penalty penalty)
        {
            var command = SyncCommandHelper.EditPenalty(penalty);
            Debug.WriteLine(">> Sending penalty update: " + command);
            this.SendCommand(command);
        }

        public void SendPenaltyDelete(Penalty penalty)
        {
            var command = SyncCommandHelper.DeletePenalty(penalty);
            Debug.WriteLine(">> Sending penalty update: " + command);
            this.SendCommand(command);
        }

        public async void SendCommand(SyncCommand command)
        {
            if (this.Status == ConnectionStatus.Connected && this.User != null)
            {
                try
                {
                    await Task.Run(() => _client.Send(command.ToString()));
                }
                catch (Exception) { }
            }
        }

        #endregion

        private void UpdateUsers(IEnumerable<User> users)
        {
            var newUsers = new List<UserContainer>();
            foreach (var user in users)
            {
                var container = _users.FromId(user.CustId);
                if (container == null)
                {
                    container = new UserContainer(user);
                }
                else
                {
                    container.User = user;
                }

                newUsers.Add(container);
            }

            _users.Clear();

            if (this.Status == ConnectionStatus.Connected)
            { 
                foreach (var user in newUsers)
                {
                    _users.Add(user);
                }
            }
            else
            {
                _users.Add(new UserContainer(_offlineUser));
            }

            this.OnUsersUpdated();
        }

        #region Save / load

        public static void SaveState(SyncState state)
        {
            try
            {
                var json = JsonConvert.SerializeObject(state);
                var path = GetStateFilepath(state.SubsessionId);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
            }
        }

        public static SyncState LoadState(long ssid)
        {
            try
            {
                var path = GetStateFilepath(ssid);
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    var state = (SyncState)JsonConvert.DeserializeObject<SyncState>(json);

                    // Disconnect all users
                    foreach (var user in state.Users)
                    {
                        user.IsConnected = false;
                        user.IsRegistered = false;
                        user.IsHost = false;
                    }

                    return state;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string GetStateFilepath(long ssid)
        {
            var path = Path.Combine(Paths.SettingsPath, "states");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return Path.Combine(path, string.Format("state_" + ssid + ".json"));
        }

        #endregion

        private void ResetStatus()
        {
            _name = string.Empty;
            _password = string.Empty;
            _status = ConnectionStatus.Disconnected;
            _user = null;
            _dispatcher.Invoke(() =>  _users.Clear());
        }
        
        public event EventHandler Connected;
        protected virtual void RaiseConnected()
        {
            if (this.Connected != null)
            {
                this.Connected(this, EventArgs.Empty);
            }
        }

        public event EventHandler StateUpdated;
        protected virtual void OnStateUpdated()
        {
            if (this.StateUpdated != null)
            {
                this.StateUpdated(this, EventArgs.Empty);
            }
        }

        public event EventHandler UsersUpdated;
        protected virtual void OnUsersUpdated()
        {
            if (this.UsersUpdated != null)
            {
                this.UsersUpdated(this, EventArgs.Empty);
            }
        }

        public event EventHandler Disconnected;
        protected virtual void OnDisconnected()
        {
            if (this.Disconnected != null)
            {
                this.Disconnected(this, EventArgs.Empty);
            }
        }

        public enum ConnectionStatus
        {
            Disconnected = 0,
            Connected = 1
        }
    }
}
