namespace NativeBLE.Core


type ISensorData =
    abstract member Init: SensorViewModel * Device -> unit

    abstract member SetMinLimit: int -> unit

    abstract member OnClickStartButton: unit -> unit
    abstract member OnClickResultButton: unit -> unit
    abstract member OnClickConnectButton: unit -> unit

