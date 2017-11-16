namespace NativeBLE.Core

type ILogger =
    abstract member TAG: string with get, set
    
    abstract member TraceInformation: string -> unit
    abstract member TraceWarning: string -> unit
    abstract member TraceError: string -> unit
    
    abstract member LogInfo: string -> unit
    abstract member LogWarning: string -> unit
    abstract member LogError: string -> unit
