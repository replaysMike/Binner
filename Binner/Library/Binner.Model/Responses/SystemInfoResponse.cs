namespace Binner.Model.Responses
{
    public class SystemInfoResponse
    {
        public string? License { get; set; }
        public string? Language { get; set; }
        public string? Currency { get; set; }
        public string Version { get; set; } = null!;
        public string LatestVersion { get; set; } = null!;
        public string Port { get; set; } = null!;
        public string IP { get; set; } = null!;
        public int TotalUsers { get; set; }
        public int TotalParts { get; set; }
        public int TotalPartTypes { get; set; }
        public int TotalUserFiles { get; set; }
        public string? UserFilesSize { get; set; }
        public string StorageProvider { get; set; } = null!;
        public string? StorageProviderSettings { get; set; }
        public string? InstallPath { get; set; }
        public string? LogPath { get; set; }
        public string EnabledIntegrations { get; set; } = null!;
        public string? UserFilesLocation { get; set; }
    }
}
