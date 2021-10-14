using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCloner
{
    public class ExecutionLogger
    {
        public delegate void LogDelegate(string text);
        LogDelegate _logAction;

        public ExecutionLogger(LogDelegate logAction)
        {
            _logAction = logAction;
        }

        public void Log(string text)
        {
            _logAction(text);
        }
    }
}
