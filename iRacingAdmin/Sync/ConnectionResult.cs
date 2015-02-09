namespace iRacingAdmin.Sync
{
    public class ConnectionResult
    {
        public ConnectionResult()
        {
            this.Success = true;
        }

        public ConnectionResult(string msg)
        {
            this.Success = false;
            this.Message = msg;
        }

        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
