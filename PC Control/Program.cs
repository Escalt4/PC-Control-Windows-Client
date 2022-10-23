using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SocketUdpClient
{
    class Program
    {
        [DllImport("Powrprof.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);
        static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    Socket listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                    IPEndPoint localIP = new IPEndPoint(IPAddress.Parse("0.0.0.0"), 4445);
                    listeningSocket.Bind(localIP);

                    while (true)
                    {
                        StringBuilder builder = new StringBuilder();
                        int bytes = 0;
                        byte[] data = new byte[256];

                        EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);

                        do
                        {
                            bytes = listeningSocket.ReceiveFrom(data, ref remoteIp);
                            builder.Append(Encoding.ASCII.GetString(data, 0, bytes));
                        }
                        while (listeningSocket.Available > 0);

                        IPEndPoint remoteFullIp = remoteIp as IPEndPoint;


                        switch (builder.ToString())
                        {
                            case "ConnectionTest":
                                sendAnswer(listeningSocket, remoteFullIp, "TestPass");
                                break;

                            case "Off":
                                sendAnswer(listeningSocket, remoteFullIp, "OffSuccess");
                                Process.Start("shutdown", "-s -t 0");
                                break;

                            case "Sleep":
                                sendAnswer(listeningSocket, remoteFullIp, "SleepSuccess");
                                listeningSocket.Close();
                                SetSuspendState(false, true, true);
                                break;

                            case "Hibernate":
                                sendAnswer(listeningSocket, remoteFullIp, "HibernateSuccess");
                                listeningSocket.Close();
                                SetSuspendState(true, true, true);
                                break;

                            default:
                                sendAnswer(listeningSocket, remoteFullIp, "Failed");
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        static void sendAnswer(Socket listeningSocket, IPEndPoint remoteFullIp, string answer)
        {
            Thread.Sleep(100);

            byte[] data = Encoding.ASCII.GetBytes(answer);
            EndPoint remotePoint = new IPEndPoint(IPAddress.Parse(remoteFullIp.Address.ToString()), 5005);
            listeningSocket.SendTo(data, remotePoint);
        }
    }
}