using System;
using System.Collections.Generic;
using iRacingAdmin.Models.Drivers;
using iRacingAdmin.Sync.Penalties;

namespace iRacingAdmin.Sync
{
    public static class SyncCommandHelper
    {
        public static SyncResponse Ping()
        {
            var response = new SyncResponse(SyncResponse.ResponseTypes.Ping);
            return response;
        }

        public static SyncCommand Pong()
        {
            var command = new SyncCommand(SyncCommand.CommandTypes.Pong);
            return command;
        }

        public static SyncCommand ProposeSyncCameras(List<User> admins, CameraDetails camera)
        {
            var command = new SyncCommand(SyncCommand.CommandTypes.ProposeSyncCameras);
            command.Data = new
                           {
                               Users = admins,
                               CameraDetails = camera,
                               Force = false,
                               ProposingAdmin = SyncManager.Instance.User.CustId
                           };
            return command;
        }

        public static SyncStateCommand AddEvent(ReplayEvent @event)
        {
            var command = new SyncStateCommand();
            command.StateUpdateType = SyncStateCommand.StateUpdateTypes.AddEvent;
            command.Data = new
            {
                Event = @event
            };
            return command;
        }

        public static SyncStateCommand EditEvent(ReplayEvent @event)
        {
            var command = new SyncStateCommand();
            command.StateUpdateType = SyncStateCommand.StateUpdateTypes.EditEvent;
            command.Data = new
            {
                Event = @event
            };
            return command;
        }

        public static SyncStateCommand DeleteEvent(ReplayEvent @event)
        {
            var command = new SyncStateCommand();
            command.StateUpdateType = SyncStateCommand.StateUpdateTypes.DeleteEvent;
            command.Data = new
            {
                Event = @event
            };
            return command;
        }

        public static SyncStateCommand AddPenalty(Penalty penalty)
        {
            var command = new SyncStateCommand();
            command.StateUpdateType = SyncStateCommand.StateUpdateTypes.AddPenalty;
            command.Data = new
                           {
                               Penalty = penalty
                           };
            return command;
        }

        public static SyncStateCommand EditPenalty(Penalty penalty)
        {
            var command = new SyncStateCommand();
            command.StateUpdateType = SyncStateCommand.StateUpdateTypes.EditPenalty;
            command.Data = new
            {
                Penalty = penalty
            };
            return command;
        }

        public static SyncStateCommand DeletePenalty(Penalty penalty)
        {
            var command = new SyncStateCommand();
            command.StateUpdateType = SyncStateCommand.StateUpdateTypes.DeletePenalty;
            command.Data = new
            {
                PenaltyId = penalty.Id
            };
            return command;
        }

        public static SyncStateCommand UpdateWatchedDriver(int carId)
        {
            var command = new SyncStateCommand();
            command.StateUpdateType = SyncStateCommand.StateUpdateTypes.WatchedDriverChanged;
            command.Data = new {CarId = carId};
            return command;
        }

        public static SyncStateCommand UpdateLiveStatus(bool live)
        {
            var command = new SyncStateCommand();
            command.StateUpdateType = SyncStateCommand.StateUpdateTypes.LiveStatusChanged;
            command.Data = new { IsLive = live };
            return command;
        }

        public static SyncStateCommand UpdateOfftracks(List<Offtrack> offtracks)
        {
            var command = new SyncStateCommand();
            command.StateUpdateType = SyncStateCommand.StateUpdateTypes.Offtracks;
            command.Data = new {Offtracks = offtracks};
            return command;
        }

        public static SyncStateCommand ClearOfftracks()
        {
            var command = new SyncStateCommand();
            command.StateUpdateType = SyncStateCommand.StateUpdateTypes.ClearOfftracks;
            return command;
        }
    }
}
