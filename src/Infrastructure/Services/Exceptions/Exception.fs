namespace Services.Exceptions.Exception

open System

type IExceptionBroker = DI.Brokers.ExceptionsDI.IExceptionBroker

type Service () =

    static member outputException (exn : Exception) =

        match exn with
        | :? AggregateException as ae -> IExceptionBroker.printAggregate ae
        | e -> IExceptionBroker.printSingle e
