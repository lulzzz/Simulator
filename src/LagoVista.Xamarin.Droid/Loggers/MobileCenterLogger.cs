﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using LagoVista.Core.PlatformSupport;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
using Microsoft.Azure.Mobile.Crashes;

namespace LagoVista.XPlat.Droid.Loggers
{
    public class MobileCenterLogger : ILogger
    {
        private String _userId;

        KeyValuePair<String, String>[] _args;


        public TimedEvent StartTimedEvent(string area, string description)
        {
            return new TimedEvent(area, description);
        }

        public void EndTimedEvent(TimedEvent evt)
        {
            var duration = DateTime.Now - evt.StartTime;

            Log(LagoVista.Core.PlatformSupport.LogLevel.Message, evt.Area, evt.Description, new KeyValuePair<string, string>("duration", Math.Round(duration.TotalSeconds, 4).ToString()));
        }


        public MobileCenterLogger(string key)
        {
            MobileCenter.Start($"android={key}", typeof(Analytics), typeof(Crashes));
        }

        public void Log(LagoVista.Core.PlatformSupport.LogLevel level, string area, string message, params KeyValuePair<string, string>[] args)
        {
            var dictionary = new Dictionary<string, string>();
            dictionary.Add("Area", "area");
            dictionary.Add("UseId", String.IsNullOrEmpty(_userId) ? "UNKNOWN" : _userId);
            dictionary.Add("Level", level.ToString());

            if (_args != null)
            {
                foreach (var arg in _args)
                {
                    dictionary.Add(arg.Key, arg.Value);
                }
            }

            foreach (var arg in args)
            {
                dictionary.Add(arg.Key, arg.Value);
            }

            Analytics.TrackEvent(message, dictionary);
        }

        public void LogException(string area, Exception ex, params KeyValuePair<string, string>[] args)
        {
            var dictionary = new Dictionary<string, string>();
            dictionary.Add("Area", "area");
            dictionary.Add("UseId",  String.IsNullOrEmpty(_userId) ? "UNKNOWN" : _userId);
            dictionary.Add("Type", "exception");
            dictionary.Add("StackTrace", ex.StackTrace);

            if(_args != null)
            {
                foreach (var arg in _args)
                {
                    dictionary.Add(arg.Key, arg.Value);
                }
            }

            foreach (var arg in args)
            {
                dictionary.Add(arg.Key, arg.Value);
            }

            Analytics.TrackEvent(ex.Message, dictionary);
        }

        public void SetKeys(params KeyValuePair<String, String>[] args)
        {
            _args = args;
        }

        public void SetUserId(string userId)
        {
            _userId = userId;
        }

        public void TrackEvent(string message, Dictionary<string, string> args)
        {
            var dictionary = new Dictionary<string, string>();
            dictionary.Add("UseId", String.IsNullOrEmpty(_userId) ? "UNKNOWN" : _userId);

            foreach (var arg in args)
            {
                dictionary.Add(arg.Key, arg.Value);
            }

            if (_args != null)
            {
                foreach (var arg in _args)
                {
                    dictionary.Add(arg.Key, arg.Value);
                }
            }

            Analytics.TrackEvent(message, dictionary);
        }
    }
}