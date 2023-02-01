using Binner.Common.Configuration;
using Binner.Common.Extensions;
using Binner.Common.StorageProviders;
using Binner.Web.ServiceHost;
using LightInject;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;
using System.Data.Common;
using System.IO;
using System.Reflection;
using System.Text;
using Topshelf;
using Topshelf.Runtime;

PrintHeader();

if (args.Length > 0)
    ProcessArgs(args);

const string LogManagerConfigFile = "nlog.config"; // TODO: Inject from appsettings
var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogManagerConfigFile);
var logger = NLog.Web.NLogBuilder.ConfigureNLog(logFile).GetCurrentClassLogger();
var displayName = typeof(BinnerWebHostService).GetDisplayName();
var serviceName = displayName.Replace(" ", "");
var serviceDescription = typeof(BinnerWebHostService).GetDescription();

// create a service using TopShelf
var rc = HostFactory.Run(x =>
{
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
    x.UnhandledExceptionPolicy = UnhandledExceptionPolicyCode.LogErrorAndStopService;
});

var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
Environment.ExitCode = exitCode;

static void PrintHeader()
{
    var version = Assembly.GetExecutingAssembly().GetName().Version;
    var banner = $"      Binner {version}      ";
    PrintBox(banner);
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Write($"O/S: {System.Runtime.InteropServices.RuntimeInformation.OSDescription} ({System.Runtime.InteropServices.RuntimeInformation.OSArchitecture})");
    Console.WriteLine($"  Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
    Console.WriteLine();
}

static void PrintBox(string text, ConsoleColor color = ConsoleColor.Green, ConsoleColor textColor = ConsoleColor.Yellow)
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

static void ProcessArgs(string[] args)
{
    if (args.Length == 0)
        return;
    switch (args[0])
    {
        case "db-info":
            PrintBox("   Binner database information   ", ConsoleColor.Blue, ConsoleColor.Yellow);

            var builder = WebApplication.CreateBuilder(args);
            var config = builder.Configuration.GetSection(nameof(WebHostServiceConfiguration)).Get<WebHostServiceConfiguration>();
            var storageConfig = builder.Configuration.GetSection(nameof(StorageProviderConfiguration)).Get<StorageProviderConfiguration>();
            Console.WriteLine($" Provider: {storageConfig.Provider}");
            Console.WriteLine($" Configuration:");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            foreach (var value in storageConfig.ProviderConfiguration)
            {
                Console.WriteLine($"   {value.Key}: {value.Value}");
            }
            Console.WriteLine($"   User File Uploads Path: {storageConfig.UserUploadedFilesPath}");
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.WriteLine($" Provider Information:");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            if (storageConfig.Provider == BinnerFileStorageProvider.ProviderName)
            {
                var fsProvider = new BinnerFileStorageProvider(storageConfig.ProviderConfiguration);
                Console.WriteLine($"   Db Created: {fsProvider.Version.Created}");
                Console.WriteLine($"   Db Version: {fsProvider.Version.Version}");
            } else
            {
            }
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.WriteLine($" Server:");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"   Environment: {config.Environment}");
            Console.WriteLine($"   IP: {config.IP}");
            Console.WriteLine($"   Port: {config.Port}");
            Console.WriteLine($"   Uri: {new Uri($"https://localhost:{config.Port}")}");

            Environment.Exit(-1);
            break;
    }
}