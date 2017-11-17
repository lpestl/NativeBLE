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
    //[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SensorDataPage : ContentPage
    {
        private ILogger logger = DependencyService.Get<ILogger>();
        private DeviceViewModel currentDeviceVM;

        public SensorDataPage(int indexDeviceVM, DeviceViewModel currentDeviceVM)
        {
            logger.TAG = "SensorDataPage";

            logger.LogInfo(String.Format("{0} - {1}: {2}", currentDeviceVM.Name, currentDeviceVM.Address, indexDeviceVM));
            //currentDeviceVM = DependencyService.Get<IDeviceList>().GetViewModel(indexDeviceVM);
            this.BindingContext = currentDeviceVM;

            InitializeComponent();
            //Title = currentDeviceVM.Name;
        }

        private void ConnectButton_Clicked(object sender, EventArgs e)
        {

        }

        private void ToolbarItem_Activated(object sender, EventArgs e)
        {

        }

        private void OnResultButton_Clicked(object sender, EventArgs e)
        {

        }
    }
}