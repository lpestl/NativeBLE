namespace NativeBLE.Core

open System
open System.Collections.ObjectModel
open System.ComponentModel
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns

type Agent<'T> = MailboxProcessor<'T>

// REF: http://www.fssnip.net/4Q
type BaseViewModel() =
    let propertyChanged = new Event<_, _>()
    let toPropName(query : Expr) = 
        match query with
        | PropertyGet(a, b, list) ->
            b.Name
        | _ -> ""

    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member x.PropertyChanged = propertyChanged.Publish

    abstract member OnPropertyChanged: string -> unit
    default x.OnPropertyChanged(propertyName : string) =
        propertyChanged.Trigger(x, new PropertyChangedEventArgs(propertyName))

    member x.OnPropertyChanged(expr : Expr) =
        let propName = toPropName(expr)
        x.OnPropertyChanged(propName)
