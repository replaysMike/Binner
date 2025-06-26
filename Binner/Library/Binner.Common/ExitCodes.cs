namespace Binner.Common
{
    public static class ExitCodes
    {
        public const int Success = 0;
        public const int ExternalApplicationRun = -1;
        public const int InvalidConfig = -2;
        public const int PortInUse = -3;
        public const int FailedToCreateCertificate = -4;
        public const int AccessDenied = -5;
        public const int InvalidOptions = -6;
    }
}
