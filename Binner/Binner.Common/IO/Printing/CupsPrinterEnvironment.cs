using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;

namespace Binner.Common.IO.Printing
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
    internal class CupsPrinterEnvironment : IPrinterEnvironment
    {
        private readonly IPrinterSettings _printerSettings;
        private LabelProperties _labelProperties;

        public CupsPrinterEnvironment(IPrinterSettings printerSettings)
        {
            _printerSettings = printerSettings;
        }

        public PrinterResult PrintLabel(PrinterOptions options, LabelProperties labelProperties, Image<Rgba32> labelImage)
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
                    const string CUPSError = "Please ensure CUPS print server is installed on your environment. Example: `sudo apt install cups`";
                    throw new CupsException(CUPSError, ex);
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

        private void SendToCups(string filename, PrinterOptions options)
        {
            if (string.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            if (options is null) throw new ArgumentNullException(nameof(options));

            var process = CreateProcess();
            var result = StartProcess(process);

            (bool isSuccess, string errorMessage, int exitCode) StartProcess(Process process)
            {
                if (process.Start())
                {
                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();
                    process.WaitForExit();
                    try
                    {
                        if (process.ExitCode == 0)
                            // print success
                            return (true, string.Empty, process.ExitCode);
                        else
                        {
                            return (false, $"Print process returned non-zero exit code ({process.ExitCode}), check for errors!", process.ExitCode);
                        }
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
                else
                {
                    return (false, $"Failed to launch process named '{process.StartInfo.FileName}'", 0);
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
                 * PageSize/Media Size: *w81h252 w101h252 w54h144 w167h288 w162h540 w162h504 w41h248 w41h144 w153h198
                 * Resolution/Resolution: 136dpi 203dpi *300dpi
                 * cupsDarkness/Darkness: Light Medium *Normal Dark
                 */
                var printerOptions = new List<string>
                {
                    //{ "-o fit-to-page" }
                    //{ "-o landscape" } // -o orientation-requested=3 (no rotation), -o orientation-requested=4 (90 degrees, landscape), -o orientation-requested=6 (180 degrees)
                    //{ "-o collate=true" }
                    //{ "-o media=A4" } // Letter,Legal,A4,COM10,DL,Transparency,Upper,Lower,MultiPurpose,LargeCapacity
                    //{ "-o media=Custom.WIDTHxLENGTHin" }
                    //{ "-n {numCopies}" }
                    //{ "-o job-hold-until=indefinite" } // hold job until released
                    //{ "-i job-id -H resume" } // release job to print
                };
                // specify label paper source (Auto, Left, Right)
                if (options.LabelSource.HasValue)
                    printerOptions.Add($"-o media={options.LabelSource.Value}");
                var optionsStr = string.Empty;
                if (printerOptions.Any())
                    optionsStr = $"{string.Join(" ", printerOptions)}";
                var process = new Process();
                process.StartInfo.FileName = "lp";
                // -d {printerName} ;specify a specific printer
                // -o {optionName}={value} ;set a printer option
                var arguments = $"-d {printerName} {optionsStr} {fileToPrint}";
                process.StartInfo.Arguments = arguments;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.EnableRaisingEvents = true;
                process.ErrorDataReceived += Process_ErrorDataReceived;
                process.OutputDataReceived += Process_OutputDataReceived;
                process.Exited += Process_Exited;
                return process;
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

        private (string filename, bool isSuccess, string errorMessage) SaveTempImage(Image<Rgba32> labelImage)
        {
            var filename = $"{Path.GetTempFileName()}.png";
            if (filename is null)
                return (filename, false, "The platform generated temporary filename was null!");
            try
            {
                labelImage.SaveAsPng(filename);
                return (filename, true, null);
            }
            catch (Exception ex)
            {
                // failed to save file
                return (filename, false, ex.GetBaseException().Message);
            }
        }

        private void DeleteTempFile(string filename)
        {
            if (string.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            if (File.Exists(filename))
                File.Delete(filename);
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            if (sender is null)
                return;
            var senderProcess = (Process)sender;
            Console.WriteLine($"EXIT: Process ({senderProcess.Id}) exited!");
        }

        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine($"ERR: {e.Data}");
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine($"OUT: {e.Data}");
        }
    }
}
