open System
open System.ComponentModel.DataAnnotations
open System.Diagnostics.CodeAnalysis
open CommandLine

open Microsoft.Extensions.DependencyInjection
open Microsoft.FSharp.Core
open Model
open Model.Constants
open DI.Interfaces
open DI.Providers
open NetScanner.ArgumentOptionsValidation


//----------------------------------------------------------------------------------------------------------------------
[<DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof<ArgumentOptions>)>]
let parser = new Parser (fun o -> o.HelpWriter <- null)

let parserResult =
    Environment.GetCommandLineArgs () |> Array.tail
    |> parser.ParseArguments<ArgumentOptions>

let ServiceProvider = ServiceProviderBuild parserResult.Value

let helpTextService = ServiceProvider.GetRequiredService<IHelpTextService>()
let exceptionService = ServiceProvider.GetRequiredService<IExceptionService>()
let ifOptionErrorsShowAndExit = ifOptionErrorsShowAndExit helpTextService
//----------------------------------------------------------------------------------------------------------------------

try
    ifOptionErrorsShowAndExit parserResult

    let mainApp = ServiceProvider.GetRequiredService<IMainApp>()
    mainApp.run ()

with
| :? ValidationException as ve -> helpTextService.showHelp <| ValidationError ve |> exit
| e -> exceptionService.outputException e
       exit EXIT_CODE_EXCEPTION
//----------------------------------------------------------------------------------------------------------------------
