namespace Binner.Model.Configuration;

public enum PrintModes
{
    /// <summary>
    /// Print directly to the printer using internal SDK
    /// </summary>
    Direct,
    /// <summary>
    /// Print using the web browser's dialog
    /// </summary>
    WebBrowser,
    /// <summary>
    /// Print using the Binner print spool service
    /// </summary>
    PrintSpoolService
}