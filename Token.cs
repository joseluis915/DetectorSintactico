using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectorSintactico
{
    class Token
    {
        public Token(string name, string informacion, int index, int linea, int columna)
        {
            Name = name;
            Informacion = informacion;
            Index = index;
            Linea = linea;
            Columna = columna;
        }

        //———————————————————————————————————————————————[ Getters y setters ]———————————————————————————————————————————————
        public string Name { get; set; }
        public string Informacion { get; private set; }
        public int Index { get; private set; }
        public int Linea { get; private set; }
        public int Columna { get; private set; }
        public int Lenght { get { return Informacion.Length; } }
    }
}