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
            PosiblesOperadores(operador, operando1, operando2);
        }

        //Por medio de cuatro divisores "if", pasamos el valor de operador a minuscula y comprobamos a que operacion debemos ir
        private static void PosiblesOperadores(string operador, double operando1, double operando2)
        {
            if (operador.ToLower() == "add")
            {
                //Se ha de crear un objeto operacion, para la comunicación del servidor al cliente.
                DatosOperacion operacion = new() { Operador1 = operando1, Operador2 = operando2, Operacion = TipoOperacion.Suma};
                TratamientoMensaje(operacion); //Se envian los datos al metodo principal y al servidor
            }
            else if (operador.ToLower() == "sub")
            {
                DatosOperacion operacion = new() { Operador1 = operando1, Operador2 = operando2, Operacion = TipoOperacion.Resta};
                TratamientoMensaje(operacion);
            }
            else if (operador.ToLower() == "plus")
            {
                DatosOperacion operacion = new() { Operador1 = operando1, Operador2 = operando2, Operacion = TipoOperacion.Multiplicacion};
                TratamientoMensaje(operacion);
            }
            else if (operador.ToLower() == "div")
            {
                DatosOperacion operacion = new() { Operador1 = operando1, Operador2 = operando2, Operacion = TipoOperacion.Division};
                TratamientoMensaje(operacion);
            }
            else
            {
                Console.WriteLine("Valores de operacion no validos");
                Console.Write("Pulse cualquier tecla para cerrar la ejecucion de la consola...");
                Console.ReadKey();
            }
        }

        private static void TratamientoMensaje(DatosOperacion operacion)
        {
            try
            {
                IPHostEntry host = Dns.GetHostEntry("localhost");
                //Conectamos el cliente con el servidor, bien con el localhost o bien con una ip
                IPAddress ipAddress = host.AddressList[0];
                //IPAddress ipAddress = IPAddress.Parse("192.168.100.126");
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 2800);
                // Creamos el socket por medio de TCP / IP
                using Socket sender = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    sender.Connect(remoteEP);
                    //Serializamos el objeto de la operacion para poder enviarlo por el socket
                    Serializacion(sender, operacion);

                    //Deserializamos el mensaje recibido y mostramos por consola el resultado
                    Deserializacion(sender);

                    // cerramos el socket.
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
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
        }

        private static void Serializacion(Socket sender, DatosOperacion operacion)
        {
            Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());
            Console.WriteLine("Socket redad for {0}", sender.LocalEndPoint.ToString());
            string jsonString = JsonSerializer.Serialize(operacion);
            var cacheEnvioOperacion = Encoding.UTF8.GetBytes(jsonString);

            // Se envian los datos a traves del socket.
            int bytesSendOperador = sender.Send(cacheEnvioOperacion); //Pasar el objeto serializado;
        }

        private static void Deserializacion(Socket sender)
        {
            byte[] bufferRec = new byte[1024];
            int bytesRec1 = sender.Receive(bufferRec);
            var mensaje = Encoding.UTF8.GetString(bufferRec, 0, bytesRec1);

            double resultado = 0;
            String descripcion = "";
            ObjetoRespuesta respuesta = new(resultado, descripcion);

            respuesta = JsonSerializer.Deserialize<ObjetoRespuesta>(mensaje); //Deserializamos el objeto recibido del cliente y lo guardamos en respuesta

            Console.WriteLine("Respuesta Recibida: " + respuesta.Respuesta + "\nDescripcion Recibida: " + respuesta.Descripcion);
            Console.WriteLine("___________________________________________________");
            Console.Write("Pulse cualquier tecla para cerrar la ejecucion de la consola...");
            Console.ReadKey();
        }
    }
}