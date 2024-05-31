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


//----------------------------------------------------------------------------------------------------------------------
[<DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof<ArgumentOptions>)>]
let parser = new Parser (fun o -> o.HelpWriter <- null)

let parserResult =
    Environment.GetCommandLineArgs () |> Array.tail
    |> parser.ParseArguments<ArgumentOptions>

let serviceProvider = ServiceProviderBuild parserResult.Value

let helpTextService = serviceProvider.GetRequiredService<IHelpTextService>()
let exceptionService = serviceProvider.GetRequiredService<IExceptionService>()
let optionValidationService = serviceProvider.GetRequiredService<IOptionValidationService>()
//----------------------------------------------------------------------------------------------------------------------

try
    optionValidationService.ifErrorsShowAndExit parserResult

    let mainApp = serviceProvider.GetRequiredService<IMainApp>()
    mainApp.run ()

with
| :? ValidationException as ve -> helpTextService.showHelp <| ValidationError ve |> exit
| e -> exceptionService.outputException e
       exit EXIT_CODE_EXCEPTION
//----------------------------------------------------------------------------------------------------------------------
