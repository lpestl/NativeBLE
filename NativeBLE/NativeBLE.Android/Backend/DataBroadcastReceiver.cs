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
    // Handles various events fired by the Service.
    // ACTION_GATT_CONNECTED: connected to a GATT server.
    // ACTION_GATT_DISCONNECTED: disconnected from a GATT server.
    // ACTION_GATT_SERVICES_DISCOVERED: discovered GATT services.
    // ACTION_DATA_AVAILABLE: received data from the device.  This can be a result of read
    //                       or notification operations.

    [BroadcastReceiver(Enabled = true)]
    class DataBroadcastReceiver : BroadcastReceiver
    {
        private Logger logger = new Logger();
        private SensorDataPage parent;

        public DataBroadcastReceiver() {
            logger.TAG = "DataBroadcastReceiver";
            logger.TraceInformation("DataBroadcastReceiver constructor");
        }

        public void SetParent(SensorDataPage dataPage)
        {
            logger.TraceInformation("Setting parent");
            parent = dataPage;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            String action = intent.Action;
            if (SensorService.ACTION_GATT_CONNECTED.Equals(action))
            {
                parent.LogEcho("Connection Event: Device Connected");
                parent.SensorView.Connected = true;
                parent.SensorView.EnableStart = true;
                parent.UpdateConnectionState("Connecting...");
            }
            else if (SensorService.ACTION_GATT_DISCONNECTED.Equals(action))
            {
                parent.SensorView.Connected = false;
                
                if (parent.sensorService != null) parent.sensorService.Close();
                else logger.TraceInformation("SensorService null at disconnetion");

                parent.SensorView.EnableStart = false;
                parent.LogEcho("Disconnection Event: Device Disconnected");
                parent.SensorView.ColorStart = Xamarin.Forms.Color.Black;
                parent.UpdateConnectionState("Disconnected");

                parent.ClearUI();
            }
            else if (SensorService.ACTION_GATT_SERVICES_DISCOVERED.Equals(action))
            {
                parent.LogEcho("Connection Event: Services Discovered");

                parent.LogEcho("Start Multiple Characteristics read");

                parent.FindPressureService(parent.sensorService.GetSupportedGattServices());
            }
            else if (BluetoothLeService.ACTION_DATA_AVAILABLE.Equals(action))
            {
                logger.TraceInformation("Data available");
                parent.UpdateConnectionState("Connected");
                parent.DisplayData(intent.GetStringExtra(SensorService.EXTRA_DATA), 
                                   intent.GetStringExtra(SensorService.EXTRA_SENSOR_A), 
                                   intent.GetStringExtra(SensorService.EXTRA_SENSOR_B));                

                parent.test.CalculateMovingAverage(intent.GetStringExtra(SensorService.EXTRA_SENSOR_A), intent.GetStringExtra(SensorService.EXTRA_SENSOR_B));

                parent.SensorView.TextStart = "START";
                parent.SensorView.ColorStart = Xamarin.Forms.Color.Green;

                parent.test.SetRecord();
            }
            else if (BluetoothLeService.RSSI_DATA_AVAILABLE.Equals(action))
            {
                String srssi;
                srssi = intent.GetStringExtra(BluetoothLeService.EXTRA_RSSI_DATA);
                parent.DisplayRssiData(srssi);
            }
        }
    }
}