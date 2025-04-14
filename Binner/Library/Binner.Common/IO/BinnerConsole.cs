using Binner.Common.Security;
using Binner.Common.Services;
using Binner.Common.StorageProviders;
using Binner.Legacy.StorageProviders;
using Binner.Model;
using Binner.Model.Configuration;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Binner.Common.IO
{
    public class BinnerConsole
    {
        private readonly string _configFile = EnvironmentVarConstants.GetEnvOrDefault(EnvironmentVarConstants.Config, AppConstants.AppSettings);
        private readonly ILogger _logger;
        private readonly CertificateUtility _certificateUtility;

        public BinnerConsole(ILogger logger, string configFile)
        {
            _logger = logger;
            _configFile = configFile;
            _certificateUtility = new CertificateUtility(_logger, _configFile);
        }

        public static void PrintVersionLogo(Version version)
        {
            var versionStr = version.ToString(3);
            var textColor = ConsoleColor.Cyan;
            var highColor = ConsoleColor.White;
            var versionColor = ConsoleColor.Green;
            Console.OutputEncoding = Encoding.Unicode;
            Console.ForegroundColor = textColor;
            Console.Write($@".......D..DDD..D........BBBBBBBBB... ╭───────╮ .......................................................
...DDDDD.DDDDD.DDDDD...BBBBBBBBBBBBB │");
            Console.ForegroundColor = versionColor;
            Console.Write($@"v{versionStr.PadRight(6)}");
            Console.ForegroundColor = textColor;
            Console.Write($@"│ ............. ......... ...............................
..DDDDDD.DDDDD.DDDDDD..BBBB....BBBBBB╰───────╯.nN.............nN................EEEEE......RRRR.......
.......................BBBB......BBBB.........NNNNNNNNNNNNN..NNNNNNNNNNNNN....EEEEEEEEEEEE..RRRRRRRRRR
..DDDDDD.");
            Console.ForegroundColor = highColor;
            Console.Write("8###8");
            Console.ForegroundColor = textColor;
            Console.Write($@".DDDDDD..BBBBBBBBBBBBB...IIII...NNNNNN..NNNNNN.NNNNNN..NNNNNN..EEEEE...EEEEEE.RRRRRRRR..
..DDDDDD.");
            Console.ForegroundColor = highColor;
            Console.Write("#####");
            Console.ForegroundColor = textColor;
            Console.Write($@".DDDDDD..BBBBBBBBBBBBB...IIII...NNNN......NNNN.NNNN......NNNN.EEEEEEEEEEEEEEE.RRRRR.....
..DDDDDD.");
            Console.ForegroundColor = highColor;
            Console.Write("8###8");
            Console.ForegroundColor = textColor;
            Console.Write($@".DDDDDD..BBBB.....BBBBB..IIII...NNNN......NNNN.NNNN......NNNN.EEEEEEEEEEEEEEE.RRRR......
.......................BBBB......BBBB..IIII...NNNN......NNNN.NNNN......NNNN..EEEE.....EEEEE.RRRR......
..DDDDDD.DDDDD.DDDDDD..BBBBBBBBBBBBBB..IIII...NNNN......NNNN.NNNN......NNNN..EEEEEEEEEEEEE..RRRR......
...DDDDD..DDD..DDDDD...BBBBBBBBBBBBB...IIII...NNNN......NNNN.NNNN......NNNN....EEEEEEEEE....RRRR......
.......D..DDD..D.......BBBBBBBBBBB.....IIII...NNNN......NNNN.NNNN......NNNN.....EEEEEEE.....RRRR......
......................................................................................................
..011000100110100101101110011011100110010101110010.011000100110100101101110011011100110010101110010...
");
            Console.WriteLine();
        }

        public static void PrintBox(string text, ConsoleColor color = ConsoleColor.Green, ConsoleColor textColor = ConsoleColor.Yellow)
        {
            Console.OutputEncoding = Encoding.Unicode;
            Console.ForegroundColor = color;
            var bar = "─";

            Console.Write("╭");
            for (var i = 0; i < text.Length; i++)
                Console.Write(bar);
            Console.WriteLine("╮");

            Console.Write($"│");
            Console.ForegroundColor = textColor;
            Console.Write(text);
            Console.ForegroundColor = color;
            Console.WriteLine($"│");

            Console.Write("╰");
            for (var i = 0; i < text.Length; i++)
                Console.Write(bar);
            Console.WriteLine("╯");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public async Task PrintHeaderAsync(string[] args, WebHostServiceConfiguration webHostConfig)
        {
            // print the logo
            var version = Assembly.GetEntryAssembly()?.GetName().Version ?? new Version();
            PrintVersionLogo(version);
            Console.ForegroundColor = ConsoleColor.Gray;

            // check if new version is available

            try
            {
                var versionService = new VersionManagementService();
                var latestVersionResponse = await versionService.GetLatestVersionAsync();
                var latestVersion = new Version(latestVersionResponse.Version.Replace("v", "", StringComparison.InvariantCultureIgnoreCase));
                if (latestVersionResponse.Version != null)
                {
                    var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
                    if (currentVersion != null && latestVersion > currentVersion)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"A new version available: v{latestVersion}");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                }
            }
            catch (Exception ex)
            {
                // suppress any i/o errors
                _logger.Warn($"Failed to fetch latest version information.");
            }

            // extra runtime details
            var displayExtraInfo = true;
            if (args.Length > 0 && (args[0].Equals("install", StringComparison.InvariantCultureIgnoreCase) || args[0].Equals("uninstall", StringComparison.InvariantCultureIgnoreCase)))
                displayExtraInfo = false;
            if (displayExtraInfo)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"O/S: {System.Runtime.InteropServices.RuntimeInformation.OSDescription} ({System.Runtime.InteropServices.RuntimeInformation.OSArchitecture})");
                Console.WriteLine($"  Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Uri: {new Uri($"{(webHostConfig.UseHttps ? "https" : "http")}://localhost:{webHostConfig.Port}")}");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine();
            }
        }

        public static bool PrintDbInfo(IConfigurationRoot configRoot, WebHostServiceConfiguration webHostConfig)
        {
            PrintBox("   Binner database information   ", ConsoleColor.Blue, ConsoleColor.Yellow);
            Console.ForegroundColor = ConsoleColor.Gray;
            var storageConfig = configRoot.GetSection(nameof(StorageProviderConfiguration)).Get<StorageProviderConfiguration>();
            if (storageConfig == null)
            {
                PrintError($"Could not read the {nameof(StorageProviderConfiguration)} section of your application configuration! Ensure it is valid json and doesn't contain formatting errors.");
                Environment.Exit(ExitCodes.InvalidConfig);
            }
            // inject configuration from environment variables (if set)
            EnvironmentVarConstants.SetConfigurationFromEnvironment(storageConfig);

            PrintLabel("Provider", ConsoleColor.White);
            PrintValue(storageConfig.Provider ?? "Unknown", ConsoleColor.Cyan);
            PrintLabel("Configuration", ConsoleColor.White);
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            foreach (var value in storageConfig.ProviderConfiguration)
            {
                PrintLabel($"   {value.Key}");
                PrintValue(value.Value);
            }
            PrintLabel("   User File Uploads Path");
            PrintValue(storageConfig.UserUploadedFilesPath ?? "Not Set");

            PrintLabel("Provider Information", ConsoleColor.White);
            Console.WriteLine();
            if (string.IsNullOrEmpty(storageConfig.Provider))
            {
                PrintError("   No storage provider set in storage configuration!");
            }
            else
            {
                if (storageConfig.Provider == BinnerFileStorageProvider.ProviderName)
                {
                    var fsProvider = new BinnerFileStorageProvider(storageConfig.ProviderConfiguration);
                    var response = fsProvider.TestConnectionAsync().GetAwaiter().GetResult();
                    PrintLabel("   Db Created");
                    PrintValue(fsProvider.Version?.Created.ToShortDateString());
                    PrintLabel("   Db Version");
                    PrintValue(fsProvider.Version?.Version);

                    PrintLabel("   Connection");
                    if (response.IsSuccess)
                        PrintOk();
                    else
                        PrintFailed();
                    PrintLabel("   Db Exists");
                    if (response.DatabaseExists)
                        PrintOk();
                    else
                        PrintFailed();
                }
                else
                {
                    var factory = new StorageProviderFactory();
                    // todo: migrate
                    /*var storageProvider = factory.Create(storageConfig.Provider, storageConfig.ProviderConfiguration);
                    var response = storageProvider.TestConnectionAsync().GetAwaiter().GetResult();
                    PrintLabel("   Connection");
                    if (response.IsSuccess)
                        PrintOk();
                    else
                        PrintFailed();
                    PrintLabel("   Db Exists");
                    if (response.DatabaseExists)
                        PrintOk();
                    else
                        PrintFailed();
                    if (response.Errors.Any())
                    {
                        PrintLabel("   Errors", ConsoleColor.Red);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        foreach (var error in response.Errors) Console.WriteLine($"    - {error}");
                        Console.WriteLine();
                    }*/
                }
            }

            PrintLabel("Server", ConsoleColor.White);
            Console.WriteLine();
            PrintLabel("   IP");
            PrintValue(webHostConfig.IP);
            PrintLabel("   Port");
            PrintValue(webHostConfig.Port);
            PrintLabel("   Uri");
            PrintValue(new Uri($"https://localhost:{webHostConfig.Port}"));

            PrintLabel("Integrations", ConsoleColor.White);
            Console.WriteLine();

            PrintLabel("   Swarm");
            Console.WriteLine();
            PrintLabel("      Enabled");
            PrintValue(webHostConfig.Integrations.Swarm.Enabled);

            PrintLabel("   DigiKey");
            Console.WriteLine();
            PrintLabel("      Enabled");
            PrintValue(webHostConfig.Integrations.Digikey.Enabled);

            PrintLabel("   Mouser");
            Console.WriteLine();
            PrintLabel("      Enabled");
            PrintValue(webHostConfig.Integrations.Mouser.Enabled);

            PrintLabel("   Octopart");
            Console.WriteLine();
            PrintLabel("      Enabled");
            PrintValue(webHostConfig.Integrations.Nexar.Enabled);

            Console.ForegroundColor = ConsoleColor.Gray;
            Environment.Exit(ExitCodes.Success);

            return false;
        }

        public static void PrintLabel(string label, ConsoleColor color = ConsoleColor.Gray)
        {
            Console.ForegroundColor = color;
            Console.Write($"{label}: ");
        }

        public static void PrintValue(object? value, ConsoleColor color = ConsoleColor.DarkGray)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"{value?.ToString()}");
        }

        public static void PrintOk()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("OK");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("]");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void PrintFailed()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("FAILED");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("]");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void PrintError(string errorMessage, string label = "ERROR:")
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(label);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"  {errorMessage}");
            Console.WriteLine();
        }

        public void CheckArgs(string[] args, WebHostServiceConfiguration webHostConfig)
        {
            // option to generate a self-signed certificate
            if (args.Length > 0 && (
                args[0].Equals("--generatecertificate", StringComparison.InvariantCultureIgnoreCase)
                || args[0].Equals("--installcertificate", StringComparison.InvariantCultureIgnoreCase)
                || args[0].Equals("-g", StringComparison.InvariantCultureIgnoreCase)
                ))
            {
                // generate a self-signed certificate and quit
                Console.WriteLine("Generating certificate...");
                try
                {
                    var result = _certificateUtility.GenerateSelfSignedCertificate(webHostConfig, true);
                    if (result.Status.HasFlag(CertificateState.Created))
                    {
                        Console.WriteLine("Successfully created certificate.");
                        if (result.Status.HasFlag(CertificateState.Registered))
                        {
                            Console.WriteLine("Successfully registered certificate.");
                        }
                        else
                        {
                            Console.WriteLine("Failed to register certificate.");
                        }
                        Environment.Exit(ExitCodes.Success);
                    }
                    else
                    {
                        Console.WriteLine($"Error: Failed to create certificate! {result.Error}");
                        Environment.Exit(ExitCodes.FailedToCreateCertificate);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: Failed to create certificate!");
                    Console.WriteLine($"Exception: {ex.GetBaseException().Message}");
                    Environment.Exit(ExitCodes.FailedToCreateCertificate);
                }
            }
        }
    }
}
