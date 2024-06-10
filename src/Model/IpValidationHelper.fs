module Model.ValidationHelper

open System
open System.Collections.Generic
open System.ComponentModel.DataAnnotations
open Motsoft.Util

//----------------------------------------------------------------------------------------------------------------------
type Errors =
    | EmptyValue
    | ValueContainsSpaces
    | EmptyOctects
    | NonNumericOctects
    | OutOfRangeOctects
    | IncorrectOctectCount

type ErrorDict = Dictionary<Errors, string>
//----------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------------------------
let validationExceptionIfTrue errorMessageFormat (value : string) boolVal =

    if boolVal then
        raise <| ValidationException (String.Format(errorMessageFormat, value))
//----------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------------------------
let validationException errorMessageFormat (value : string) =

    validationExceptionIfTrue errorMessageFormat value true
//----------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------------------------
let checkEmptyValueTry (errors : ErrorDict) (value : string) =
    value
    |> String.IsNullOrWhiteSpace
    |> validationExceptionIfTrue errors[EmptyValue] value
//----------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------------------------
let checkValueContainsSpacesTry (errors : ErrorDict) (value : string) =
    value
    |> splitWithOptions "." StringSplitOptions.None
    |> Array.iter (fun o -> o.Contains " "
                            |> validationExceptionIfTrue errors[ValueContainsSpaces] value)
//----------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------------------------
let checkEmptyOctects (errors : ErrorDict) (value : string) =
    value
    |> splitWithOptions "." StringSplitOptions.None
    |> Array.iter (fun o -> o
                            |> String.IsNullOrWhiteSpace
                            |> validationExceptionIfTrue errors[EmptyOctects] value)
//----------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------------------------
let checkNonNumericOctectsTry (errors : ErrorDict) (value : string) =
    value
    |> split "."
    |> Array.iter
           (fun o -> if (Int32.TryParse o |> fst) = false then
                        validationException errors[NonNumericOctects] value)
//----------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------------------------
let checkOutOfRangeOctectsTry (errors : ErrorDict) (value : string) =
    value
    |> split "."
    |> Array.iter
           (fun o -> match Int32.TryParse o with
                     | true, intVal when intVal < 0 || intVal > 254 ->
                         validationException errors[OutOfRangeOctects] value
                     | _ -> ())
//----------------------------------------------------------------------------------------------------------------------

//----------------------------------------------------------------------------------------------------------------------
let checkIncorrectOctectCountTry (errors : ErrorDict) octetCount (value : string) =
    value
    |> splitWithOptions "." StringSplitOptions.None
    |> Array.length <> octetCount
    |> validationExceptionIfTrue errors[IncorrectOctectCount] value
//----------------------------------------------------------------------------------------------------------------------
