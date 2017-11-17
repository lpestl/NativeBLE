namespace NativeBLE.Core

type IDeviceList =
    abstract member IsEmpty: unit -> bool
    abstract member Clear: unit -> unit
    abstract member GetViewModel: int -> DeviceViewModel
    abstract member Contains: DeviceViewModel -> bool

