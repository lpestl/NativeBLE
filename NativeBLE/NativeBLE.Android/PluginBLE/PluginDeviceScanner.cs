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
using Android.Support.V4.App;
using Android;
using Android.Content.PM;
using Android.Bluetooth;
using static Android.Support.V4.App.ActivityCompat;
using Android.Bluetooth.LE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE;
using Plugin.BLE.Abstractions.EventArgs;
using System.Threading.Tasks;

[assembly: Xamarin.Forms.Dependency(typeof(NativeBLE.Droid.PluginBLE.PluginDeviceScanner))]
namespace NativeBLE.Droid.PluginBLE
{
    class PluginDeviceScanner : IPluginDeviceScanner
    {
        private Logger logger = new Logger();

        private Activity thisActivity;
        private MainPageViewModel scannerViewModel;

        private IBluetoothLE ble;
        private Plugin.BLE.Abstractions.Contracts.IAdapter adapter;

        private List<IDevice> deviceList = new List<IDevice>();
        private List<Device> deviceNativeList = new List<Device>();

        public PluginDeviceScanner()
        {
            logger.TAG = "PluginDeviceScanner";
            logger.TraceInformation("Plugin Device Scanner constructor");

            thisActivity = Xamarin.Forms.Forms.Context as Activity;
        }

        public void Init(MainPageViewModel value)
        {
            logger.TraceInformation("Setup scanner ViewModel on MainPage");
            scannerViewModel = value;
        }

        public void GetBluetoothAdapter()
        {
            deviceList.Clear();
            deviceNativeList.Clear();

            ble = CrossBluetoothLE.Current;
            adapter = CrossBluetoothLE.Current.Adapter;

            ble.StateChanged += Ble_StateChanged;

        }

        private void Ble_StateChanged(object sender, BluetoothStateChangedArgs e)
        {
            logger.TraceInformation(String.Format("Change blutoth status: {0} from {1}", e.NewState.ToString(), e.OldState.ToString()));
        }

        public Device GetDevice(int value)
        {
            return deviceNativeList[value];
        }
        
        public void ScanLeDevice(int mode)
        {
            StartScan(mode);
        }

        public async void StopScan()
        {
            await PluginBleStopScan();
        }

        private async void StartScan(int mode)
        {
            if (deviceList.Count != 0)
            {
                logger.TraceInformation("Cleaning device`s list");
                deviceNativeList.Clear();
                deviceList.Clear();
                scannerViewModel.Devices.Clear();
            }

            adapter.ScanTimeout = 20000;

            switch (mode)
            {
                case 0:
                    adapter.ScanMode = Plugin.BLE.Abstractions.Contracts.ScanMode.Balanced;
                    break;
                case 1:
                    adapter.ScanMode = Plugin.BLE.Abstractions.Contracts.ScanMode.LowLatency;
                    break;
                case 2:
                    adapter.ScanMode = Plugin.BLE.Abstractions.Contracts.ScanMode.LowPower;
                    break;
                case 3:
                    adapter.ScanMode = Plugin.BLE.Abstractions.Contracts.ScanMode.Passive;
                    break;
                default:
                    adapter.ScanMode = Plugin.BLE.Abstractions.Contracts.ScanMode.Balanced;
                    break;
            }

            logger.TraceInformation(String.Format("Selected mode: {0} - {1}", mode, adapter.ScanMode.ToString()));

            adapter.DeviceDiscovered += Adapter_DeviceDiscovered;
            adapter.ScanTimeoutElapsed += Adapter_ScanTimeoutElapsed;
            scannerViewModel.Scanning = true;
            await adapter.StartScanningForDevicesAsync();
        }

        private void Adapter_DeviceDiscovered(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            var result = e.Device.NativeDevice as BluetoothDevice;
            logger.TraceInformation(String.Format("onScanResult: found {0} - {1}", result.Name, result.Address));

            var resultDevice = new Device(result.Name, result.Address);
            if (!deviceNativeList.Exists(x => x.Address.Equals(resultDevice.Address)))
            {
                deviceNativeList.Add(resultDevice);
                deviceList.Add(e.Device);
                scannerViewModel.Devices.Add(new DeviceViewModel(resultDevice.Name, resultDevice.Address));
            }            
        }

        private void Adapter_ScanTimeoutElapsed(object sender, EventArgs e)
        {
            logger.TraceInformation("Adapter Scan Timeout Elapsed");
            scannerViewModel.Scanning = false;
            adapter.ScanTimeoutElapsed -= Adapter_ScanTimeoutElapsed;
        }

        private async Task PluginBleStopScan()
        {
            logger.TraceInformation("Press Stop Scan");
            await adapter.StopScanningForDevicesAsync();
            scannerViewModel.Scanning = false;
            adapter.ScanTimeoutElapsed -= Adapter_ScanTimeoutElapsed;
        }

        public IDevice GetIDevice(int value)
        {
            return deviceList[value];
        }

        ~PluginDeviceScanner()
        {
            logger.TraceInformation("Destroy");
            ble.StateChanged -= Ble_StateChanged;
        }
    }
}