﻿using Binner.Common;
using Binner.Model.IO.Printing;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace Binner.Services.IO.Printing
{
    /// <summary>
    /// Unix environment printer via CUPS
    /// </summary>
    /// <remarks>
    /// https://www.cups.org/doc/options.html
    /// </remarks>
    [SupportedOSPlatform("linux"),]
    [SupportedOSPlatform("macos")]
    [SupportedOSPlatform("freebsd")]
    [SupportedOSPlatform("osx")]
    public class CupsPrinterEnvironment : IPrinterEnvironment
    {
        private const bool OutputDebug = true;
        private const bool FlipLabelImage = true;
        private const string CupsError = "Please ensure CUPS print server is installed on your environment. Example: `sudo apt install cups`";
        private readonly ILogger<CupsPrinterEnvironment> _logger;
        private readonly IPrinterSettings _printerSettings;
        private LabelDefinition _labelProperties;

        public CupsPrinterEnvironment(ILogger<CupsPrinterEnvironment> logger, IPrinterSettings printerSettings)
        {
            _logger = logger;
            _printerSettings = printerSettings;
            _labelProperties = new LabelDefinition();
        }

        public PrinterResult PrintLabel(PrinterOptions options, LabelDefinition labelProperties, Image<Rgba32> labelImage)
        {
            _labelProperties = labelProperties;

            // save the image to file system
            var (filename, isSuccess, errorMessage) = SaveTempImage(labelImage);
            if (isSuccess)
            {
                // execute lp process to print to CUPS
                try
                {
                    SendToCups(filename, options);
                }
                catch (Exception ex)
                {
                    throw new CupsException(CupsError, ex);
                }
                finally
                {
                    DeleteTempFile(filename);
                }
            }
            else
            {
                return new PrinterResult
                {
                    IsSuccess = false,
                    ErrorMessage = errorMessage,
                    ErrorCode = -1
                };
            }

            return new PrinterResult
            {
                IsSuccess = true
            };
        }

        private void SendToCups(string? filename, PrinterOptions options)
        {
            if (string.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            if (options is null) throw new ArgumentNullException(nameof(options));

            var process = CreateProcess();
            var result = StartProcess(process);

            (bool isSuccess, string errorMessage, int exitCode) StartProcess(Process innerProcess)
            {
                if (innerProcess.Start())
                {
                    innerProcess.BeginErrorReadLine();
                    innerProcess.BeginOutputReadLine();
                    WriteDebug($"CUPS: Print process id {innerProcess.Id} started!");
                    //_logger.LogInformation($"Printing label at dpi X:{e.PageSettings.PrinterResolution.X}, Y:{e.PageSettings.PrinterResolution.Y}");
                    innerProcess.WaitForExit();
                    try
                    {
                        if (innerProcess.ExitCode == 0)
                            // print success
                            return (true, string.Empty, innerProcess.ExitCode);
                        else
                        {
                            return (false, $"CUPS: Print process returned non-zero exit code ({innerProcess.ExitCode}), check for errors!", innerProcess.ExitCode);
                        }
                    }
                    finally
                    {
                        innerProcess.Dispose();
                    }
                }
                else
                {
                    return (false, $"Failed to launch process named '{innerProcess.StartInfo.FileName}'. {CupsError}", 0);
                }
            }

            Process CreateProcess()
            {
                // print using CUPS command line (lp & lpr utilities)
                //var printerName = "DYMO_LabelWriter_450_Twin_Turbo";
                var printerName = GetCupsSafePrinterName(_printerSettings.PrinterName);
                var fileToPrint = filename;
                /* lpoptions -p {printerName}
                 * copies=1 device-uri=usb://DYMO/LabelWriter%20450%20Twin%20Turbo?serial=19010918445943 finishings=3 job-cancel-after=10800 job-hold-until=no-hold job-priority=50 job-sheets=none,none marker-change-time=0 number-up=1 printer-commands=none printer-info='DYMO LabelWriter 450 Twin Turbo' printer-is-accepting-jobs=true printer-is-shared=false printer-is-temporary=false printer-location printer-make-and-model='Dymo Label Printer' printer-state=3 printer-state-change-time=1640312198 printer-state-reasons=none printer-type=2101252 printer-uri-supported=ipp://localhost/printers/DYMO_LabelWriter_450_Twin_Turbo
                 */
                /* lpoptions -p {printerName} -l
                 * PageSize/Media Size: w154h64 w72h154 w72h72 w162h90 w54h144 w118h252 w167h188 w79h252 w167h252 w167h288 w154h198 *w82h248 w154h64.1 w79h252.1 w102h252 w154h286 w154h198.1 w54h424 w131h221 w57h248 w54h144.1 w72h72.1 w72h72.2 w162h90.1 30334_2-1_4_in_x_1-1_4_in w73h86 w72h154.1 w118h252.1 w54h203 w54h180 w36h136 w72h108 w65h90 w167h288.1 w168h252 w144h169 w71h144 w144h252 w80h144 w162h504 30383_PC_Postage_3-Part w167h540 30384_PC_Postage_2-Part w167h756 30387_PC_Postage_EPS w167h188.1 w176h292 w167h288.2 w112h126 w79h252.2 w102h252.1 w154h286.1 w154h286.2 w154h198.2 w63h419 w139h221 w36h144 w108h539 w167h539 w154h7680 w154h792 Custom.WIDTHxHEIGHT
                 * Resolution/Resolution: 136dpi 203dpi *300dpi
                 * cupsDarkness/Darkness: Light Medium *Normal Dark
                 */

                var printerOptions = new List<string>
                {
                    // note that the orientation doesn't seem to rotate the image on the Dymo 450 Turbo. We will handle rotation in the image itself
                    //{ $"-o orientation-requested=6" }
                    //{ "-o fit-to-page" }
                    //{ "-o landscape" } // -o orientation-requested=3 (no rotation), -o orientation-requested=4 (90 degrees, landscape), -o orientation-requested=6 (180 degrees)
                    //{ "-o collate=true" }
                    //{ "-o media=A4" } // Letter,Legal,A4,COM10,DL,Transparency,Upper,Lower,MultiPurpose,LargeCapacity
                    //{ "-o media=Custom.WIDTHxLENGTHin" }
                    //{ "-n {numCopies}" }
                    //{ "-o job-hold-until=indefinite" } // hold job until released
                    //{ "-i job-id -H resume" } // release job to print
                };

                // -o {optionName}={value} ;set a printer option
                // specify label paper source (Auto, Left, Right) and the media type (label type)
                if (options.LabelSource.HasValue)
                    printerOptions.Add($"-o media={options.LabelSource.Value},{_labelProperties.MediaSize.DriverName}");
                else
                    printerOptions.Add($"-o media={_labelProperties.MediaSize.DriverName}");
                var optionsStr = string.Empty;
                if (printerOptions.Any())
                    optionsStr = $"{string.Join(" ", printerOptions)}";
                var innerProcess = new Process();
                innerProcess.StartInfo.FileName = "lp";
                // -d {printerName} ;specify a specific printer
                var arguments = $"-d {printerName} {optionsStr} {fileToPrint}";
                innerProcess.StartInfo.Arguments = arguments;
                innerProcess.StartInfo.RedirectStandardOutput = true;
                innerProcess.StartInfo.RedirectStandardError = true;
                innerProcess.StartInfo.UseShellExecute = false;
                innerProcess.StartInfo.CreateNoWindow = true;
                innerProcess.EnableRaisingEvents = true;
                innerProcess.ErrorDataReceived += Process_ErrorDataReceived;
                innerProcess.OutputDataReceived += Process_OutputDataReceived;
                innerProcess.Exited += Process_Exited;
                return innerProcess;
            }

            string GetCupsSafePrinterName(string printerName)
            {
                // respect CUPS allowed printer names
                return printerName.Replace(" ", "_")
                    .Replace("\t", "")
                    .Replace("/", "")
                    .Replace("#", "");
            }
        }

        private (string? filename, bool isSuccess, string? errorMessage) SaveTempImage(Image<Rgba32> labelImage)
        {
            var filename = $"{Path.GetTempFileName()}.png";
            try
            {
                if (FlipLabelImage)
                {
                    // can't get CUPS to flip the label, could be the driver causing this. Flip the label image so it matches Windows output
                    labelImage.Mutate(x => x.Rotate(RotateMode.Rotate180));
                }
                //
                labelImage.SaveAsPng(filename);
                return (filename, true, null);
            }
            catch (Exception ex)
            {
                // failed to save file
                return (filename, false, ex.GetBaseException().Message);
            }
        }

        private void DeleteTempFile(string? filename)
        {
            if (string.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            if (File.Exists(filename))
                File.Delete(filename);
        }

        private void Process_Exited(object? sender, EventArgs e)
        {
            if (sender is null)
                return;
            var senderProcess = (Process)sender;
            if (OutputDebug) Console.WriteLine($"CUPS: Process {senderProcess.Id} finished with exit code {senderProcess.ExitCode}");
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(e.Data))
                WriteDebug($"CUPS Error: {e.Data}");
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
                WriteDebug($"CUPS Message: {e.Data}");
        }

        private void WriteDebug(string message)
        {
            if (OutputDebug)
                Console.WriteLine(message);
        }
    }
}
