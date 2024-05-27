namespace Brokers

open System
open DI.Interfaces

type ExceptionBroker () as this =

    let self = this :> IExceptionBroker

    interface IExceptionBroker with
        //--------------------------------------------------------------------------------------------------------------
        member _.printSingle (exn : Exception) =

            Console.Error.WriteLine exn
            if exn.InnerException <> null then self.printSingle exn.InnerException
        //--------------------------------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------------------------------
        member _.printAggregate (aggregateException : AggregateException) =

            aggregateException.InnerExceptions
            |> Seq.iter (fun e -> match e with
                                  | :? AggregateException as ae -> self.printAggregate ae
                                  | e -> self.printSingle e)
        //--------------------------------------------------------------------------------------------------------------
