using CommandLine;

namespace Binner.Common
{
    public class CommandLineOptions : IResetUserOptions
    {
        [Option("generatecertificate", Required = false, HelpText = "Generate a new certificate")]
        public bool GenerateCertificate { get; set; }

        public bool ResetUser { get; set; }
        
        public string Username { get; set; } = null!;

        [Option("dbinfo", Required = false, HelpText = "Print out database information")]
        public bool DbInfo { get; set; }

    }

    public interface IResetUserOptions
    {
        [Option("resetuser", Required = false, HelpText = "Reset the password for a specified user", SetName = "resetuser")]
        bool ResetUser { get; set; }

        [Option("username", Required = false, HelpText = "Specify the user to modify", SetName = "resetuser")]
        string Username { get; set; }
    }
}
