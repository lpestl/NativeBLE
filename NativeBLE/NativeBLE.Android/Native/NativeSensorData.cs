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
using Android.Bluetooth;
using Android.Util;
using System.Drawing;
using Java.Util;
using static Android.Views.View;

[assembly: Xamarin.Forms.Dependency(typeof(NativeBLE.Droid.Native.NativeSensorData))]
namespace NativeBLE.Droid.Native
{
    class NativeSensorData : ISensorData
    {
        private static String TAG = "NativeSensorData";


        private static NativeSensorData instance;
        
        public static NativeSensorData Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new NativeSensorData();
                }
                return instance;
            }
        }

        public static NativeSensorData GetInstance()
        {
            return instance;
        }

        private SensorViewModel sensorViewModel;

        private BluetoothLeService mBluetoothLeService = BluetoothLeService.Instance;
        private BluetoothGattCharacteristic mPressureCharacteristic;
        private BluetoothGattCharacteristic mBatteryCharacteristic;
        private BluetoothGattCharacteristic mFirmwareVersionCharacteristic;
        private BluetoothGattCharacteristic mBatchVersionCharacteristic;
        private AlertDialog alertDialog;
        private AlertDialog alertDialog2;
        private static int MinLimit = 800;
        private static int MaxLimit = 3500;
        private Handler myhandler;

        private List<List<BluetoothGattCharacteristic>> mGattCharacteristics =
                new List<List<BluetoothGattCharacteristic>>();
        //private bool mConnected = false;
        private BluetoothGatt mBluetoothGatt;
        private LinkedList<String> lMovingAverage = new LinkedList<String>();
        private LinkedList<String> lstDynamicMinA = new LinkedList<String>();
        private LinkedList<String> lstlDynamicMaxA = new LinkedList<String>();

        private BluetoothGattCharacteristic mNotifyCharacteristic;

        private String LIST_NAME = "NAME";
        private String LIST_UUID = "UUID";
        private int MOVING_AVERAGE_SIZE = 4;
        private int DYNAMIC_AVERAGE_SIZE = 75;
        private double SENSOR_INTERVAL = 0.15;
        private double dCalibrationTime = 0;
        private long lDynamicMinResultA = 0;
        private long lDynamicMaxResultA = 0;
        private long result = 0;
        private long average = 0;
        private static long[] measurement = new long[3];
        private static long[,] average_measurement = new long[2,2];
        private long lMin = 0;
        private long lMax = 0;
        private long lDynamicMinA = 0;
        private long lDynamicMaxA = 0;
        private long lPercentageWithOffset;
        private long dOffset = 5;
        private int iDynamicMinIterator = 0;
        private int iDynamicMaxIterator = 0;
        private static bool sensorSelector;






        private bool bRecordMax = false;
        private bool bRecordMin = false;
        private bool bRecordDynamicMax;
        private bool bRecordDynamicMin;


        public void SetSensorViewModel(SensorViewModel value)
        {
            sensorViewModel = value;
        }

        public NativeSensorData() {
            instance = this;
        }

        private ConnectionCountDownTimer mConnectionCountDownTimer = new ConnectionCountDownTimer(4000, 1000);
        private DisconnectionCountDownTimer mDisconnectionCountDownTimer = new DisconnectionCountDownTimer(4000,1000);
        private NativeServiceConnection mServiceConnection = new NativeServiceConnection();

        public class ConnectionCountDownTimer : CountDownTimer
        {
            int i = 0;
            public ConnectionCountDownTimer(long millisInFuture, long countDownInterval) : base(millisInFuture, countDownInterval) {}

            public override void OnFinish()
            {
                if (!NativeSensorData.Instance.sensorViewModel.Connected)
                {
                    NativeSensorData.Instance.sensorViewModel.DebugString = "Reconnection Attempt";
                    Log.Debug(TAG, "Reconnection Attempt");
                    NativeSensorData.Instance.mBluetoothLeService.connect(NativeSensorData.Instance.sensorViewModel.Address);
                }
            }

            public override void OnTick(long millisUntilFinished)
            {
                Log.Debug(TAG, "ConnectionWatch tick");
                i++;
                if (i == 3)
                {
                    if (!NativeSensorData.Instance.sensorViewModel.Connected)
                    {
                        NativeSensorData.Instance.mBluetoothLeService.disconnect();
                        Log.Debug(TAG, "Disconnect before reconnection attempt");
                    }
                }
            }
        }

        public class DisconnectionCountDownTimer : CountDownTimer
        {
            int i = 0;
            public DisconnectionCountDownTimer(long millisInFuture, long countDownInterval) : base(millisInFuture, countDownInterval) {}

            public override void OnFinish()
            {
                //throw new NotImplementedException();
            }

            public override void OnTick(long millisUntilFinished)
            {
                Log.Debug(TAG, "DisconnectionWatch tick");
                if (!NativeSensorData.Instance.sensorViewModel.bDisconnectionWatchFlag)
                {
                    NativeSensorData.Instance.mBluetoothLeService.disconnect();
                    Log.Debug(TAG, "On tick disconnection attempt");
                }
                i++;
                if (i == 2)
                {
                    if (!NativeSensorData.Instance.sensorViewModel.bDisconnectionWatchFlag)
                    {
                        NativeSensorData.Instance.mBluetoothLeService.disconnect();
                        //mBluetoothLeService.close();
                        Log.Debug(TAG, "Second disconnection attempt");
                    }
                }
            }
        }

        private void StartConnectionWatch() {
            mConnectionCountDownTimer.Start();
            Log.Debug(TAG, "ConnectionWatch counter started");
        }

        private void StartDisconnectionWatch() {
            mDisconnectionCountDownTimer.Start();
            Log.Debug(TAG, "DisonnectionWatch counter started");
        }

        public class NativeServiceConnection : Java.Lang.Object, IServiceConnection
        {
            public void OnServiceConnected(ComponentName name, IBinder service)
            {
                NativeSensorData.Instance.mBluetoothLeService = ((BluetoothLeService.LocalBinder)service).getService();
                if (!NativeSensorData.Instance.mBluetoothLeService.initialize())
                {
                    Log.Error(TAG, "Unable to initialize Bluetooth");
                    //finish();
                }
                // Automatically connects to the device upon successful start-up initialization.
                NativeSensorData.Instance.mBluetoothLeService.connect(NativeSensorData.Instance.sensorViewModel.Address);
                NativeSensorData.Instance.StartConnectionWatch();
                Log.Debug(TAG, "ConnectionWatch started");

                //Log.d(TAG, "System paused for 2s");
                //SystemClock.sleep(2000);
                //launch a timer here
            }

            public void OnServiceDisconnected(ComponentName name)
            {
                NativeSensorData.Instance.mBluetoothLeService = null;
            }
        }

        // Handles various events fired by the Service.
        // ACTION_GATT_CONNECTED: connected to a GATT server.
        // ACTION_GATT_DISCONNECTED: disconnected from a GATT server.
        // ACTION_GATT_SERVICES_DISCOVERED: discovered GATT services.
        // ACTION_DATA_AVAILABLE: received data from the device.  This can be a result of read
        //                       or notification operations.
        private NativeBroadcastReceiver mGattUpdateReceiver = new NativeBroadcastReceiver();

        [BroadcastReceiver(Enabled = true)]
        public class NativeBroadcastReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {
                String action = intent.Action;
                if (BluetoothLeService.ACTION_GATT_CONNECTED.Equals(action))
                {
                    Log.Debug(TAG, "Connection Event: Device Connected");
                    NativeSensorData.Instance.sensorViewModel.DebugString = "Connection Event: Device Connected";
                    NativeSensorData.Instance.sensorViewModel.Connected = true;
                    NativeSensorData.Instance.sensorViewModel.EnableStart = true;
                    // mStartButton.setEnabled(TRUE);
                    NativeSensorData.Instance.updateConnectionState("Connecting...");
                    //invalidateOptionsMenu();
                }
                else if (BluetoothLeService.ACTION_GATT_DISCONNECTED.Equals(action))
                {
                    NativeSensorData.Instance.sensorViewModel.Connected = false;
                    if (NativeSensorData.Instance.sensorViewModel.bDisconnectionWatchFlag == false)
                    {
                        if (NativeSensorData.Instance.mBluetoothGatt != null) NativeSensorData.Instance.mBluetoothGatt.Close();
                        else Log.Debug(TAG, "GattNull at disconnetion");
                        if (NativeSensorData.Instance.mBluetoothLeService != null) NativeSensorData.Instance.mBluetoothLeService.close();
                        else Log.Debug(TAG, "BluetoothLeService null at disconnetion");
                    }
                    NativeSensorData.Instance.sensorViewModel.EnableStart = false;
                    //mStartButton.setEnabled(FALSE);
                    NativeSensorData.Instance.sensorViewModel.bDisconnectionWatchFlag = true;
                    Log.Debug(TAG, "Disconnection Event: Device Disconnected");
                    NativeSensorData.Instance.sensorViewModel.DebugString = "Connection Event: Device Disconnected";
                    NativeSensorData.Instance.sensorViewModel.ColorStart = Xamarin.Forms.Color.Black;
                    NativeSensorData.Instance.updateConnectionState("Disconnected");
                    //invalidateOptionsMenu();
                    clearUI();
                }
                else if (BluetoothLeService.ACTION_GATT_SERVICES_DISCOVERED.Equals(action))
                {

                    Log.Debug(TAG, "Connection Event: Services Discovered");
                    NativeSensorData.Instance.sensorViewModel.DebugString = "Connection Event: Services Discovered";
                    // Show all the supported services and characteristics on the user interface.
                    //displayGattServices(mBluetoothLeService.getSupportedGattServices());
                    Log.Debug(TAG, "Start Multiple Characteristics read");
                    NativeSensorData.Instance.sensorViewModel.DebugString = "Start Multiple Characteristics read";
                    //CallMultipleCharacteristics(mBluetoothLeService.getSupportedGattServices());


                    NativeSensorData.Instance.findPressureService(NativeSensorData.Instance.mBluetoothLeService.getSupportedGattServices());


                }
                else if (BluetoothLeService.ACTION_DATA_AVAILABLE.Equals(action))
                {
                    Log.Debug(TAG, "Data available");
                    NativeSensorData.Instance.updateConnectionState("Connected");
                    NativeSensorData.Instance.displayData(intent.GetStringExtra(BluetoothLeService.EXTRA_DATA), intent.GetStringExtra(BluetoothLeService.EXTRA_SENSOR_A), intent.GetStringExtra(BluetoothLeService.EXTRA_SENSOR_B));
                    NativeSensorData.Instance.displayBatteryData(intent.GetStringExtra(BluetoothLeService.EXTRA_BATTERY_DATA));
                    NativeSensorData.Instance.displayFirmwareVersionData(intent.GetStringExtra(BluetoothLeService.EXTRA_FIRMWARE_VERSION_DATA));
                    NativeSensorData.Instance.displayBatchVersionData(intent.GetStringExtra(BluetoothLeService.EXTRA_BATCH_VERSION_DATA));
                    //displaySensorA(intent.getStringExtra(BluetoothLeService.EXTRA_SENSOR_A));
                    NativeSensorData.Instance.CalculateMovingAverage(intent.GetStringExtra(BluetoothLeService.EXTRA_SENSOR_A), intent.GetStringExtra(BluetoothLeService.EXTRA_SENSOR_B));
                    //DynamicCalibration(intent.getStringExtra(BluetoothLeService.EXTRA_SENSOR_A));
                    //CalculatePercentage(intent.getStringExtra(BluetoothLeService.EXTRA_SENSOR_A));
                    NativeSensorData.Instance.sensorViewModel.TextStart = "START";
                    //mStartButton.setBackgroundColor(0xC000FF00);
                    NativeSensorData.Instance.sensorViewModel.ColorStart = Xamarin.Forms.Color.Green;

                    if (NativeSensorData.Instance.lMovingAverage.Count == NativeSensorData.Instance.MOVING_AVERAGE_SIZE - 1)
                    {
                        Log.Debug(TAG, "Stack is full");
                        if (NativeSensorData.Instance.bRecordMax == true)
                        {
                            //Log.Debug(TAG, "Recording max true");
                            NativeSensorData.Instance.lMax = NativeSensorData.Instance.average;
                            // mMaxField.setText(Long.toString(lMax));
                            NativeSensorData.Instance.bRecordMax = false;
                            Log.Debug(TAG, "Recording max set to false");
                            NativeSensorData.Instance.bRecordMin = false;
                        }
                        if (NativeSensorData.Instance.bRecordMin == true)
                        {
                            NativeSensorData.Instance.lMin = NativeSensorData.Instance.average;
                            //  mMinField.setText(Long.toString(lMin));
                            NativeSensorData.Instance.bRecordMin = false;
                            Log.Debug(TAG, "Recording min set to false");
                            NativeSensorData.Instance.bRecordMax = false;
                        }
                    }

                    //Quand le bouton "Enregistrer max dynamique" est pressé
                    if (NativeSensorData.Instance.bRecordDynamicMax == true)
                    {
                        for (int i = 0; i < NativeSensorData.Instance.lMovingAverage.Count; i++)
                        {
                            Log.Debug(TAG, (NativeSensorData.Instance.lMovingAverage.ElementAt(i)));
                        }


                    }
                }
                else if (BluetoothLeService.RSSI_DATA_AVAILABLE.Equals(action))
                {
                    // Log.Debug(TAG, "RSSI Data available");
                    String srssi;
                    srssi = intent.GetStringExtra(BluetoothLeService.EXTRA_RSSI_DATA);
                    NativeSensorData.Instance.displayRssiData(srssi);
                    //Log.Debug(TAG, srssi);

                }
            }

            private void clearUI()
            {
                NativeSensorData.Instance.sensorViewModel.Data = "No data";
                NativeSensorData.Instance.sensorViewModel.SensorA = "No data";
                NativeSensorData.Instance.sensorViewModel.SensorB = "No data";
                NativeSensorData.Instance.sensorViewModel.BataryLevel = "No data";
            }

        }

        public SensorViewModel GetSensorViewModel()
        {
            return sensorViewModel;
        }

        private static int itrTest = 0;
        
        public void OnClickStartButton()
        {
            alertDialog = new AlertDialog.Builder(mThisActivity).Create();
            alertDialog.SetTitle("Sensor A: Apply the pressure");
            alertDialog2 = new AlertDialog.Builder(mThisActivity).Create();
            alertDialog2.SetTitle("Maintain the pressure");
            sensorSelector = false;

            alertDialog.SetOnShowListener(new SensorAdialogOnShowListener());
            alertDialog.SetButton((int)DialogButtonType.Positive, "OK", new NullDialogOnClickListener());

            alertDialog.Show();
        }

        public class SensorAdialogOnShowListener : Java.Lang.Object, IDialogInterfaceOnShowListener
        {
            public void OnShow(IDialogInterface dialog)
            {
                Button positiveButton = NativeSensorData.Instance.alertDialog.GetButton((int)DialogButtonType.Positive);
                positiveButton.SetOnClickListener(new SensorPositiveClickListener());
            }
        }

        public class NullDialogOnClickListener : Java.Lang.Object, IDialogInterfaceOnClickListener
        {
            public void OnClick(IDialogInterface dialog, int which)
            {
            }
        }

        public class SensorPositiveClickListener : Java.Lang.Object, IOnClickListener
        {
            public void OnClick(View v)
            {
                NativeSensorData.Instance.alertDialog2.SetMessage("00:4");
                NativeSensorData.Instance.alertDialog2.Show();
                NativeSensorData.Instance.lMovingAverage.Clear();
                NativeSensorData.Instance.result = 0;
                Log.Warn(TAG, "Moving Average Cleared");

                var countTimer = new SensorAcountDownTimer(4000, 1000);
                countTimer.Start();

                Log.Warn(TAG, itrTest.ToString());
            }
        }

        public class SensorAcountDownTimer : CountDownTimer
        {
            public SensorAcountDownTimer(long millisInFuture, long countDownInterval) : base(millisInFuture, countDownInterval)
            {
            }

            public override void OnFinish()
            {
                itrTest++;
                switch (itrTest)
                {
                    case 0:
                        NativeSensorData.Instance.alertDialog.SetTitle("SENSOR A: Apply the pressure");
                        NativeSensorData.Instance.alertDialog2.SetTitle("Keep on applying pressure");
                        NativeSensorData.Instance.alertDialog2.Dismiss();
                        break;
                    case 1:
                        NativeSensorData.Instance.alertDialog.SetTitle("SENSOR A: Release the pressure");
                        NativeSensorData.Instance.alertDialog2.SetTitle("Keep on releasing pressure");
                        Arrays.Sort(measurement);
                        // Log.w(TAG,"Max of sensor A applying pressure");
                        //Log.w(TAG,Long.toString(measurement[2]));
                        average_measurement[0,0] = measurement[2];
                        NativeSensorData.Instance.sensorViewModel.SensorA_TopResult = average_measurement[0, 0].ToString();
                        NativeSensorData.Instance.alertDialog2.Dismiss();
                        break;
                    case 2:
                        NativeSensorData.Instance.alertDialog.SetTitle("SENSOR B: Apply the pressure");
                        sensorSelector = true;
                        NativeSensorData.Instance.alertDialog2.SetTitle("Keep on applying pressure");
                        Arrays.Sort(measurement);
                        // Log.w(TAG,"Max of sensor A release");
                        //Log.w(TAG,Long.toString(measurement[2]));
                        average_measurement[1,0] = measurement[2];
                        NativeSensorData.Instance.sensorViewModel.SensorA_BottomResult = average_measurement[1,0].ToString();
                        NativeSensorData.Instance.alertDialog2.Dismiss();
                        break;
                    case 3:
                        NativeSensorData.Instance.alertDialog.SetTitle("SENSOR B: Release the pressure");
                        NativeSensorData.Instance.alertDialog2.SetTitle("Keep on releasing pressure");
                        Arrays.Sort(measurement);
                        //Log.w(TAG,"Max of sensor B applying pressure");
                        // Log.w(TAG,Long.toString(measurement[2]));
                        average_measurement[0, 1] = measurement[2];
                        NativeSensorData.Instance.sensorViewModel.SensorB_TopResult = average_measurement[0,1].ToString();
                        NativeSensorData.Instance.alertDialog2.Dismiss();

                        break;
                    case 4:
                        itrTest = 0;
                        sensorSelector = false;
                        Arrays.Sort(measurement);
                        average_measurement[1,1] = measurement[2];
                        NativeSensorData.Instance.sensorViewModel.SensorB_BottomResult = average_measurement[1,1].ToString();
                        //Log.w(TAG,"Max of sensor B release");
                        //Log.w(TAG,Long.toString(measurement[2]));
                        if (NativeSensorData.Instance.TestValidation() == true)
                        {
                            NativeSensorData.Instance.sensorViewModel.VisibleResult = true;
                            NativeSensorData.Instance.sensorViewModel.TextResult = "TEST PASSED!!!";
                            NativeSensorData.Instance.sensorViewModel.ColorResult = Xamarin.Forms.Color.Green;
                        }
                        else
                        {
                            NativeSensorData.Instance.sensorViewModel.VisibleResult = true;
                            NativeSensorData.Instance.sensorViewModel.TextResult = "TEST FAILED";
                            NativeSensorData.Instance.sensorViewModel.ColorResult = Xamarin.Forms.Color.Red;
                        }


                        NativeSensorData.Instance.alertDialog2.Dismiss();
                        NativeSensorData.Instance.alertDialog.Dismiss();
                        break;
                }


                Log.Warn(TAG, itrTest.ToString());

            }

            public override void OnTick(long millisUntilFinished)
            {
                NativeSensorData.Instance.lMovingAverage.Clear();
                long itrClock;
                itrClock = millisUntilFinished / 1000;
                int i = (int)itrClock;
                Log.Warn(TAG, "i =");
                Log.Warn(TAG, i.ToString());
                measurement[i - 1] = NativeSensorData.Instance.average;
                NativeSensorData.Instance.result = 0;
                Log.Warn(TAG, NativeSensorData.Instance.average.ToString());

                NativeSensorData.Instance.alertDialog2.SetMessage("00:" + (millisUntilFinished / 1000));
            }
        }

        private Activity mThisActivity = Xamarin.Forms.Forms.Context as Activity;
        private void updateConnectionState(string v)
        {
            mThisActivity.RunOnUiThread(() =>
            {
                NativeSensorData.Instance.sensorViewModel.ConnectionState = v;
                NativeSensorData.Instance.sensorViewModel.TextStart = v;
            });
        }

        private void findPressureService(IList<BluetoothGattService> gattServices)
        {

            if (gattServices == null) return;
            String uuid = null;
            long lData;
            String sData;
            
            foreach (BluetoothGattService gattService in gattServices)
            {
                updateConnectionState("Reading services");
                Log.Debug(TAG, "Loop through all services");
                NativeSensorData.Instance.sensorViewModel.DebugString = "Loop through all services";
                // IF UUID = HEART RATE MONITOR THEN POPULATE THE ARRAY
                // AR 28/04/2017
                if (BluetoothLeService.UUID_PRESSURE_SERVICE.Equals(gattService.Uuid))
                {

                    Log.Debug(TAG, "Found Pressure Service");
                    NativeSensorData.Instance.sensorViewModel.DebugString = "Found Pressure Service";
                    //uuid = gattService.getUuid().toString();
                    if (gattService.GetCharacteristic(UUID.FromString(SampleGattAttributes.PRESSURE_NOTIFICATION_HANDLE)) == null)
                    {
                        Log.Debug(TAG, "Pressure Char Null");
                        return;
                    }
                    else
                    {
                        mPressureCharacteristic = gattService.GetCharacteristic(UUID.FromString(SampleGattAttributes.PRESSURE_NOTIFICATION_HANDLE));
                        // bPressureData = mPressureCharacteristic.getValue();
                        mNotifyCharacteristic = mPressureCharacteristic;
                        mBluetoothLeService.setCharacteristicNotification(mPressureCharacteristic, true);
                        Log.Debug(TAG, "Notification set up for Pressure");
                        NativeSensorData.Instance.sensorViewModel.DebugString = "Notification set up for Pressure";

                    }
                    return;
                }                
            }
        }

        private bool TestValidation()
        {
            int success = 0;
            if (average_measurement[0,0] > MaxLimit)
            {
                success++;
            }
            else
            {
                Log.Debug(TAG, "[TEST] Sensor A too weak");
                //Toast.makeText(this, "Sensor A too weak", Toast.LENGTH_LONG).show();
            }

            if (average_measurement[1,0] < MinLimit)
            {
                success++;
            }
            else
            {
                Log.Debug(TAG, "[TEST] Failure to relase sensor A");
                //Toast.makeText(this, "Failure to relase sensor A", Toast.LENGTH_LONG).show();
            }

            if (average_measurement[0,1] > MaxLimit)
            {
                success++;
            }
            else
            {
                Log.Debug(TAG, "[TEST] Sensor B too weak");
                //Toast.makeText(this, "Sensor B too weak", Toast.LENGTH_LONG).show();
            }
            if (average_measurement[1, 1] < MinLimit)
            {
                success++;
            }
            else
            {
                Log.Debug(TAG, "[TEST] Failure to properly release sensor B");
                //Toast.makeText(this, "Failure to properly release sensor B", Toast.LENGTH_LONG).show();
            }


            if (success == 4)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        private void displayRssiData(String data)
        {
            if (data != null)
            {
                //mDataField.setText(data);

                NativeSensorData.Instance.sensorViewModel.RSSI = data;
                Log.Debug(TAG, "Displaying RSSI");
                // mDebug.setText("Displaying Battery Data");
            }
        }

        private void displayData(String data, String sensor_a, String sensor_b)
        {
            if (data != null)
            {
                NativeSensorData.Instance.sensorViewModel.Data = data;
                NativeSensorData.Instance.sensorViewModel.SensorA = sensor_a;
                NativeSensorData.Instance.sensorViewModel.SensorB = sensor_b;
                // mBatteryLevel.setText(data);
                Log.Debug(TAG, "Displaying Pressure Data");
                NativeSensorData.Instance.sensorViewModel.DebugString = "Displaying Pressure Data";
            }
        }

        private void displayBatteryData(String data)
        {
            if (data != null)
            {
                //mDataField.setText(data);
                NativeSensorData.Instance.sensorViewModel.BataryLevel = data;
                Log.Debug(TAG, "Displaying Battery Data");
                NativeSensorData.Instance.sensorViewModel.DebugString = "Displaying Battery Data";
            }
        }

        private void displayFirmwareVersionData(String data)
        {
            if (data != null)
            {
                //mDataField.setText(data);
                NativeSensorData.Instance.sensorViewModel.FirmwareVersion = data;
                Log.Debug(TAG, "Displaying Firmware Data");
                NativeSensorData.Instance.sensorViewModel.DebugString = "Displaying Firware Data";
            }
        }

        private void displayBatchVersionData(String data)
        {
            if (data != null)
            {
                //mDataField.setText(data);

                NativeSensorData.Instance.sensorViewModel.BatchVersion = data;
                Log.Debug(TAG, "Displaying Batch Version Data");
                NativeSensorData.Instance.sensorViewModel.DebugString = "Displaying Battery Data";
            }
        }

        private void CalculateMovingAverage(String dataA, String dataB)
        {
            String data;
            if (sensorSelector != true)
            {
                data = dataA;
            }
            else
            {
                data = dataB;
            }
            if (data != null)
            {

                if (lMovingAverage.Count < MOVING_AVERAGE_SIZE)
                {

                    // Log.d(TAG, "Hey, there is still some emplty space left in Moving Average list");
                    lMovingAverage.AddFirst(data);
                    //Log.d(TAG, "Value added with sucess");
                    result = result + long.Parse(data);
                    // average = result / lMovingAverage.size();
                    //Log.d(TAG, "The Average is" + average);
                }
                else
                {
                    //String St;

                    //Log.d(TAG, "Oh no, Moving Average List is full");
                    //result = result + Long.parseLong(data);
                    //St = lMovingAverage.getLast();
                    result = result + long.Parse(data);
                    result = result - long.Parse(lMovingAverage.Last.Value);
                    average = result / lMovingAverage.Count;

                    lMovingAverage.RemoveLast();
                    lMovingAverage.AddFirst(data);
                    //Log.d(TAG, "The Average is" + average);
                    //mAverageField.setText(Long.toString(average));

                }

            }

        }

        public void SetMinLimit(int value)
        {
            MinLimit = value;
        }

        private static IntentFilter makeGattUpdateIntentFilter()
        {
            IntentFilter intentFilter = new IntentFilter();
            intentFilter.AddAction(BluetoothLeService.ACTION_GATT_CONNECTED);
            intentFilter.AddAction(BluetoothLeService.ACTION_GATT_DISCONNECTED);
            intentFilter.AddAction(BluetoothLeService.ACTION_GATT_SERVICES_DISCOVERED);
            intentFilter.AddAction(BluetoothLeService.ACTION_DATA_AVAILABLE);
            intentFilter.AddAction(BluetoothLeService.RSSI_DATA_AVAILABLE);
            return intentFilter;
        }

        public void OnResume()
        {
            mThisActivity.ApplicationContext.RegisterReceiver(mGattUpdateReceiver, makeGattUpdateIntentFilter());
            if (mBluetoothLeService != null)
            {
                bool result = mBluetoothLeService.connect(NativeSensorData.Instance.sensorViewModel.Address);
                // Log.d(TAG, "Connect request result=" + result);

            }
        }

        public void OnPause()
        {
            if (mGattUpdateReceiver != null)
            {
                mThisActivity.ApplicationContext.UnregisterReceiver(mGattUpdateReceiver);
            }
        }

        public void OnDestroy()
        {
            mThisActivity.ApplicationContext.UnbindService(mServiceConnection);
            mBluetoothLeService = null;
        }
        
        public void OnClickResultButton()
        {
            //throw new NotImplementedException();
        }

        public void OnClickConnectButton()
        {
            if (!sensorViewModel.Connected)
            {
                StartConnectionWatch();
                Log.Debug(TAG, "Connection Watch started");
                mBluetoothLeService.connect(sensorViewModel.Address);
            } else
            {
                sensorViewModel.bDisconnectionWatchFlag = false;
                StartDisconnectionWatch();
                mBluetoothLeService.disconnect();
            }
        }

        public void Init(SensorViewModel value)
        {
            sensorViewModel = value;
            Intent gattServiceIntent = new Intent(mThisActivity.ApplicationContext, typeof(BluetoothLeService));
            mThisActivity.ApplicationContext.BindService(gattServiceIntent, mServiceConnection, Bind.AutoCreate);
            OnResume();
        }
    }
}