using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
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

            ObjetoRespuesta resultado = new ObjetoRespuesta();

            Console.WriteLine("Escriba la operacion a realizar (add, sub, plus o div):");
            Console.WriteLine("------------------------\n");
            operador = Console.ReadLine();
            try
            {
                Console.WriteLine("Escriba ahora los dos valores numericos a tratar, separados por enter");
                Console.WriteLine("[VALOR 1]");
                Console.WriteLine("[VALOR 2]");
                operando1 = double.Parse(Console.ReadLine());
                operando2 = double.Parse(Console.ReadLine());
            }
            catch (FormatException e)
            {
                Console.WriteLine("Error. el valor introducido no es numerico" + e.ToString());
            }

            //Una vez declarado el operador y los numeros operandos, se pasara a una de las cuatro opciones en el metodo aparte
            resultado = (ObjetoRespuesta)PosiblesOperadores(operador, operando1, operando2, resultado);

            Console.WriteLine(resultado.Respuesta);
            Console.WriteLine("___________________________________________________");
            Console.Write("Pulse cualquier tecla para cerrar la ejecucion de la consola...");
            Console.ReadKey();
        }

        private static object PosiblesOperadores(string operador, double operando1, double operando2, ObjetoRespuesta resultado)
        {
            if (operador == "add")
            {
                //Se ha de crear un objeto operacion, para la comunicación del servidor al cliente.
                DatosOperacion operacion = new DatosOperacion
                {
                    Operador1 = operando1,
                    Operador2 = operando2,
                    Operacion = TipoOperacion.Suma
                };
                resultado = (ObjetoRespuesta)EnviaMensajeAsync(operacion); //Se envia al metodo principal (y al cliente), el resultado tratado lo devolvera a esta linea y saldra del if
            }
            else if (operador == "sub")
            {
                DatosOperacion operacion = new DatosOperacion
                {
                    Operador1 = operando1,
                    Operador2 = operando2,
                    Operacion = TipoOperacion.Resta
                };
                resultado = (ObjetoRespuesta)EnviaMensajeAsync(operacion);
            }
            else if (operador == "plus")
            {
                DatosOperacion operacion = new DatosOperacion
                {
                    Operador1 = operando1,
                    Operador2 = operando2,
                    Operacion = TipoOperacion.Multiplicacion
                };
                resultado = (ObjetoRespuesta)EnviaMensajeAsync(operacion);
            }
            else if (operador == "div")
            {
                DatosOperacion operacion = new DatosOperacion
                {
                    Operador1 = operando1,
                    Operador2 = operando2,
                    Operacion = TipoOperacion.Division
                };
                resultado = (ObjetoRespuesta)EnviaMensajeAsync(operacion);
            }
            else
                Console.WriteLine("Valores de operacion no validos");
            return resultado;
        }

        static object EnviaMensajeAsync(DatosOperacion operacion)
        {
            ObjetoRespuesta resultado = new ObjetoRespuesta();
            try
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
                    //Ponemos en metodo aparte toda la conexion (envio y recibimiento) del mensaje
                    resultado = (ObjetoRespuesta)ConexionServidor(sender, remoteEP, resultado, operacion); 
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
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return resultado;
        }

        private static object ConexionServidor(Socket sender, IPEndPoint remoteEP, ObjetoRespuesta resultado, DatosOperacion operacion)
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

            // Se envian los datos a traves del socket.
            int bytesSendOperador = sender.Send(cacheEnvioOperacion); //Pasar el objeto serializado

            resultado = (ObjetoRespuesta)RetornoDesdeServidor(sender, resultado); //Recibimos el resultado desde el servidor en este metodo
            return resultado;
        }

        private static object RetornoDesdeServidor(Socket sender, ObjetoRespuesta resultado)
        {
            // Recibimos respuesta desde el servidor.
            byte[] bufferRec = new byte[1024];
            int bytesRec1 = sender.Receive(bufferRec);

            //Deserializamos el mensaje recibido
            resultado.Respuesta = Encoding.UTF8.GetString(bufferRec, 0, bytesRec1);
            // cerramos el socket.
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();

            return resultado;
        }
    }
}
