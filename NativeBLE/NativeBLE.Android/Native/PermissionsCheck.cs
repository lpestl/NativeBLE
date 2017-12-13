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

[assembly: Xamarin.Forms.Dependency(typeof(NativeBLE.Droid.Native.PermissionsCheck))]
namespace NativeBLE.Droid.Native
{
    class PermissionsCheck : Java.Lang.Object, IPermissionsCheck, IOnRequestPermissionsResultCallback
    {
        private Logger logger = new Logger();
        private Activity thisActivity;

        private static int PERMISSION_REQUEST_COARSE_LOCATION = 0;

        public PermissionsCheck()
        {
            logger.TAG = "PermissionsCheck";
            thisActivity = Xamarin.Forms.Forms.Context as Activity;
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

        public void RestartAdapter()
        {
            BluetoothManager bluetoothManager = thisActivity.GetSystemService(Context.BluetoothService) as BluetoothManager;
            var bluetoothAdapter = bluetoothManager.Adapter;

            if (bluetoothAdapter != null)
            {
                if (!bluetoothAdapter.IsEnabled)
                {
                    logger.TraceInformation("Try to turn on the Bluetooth adapter");
                    
                    bluetoothAdapter.Enable();
                } else
                {
                    logger.TraceInformation("Try to turn off the Bluetooth adapter");

                    bluetoothAdapter.Disable();
                    
                    logger.TraceInformation("Try to turn on the Bluetooth adapter");

                    bluetoothAdapter.Enable();
                }
            }
            else
            {
                logger.TraceError("Bluetooth not supported.");
            }

        }
    }
}