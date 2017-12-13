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
using Plugin.BLE.Abstractions.Contracts;
using System.Diagnostics;
using Plugin.BLE;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using System.Threading;
using Android.Bluetooth;

[assembly: Xamarin.Forms.Dependency(typeof(NativeBLE.Droid.PluginBLE.PluginSensorData))]
namespace NativeBLE.Droid.PluginBLE
{
    class PluginSensorData : IPluginSensorData
    {
        private Logger logger = new Logger();

        private Activity thisActivity;

        private TestSensors test;
        private SensorViewModel sensorViewModel;
        public IDevice currentDevice;

        private IBluetoothLE ble;
        private Plugin.BLE.Abstractions.Contracts.IAdapter adapter;

        public Stopwatch connectionStopwatch = new Stopwatch();
        public Stopwatch firstDataStopwatch = new Stopwatch();
        public Stopwatch disconnectionStopwatch = new Stopwatch();
        public bool firstData = false;

        public static ProgressDialog progressDialog;

        public static String PRESSURE_SERVICE = "0000AA40-0000-1000-8000-00805f9b34fb";
        public static String BATTERY_SERVICE = "0000180f-0000-1000-8000-00805f9b34fb";
        public static String DEVICE_INFO_SERVICE = "0000180a-0000-1000-8000-00805f9b34fb";
        public static String PRESSURE_NOTIFICATION_HANDLE = "0000aa41-0000-1000-8000-00805f9b34fb";
        public static String BATTERY_NOTIFICATION_HANDLE = "00002a19-0000-1000-8000-00805f9b34fb";
        public static String FIRMWARE_VERSION_HANDLE = "00002a27-0000-1000-8000-00805f9b34fb";
        public static String BATCH_VERSION_HANDLE = "00002a26-0000-1000-8000-00805f9b34fb";

        public PluginSensorData()
        {
            logger.TAG = "PluginSensorData";
            logger.TraceInformation("Native PluginSensorData constructor");
        }

        public void Init(SensorViewModel sensorView, IDevice device)
        {
            sensorViewModel = sensorView;
            currentDevice = device;

            ble = CrossBluetoothLE.Current;
            adapter = CrossBluetoothLE.Current.Adapter;

            ble.StateChanged += Ble_StateChanged;

            thisActivity = Xamarin.Forms.Forms.Context as Activity;
            test = new TestSensors(thisActivity, sensorViewModel);

            adapter.DeviceConnected += Adapter_DeviceConnectedAsync;
            adapter.DeviceDisconnected += Adapter_DeviceDisconnected;
            DisplayRssiData(currentDevice.Rssi.ToString());
            
            ConnectAsync();
        }
        
        public void Init(SensorViewModel sensorView, MixedDeviceData device)
        {
            sensorViewModel = sensorView;

            thisActivity = Xamarin.Forms.Forms.Context as Activity;
            test = new TestSensors(thisActivity, sensorViewModel);

            ble = CrossBluetoothLE.Current;
            adapter = CrossBluetoothLE.Current.Adapter;

            currentDevice = new Plugin.BLE.Android.Device(adapter as Plugin.BLE.Android.Adapter, device.NativeDevice as BluetoothDevice, null, device.Rssi, device.ScanRecord);

            ble.StateChanged += Ble_StateChanged;

            adapter.DeviceConnected += Adapter_DeviceConnectedAsync;
            adapter.DeviceDisconnected += Adapter_DeviceDisconnected;
            DisplayRssiData(currentDevice.Rssi.ToString());

            ConnectAsync();
        }

        private void Adapter_DeviceDisconnected(object sender, DeviceEventArgs e)
        {
            Disconnected();

            LogEcho("Disconnection Event: Device Disconnected");
            sensorViewModel.ColorStart = Xamarin.Forms.Color.Black;
            sensorViewModel.ConnectionState = "Disconnected.";
            sensorViewModel.TextStart = "Disconnected";

            ClearUI();
        }

        private void ClearUI()
        {
            sensorViewModel.Data = "No data";
            sensorViewModel.SensorA = "No data";
            sensorViewModel.SensorB = "No data";
            sensorViewModel.BataryLevel = "No data";
        }

        private async void Adapter_DeviceConnectedAsync(object sender, DeviceEventArgs e)
        {
            Connected();            

            LogEcho("Connection Event: Device Connected");
            sensorViewModel.ConnectionState = "Connecting...";
            sensorViewModel.TextStart = "Connecting...";

            logger.TraceInformation("GetServiceAsync");
            var service = await currentDevice.GetServiceAsync(Guid.Parse(PRESSURE_SERVICE));
            logger.TraceInformation("UpdateRssiAsync");
            bool rssiStatus = await currentDevice.UpdateRssiAsync();
            logger.TraceInformation("GetCharacteristicAsync");
            var characteristic = await service.GetCharacteristicAsync(Guid.Parse(PRESSURE_NOTIFICATION_HANDLE));
            
            firstDataStopwatch.Stop();
            sensorViewModel.FirstDataTimeSpan = firstDataStopwatch.Elapsed;
            firstDataStopwatch.Reset();
            firstData = true;

            logger.TraceInformation("Data available");
            sensorViewModel.ConnectionState = "Connected";
            sensorViewModel.TextStart = "Connected";

            HideProgressDialog();

            characteristic.ValueUpdated += Characteristic_ValueUpdatedAsync;

            await characteristic.StartUpdatesAsync();
        }

        private async void Characteristic_ValueUpdatedAsync(object sender, CharacteristicUpdatedEventArgs e)
        {
            if (PRESSURE_NOTIFICATION_HANDLE.Equals(e.Characteristic.Uuid.ToString()))
            {
                logger.TraceInformation("Building Broadcaster: Pressure characteristic data");
                logger.TraceInformation(e.Characteristic.Uuid.ToString());
                // For all other profiles, writes the data formatted in HEX.
                byte[] data = e.Characteristic.Value;

                if (data != null && data.Length > 0)
                {
                    Bundle extras = new Bundle();

                    long lData = 0;
                    String sData;

                    var stringBytes = BitConverter.ToString(data);

                    lData = BytesToLong(data, 0, 1);
                    sData = lData.ToString();

                    sensorViewModel.Data = stringBytes;
                    sensorViewModel.SensorA = sData;
                    sensorViewModel.SensorB = BytesToLong(data, 2, 3).ToString();

                    test.CalculateMovingAverage(sData, BytesToLong(data, 2, 3).ToString());

                    sensorViewModel.TextStart = "START";
                    sensorViewModel.ColorStart = Xamarin.Forms.Color.Green;

                    test.SetRecord();

                    sensorViewModel.RSSI = currentDevice.Rssi.ToString();
                    bool rssiStatus = await currentDevice.UpdateRssiAsync();

                    HideProgressDialog();
                }
            }

        }
        
        public static long BytesToLong(byte[] b, int minbyte, int maxbyte)
        {
            long result = 0;
            for (int i = minbyte; i <= maxbyte; i++)
            {
                result <<= 8;
                result |= (b[i] & 0xFF);
            }
            return result;
        }

        private void Ble_StateChanged(object sender, BluetoothStateChangedArgs e)
        {
            logger.TraceInformation(String.Format("Change blutoth status: {0} from {1}", e.NewState.ToString(), e.OldState.ToString()));
        }

        public void DisplayRssiData(String data)
        {
            if (data != null)
            {
                sensorViewModel.RSSI = data;
                logger.TraceInformation("Displaying RSSI");
            }
        }

        public async void ConnectAsync()
        {
            logger.TraceInformation("Try connect to device");
            
            connectionStopwatch.Reset();
            connectionStopwatch.Start();
            sensorViewModel.ConnectionState = "Connecting...";
            progressDialog = ProgressDialog.Show(thisActivity, "Please wait...", "Connecting...", true);
            
            try
            {
                logger.TraceInformation("Await connection");
                await adapter.ConnectToDeviceAsync(currentDevice);
            }
            catch (DeviceConnectionException e)
            {
                logger.TraceError($"Connect exception: {e.Source} - {e.Message}");
            }
            logger.TraceInformation("Connecting succes");
            //HideProgressDialog();
        }

        private void Connected()
        {
            logger.TraceInformation("Connected setup");
            connectionStopwatch.Stop();
            sensorViewModel.ConnectionTimeSpan = connectionStopwatch.Elapsed;
            connectionStopwatch.Reset();
            firstDataStopwatch.Reset();
            firstDataStopwatch.Start();
            sensorViewModel.ConnectionState = "Connected";
            sensorViewModel.Connected = true;
        }

        private void Disconnected()
        {
            logger.TraceInformation("Disconnected setup");
            disconnectionStopwatch.Stop();
            sensorViewModel.DisconnectionTimeSpan = disconnectionStopwatch.Elapsed;
            disconnectionStopwatch.Reset();
            firstData = false;
            sensorViewModel.ConnectionState = "Disconnected";
            sensorViewModel.Connected = false;
        }

        public async void DisconnectAsync()
        {
            logger.TraceInformation("Try disconnect");

            disconnectionStopwatch.Reset();
            disconnectionStopwatch.Start();
            progressDialog = ProgressDialog.Show(thisActivity, "Please wait...", "Disconnecting...", true);


            var service = await currentDevice.GetServiceAsync(Guid.Parse(PRESSURE_SERVICE));
            var characteristic = await service.GetCharacteristicAsync(Guid.Parse(PRESSURE_NOTIFICATION_HANDLE));

            characteristic.ValueUpdated -= Characteristic_ValueUpdatedAsync;

            try
            {
                logger.TraceInformation("Await disconnection");
                await adapter.DisconnectDeviceAsync(currentDevice);
            }
            catch (DeviceConnectionException e)
            {
                logger.TraceError($"Connect exception: {e.Source} - {e.Message}");
            }

            HideProgressDialog();
            logger.TraceInformation("Disconnecting succes");
        }

        public void HideProgressDialog()
        {
            new Thread(new ThreadStart(delegate {
                //HIDE PROGRESS DIALOG
                thisActivity.RunOnUiThread(() => {
                    if (progressDialog != null)
                    {
                        progressDialog.Hide();
                        progressDialog.Dismiss();
                        //progressDialog = null;
                    }
                });
            })).Start();
        }

        public void OnClickConnectButton()
        {
            if (!sensorViewModel.Connected)
            {
                ConnectAsync();
            }
            else
            {
                DisconnectAsync();
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

        public void LogEcho(String msg)
        {
            logger.TraceInformation(msg);
            if (sensorViewModel != null)
            {
                sensorViewModel.DebugString = msg;
            }
        }

        ~PluginSensorData()
        {
            ble.StateChanged -= Ble_StateChanged;

            adapter.DeviceConnected -= Adapter_DeviceConnectedAsync;
            adapter.DeviceDisconnected -= Adapter_DeviceDisconnected;
        }
    }
}