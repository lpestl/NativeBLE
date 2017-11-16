using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using NativeBLE.Core;
using Android.Util;

[assembly: Xamarin.Forms.Dependency(typeof(NativeBLE.Droid.Native.Logger))]
namespace NativeBLE.Droid.Native
{
    class Logger : ILogger
    {
        public Logger()
        {
            TAG = "NoName";
        }

        public string TAG { get; set; }

        public void LogError(string value)
        {
            Log.Debug(TAG, String.Format("[ERROR] {0}", value));
        }

        public void LogInfo(string value)
        {
            Log.Debug(TAG, String.Format("[INFO] {0}", value));
        }

        public void LogWarning(string value)
        {
            Log.Debug(TAG, String.Format("[WARNING] {0}", value));
        }

        public void TraceError(string value)
        {
            Log.Error(TAG, value);
        }

        public void TraceInformation(string value)
        {
            Log.Info(TAG, value);
        }

        public void TraceWarning(string value)
        {
            Log.Warn(TAG, value);
        }
    }
}