namespace NativeBLE.Core

open Xamarin.Forms
open System
open System.Collections.ObjectModel
open System.Linq.Expressions

type SensorViewModel(device: DeviceViewModel) =
    inherit BaseViewModel()

    let logger = DependencyService.Get<ILogger>()
    do
        logger.TAG <- "SensorViewModel"
        
    let deviceViewModel = device
    let sensorData = SensorData()
    
    let mutable mConnected = false
    let mutable mToolBarButtonText = "Connect"
    let mutable mDebug = String.Empty

    // Result button
    let mutable mVisibleResultButton = false
    let mutable mTextResultButton = ""
    let mutable mColorResultButton = Xamarin.Forms.Color.Gray
    // Start button
    //let mutable mEnableStartButton = false
    let mutable mTextStartButton = "Disconnected"
    let mutable mColorTextStartButton = Xamarin.Forms.Color.Black

    let mutable mSleeveModeText = "Sleeve mode (>800 release)"
    
    let mutable mConnectionTimeString = "00:000"
    let mutable mFirstDataTimeString = "00:000"
    let mutable mDisconnectionTimeString = "00:000"
    let mutable mConnectionTimeColor = Xamarin.Forms.Color.Green
    let mutable mFirstDataTimeColor = Xamarin.Forms.Color.Green
    let mutable mDisconnectionTimeColor = Xamarin.Forms.Color.Green


    member x.Name with get() = deviceViewModel.Name
    member x.Address with get() = deviceViewModel.Address

    member x.ToolBarButtonText 
        with get() = mToolBarButtonText
        and set value = 
            mToolBarButtonText <- value
            base.OnPropertyChanged <@ x.ToolBarButtonText @>

    member x.Connected 
        with get() = mConnected
        and set value = 
            mConnected <- value
            if mConnected then
                x.ToolBarButtonText <- "Disconnect"
            else
                x.ToolBarButtonText <- "Connect"
            base.OnPropertyChanged <@ x.Connected @>

    member x.SleeveModeText 
        with get() = mSleeveModeText
        and set value =
            mSleeveModeText <- value
            base.OnPropertyChanged <@ x.SleeveModeText @>

    member x.ConnectionState 
        with get() = sensorData.ConnectionState
        and set value =
            sensorData.ConnectionState <- value
            x.TextStart <- "Start"
            //x.EnableStart <- true
            base.OnPropertyChanged <@ x.ConnectionState @>

    //member x.EnableStart 
    //    with get() = mEnableStartButton
    //    and set value =
    //        mEnableStartButton <- value
    //        base.OnPropertyChanged <@ x.EnableStart @>    

    member x.TextStart 
        with get() = mTextStartButton
        and set value =
            mTextStartButton <- value
            base.OnPropertyChanged <@ x.TextStart @>    

    member x.ColorStart
        with get() = mColorTextStartButton
        and set value =
            mColorTextStartButton <- value
            base.OnPropertyChanged <@ x.ColorStart @>    

    member x.SetResult() =
        x.VisibleResult <- true
        x.TextResult <- "Test passed!"
        x.ColorResult <- Xamarin.Forms.Color.Green

    member x.VisibleResult 
        with get() = mVisibleResultButton
        and set value =
            mVisibleResultButton <- value
            base.OnPropertyChanged <@ x.VisibleResult @>    

    member x.TextResult 
        with get() = mTextResultButton
        and set value =
            mTextResultButton <- value
            base.OnPropertyChanged <@ x.TextResult @>    

    member x.ColorResult
        with get() = mColorResultButton
        and set value =
            mColorResultButton <- value
            base.OnPropertyChanged <@ x.ColorResult @>    

     member x.DebugString
        with get() = mDebug
        and set value =
            mDebug <- value
            base.OnPropertyChanged <@ x.DebugString @>       

     member x.SleeveMode
        with get() = sensorData.SleeveMode
        and set value =
            sensorData.SleeveMode <- value
            if sensorData.SleeveMode then
                x.SleeveModeText <- "Sleeve mode (>800 release)"
            else 
                x.SleeveModeText <- "Plastic mode (0 release)"
            base.OnPropertyChanged <@ x.SleeveMode @>       

     member x.Data
        with get() = sensorData.Data
        and set value =
            sensorData.Data <- value
            base.OnPropertyChanged <@ x.Data @>
            
     member x.SensorA
        with get() = sensorData.SensorA
        and set value =
            sensorData.SensorA <- value
            base.OnPropertyChanged <@ x.SensorA @>     
            
     member x.SensorB
        with get() = sensorData.SensorB
        and set value =
            sensorData.SensorB <- value
            base.OnPropertyChanged <@ x.SensorB @>     
            
     member x.BataryLevel
        with get() = sensorData.BataryLevel
        and set value =
            sensorData.BataryLevel <- value
            base.OnPropertyChanged <@ x.BataryLevel @>     
            
     member x.BatchVersion
        with get() = sensorData.BatchVersion
        and set value =
            sensorData.BatchVersion <- value
            base.OnPropertyChanged <@ x.BatchVersion @>     
            
     member x.RSSI
        with get() = sensorData.RSSI
        and set value =
            sensorData.RSSI <- value
            base.OnPropertyChanged <@ x.RSSI @>     
            
     member x.FirmwareVersion
        with get() = sensorData.FirmwareVersion
        and set value =
            sensorData.FirmwareVersion <- value
            base.OnPropertyChanged <@ x.FirmwareVersion @>     
            
     member x.SensorA_TopResult
        with get() = sensorData.SensorA_TopResult
        and set value =
            sensorData.SensorA_TopResult <- value
            base.OnPropertyChanged <@ x.SensorA_TopResult @>     
            
     member x.SensorA_BottomResult
        with get() = sensorData.SensorA_BottomResult
        and set value =
            sensorData.SensorA_BottomResult <- value
            base.OnPropertyChanged <@ x.SensorA_BottomResult @>     
            
     member x.SensorB_TopResult
        with get() = sensorData.SensorB_TopResult
        and set value =
            sensorData.SensorB_TopResult <- value
            base.OnPropertyChanged <@ x.SensorB_TopResult @>     
            
     member x.SensorB_BottomResult
        with get() = sensorData.SensorB_BottomResult
        and set value =
            sensorData.SensorB_BottomResult <- value
            base.OnPropertyChanged <@ x.SensorB_BottomResult @>     
            
     member x.ConnectedStateText 
        with get() = if mConnected then "Connected" else "Disconnected"
        and set value =
            if value = "Disconnected" then
                mConnected <- false 
            else 
                mConnected <- true
                
            base.OnPropertyChanged <@ x.ConnectedStateText @>

     member x.ConnectionTimeString
        with get() = mConnectionTimeString
        and set value =
            mConnectionTimeString <- value
            base.OnPropertyChanged <@ x.ConnectionTimeString @>

     member x.FirstDataTimeString
        with get() = mFirstDataTimeString
        and set value = 
            mFirstDataTimeString <- value
            base.OnPropertyChanged <@ x.FirstDataTimeString @>

     member x.DisconnectionTimeString
        with get() = mDisconnectionTimeString
        and set value = 
            mDisconnectionTimeString <- value
            base.OnPropertyChanged <@ x.DisconnectionTimeString @>

     member x.ConnectionTimeColor
        with get() = mConnectionTimeColor
        and set value =
            mConnectionTimeColor <- value
            base.OnPropertyChanged <@ x.ConnectionTimeColor @>

     member x.FirstDataTimeColor
        with get() = mFirstDataTimeColor
        and set value = 
            mFirstDataTimeColor <- value
            base.OnPropertyChanged <@ x.FirstDataTimeColor @>

     member x.DisconnectionTimeColor
        with get() = mDisconnectionTimeColor
        and set value =
            mDisconnectionTimeColor <- value
            base.OnPropertyChanged <@ x.DisconnectionTimeColor @>

     member x.ConnectionTimeSpan 
        with get() = sensorData.ConnectionTimeSpan
        and set value =
            sensorData.ConnectionTimeSpan <- value
            x.ConnectionTimeString <- String.Format("{0:00}.{1:000}", sensorData.ConnectionTimeSpan.Seconds, sensorData.ConnectionTimeSpan.Milliseconds)
            if sensorData.ConnectionTimeSpan.Seconds < 3 then
                x.ConnectionTimeColor <- Xamarin.Forms.Color.Green
            elif sensorData.ConnectionTimeSpan.Seconds < 9 then
                x.ConnectionTimeColor <- Xamarin.Forms.Color.Orange
            else 
                x.ConnectionTimeColor <- Xamarin.Forms.Color.Red
            base.OnPropertyChanged <@ x.ConnectionTimeSpan @>

     member x.FirstDataTimeSpan
        with get() = sensorData.FirstDataTimeSpan
        and set value =
            sensorData.FirstDataTimeSpan <- value
            x.FirstDataTimeString <- String.Format("{0:00}.{1:000}", sensorData.FirstDataTimeSpan.Seconds, sensorData.FirstDataTimeSpan.Milliseconds)
            if sensorData.FirstDataTimeSpan.Seconds < 3 then
                x.FirstDataTimeColor <- Xamarin.Forms.Color.Green
            elif sensorData.FirstDataTimeSpan.Seconds < 9 then
                x.FirstDataTimeColor <- Xamarin.Forms.Color.Orange
            else 
                x.FirstDataTimeColor <- Xamarin.Forms.Color.Red
            base.OnPropertyChanged <@ x.FirstDataTimeSpan @>

     member x.DisconnectionTimeSpan
        with get() = sensorData.DisconnectionTimeSpan
        and set value =
            sensorData.DisconnectionTimeSpan <- value
            x.DisconnectionTimeString <- String.Format("{0:00}.{1:000}", sensorData.DisconnectionTimeSpan.Seconds, sensorData.DisconnectionTimeSpan.Milliseconds)
            if sensorData.DisconnectionTimeSpan.Seconds < 3 then
                x.DisconnectionTimeColor <- Xamarin.Forms.Color.Green
            elif sensorData.DisconnectionTimeSpan.Seconds < 9 then
                x.DisconnectionTimeColor <- Xamarin.Forms.Color.Orange
            else 
                x.DisconnectionTimeColor <- Xamarin.Forms.Color.Red
            base.OnPropertyChanged <@ x.DisconnectionTimeSpan @>