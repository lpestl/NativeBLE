using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using NativeBLE.Core;
using NativeBLE.Core.Forms;

namespace NativeBLE.Core.Forms
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SensorDataPage : ContentPage
    {
        private ILogger logger = DependencyService.Get<ILogger>();
        private SensorViewModel sensorViewModel;
        private ISensorData nativeSensorData = DependencyService.Get<ISensorData>();

        public SensorDataPage(Device currentDevice)
        {
            logger.TAG = "SensorDataPage";

            logger.LogInfo(String.Format("{0} - {1}", currentDevice.Name, currentDevice.Address));
            sensorViewModel = new SensorViewModel(new DeviceViewModel(currentDevice.Name, currentDevice.Address));
            nativeSensorData.Init(sensorViewModel, currentDevice);
            this.BindingContext = sensorViewModel;

            logger.TraceInformation("----  The ending of the problem place. -----");
            logger.TraceInformation("--------------------------------------------");

            InitializeComponent();           
            
        }

        private void ConnectButton_Clicked(object sender, EventArgs e)
        {
            nativeSensorData.OnClickStartButton();
        }
        
        private void OnResultButton_Clicked(object sender, EventArgs e)
        {
            nativeSensorData.OnClickResultButton();
        }

        private void Switch_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (mSwitch.IsToggled)
            {
                nativeSensorData.SetMinLimit(800);
            } else
            {
                nativeSensorData.SetMinLimit(0);
            }
        }

        private void ToolbarItem_Clicked(object sender, EventArgs e)
        {
            nativeSensorData.OnClickConnectButton();
        }
    }
}