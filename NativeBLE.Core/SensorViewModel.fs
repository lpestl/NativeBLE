namespace NativeBLE.Core

open Xamarin.Forms
open System.Collections.ObjectModel

type SensorViewModel(i: int) =
    inherit BaseViewModel()

    let logger = DependencyService.Get<ILogger>()
    do
        logger.TAG <- "SensorViewModel"

    let mutable index = i
    let deviceViewModel = DependencyService.Get<IDeviceList>().GetViewModel(i)
