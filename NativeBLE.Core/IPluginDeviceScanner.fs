namespace NativeBLE.Core

open Plugin.BLE.Abstractions.Contracts


type IPluginDeviceScanner = 
    abstract member GetBluetoothAdapter: unit -> unit
    abstract member GetDevice: int -> Device
    abstract member GetIDevice: int -> IDevice

    abstract member ScanLeDevice: int -> unit
    abstract member StopScan: unit -> unit
    
    abstract member Init: MainPageViewModel -> unit

