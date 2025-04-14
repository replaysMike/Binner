﻿using Binner.Common;
using Binner.Common.Extensions;
using Binner.Common.IO;
using Binner.Model;
using Binner.Model.Configuration;
using Binner.Web.ServiceHost;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Topshelf;
using Topshelf.Runtime;
using Topshelf.Runtime.DotNetCore;

IConfigurationRoot configRoot;
WebHostServiceConfiguration? webHostConfig;
var configFile = EnvironmentVarConstants.GetEnvOrDefault(EnvironmentVarConstants.Config, Path.Combine(AppContext.BaseDirectory, AppConstants.AppSettings));
// setup nlog logging
var LogManagerConfigFile = EnvironmentVarConstants.GetEnvOrDefault(EnvironmentVarConstants.NlogConfig, AppConstants.NLogConfig);
var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogManagerConfigFile);
var logger = NLog.Web.NLogBuilder.ConfigureNLog(logFile).GetCurrentClassLogger();

// create a console
var console = new BinnerConsole(logger, configFile);

if (!Directory.Exists(Path.GetDirectoryName(configFile)))
{
    BinnerConsole.PrintError($"Path to configuration file '{Path.GetDirectoryName(configFile)}' does not exist!");
    Environment.Exit(ExitCodes.InvalidConfig);
    return;
}
if (!File.Exists(configFile))
{
    BinnerConsole.PrintError($"Configuration file '{configFile}' does not exist!");
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
        BinnerConsole.PrintError($"Could not read the {nameof(WebHostServiceConfiguration)} section of your configuration file '{configFile}'! Ensure it exists and doesn't contain formatting errors.");
        Environment.Exit(ExitCodes.InvalidConfig);
        return;
    }
}
catch (Exception ex)
{
    BinnerConsole.PrintError($"Could not read your configuration file '{configFile}'! Ensure it exists and doesn't contain formatting errors.");
    BinnerConsole.PrintError($"{ex.GetType().Name}:\n    {ex.GetBaseException().Message}", "EXCEPTION:");
    Environment.Exit(ExitCodes.InvalidConfig);
    return;
}

// process any optional arguments
console.CheckArgs(args, webHostConfig);

// print the header and runtime information
await console.PrintHeaderAsync(args, webHostConfig);

// check if port is in use before proceeding
if (Ports.IsPortInUse(webHostConfig.Port))
{
    BinnerConsole.PrintError($"The port '{webHostConfig.Port}' is currently in use.");
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

    x.AddCommandLineSwitch("dbinfo", v => BinnerConsole.PrintDbInfo(configRoot, webHostConfig));
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

// exit with code
var exitCode = (int)Convert.ChangeType(rc, rc.GetTypeCode());
Environment.ExitCode = exitCode;
