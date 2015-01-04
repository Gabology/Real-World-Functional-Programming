type Client = 
    { Name           : string 
      Income         : int
      YearsInJob     : int
      UsesCreditCard : bool
      CriminalRecord : bool}

type ClientTest = 
    { Check  : Client -> bool
      Report : Client -> unit }

type QueryInfo =
    { Title    : string
      Check    : Client -> bool
      Positive : Decision
      Negative : Decision }

and Decision = 
    | Result of string
    | Query  of QueryInfo

let rec tree = 
    Query { Title = "More than $40k"
            Check = (fun cl -> cl.Income > 40000)
            Positive = moreThan40; Negative = lessThan40 }

and moreThan40 = 
    Query { Title = "Has criminal record"
            Check = (fun cl -> cl.CriminalRecord)
            Positive = Result "NO"; Negative = Result "YES" }

and lessThan40 = 
    Query { Title = "Years in job" 
            Check = (fun cl -> cl.YearsInJob > 1) 
            Positive = Result "YES"; Negative = usesCredit }

and usesCredit =
    Query { Title = "Uses credit card" 
            Check = (fun cl -> cl.UsesCreditCard) 
            Positive = Result "YES"; Negative = Result "No" }

let checkCriminal client = client.CriminalRecord = true
let reportCriminal client = 
    printfn "Checking 'criminal record' of '%s' failed!" client.Name

let lessThanTest readFunc minValue propertyName = 
    let report client = 
        printfn "Checking '%s' of '%s' failed (less than %d)"
                 propertyName client.Name minValue
    { Check  = (fun client -> readFunc client < minValue) 
      Report = report }

let checkIncome client = client.Income < 30000
let reportIncome client = 
    printfn "Checking of 'income' of '%s' failed (%s)"
            client.Name "less than 30000"

let checkJobYears client = client.YearsInJob < 2
let reportJobYears client = 
    printfn "Checking 'years in the job' of '%s' failed (%s)"
            client.Name "less than 2"

let john = { Name = "John Doe"; Income = 40000; YearsInJob = 1
             UsesCreditCard = true; CriminalRecord = false }

let rec testClientTree client tree = 
    match tree with 
    | Result(message) ->
        printfn " OFFER A LOAN: %s" message
    | Query(qInfo) ->
        let result, case =
            if qInfo.Check(client) then "yes", qInfo.Positive
            else "no", qInfo.Negative
        do printfn " - %s? %s" qInfo.Title result
        testClientTree client case

let testsWithReports = 
    [ lessThanTest (fun client -> client.Income) 30000 "income";
      lessThanTest (fun client -> client.YearsInJob) 2 "years in the job" ]

let testClientWithReports client = 
    let issues = 
        testsWithReports
        |> List.filter (fun tr -> tr.Check client)
    let suitable = issues.Length <= 1
    issues |> List.iter (fun tr -> tr.Report client)
    printfn "Offer loan: %s"
            (if suitable then "YES" else "NO")