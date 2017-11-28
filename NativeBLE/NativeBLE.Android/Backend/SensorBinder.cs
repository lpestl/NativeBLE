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
    public class SensorBinder : Binder
    {
        private Logger logger = new Logger();
        public SensorBinder(SensorService service)
        {
            logger.TAG = "SensorBinder";
            logger.TraceInformation("UI -> Binder <- Service - is setup.");
            this.Service = service;
        }

        public SensorService Service { get; private set; }
    }
}