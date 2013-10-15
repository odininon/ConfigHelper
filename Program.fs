
// NOTE: If warnings appear, you may need to retarget this project to .NET 4.0. Show the Solution
// Pad, right-click on the project node, choose 'Options --> Build --> General' and change the target
// framework to .NET 4.0 or .NET 4.5.

module ConfigHelper.Main

open System
open System.IO
open System.Collections.Generic

let readFile (filePath:string) = seq {
    use sr = new StreamReader (filePath)
    while not sr.EndOfStream do
        yield sr.ReadLine ()
}

let getFiles (filePath:string) = Directory.GetFiles(filePath, "*.cfg", SearchOption.AllDirectories)

let seqFilter (f:('a -> bool)) (sq:'a seq) = sq |> Seq.filter f

let removeComments = seqFilter (fun (x:string) -> not (x.Contains "#"))
let removeEmptyLines = seqFilter (fun (x:string) -> not (x.Equals ""))

let trimLines (sq:string seq) = sq |> removeComments |> removeEmptyLines

let cleanLines (l:string list) =
    let filtered = l |> Seq.map (fun (s:string) -> s.Substring 4)
    filtered 
    |> Seq.toList

let printCollection targetSet = targetSet |> Seq.iter (fun x -> printfn "%O" x)
 
let getIndex index (s:string) =
    let list = s.Split [|'='|]
    list.[index]

let getValue = getIndex 1 
let getKey = getIndex 0   
    
let newBlock (i:int) (l:string list) =
    l |> List.mapi (fun index s -> (getKey s) + "=" + string (i + index))

let writeNewFile (file:string) (sq:string seq) =
    use wr = new StreamWriter(file, false)
    sq |> Seq.iter (fun l -> wr.WriteLine l)
    wr.Close
    
let sortByValue (s1:string) (s2:string) = String.Compare(getValue s1, getValue s2)

let getSorted (s:string) (sq:string seq) =
    let index = sq |> Seq.findIndex (fun x -> x.Equals (s + " {"))

    let newsq = sq |> Seq.skip (index + 1)
    let indexEnd = newsq |> Seq.findIndex (fun x -> x.Equals "}")
    
    let head = sq |> Seq.take (index + 1)
    let newBlock = newsq |> Seq.take indexEnd |> Seq.toList |> cleanLines |> List.sortWith sortByValue |> Seq.map (fun s -> "    " + s)
    let tail = sq |> Seq.skip (index + Seq.length newBlock + 1)
    
    let headContent = newBlock |> Seq.append head
    tail |> Seq.append headContent

let hasBlock (s:string) (sq:string seq) =
    let index = sq |> Seq.tryFindIndex (fun x -> x.Equals (s + " {"))
    match index with
    | None -> false
    | _ -> true

let sortFile (f:string) =
    let file = trimLines(readFile f)
    printfn "%O" f

    let newFile =
        let blockFile = 
             match hasBlock "block" file with
             | true -> getSorted "block" file
             | false -> file
            
        let itemFile =
             match hasBlock "item" blockFile with
             | true -> getSorted "item" blockFile
             | false -> blockFile
        itemFile
        
                
    printfn "%O" f
//    File.WriteAllLines (f + ".tst", newFile)

let sortFiles (f:string []) =
    f |> Seq.iter (fun f -> sortFile f)

[<EntryPoint>]
let main args = 
    if args.Length <> 1 then
       failwith "Usage: program.exe directory"

    let files = getFiles args.[0]
    sortFiles files
    printfn "%d configs sorted and cleaned up" (Seq.length files)  
    0