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
    class DeviceScanner : ScanCallback, IDeviceScanner, IOnRequestPermissionsResultCallback
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
        private static int PERMISSION_REQUEST_COARSE_LOCATION = 0;

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

        public bool CheckPermissions()
        {
            return (ActivityCompat.CheckSelfPermission(thisActivity, Manifest.Permission.AccessCoarseLocation) == Permission.Granted);
            //|| (ActivityCompat.CheckSelfPermission(thisActivity, Manifest.Permission.AccessFineLocation) == Permission.Granted);
        }

        public bool CheckSupportBLE()
        {
            if (!thisActivity.PackageManager.HasSystemFeature(PackageManager.FeatureBluetoothLe))
            {
                logger.TraceError("BLE is not supported");
                return false;
            }
            return true;
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

        public void GetRuntimePermissions()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                logger.TraceInformation("Build.VERSION.SdkInt >= API 23");
                const string permission = Manifest.Permission.AccessCoarseLocation;
                if (thisActivity.CheckSelfPermission(permission) != Permission.Granted)
                {
                    logger.TraceWarning("AccessCoarseLocation Permission is not granted");
                    AlertDialog.Builder builder = new AlertDialog.Builder(thisActivity);
                    builder.SetTitle("This app needs location access");
                    builder.SetMessage("Please grant location access so this app can detect bluetooth devices.");
                    builder.SetPositiveButton("OK", (senderAlert, args) => {
                        logger.TraceInformation("Try get AccessCoarseLocation Permission");
                        thisActivity.RequestPermissions(new String[] { Manifest.Permission.AccessCoarseLocation }, PERMISSION_REQUEST_COARSE_LOCATION);
                    });
                    builder.Show();
                }
            }
            else
            {
                logger.TraceWarning("Build.VERSION.SdkInt < API 23");
            }
        }

        public void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode == PERMISSION_REQUEST_COARSE_LOCATION)
            {
                if (grantResults[0] == Permission.Granted)
                {
                    logger.TraceInformation("coarse location permission granted");
                }
                else
                {
                    AlertDialog.Builder builder = new AlertDialog.Builder(thisActivity);
                    builder.SetTitle("Functionality limited");
                    logger.TraceInformation("Since location access has not been granted, this app will not be able to discover beacons when in the background.");
                    builder.SetMessage("Since location access has not been granted, this app will not be able to discover beacons when in the background.");
                    builder.SetPositiveButton("OK", (senderAlert, args) => { });
                    builder.Show();
                }
            }
        }

        public void ScanLeDevice()
        {
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
    }
}