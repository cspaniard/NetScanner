module Model.IpNetworkValidation

open Model.ValidationHelper

let IpNetworkErrors = ErrorDict()

IpNetworkErrors.Add(ValueIsEmpty, "El valor de la red está vacío.")
IpNetworkErrors.Add(ValueContainsSpaces, "La red proporcionada contiene espacios.")
IpNetworkErrors.Add(OctectIsEmpty, "La red contiene octetos vacíos.")
IpNetworkErrors.Add(OctectIsNotNumber, "La red contiene partes que no son números.")
IpNetworkErrors.Add(OctectOutOfRange, "La red contiene octetos fuera de rango.")
IpNetworkErrors.Add(OctectIncorrectCount, "La red tiene un número erróneo de octetos.")

let getValidatorsList () =
    [|
        checkEmpty IpNetworkErrors
        checkOctectsForSpacesOrEmpty IpNetworkErrors
        checkOctectsAreIntsInRange IpNetworkErrors
        checkOctectCount IpNetworkErrors 3
    |]
