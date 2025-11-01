using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Foundation.Utilities.CrossUtil
{
    public class HelperReflection
    {

        /// <summary>
        /// Obtiene el nombre de la propiedad a traves de un lamda. Modo de uso:
        /// var resul = HelperReflection.GetNameProperty(x => objIN.propiedad).
        /// </summary>
        /// <param name="objIN">parametro enviado en el siguiente formato nx => objIN.propiedad.</param>
        /// <returns></returns>
        public static string GetNameProperty(Expression<Func<object, object>> expression)
        {
            var lambda = expression as LambdaExpression;

            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = lambda.Body as UnaryExpression;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            else
            {
                memberExpression = lambda.Body as MemberExpression;
            }

            if (memberExpression == null)
            {
                throw new ArgumentException("Ingrese un argumento similar a x => obj.Mipropiedad");
            }
            if (memberExpression != null)
            {
                var propertyInfo = memberExpression.Member as PropertyInfo;
                return propertyInfo.Name;
            }

            return null;
        }

        /// <summary>
        /// Metodo que obtiene el nombre del metodo en el cual
        /// se encuentra ejecutando el stack
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetCurrentMethod()
        {
            string stackTraceCustom = string.Empty;
            StackTrace st = new StackTrace();

            foreach (var item in st.GetFrames())
            {
                stackTraceCustom += string.Format("{0} Metodo: {1} \n ", item.GetMethod().ReflectedType.FullName, item.GetMethod().Name);
            }

            return stackTraceCustom;
        }
    }
}
