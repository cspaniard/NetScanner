module Model.IpNetworkValidation

open Model.ValidationHelper

let IpNetworkErrors = ErrorDict ()
let [<Literal>] PREFIX = "IpNetwork ({0}):"

IpNetworkErrors.Add (ValueIsEmpty, $"{PREFIX} El valor de la red está vacío.")
IpNetworkErrors.Add (ValueContainsSpaces, $"{PREFIX} La red proporcionada contiene espacios.")
IpNetworkErrors.Add (OctectIsEmpty, $"{PREFIX} La red contiene octetos vacíos.")
IpNetworkErrors.Add (OctectIsNotNumber, $"{PREFIX} La red contiene partes que no son números.")
IpNetworkErrors.Add (OctectOutOfRange, $"{PREFIX} La red contiene octetos fuera de rango.")
IpNetworkErrors.Add (OctectIncorrectCount, $"{PREFIX} La red tiene un número erróneo de octetos.")

let getValidatorsList () =
    [|
        checkEmptyTry IpNetworkErrors
        checkOctectsForSpacesOrEmptyTry IpNetworkErrors
        checkOctectsAreIntsInRangeTry IpNetworkErrors
        checkOctectCountTry IpNetworkErrors 3
    |]
