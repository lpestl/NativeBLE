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
using NativeBLE.Droid.Native;
using Android.Bluetooth;
using Xamarin.Forms;
using Java.Util;
using System.Diagnostics;

[assembly: Xamarin.Forms.Dependency(typeof(NativeBLE.Droid.Backend.SensorDataPage))]
namespace NativeBLE.Droid.Backend
{
    class SensorDataPage : ISensorData
    {
        private Logger logger = new Logger();

        private Activity thisActivity;

        private SensorViewModel sensorView;
        public Core.Device currentDevice;

        private SensorServiceConnection serviceConnection = null;
        public SensorService sensorService = null;

        private BluetoothGattCharacteristic pressureCharacteristic;

        private List<List<BluetoothGattCharacteristic>> gattCharacteristicsLists = new List<List<BluetoothGattCharacteristic>>();

        public SensorViewModel SensorView { get => sensorView; set => sensorView = value; }
        public TestSensors test;
        private DataBroadcastReceiver dataBroadcastReceiver;
        public static SensorDataPage Instance = null;

        public Stopwatch connectionStopwatch = new Stopwatch();
        public Stopwatch firstDataStopwatch = new Stopwatch();
        public Stopwatch disconnectionStopwatch = new Stopwatch();
        public bool firstData = false;
        
        public SensorDataPage()
        {
            logger.TAG = "SensorDataPage";
            logger.TraceInformation("Native SensorDataPage constructor");

            MessagingCenter.Subscribe<MainActivity>(this, "OnResume", (sender) => { this.OnResume(); });
            MessagingCenter.Subscribe<MainActivity>(this, "OnPause", (sender) => { this.OnPause(); });
            MessagingCenter.Subscribe<MainActivity>(this, "OnDestroy", (sender) => { this.OnDestroy(); });

        }

        public void Init(SensorViewModel value, Core.Device device)
        {
            SensorView = value;
            currentDevice = device;

            thisActivity = Xamarin.Forms.Forms.Context as Activity;
            test = new TestSensors(thisActivity, SensorView);

            dataBroadcastReceiver = new DataBroadcastReceiver();
            dataBroadcastReceiver.SetParent(this);

            Instance = this;

            if (serviceConnection == null)
            {
                this.serviceConnection = new SensorServiceConnection();
            }

            Intent serviceToStart = new Intent(thisActivity.ApplicationContext, typeof(SensorService));
            thisActivity.ApplicationContext.BindService(serviceToStart, this.serviceConnection, Bind.AutoCreate);
        }

        public void LogEcho(String msg)
        {
            logger.TraceInformation(msg);
            if (SensorView != null)
            {
                SensorView.DebugString = msg;
            }
        }

        public void ClearUI()
        {
            SensorView.Data = "No data";
            SensorView.SensorA = "No data";
            SensorView.SensorB = "No data";
            SensorView.BataryLevel = "No data";
        }

        public void DisplayRssiData(String data)
        {
            if (data != null)
            {
                SensorView.RSSI = data;
                logger.TraceInformation("Displaying RSSI");
            }
        }

        public void DisplayData(String data, String sensor_a, String sensor_b)
        {
            if (data != null)
            {
                SensorView.Data = data;
                SensorView.SensorA = sensor_a;
                SensorView.SensorB = sensor_b;
                LogEcho("Displaying Pressure Data");
            }
        }

        public static IntentFilter MakeGattUpdateIntentFilter()
        {
            IntentFilter intentFilter = new IntentFilter();
            intentFilter.AddAction(SensorService.ACTION_GATT_CONNECTED);
            intentFilter.AddAction(SensorService.ACTION_GATT_DISCONNECTED);
            intentFilter.AddAction(SensorService.ACTION_GATT_SERVICES_DISCOVERED);
            intentFilter.AddAction(SensorService.ACTION_DATA_AVAILABLE);
            intentFilter.AddAction(SensorService.RSSI_DATA_AVAILABLE);
            return intentFilter;
        }

        public void UpdateConnectionState(string v)
        {
            thisActivity.RunOnUiThread(() =>
            {
                SensorView.ConnectionState = v;
                SensorView.TextStart = v;
            });
        }

        public void FindPressureService(IList<BluetoothGattService> gattServices)
        {
            if (gattServices == null) return;

            foreach (BluetoothGattService gattService in gattServices)
            {
                UpdateConnectionState("Reading services");
                LogEcho("Loop through all services");

                if (SensorService.UUID_PRESSURE_SERVICE.Equals(gattService.Uuid))
                {
                    LogEcho("Found Pressure Service");

                    if (gattService.GetCharacteristic(UUID.FromString(SampleGattAttributes.PRESSURE_NOTIFICATION_HANDLE)) == null)
                    {
                        LogEcho("Pressure Char Null");
                        return;
                    }
                    else
                    {
                        pressureCharacteristic = gattService.GetCharacteristic(UUID.FromString(SampleGattAttributes.PRESSURE_NOTIFICATION_HANDLE));
                        
                        sensorService.SetCharacteristicNotification(pressureCharacteristic, true);
                        LogEcho("Notification set up for Pressure");
                    }
                    return;
                }
            }
        }

        public void OnClickConnectButton()
        {
            if (!SensorView.Connected)
            {
                sensorService.Connect(currentDevice.Address);
            }
            else
            {
                sensorService.Disconnect();
            }
        }

        public void OnClickResultButton()
        {
            //throw new NotImplementedException();
        }

        public void OnClickStartButton()
        {
            test.StartTest();
        }
        
        public void SetMinLimit(int value)
        {
            test.SetMinLimit(value);
        }

        public void OnDestroy()
        {
            logger.TraceInformation("OnDestroy called");

            MessagingCenter.Unsubscribe<MainActivity>(this, "OnResume");
            MessagingCenter.Unsubscribe<MainActivity>(this, "OnPause");
            MessagingCenter.Unsubscribe<MainActivity>(this, "OnDestroy");

            thisActivity.ApplicationContext.UnbindService(serviceConnection);
            thisActivity.ApplicationContext.StopService(new Intent(thisActivity, typeof(SensorService)));
            sensorService = null;
        }

        public void OnPause()
        {
            logger.TraceInformation("OnPause called");
            if (dataBroadcastReceiver != null)
            {
                thisActivity.ApplicationContext.UnregisterReceiver(dataBroadcastReceiver);
            }
        }

        public void OnResume()
        {
            logger.TraceInformation("OnResume called");
            thisActivity.ApplicationContext.RegisterReceiver(dataBroadcastReceiver, MakeGattUpdateIntentFilter());
            if (sensorService != null)
            {
                bool result = sensorService.Connect(currentDevice.Address);
            }
        }

        ~SensorDataPage()
        {
            OnDestroy();
        }
    }
}