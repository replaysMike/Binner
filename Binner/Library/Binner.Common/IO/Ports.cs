namespace Binner.Common.IO
{
    public static class Ports
    {
        /// <summary>
        /// Check if a TCP port is currently in use
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool IsPortInUse(int port)
        {
            try
            {
                using (var tcpListener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Any, port))
                {
                    tcpListener.Start();
                    tcpListener.Stop();
                }
                return false;
            }
            catch (System.Net.Sockets.SocketException)
            {
                return true;
            }
        }
    }
}
