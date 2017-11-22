﻿namespace NativeBLE.Core


type IDeviceScanner =
    abstract member CheckPermissions: unit -> bool
    abstract member CheckSupportBLE: unit -> bool
    abstract member GetRuntimePermissions: unit -> unit
    abstract member GetBluetoothAdapter: unit -> unit

    abstract member ScanLeDevice: unit -> unit
    abstract member StopScan: unit -> unit
    
    abstract member PageViewModel: MainPageViewModel with get,set
