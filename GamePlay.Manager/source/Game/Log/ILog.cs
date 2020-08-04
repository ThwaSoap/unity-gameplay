using System;
using System.Collections.Generic;
using System.Text;

namespace GamePlay.Internal
{  
    interface ILog
    {
        void Log(string _message);
        void Warning(string _message);
        void Error(string _message); 
        void Log(string _message, params object[] _args);
        void Warning(string _message, params object[] _args); 
        void Error(string _message, params object[] _args);
    }
}
