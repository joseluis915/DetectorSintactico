using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace DetectorSintactico
{
    //—————————————————————————————————————————————[ Lógica de interacción para la ventana principal ]—————————————————————————————————————————————

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        Detector detector = new Detector();
        bool Recargar;
        List<string> palabrasReservadas;

        private void Logica(object sender, RoutedEventArgs e)
        {
            using (StreamReader sr = new StreamReader(@"..\..\Detector.cs"))
            {
                //———————————————————————————————————————————————[ Tokens ]———————————————————————————————————————————————
                //EvaluadorCodigoTextBox.Text = sr.ReadToEnd();    //Evaluar parte del codigo de este programa.
                detector.AgregarReglaToken(@"\s+", "ESPACIO", true);
                detector.AgregarReglaToken(@"\b[_a-zA-Z][\w]*\b", "IDENTIFICADOR");
                detector.AgregarReglaToken("\".*?\"", "CADENA");
                detector.AgregarReglaToken(@"'\\.'|'[^\\]'", "CARACTER");
                detector.AgregarReglaToken("//[^\r\n]*", "COMENTARIO");
                detector.AgregarReglaToken("/[*].*?[*]/", "COMENTARIO2");
                detector.AgregarReglaToken(@"\d*\.?\d+", "NUMERO");
                detector.AgregarReglaToken(@"[\(\)\{\}\[\];,]", "DELIMITADOR");
                detector.AgregarReglaToken(@"[\.=\+\-/*%]", "OPERADOR");
                detector.AgregarReglaToken(@">|<|==|>=|<=|!", "COMPARADOR");

                //———————————————————————————————————————————————[ Palabras reservadas ]———————————————————————————————————————————————
                palabrasReservadas = new List<string>()
                {
                    "abstract", "as", "async", "await", "checked", "const", "continue", "default", "delegate", "base", "break", "case",
                    "do", "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed", "for", "foreach", "goto", "if",
                    "implicit", "in", "interface", "internal", "is", "lock", "new", "null", "operator","catch", "out", "override", "params",
                    "private", "protected", "public", "readonly", "ref", "return", "sealed", "sizeof", "stackalloc", "static", "switch",
                    "this", "throw", "true", "try", "typeof", "namespace", "unchecked", "unsafe", "virtual", "void", "while", "float", "int",
                    "long", "object", "get", "set", "new", "partial", "yield", "add", "remove", "value", "alias", "ascending", "descending",
                    "from", "group", "into", "orderby", "select", "where", "join", "equals", "using","bool", "byte", "char", "decimal",
                    "double", "dynamic", "sbyte", "short", "string", "uint", "ulong", "ushort", "var", "class", "struct" 
                };

                detector.Compilar(RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture);

                Recargar = true;
                Analizador();
                EvaluadorCodigoTextBox.Focus();
            }
        }
        //—————————————————————————————————————————————[ Analizador ]—————————————————————————————————————————————
        private void Analizador()
        {
            InfoSintacticaListView.Items.Clear();

            int n = 0, e = 0;

            foreach (var tk in detector.GetTokens(EvaluadorCodigoTextBox.Text))
            {
                if (tk.Name == "ERROR") e++;

                if (tk.Name == "IDENTIFICADOR")
                    if (palabrasReservadas.Contains(tk.Informacion))
                        tk.Name = "RESERVADA";

                InfoSintacticaListView.Items.Add(tk);
                n++;
            }
            //—————————————————————————————————————————————[ Titulo Dinamico ]—————————————————————————————————————————————
            this.Title = string.Format($"Detector Sintactico | {n} Tokens | {e} Errores.");
        }
        //—————————————————————————————————————————————[ CodigoModificado ]—————————————————————————————————————————————
        private void CodigoModificado(object sender, TextChangedEventArgs e)
        {
            if (Recargar)
                Analizador();
        }
    }
}
