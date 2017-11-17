using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace NativeBLE.Core
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SensorDataPage : ContentPage
    {
        public SensorDataPage(DeviceViewModel currentDeviceVM)
        {
            InitializeComponent();
            Title = currentDeviceVM.Name;
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