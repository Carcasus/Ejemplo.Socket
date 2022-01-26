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
            double resultado = 0;
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
                while (true)
                {
                    tratamientoMensaje(listener, resultado);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("\n Press any key to continue...");
            Console.ReadKey();
        }

        private static void tratamientoMensaje(Socket listener, double resultado)
        {
            Socket handler = listener.Accept();
            Console.WriteLine("Socket connected to {0}",
                handler.RemoteEndPoint.ToString());
            var cacheMenaje = new byte[4096];
            int bytesMenaje = handler.Receive(cacheMenaje);
            var respuesta = "";
            if (bytesMenaje > 0) //Comprueba que no llega un mensaje vacio
            {
                var mensaje = Encoding.UTF8.GetString(cacheMenaje, 0, bytesMenaje);
                var obj = JsonSerializer.Deserialize<DatosOperacion>(mensaje); //Deserializamos el mensaje recibido del cliente

                respuesta = hacerLaOperacion(resultado, obj, respuesta, mensaje); //Tratamos los datos en un metodo aparte
            }

            enviarRespuesta(respuesta, handler);
        }

        //Ejecutamos la operacion recibida de uno de los clientes, retornamos la respuesta, que sera enviada de vuelta al cliente.
        private static string hacerLaOperacion(double resultado, DatosOperacion obj, string respuesta, string mensaje)
        {
            //Pasan los datos a su if correspondiente y se hace la operacion
            if (obj.operacion == TipoOperacion.Suma)
            {
                resultado = obj.Operador1 + obj.Operador2;
                respuesta = "La suma entre " + obj.Operador1 + " y " + obj.Operador2 + " daria como resultado = " + resultado;

                Console.WriteLine("{0} -> {1}", mensaje, respuesta);
            }
            else if (obj.operacion == TipoOperacion.Resta)
            {
                resultado = obj.Operador1 - obj.Operador2;
                respuesta = "La resta entre " + obj.Operador1 + " y " + obj.Operador2 + " daria como resultado = " + resultado;

                Console.WriteLine("{0} -> {1}", mensaje, respuesta);
            }
            else if (obj.operacion == TipoOperacion.Multiplicacion)
            {
                resultado = obj.Operador1 * obj.Operador2;
                respuesta = "La multiplicacion entre " + obj.Operador1 + " y " + obj.Operador2 + " daria como resultado = " + resultado;

                Console.WriteLine("{0} -> {1}", mensaje, respuesta);
            }
            else if (obj.operacion == TipoOperacion.Division)
            {
                //Si el segundo operador es 0, impedimos que se haga calculo alguno
                if (obj.Operador2 == 0.0)
                {
                    Console.WriteLine("No se dividir entre 0");
                    respuesta = "No se dividir entre 0";
                }
                else
                {
                    resultado = obj.Operador1 / obj.Operador2;
                    respuesta = "La division entre " + obj.Operador1 + " y " + obj.Operador2 + " daria como resultado = " + resultado;
                }
                Console.WriteLine("{0} -> {1}", mensaje, respuesta);
            }
            return respuesta;
        }

        private static void enviarRespuesta(string respuesta, Socket handler)
        {
            var cacheRespuesta = Encoding.UTF8.GetBytes(respuesta);
            handler.Send(cacheRespuesta); //Se envia la respuesta al cliente
            Thread.Sleep(0);

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
    }
}
