namespace NativeBLE.Core

open System.ComponentModel
open System
open System.Collections.ObjectModel

type Device() =
    member val Name = String.Empty with get, set
    member val Address = String.Empty with get, set

type DeviceViewModel(name: string,
                     address: string)=
    inherit BaseViewModel()

    let device = new Device()
    do
        device.Name <- name
        device.Address <- address

    member x.Name 
        with get() = device.Name
        and set value =
            device.Name <- value
            base.OnPropertyChanged <@ x.Name @>

    member x.Address 
        with get() = device.Address
        and set value =
            device.Address <- value
            base.OnPropertyChanged <@ x.Address @>
            