using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Configgy.Server;

namespace Configgy
{
    public class ConsoleLogger : ILogger
    {
        private ConsoleColor _defaultColor = Console.ForegroundColor;

        public void Info(string message)
        {
            WriteMessage("INFO: " + message);
        }
        
        public void Warning(string message)
        {
            WriteMessage("WARNING: " + message, color: ConsoleColor.Yellow);
        }

        public void Error(string message)
        {
            WriteMessage("ERROR: " + message, color: ConsoleColor.Red);
        }

        public void Error(string message, Exception ex)
        {
            WriteMessage("ERROR: " + message + "\n" + ex.ToString(), color: ConsoleColor.Red);
        }

        public void Error(Exception ex)
        {
            WriteMessage("ERROR: " + ex.ToString(), color: ConsoleColor.Red);
        }


        private void WriteMessage(string message, ConsoleColor? color = null)
        {
            color = color ?? _defaultColor;

            Console.ForegroundColor = color.Value;

            Console.WriteLine("[{0:dd/MM/yyyy HH:mm:ss.fff}] {1}", DateTime.Now, message);

            Console.ForegroundColor = _defaultColor;
        }
    }
}
