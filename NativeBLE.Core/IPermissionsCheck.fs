namespace NativeBLE.Core


type IPermissionsCheck =    
    abstract member CheckPermissions: unit -> bool
    abstract member CheckSupportBLE: unit -> bool
    abstract member GetRuntimePermissions: unit -> unit

    abstract member RestartAdapter: unit -> unit

