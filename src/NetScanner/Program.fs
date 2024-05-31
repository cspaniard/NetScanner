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

let HelpTextService = serviceProvider.GetRequiredService<IHelpTextService>()
let ExceptionService = serviceProvider.GetRequiredService<IExceptionService>()
let OptionValidationService = serviceProvider.GetRequiredService<IOptionValidationService>()
//----------------------------------------------------------------------------------------------------------------------

try
    OptionValidationService.ifErrorsShowAndExit parserResult

    let mainApp = serviceProvider.GetRequiredService<IMainApp>()
    mainApp.run ()

with
| :? ValidationException as ve -> HelpTextService.showHelp <| ValidationError ve |> exit
| e -> ExceptionService.outputException e
       exit EXIT_CODE_EXCEPTION
//----------------------------------------------------------------------------------------------------------------------
