module Model.IpAddressValidation

open Model.ValidationHelper

let IpAdressErrors = ErrorDict ()
let [<Literal>] PREFIX = "IpAddress ({0}):"

IpAdressErrors.Add (EmptyValue, $"{PREFIX} El valor de la IP está vacío.")
IpAdressErrors.Add (ValueContainsSpaces, $"{PREFIX} La IP proporcionada contiene espacios.")
IpAdressErrors.Add (EmptyOctects, $"{PREFIX} La IP contiene octetos vacíos.")
IpAdressErrors.Add (NonNumericOctects, $"{PREFIX} La IP contiene octetos que no son números.")
IpAdressErrors.Add (OutOfRangeOctects, $"{PREFIX} La IP contiene octetos fuera de rango.")
IpAdressErrors.Add (IncorrectOctectCount, $"{PREFIX} La IP tiene un número erróneo de octetos.")

let getValidatorsList () =
    [|
        checkEmptyValueTry IpAdressErrors
        checkValueContainsSpacesTry IpAdressErrors
        checkEmptyOctects IpAdressErrors
        checkNonNumericOctectsTry IpAdressErrors
        checkOutOfRangeOctectsTry IpAdressErrors
        checkIncorrectOctectCountTry IpAdressErrors 4
    |]
