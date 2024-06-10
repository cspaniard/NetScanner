module Model.IpNetworkValidation

open Model.ValidationHelper

let IpNetworkErrors = ErrorDict ()
let [<Literal>] PREFIX = "IpNetwork ({0}):"

IpNetworkErrors.Add (EmptyValue, $"{PREFIX} El valor de la red está vacío.")
IpNetworkErrors.Add (ValueContainsSpaces, $"{PREFIX} La red proporcionada contiene espacios.")
IpNetworkErrors.Add (EmptyOctects, $"{PREFIX} La red contiene octetos vacíos.")
IpNetworkErrors.Add (NonNumericOctects, $"{PREFIX} La red contiene partes que no son números.")
IpNetworkErrors.Add (OutOfRangeOctects, $"{PREFIX} La red contiene octetos fuera de rango.")
IpNetworkErrors.Add (IncorrectOctectCount, $"{PREFIX} La red tiene un número erróneo de octetos.")

let getValidatorsList () =
    [|
        checkEmptyValueTry IpNetworkErrors
        checkValueContainsSpacesTry IpNetworkErrors
        checkEmptyOctects IpNetworkErrors
        checkNonNumericOctectsTry IpNetworkErrors
        checkOutOfRangeOctectsTry IpNetworkErrors
        checkIncorrectOctectCountTry IpNetworkErrors 3
    |]
