using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using NativeBLE.Core;
using NativeBLE.Core.Forms;

namespace NativeBLE.Core.Forms
{
    public partial class MainPage : ContentPage
    {
        private ILogger logger;
        private MainPageViewModel mainPageViewModel = new MainPageViewModel();
        private IDeviceScanner deviceScanner = DependencyService.Get<IDeviceScanner>();

        public MainPage()
        {
            logger = DependencyService.Get<ILogger>();
            logger.TAG = "MainPage";

            this.BindingContext = mainPageViewModel;

            InitializeComponent();
            Title = "Bluetooth LE Scanner";

            deviceScanner.pageViewModel = mainPageViewModel;

            if (deviceScanner.CheckSupportBLE())
                logger.LogInfo("BLE is supported!");
            else
                logger.LogWarning("Bluetooth Low Energy is not supported.");

            if (deviceScanner.CheckPermissions())
                logger.LogInfo("Android permission AccessCoarseLocation granted!");
            else
                logger.LogWarning("The application does not have the necessary permission (AccessCoarseLocation).");
                // Only if SDK API VERSION >= 23
                deviceScanner.GetRuntimePermissions();

            deviceScanner.GetBluetoothAdapter();
            //deviceScanner.ScanLeDevice();
        }

        private void OnClick(object sender, EventArgs e)
        {
            logger.LogInfo("On Click");
            if (mainPageViewModel.Scanning)
            {
                deviceScanner.StopScan();
                IsBusy = false;
            } else
            {
                deviceScanner.ScanLeDevice();
                IsBusy = true;
            }
        }

        //private void phonesList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        //{
        //    if (e.SelectedItem != null)
        //        selected.Text = e.SelectedItem.ToString();
        //}
    }
}
