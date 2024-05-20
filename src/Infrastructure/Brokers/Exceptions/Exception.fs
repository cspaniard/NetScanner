namespace Brokers.Exceptions.Exception

open System

type Broker () =

    //------------------------------------------------------------------------------------------------------------------
    static member printSingle (exn : Exception) =

        Console.Error.WriteLine exn
        if exn.InnerException <> null then Broker.printSingle exn.InnerException
    //------------------------------------------------------------------------------------------------------------------

    //------------------------------------------------------------------------------------------------------------------
    static member printAggregate (aggregateException : AggregateException) =

        aggregateException.InnerExceptions
        |> Seq.iter (fun e -> match e with
                              | :? AggregateException as ae -> Broker.printAggregate ae
                              | e -> Broker.printSingle e)
    //------------------------------------------------------------------------------------------------------------------
