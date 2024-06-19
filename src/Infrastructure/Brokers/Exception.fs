namespace Brokers

open System
open DI.Interfaces

type ExceptionBroker (debug : bool) as this =

    let self = this :> IExceptionBroker

    interface IExceptionBroker with
        //--------------------------------------------------------------------------------------------------------------
        member _.printSingleTry (exn : Exception) =

            if debug then
                exn.ToString()
            else
                exn.Message
            |> Console.Error.WriteLine

            if exn.InnerException <> null then self.printSingleTry exn.InnerException
        //--------------------------------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------------------------------
        member _.printAggregateTry (aggregateException : AggregateException) =

            if debug then
                Console.Error.WriteLine $"STACK TRACE: {aggregateException.Message}"
                Console.Error.WriteLine aggregateException.StackTrace

            aggregateException.InnerExceptions
            |> Seq.iter (fun e -> match e with
                                  | :? AggregateException as ae -> self.printAggregateTry ae
                                  | e -> self.printSingleTry e)
        //--------------------------------------------------------------------------------------------------------------
