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
using NativeBLE.Core;
using System.Collections.ObjectModel;

namespace NativeBLE.Droid.Native
{
    class NativeDeviceList
    {
        private List<BluetoothDevice> deviceList = new List<BluetoothDevice>();
        public ObservableCollection<DeviceViewModel> DeviceViewModelList { get; set; }

        public bool IsEmpty()
        {
            return deviceList.Count == 0;
        }

        public void Clear()
        {
            deviceList.Clear();
            DeviceViewModelList.Clear();
        }

        public void Add(BluetoothDevice device)
        {
            deviceList.Add(device);
            DeviceViewModelList.Add(new DeviceViewModel(device.Name, device.Address));
        }

        public BluetoothDevice Get(int index)
        {
            return deviceList[index];
        }

        public DeviceViewModel GetViewModel(int index)
        {
            return DeviceViewModelList[index];
        }

        public bool Contains(BluetoothDevice device)
        {
            return deviceList.Contains(device);
        }

        public bool Contains(DeviceViewModel value)
        {
            var contais = false;
            foreach (var device in DeviceViewModelList)
            {
                if (device.Address.Equals(value.Address))
                {
                    contais = true;
                    break;
                }
            }
            return contais;
        }
    }
}