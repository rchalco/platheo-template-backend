using NLog;
using VectorStinger.Foundation.Utilities.CrossUtil;
using VectorStinger.Foundation.Utilities.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Foundation.Utilities.Exceptions
{
    public class ManagerException
    {
        public string ProcessException(Exception ex)
        {
            string ticket = CustomGuid.GetGuid();
            string msg = $"Ocurrio un erro al procesar la informacion por favor comuniquese con el administrador de sistemas. Ticket {ticket} ";
            string msgError = $"Ticket {ticket} {ex.Message}";
            int contador = 0;
            string stackTrace = ex.StackTrace;
            while (ex.InnerException != null && contador < 5)
            {
                contador++;
                msgError += " inner -> " + ex.InnerException.Message;
                ex = ex.InnerException;
                stackTrace += ". " + ex.StackTrace;
            }
            msgError += " stactrace -> " + stackTrace;

            Binnacle.ProcessEvent(new Event
            {
                category = Event.Category.Error,
                description = msgError
            });

            return msg;
        }
    }
}
