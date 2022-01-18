using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Linq;
using LibreriaDeDatos;

namespace Calculator.Cliente
{
    internal class Program
    {
        static void Main(string[] args)
        {
            String operador = "";
            double operando1 = 0;
            double operando2 = 0;

            int codigoFinal = 0;
            
            Console.WriteLine("Escriba la operacion a realizar (add, sub, plus o div):");
            Console.WriteLine("------------------------\n");
            operador = Console.ReadLine();

                Console.WriteLine("Escriba ahora los dos valores numericos a tratar");
                operando1 = double.Parse(Console.ReadLine());
                operando2 = double.Parse(Console.ReadLine());

            if (operador == "add"){
                DatosOperacion operacion = new DatosOperacion
                {
                    Operador1 = operando1,
                    Operador2 = operando2,
                    operacion = TipoOperacion.Suma
                };
                var resultado = EnviaMensajeAsync(operacion);
            }
            else if (operador == "sub")
            {
                DatosOperacion operacion = new DatosOperacion
                {
                    Operador1 = operando1,
                    Operador2 = operando2,
                    operacion = TipoOperacion.Resta
                };
                var resultado = EnviaMensajeAsync(operacion);
            }
            else if (operador == "plus")
            {
                DatosOperacion operacion = new DatosOperacion
                {
                    Operador1 = operando1,
                    Operador2 = operando2,
                    operacion = TipoOperacion.Multiplicacion
                };
                var resultado = EnviaMensajeAsync(operacion);
            }
            else if (operador == "div")
            {
                DatosOperacion operacion = new DatosOperacion
                {
                    Operador1 = operando1,
                    Operador2 = operando2,
                    operacion = TipoOperacion.Division
                };
                var resultado = EnviaMensajeAsync(operacion);
            }
            else
                Console.WriteLine("Valores de operacion no validos");

            Console.Write("Press any key to close the Calculator console app...");
            Console.ReadKey();
        }

        static async System.Threading.Tasks.Task<string> EnviaMensajeAsync(DatosOperacion operacion)
        {
            try
            {
                double num1 = 0;
                double num2 = 0;
                if (!double.TryParse(operacion.Operador1.ToString(), out num1) || !double.TryParse(operacion.Operador2.ToString(), out num2))
                {
                    Console.WriteLine("La operacion dara error por que los operandos no son numericos");
                }
                else
                {
                    IPHostEntry host = Dns.GetHostEntry("localhost");
                    //IPAddress ipAddress = host.AddressList[0];

                    IPAddress ipAddress = IPAddress.Parse("192.168.100.126");

                    IPEndPoint remoteEP = new IPEndPoint(ipAddress, 2800);

                    // Creamos el socket por medio de TCP / IP
                    using Socket sender = new Socket(ipAddress.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp);
                    try
                    {
                        // Connect to Remote EndPoint
                        sender.Connect(remoteEP);

                        Console.WriteLine("Socket connected to {0}",
                            sender.RemoteEndPoint.ToString());

                        Console.WriteLine("Socket redad for {0}",
                            sender.LocalEndPoint.ToString());

                        //Serializamos el objeto de la operacion para poder enviarlo por el socket
                        string jsonString = JsonSerializer.Serialize(operacion);
                        var cacheEnvioOperacion = Encoding.UTF8.GetBytes(jsonString);

                        // Send the data through the socket.
                        int bytesSendOperador = sender.Send(cacheEnvioOperacion); //Pasar el objeto serializado

                        // Receive the response from the remote device.
                        byte[] bufferRec = new byte[1024];
                        int bytesRec1 = sender.Receive(bufferRec);

                        //Deserializamos el mensaje recibido
                        var resultado = Encoding.UTF8.GetString(bufferRec, 0, bytesRec1);
                        //var obj = await JsonSerializer.DeserializeAsync<T>(resultado);
                        // Release the socket.
                        sender.Shutdown(SocketShutdown.Both);
                        sender.Close();

                        return resultado;

                    }
                    catch (ArgumentNullException ane)
                    {
                        Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                    }
                    catch (SocketException se)
                    {
                        Console.WriteLine("SocketException : {0}", se.ToString());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Unexpected exception : {0}", e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return null;
        }
    }
}
