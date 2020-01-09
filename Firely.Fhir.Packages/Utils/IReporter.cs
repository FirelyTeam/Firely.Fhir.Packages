using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Firely
{
    public interface IReporter
    {
        void Report(string message);
    }

    public class ConsoleReporter : IReporter
    {
        public void Report(string message)
        {
            Console.WriteLine(message);
        }
    }
}
