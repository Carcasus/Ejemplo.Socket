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
            string operando1 = "";
            string operando2 = "";

            int codigoFinal = 0;
            
            Console.WriteLine("Escriba la operacion a realizar (add, sub, plus o div):");
            Console.WriteLine("------------------------\n");
            operador = Console.ReadLine();

            if (operador == "add" || operador == "sub" || operador == "plus" || operador == "div")
            {
                Console.WriteLine("Escriba ahora los dos valores numericos a tratar");
                operando1 = Console.ReadLine();
                operando2 = Console.ReadLine();
                Operacion operacion = new Operacion
                { operador1 = operando1,
                };
                var resultado = EnviaMensaje(operador, operando1, operando2);
            }
            else
                Console.WriteLine("Valores de operacion no validos");

            Console.Write("Press any key to close the Calculator console app...");
            Console.ReadKey();
        }

        static string EnviaMensaje(Operacion operacion)
        {
            try
            {
                double num1 = 0;
                double num2 = 0;
                if (!double.TryParse(operando1, out num1) || !double.TryParse(operando2, out num2))
                {
                    Console.WriteLine("La operacion dara error por que los operandos no son numericos");
                }
                else
                {
                    // Connect to a Remote server
                    // Get Host IP Address that is used to establish a connection
                    // In this case, we get one IP address of localhost that is IP : 127.0.0.1
                    // If a host has multiple addresses, you will get a list of addresses

                    IPHostEntry host = Dns.GetHostEntry("localhost");
                    IPAddress ipAddress = host.AddressList[0];

                    //IPAddress ipAddress = IPAddress.Parse("192.168.100.126");

                    IPEndPoint remoteEP = new IPEndPoint(ipAddress, 2800);

                    // Create a TCP/IP  socket.
                    using Socket sender = new Socket(ipAddress.AddressFamily,
                        SocketType.Stream, ProtocolType.Tcp);

                    // Connect the socket to the remote endpoint. Catch any errors.

                    try
                    {
                        // Connect to Remote EndPoint
                        sender.Connect(remoteEP);

                        Console.WriteLine("Socket connected to {0}",
                            sender.RemoteEndPoint.ToString());

                        Console.WriteLine("Socket redad for {0}",
                            sender.LocalEndPoint.ToString());

                        var cacheEnvioOperador = Encoding.UTF8.GetBytes(operador);
                        var cacheEnvioOperando1 = Encoding.UTF8.GetBytes(operando1);
                        var cacheEnvioOperando2 = Encoding.UTF8.GetBytes(operando2);

                        // Send the data through the socket.
                        int bytesSendOperador = sender.Send(cacheEnvioOperador); //Pasar el objeto serializado

                        // Receive the response from the remote device.
                        byte[] bufferRec = new byte[1024];
                        int bytesRec1 = sender.Receive(bufferRec);

                        var resultado = Encoding.UTF8.GetString(bufferRec, 0, bytesRec1);

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
