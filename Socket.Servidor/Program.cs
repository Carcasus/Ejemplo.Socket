using LibreriaDeDatos;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Calculator.Servidor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            //IPAddress ipAddress = IPAddress.Parse("ip cliente"); //En el caso de usar una ip que no sea localhost
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 2800);
            try
            {
                // Creamos un socket que use el protocolo tcp
                using Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                // Un socket debe estar asociado con un endpoint usando el metodo Bind
                listener.Bind(localEndPoint);
                // Especificamos que habra hasta 10 solicitudes antes de que el servidor entre en estado de ocupado
                listener.Listen(10);
                Console.WriteLine("Waiting for a connection ..." + listener.LocalEndPoint.ToString());
                Socket handler = listener.Accept();
                //Recogemos los datos que llegaran desde cliente dentro del metodo Deserializacion
                Deserializacion(handler);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("\n Press any key to continue...");
            Console.ReadKey();
        }

        private static void Deserializacion(Socket handler)
        {
            Console.WriteLine("Socket connected to {0}", handler.RemoteEndPoint.ToString());
            var cacheMenaje = new byte[4096];
            int bytesMenaje = handler.Receive(cacheMenaje);

            var mensaje = Encoding.UTF8.GetString(cacheMenaje, 0, bytesMenaje);
            var datosOperacion = JsonSerializer.Deserialize<DatosOperacion>(mensaje); //Deserializamos el mensaje recibido del cliente
            if (bytesMenaje > 0) //Comprueba que no llega un mensaje vacio
            {
                ObjetoRespuesta resultado = TratamientoMensaje(datosOperacion); //Se hace la operacion y recogemos en el objeto el resultado
                //Enviamos de vuelta los datos actualizados al cliente
                Serializacion(handler, resultado);
            }
        }

        private static ObjetoRespuesta TratamientoMensaje(DatosOperacion datosOperacion)
        {
            String descripcion = "";
            double respuesta = 0;
            //Pasan los datos a su if correspondiente y se hace la operacion
            if (datosOperacion.Operacion == TipoOperacion.Suma)
            {
                respuesta = datosOperacion.Operador1 + datosOperacion.Operador2;
                descripcion = "La suma entre " + datosOperacion.Operador1 + " y " + datosOperacion.Operador2 + " daria como resultado = " + respuesta;
                Console.WriteLine("{0} -> {1}", datosOperacion, descripcion);
            }
            else if (datosOperacion.Operacion == TipoOperacion.Resta)
            {
                respuesta = datosOperacion.Operador1 - datosOperacion.Operador2;
                descripcion = "La resta entre " + datosOperacion.Operador1 + " y " + datosOperacion.Operador2 + " daria como resultado = " + respuesta;
                Console.WriteLine("{0} -> {1}", datosOperacion, descripcion);
            }
            else if (datosOperacion.Operacion == TipoOperacion.Multiplicacion)
            {
                respuesta = datosOperacion.Operador1 * datosOperacion.Operador2;
                descripcion = "La multiplicacion entre " + datosOperacion.Operador1 + " y " + datosOperacion.Operador2 + " daria como resultado = " + respuesta;
                Console.WriteLine("{0} -> {1}", datosOperacion, descripcion);
            }
            else if (datosOperacion.Operacion == TipoOperacion.Division)
            {
                if (datosOperacion.Operador2 == 0.0)//Si el segundo operador es 0, impedimos que se haga calculo alguno
                {
                    Console.WriteLine("No se dividir entre 0");
                    descripcion = "No se dividir entre 0";
                }
                else
                {
                    respuesta = datosOperacion.Operador1 / datosOperacion.Operador2;
                    descripcion = "La division entre " + datosOperacion.Operador1 + " y " + datosOperacion.Operador2 + " daria como resultado = " + respuesta;
                }
                Console.WriteLine("{0} -> {1}", datosOperacion, descripcion);
            }
            ObjetoRespuesta resultado = new(respuesta, descripcion);
            return resultado;
        }
        private static void Serializacion(Socket handler, ObjetoRespuesta resultado)
        {
            string jsonString = JsonSerializer.Serialize(resultado);
            var cacheRespuesta = Encoding.UTF8.GetBytes(jsonString);
            handler.Send(cacheRespuesta); //Se envia la respuesta al cliente
            Thread.Sleep(0);
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
    }
}
