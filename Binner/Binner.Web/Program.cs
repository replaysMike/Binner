using Binner.Common;
using Binner.Common.Extensions;
using Binner.Common.StorageProviders;
using Binner.Legacy.StorageProviders;
using Binner.Model.Configuration;
using Binner.Web.ServiceHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using Topshelf;
using Topshelf.Runtime;

WebApplicationBuilder builder;
WebHostServiceConfiguration? config;
try
{    // Question : Can we use Config class to reduce the risk of error with BinnerWebHostService.InitializeWebHostAsync ?
    builder = WebApplication.CreateBuilder();
    builder.Configuration.AddEnvironmentVariables();
    config = builder.Configuration.GetSection(nameof(WebHostServiceConfiguration)).Get<WebHostServiceConfiguration>();
    if (config == null)
    {
        PrintError($"Could not read the {nameof(WebHostServiceConfiguration)} section of your appsettings.json! Ensure it is valid json and doesn't contain formatting errors.");
        Environment.Exit(ExitCodes.InvalidConfig);
        return;
    }
}
catch (Exception ex)
{
    PrintError($"Could not read your appsettings.json! Ensure it is valid json and doesn't contain formatting errors.");
    PrintError($"{ex.GetType().Name}:\n    {ex.GetBaseException().Message}", "EXCEPTION:");
    Environment.Exit(ExitCodes.InvalidConfig);
    return;
}
PrintHeader();

const string LogManagerConfigFile = "nlog.config"; // TODO: Inject from appsettings
var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogManagerConfigFile);
var logger = NLog.Web.NLogBuilder.ConfigureNLog(logFile).GetCurrentClassLogger();
var displayName = typeof(BinnerWebHostService).GetDisplayName();
var serviceName = displayName.Replace(" ", "");
var serviceDescription = typeof(BinnerWebHostService).GetDescription();

// create a service using TopShelf
var rc = HostFactory.Run(x =>
{
    x.AddCommandLineSwitch("dbinfo", v => PrintDbInfo());
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
    var version = Assembly.GetExecutingAssembly().GetName().Version;
    var banner = $"      Binner {version}      ";
    PrintBox(banner);

    var displayExtraInfo = true;
    if (args.Length > 0 && (args[0].Equals("install", StringComparison.InvariantCultureIgnoreCase) || args[0].Equals("uninstall", StringComparison.InvariantCultureIgnoreCase)))
        displayExtraInfo = false;
    if (displayExtraInfo)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"O/S: {System.Runtime.InteropServices.RuntimeInformation.OSDescription} ({System.Runtime.InteropServices.RuntimeInformation.OSArchitecture})");
        Console.WriteLine($"  Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"Uri: {new Uri($"https://localhost:{config.Port}")}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine();
    }
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

bool PrintDbInfo()
{
    PrintBox("   Binner database information   ", ConsoleColor.Blue, ConsoleColor.Yellow);
    Console.ForegroundColor = ConsoleColor.Gray;
    var storageConfig = builder.Configuration.GetSection(nameof(StorageProviderConfiguration)).Get<StorageProviderConfiguration>();
    if (storageConfig == null)
    {
        PrintError($"Could not read the {nameof(StorageProviderConfiguration)} section of your appsettings.json! Ensure it is valid json and doesn't contain formatting errors.");
        Environment.Exit(ExitCodes.InvalidConfig);
    }

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
    PrintLabel("   Environment");
    PrintValue(config.Environment);
    PrintLabel("   IP");
    PrintValue(config.IP);
    PrintLabel("   Port");
    PrintValue(config.Port);
    PrintLabel("   Uri");
    PrintValue(new Uri($"https://localhost:{config.Port}"));

    PrintLabel("Integrations", ConsoleColor.White);
    Console.WriteLine();

    PrintLabel("   Swarm");
    Console.WriteLine();
    PrintLabel("      Enabled");
    PrintValue(config.Integrations.Swarm.Enabled);

    PrintLabel("   DigiKey");
    Console.WriteLine();
    PrintLabel("      Enabled");
    PrintValue(config.Integrations.Digikey.Enabled);

    PrintLabel("   Mouser");
    Console.WriteLine();
    PrintLabel("      Enabled");
    PrintValue(config.Integrations.Mouser.Enabled);

    PrintLabel("   Octopart");
    Console.WriteLine();
    PrintLabel("      Enabled");
    PrintValue(config.Integrations.Nexar.Enabled);

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
