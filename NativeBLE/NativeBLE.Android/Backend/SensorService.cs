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
using Xamarin.Forms;
using NativeBLE.Droid.Native;
using Android.Bluetooth;
using Java.Util;
using System.Threading;

namespace NativeBLE.Droid.Backend
{
    [Service(Name = "com.lpestl.NativeBLE.sensorservice")]
    public class SensorService : Service
    {
        private Logger logger = new Logger();

        public IBinder Binder { get; private set; }

        public BluetoothManager bluetoothManager;
        public BluetoothAdapter bluetoothAdapter;
        public String bluetoothDeviceAddress;
        public BluetoothGatt bluetoothGatt;
        public ProfileState connectionState = ProfileState.Disconnected;
        public static ProgressDialog progressDialog;

        public static String ACTION_GATT_CONNECTED =            "com.lpestl.bluetooth.le.ACTION_GATT_CONNECTED";
        public static String ACTION_GATT_DISCONNECTED =         "com.lpestl.bluetooth.le.ACTION_GATT_DISCONNECTED";
        public static String ACTION_GATT_SERVICES_DISCOVERED =  "com.lpestl.bluetooth.le.ACTION_GATT_SERVICES_DISCOVERED";
        public static String ACTION_DATA_AVAILABLE =            "com.lpestl.bluetooth.le.ACTION_DATA_AVAILABLE";
        public static String RSSI_DATA_AVAILABLE =              "com.lpestl.bluetooth.le.RSSI_DATA_AVAILABLE";
        public static String EXTRA_DATA =                       "com.lpestl.bluetooth.le.EXTRA_DATA";
        
        public static String EXTRA_RSSI_DATA =                  "RSSI data";
        public static String EXTRA_SENSOR_A =                   "SENSOR A";
        public static String EXTRA_SENSOR_B =                   "SENSOR B";

        public static UUID UUID_PRESSURE_SERVICE =              UUID.FromString(SampleGattAttributes.PRESSURE_SERVICE);
        public static UUID UUID_DEVICE_INFO_SERVICE =           UUID.FromString(SampleGattAttributes.DEVICE_INFO_SERVICE);

        public static int iRssi = 0;

        private Activity thisActivity;
        private ServiceBluetothGattCallback gattCallback = new ServiceBluetothGattCallback();

        public SensorService()
        {
            logger.TAG = "SensorService";
            logger.TraceInformation("Sensor Service constuctor");
            gattCallback.SetService(this);
        }

        public override IBinder OnBind(Intent intent)
        {
            logger.TraceInformation("Sensor Service called from Event OnBind");
            this.Binder = new SensorBinder(this);
            return this.Binder;
        }

        public override bool OnUnbind(Intent intent)
        {
            logger.TraceInformation("Sensor Service called from Event OnUnbind");
            Close();
            return base.OnUnbind(intent);
        }

        public override void OnCreate()
        {
            base.OnCreate();
            logger.TraceInformation("Sensor Service called from Event OnCreate");
        }

        public override void OnDestroy()
        {
            this.Binder = null;
            logger.TraceInformation("Sensor Service called from Event OnDestroy");
            base.OnDestroy();
        }

        public bool Initialize()
        {
            // For API level 18 and above, get a reference to BluetoothAdapter through BluetoothManager.
            thisActivity = Xamarin.Forms.Forms.Context as Activity;
            if (bluetoothManager == null)
            {
                bluetoothManager = thisActivity.GetSystemService(Context.BluetoothService) as BluetoothManager;
                if (bluetoothManager == null)
                {
                    logger.TraceError("Unable to initialize BluetoothManager.");
                    if (SensorDataPage.Instance != null) SensorDataPage.Instance.SensorView.DebugString = "Unable to initialize BluetoothManager.";
                    Toast.MakeText(thisActivity.ApplicationContext, "Unable to initialize BluetoothManager.", ToastLength.Short).Show();
                    return false;
                }
            }

            bluetoothAdapter = bluetoothManager.Adapter;
            if (bluetoothAdapter == null)
            {
                logger.TraceError("Unable to obtain a BluetoothAdapter.");
                if (SensorDataPage.Instance != null) SensorDataPage.Instance.SensorView.DebugString = "Unable to obtain a BluetoothAdapter.";
                Toast.MakeText(thisActivity.ApplicationContext, "Unable to obtain a BluetoothAdapter.", ToastLength.Short).Show();
                return false;
            }

            return true;
        }

        /**
         * Connects to the GATT server hosted on the Bluetooth LE device.
         *
         * @param address The device address of the destination device.
         *
         * @return Return true if the connection is initiated successfully. The connection result
         *         is reported asynchronously through the
         *         {@code BluetoothGattCallback#onConnectionStateChange(android.bluetooth.BluetoothGatt, int, int)}
         *         callback.
         */
        public bool Connect(String address)
        {
            logger.TraceInformation("Try connect to device");
            if (bluetoothAdapter == null || address == null)
            {
                logger.TraceWarning("BluetoothAdapter not initialized or unspecified address.");
                Toast.MakeText(thisActivity.ApplicationContext, "BluetoothAdapter not initialized or unspecified address.", ToastLength.Short).Show();
                return false;
            }
            
            BluetoothDevice device = bluetoothAdapter.GetRemoteDevice(address);
            if (device == null)
            {
                logger.TraceWarning("Device not found.  Unable to connect.");
                Toast.MakeText(thisActivity.ApplicationContext, "Device not found. Unable to connect..", ToastLength.Short).Show();
                return false;
            }
            // We want to directly connect to the device, so we are setting the autoConnect
            // parameter to false.
            bluetoothGatt = device.ConnectGatt(this, false, gattCallback);
            logger.TraceInformation("Trying to create a new connection.");
            bluetoothDeviceAddress = address;
            connectionState = ProfileState.Connecting;
            if (SensorDataPage.Instance != null)
            {
                SensorDataPage.Instance.connectionStopwatch.Reset();
                SensorDataPage.Instance.connectionStopwatch.Start();
                //SensorDataPage.Instance.firstDataStopwatch.Start();
                SensorDataPage.Instance.SensorView.ConnectionState = "Connecting...";
            }

            progressDialog = ProgressDialog.Show(thisActivity, "Please wait...", "Connecting...", true);
            
            return true;
        }

        /**
         * Disconnects an existing connection or cancel a pending connection. The disconnection result
         * is reported asynchronously through the
         * {@code BluetoothGattCallback#onConnectionStateChange(android.bluetooth.BluetoothGatt, int, int)}
         * callback.
         */
        public void Disconnect()
        {
            logger.TraceInformation("Try disconnect");
            if (bluetoothAdapter == null || bluetoothGatt == null)
            {
                logger.TraceWarning("BluetoothAdapter not initialized");
                return;
            }
            SensorDataPage.Instance.disconnectionStopwatch.Reset();
            SensorDataPage.Instance.disconnectionStopwatch.Start();
            progressDialog = ProgressDialog.Show(thisActivity, "Please wait...", "Disconnecting...", true);
            bluetoothGatt.Disconnect();
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

        /**
         * After using a given BLE device, the app must call this method to ensure resources are
         * released properly.
         */
        public void Close()
        {
            if (bluetoothGatt == null)
            {
                return;
            }
            bluetoothGatt.Close();
            bluetoothGatt = null;
        }

        internal IList<BluetoothGattService> GetSupportedGattServices()
        {
            if (bluetoothGatt == null) return null;

            return bluetoothGatt.Services;
        }

        /**
         * Request a read on a given {@code BluetoothGattCharacteristic}. The read result is reported
         * asynchronously through the {@code BluetoothGattCallback#onCharacteristicRead(android.bluetooth.BluetoothGatt, android.bluetooth.BluetoothGattCharacteristic, int)}
         * callback.
         *
         * @param characteristic The characteristic to read from.
         */
        public void ReadCharacteristic(BluetoothGattCharacteristic characteristic)
        {
            if (bluetoothAdapter == null || bluetoothGatt == null)
            {
                logger.TraceWarning("BluetoothAdapter not initialized");
                return;
            }
            bluetoothGatt.ReadCharacteristic(characteristic);
            logger.TraceInformation("Read Characteristic");
        }

        /**
         * Enables or disables notification on a give characteristic.
         *
         * @param characteristic Characteristic to act on.
         * @param enabled If true, enable notification.  False otherwise.
         */
        internal void SetCharacteristicNotification(BluetoothGattCharacteristic characteristic, bool enabled)
        {
            if (bluetoothAdapter == null || bluetoothGatt == null)
            {
                logger.TraceWarning("BluetoothAdapter not initialized");
                return;
            }

            if (characteristic.Uuid.Equals(UUID.FromString(SampleGattAttributes.PRESSURE_NOTIFICATION_HANDLE)))
            {
                bluetoothGatt.SetCharacteristicNotification(characteristic, enabled);
                logger.TraceInformation("Setting notification: Pressure Characteristic detected ");

                BluetoothGattDescriptor descriptor = characteristic.GetDescriptor(UUID.FromString("00002902-0000-1000-8000-00805F9B34FB"));
                if (descriptor != null)
                {
                    logger.TraceInformation("Setting notification: Pressure Descriptor Found");
                    descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray<byte>());
                    bluetoothGatt.WriteDescriptor(descriptor);
                    logger.TraceInformation("Setting notification: Write Pressure Descriptor");

                }
                else { logger.TraceWarning("NOTIFICATION SET UP IGNORED"); }

            }
        }

        public void BroadcastUpdate(String action)
        {
            Intent intent = new Intent(action);

            SendBroadcast(intent);
        }

        public void BroadcastUpdate(String action, int rssi)
        {
            Intent intent = new Intent(action);

            Bundle extras = new Bundle();
            extras.PutString(EXTRA_RSSI_DATA, rssi.ToString());
            intent.PutExtras(extras);
            logger.TraceInformation("RSSI loaded within intent");

            SendBroadcast(intent);
        }

        public void BroadcastUpdate(String action,
                                     BluetoothGattCharacteristic characteristic)
        {
            Intent intent = new Intent(action);

            // This is special handling for the Heart Rate Measurement profile.  Data parsing is
            // carried out as per profile specifications:
            // http://developer.bluetooth.org/gatt/characteristics/Pages/CharacteristicViewer.aspx?u=org.bluetooth.characteristic.heart_rate_measurement.xml
            
            if (SampleGattAttributes.PRESSURE_NOTIFICATION_HANDLE.Equals(characteristic.Uuid.ToString()))
            {
                logger.TraceInformation("Building Broadcaster: Pressure characteristic data");
                logger.TraceInformation(characteristic.Uuid.ToString());
                // For all other profiles, writes the data formatted in HEX.
                byte[] data = characteristic.GetValue();

                if (data != null && data.Length > 0)
                {
                    Bundle extras = new Bundle();

                    long lData = 0;
                    String sData;

                    var stringBytes = BitConverter.ToString(data);

                    lData = BytesToLong(data, 0, 1);
                    sData = lData.ToString();

                    extras.PutString(EXTRA_DATA, stringBytes);
                    extras.PutString(EXTRA_SENSOR_A, sData);
                    extras.PutString(EXTRA_SENSOR_B, BytesToLong(data, 2, 3).ToString());

                    intent.PutExtras(extras);
                }
            }
            SendBroadcast(intent);
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
    }
}