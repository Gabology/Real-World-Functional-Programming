open System
open System.Drawing
open System.Windows.Forms

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

type FilePath = string
type TextContent = { Text: string; Font: Font }
type ScreenElement = 
    | TextElement of TextContent * Rect
    | ImageElement of FilePath * Rect

let fntText = new Font("Calibri", 12.0f)
let fntHead = new Font("Calibri", 15.0f)

let elements = 
    [ TextElement
         ({ Text = "Functional Programming for the Real World"; Font = fntHead }, 
          { Left = 5.0f; Top = 0.0f; Width = 410.0f; Height = 30.0f });
      ImageElement
         ("cover.jpg", 
          { Left = 120.0f; Top = 30.0f; Width = 150.0f; Height = 200.0f });
      TextElement
        ({ Text = "In this book, we'll introduce you to the essential "+
                  "concepts of functional programming, but thanks to the .NET "+ 
                  "Framework, we won't be limited to theoretical examples. We'll "+ 
                  "use many of the rich .NET libraries to show how functional "+
                  "programming can be used in the real-world."; 
        Font = fntText }, 
        { Left = 10.0f; Top = 230.0f; Width = 400.0f; Height = 400.0f }) ]

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

let docImage = drawImage (450, 400) 20.0f (drawElements elements)
let mainForm = new Form(Text = "Document", BackgroundImage = docImage, 
                    Width = docImage.Width, Height = docImage.Height)

[<STAThread>]
do
    Application.Run(mainForm)