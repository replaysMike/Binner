namespace Binner.Model
{
    public enum DatasheetState
    {
        /// <summary>
        /// Datasheet has not been scheduled for processing (No UrlCrawlQueue record) and needs to be scheduled to download or process
        /// </summary>
        Unscheduled = 0,
        /// <summary>
        /// Datasheet has been scheduled for processing, but not yet downloaded
        /// </summary>
        DownloadQueued,
        /// <summary>
        /// Datasheet has been queued and will be downloaded soon
        /// </summary>
        DownloadPending,
        /// <summary>
        /// Datasheet PDF file has been located and will be downloaded
        /// </summary>
        DownloadDiscovered,
        /// <summary>
        /// Datasheet is being downloaded
        /// </summary>
        Downloading,
        /// <summary>
        /// Datasheet has been downloaded and is ready for processing
        /// </summary>
        Downloaded,
        /// <summary>
        /// Download of the datasheet has failed
        /// </summary>
        DownloadError,
        /// <summary>
        /// Datasheet PDF is scheduled to be processed
        /// </summary>
        PdfProcessingPending,
        /// <summary>
        /// Datasheet PDF is currently processing
        /// </summary>
        PdfProcessing,
        /// <summary>
        /// Datasheet PDF has been processed and is ready for use
        /// </summary>
        PdfProcessed,
        /// <summary>
        /// There was an error processing the PDF
        /// </summary>
        PdfProcessingError,
    }
}
