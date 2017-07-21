using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RealSimpleNet.Helpers
{
    public class Validations
    {
        /// <summary>
        /// Determina si una cadena es numerica
        /// </summary>
        /// <param name="valor">El valor a evaluar</param>
        /// <returns>bool</returns>
        public static bool IsNumeric(object valor)
        {
            Decimal d;
            return Decimal.TryParse(valor.ToString(), out d);
        }

        /// <summary>
        /// Evalua una expresion y si esta es nula, regresa un reemplazo
        /// </summary>
        /// <param name="expression">La expresion a evaluar</param>
        /// <param name="replacement">El reemplazo a retornar</param>
        /// <returns>object</returns>
        public static object IsNull(object expression, object replacement)
        {
            if (expression == null || Convert.IsDBNull(expression))
            {
                return replacement;
            }
            else
            {
                if (expression.GetType() == typeof(string))
                {
                    if ((string)expression == "" || (string)expression == string.Empty)
                    {
                        return replacement;
                    }
                }
                return expression;
            }
        }

        public static bool IsNullOrEmpty(object expression)
        {
            if (expression == null || Convert.IsDBNull(expression))
            {
                return true;
            }
            else
            {
                if (expression.GetType() == typeof(string))
                {
                    if ((string)expression == "" || (string)expression == string.Empty)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Regresa un entero nulable a partir de una expresión evaluada
        /// </summary>
        /// <param name="expression">La expresión a evaluar</param>
        /// <returns>int?</returns>
        public static int? GetNullable(object expression)
        {
            if (expression == null || Convert.IsDBNull(expression))
            {
                return null;
            }
            else
            {
                if (expression.GetType() == typeof(string))
                {
                    if (string.IsNullOrEmpty((string)expression))
                    {
                        return null;
                    }
                }
                return Convert.ToInt32(expression);
            }
        }

        public static T GetValue<T>(object val)
        {
            try
            {
                if (val == null) return default(T);

                if (val.GetType() == typeof(String))
                {
                    if ((string.IsNullOrEmpty((string)val)))
                    {
                        return default(T);
                    }
                }


                object num = null;
                if (IsNumeric(val))
                {
                    switch (typeof(T).ToString())
                    {
                        case "System.Int32":
                            num = Convert.ToInt32(val);
                            break;
                        case "System.Decimal":
                            num = Convert.ToDecimal(val);
                            break;
                        case "System.Double":
                            num = Convert.ToDouble(val);
                            break;
                        case "System.Single":
                            num = Convert.ToSingle(val);
                            break;
                        default:
                            num = Convert.ToInt32(val);
                            break;
                    }

                    return (T)num;
                }

                return (T)val;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "El valor {0} no puede ser convertido en el tipo {1}\r\n\r\n" + ex.Message,
                        val,
                        typeof(T).ToString()
                    )
                );
            } // end getValue
        }

        /// <summary>
        /// Enumeracion de reglas de validacion para controles
        /// </summary>
        public enum ValidateRule
        {
            Required = 1,
            Numeric = 2
        }
        
    } // end class Validations
} // end namespace Helpers
