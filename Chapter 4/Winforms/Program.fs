open System
open System.Drawing
open System.Windows.Forms
open System.IO


(* GUI Initialization here *)
let mainForm = new Form(Width = 620, Height = 450, Text = "Pie Chart")

let menu = new ToolStrip()
let btnOpen = new ToolStripButton("Open")
let btnSave = new ToolStripButton("Save", Enabled = false)

menu.Items.Add(btnOpen) |> ignore
menu.Items.Add(btnSave) |> ignore

let boxChart =
    new PictureBox
        (BackColor = Color.White, Dock = DockStyle.Fill,
        SizeMode = PictureBoxSizeMode.CenterImage)

mainForm.Controls.Add(menu)
mainForm.Controls.Add(boxChart)

(* Utility functions *)

let rec processLines lines = 
    let convertDataRow (csvLine:string) = 
        let cells = csvLine.Split(',') |> Seq.toList
        match cells with
        | title::number::_ ->
            (title, int number)
        | _ -> failwith "Incorrect data format!"

    match lines with
    | [] -> []
    | currentLine::remaining ->
        let parsedLine = convertDataRow currentLine
        let parsedTail = processLines remaining
        parsedLine::parsedTail

let rnd = new Random()
let randomBrush() =
    let r, g, b = rnd.Next(256), rnd.Next(256), rnd.Next(256)
    new SolidBrush(Color.FromArgb(r,g,b))

let drawPieSegment (gr: Graphics) title startAngle occupiedAngle =
    use br = randomBrush()
    gr.FillPie
        (br, 170, 70, 260, 260,
        startAngle, occupiedAngle)

let drawStep drawingFunc (gr: Graphics) sum data = 
    let rec drawStepUtil data angleSoFar = 
        match data with 
        | [] -> ()
        | [title, value]  ->
            let angle = 360 - angleSoFar
            drawingFunc gr title angleSoFar angle
        | (title, value)::tail ->
            let angle = int(float value / sum * 360.0)
            drawingFunc gr title angleSoFar angle
            drawStepUtil tail (angleSoFar + angle)  
    drawStepUtil data 0

let fnt = new Font("Times new Roman", 11.0f)

let centerX, centerY = 300.0, 200.0
let labelDistance = 150.0

let drawLabel (gr: Graphics) title startAngle angle = 
    let lblAngle = float(startAngle + angle / 2)
    let ra = Math.PI * 2.0 * lblAngle / 360.0
    let x = centerX + labelDistance * cos(ra)
    let y = centerY + labelDistance * sin(ra)
    let text = sprintf "%s (%.2f%c)" title (float angle / 360.0 * 100.0) '%'
    let size = gr.MeasureString(text, fnt)
    let rc = new PointF(float32(x) - size.Width / 2.0f,
                        float32(y) - size.Height / 2.0f)
    gr.DrawString(text, fnt, Brushes.Black, new RectangleF(rc, size))

let drawChart file = 
    let lines = File.ReadLines(file) |> Seq.toList
    let data = processLines lines
    let sum = float (data |> List.sumBy snd)

    let pieChart = new Bitmap(600, 400)
    use gr = Graphics.FromImage(pieChart)
    
    do
        gr.Clear(Color.White)
        drawStep drawPieSegment gr sum data  
        drawStep drawLabel gr sum data  
    pieChart

let openAndDrawChart e = 
    let dlg = new OpenFileDialog(Filter="CSV Files|*.csv")
    if (dlg.ShowDialog() = DialogResult.OK) then
        let pieChart = drawChart dlg.FileName
        boxChart.Image <- pieChart
        btnSave.Enabled <- true

let saveDrawing e =
    let dlg = new SaveFileDialog(Filter="PNG Files|*.png")
    if (dlg.ShowDialog() = DialogResult.OK) then
        boxChart.Image.Save(dlg.FileName)
    

[<STAThread>]
do
    btnOpen.Click.Add(openAndDrawChart)
    btnSave.Click.Add(saveDrawing)
    Application.Run(mainForm)
