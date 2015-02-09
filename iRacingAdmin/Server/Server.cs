using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using Alchemy;
using Alchemy.Classes;
using iRacingAdmin.Models.Drivers;
using iRacingAdmin.Models.Server;
using iRacingAdmin.Properties;
using iRacingAdmin.Sync;
using iRacingAdmin.Sync.Penalties;
using iRacingSimulator;

namespace iRacingAdmin.Server
{
    public class Server
    {
        private const int MIN_NAME_LENGTH = 1;
        private TimeSpan MAX_PING_TIMEOUT = TimeSpan.FromSeconds(10);
        private TimeSpan PING_INTERVAL = TimeSpan.FromSeconds(200000);
        private TimeSpan MESSAGE_INTERVAL = TimeSpan.FromMilliseconds(20);

        private WebSocketServer _server;
        private SyncState _state;
        private string _password;

        private Thread _messageThread;
        private Thread _pingThread;
        private Thread _webThread;
        private Queue<UserContext> _messageQueue; 

        private int _adminId;
        private long _subSessionId;

        private UserConnectionList _connections = new UserConnectionList(true);
        //private UserList _users = new UserList(true);

        private Server()
        {
            _messageQueue = new Queue<UserContext>();

            if (!Connection.Instance.IsRunning)
            {
                Connection.Instance.Start();
            }
        }

        private static Server _instance;
        public static Server Instance
        {
            get { return _instance ?? (_instance = new Server()); }
        }

        private ServerStatus _status;

        public ServerStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                this.OnStatusChanged();
            }
        }

        public SyncState State
        {
            get { return _state; }
        }

        public List<User> Users
        {
            get { return State.Users.ToList(); }
        }

        public int MessageQueueSize { get { return _messageQueue.Count; } }

        private ServerInfo _localServerInfo;
        public ServerInfo LocalServerInfo { get { return _localServerInfo; } }

        public void Initialize(int port, string password, int adminId, long ssid)
        {
            this.Stop();

            // Clear the message handling queue
            _messageQueue.Clear();
            this.OnMessageQueueUpdated();

            // Find existing state
            var existing = LoadState(ssid);
            
            _state = existing ?? new SyncState(ssid);

            _password = password;
            _adminId = adminId;
            _subSessionId = ssid;

            // Start new server
            _server = new WebSocketServer(port, IPAddress.Any)
            {
                OnReceive = OnReceive,
                OnConnected = OnConnected,
                OnDisconnect = OnDisconnect,
                TimeOut = new TimeSpan(0, 5, 0)
            };

            _server.Start();
            this.Status = ServerStatus.Active;

            _localServerInfo = ServerInfo.Local(port, password);

            // Start message queue handling thread
            _messageThread = new Thread(HandleMessageQueue);
            _messageThread.IsBackground = false;
            _messageThread.Start();

            // Start ping loop
            _pingThread = new Thread(PingLoop);
            _pingThread.IsBackground = true;
            _pingThread.Start();

            // Start web-update thread
            _webThread = new Thread(WebUpdateLoop);
            _webThread.IsBackground = true;
            _webThread.Start();
        }

        public void Stop()
        {
            // Stop server
            if (_server != null)
            {
                // Save state
                this.SaveState();

                // TODO: Disconnect everyone
                this.BroadcastDisconnect();
                foreach (var conn in _connections)
                {
                    if (conn.IsRegistered)
                    {
                        conn.User.IsConnected = false;
                        conn.User.IsRegistered = false;
                    }
                }
                foreach (var conn in _connections)
                {
                    conn.User = null;
                }
                _connections.Clear();

                _server.Stop();
                _server.Dispose();
            }
            _server = null;
            this.Status = ServerStatus.Stopped;

            _localServerInfo = null;

            _messageQueue.Clear();
            this.OnMessageQueueUpdated();

            // Stop threads
            if (_messageThread != null)
            {
                _messageThread.Abort();
                _messageThread = null;
            }

            if (_pingThread != null)
            {
                _pingThread.Abort();
                _pingThread = null;
            }

            if (_webThread != null)
            {
                _webThread.Abort();
                _webThread = null;
            }
        }

        private bool _isHandlingMessage;
        private void HandleMessageQueue()
        {
            // Is there a message queued up, and are we not currently handling a message?
            if (_messageQueue.Count > 0 && !_isHandlingMessage)
            {
                // Handle the message
                _isHandlingMessage = true;

                var message = _messageQueue.Dequeue();
                this.OnMessageQueueUpdated();

                // Synchronous
                this.HandleReceivedMessage(message);

                // Message handled
                _isHandlingMessage = false;
            }
            else
            {
                // No message was queued up, or we are already handling a message, wait before checking again
                Thread.Sleep(MESSAGE_INTERVAL);
            }

            // Check again
            HandleMessageQueue();
        }

        private void PingLoop()
        {
            while (this.Status == ServerStatus.Active)
            {
                this.BroadcastPingRequest();
                Thread.Sleep(PING_INTERVAL);
            }
        }

        private void WebUpdateLoop()
        {
            while (this.Status == ServerStatus.Active)
            {
                if (Settings.Default.LiveAdminEnabled)
                {
                    this.SendWebUpdate();
                    Thread.Sleep(Properties.Settings.Default.LiveAdminInterval*1000);
                }
            }
        }

        private void OnReceive(UserContext context)
        {
            // Add the received message to the end of the queue
            _messageQueue.Enqueue(context);
            this.OnMessageQueueUpdated();
        }

        private void HandleReceivedMessage(UserContext context)
        {
            // Handle just one message
            // Should not be run if already handling a message

            try
            {
                var value = context.DataFrame.ToString();
                var command = SyncMessage.FromString<SyncCommand>(value);
                if (command == null)
                {
                    this.OnLog("Unknown message: " + value);
                    return;
                }

                switch (command.Type)
                {
                    case SyncCommand.CommandTypes.Register:
                        this.RegisterUser(context, command);
                        break;

                    case SyncCommand.CommandTypes.Pong:
                        this.ReceivedPong(context, command);
                        break;

                    case SyncCommand.CommandTypes.RequestState:
                        this.SendState(context);
                        break;

                    case SyncCommand.CommandTypes.UpdateState:
                        var stateCommand = SyncMessage.FromString<SyncStateCommand>(value);
                        this.HandleStateUpdate(context, stateCommand);
                        break;

                    case SyncCommand.CommandTypes.ProposeSyncCameras:
                        this.HandleCameraSyncProposal(context, command);
                        break;

                    case SyncCommand.CommandTypes.RequestCameraState:
                        this.HandleCameraStateRequest(command);
                        break;
                }
            }
            catch (Exception ex)
            {
                App.Instance.LogError("Handling server message", ex);
                this.OnLog("Generic error handling server message.");
            }
        }

        private void OnConnected(UserContext context)
        {
            // Client connected
            // Store his connection details so we can link the connection to a user on registration

            this.OnLog("Client connected: " + context.ClientAddress);

            var conn = new UserConnection();
            conn.Context = context;
            conn.ClientAddress = context.ClientAddress.ToString();

            _connections.Add(conn);
        }

        private void OnDisconnect(UserContext context)
        {
            Debug.WriteLine(">>>> DISCONNECT <<<<");

            // Client disconnected
            // Find the client and remove him
            var conn = this.FindConnection(context);
            if (conn != null)
            {
                lock (conn.LockObject)
                {
                    this.OnLog(string.Format("Client disconnected: {0} ({1})", conn.Username, conn.ClientAddress));

                    if (conn.User != null) conn.User.IsConnected = false;
                    conn.User = null;
                    _connections.Remove(conn);

                    this.BroadcastUserlist();
                }
            }
            else
            {
                this.OnLog(string.Format("Unknown disconnected: {0}", context.ClientAddress));
            }
        }

        private UserConnection FindConnection(UserContext context)
        {
            return FindConnection(context.ClientAddress.ToString());
        }

        private UserConnection FindConnection(string ip)
        {
            return _connections.FromAddress(ip);
        }

        private UserConnection FindConnection(User user)
        {
            var id = user.CustId;
            return _connections.FirstOrDefault(c => c.IsRegistered && c.User.CustId == id);
        }
        
        private void RegisterUser(UserContext context, SyncCommand command)
        {
            // Client is trying to register
            // Find his connection and link it to a new user
            var conn = FindConnection(context);
            if (conn != null)
            {
                if (!conn.IsRegistered)
                {
                    var id = (int)command.Data.Id;
                    var ssid = (long)command.Data.Ssid;
                    var name = command.Data.Name.ToString();
                    var shortname = command.Data.ShortName.ToString();
                    var password = command.Data.Password.ToString();

                    // Check password
                    if (password == _password)
                    {
                        // Check subsession ID
                        if (ssid == _subSessionId)
                        {
                            // Check name length
                            if (!string.IsNullOrWhiteSpace(name) && name.Length >= MIN_NAME_LENGTH)
                            {
                                
                                // Try to find previously connected user
                                var user = this.State.Users.FromId(id);
                                if (user == null)
                                {
                                    // New user
                                    user = new User();
                                    user.CustId = id;
                                    this.State.Users.Add(user);

                                    UserColors.SetColor(user);
                                }

                                if (!user.IsConnected)
                                {
                                    user.Name = name;
                                    user.ShortName = shortname;
                                    user.IsRegistered = true;
                                    user.IsConnected = true;
                                    user.IsHost = id == _adminId;

                                    // Link him to his connection
                                    conn.User = user;

                                    this.OnLog("User registered: " + name);

                                    var response = new SyncResponse(SyncResponse.ResponseTypes.Connect);
                                    response.Data = new { Success = true, User = user };

                                    this.Reply(context, response);
                                    this.BroadcastUserlist();
                                    this.SendState(context);
                                }
                                else
                                {
                                    this.OnLog("Already connected: " + name);
                                    this.FailRegistration(context, "You are already connected.");
                                }
                            }
                            else
                            {
                                this.OnLog("Name too short: " + name);
                                this.FailRegistration(context,
                                    string.Format("Please choose a name with a minimum of {0} characters.", MIN_NAME_LENGTH));
                            }
                        }
                        else
                        {
                            this.OnLog("Incorrect session ID.");
                            this.FailRegistration(context, "Incorrect subsession ID: you must join the same session as the server host.");
                        }
                    }
                    else
                    {
                        this.OnLog("Incorrect password attempt: " + password);
                        this.FailRegistration(context, "Incorrect password.");
                    }
                }
                else
                {
                    this.OnLog("Already registered: " + conn.Username);
                    this.FailRegistration(context, "You are already registered.");
                }
            }
            else
            {
                this.OnLog("Unknown client tried to register: " + context.ClientAddress);
                this.FailRegistration(context, "Unknown client.");
            }
        }

        private void FailRegistration(UserContext context, string message)
        {
            var response = new SyncResponse(SyncResponse.ResponseTypes.Connect);
            response.Data = new { Success = false, Message = message};
            this.Reply(context, response);
        }

        private void ReceivedPong(UserContext context, SyncCommand command)
        {
            var conn = this.FindConnection(context);
            if (conn != null && conn.IsRegistered)
            {
                var user = conn.User;

                user.LastPongReceived = DateTime.Now.ToUniversalTime();
                var ping = (user.LastPongReceived.GetValueOrDefault() - user.LastPingSent.GetValueOrDefault()).Ticks / 2f;

                user.Ping = (int)Math.Round(ping / 10000f); // ms
            }
        }

        private void HandleStateUpdate(UserContext context, SyncCommand command)
        {
            Debug.WriteLine(">> Received state update: " + command);

            var conn = this.FindConnection(context);
            if (conn != null && conn.IsRegistered)
            {
                var message = (SyncStateCommand)command;

                switch (message.StateUpdateType)
                {
                    case SyncStateCommand.StateUpdateTypes.WatchedDriverChanged:
                        this.UpdateWatchedDriver(conn, message);
                        break;

                    case SyncStateCommand.StateUpdateTypes.LiveStatusChanged:
                        this.UpdateLiveStatus(conn, message);
                        break;

                    case SyncStateCommand.StateUpdateTypes.AddPenalty:
                        this.AddPenalty(conn, message);
                        break;

                    case SyncStateCommand.StateUpdateTypes.EditPenalty:
                        this.EditPenalty(conn, message);
                        break;

                    case SyncStateCommand.StateUpdateTypes.DeletePenalty:
                        this.DeletePenalty(conn, message);
                        break;

                        case SyncStateCommand.StateUpdateTypes.AddEvent:
                        this.AddEvent(conn, message);
                        break;

                    case SyncStateCommand.StateUpdateTypes.Offtracks:
                        this.UpdateOfftracks(conn, message);
                        break;

                    case SyncStateCommand.StateUpdateTypes.ClearOfftracks:
                        this.ClearOfftracks();
                        break;
                }

                // Save state to file
                this.SaveState();

                // Broadcast the new state to all clients
                this.BroadcastState();
            }
        }

        private void HandleCameraSyncProposal(UserContext context, SyncCommand command)
        {
            // Client requests OTHER clients to sync to his camera
            // Re-send data to clients
            var users = command.Data.Users.ToObject<List<User>>();
            //var users = (List<User>)JsonConvert.DeserializeObject<List<User>>(command.Data.Users.ToString());

            var response = new SyncResponse(SyncResponse.ResponseTypes.ProposeSyncCameras);
            response.Data = command.Data;
            var responseString = response.ToString();

            foreach (var user in users)
            {
                var conn = FindConnection(user);
                if (conn != null && conn.ClientAddress == context.ClientAddress.ToString()) continue; // skip requesting user
                if (conn != null)
                {
                    conn.Context.Send(responseString);
                }
            }
        }

        private void HandleCameraStateRequest(SyncCommand command)
        {
            // Client requests camera state of one other user
            
            // Obtain camera details and then 
        }

        private void UpdateWatchedDriver(UserConnection conn, SyncStateCommand command)
        {
            int id = conn.User.CustId;
            if (!this.State.WatchedDrivers.ContainsKey(id))
            {
                this.State.WatchedDrivers.Add(id, -1); // -1 means 'not focused on driver'
            }
            this.State.WatchedDrivers[id] = (int)command.Data.CarId;
        }

        private void UpdateLiveStatus(UserConnection conn, SyncStateCommand command)
        {
            int id = conn.User.CustId;
            if (!this.State.LiveStatus.ContainsKey(id))
            {
                this.State.LiveStatus.Add(id, false);
            }
            this.State.LiveStatus[id] = (bool)command.Data.IsLive;
        }

        private void AddPenalty(UserConnection conn, SyncStateCommand command)
        {
            var penalty = command.Data.Penalty.ToObject<Penalty>();
            this.UpdatePenalty(penalty);
        }

        private void EditPenalty(UserConnection conn, SyncStateCommand command)
        {
            var penalty = command.Data.Penalty.ToObject<Penalty>();
            this.UpdatePenalty(penalty);
        }

        private void DeletePenalty(UserConnection conn, SyncStateCommand command)
        {
            var id = (string)command.Data.PenaltyId;
            var penalty = this.State.Penalties.FirstOrDefault(p => p.Id == id);
            if (penalty != null)
            {
                this.State.Penalties.Remove(penalty);
            }
        }

        private void UpdatePenalty(Penalty penalty)
        {
            Debug.WriteLine(">>>>>>>>>>>>>>>> Penalty updated: " + penalty.Id);
            var existing = this.State.Penalties.SingleOrDefault(p => p.Id == penalty.Id);
            if (existing != null)
            {
                this.State.Penalties.Remove(existing);
            }

            this.State.Penalties.Add(penalty);
        }

        private void AddEvent(UserConnection conn, SyncStateCommand command)
        {
            var @event = command.Data.Event.ToObject<ReplayEvent>();
            this.UpdateEvent(@event);
        }

        private void UpdateEvent(ReplayEvent @event)
        {
            var existing = this.State.Events.SingleOrDefault(e => e.Id == @event.Id);
            if (existing != null)
            {
                this.State.Events.Remove(existing);
            }

            this.State.Events.Add(@event);
        }

        private void UpdateOfftracks(UserConnection conn, SyncStateCommand command)
        {
            var offtracks = (List<Offtrack>)command.Data.Offtracks.ToObject<List<Offtrack>>();
            if (offtracks.Count == 0) return;

            foreach (var offtrack in offtracks)
            {
                var id = offtrack.DriverId;
                if (!this.State.OfftrackHistories.ContainsKey(id))
                {
                    this.State.OfftrackHistories.Add(id, new List<Offtrack>());
                }

                // Does an offtrack with the same id already exist?
                var existing = this.State.OfftrackHistories[id].LastOrDefault(o => o.UniqueId == offtrack.UniqueId);
                if (existing != null)
                {
                    // Update this offtrack
                    this.State.OfftrackHistories[id].Remove(existing);
                }
                this.State.OfftrackHistories[id].Add(offtrack);
            }
        }

        private void ClearOfftracks()
        {
            this.State.OfftrackHistories.Clear();
        }

        private void SendState(UserContext context)
        {
            // Re-send the state if connected
            var conn = this.FindConnection(context);
            if (conn != null && conn.IsRegistered)
            {
                var response = new SyncResponse(SyncResponse.ResponseTypes.UpdateState);
                response.Data = new { State = _state };
                context.Send(response.ToString());
            }
        }

        private void Reply(UserContext context, SyncResponse response)
        {
            context.Send(response.ToString());
        }

        private void BroadcastPingRequest()
        {
            // Do not use Broadcast - create new ping response for every user
            foreach (var conn in _connections)
            {
                if (conn.IsRegistered)
                {
                    var now = DateTime.Now.ToUniversalTime();

                    // Only send ping if last pong was received
                    if (conn.User.LastPongReceived == null || conn.User.LastPongReceived.GetValueOrDefault() > conn.User.LastPingSent)
                    {
                        var response = SyncCommandHelper.Ping();
                        conn.User.LastPingSent = now;
                        conn.Context.Send(response.ToString());
                    }
                    else
                    {
                        // No ping received yet - set max ping
                        conn.User.Ping = 1000;
                    }
                }
            }

            this.BroadcastUserlist();
        }

        private void BroadcastUserlist()
        {
            var users = this.State.Users.ToList(); // _users.Where(u => u.IsRegistered).ToList();
            
            var response = new SyncResponse(SyncResponse.ResponseTypes.UserList);
            response.Data = new { Users = users };
            this.Broadcast(response.ToString());

            this.OnUpdateConnections();
        }

        private void BroadcastState()
        {
            // Copy settings that clients require
            _state.OfftrackLimit = Properties.Settings.Default.OfftrackLimit;

            var response = new SyncResponse(SyncResponse.ResponseTypes.UpdateState);
            response.Data = new { State = _state };
            Debug.WriteLine(">> Broadcasting state: " + response);

            this.Broadcast(response.ToString());
        }

        private void BroadcastDisconnect()
        {
            var response = new SyncResponse(SyncResponse.ResponseTypes.Disconnect);
            response.Data = new {Reason = "Admin stopped server."};
            this.Broadcast(response.ToString());
        }

        private void Broadcast(string message)
        {
            var conns = _connections.Where(c => c.IsRegistered);
            foreach (var conn in conns)
            {
                conn.Context.Send(message);
            }
        }

        private void SaveState()
        {
            SyncManager.SaveState(this.State);
        }

        private SyncState LoadState(long ssid)
        {
            var state = SyncManager.LoadState(ssid);

            if (state != null)
            {
                // Fix user colors
                foreach (var user in state.Users)
                {
                    UserColors.CountColor(user);
                }
            }

            return state;
        }

        private void SendWebUpdate()
        {
            var message = LiveAdmin.SendUpdate(_state);
            if (!string.IsNullOrWhiteSpace(message))
            {
                this.OnLog("Live admin web: " + message);
            }
        }

        public event EventHandler<LogEventArgs> Log;
        public event EventHandler MessageQueueUpdated;
        public event EventHandler UpdateConnections;
        public event EventHandler UpdateState;
        public event EventHandler StatusChanged;

        protected virtual void OnLog(string message)
        {
            if (this.Log != null)
            {
                this.Log(this, new LogEventArgs(message));
            }
        }

        protected virtual void OnMessageQueueUpdated()
        {
            if (this.MessageQueueUpdated != null) this.MessageQueueUpdated(this, EventArgs.Empty);

            if (this.MessageQueueSize > 25)
            {
                this.OnLog("Message queue size extremely high!");
            }
        }

        protected virtual void OnUpdateConnections()
        {
            if (this.UpdateConnections != null) this.UpdateConnections(this, EventArgs.Empty);
        }

        protected virtual void OnUpdateState()
        {
            if (this.UpdateState != null) this.UpdateState(this, EventArgs.Empty);
        }

        protected virtual void OnStatusChanged()
        {
            if (this.StatusChanged != null) this.StatusChanged(this, EventArgs.Empty);
        }

        public class LogEventArgs : EventArgs
        {
            public LogEventArgs(string message)
            {
                this.Message = message;
            }

            public string Message { get; private set; }
        }

        public enum ServerStatus
        {
            Stopped = 0,
            Active = 1
        }
    }
}
