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
                if (SensorDataPage.Instance != null) SensorDataPage.Instance.SensorView.ConnectionState = "Connected";
                bool rssiStatus = sensorService.bluetoothGatt.ReadRemoteRssi();
                sensorService.BroadcastUpdate(intentAction);
                logger.TraceInformation("Connected to GATT server.");
                // Attempts to discover services after successful connection.
                logger.TraceInformation("Attempting to start service discovery:" +
                        sensorService.bluetoothGatt.DiscoverServices());
            }
            else if (newState == ProfileState.Disconnected)
            {
                intentAction = SensorService.ACTION_GATT_DISCONNECTED;
                sensorService.connectionState = ProfileState.Disconnected;
                if (SensorDataPage.Instance != null) SensorDataPage.Instance.SensorView.ConnectionState = "Disconnected";
                logger.TraceInformation("Disconnected from GATT server.");
                sensorService.BroadcastUpdate(intentAction);
            }
        }

        public override void OnServicesDiscovered(BluetoothGatt gatt, [GeneratedEnum] GattStatus status)
        {
            if (status == BluetoothGatt.GattSuccess)
            {
                sensorService.BroadcastUpdate(SensorService.ACTION_GATT_SERVICES_DISCOVERED);
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
                bool rssiStatus = sensorService.bluetoothGatt.ReadRemoteRssi();
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