using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RealSimpleNet.Helpers
{
    public class Strings
    {
        public static string Left(string str, int len)
        {
            return str.Substring(0, len);
        }

        public static string Right(string str, int len)
        {
            return str.Substring(str.Length - len);
        }

        public static string Mid(string str, int start, int len)
        {
            return str.Substring(start, len);
        }

        public static string Reverse(string s)
        {
            char[] arr = s.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }
        /// <summary>
        /// Agrega espaciones en blanco al final de una cadena
        /// </summary>
        /// <param name="str">La cadena a modificar</param>
        /// <param name="spaces">La cantidad de espacios en blanco a agregar</param>
        /// <returns>string</returns>
        public static string AddSpaces(string str, int spaces)
        {
            string blankspaces = "";
            int i;
            for (i = 0; i < spaces; i++)
            {
                blankspaces += " ";
            }
            return blankspaces + str + blankspaces;
        } // end string AddSpaces

        /// <summary>
        /// Regresa una cadena con formato "N2"
        /// </summary>
        /// <param name="valor">Valor a aplicar el formato</param>
        /// <returns>string</returns>
        public static string N2(object valor)
        {
            return string.Format("{0:N2}", valor);
        } // end string N2
    } // end class Strings
} // end namespace Helpers
