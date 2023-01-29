using Binner.Common.Extensions;
using Binner.Web.ServiceHost;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using Topshelf;
using Topshelf.Runtime;

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
    Console.OutputEncoding = Encoding.Unicode;
    Console.ForegroundColor = ConsoleColor.Green;
    var bar = "─";
    var banner = $"      Binner {version}      ";

    Console.Write("╭");
    for (var i = 0; i < banner.Length; i++)
        Console.Write(bar);
    Console.WriteLine("╮");

    Console.Write($"│");
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write(banner);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"│");

    Console.Write("╰");
    for (var i = 0; i < banner.Length; i++)
        Console.Write(bar);
    Console.WriteLine("╯");
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Write($"O/S: {System.Runtime.InteropServices.RuntimeInformation.OSDescription} ({System.Runtime.InteropServices.RuntimeInformation.OSArchitecture})");
    Console.WriteLine($"  Runtime: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Gray;
}