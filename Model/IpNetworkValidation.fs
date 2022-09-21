module NetScanner.Model.IpNetworkValidation

open Model.ValidationHelper

let getValidatorsList () =
    [|
        checkEmpty
        checkOctectAreIntsInRange
        checkOctectCount 3
    |]
