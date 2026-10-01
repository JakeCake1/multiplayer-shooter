namespace Shooter.Application
{
    public readonly struct NetworkSessionStartResult
    {
        private NetworkSessionStartResult(bool succeeded, NetworkSessionError error, string detail)
        {
            Succeeded = succeeded;
            Error = error;
            Detail = detail;
        }

        public bool Succeeded { get; }

        public NetworkSessionError Error { get; }

        public string Detail { get; }

        public static NetworkSessionStartResult Success()
        {
            return new NetworkSessionStartResult(true, NetworkSessionError.None, string.Empty);
        }

        public static NetworkSessionStartResult Failure(NetworkSessionError error, string detail)
        {
            return new NetworkSessionStartResult(false, error, detail ?? string.Empty);
        }
    }
}
