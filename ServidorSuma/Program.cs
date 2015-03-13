using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServidorSuma
{
    class Program
    {

        private static byte[] ipIn;
        private static int portIn;
        private static byte[] ipOut;
        private static int portOut;
        private static Socket sender;
        private static Socket handler;

        public static void Main(String[] args)
        {
            Console.WriteLine("Servidor de suma");

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[1];
            ipIn = ipHostInfo.AddressList[1].GetAddressBytes();
            Console.WriteLine("\nIngrese la ip desde la cual se van a recibir datos, ej: " + ipHostInfo.AddressList[1].ToString() + ", oprima enter para ingresar la ip por defecto: " + ipHostInfo.AddressList[1].ToString());
            string strIp = Console.ReadLine();
            if (strIp != "")
            {
                string[] ipArray = strIp.Split(new char[] { '.' });
                ipIn = new byte[] { byte.Parse(ipArray[0]), byte.Parse(ipArray[1]), byte.Parse(ipArray[2]), byte.Parse(ipArray[3]) };
            }
            Console.WriteLine("\nIngrese el puerto desde el cual se van a recibir datos, ej: " + "1000" + "");
            string strPuerto = Console.ReadLine();
            portIn = int.Parse(strPuerto);

            ipOut = ipHostInfo.AddressList[1].GetAddressBytes();
            Console.WriteLine("\nIngrese la ip hacia la cual se va a enviar los datos de resultado, ej: " + ipHostInfo.AddressList[1].ToString() + ", oprima enter para ingresar la ip por defecto: " + ipHostInfo.AddressList[1].ToString());
            string strIpOut = Console.ReadLine();
            if (strIpOut != "")
            {
                string[] ipArray = strIpOut.Split(new char[] { '.' });
                ipOut = new byte[] { byte.Parse(ipArray[0]), byte.Parse(ipArray[1]), byte.Parse(ipArray[2]), byte.Parse(ipArray[3]) };
            }
            Console.WriteLine("\nIngrese el puerto hacia el cual se van a enviar los datos de resultado, ej: " + "2000" + "");
            string strPuertoOut = Console.ReadLine();
            portOut = int.Parse(strPuertoOut);

            Console.WriteLine("\nOprima enter para realizar una petición de registro a una aplicación web");
            Console.ReadLine();
            bool flagRegistro = false;
            while (!flagRegistro)
            {
                try
                {
                    IPEndPoint remoteEP = new IPEndPoint(new IPAddress(ipOut), 555);
                    sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    sender.Connect(remoteEP);
                    byte[] msg = Encoding.ASCII.GetBytes(ipOut[0] + "." + ipOut[1] + "." + ipOut[2] + "." + ipOut[3] + ":" + portOut + "<EOF>");
                    int bytesSent = sender.Send(msg);
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                    Console.WriteLine("\nEl registro fue exitoso, oprima enter para continuar");
                    Console.ReadLine();
                    Thread.Sleep(1000);
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Registro no exitoso, oprima enter para intentar nuevamente");
                    Console.ReadLine();
                    flagRegistro = false;
                }
            }

            Thread thread = new Thread(new ThreadStart(ProcesarDatos));
            thread.Start();

            Console.WriteLine("\nRecibiendo datos desde: " + ipIn[0] + "." + ipIn[1] + "." + ipIn[2] + "." + ipIn[3] + ":" + portIn + "");
            Console.WriteLine("Enviando datos hacia: " + ipOut[0] + "." + ipOut[1] + "." + ipOut[2] + "." + ipOut[3] + ":" + portOut + ", oprima enter para terminar");
            Console.ReadLine();
            TerminarConexiones();
        }

        private static void ProcesarDatos()
        {
            string data;
            byte[] bytes = new Byte[1024];
            try
            {
                IPEndPoint localEP = new IPEndPoint(new IPAddress(ipIn), portIn);
                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint remoteEP = new IPEndPoint(new IPAddress(ipOut), portOut);
                sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sender.Connect(remoteEP);
                listener.Bind(localEP);
                listener.Listen(30);
                while (true)
                {
                    handler = listener.Accept();
                    data = null;
                    DateTime fechaIni = DateTime.Now;
                    while (true)
                    {
                        bytes = new byte[1024];
                        int bytesRec = handler.Receive(bytes);
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        TimeSpan span = DateTime.Now - fechaIni;
                        int ms = (int)span.TotalMilliseconds;
                        if (ms > 1000)
                        {
                            if (data.Length > 0)
                                data = data.Substring(0, data.Length - 1);
                            if (!ValidarEntrada(data))
                            {
                                TerminarConexiones();
                                break;
                            }
                            int suma = CalcularSuma(data);
                            EnviarDatos(suma);
                            Console.WriteLine("Text received : {0}", data);
                            Console.WriteLine("Text received : {0}", suma);
                            data = null;
                            fechaIni = DateTime.Now;
                        }
                    }
                    break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void EnviarDatos(int valor)
        {
            try
            {
                byte[] msg = Encoding.ASCII.GetBytes(valor.ToString() + ",");
                int bytesSent = sender.Send(msg);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void TerminarConexiones()
        {
            try
            {
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static int CalcularSuma(string data)
        {
            int resp = 0;
            string[] arrayData = data.Split(new char[] { ',' });
            foreach (string item in arrayData)
            {
                resp += int.Parse(item);
            }
            return resp;
        }

        private static bool ValidarEntrada(string data)
        {
            int entrada;
            string[] arrayData = data.Split(new char[] { ',' });
            foreach (string item in arrayData)
            {
                if (!int.TryParse(item, out entrada))
                    return false;
            }
            return true;
        }

    }
}
