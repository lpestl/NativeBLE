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

[assembly: Xamarin.Forms.Dependency(typeof(NativeBLE.Droid.Native.NativeDeviceScanner))]
namespace NativeBLE.Droid.Native
{
    class NativeDeviceScanner : ScanCallback, IDeviceScanner
    {
        private BluetoothAdapter mBluetoothAdapter;
        private NativeDeviceList mDeviceList = new NativeDeviceList();
        private Handler mHandler;

        private static int REQUEST_ENABLE_BT = 1;
        // Stops scanning after 10 seconds.
        private static long SCAN_PERIOD = 20000;
        private static int PERMISSION_REQUEST_COARSE_LOCATION = 1;

        public static ParcelUuid parcelUuid = ParcelUuid.FromString("0000aa40-0000-1000-8000-00805f9b34fb");

        private Activity mThisActivity;
        private Logger logger;

        private MainPageViewModel pageViewModel;
        public MainPageViewModel PageViewModel
        {
            get { return pageViewModel; }
            set
            {
                pageViewModel = value;
                mDeviceList.DeviceViewModelList = value.Devices;
            }
        }

        public NativeDeviceScanner()
        {
            logger = new Logger();
            logger.TAG = "NativeDeviceScanner";
            logger.LogInfo("NativeDeviceScanner constructor");
            
            mHandler = new Handler();
            mThisActivity = Xamarin.Forms.Forms.Context as Activity;
        }

        public bool CheckPermissions()
        {
            return (ActivityCompat.CheckSelfPermission(mThisActivity, Manifest.Permission.AccessCoarseLocation) == Permission.Granted);
            //|| (ActivityCompat.CheckSelfPermission(thisActivity, Manifest.Permission.AccessFineLocation) == Permission.Granted);
        }

        public bool CheckSupportBLE()
        {
            if (!mThisActivity.PackageManager.HasSystemFeature(PackageManager.FeatureBluetoothLe))
            {
                logger.LogWarning("BLE is not supported");
                return false;
            }
            return true;
        }

        public void GetRuntimePermissions()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                logger.LogInfo("Build.VERSION.SdkInt >= API 23");
                const string permission = Manifest.Permission.AccessCoarseLocation;
                if (mThisActivity.CheckSelfPermission(permission) != (int)Permission.Granted)
                {
                    logger.LogWarning("AccessCoarseLocation Permission is not granted");
                    AlertDialog.Builder builder = new AlertDialog.Builder(Android.App.Application.Context.ApplicationContext);
                    builder.SetTitle("This app needs location access");
                    builder.SetMessage("Please grant location access so this app can detect bluetooth devices.");
                    builder.SetPositiveButton("OK", (senderAlert, args) => {
                        logger.LogInfo("Try get AccessCoarseLocation Permission");
                        mThisActivity.RequestPermissions(new String[] { Manifest.Permission.AccessCoarseLocation }, PERMISSION_REQUEST_COARSE_LOCATION);
                    });
                    builder.Show();
                }
            } else
            {
                logger.LogWarning("Build.VERSION.SdkInt < API 23");
            }
        }

        public void GetBluetoothAdapter()
        {
            logger.LogInfo("Get bluetooth Adapter");
            BluetoothManager bluetoothManager = mThisActivity.GetSystemService(Context.BluetoothService) as BluetoothManager;
            mBluetoothAdapter = bluetoothManager.Adapter;
            if (mBluetoothAdapter != null)
            {
                if (!mBluetoothAdapter.IsEnabled)
                {
                    logger.LogInfo("Try to turn on the Bluetooth adapter");

                    Intent enableBtIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
                    mThisActivity.StartActivityForResult(enableBtIntent, REQUEST_ENABLE_BT);
                    
                    //mBluetoothAdapter.Enable();
                }
            } else
            {
                logger.LogError("Bluetooth not supported.");
            }
        }
        
        public void ScanLeDevice()
        {
            mHandler.PostDelayed(() => {
                logger.LogInfo("mHandler.PostDelayed called. Scanner stoped.");
                pageViewModel.Scanning = false;
                mBluetoothAdapter.BluetoothLeScanner.StopScan(this);
                mBluetoothAdapter.BluetoothLeScanner.Dispose();
            }, SCAN_PERIOD);

            pageViewModel.Scanning = true;
            ScanFilter scanFilter = (new ScanFilter.Builder()).SetServiceUuid(parcelUuid).Build();
            List<ScanFilter> scanFilters = new List<ScanFilter>();
            
            //if (pageViewModel.Devices.Count > 0)
            //{
            //    pageViewModel.Devices.Clear();
            //}
            if (!mDeviceList.IsEmpty())
            {
                mDeviceList.Clear();
            }

            scanFilters.Add(scanFilter);
            ScanSettings scanSettings =
                    new ScanSettings.Builder().Build();

            mBluetoothAdapter.BluetoothLeScanner.StartScan(scanFilters, scanSettings, this);
            logger.LogInfo("Start scanning.");
        }

        public void StopScan()
        {
            pageViewModel.Scanning = false;

            mBluetoothAdapter.BluetoothLeScanner.StopScan(this);
            mBluetoothAdapter.BluetoothLeScanner.Dispose();

            //mHandler.removeCallbacks(r);
            logger.LogInfo("Turn Scanning Off");
        }

        public override void OnScanResult(ScanCallbackType callbackType, ScanResult result)
        {
            logger.LogInfo(String.Format("onScanResult: found {0} - {1}", result.Device.Name, result.Device.Address));
            base.OnScanResult(callbackType, result);

            //var contais = false;
            //foreach (var device in pageViewModel.Devices)
            //{
            //    if (device.Address.Equals(result.Device.Address))
            //    {
            //        contais = true;
            //        break;
            //    }
            //}

            if (!mDeviceList.Contains(new DeviceViewModel(result.Device.Name, result.Device.Address)))
            {
                //pageViewModel.Devices.Add(new DeviceViewModel(result.Device.Name, result.Device.Address));
                mDeviceList.Add(result.Device);
            }
        }        
    }
}