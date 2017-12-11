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

        private IDeviceScanner deviceScanner;// = DependencyService.Get<IDeviceScanner>();

        private ConnectionAlgorithmViewModel connectionAlgorithmVM = new ConnectionAlgorithmViewModel();
        private ConnectionAlgorithm connectionAlgorithm = new ConnectionAlgorithm();

        private IBluetoothLE ble;
        private IAdapter adapter;

        public MainPage()
        {
            logger = DependencyService.Get<ILogger>();
            logger.TAG = "MainPage";

            logger.TraceInformation(String.Format("Current algorithm: {0}", connectionAlgorithm.Algorithm.ToString()));

            this.BindingContext = mainPageViewModel;

            InitializeComponent();
            Title = "Bluetooth LE Scanner";

            ToolbarItems.Add(new ToolbarItem("Settings", "settings.png", async () => { await Navigation.PushAsync(new ChoiceAlgorithm(connectionAlgorithmVM)); }));

            connectionAlgorithmVM.AlgorithmSelected += ConnectionAlgorithmVM_AlgorithmSelected;

            Init();

            mainPageViewModel.StringDebug = "Used: Native alg.; Mode: standart";
        }

        public void Init()
        {
            logger.TraceInformation("Clear last algorithm");
            deviceScanner = null;

            if (ble != null)
            {
                ble.StateChanged -= Ble_StateChanged;
                ble = null;
            }
            if (adapter != null)
            {
                adapter = null;
            }

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

        private void PluginBleInit()
        {
            ble = CrossBluetoothLE.Current;
            adapter = CrossBluetoothLE.Current.Adapter;

            ble.StateChanged += Ble_StateChanged;
        }

        private void Ble_StateChanged(object sender, Plugin.BLE.Abstractions.EventArgs.BluetoothStateChangedArgs e)
        {
            logger.TraceInformation(String.Format("Change blutoth status: {0} from {1}", e.NewState.ToString(), e.OldState.ToString()));
        }

        public void NativeDeviceScannerInit()
        {
            deviceScanner = DependencyService.Get<IDeviceScanner>();
            deviceScanner.Init(mainPageViewModel);

            if (deviceScanner.CheckSupportBLE())
                logger.LogInfo("BLE is supported!");
            else
                logger.LogWarning("Bluetooth Low Energy is not supported.");

            if (deviceScanner.CheckPermissions())
                logger.LogInfo("Android permission AccessCoarseLocation granted!");
            else
            {
                logger.LogWarning("The application does not have the necessary permission (AccessCoarseLocation).");
                // Only if SDK API VERSION >= 23
                deviceScanner.GetRuntimePermissions();
            }

            deviceScanner.GetBluetoothAdapter();
        }

        private void ConnectionAlgorithmVM_AlgorithmSelected(object sender, ConnectionAlgorithm args)
        {
            connectionAlgorithm = args;
            logger.TraceInformation(String.Format("Used: {0}; Mode: {1}", connectionAlgorithm.Algorithm.ToString(), "Standart"));
            mainPageViewModel.StringDebug = String.Format("Used: {0}; Mode: {1}", connectionAlgorithm.Algorithm.ToString(), "Standart");
            Init();
        }

        private async void OnClick(object sender, EventArgs e)
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
                    await PluginBleStopScanAsync();
                }
                else if (connectionAlgorithm.Algorithm == ConnectionAlgorithmType.CombineNativeAndCross)
                {
                    logger.TraceInformation("Combine Native /& Ble Cross stop scan");
                }

                IsBusy = false;
            } else
            {
                if (connectionAlgorithm.Algorithm == ConnectionAlgorithmType.Native)
                {
                    logger.TraceInformation("Native start scan");
                    deviceScanner.ScanLeDevice();
                }
                else if (connectionAlgorithm.Algorithm == ConnectionAlgorithmType.PluginBLE)
                {
                    logger.TraceInformation("Plugin BLE Start Scan");
                    PluginBleStartScan();
                }
                else if (connectionAlgorithm.Algorithm == ConnectionAlgorithmType.CombineNativeAndCross)
                {
                    logger.TraceInformation("Combine Native /& Ble Cross start scan");
                }
                IsBusy = true;
            }
        }

        private void PluginBleStartScan()
        {
            adapter.ScanTimeout = 20000;

        }

        private async Task PluginBleStopScanAsync()
        {
            await adapter.StartScanningForDevicesAsync();
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

                var device = deviceScanner.GetDevice(index);
                logger.TraceInformation($"Finded device on native devices list : {index} - {device.Address}");

                deviceScanner.StopScan();

                //await DisplayAlert($"Выбранно устройство {index}", $"{device.Name} - {device.Address}", "OK");
                await Navigation.PushAsync(new SensorDataPage(device, connectionAlgorithm));
            }
        }

        private async Task ToolbarItem_ActivatedAsync(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new ChoiceAlgorithm(connectionAlgorithmVM));
        }

        ~MainPage()
        {
            connectionAlgorithmVM.AlgorithmSelected -= ConnectionAlgorithmVM_AlgorithmSelected;
        }
    }
}
