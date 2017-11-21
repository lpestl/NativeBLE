namespace NativeBLE.Core


type ISensorData =
    abstract member Init: SensorViewModel -> unit
    //abstract member SetSensorViewModel: SensorViewModel -> unit
    abstract member SetMinLimit: int -> unit
    abstract member OnResume: unit -> unit
    abstract member OnPause: unit -> unit
    abstract member OnDestroy: unit -> unit
    abstract member OnClickStartButton: unit -> unit
    abstract member OnClickResultButton: unit -> unit
    abstract member OnClickConnectButton: unit -> unit

