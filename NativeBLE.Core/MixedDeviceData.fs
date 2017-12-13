namespace NativeBLE.Core

open Plugin.BLE.Abstractions.Contracts
open System


type MixedDeviceData(nativeDevice : Object, rssi : int, scanRecord : byte[]) = 
    member val NativeDevice = nativeDevice with get, set 
    member val Rssi = rssi with get, set
    member val ScanRecord = scanRecord with get, set

