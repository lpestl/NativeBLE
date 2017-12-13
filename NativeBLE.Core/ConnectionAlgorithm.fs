namespace NativeBLE.Core

open Microsoft.FSharp.Reflection

type ConnectionAlgorithmType =
    | Native
    | PluginBLE
    | CombineNativeAndCross

    override x.ToString() =
        match x with
            | Native -> sprintf("Native BLE")
            | PluginBLE -> sprintf("Xamarin crossplatform plugin BLE");
            | CombineNativeAndCross -> sprintf("Mixed connection algorithm");

     static member GetAllValues () =
        FSharpType.GetUnionCases(typeof<ConnectionAlgorithmType>)
        |> Array.map (fun case -> FSharpValue.MakeUnion(case, [||]) :?> ConnectionAlgorithmType)

type ConnectionAlgorithm () =
    member val Algorithm = ConnectionAlgorithmType.CombineNativeAndCross with get, set

type ConnectionAlgorithmViewModel() =
    inherit BaseViewModel()

    let mutable isAlgorithmSelected = false
    let mutable algorithm = Event<ConnectionAlgorithm>()
    
    [<CLIEvent>]
    member val AlgorithmSelected = algorithm.Publish

    member x.IsAlgorithmSelected
        with set value =
            isAlgorithmSelected <- true
            base.OnPropertyChanged <@ x.IsSelectionComplete @>

    /// #Bindable
    member x.IsSelectionComplete = isAlgorithmSelected

    member x.Algoritm 
        with get() = algorithm
        and set value =
            algorithm <- value
            base.OnPropertyChanged <@ x.Algoritm @>
            
    member x.SaveAlgoritmSelection(connectionAlgorithm) =
        algorithm.Trigger(connectionAlgorithm)