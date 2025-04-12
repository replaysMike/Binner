using Binner.Common;
using Binner.Common.Extensions;
using Binner.Common.IO;
using Binner.Common.StorageProviders;
using Binner.Legacy.StorageProviders;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Web.ServiceHost;
using Microsoft.Extensions.Configuration;
using StrawberryShake.Transport.WebSockets;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using Topshelf;
using Topshelf.Runtime;
using Topshelf.Runtime.DotNetCore;

IConfigurationRoot configRoot;
WebHostServiceConfiguration? webHostConfig;
var configFile = EnvironmentVarConstants.GetEnvOrDefault(EnvironmentVarConstants.Config, Path.Combine(AppContext.BaseDirectory, AppConstants.AppSettings));
if (!Directory.Exists(Path.GetDirectoryName(configFile)))
{
    PrintError($"Path to configuration file '{Path.GetDirectoryName(configFile)}' does not exist!");
    Environment.Exit(ExitCodes.InvalidConfig);
    return;
}
if (!File.Exists(configFile))
{
    PrintError($"Configuration file '{configFile}' does not exist!");
    // if the environment is Docker and container environment is Development, wait for a minute before exiting to allow debugging of the container
    if (File.Exists("//build_date.info") && EnvironmentVarConstants.GetEnvOrDefault(EnvironmentVarConstants.Environment, "")?.Equals("Development", StringComparison.InvariantCultureIgnoreCase) == true)
        System.Threading.Tasks.Task.Delay(60 * 1000).GetAwaiter().GetResult();
    Environment.Exit(ExitCodes.InvalidConfig);
    return;
}
try
{
    var configBasePath = Path.GetDirectoryName(Path.GetFullPath(configFile)) ?? AppContext.BaseDirectory;
    var configBuilder = new ConfigurationBuilder()
        .SetBasePath(configBasePath)
        .AddJsonFile(configFile, optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();
    configRoot = configBuilder.Build();

    webHostConfig = configRoot.GetSection(nameof(WebHostServiceConfiguration)).Get<WebHostServiceConfiguration>();
    if (webHostConfig == null)
    {
        PrintError($"Could not read the {nameof(WebHostServiceConfiguration)} section of your configuration file '{configFile}'! Ensure it exists and doesn't contain formatting errors.");
        Environment.Exit(ExitCodes.InvalidConfig);
        return;
    }
}
catch (Exception ex)
{
    PrintError($"Could not read your configuration file '{configFile}'! Ensure it exists and doesn't contain formatting errors.");
    PrintError($"{ex.GetType().Name}:\n    {ex.GetBaseException().Message}", "EXCEPTION:");
    Environment.Exit(ExitCodes.InvalidConfig);
    return;
}

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
        var result = BinnerWebHostService.GenerateSelfSignedCertificate(webHostConfig, true);
        if (result.Status.HasFlag(BinnerWebHostService.CertificateState.Created))
        {
            Console.WriteLine("Successfully created certificate.");
            if (result.Status.HasFlag(BinnerWebHostService.CertificateState.Registered))
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

// print the official header
PrintHeader();

// setup nlog logging
var LogManagerConfigFile = EnvironmentVarConstants.GetEnvOrDefault(EnvironmentVarConstants.NlogConfig, AppConstants.NLogConfig);
var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogManagerConfigFile);
var logger = NLog.Web.NLogBuilder.ConfigureNLog(logFile).GetCurrentClassLogger();

// check if port is in use before proceeding
if (Ports.IsPortInUse(webHostConfig.Port))
{
    PrintError($"The port '{webHostConfig.Port}' is currently in use.");
    Environment.Exit(ExitCodes.PortInUse);
    return;
}

// setup service info
var displayName = typeof(BinnerWebHostService).GetDisplayName();
var serviceName = displayName.Replace(" ", "");
var serviceDescription = typeof(BinnerWebHostService).GetDescription();

// create a service using TopShelf
var rc = HostFactory.Run(x =>
{
    if (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
    {
        x.UseEnvironmentBuilder(target => new DotNetCoreEnvironmentBuilder(target));
    }

    x.AddCommandLineSwitch("dbinfo", v => PrintDbInfo(configRoot));
    x.ApplyCommandLine();

    x.Service<BinnerWebHostService>(s =>
    {
        s.ConstructUsing(name => new BinnerWebHostService());
        s.WhenStarted((tc, hostControl) => tc.Start(hostControl));
        s.WhenStopped((tc, hostControl) => tc.Stop(hostControl));
    });
    x.RunAsLocalSystem();

    x.SetDescription(serviceDescription);
    x.SetDisplayName(displayName);
    x.SetServiceName(serviceName);
    x.BeforeInstall(() => logger.Info($"Installing service {serviceName}..."));
    x.BeforeUninstall(() => logger.Info($"Uninstalling service {serviceName}..."));
    x.AfterInstall(() => logger.Info($"{serviceName} service installed."));
    x.AfterUninstall(() => logger.Info($"{serviceName} service uninstalled."));
    x.OnException((ex) => logger.Error(ex, $"{serviceName} exception thrown: {ex.Message}"));

    x.SetHelpTextPrefix("\nCustom commands: \n\n  Binner.Web.exe [-switch]\n\n    dbinfo              Shows database configuration diagnostics info\n\n");

    x.UnhandledExceptionPolicy = UnhandledExceptionPolicyCode.LogErrorAndStopService;
});

var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
Environment.ExitCode = exitCode;

void PrintHeader()
{
    var version = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
    //var banner = $"      Binner {version}      ";
    //PrintBox(banner);
    PrintVersionLogo(version);
    Console.ForegroundColor = ConsoleColor.Gray;

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

void PrintVersionLogo(string version)
{
    var textColor = ConsoleColor.Cyan;
    var highColor = ConsoleColor.White;
    var versionColor = ConsoleColor.Green;
    Console.OutputEncoding = Encoding.Unicode;
    Console.ForegroundColor = textColor;
    Console.Write($@"       D  DDD  D        BBBBBBBBB    ╭───────╮
   DDDDD DDDDD DDDDD   BBBBBBBBBBBBB │");
    Console.ForegroundColor = versionColor;
    Console.Write($@"v{version.PadRight(6)}");
    Console.ForegroundColor = textColor;
    Console.Write($@"│
  DDDDDD DDDDD DDDDDD  BBBB    BBBBBB╰───────╯ nN             nN                EEEEE      RRRR
                       BBBB      BBBB         NNNNNNNNNNNNN  NNNNNNNNNNNNN    EEEEEEEEEEEE  RRRRRRRRRR
  DDDDDD ");
    Console.ForegroundColor = highColor;
    Console.Write("8###8");
    Console.ForegroundColor = textColor;
    Console.Write($@" DDDDDD  BBBBBBBBBBBBB   IIII   NNNNNN  NNNNNN NNNNNN  NNNNNN  EEEEE   EEEEEE RRRRRRRR
  DDDDDD ");
    Console.ForegroundColor = highColor;
    Console.Write("#####");
    Console.ForegroundColor = textColor;
    Console.Write($@" DDDDDD  BBBBBBBBBBBBB   IIII   NNNN      NNNN NNNN      NNNN EEEEEEEEEEEEEEE RRRRR
  DDDDDD ");
    Console.ForegroundColor = highColor;
    Console.Write("8###8");
    Console.ForegroundColor = textColor;
    Console.Write($@" DDDDDD  BBBB     BBBBB  IIII   NNNN      NNNN NNNN      NNNN EEEEEEEEEEEEEEE RRRR
                       BBBB      BBBB  IIII   NNNN      NNNN NNNN      NNNN  EEEE     EEEEE RRRR
  DDDDDD DDDDD DDDDDD  BBBBBBBBBBBBBB  IIII   NNNN      NNNN NNNN      NNNN  EEEEEEEEEEEEE  RRRR
   DDDDD  DDD  DDDDD   BBBBBBBBBBBBB   IIII   NNNN      NNNN NNNN      NNNN    EEEEEEEEE    RRRR
       D  DDD  D       BBBBBBBBBBB     IIII   NNNN      NNNN NNNN      NNNN     EEEEEEE     RRRR
");
    Console.WriteLine();
}

void PrintBox(string text, ConsoleColor color = ConsoleColor.Green, ConsoleColor textColor = ConsoleColor.Yellow)
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

bool PrintDbInfo(IConfigurationRoot configRoot)
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

void PrintLabel(string label, ConsoleColor color = ConsoleColor.Gray)
{
    Console.ForegroundColor = color;
    Console.Write($"{label}: ");
}

void PrintValue(object? value, ConsoleColor color = ConsoleColor.DarkGray)
{
    Console.ForegroundColor = color;
    Console.WriteLine($"{value?.ToString()}");
}

void PrintOk()
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("[");
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("OK");
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("]");
    Console.ForegroundColor = ConsoleColor.Gray;
}

void PrintFailed()
{
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("[");
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Write("FAILED");
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("]");
    Console.ForegroundColor = ConsoleColor.Gray;
}

void PrintError(string errorMessage, string label = "ERROR:")
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(label);
    Console.ForegroundColor = ConsoleColor.Gray;
    Console.WriteLine($"  {errorMessage}");
    Console.WriteLine();
}
