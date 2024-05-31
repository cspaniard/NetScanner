namespace Services

open System
open DI.Interfaces

type ExceptionService (ExceptionBroker : IExceptionBroker) =

    interface IExceptionService with
        //--------------------------------------------------------------------------------------------------------------
        member _.outputException (exn : Exception) =

            match exn with
            | :? AggregateException as ae -> ExceptionBroker.printAggregate ae
            | e -> ExceptionBroker.printSingle e
        //--------------------------------------------------------------------------------------------------------------
