using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DetectorSintactico
{
    class Detector
    {
        Regex detect;
        StringBuilder patron;
        bool necesitaCompilar;
        List<string> nombresToken;
        int[] grupoNumeros;

        public Detector()
        {
            necesitaCompilar = true;
            nombresToken = new List<string>();
        }

        //———————————————————————————————————————————————[ Esta funcion agrega una nueva regla para reconocer token ]———————————————————————————————————————————————
        /// <param name="Patron"> Patron en el que debe encajar</param>
        /// <param name="nToken"> Nombre Token, Id único para este patrón</param>
        /// <param name="Pasar"> True para no devolver este token</param>
        public void AgregarReglaToken(string Patron, string nToken, bool Pasar = false)
        {
            if (string.IsNullOrWhiteSpace(nToken))
                throw new ArgumentException(string.Format("{0} No es un nombre válido.", nToken));

            if (string.IsNullOrEmpty(Patron))
                throw new ArgumentException(string.Format("El patrón {0} no es válido.", Patron));

            if (patron == null)
                patron = new StringBuilder(string.Format("(?<{0}>{1})", nToken, Patron));
            else
                patron.Append(string.Format("|(?<{0}>{1})", nToken, Patron));

            if (!Pasar)
                nombresToken.Add(nToken);

            necesitaCompilar = true;
        }
        //———————————————————————————————————————————————[ Esta funcion Reinicia el Detector ]———————————————————————————————————————————————
        public void ReiniciarDetector()
        {
            detect = null;
            patron = null;
            necesitaCompilar = true;
            nombresToken.Clear();
            grupoNumeros = null;
        }

        //———————————————————————————————————————————————[ Esta funcion analiza una entrada en busca de tokens validos y errores ]———————————————————————————————————————————————
        /// <param name="texto"> entrada a analizar</param>
        public IEnumerable<Token> GetTokens(string texto)
        {
            if (necesitaCompilar) throw new Exception("Compilación Requerida, llame al método Compilar.");
            Match encontrado = detect.Match(texto);

            if (!encontrado.Success) yield break;
            int linea = 1, iniciar = 0, indice = 0;

            while (encontrado.Success)
            {
                if (encontrado.Index > indice)
                {
                    string token = texto.Substring(indice, encontrado.Index - indice);
                    yield return new Token("ERROR", token, indice, linea, (indice - iniciar) + 1);
                    linea += ContarNuevasLineas(token, indice, ref iniciar);
                }

                for (int i = 0; i < grupoNumeros.Length; i++)
                {
                    if (encontrado.Groups[grupoNumeros[i]].Success)
                    {
                        string name = detect.GroupNameFromNumber(grupoNumeros[i]);
                        yield return new Token(name, encontrado.Value, encontrado.Index, linea, (encontrado.Index - iniciar) + 1);

                        break;
                    }
                }

                linea += ContarNuevasLineas(encontrado.Value, encontrado.Index, ref iniciar);
                indice = encontrado.Index + encontrado.Length;
                encontrado = encontrado.NextMatch();
            }

            if (texto.Length > indice)
            {
                yield return new Token("ERROR", texto.Substring(indice), indice, linea, (indice - iniciar) + 1);
            }
        }

        //———————————————————————————————————————————————[ Esta funcion crea el AFN con los patrones establecidos  ]———————————————————————————————————————————————
        public void Compilar(RegexOptions options)
        {
            if (patron == null) throw new Exception("Agrege uno o más patrones, llame al método AddTokenRule(Patron, nToken).");

            if (necesitaCompilar)
            {
                try
                {
                    detect = new Regex(patron.ToString(), options);

                    grupoNumeros = new int[nombresToken.Count];
                    string[] nGrupos = detect.GetGroupNames();

                    for (int i = 0, indic = 0; i < nGrupos.Length; i++)
                    {
                        if (nombresToken.Contains(nGrupos[i]))
                        {
                            grupoNumeros[indic++] = detect.GroupNumberFromName(nGrupos[i]);
                        }
                    }

                    necesitaCompilar = false;
                }
                catch (Exception exept)
                {
                    throw exept;
                }
            }
        }

        //———————————————————————————[ Esta funcion cuenta la cantidad de lineas presentes en un token, establece el inicio de linea. ]————————————————————————————
        private int ContarNuevasLineas(string token, int indice, ref int inicioLinea)
        {
            int linea = 0;

            for (int i = 0; i < token.Length; i++)
                if (token[i] == '\n')
                {
                    linea++;
                    inicioLinea = indice + i + 1;
                }

            return linea;
        }
    }
}