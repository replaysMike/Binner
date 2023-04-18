namespace Binner.Common.IO
{
    public static class HardwareId
    {
        /// <summary>
        /// Get the machine's device id
        /// </summary>
        /// <returns></returns>
        public static string Get()
        {
            return libc.hwid.HwId.Generate();
        }
    }
}
