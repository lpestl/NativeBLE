using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Bluetooth.LE;
using Android.Bluetooth;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using NativeBLE.Core;
using Xamarin.Forms;
using Android;
using Android.Content.PM;
using Android.Support.V4.App;
using Android.Locations;
using Android.Util;
using Java.Util;

namespace NativeBLE.Droid.Native
{
    public class SampleGattAttributes
    {
        private Dictionary<String, String> attributes = new Dictionary<String, String>();
        public static String PRESSURE_SERVICE = "0000AA40-0000-1000-8000-00805f9b34fb";
        public static String BATTERY_SERVICE = "0000180f-0000-1000-8000-00805f9b34fb";
        public static String DEVICE_INFO_SERVICE = "0000180a-0000-1000-8000-00805f9b34fb";
        public static String PRESSURE_NOTIFICATION_HANDLE = "0000aa41-0000-1000-8000-00805f9b34fb";
        public static String BATTERY_NOTIFICATION_HANDLE = "00002a19-0000-1000-8000-00805f9b34fb";
        public static String FIRMWARE_VERSION_HANDLE = "00002a27-0000-1000-8000-00805f9b34fb";
        public static String BATCH_VERSION_HANDLE = "00002a26-0000-1000-8000-00805f9b34fb";


        //public static String HEART_RATE_MEASUREMENT = "0000AA40-0000-1000-8000-00805f9b34fb";
        //public static String CLIENT_CHARACTERISTIC_CONFIG = "0000AA41-0000-1000-8000-00805F9B34FB";

        public SampleGattAttributes() {
            // Sample Services.
            attributes["0000180d-0000-1000-8000-00805f9b34fb"] = "Heart Rate Service";
            attributes["0000180a-0000-1000-8000-00805f9b34fb"] = "Device Information Service";
            // Sample Characteristics.
            attributes[PRESSURE_SERVICE] = "Pressure Service Management";

            attributes["00002a29-0000-1000-8000-00805f9b34fb"] = "Manufacturer Name String";
        }

        public String lookup(String uuid, String defaultName)
        {
            String name = attributes[uuid];
            return name == null ? defaultName : name;
        }
    }
}