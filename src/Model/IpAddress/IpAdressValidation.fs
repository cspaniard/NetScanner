module NetScanner.Model.IpAddressValidation

open Model.ValidationHelper

let IpAdressErrors = ErrorDict()

IpAdressErrors.Add(ValueIsEmpty, "El valor de la IP está vacío.")
IpAdressErrors.Add(ValueContainsSpaces, "La IP proporcionada contiene espacios.")
IpAdressErrors.Add(OctectIsEmpty, "La IP contiene octetos vacíos.")
IpAdressErrors.Add(OctectIsNotNumber, "La IP contiene partes que no son números.")
IpAdressErrors.Add(OctectOutOfRange, "La IP contiene octetos fuera de rango.")
IpAdressErrors.Add(OctectIncorrectCount, "La IP tiene un número erróneo de octetos.")

let getValidatorsList () =
    [|
        checkEmpty IpAdressErrors
        checkOctectsForSpacesOrEmpty IpAdressErrors
        checkOctectCount IpAdressErrors 4
        checkOctectsAreIntsInRange IpAdressErrors
    |]
