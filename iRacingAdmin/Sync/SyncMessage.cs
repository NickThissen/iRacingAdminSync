using System;
using Newtonsoft.Json;

namespace iRacingAdmin.Sync
{
    public abstract class SyncMessage
    {
        protected SyncMessage()
        {
            
        }

        public dynamic Data { get; set; }

        public static T FromString<T>(string json)
            where T : SyncMessage
        {
            try
            {
                var message = (T) JsonConvert.DeserializeObject<T>(json);
                return message;
            }
            catch (Exception ex)
            {
                // Unknown message
                return null;
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    /// <summary>
    /// Command from client to server
    /// </summary>
    public class SyncCommand : SyncMessage
    {
        public SyncCommand() : this(CommandTypes.Register) { }

        public SyncCommand(CommandTypes type)
        {
            this.Type = type;
        }

        public CommandTypes Type { get; set; }

        public enum CommandTypes
        {
            Register = 0,
            Pong,
            RequestState,
            UpdateState,
            ProposeSyncCameras,
            RequestCameraState,
            SyncPenalties
        }
    }

    public class SyncStateCommand : SyncCommand
    {
        public SyncStateCommand() : base(SyncCommand.CommandTypes.UpdateState)
        {
        }

        public StateUpdateTypes StateUpdateType { get; set; }

        public enum StateUpdateTypes
        {
            WatchedDriverChanged = 0,
            LiveStatusChanged,
            AddPenalty,
            EditPenalty,
            DeletePenalty,
            AddEvent,
            EditEvent,
            DeleteEvent,
            Offtracks,
            ClearOfftracks
        }
    }

    /// <summary>
    /// Response from server to client
    /// </summary>
    public class SyncResponse : SyncMessage
    {
        public SyncResponse() : this(ResponseTypes.Connect) { }

        public SyncResponse(ResponseTypes type)
        {
            this.Type = type;
        }

        public ResponseTypes Type { get; set; }

        public enum ResponseTypes
        {
            Ping,
            Connect,
            Disconnect,
            UpdateState,
            ProposeSyncCameras,
            UserList,
            Error = 255
        }
    }
}
