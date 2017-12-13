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
using Android.Bluetooth;
using NativeBLE.Droid.Native;
using System.Threading;
using System.Diagnostics;

namespace NativeBLE.Droid.Backend
{
    class ServiceBluetothGattCallback : BluetoothGattCallback
    {
        private SensorService sensorService = null;
        private Logger logger = new Logger();

        public void SetService(SensorService service)
        {
            sensorService = service;
        }

        public override void OnConnectionStateChange(BluetoothGatt gatt, [GeneratedEnum] GattStatus status, [GeneratedEnum] ProfileState newState)
        {
            String intentAction;
            logger.TAG = "ServiceBluetothGattCallback";
            
            if (newState == ProfileState.Connected)
            {
                intentAction = SensorService.ACTION_GATT_CONNECTED;
                sensorService.connectionState = ProfileState.Connected;
                if (SensorDataPage.Instance != null)
                {
                    SensorDataPage.Instance.connectionStopwatch.Stop();
                    SensorDataPage.Instance.SensorView.ConnectionTimeSpan = SensorDataPage.Instance.connectionStopwatch.Elapsed;
                    SensorDataPage.Instance.connectionStopwatch.Reset();
                    SensorDataPage.Instance.firstDataStopwatch.Reset();
                    SensorDataPage.Instance.firstDataStopwatch.Start();
                    SensorDataPage.Instance.SensorView.ConnectionState = "Connected";
                    SensorDataPage.Instance.SensorView.Connected = true;
                }
                bool rssiStatus = gatt.ReadRemoteRssi();
                sensorService.BroadcastUpdate(intentAction);

                logger.TraceInformation("Connected to GATT server.");
                // Attempts to discover services after successful connection.
                logger.TraceInformation("Attempting to start service discovery:" +
                        gatt.DiscoverServices());
            }
            else if (newState == ProfileState.Disconnected)
            {
                intentAction = SensorService.ACTION_GATT_DISCONNECTED;
                sensorService.connectionState = ProfileState.Disconnected;

                logger.TraceInformation("Disconnected from GATT server.");
                sensorService.BroadcastUpdate(intentAction);
                sensorService.Close();

                if (SensorDataPage.Instance != null)
                {
                    SensorDataPage.Instance.disconnectionStopwatch.Stop();
                    SensorDataPage.Instance.SensorView.DisconnectionTimeSpan = SensorDataPage.Instance.disconnectionStopwatch.Elapsed;
                    SensorDataPage.Instance.disconnectionStopwatch.Reset();
                    SensorDataPage.Instance.firstData = false;
                    SensorDataPage.Instance.SensorView.ConnectionState = "Disconnected";
                    SensorDataPage.Instance.SensorView.Connected = false;
                }
                sensorService.HideProgressDialog();
            }
        }

        public override void OnServicesDiscovered(BluetoothGatt gatt, [GeneratedEnum] GattStatus status)
        {
            if (status == BluetoothGatt.GattSuccess)
            {
                sensorService.BroadcastUpdate(SensorService.ACTION_GATT_SERVICES_DISCOVERED);

                sensorService.HideProgressDialog();

                if (!SensorDataPage.Instance.firstData)
                {
                    SensorDataPage.Instance.firstDataStopwatch.Stop();
                    SensorDataPage.Instance.SensorView.FirstDataTimeSpan = SensorDataPage.Instance.firstDataStopwatch.Elapsed;
                    SensorDataPage.Instance.firstDataStopwatch.Reset();
                    SensorDataPage.Instance.firstData = true;
                }
            }
            else
            {
                logger.TraceWarning("onServicesDiscovered received: " + status);
            }
        }

        public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status)
        {
            logger.TraceInformation("Event: Characteristic Read");
            
            if (SampleGattAttributes.PRESSURE_NOTIFICATION_HANDLE.Equals(characteristic.Uuid.ToString()))
            {
                if (status == BluetoothGatt.GattSuccess)
                {
                    sensorService.BroadcastUpdate(SensorService.ACTION_DATA_AVAILABLE, characteristic);

                }
                logger.TraceInformation("Broadcast Pressure Characteristic");
                logger.TraceInformation(characteristic.Uuid.ToString());
            }            
        }

        public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
        {
            logger.TraceInformation("Event: Characteristic change");
            
            if (SampleGattAttributes.PRESSURE_NOTIFICATION_HANDLE.Equals(characteristic.Uuid.ToString()))
            {
                sensorService.BroadcastUpdate(SensorService.ACTION_DATA_AVAILABLE, characteristic);
                logger.TraceInformation("Broadcast Pressure Characteristic");
                logger.TraceInformation(characteristic.Uuid.ToString());
                bool rssiStatus = gatt.ReadRemoteRssi();
            }
        }

        public override void OnReadRemoteRssi(BluetoothGatt gatt, int rssi, [GeneratedEnum] GattStatus status)
        {
            if (status == BluetoothGatt.GattSuccess)
            {
                logger.TraceInformation(String.Format("BluetoothGatt ReadRssi {0}", rssi));
                SensorService.iRssi = rssi;

                sensorService.BroadcastUpdate(SensorService.RSSI_DATA_AVAILABLE, rssi);
            }
        }
    }
}