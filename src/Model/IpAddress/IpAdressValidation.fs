module Model.IpAddressValidation

open Model.ValidationHelper

let IpAdressErrors = ErrorDict ()

IpAdressErrors.Add (ValueIsEmpty, "El valor de la IP está vacío.")
IpAdressErrors.Add (ValueContainsSpaces, "La IP proporcionada contiene espacios.")
IpAdressErrors.Add (OctectIsEmpty, "La IP contiene octetos vacíos.")
IpAdressErrors.Add (OctectIsNotNumber, "La IP contiene octetos que no son números.")
IpAdressErrors.Add (OctectOutOfRange, "La IP contiene octetos fuera de rango.")
IpAdressErrors.Add (OctectIncorrectCount, "La IP tiene un número erróneo de octetos.")

let getValidatorsList () =
    [|
        checkEmptyTry IpAdressErrors
        checkOctectsForSpacesOrEmptyTry IpAdressErrors
        checkOctectCountTry IpAdressErrors 4
        checkOctectsAreIntsInRangeTry IpAdressErrors
    |]
