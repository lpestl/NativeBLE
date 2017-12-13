namespace NativeBLE.Core

open Plugin.BLE.Abstractions.Contracts


type IDeviceScanner =
    abstract member GetBluetoothAdapter: unit -> unit
    abstract member GetDevice: int -> Device
    abstract member GetMixedDeviceData: int -> MixedDeviceData

    abstract member ScanLeDevice: unit -> unit
    abstract member StopScan: unit -> unit
    
    abstract member Init: MainPageViewModel -> unit
