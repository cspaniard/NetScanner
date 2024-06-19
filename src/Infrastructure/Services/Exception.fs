namespace Services

open System
open DI.Interfaces

type ExceptionService (ExceptionBroker : IExceptionBroker) =

    interface IExceptionService with
        //--------------------------------------------------------------------------------------------------------------
        member _.outputExceptionTry (exn : Exception) =

            match exn with
            | :? AggregateException as ae -> ExceptionBroker.printAggregateTry ae
            | e -> ExceptionBroker.printSingleTry e
        //--------------------------------------------------------------------------------------------------------------
