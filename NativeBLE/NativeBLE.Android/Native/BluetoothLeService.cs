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
using Java.Nio.Charset;

namespace NativeBLE.Droid.Native
{
    //[Service]
    public class BluetoothLeService : Service
    {
        private static String TAG = "BluetoothLeService";

        private static BluetoothLeService instance;

        public BluetoothLeService() { }

        public static BluetoothLeService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new BluetoothLeService();
                }
                return instance;
            }
        }

        private BluetoothManager mBluetoothManager;
        private BluetoothAdapter mBluetoothAdapter;
        private String mBluetoothDeviceAddress;
        private BluetoothGatt mBluetoothGatt;
        private ProfileState mConnectionState = ProfileState.Disconnected;
        
        public static String ACTION_GATT_CONNECTED =              "com.lpestl.bluetooth.le.ACTION_GATT_CONNECTED";
        public static String ACTION_GATT_DISCONNECTED =           "com.lpestl.bluetooth.le.ACTION_GATT_DISCONNECTED";
        public static String ACTION_GATT_SERVICES_DISCOVERED =    "com.lpestl.bluetooth.le.ACTION_GATT_SERVICES_DISCOVERED";
        public static String ACTION_DATA_AVAILABLE =              "com.lpestl.bluetooth.le.ACTION_DATA_AVAILABLE";
        public static String RSSI_DATA_AVAILABLE =                "com.lpestl.bluetooth.le.RSSI_DATA_AVAILABLE";
        public static String EXTRA_DATA =                         "com.lpestl.bluetooth.le.EXTRA_DATA";
        public static String EXTRA_BATTERY_DATA =                 "Battery Data";

        public static String EXTRA_FIRMWARE_VERSION_DATA =        "Firmware Version Data";
        public static String EXTRA_BATCH_VERSION_DATA =           "Batch Version Data";

        public static String EXTRA_RSSI_DATA =                    "Batch Version Data";
        public static String EXTRA_SENSOR_A =                     " SENSOR A";
        public static String EXTRA_SENSOR_B =                     " SENSOR B";
        public static UUID UUID_PRESSURE_SERVICE =
                UUID.FromString(SampleGattAttributes.PRESSURE_SERVICE);
        public static UUID UUID_BATTERY_SERVICE =
                UUID.FromString(SampleGattAttributes.BATTERY_SERVICE);
        public static UUID UUID_DEVICE_INFO_SERVICE =
                UUID.FromString(SampleGattAttributes.DEVICE_INFO_SERVICE);

        public static int iRssi = 0;

        private IBinder mBinder = new LocalBinder();
        private MyBluetoothGattCallback mGattCallback = new MyBluetoothGattCallback();
        private Activity mThisActivity;

        public class LocalBinder : Binder
        {
            public BluetoothLeService getService() {
                //return BluetoothLeService.this;
                return BluetoothLeService.Instance;
            }
        }

        public override IBinder OnBind(Intent intent)
        {
            return mBinder;
        }

        public override bool OnUnbind(Intent intent)
        {
            Log.Debug(TAG, "Unbind callback recived");
            close();
            return base.OnUnbind(intent);
        }
        //private Xamarin.Forms.Button mStartButton;
        //private TextView mDebug;

        private void broadcastUpdate(String action)
        {
            Intent intent = new Intent(action);

            mThisActivity.ApplicationContext.SendBroadcast(intent);
        }

        private void broadcastUpdate(String action, int rssi)
        {
            Intent intent = new Intent(action);


            Bundle extras = new Bundle();
            extras.PutString(EXTRA_RSSI_DATA, rssi.ToString());
            intent.PutExtras(extras);
            Log.Debug(TAG, "RSSI loaded within intent");

            mThisActivity.ApplicationContext.SendBroadcast(intent);

        }

        private void broadcastUpdate(String action,
                                     BluetoothGattCharacteristic characteristic)
        {
            Intent intent = new Intent(action);

            // This is special handling for the Heart Rate Measurement profile.  Data parsing is
            // carried out as per profile specifications:
            // http://developer.bluetooth.org/gatt/characteristics/Pages/CharacteristicViewer.aspx?u=org.bluetooth.characteristic.heart_rate_measurement.xml


            if (SampleGattAttributes.BATTERY_NOTIFICATION_HANDLE.Equals(characteristic.Uuid.ToString()))
            {
                Log.Debug(TAG, "Building Broadcaster: Battery characteristic data");

                byte[] data = characteristic.GetValue();

                if (data != null && data.Length > 0)
                {
                    Bundle extras = new Bundle();
                    Log.Debug(TAG, "Battery Data not null");
                    Log.Debug(TAG, data.ToString());
                    long lData = 0;
                    String sData;
                    lData = bytesToLong(data, 0, 0);
                    Log.Debug(TAG, "Data converted to Long");

                    sData = lData.ToString();
                    Log.Debug(TAG, "Long converted to String");
                    Log.Debug(TAG, sData);

                    extras.PutString(EXTRA_BATTERY_DATA, sData);
                    Log.Warn(TAG, "Battery data send as an extra_battery_data extra");
                    intent.PutExtras(extras);
                }

            }

            if (SampleGattAttributes.FIRMWARE_VERSION_HANDLE.Equals(characteristic.Uuid.ToString()))
            {
                Log.Debug(TAG, "Building Broadcaster: Firmware version characteristic data");

                byte[] data = characteristic.GetValue();

                if (data != null && data.Length > 0)
                {
                    Bundle extras = new Bundle();
                    Log.Debug(TAG, "Batch Data not null");
                    Log.Debug(TAG, data.ToString());

                    long lData = 0;
                    //String sData;
                    //lData = bytesToLong(data, 1, 1);
                    Log.Debug(TAG, "Data converted to Long");
                    //String sData2 = new String(data, "UTF_8");
                    String sData = lData.ToString();//new String(data, StandardCharsets.Utf8);
                    //String text = new String(data, 0, data.length, "ASCII");
                    // sData = Long.toString(lData);
                    Log.Debug(TAG, "Data converted to String");
                    Log.Debug(TAG, sData);

                    // Log.d(TAG, sdata2);
                    extras.PutString(EXTRA_FIRMWARE_VERSION_DATA, sData);
                    Log.Warn(TAG, "Firmware version data send as an extra_firmware_version_data extra");
                    intent.PutExtras(extras);
                }

            }

            if (SampleGattAttributes.BATCH_VERSION_HANDLE.Equals(characteristic.Uuid.ToString()))
            {
                Log.Debug(TAG, "Building Broadcaster: Batch version characteristic data");

                byte[] data = characteristic.GetValue();

                if (data != null && data.Length > 0)
                {
                    Bundle extras = new Bundle();
                    Log.Debug(TAG, "Batch Data not null");
                    String sData = data.ToString();
                    Log.Debug(TAG, sData);
                    extras.PutString(EXTRA_BATCH_VERSION_DATA, sData);
                    Log.Warn(TAG, "Batch version data send as an extra_batch_version_data extra");
                    intent.PutExtras(extras);
                }

            }


            if (SampleGattAttributes.PRESSURE_NOTIFICATION_HANDLE.Equals(characteristic.Uuid.ToString()))
            {
                Log.Debug(TAG, "Building Broadcaster: Pressure characteristic data");
                Log.Debug(TAG, characteristic.Uuid.ToString());
                // For all other profiles, writes the data formatted in HEX.
                byte[] data = characteristic.GetValue();

                if (data != null && data.Length > 0)
                {
                    Bundle extras = new Bundle();
                    // Log.d(TAG, "Data not null");
                    //intent.putExtra(EXTRA_DATA,data);
                    long lData = 0;
                    String sData;

                    var stringBytes = BitConverter.ToString(data);
                    //StringBuilder stringBuilder = new StringBuilder(data.Length);
                    //foreach (byte byteChar in data)
                    //    stringBuilder.Append(String.Format("%02X ", byteChar));

                    lData = bytesToLong(data, 0, 1);
                    sData = lData.ToString();

                    extras.PutString(EXTRA_DATA, stringBytes);
                    extras.PutString(EXTRA_SENSOR_A, sData);
                    extras.PutString(EXTRA_SENSOR_B, bytesToLong(data, 2, 3).ToString());
                    //intent.putExtra(EXTRA_DATA, data.toString());
                    intent.PutExtras(extras);
                }
            }
            mThisActivity.ApplicationContext.SendBroadcast(intent);
        }

        public static long bytesToLong(byte[] b, int minbyte, int maxbyte)
        {
            long result = 0;
            for (int i = minbyte; i <= maxbyte; i++)
            {
                result <<= 8;
                result |= (b[i] & 0xFF);
            }
            return result;
        }

        public class MyBluetoothGattCallback : BluetoothGattCallback
        {
            public override void OnConnectionStateChange(BluetoothGatt gatt, [GeneratedEnum] GattStatus status, [GeneratedEnum] ProfileState newState)
            {
                //base.OnConnectionStateChange(gatt, status, newState);
                String intentAction;
                // mDebug.findViewById(R.id.debug_text);

                if (newState == ProfileState.Connected)
                {
                    intentAction = ACTION_GATT_CONNECTED;
                    BluetoothLeService.Instance.mConnectionState = ProfileState.Connected;
                    NativeSensorData.Instance.GetSensorViewModel().ConnectionState = "Connected";
                    bool rssiStatus = BluetoothLeService.Instance.mBluetoothGatt.ReadRemoteRssi();
                    BluetoothLeService.Instance.broadcastUpdate(intentAction);
                    //Toast.makeText(DeviceControlActivity.this,"Connected to GATT server.",Toast.LENGTH_SHORT).show();
                    Log.Debug(TAG, "Connected to GATT server.");
                    // Attempts to discover services after successful connection.
                    Log.Debug(TAG, "Attempting to start service discovery:" +
                            BluetoothLeService.Instance.mBluetoothGatt.DiscoverServices());
                    //Toast.makeText(getApplicationContext(),"Attempting to start service discovery:",Toast.LENGTH_SHORT).show();
                    //readCharacteristic();

                }
                else if (newState == ProfileState.Disconnected)
                {
                    intentAction = ACTION_GATT_DISCONNECTED;
                    BluetoothLeService.Instance.mConnectionState = ProfileState.Disconnected;
                    NativeSensorData.Instance.GetSensorViewModel().ConnectionState = "Disconnected";
                    Log.Debug(TAG, "Disconnected from GATT server.");
                    // Toast.makeText(getApplicationContext(),"Disconnected from GATT server.",Toast.LENGTH_SHORT).show();
                    BluetoothLeService.Instance.broadcastUpdate(intentAction);

                }
            }

            public override void OnServicesDiscovered(BluetoothGatt gatt, [GeneratedEnum] GattStatus status)
            {
                //base.OnServicesDiscovered(gatt, status);
                if (status == BluetoothGatt.GattSuccess)
                {
                    BluetoothLeService.Instance.broadcastUpdate(ACTION_GATT_SERVICES_DISCOVERED);
                }
                else
                {
                    Log.Warn(TAG, "onServicesDiscovered received: " + status);
                }
            }

            public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status)
            {
                //base.OnCharacteristicRead(gatt, characteristic, status);
                Log.Debug(TAG, "Event: Characteristic Read");

                if (SampleGattAttributes.BATTERY_NOTIFICATION_HANDLE.Equals(characteristic.Uuid.ToString()))
                {
                    if (status == BluetoothGatt.GattSuccess)
                    {
                        BluetoothLeService.Instance.broadcastUpdate(ACTION_DATA_AVAILABLE, characteristic);

                    }
                    Log.Debug(TAG, "Broadcast Battery Characteristic");
                    Log.Debug(TAG, characteristic.Uuid.ToString());
                }


                if (SampleGattAttributes.PRESSURE_NOTIFICATION_HANDLE.Equals(characteristic.Uuid.ToString()))
                {
                    if (status == BluetoothGatt.GattSuccess)
                    {
                        BluetoothLeService.Instance.broadcastUpdate(ACTION_DATA_AVAILABLE, characteristic);

                    }
                    Log.Debug(TAG, "Broadcast Pressure Characteristic");
                    Log.Debug(TAG, characteristic.Uuid.ToString());
                }

                if (SampleGattAttributes.FIRMWARE_VERSION_HANDLE.Equals(characteristic.Uuid.ToString()))
                {
                    if (status == BluetoothGatt.GattSuccess)
                    {
                        BluetoothLeService.Instance.broadcastUpdate(ACTION_DATA_AVAILABLE, characteristic);

                    }
                    Log.Debug(TAG, "Broadcast Firmware Version Characteristic");
                    Log.Debug(TAG, characteristic.Uuid.ToString());
                }

                if (SampleGattAttributes.BATCH_VERSION_HANDLE.Equals(characteristic.Uuid.ToString()))
                {
                    if (status == BluetoothGatt.GattSuccess)
                    {
                        BluetoothLeService.Instance.broadcastUpdate(ACTION_DATA_AVAILABLE, characteristic);

                    }
                    Log.Debug(TAG, "Broadcast Batch Version Characteristic");
                    Log.Debug(TAG, characteristic.Uuid.ToString());
                }

                // if()


            }


            public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
            {
                //base.OnCharacteristicChanged(gatt, characteristic);
                Log.Debug(TAG, "Event: Characteristic change");
                
                if (SampleGattAttributes.PRESSURE_NOTIFICATION_HANDLE.Equals(characteristic.Uuid.ToString()))
                {
                    BluetoothLeService.Instance.broadcastUpdate(ACTION_DATA_AVAILABLE, characteristic);
                    Log.Debug(TAG, "Broadcast Pressure Characteristic");
                    Log.Debug(TAG, characteristic.Uuid.ToString());
                    bool rssiStatus = BluetoothLeService.Instance.mBluetoothGatt.ReadRemoteRssi();
                }
            }


            public override void OnReadRemoteRssi(BluetoothGatt gatt, int rssi, [GeneratedEnum] GattStatus status)
            {
                //base.OnReadRemoteRssi(gatt, rssi, status);
                if (status == BluetoothGatt.GattSuccess)
                {
                    Log.Debug(TAG, String.Format("BluetoothGatt ReadRssi {0}", rssi));
                    iRssi = rssi;

                    BluetoothLeService.Instance.broadcastUpdate(RSSI_DATA_AVAILABLE, rssi);
                }
            }
        }

        public bool initialize()
        {
            // For API level 18 and above, get a reference to BluetoothAdapter through
            // BluetoothManager.
            mThisActivity = mThisActivity = Xamarin.Forms.Forms.Context as Activity;
            if (mBluetoothManager == null)
            {
                //var sysService = GetSystemService(Context.BluetoothService);
                mBluetoothManager = mThisActivity.GetSystemService(Context.BluetoothService) as BluetoothManager;
                if (mBluetoothManager == null)
                {
                    Log.Error(TAG, "Unable to initialize BluetoothManager.");
                    //Toast.MakeText(getApplicationContext(), "Unable to initialize BluetoothManager.", Toast.LENGTH_SHORT).show();
                    return false;
                }
            }

            mBluetoothAdapter = mBluetoothManager.Adapter;
            if (mBluetoothAdapter == null)
            {
                Log.Error(TAG, "Unable to obtain a BluetoothAdapter.");
                //Toast.makeText(getApplicationContext(), "Unable to initialize BluetoothManager.", Toast.LENGTH_SHORT).show();
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
        public bool connect(String address)
        {
            //mDebug.findViewById(R.id.debug_text);
            if (mBluetoothAdapter == null || address == null)
            {
                Log.Warn(TAG, "BluetoothAdapter not initialized or unspecified address.");
                //mDebug.setText("BluetoothAdapter not initialized or unspecified address.");
                //Toast.makeText(getApplicationContext(), "BluetoothAdapter not initialized or unspecified address.", Toast.LENGTH_SHORT).show();
                return false;
            }

            // Previously connected device.  Try to reconnect.
            /*
            if (mBluetoothDeviceAddress != null && address.equals(mBluetoothDeviceAddress)
                    && mBluetoothGatt != null) {
                Log.d(TAG, "Trying to use an existing mBluetoothGatt for connection.");
                //mDebug.setText("Trying to use an existing mBluetoothGatt for connection.");
                Toast.makeText(getApplicationContext(),"Trying to use an existing mBluetoothGatt for connection..",Toast.LENGTH_SHORT).show();
                if (mBluetoothGatt.connect()) {
                    mConnectionState = STATE_CONNECTING;
                   // mDebug.setText("Connecting...");
                    Toast.makeText(getApplicationContext(),"Connecting...",Toast.LENGTH_SHORT).show();
                    return true;
                } else {
                    Toast.makeText(getApplicationContext(),"Not connecting...",Toast.LENGTH_SHORT).show();
                    return false;
                }
            }
            */

            BluetoothDevice device = mBluetoothAdapter.GetRemoteDevice(address);
            if (device == null)
            {
                Log.Warn(TAG, "Device not found.  Unable to connect.");
                //mDebug.setText("Device not found.  Unable to connect.");
                //Toast.makeText(getApplicationContext(), "Device not found.  Unable to connect..", Toast.LENGTH_SHORT).show();
                return false;
            }
            // We want to directly connect to the device, so we are setting the autoConnect
            // parameter to false.
            /*
            mBluetoothAdapter.cancelDiscovery();
            if(mBluetoothGatt == null)
                mBluetoothGatt = device.connectGatt(this, false, mGattCallback);
            else
                mBluetoothGatt.discoverServices();
                */
            mBluetoothGatt = device.ConnectGatt(this, false, mGattCallback);
            Log.Debug(TAG, "Trying to create a new connection.");
            //Toast.makeText(getApplicationContext(), "Trying to create a new connection.", Toast.LENGTH_SHORT).show();
            mBluetoothDeviceAddress = address;
            mConnectionState = ProfileState.Connecting;
            NativeSensorData.Instance.GetSensorViewModel().ConnectionState = "Connecting...";
            return true;
        }

        /**
         * Disconnects an existing connection or cancel a pending connection. The disconnection result
         * is reported asynchronously through the
         * {@code BluetoothGattCallback#onConnectionStateChange(android.bluetooth.BluetoothGatt, int, int)}
         * callback.
         */
        public void disconnect()
        {
            if (mBluetoothAdapter == null || mBluetoothGatt == null)
            {
                Log.Warn(TAG, "BluetoothAdapter not initialized");
                return;
            }
            //Toast.makeText(getApplicationContext(), "Calling for a disconnect.", Toast.LENGTH_SHORT).show();
            mBluetoothGatt.Disconnect();
            /*
            if(mBluetoothGatt != null) {
                mBluetoothGatt.close();
                mBluetoothGatt = null;
            }
            */
        }

        /**
         * After using a given BLE device, the app must call this method to ensure resources are
         * released properly.
         */
        public void close()
        {
            if (mBluetoothGatt == null)
            {
                return;
            }
            mBluetoothGatt.Close();
            mBluetoothGatt = null;
            //Toast.makeText(getApplicationContext(), "Close GATT connection.", Toast.LENGTH_SHORT).show();
        }

        /**
         * Request a read on a given {@code BluetoothGattCharacteristic}. The read result is reported
         * asynchronously through the {@code BluetoothGattCallback#onCharacteristicRead(android.bluetooth.BluetoothGatt, android.bluetooth.BluetoothGattCharacteristic, int)}
         * callback.
         *
         * @param characteristic The characteristic to read from.
         */
        public void readCharacteristic(BluetoothGattCharacteristic characteristic)
        {
            if (mBluetoothAdapter == null || mBluetoothGatt == null)
            {
                Log.Warn(TAG, "BluetoothAdapter not initialized");
                return;
            }
            mBluetoothGatt.ReadCharacteristic(characteristic);
            Log.Debug(TAG, "Read Characteristic");
        }

        /**
         * Enables or disables notification on a give characteristic.
         *
         * @param characteristic Characteristic to act on.
         * @param enabled If true, enable notification.  False otherwise.
         */
        public void setCharacteristicNotification(BluetoothGattCharacteristic characteristic,
                                                  bool enabled)
        {
            if (mBluetoothAdapter == null || mBluetoothGatt == null)
            {
                Log.Warn(TAG, "BluetoothAdapter not initialized");
                return;
            }

            if (characteristic.Uuid.Equals(UUID.FromString(SampleGattAttributes.PRESSURE_NOTIFICATION_HANDLE)))
            {
                mBluetoothGatt.SetCharacteristicNotification(characteristic, enabled);


                // This is specific to Heart Rate Measurement.


                Log.Debug(TAG, "Setting notification: Pressure Characteristic detected ");

                BluetoothGattDescriptor descriptor = characteristic.GetDescriptor(UUID.FromString("00002902-0000-1000-8000-00805F9B34FB"));
                if (descriptor != null)
                {
                    Log.Debug(TAG, "Setting notification: Pressure Descriptor Found");
                    descriptor.SetValue(BluetoothGattDescriptor.EnableNotificationValue.ToArray<byte>());
                    mBluetoothGatt.WriteDescriptor(descriptor);
                    Log.Debug(TAG, "Setting notification: Write Pressure Descriptor");

                } else { Log.Warn(TAG, "NOTIFICATION SET UP IGNORED"); }

            }

        }

        /**
         * Retrieves a list of supported GATT services on the connected device. This should be
         * invoked only after {@code BluetoothGatt#discoverServices()} completes successfully.
         *
         * @return A {@code List} of supported services.
         */
        public IList<BluetoothGattService> getSupportedGattServices()
        {
            if (mBluetoothGatt == null) return null;

            return mBluetoothGatt.Services;
        }
    }
}