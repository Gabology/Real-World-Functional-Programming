open System
open System.Drawing
open System.Windows.Forms
open System.Xml.Linq

type Rect = 
    { Left   : float32
      Top    : float32
      Width  : float32
      Height : float32 } 
      with   
        member this.deflate wspace hspace = 
            { Left = this.Left + wspace
              Top = this.Top + hspace
              Width = this.Width - (2.0f * wspace)
              Height = this.Height - (2.0f * hspace) }
        
        member this.toRectangleF() = 
            RectangleF(this.Left, this.Top, this.Width, this.Height)

type Orientation = Vertical | Horizontal
type FilePath = string
type TextContent = {Text: string; Font: Font}

type ScreenElement = 
    | TextElement of TextContent * Rect
    | ImageElement of FilePath * Rect

type DocumentPart = 
    | SplitPart  of Orientation * DocumentPart list
    | TitledPart of TextContent * DocumentPart
    | TextPart   of TextContent
    | ImagePart  of FilePath

let rec documentToScreen(doc, bounds:Rect) = 
    match doc with
    | SplitPart(Horizontal, parts) ->
        let width  = bounds.Width / float32 parts.Length
        parts |> List.mapi 
            (fun i part ->
                let left = bounds.Left + float32 i * width
                let bounds = { bounds with Left = left; Width = width }
                documentToScreen(part, bounds))
        |> List.concat
    | SplitPart(Vertical, parts) ->
        let height = bounds.Height / float32 parts.Length
        parts |> List.mapi
            (fun i part ->
                let top = bounds.Top + float32 i * height
                let bounds = { bounds with Top = top; Height = height }
                documentToScreen(part, bounds))
        |> List.concat
    | TitledPart(tx, content) -> 
        let titleBounds = { bounds with Height = 35.0f }
        let restBounds = { bounds with Height = bounds.Height - 35.0f;
                                       Top = bounds.Top + 35.0f }
        let convertedBody = documentToScreen(content, restBounds)
        TextElement(tx, titleBounds)::convertedBody
    | TextPart(tx) -> [ TextElement(tx, bounds) ]
    | ImagePart(im) -> [ ImageElement(im, bounds) ]

type XElement with 
    member this.attr(name, defaultValue) =
        let attr = this.Attribute(XName.Get(name))
        if attr <> null then attr.Value else defaultValue

let parseOrientation (node:XElement) = 
    match node.attr("orientation", String.Empty) with
    | "horizontal" -> Horizontal
    | "vertical" -> Vertical
    | _ -> failwith "Unknown orientation"

let parseFont (node:XElement) =
    let style = node.attr("style", String.Empty)
    let style = 
        match (style.Contains("bold"), style.Contains("italic")) with
        | true, false  -> FontStyle.Bold
        | false, true  -> FontStyle.Italic
        | true, true   -> FontStyle.Bold ||| FontStyle.Italic
        | false, false -> FontStyle.Regular
    let name = node.attr("font", "Calibri")
    new Font(name, float32 (node.attr("size", "12")), style)

let rec loadPart (node:XElement) = 
    match node.Name.LocalName with
    | "titled" ->
        let header = { Text = node.attr("title", String.Empty); Font = parseFont node }
        let body = loadPart (Seq.head (node.Elements())) 
        TitledPart(header, body)
    | "split" ->
        let orientation = parseOrientation node
        let nodes = node.Elements() |> List.ofSeq |> List.map loadPart
        SplitPart(orientation, nodes)
    | "text" ->
        TextPart({Text = node.Value; Font = parseFont node})
    | "image" ->
        ImagePart(node.attr("filename", String.Empty))
    | otherNode -> failwith ("Unknown node: " + otherNode)

let rec mapDocument f docPart = 
    let processed = 
        match docPart with
        | TitledPart(tx, content) ->
            TitledPart(tx, mapDocument f content)
        | SplitPart(orientation, parts) ->
            let mappedParts = parts |> List.map (mapDocument f)
            SplitPart(orientation, mappedParts)
        | _ -> docPart
    f processed

let isText part = 
    match part with | TextPart(_) -> true | _ -> false

let shrinkDocument part = 
    match part with
    | SplitPart (_, parts) when List.forall isText parts ->
        let res = 
            List.fold (fun acc (TextPart(current)) -> 
                { Text = acc.Text + " " + current.Text
                  Font = current.Font }) 
                  { Text = String.Empty; Font = null } parts
        TextPart(res)
    | otherPart -> otherPart

let rec aggregateDocument f state docPart = 
    let state = f state docPart
    match docPart with
    | TitledPart(_, part) ->
        aggregateDocument f state part
    | SplitPart(_, parts) ->
        List.fold (aggregateDocument f) state parts
    | _ -> state

let totalWords doc = 
    aggregateDocument (fun count part -> 
        match part with
        | TextPart(tx) | TitledPart (tx, _) -> 
            count + tx.Text.Split(' ').Length
        | _ -> count) 0 doc

let drawElements elements (gr:Graphics) = 
    elements |> List.iter
       (function
        | TextElement(text, boundingBox) ->
            let boxf = boundingBox.toRectangleF()
            gr.DrawString(text.Text, text.Font, Brushes.Black, boxf)
        | ImageElement(imagePath, boundingBox) ->
            let bmp = new Bitmap(imagePath)
            let wspace, hspace = (boundingBox.Width / 10.0f, boundingBox.Height / 10.0f)
            let rc = (boundingBox.deflate wspace hspace).toRectangleF()
            gr.DrawImage(bmp, rc))

let drawImage (width:int, height:int) space drawingFunc = 
    let bmp = new Bitmap(width, height)
    use gr = Graphics.FromImage(bmp)
    do
        gr.Clear(Color.White)
        gr.TranslateTransform(space, space)
        drawingFunc gr
    bmp

[<STAThread>]
do
    let doc = loadPart (XDocument.Load(@"..\..\document.xml").Root)
    let shrinkedDoc = doc |> mapDocument shrinkDocument
    let bounds = { Left = 0.0f; Top = 0.0f; Width  = 520.0f; Height = 630.0f }
    let parts = documentToScreen(shrinkedDoc, bounds)
    let img = drawImage (570, 680) 25.0f (drawElements parts)
    let main = new Form(Text = "Document", BackgroundImage = img,
                        ClientSize = Size(570,680))
    let x = totalWords shrinkedDoc
    Application.Run(main)
    