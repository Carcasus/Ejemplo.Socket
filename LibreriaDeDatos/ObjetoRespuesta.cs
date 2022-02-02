using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibreriaDeDatos
{
    public class ObjetoRespuesta
    {
        public ObjetoRespuesta(double respuesta, string descripcion)
        {
            Respuesta = respuesta;
            Descripcion = descripcion;
        }

        public DatosOperacion DatosOperacion { get; set; }
        public Double Respuesta { get; set; }
        public String Descripcion { get; set; }
    }
}
