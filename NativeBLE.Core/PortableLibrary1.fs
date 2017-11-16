namespace NativeBLE.Core

open Xamarin.Forms
open System.Diagnostics

type Class1() = 
    let logger = DependencyService.Get<ILogger>()
    do
        logger.TAG <- "TEST"

    let deviceScaner = DependencyService.Get<IDeviceScanner>()
    do        
        //deviceScaner.SetDevicesList(list)
        if deviceScaner.CheckSupportBLE() then 
            logger.LogInfo("BLE is supported!")
        else 
            logger.LogWarning("Bluetooth Low Energy is not supported.")

        if deviceScaner.CheckPermissions() then 
            logger.LogInfo("Android permission AccessCoarseLocation granted!")
        else 
            logger.LogWarning("The application does not have the necessary permission (AccessCoarseLocation).")
            // Only if SDK API VERSION >= 23
            deviceScaner.GetRuntimePermissions();
        
        deviceScaner.GetBluetoothAdapter()
        //deviceScaner.GetBluetoothLeScanner() |> ignore
        deviceScaner.ScanLeDevice()


    member this.X = "F#"
