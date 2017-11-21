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

        public SensorDataPage(int indexDeviceVM, DeviceViewModel currentDeviceVM)
        {
            logger.TAG = "SensorDataPage";

            logger.LogInfo(String.Format("{0} - {1}: {2}", currentDeviceVM.Name, currentDeviceVM.Address, indexDeviceVM));
            sensorViewModel = new SensorViewModel(currentDeviceVM, indexDeviceVM);
            nativeSensorData.Init(sensorViewModel);
            this.BindingContext = sensorViewModel;

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