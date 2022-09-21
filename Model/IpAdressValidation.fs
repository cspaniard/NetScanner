module NetScanner.Model.IpAddressValidation

open Model.ValidationHelper

let getValidatorsList () =
    [|
        checkEmpty
        checkOctectCount 4
        checkOctectAreIntsInRange
    |]
