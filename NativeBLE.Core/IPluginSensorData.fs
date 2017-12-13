namespace NativeBLE.Core

open Plugin.BLE.Abstractions.Contracts


type IPluginSensorData = 
    abstract member Init: SensorViewModel * IDevice -> unit

    abstract member SetMinLimit: int -> unit

    abstract member OnClickStartButton: unit -> unit
    abstract member OnClickResultButton: unit -> unit
    abstract member OnClickConnectButton: unit -> unit

