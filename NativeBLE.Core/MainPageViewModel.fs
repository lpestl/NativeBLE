namespace NativeBLE.Core

open Xamarin.Forms
open System.Collections.ObjectModel

type MainPageViewModel() =
    inherit BaseViewModel()

    let logger = DependencyService.Get<ILogger>()
    do
        logger.TAG <- "MainPageViewModel"

    let devices = new ObservableCollection<DeviceViewModel>()
    
    let mutable scanning = false 
    let mutable scanButtonText = "Scan"

    member x.Scanning 
        with get() = scanning
        and set value =
            scanning <- value
            if scanning then 
                x.ScanButtonText <- "Stop"
            else 
                x.ScanButtonText <- "Scan"
            base.OnPropertyChanged <@ x.Scanning @>

    member x.Devices 
        with get() = devices

    member x.ScanButtonText
        with get() = scanButtonText
        and set value =
            scanButtonText <- value
            base.OnPropertyChanged <@ x.ScanButtonText @>