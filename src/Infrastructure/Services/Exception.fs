namespace Services

open System
open DI.Interfaces

type ExceptionService (exceptionBroker : IExceptionBroker) =

    interface IExceptionService with
        //--------------------------------------------------------------------------------------------------------------
        member _.outputException (exn : Exception) =

            match exn with
            | :? AggregateException as ae -> exceptionBroker.printAggregate ae
            | e -> exceptionBroker.printSingle e
        //--------------------------------------------------------------------------------------------------------------
