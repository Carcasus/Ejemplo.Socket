using LibreriaDeDatos;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Calculator.Servidor
{
    internal class Program
    {
        static void  Main(string[] args)
        {

            double resultado = 0;

            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];

            //IPAddress ipAddress = IPAddress.Parse("ip escucha");

            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 2800);

            try
            {
                // Create a Socket that will use Tcp protocol
                using Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                // A Socket must be associated with an endpoint using the Bind method
                listener.Bind(localEndPoint);
                // Specify how many requests a Socket can listen before it gives Server busy response.
                // We will listen 10 requests at a time
                listener.Listen(10);

                Console.WriteLine("Waiting for a connection ..." + listener.LocalEndPoint.ToString());

                while (true)
                {
                    
                    Socket handler = listener.Accept();

                    Console.WriteLine("Socket connected to {0}",
                        handler.RemoteEndPoint.ToString());

                    var cacheMenaje = new byte[4096];
                    
                    int bytesMenaje = handler.Receive(cacheMenaje);

                    if (bytesMenaje > 0) //Comprueba que no llega un mensaje vacio
                    {
                        var mensaje = Encoding.UTF8.GetString(cacheMenaje, 0, bytesMenaje); 
                        var obj = JsonSerializer.Deserialize<DatosOperacion>(mensaje); //Deserializamos el mensaje recibido del cliente
                        
                        //Pasan los datos a su if correspondiente y se hace la operacion
                        if (obj.operacion == TipoOperacion.Suma)
                        {
                            resultado = obj.Operador1 + obj.Operador2;
                            var respuesta = "La suma entre "+obj.Operador1+" y "+obj.Operador2+" daria como resultado = " + resultado;

                            Console.WriteLine("{0} -> {1}", mensaje, respuesta);

                            var cacheRespuesta = Encoding.UTF8.GetBytes(respuesta);
                            handler.Send(cacheRespuesta); //Se envia la respuesta al cliente
                            Thread.Sleep(0);
                        }
                        else if (obj.operacion == TipoOperacion.Resta)
                        {
                            resultado = obj.Operador1 - obj.Operador2;
                            var respuesta = "La resta entre " + obj.Operador1 + " y " + obj.Operador2 + " daria como resultado = " + resultado;

                            Console.WriteLine("{0} -> {1}", mensaje, respuesta);

                            var cacheRespuesta = Encoding.UTF8.GetBytes(respuesta);
                            handler.Send(cacheRespuesta);
                            Thread.Sleep(0);
                        }
                        else if (obj.operacion == TipoOperacion.Multiplicacion)
                        {
                            resultado = obj.Operador1 * obj.Operador2;
                            var respuesta = "La multiplicacion entre " + obj.Operador1 + " y " + obj.Operador2 + " daria como resultado = " + resultado;

                            Console.WriteLine("{0} -> {1}", mensaje, respuesta);

                            var cacheRespuesta = Encoding.UTF8.GetBytes(respuesta);
                            handler.Send(cacheRespuesta);
                            Thread.Sleep(0);
                        }
                        else if (obj.operacion == TipoOperacion.Division)
                        {
                            resultado = obj.Operador1 / obj.Operador2;
                            var respuesta = "La division entre " + obj.Operador1 + " y " + obj.Operador2 + " daria como resultado = " + resultado;

                            Console.WriteLine("{0} -> {1}", mensaje, respuesta);

                            var cacheRespuesta = Encoding.UTF8.GetBytes(respuesta);
                            handler.Send(cacheRespuesta);
                            Thread.Sleep(0);
                        }
                        
                    }

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\n Press any key to continue...");
            Console.ReadKey();
        }
    }
}
