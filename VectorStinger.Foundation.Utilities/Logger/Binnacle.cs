using iText.Kernel.Geom;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Foundation.Utilities.Logger
{
    public class Binnacle
    {
        public static void ProcessEvent(Event _event)
        {
            if (!Directory.Exists("c:\\tmp\\logs"))
            {
                Directory.CreateDirectory("c:\\tmp\\logs");
            }
            string pathLog = $"c:\\tmp\\logs\\{DateTime.Now.ToString("yyyyMMdd")}";
            if (!File.Exists(pathLog))
            {
                using (FileStream fs = File.Create(pathLog))
                {
                    fs.Close();
                }
            }
            File.AppendAllText(pathLog, DateTime.Now.ToString() + ":" + _event.description);

            /*ILogger log = LogManager.GetCurrentClassLogger();
            switch (_event.category)
            {
                case Event.Category.Information:
                    log.Info(_event.description);
                    break;
                case Event.Category.Warning:
                    log.Warn(_event.description);
                    break;
                case Event.Category.Error:
                    log.Error(_event.description);
                    break;
                default:
                    log.Info(_event.description);
                    break;
            }*/

        }
    }
}
