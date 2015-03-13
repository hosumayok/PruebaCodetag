using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace GeneradorNumerico
{
    class Program
    {

        private static readonly Random getrandom = new Random();
        private static readonly object syncLock = new object();
        private static byte[] ip;
        private static int port;
        private static Socket sender;

        public static void Main(String[] args)
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[1];
            ip = ipHostInfo.AddressList[1].GetAddressBytes();

            Console.WriteLine("Generador de numeros aleatorios");
            Console.WriteLine("\nIngrese la ip a la cual se van a enviar datos, ej: " + ipHostInfo.AddressList[1].ToString() + ", oprima enter para ingresar la ip por defecto: " + ipHostInfo.AddressList[1].ToString());
            string strIp = Console.ReadLine();
            if (strIp != "")
            {
                string[] ipArray = strIp.Split(new char[] { '.' });
                ip = new byte[] { byte.Parse(ipArray[0]), byte.Parse(ipArray[1]), byte.Parse(ipArray[2]), byte.Parse(ipArray[3]) };
            }
            Console.WriteLine("\nIngrese el puerto al cual se van a enviar datos, ej: " + "1000" + "");
            string strPuerto = Console.ReadLine();
            port = int.Parse(strPuerto);
            Thread thread = new Thread(new ThreadStart(EnviarDatos));
            thread.Start();
            Console.WriteLine("\nEnviando datos a: " + ip[0] + "." + ip[1] + "." + ip[2] + "." + ip[3] + ":" + port + ", oprima enter para terminar");
            Console.ReadLine();
            TerminarConexion();
        }

        private static void EnviarDatos()
        {
            try
            {
                IPEndPoint remoteEP = new IPEndPoint(new IPAddress(ip), port);
                sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sender.Connect(remoteEP);
                while (true)
                {
                    byte[] msg = Encoding.ASCII.GetBytes(GenerarNumeroAleatorio(0, 20).ToString() + ",");
                    int bytesSent = sender.Send(msg);
                    Thread.Sleep(201);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void TerminarConexion()
        {
            try
            {
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static int GenerarNumeroAleatorio(int min, int max)
        {
            lock (syncLock)
            { // synchronize
                return getrandom.Next(min, max);
            }
        }

    }
}
