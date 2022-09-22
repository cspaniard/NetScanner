module NetScanner.Model.IpNetworkValidation

open Model.ValidationHelper

let getValidatorsList () =
    [|
        checkEmpty
        checkOctectForSpacesOrEmpty
        checkOctectAreIntsInRange
        checkOctectCount 3
    |]
