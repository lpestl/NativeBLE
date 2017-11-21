namespace NativeBLE.Core

type SensorData() =
    member val ConnectionState = "Disconnected" with get, set
    member val SleeveMode = true with get,set
    member val Data = "" with get, set    
    member val SensorA = "No data" with get, set
    member val SensorB = "No data" with get, set
    member val BataryLevel = "N/A" with get, set
    member val BatchVersion = "N/A" with get, set
    member val RSSI = "N/A" with get, set
    member val FirmwareVersion = "N/A" with get, set
    member val SensorA_TopResult = "N/A" with get, set
    member val SensorA_BottomResult = "N/A" with get, set
    member val SensorB_TopResult = "N/A" with get, set
    member val SensorB_BottomResult = "N/A" with get, set

