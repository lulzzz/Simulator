﻿using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Simulator.Utilities
{
    public class Logger : ILogger
    {
        public void Log(LogLevel level, string area, string message, params KeyValuePair<string, string>[] args)
        {
            Debug.WriteLine($"{area} - {message}");
        }

        public void LogException(string area, Exception ex, params KeyValuePair<string, string>[] args)
        {
            Debug.WriteLine($"{area} - {ex.Message}");
        }

        public void SetKeys(params string[] args)
        {
            
        }

        public void SetUserId(string userId)
        {
            
        }

        public void TrackEvent(string message, Dictionary<string, string> parameters)
        {
            
        }
    }
}
