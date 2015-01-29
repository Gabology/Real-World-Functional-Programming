open System.IO
open System.IO.Compression
open System.Net
open System.Web
open System

// LINQ to XML helper methods

#r "System.Xml.dll"
#r "System.Xml.Linq.dll"
open System.Xml.Linq

let wb = "http://www.worldbank.org"
let xAttr s (el:XElement) =
    el.Attribute(XName.Get(s)).Value
let xElem s (el:XContainer) =
    el.Element(XName.Get(s, wb))
let xValue (el:XElement) =
    el.Value
let xElems s (el:XContainer) =
    el.Elements(XName.Get(s, wb))

let xNested path (el:XContainer) = 
    let res = path |> Seq.fold (fun xn s ->
        let child = xElem s xn
        child :> XContainer) el
    res :?> XElement

/////////////////////////////////////

let downloadUrl(url:string) = 
    async { // Tbh there should be pattern matching here since 
            // 'Create' is a factory method that could return different
            // WebRequests depending on the input URL, so we would have to 
            // Type checking in a pattern match to make sure the cast is safe 
            let request = HttpWebRequest.Create(url) :?> HttpWebRequest
            do request.AutomaticDecompression <- DecompressionMethods.GZip
            use! response = request.AsyncGetResponse()
            let stream = response.GetResponseStream()
            use reader = new StreamReader(stream, Text.Encoding.UTF8)
            return! Async.AwaitTask <| reader.ReadToEndAsync() }

let worldBankUrl(functions, props) =
    seq { yield "http://api.worldbank.org"
          for item in functions do
            yield "/" + HttpUtility.UrlEncode(item:string)
          yield "?per_page=100" 
          for key, value in props do
            yield "&" + key + "=" + HttpUtility.UrlEncode(value:string) }
    |> String.Concat

let worldBankDownload(properties) = 
    let url = worldBankUrl(properties)
    let rec loop attempts = 
        async { try
                 return! downloadUrl(url)
                with _ when attempts > 0 ->
                    do printfn "Failed, retrying (%d): %A" attempts properties
                    do! Async.Sleep 500
                    return! loop (attempts - 1) }
    loop 20

let worldBankRequest(props) = 
    async { let! text = worldBankDownload(props)
            return XDocument.Parse(text) }

let regions =
    seq { let props = ["region"],[]
          let doc = 
            Async.RunSynchronously (worldBankRequest props)
            |> xElem "regions"
          for entry in doc |> xElems "region" do
            yield entry |> xElem "name" |> xValue   
     }

let rec getIndicatorData(date, indicator, page) = 
    async { let args = ["countries"; "indicators"; indicator],
                       ["date", date; "page", string page]
            let! doc = worldBankRequest args
            let pages = 
                doc |> xNested ["data"] |> xAttr "pages" |> int
            if pages = page then
                return [doc]
            else
                let! rest = getIndicatorData(date, indicator, page + 1)
                return doc::rest
    }

let downloadAll = 
    seq { for ind in [ "AG.SRF.TOTL.K2"; "AG.LND.FRST.ZS"] do
            for year in [ "1990:1990"; "2000:2000"; "2005:2005" ] do
                yield getIndicatorData(year, ind, 1) }

let data = Async.RunSynchronously (Async.Parallel downloadAll)

let readSingleValue parse node = 
    let value = node |> xElem "value" |> xValue
    let country = node |> xElem "country" |> xValue
    let year = node |> xElem "date" |> xValue |> int
    if String.IsNullOrEmpty(value) then []
    else [ (year, country), parse(value) ]

let readValues parse data = 
    seq { for page in data do
            let root = page |> xNested ["data"]
            for node in root |> xElems "data" do
                yield! node |> readSingleValue parse }

[<Measure>] type km
[<Measure>] type h
[<Measure>] type percent

let areas =
    Seq.concat data.[0..2]
    |> readValues (fun a -> float a * 1.0<km^2>)
    |> Map.ofSeq

let forests =
    Seq.concat data.[3..5]
    |> readValues (fun a -> float a * 1.0<percent>)
    |> Map.ofSeq

let calculateForests(area:float<km^2>, forest:float<percent>) =
    let forestArea = forest * area
    forestArea / 100.0<percent>

let years = [ 1990; 2000; 2005 ]
let dataAvailable key = 
    years |> Seq.forall (fun yr -> 
        (Map.containsKey (yr, key) areas) &&
        (Map.containsKey (yr, key) forests))

let getForestData key = 
    [| for yr in years do
        yield calculateForests(areas.[yr, key], forests.[yr, key]) |]

let stats = 
    seq { for name in regions do 
            if dataAvailable name then
               yield name, getForestData name }

/////////////////////////////////
/// EXCEL VISUALIZATION BELOW //
///////////////////////////////

#r "office.dll"
#r "Microsoft.Office.Interop.Excel.dll"
open System
open Microsoft.Office.Interop.Excel

let app = new ApplicationClass(Visible = true)
let workbook = app.Workbooks.Add(XlWBATemplate.xlWBATWorksheet)
let worksheet = (workbook.Worksheets.[1] :?> _Worksheet)

worksheet.Range("C2", "E2").Value2 <- [| 1990; 2000; 2005 |]

let statsArray = stats |> Array.ofSeq
let names = Array2D.init statsArray.Length 1 (fun index _ ->
    let name, _ = statsArray.[index]
    name)

let dataArray = Array2D.init statsArray.Length 3 (fun index year -> 
    let _, values = statsArray.[index]
    let yearValue = values.[year]
    yearValue / 1000000.0)

let endColumn = string (statsArray.Length + 2)

worksheet.Range("B3", "B" + endColumn).Value2 <- names
worksheet.Range("C3", "E" + endColumn).Value2 <- dataArray

let chartObjects = (worksheet.ChartObjects() :?> ChartObjects)
let chartObject = chartObjects.Add(500.0, 20.0, 550.0, 350.0)

do chartObject.Chart.ChartWizard
    (Title = "Area covered by forests",
     Source = worksheet.Range("B2", "E" + endColumn),
     Gallery = XlChartType.xl3DColumn,
     PlotBy = XlRowCol.xlColumns,
     SeriesLabels = 1, 
     CategoryLabels = 1,
     CategoryTitle = "", 
     ValueTitle = "Forests (mil km^2)")

chartObject.Chart.ChartStyle <- 5