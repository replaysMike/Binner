using CommandLine;

namespace Binner.Common
{
    public class CommandLineOptions : IResetUserOptions
    {
        [Option(Hidden = true)]
        public bool AutoStart { get; set; } // used by TopShelf args
        [Option(Hidden = true)]
        public bool LocalSystem { get; set; } // used by TopShelf args
        [Option(Hidden = true)]
        public bool LocalService { get; set; } // used by TopShelf args
        [Option('p', "password", Hidden = true)]
        public string? Password { get; set; } // used by TopShelf args

        [Option('g', "generatecertificate", Required = false, HelpText = "Generate a new certificate")]
        public bool GenerateCertificate { get; set; }

        public bool ResetUser { get; set; }
        public string Username { get; set; } = null!;

        [Option('d', "dbinfo", Required = false, HelpText = "Print out database information")]
        public bool DbInfo { get; set; }

    }

    public interface IResetUserOptions
    {
        [Option('r', "resetuser", Required = false, HelpText = "Reset the password for a specified user", SetName = "resetuser")]
        bool ResetUser { get; set; }

        [Option('u', "username", Required = false, HelpText = "Specify the user to modify", SetName = "resetuser")]
        string Username { get; set; }
    }
}
