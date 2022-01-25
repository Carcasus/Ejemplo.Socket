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

            
            var resultado= "";
            
            Console.WriteLine("Escriba la operacion a realizar (add, sub, plus o div):");
            Console.WriteLine("------------------------\n");
            operador = Console.ReadLine();

                Console.WriteLine("Escriba ahora los dos valores numericos a tratar");
                operando1 = double.Parse(Console.ReadLine());
                operando2 = double.Parse(Console.ReadLine());

            //Una vez declarado el operador y los numeros operandos, se pasara a una de las cuatro opciones en el metodo aparte
            resultado = posiblesOperadores(operador, operando1, operando2, resultado);
            
            Console.WriteLine("El resultado es " + resultado);
            Console.WriteLine("___________________________________________________");
            Console.Write("Press any key to close the Calculator console app...");
            Console.ReadKey();
        }

        private static string posiblesOperadores(string operador, double operando1, double operando2, string resultado)
        {
            if (operador == "add")
            {
                //Se ha de crear un objeto operacion, para la comunicación del servidor al cliente.
                DatosOperacion operacion = new DatosOperacion
                {
                    Operador1 = operando1,
                    Operador2 = operando2,
                    operacion = TipoOperacion.Suma
                };
                resultado = EnviaMensajeAsync(operacion); //Se envia al metodo principal (y al cliente), el resultado tratado lo devolvera a esta linea y saldra del if
            }
            else if (operador == "sub")
            {
                DatosOperacion operacion = new DatosOperacion
                {
                    Operador1 = operando1,
                    Operador2 = operando2,
                    operacion = TipoOperacion.Resta
                };
                resultado = EnviaMensajeAsync(operacion);
            }
            else if (operador == "plus")
            {
                DatosOperacion operacion = new DatosOperacion
                {
                    Operador1 = operando1,
                    Operador2 = operando2,
                    operacion = TipoOperacion.Multiplicacion
                };
                resultado = EnviaMensajeAsync(operacion);
            }
            else if (operador == "div")
            {
                DatosOperacion operacion = new DatosOperacion
                {
                    Operador1 = operando1,
                    Operador2 = operando2,
                    operacion = TipoOperacion.Division
                };
                resultado = EnviaMensajeAsync(operacion);
            }
            else
                Console.WriteLine("Valores de operacion no validos");
            return resultado;
        }

        static string EnviaMensajeAsync(DatosOperacion operacion)
        {
            var resultado = "";
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
                    //Conectamos el cliente con el servidor, bien con el localhost o bien con una ip
                    IPAddress ipAddress = host.AddressList[0]; 
                    //IPAddress ipAddress = IPAddress.Parse("192.168.100.126");

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
                        resultado = Encoding.UTF8.GetString(bufferRec, 0, bytesRec1);
                        // Release the socket.
                        sender.Shutdown(SocketShutdown.Both);
                        sender.Close();

                        return resultado;

                    }
                    catch (ArgumentNullException ane)
                    {
                        Console.WriteLine("Error, No se estan pasando los suficientes datos : {0}", ane.ToString());
                    }
                    catch (SocketException se)
                    {
                        Console.WriteLine("Error en los sockets : {0}", se.ToString());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error, Fallo inesperado : {0}", e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return resultado;
        }
    }
}
