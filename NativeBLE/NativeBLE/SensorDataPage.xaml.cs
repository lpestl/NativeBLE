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
    }
}