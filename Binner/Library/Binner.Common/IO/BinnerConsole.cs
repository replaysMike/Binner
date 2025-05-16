using Binner.Common.Database;
using Binner.Common.Extensions;
using Binner.Common.Security;
using Binner.Common.Services;
using Binner.Data;
using Binner.Data.Model;
using Binner.Global.Common;
using Binner.Legacy.StorageProviders;
using Binner.Model;
using Binner.Model.Configuration;
using CommandLine;
using LightInject;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IConfigurationRoot _configurationRoot;
        private readonly WebHostServiceConfiguration _webHostConfig;

        public BinnerConsole(ILogger logger, string configFile, IConfigurationRoot configurationRoot, WebHostServiceConfiguration webHostConfig)
        {
            _logger = logger;
            _configFile = configFile;
            _certificateUtility = new CertificateUtility(_logger, _configFile);
            _configurationRoot = configurationRoot;
            _webHostConfig = webHostConfig;
        }

        public async Task CheckNewVersionAsync()
        {
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

            // extra runtime details
            var displayExtraInfo = true;
            var supressCommands = new[] { "install", "uninstall", "start", "stop" };
            if (args.Length > 0 && supressCommands.Contains(args[0].ToLower()))
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

        public async Task<bool> PrintDbInfo()
        {
            var storageProviderConfiguration = LoadStorageConfiguration();
            var builder = new ContainerBuilder(_webHostConfig, storageProviderConfiguration);
            var container = builder.Build();

            PrintBox("   Binner database information   ", ConsoleColor.Blue, ConsoleColor.Yellow);
            Console.ForegroundColor = ConsoleColor.Gray;

            // inject configuration from environment variables (if set)
            EnvironmentVarConstants.SetConfigurationFromEnvironment(storageProviderConfiguration);

            PrintLabel("Provider", ConsoleColor.White);
            PrintValue(storageProviderConfiguration.Provider ?? "Unknown", ConsoleColor.Cyan);
            PrintLabel("Configuration", ConsoleColor.White);
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            foreach (var value in storageProviderConfiguration.ProviderConfiguration)
            {
                PrintLabel($"   {value.Key}");
                if (value.Key.Contains("password", StringComparison.InvariantCultureIgnoreCase))
                {
                    PrintValue(value.Value.Sanitize());
                }
                else
                {
                    PrintValue(value.Value);
                }
            }
            PrintLabel("   User File Uploads Path");
            PrintValue(storageProviderConfiguration.UserUploadedFilesPath ?? "Not Set");

            PrintLabel("Provider Information", ConsoleColor.White);
            Console.WriteLine();
            if (string.IsNullOrEmpty(storageProviderConfiguration.Provider))
            {
                PrintError("   No storage provider set in storage configuration!");
            }
            else
            {
                var isLegacy = false;
                // check if it's a legacy format
                if (storageProviderConfiguration.Provider == BinnerFileStorageProvider.ProviderName)
                {
                    try
                    {
                        var fsProvider = new BinnerFileStorageProvider(storageProviderConfiguration.ProviderConfiguration);
                        var legacyResponse = fsProvider.TestConnectionAsync().GetAwaiter().GetResult();
                        PrintLabel("   Db type");
                        PrintValue("Legacy");
                        PrintLabel("   Db Created");
                        PrintValue(fsProvider.Version?.Created.ToShortDateString());
                        PrintLabel("   Db Version");
                        PrintValue(fsProvider.Version?.Version);

                        PrintLabel("   Connection");
                        if (legacyResponse.IsSuccess)
                            PrintOk();
                        else
                            PrintFailed();
                        PrintLabel("   Db Exists");
                        if (legacyResponse.DatabaseExists)
                            PrintOk();
                        else
                            PrintFailed();

                        try
                        {
                            await RunDiagnosticsAsync(container);
                        }
                        catch (Exception ex)
                        {
                            PrintError($"An error occurred while running diagnostics. {ex.GetBaseException().Message}");
                        }

                        isLegacy = true;
                    }
                    catch (Exception ex)
                    {
                        // not a legacy format file
                    }
                }

                if (!isLegacy)
                {
                    var storageProvider = container.GetInstance<IStorageProvider>();

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
                    }
                    else
                    {
                        PrintLabel("   Parts");
                        var parts = await storageProvider.GetPartsCountAsync(new UserContext { UserId = 1, OrganizationId = 1 });
                        PrintValue($"{parts}");

                        PrintLabel("   Users");
                        var userCount = await storageProvider.GetUserCountAsync();
                        PrintValue($"{userCount}");

                        PrintLabel("   Admin Users");
                        var adminUserCount = await storageProvider.GetUserAdminCountAsync();
                        PrintValue($"{adminUserCount}");

                        try
                        {
                            await RunDiagnosticsAsync(container);
                        }
                        catch (Exception ex)
                        {
                            PrintError($"An error occurred while running diagnostics. {ex.GetBaseException().Message}");
                        }

                    }
                }
            }

            PrintLabel("Server", ConsoleColor.White);
            Console.WriteLine();
            PrintLabel("   IP");
            PrintValue(_webHostConfig.IP);
            PrintLabel("   Port");
            PrintValue(_webHostConfig.Port);
            PrintLabel("   Uri");
            PrintValue(new Uri($"https://localhost:{_webHostConfig.Port}"));
            PrintLabel("   UseHttps");
            PrintValue(_webHostConfig.UseHttps);
            PrintLabel("   SslCertificate");
            PrintValue(_webHostConfig.SslCertificate);
            PrintLabel("   License Key");
            PrintValue(_webHostConfig.Licensing?.LicenseKey.Sanitize());

            PrintLabel("Locale", ConsoleColor.White);
            Console.WriteLine();

            PrintLabel("   Language");
            PrintValue(_webHostConfig.Locale.Language);
            PrintLabel("   Currency");
            PrintValue(_webHostConfig.Locale.Currency);

            PrintLabel("Integrations", ConsoleColor.White);
            Console.WriteLine();

            PrintLabel("   Swarm");
            Console.WriteLine();
            PrintLabel("      Enabled");
            PrintValue(_webHostConfig.Integrations.Swarm.Enabled);

            PrintLabel("   DigiKey");
            Console.WriteLine();
            PrintLabel("      Enabled");
            PrintValue(_webHostConfig.Integrations.Digikey.Enabled);

            PrintLabel("   Mouser");
            Console.WriteLine();
            PrintLabel("      Enabled");
            PrintValue(_webHostConfig.Integrations.Mouser.Enabled);

            PrintLabel("   Octopart");
            Console.WriteLine();
            PrintLabel("      Enabled");
            PrintValue(_webHostConfig.Integrations.Nexar.Enabled);

            PrintLabel("   Arrow");
            Console.WriteLine();
            PrintLabel("      Enabled");
            PrintValue(_webHostConfig.Integrations.Arrow.Enabled);

            PrintLabel("   TME");
            Console.WriteLine();
            PrintLabel("      Enabled");
            PrintValue(_webHostConfig.Integrations.Tme.Enabled);

            Console.ForegroundColor = ConsoleColor.Gray;
            Environment.Exit(ExitCodes.Success);

            return false;
        }

        private static async Task RunDiagnosticsAsync(IServiceContainer container)
        {
            PrintLabel("Diagnostics", ConsoleColor.White);
            Console.WriteLine();

            var context = container.GetInstance<BinnerContext>();
            //await using var context = await _contextFactory.CreateDbContextAsync();
            PrintLabel("   Checking users");
            var hasError = false;
            if (await context.Users.Where(x => x.OrganizationId == 0).AnyAsync())
            {
                PrintErrorItem("User(s) missing OrganizationId"); hasError = true;
            }
            if (await context.Users.Where(x => x.UserId == 0).AnyAsync())
            {
                PrintErrorItem("User(s) missing UserId"); hasError = true;
            }
            if (!await context.Users.Where(x => x.IsAdmin).AnyAsync())
            {
                PrintErrorItem("No admin user exists."); hasError = true;
            }
            var usersEmptyPassword = await context.Users.Where(x => string.IsNullOrEmpty(x.PasswordHash)).ToListAsync();
            if (usersEmptyPassword.Any())
            {
                PrintErrorItem("User(s) empty password set."); hasError = true;
                foreach(var user in usersEmptyPassword)
                {
                    Console.WriteLine($"         User: {user.EmailAddress}");
                }
            }
            if (!hasError)
                PrintOk();

            hasError = false;
            PrintLabel("   Checking part types");
            if (await CheckOrgAsync(context, context.PartTypes, nameof(context.PartTypes))) hasError = true;
            if (await CheckEmptyAsync(context, context.PartTypes, nameof(context.PartTypes))) hasError = true;

            if (!hasError)
                PrintOk();

            hasError = false;
            PrintLabel("   Checking login history");
            if (!await context.UserLoginHistory.AnyAsync())
            {
                PrintWarnItem("No login history exists."); hasError = true;
            }
            if (!hasError)
                PrintOk();

            hasError = false;
            PrintLabel("   Checking organizations");
            if (!await context.Organizations.AnyAsync())
            {
                PrintErrorItem("No organizations exist."); hasError = true;
            }
            if (!hasError)
                PrintOk();

            hasError = false;
            PrintLabel("   Checking oAuthCredentials");
            if (await CheckOrgAsync(context, context.OAuthCredentials, nameof(context.OAuthCredentials))) hasError = true;
            if (await CheckEmptyAsync(context, context.OAuthCredentials, nameof(context.OAuthCredentials), true)) hasError = true;
            if (!hasError)
                PrintOk();

            hasError = false;
            PrintLabel("   Checking oAuthRequests");
            if (await CheckOrgAsync(context, context.OAuthRequests, nameof(context.OAuthRequests))) hasError = true;
            if (await CheckEmptyAsync(context, context.OAuthRequests, nameof(context.OAuthRequests), true)) hasError = true;
            if (!hasError)
                PrintOk();

            hasError = false;
            PrintLabel("   Checking labels");
            if (await CheckOrgAsync(context, context.Labels, nameof(context.Labels))) hasError = true;
            if (await CheckEmptyAsync(context, context.Labels, nameof(context.Labels))) hasError = true;
            if (!hasError)
                PrintOk();

            hasError = false;
            PrintLabel("   Checking label templates");
            if (await CheckOrgAsync(context, context.LabelTemplates, nameof(context.LabelTemplates))) hasError = true;
            if (await CheckEmptyAsync(context, context.LabelTemplates, nameof(context.LabelTemplates))) hasError = true;
            if (!hasError)
                PrintOk();

            hasError = false;
            PrintLabel("   Checking BOM Projects");
            if (await CheckOrgAsync(context, context.Projects, nameof(context.Projects))) hasError = true;
            if (await CheckOrgAsync(context, context.ProjectPartAssignments, nameof(context.ProjectPartAssignments))) hasError = true;
            if (await CheckOrgAsync(context, context.ProjectPcbAssignments, nameof(context.ProjectPcbAssignments))) hasError = true;
            if (await CheckOrgAsync(context, context.ProjectPcbProduceHistory, nameof(context.ProjectPcbProduceHistory))) hasError = true;
            if (await CheckOrgAsync(context, context.ProjectProduceHistory, nameof(context.ProjectProduceHistory))) hasError = true;
            if (!hasError)
                PrintOk();

            
        }

        private static async Task<bool> CheckOrgAsync<T>(BinnerContext context, IQueryable<T> query, string tableName) where T : IUserData
        {
            if (await query.Where(x => x.OrganizationId == 0).AnyAsync())
            {
                PrintErrorItem($"{tableName}(s) missing OrganizationId.");
                return true;
            }
            if (await query.Where(x => x.UserId == 0).AnyAsync())
            {
                PrintErrorItem($"{tableName}(s) missing UserId.");
                return true;
            }
            return false;
        }

        private static async Task<bool> CheckEmptyAsync<T>(BinnerContext context, IQueryable<T> query, string tableName, bool isWarning = false) where T : IUserData
        {
            if (!await query.AnyAsync())
            {
                if(!isWarning)
                    PrintErrorItem($"{tableName}(s) is empty.");
                else
                    PrintWarnItem($"{tableName}(s) is empty.");
                return true;
            }
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

        public static void PrintErrorItem(string message)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\n      * [");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(message);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("]");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void PrintWarnItem(string warn)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\n      * [");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(warn);
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

        private StorageProviderConfiguration LoadStorageConfiguration()
        {
            var storageConfig = _configurationRoot.GetSection(nameof(StorageProviderConfiguration)).Get<StorageProviderConfiguration>();
            if (storageConfig == null)
            {
                PrintError($"Could not read the {nameof(StorageProviderConfiguration)} section of your application configuration! Ensure it is valid json and doesn't contain formatting errors.");
                Environment.Exit(ExitCodes.InvalidConfig);
            }
            return storageConfig;
        }

        public async Task<bool> CheckArgsAsync(string[] args, WebHostServiceConfiguration webHostConfig)
        {
            var isHandled = false;
            var parserResult = Parser.Default.ParseArguments<CommandLineOptions>(args);
            await parserResult
                .WithParsedAsync(async o =>
                {
                    if (o.DbInfo)
                    {
                        // print database info
                        PrintDbInfo();
                        Environment.Exit(ExitCodes.Success);
                    }
                    if (o.GenerateCertificate)
                    {
                        // option to generate a self-signed certificate
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

                    if (o.ResetUser)
                    {
                        if (!Environment.IsPrivilegedProcess)
                        {
                            Console.WriteLine($"Error: This command must be run in an elevated process (Administrator/root).");
                            Environment.Exit(ExitCodes.AccessDenied);
                        }

                        if (string.IsNullOrEmpty(o.Username))
                        {
                            Console.WriteLine($"Error: --username must be specified.");
                            Environment.Exit(ExitCodes.InvalidOptions);
                        }

                        // reset password for user
                        var storageProviderConfiguration = LoadStorageConfiguration();
                        var builder = new ContainerBuilder(_webHostConfig, storageProviderConfiguration);
                        var container = builder.Build();

                        var storageProvider = container.GetInstance<IStorageProvider>();
                        var isSuccess = await storageProvider.ResetUserCredentialsAsync(o.Username);
                        if (isSuccess)
                        {
                            Console.WriteLine($"User '{o.Username}' was reset successfully.");
                            Environment.Exit(ExitCodes.Success);
                        }
                        else
                        {
                            Console.WriteLine($"Error: Failed to reset user '{o.Username}'. Please ensure that you have a valid username.");
                            Environment.Exit(ExitCodes.Success);
                        }
                    }
                });

            parserResult.WithNotParsed(errors =>
            {
                // user requested help
                if (errors.Any(x => x is HelpRequestedError) || errors.Any(x => x is VersionRequestedError))
                    Environment.Exit(ExitCodes.Success);

                Console.WriteLine("One or more errors occurred:");
                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }
                Environment.Exit(ExitCodes.InvalidOptions);
            });
            return isHandled;
        }

    }
}
