using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
[assembly: OwinStartup(typeof(SignalRChat.Startup))]
namespace SignalRChat
{
    public class Startup
    {

        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            Thread child = new Thread(new ThreadStart(EscucharPeticionesDeRegistro));
            child.Start();
        }

        private void EscucharPeticionesDeRegistro()
        {
            string data;
            byte[] bytes = new Byte[1024];
            Socket handler;
            try
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[1];
                IPEndPoint localEP = new IPEndPoint(ipAddress, 555);
                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(localEP);
                listener.Listen(30);
                while (true)
                {
                    handler = listener.Accept();
                    data = null;
                    while (true)
                    {
                        bytes = new byte[1024];
                        int bytesRec = handler.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("<EOF>") > -1)
                        {
                            break;
                        }
                    }

                    var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                    context.Clients.All.configurarGrafica(data.Replace("<EOF>", ""));

                    Thread child = new Thread(() => EscucharDatosEntrantes(data.Replace("<EOF>", "")));

                    child.Start();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void EscucharDatosEntrantes(string sumServerUri)
        {
            Thread.Sleep(500);
            byte[] ip;
            string data;
            byte[] bytes = new Byte[1024];
            Socket handler;
            int puerto;
            try
            {
                string strIp = sumServerUri.Substring(0, sumServerUri.IndexOf(":"));
                string[] ipArray = strIp.Split(new char[] { '.' });
                ip = new byte[] { byte.Parse(ipArray[0]), byte.Parse(ipArray[1]), byte.Parse(ipArray[2]), byte.Parse(ipArray[3]) };
                int.TryParse(sumServerUri.Substring(sumServerUri.IndexOf(":") + 1), out puerto);
                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[1];
                IPEndPoint localEP = new IPEndPoint(new IPAddress(ip), puerto);
                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                listener.Bind(localEP);
                listener.Listen(30);
                while (true)
                {
                    handler = listener.Accept();
                    data = null;
                    while (true)
                    {
                        bytes = new byte[1024];
                        int bytesRec = handler.Receive(bytes);
                        data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        var context = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();
                        context.Clients.All.recibirDato(data.Replace(",", ""), sumServerUri);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

    }
}