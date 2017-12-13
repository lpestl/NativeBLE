using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using NativeBLE.Core;
using NativeBLE.Core.Forms;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;

namespace NativeBLE.Core.Forms
{
    public partial class MainPage : ContentPage
    {
        private ILogger logger;
        private MainPageViewModel mainPageViewModel = new MainPageViewModel();
        private IPermissionsCheck permissionsCheck;

        private IDeviceScanner deviceScanner;
        private IPluginDeviceScanner pluginDeviceScanner;

        private Device device;
        private IDevice idevice;

        private ConnectionAlgorithmViewModel connectionAlgorithmVM = new ConnectionAlgorithmViewModel();
        private ConnectionAlgorithm connectionAlgorithm = new ConnectionAlgorithm();
        
        public MainPage()
        {
            // Init logger
            logger = DependencyService.Get<ILogger>();
            logger.TAG = "MainPage";

            logger.TraceInformation(String.Format("Current algorithm: {0}", connectionAlgorithm.Algorithm.ToString()));

            // Init permissions checker
            permissionsCheck = DependencyService.Get<IPermissionsCheck>();

            // Init UI
            this.BindingContext = mainPageViewModel;

            InitializeComponent();
            Title = "Bluetooth LE Scanner";

            ToolbarItems.Add(new ToolbarItem("Settings", "settings.png", async () => { await Navigation.PushAsync(new ChoiceAlgorithm(connectionAlgorithmVM)); }));

            mainPageViewModel.StringDebug = "Used: Native; Mode: standart";
            connectionAlgorithmVM.AlgorithmSelected += ConnectionAlgorithmVM_AlgorithmSelected;

            // Bluetooth init
            Init();
        }

        public void Init()
        {
            // Clear last algorithm
            logger.TraceInformation("Clear last algorithm");
            // Hide pluginBLE mode picker
            ModeBlePicker.Items.Clear();
            ModeBlePicker.IsVisible = false;
            // Clear Native scanner
            deviceScanner = null;

            // Clear pluginBLE
            pluginDeviceScanner = null;

            // Get permissions
            GetPermissions();

            // Check picked algorithm and init it
            if (connectionAlgorithm.Algorithm == ConnectionAlgorithmType.Native)
            {
                logger.TraceInformation("Init Native");
                NativeDeviceScannerInit();
            }
            else if (connectionAlgorithm.Algorithm == ConnectionAlgorithmType.PluginBLE)
            {
                logger.TraceInformation("Init Plugin BLE");
                PluginBleInit();
            }
            else if (connectionAlgorithm.Algorithm == ConnectionAlgorithmType.CombineNativeAndCross)
            {
                logger.TraceInformation("Init combine Native /& Ble Cross");
                // CombineNativeAndBleCrossInit();
            }
        }

        private void GetPermissions()
        {
            if (permissionsCheck.CheckSupportBLE())
                logger.LogInfo("BLE is supported!");
            else
                logger.LogWarning("Bluetooth Low Energy is not supported.");

            if (permissionsCheck.CheckPermissions())
                logger.LogInfo("Android permission AccessCoarseLocation granted!");
            else
            {
                logger.LogWarning("The application does not have the necessary permission (AccessCoarseLocation).");
                // Only if SDK API VERSION >= 23
                permissionsCheck.GetRuntimePermissions();
            }
        }

        private void ConnectionAlgorithmVM_AlgorithmSelected(object sender, ConnectionAlgorithm args)
        {
            connectionAlgorithm = args;
            logger.TraceInformation(String.Format("Used: {0}; Mode: {1}", connectionAlgorithm.Algorithm.ToString(), "Standart"));
            mainPageViewModel.StringDebug = String.Format("Used: {0}; Mode: {1}", connectionAlgorithm.Algorithm.ToString(), "Standart");
            Init();
        }

        private void OnClick(object sender, EventArgs e)
        {
            logger.LogInfo("On Click");
            if (mainPageViewModel.Scanning)
            {
                if (connectionAlgorithm.Algorithm == ConnectionAlgorithmType.Native)
                {
                    logger.TraceInformation("Native stop scan");
                    deviceScanner.StopScan();
                }
                else if (connectionAlgorithm.Algorithm == ConnectionAlgorithmType.PluginBLE)
                {
                    logger.TraceInformation("Plugin BLE stop scan");
                    pluginDeviceScanner.StopScan();
                }
                else if (connectionAlgorithm.Algorithm == ConnectionAlgorithmType.CombineNativeAndCross)
                {
                    logger.TraceInformation("Combine Native /& Ble Cross stop scan");
                    // TODO: Stop combine alg
                }

                IsBusy = false;
            }
            else
            {
                if (connectionAlgorithm.Algorithm == ConnectionAlgorithmType.Native)
                {
                    logger.TraceInformation("Native start scan");
                    deviceScanner.ScanLeDevice();
                }
                else if (connectionAlgorithm.Algorithm == ConnectionAlgorithmType.PluginBLE)
                {
                    logger.TraceInformation("Plugin BLE Start Scan");
                    pluginDeviceScanner.ScanLeDevice(ModeBlePicker.SelectedIndex);
                }
                else if (connectionAlgorithm.Algorithm == ConnectionAlgorithmType.CombineNativeAndCross)
                {
                    logger.TraceInformation("Combine Native /& Ble Cross start scan");
                    // TODO: Start combine alg
                }
                IsBusy = true;
            }
        }

        public async void OnChoiceDevice(object sender, ItemTappedEventArgs e)
        {
            logger.TraceInformation("--------------------------------------------");
            logger.TraceInformation("----The beginning of the problem place.-----");

            if (e.Item is DeviceViewModel selectedDevice)
            {
                logger.TraceInformation($"OnTapped on e.Item: {selectedDevice.Name} - {selectedDevice.Address}");

                var index = mainPageViewModel.Devices.IndexOf(selectedDevice);
                logger.TraceInformation($"Finded index on ObservibleCollection : {index} - {mainPageViewModel.Devices[index].Address}");
                
                if (connectionAlgorithm.Algorithm == ConnectionAlgorithmType.Native)
                {
                    device = deviceScanner.GetDevice(index);
                    deviceScanner.StopScan();
                }
                else if (connectionAlgorithm.Algorithm == ConnectionAlgorithmType.PluginBLE)
                {
                    device = pluginDeviceScanner.GetDevice(index);
                    idevice = pluginDeviceScanner.GetIDevice(index);
                    pluginDeviceScanner.StopScan();
                }
                else if (connectionAlgorithm.Algorithm == ConnectionAlgorithmType.CombineNativeAndCross)
                {
                    // TODO: Get combine device
                }

                logger.TraceInformation($"Finded device on native devices list : {index} - {device.Address}");
                
                //await DisplayAlert($"Выбранно устройство {index}", $"{device.Name} - {device.Address}", "OK");
                await Navigation.PushAsync(new SensorDataPage(device, idevice, index, connectionAlgorithm));
            }
        }

        #region Native
        public void NativeDeviceScannerInit()
        {
            deviceScanner = DependencyService.Get<IDeviceScanner>();

            mainPageViewModel.Devices.Clear();

            deviceScanner.Init(mainPageViewModel);
            
            deviceScanner.GetBluetoothAdapter();
        }        
        #endregion

        #region PluginBLE
        private void PluginBleInit()
        {
            pluginDeviceScanner = DependencyService.Get<IPluginDeviceScanner>();

            mainPageViewModel.Devices.Clear();
            pluginDeviceScanner.Init(mainPageViewModel);

            pluginDeviceScanner.GetBluetoothAdapter();

            ModeBlePicker.IsVisible = true;
            ModeBlePicker.Items.Add(ScanMode.Balanced.ToString());
            ModeBlePicker.Items.Add(ScanMode.LowLatency.ToString());
            ModeBlePicker.Items.Add(ScanMode.LowPower.ToString());
            ModeBlePicker.Items.Add(ScanMode.Passive.ToString());
            ModeBlePicker.SelectedIndex = 0;
        }     
        #endregion
        
        ~MainPage()
        {
            connectionAlgorithmVM.AlgorithmSelected -= ConnectionAlgorithmVM_AlgorithmSelected;
        }
    }
}
