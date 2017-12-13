using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using NativeBLE.Core;
using NativeBLE.Core.Forms;
using Plugin.BLE.Abstractions.Contracts;

namespace NativeBLE.Core.Forms
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SensorDataPage : ContentPage
    {
        private ILogger logger = DependencyService.Get<ILogger>();
        private SensorViewModel sensorViewModel;
        private ConnectionAlgorithm connectionAlg;

        private ISensorData nativeSensorData;
        private IPluginSensorData pluginSensorData;

        public SensorDataPage(Device currentDevice, IDevice idevice, int indexDevice, ConnectionAlgorithm connectionAlgorithm)
        {
            logger.TAG = "SensorDataPage";

            logger.LogInfo(String.Format("{0} - {1}_{2}", currentDevice.Name, currentDevice.Address, indexDevice));
            sensorViewModel = new SensorViewModel(new DeviceViewModel(currentDevice.Name, currentDevice.Address));
            connectionAlg = connectionAlgorithm;

            Init(sensorViewModel, currentDevice, idevice, indexDevice);
            
            this.BindingContext = sensorViewModel;

            InitializeComponent();

            logger.TraceInformation("----  The ending of the problem place. -----");
            logger.TraceInformation("--------------------------------------------");            
        }

        private void Init(SensorViewModel sensorViewModel, Device currentDevice, IDevice idevice, int indexDevice)
        {
            nativeSensorData = null;
            pluginSensorData = null;

            if (connectionAlg.Algorithm == ConnectionAlgorithmType.Native)
            {
                nativeSensorData = DependencyService.Get<ISensorData>();
                nativeSensorData.Init(sensorViewModel, currentDevice);
            }
            else if (connectionAlg.Algorithm == ConnectionAlgorithmType.PluginBLE)
            {
                pluginSensorData = DependencyService.Get<IPluginSensorData>();
                pluginSensorData.Init(sensorViewModel, idevice);
            }
            else if (connectionAlg.Algorithm == ConnectionAlgorithmType.CombineNativeAndCross)
            {
                // TODO: Combine Init
            }
        }

        private void ConnectButton_Clicked(object sender, EventArgs e)
        {
            if (connectionAlg.Algorithm == ConnectionAlgorithmType.Native)
            {
                nativeSensorData.OnClickStartButton();
            }
            else if (connectionAlg.Algorithm == ConnectionAlgorithmType.PluginBLE)
            {
                pluginSensorData.OnClickStartButton();
            }
            else if (connectionAlg.Algorithm == ConnectionAlgorithmType.CombineNativeAndCross)
            {
                // TODO: Combine Init
            }
        }
        
        private void OnResultButton_Clicked(object sender, EventArgs e)
        {
            if (connectionAlg.Algorithm == ConnectionAlgorithmType.Native)
            {
                nativeSensorData.OnClickResultButton();
            }
            else if (connectionAlg.Algorithm == ConnectionAlgorithmType.PluginBLE)
            {
                pluginSensorData.OnClickResultButton();
            }
            else if (connectionAlg.Algorithm == ConnectionAlgorithmType.CombineNativeAndCross)
            {
                // TODO: Combine Init
            }
        }

        private void Switch_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (mSwitch.IsToggled)
            {
                if (nativeSensorData != null)
                {
                    nativeSensorData.SetMinLimit(800);
                }

            } else
            {
                if (nativeSensorData != null)
                {
                    nativeSensorData.SetMinLimit(0);
                }
            }
        }

        private void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            if (connectionAlg.Algorithm == ConnectionAlgorithmType.Native)
            {
                nativeSensorData.OnClickConnectButton();
            }
            else if (connectionAlg.Algorithm == ConnectionAlgorithmType.PluginBLE)
            {
                pluginSensorData.OnClickConnectButton();
            }
            else if (connectionAlg.Algorithm == ConnectionAlgorithmType.CombineNativeAndCross)
            {
                // TODO: Combine Init
            }
        }
    }
}