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

[assembly: Xamarin.Forms.Dependency(typeof(NativeBLE.Droid.Backend.DeviceScanner))]
namespace NativeBLE.Droid.Backend
{
    class DeviceScanner : ScanCallback, IDeviceScanner
    {
        private Logger logger = new Logger();

        private Activity thisActivity;
        private MainPageViewModel scannerViewModel;
        private Action scanerCallback;

        private BluetoothAdapter bluetoothAdapter;
        private Handler mHandler = new Handler();

        private static int REQUEST_ENABLE_BT = 1;
        // Stops scanning after 10 seconds.
        private static long SCAN_PERIOD = 20000;

        private List<Device> devicesList = new List<Device>();

        public static ParcelUuid parcelUuid = ParcelUuid.FromString("0000aa40-0000-1000-8000-00805f9b34fb");        

        public DeviceScanner()
        {
            logger.TAG = "DeviceScanner";
            logger.TraceInformation("Native Device Scanner constructor");

            thisActivity = Xamarin.Forms.Forms.Context as Activity;
        }

        public void Init(MainPageViewModel value)
        {
            logger.TraceInformation("Setup scanner ViewModel on MainPage");
            scannerViewModel = value;

            scanerCallback = () =>
            {
                logger.TraceInformation("mHandler.PostDelayed called. Scanner stoped.");
                scannerViewModel.Scanning = false;
                bluetoothAdapter.BluetoothLeScanner.StopScan(this);
            };
        }       
        
        public void GetBluetoothAdapter()
        {
            logger.TraceInformation("Get bluetooth Adapter");
            BluetoothManager bluetoothManager = thisActivity.GetSystemService(Context.BluetoothService) as BluetoothManager;
            bluetoothAdapter = bluetoothManager.Adapter;
            if (bluetoothAdapter != null)
            {
                if (!bluetoothAdapter.IsEnabled)
                {
                    logger.TraceInformation("Try to turn on the Bluetooth adapter");

                    Intent enableBtIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
                    thisActivity.StartActivityForResult(enableBtIntent, REQUEST_ENABLE_BT);

                    // NOTE: This function enables the Bluetooth adapter without requesting permission.
                    //mBluetoothAdapter.Enable();
                }
            }
            else
            {
                logger.TraceError("Bluetooth not supported.");
            }
        }
        
        public void ScanLeDevice()
        {
            if (bluetoothAdapter == null)
            {
                logger.TraceWarning("Turn on the bluetooth adapter before you begin scanning.");
                return;
            }

            mHandler.PostDelayed(scanerCallback, SCAN_PERIOD);

            scannerViewModel.Scanning = true;
            ScanFilter scanFilter = (new ScanFilter.Builder()).SetServiceUuid(parcelUuid).Build();
            List<ScanFilter> scanFilters = new List<ScanFilter>();
            
            if (devicesList.Count != 0)
            {
                logger.TraceInformation("Cleaning device`s list");
                devicesList.Clear();
                scannerViewModel.Devices.Clear();
            }

            scanFilters.Add(scanFilter);
            ScanSettings scanSettings =
                    new ScanSettings.Builder().Build();

            bluetoothAdapter.BluetoothLeScanner.StartScan(scanFilters, scanSettings, this);
            logger.TraceInformation("Start scanning.");
        }

        public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult result)
        {
            logger.TraceInformation(String.Format("onScanResult: found {0} - {1}", result.Device.Name, result.Device.Address));
            base.OnScanResult(callbackType, result);

            var resultDevice = new Device(result.Device.Name, result.Device.Address);
            if (!devicesList.Exists(x => x.Address.Equals(resultDevice.Address)))
            {
                devicesList.Add(resultDevice);
                scannerViewModel.Devices.Add(new DeviceViewModel(resultDevice.Name, resultDevice.Address));
            }
        }

        public void StopScan()
        {
            scannerViewModel.Scanning = false;

            bluetoothAdapter.BluetoothLeScanner.StopScan(this);

            mHandler.RemoveCallbacks(scanerCallback);
            logger.TraceInformation("Turn Scanning Off");
        }

        public Device GetDevice(int value)
        {
            return devicesList[value];
        }

        ~DeviceScanner()
        {
            logger.TraceInformation("Destroy");
        }
    }
}