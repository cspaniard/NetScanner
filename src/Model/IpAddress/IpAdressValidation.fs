module Model.IpAddressValidation

open Model.ValidationHelper

let IpAdressErrors = ErrorDict ()
let [<Literal>] PREFIX = "IpAddress ({0}):"

IpAdressErrors.Add (ValueIsEmpty, $"{PREFIX} El valor de la IP está vacío.")
IpAdressErrors.Add (ValueContainsSpaces, $"{PREFIX} La IP proporcionada contiene espacios.")
IpAdressErrors.Add (OctectIsEmpty, $"{PREFIX} La IP contiene octetos vacíos.")
IpAdressErrors.Add (OctectIsNotNumber, $"{PREFIX} La IP contiene octetos que no son números.")
IpAdressErrors.Add (OctectOutOfRange, $"{PREFIX} La IP contiene octetos fuera de rango.")
IpAdressErrors.Add (OctectIncorrectCount, $"{PREFIX} La IP tiene un número erróneo de octetos.")

let getValidatorsList () =
    [|
        checkEmptyTry IpAdressErrors
        checkOctectsForSpacesOrEmptyTry IpAdressErrors
        checkOctectCountTry IpAdressErrors 4
        checkOctectsAreIntsInRangeTry IpAdressErrors
    |]
