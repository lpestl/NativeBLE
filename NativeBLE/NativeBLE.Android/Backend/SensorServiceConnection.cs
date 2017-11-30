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
using NativeBLE.Droid.Native;

namespace NativeBLE.Droid.Backend
{
    public class SensorServiceConnection : Java.Lang.Object, IServiceConnection
    {
        static readonly string TAG = typeof(SensorServiceConnection).FullName;
        private Logger logger = new Logger();

        public bool IsConnected { get; private set; }
        public SensorBinder Binder { get; private set; }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            Binder = service as SensorBinder;
            IsConnected = this.Binder != null;

            string message = "onServiceConnected - ";
            logger.TraceInformation($"OnServiceConnected {name.ClassName}");

            if (IsConnected)
            {
                message = message + " bound to service " + name.ClassName;
            }
            else
            {
                message = message + " not bound to service " + name.ClassName;
            }

            logger.TraceInformation(message);

            if (!Binder.Service.Initialize())
            {
                logger.TraceError("Unable to initialize Bluetooth");
            }

            SensorDataPage.Instance.sensorService = Binder.Service;
            SensorDataPage.Instance.OnResume();
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            logger.TraceInformation($"OnServiceDisconnected {name.ClassName}");
            IsConnected = false;
            Binder = null;
        }

        //public string GetFormattedTimestamp()
        //{
        //    if (!IsConnected)
        //    {
        //        return null;
        //    }

        //    return Binder?.GetFormattedTimestamp();
        //}
    }
}