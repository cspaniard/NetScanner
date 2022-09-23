module Model.HelpTextHelper

open System
open CommandLine

let [<Literal>] LEFT_MARGIN = 2

type ArgLinesInfo = ArgLinesInfo of paramNames : string * helpText : string

type AppErrors =
    | ArgErrors of seq<Error>
    | ExceptionErrors of seq<Exception>
