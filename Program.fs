
// NOTE: If warnings appear, you may need to retarget this project to .NET 4.0. Show the Solution
// Pad, right-click on the project node, choose 'Options --> Build --> General' and change the target
// framework to .NET 4.0 or .NET 4.5.

module ConfigHelper.Main

open System
open System.IO

let readFile (filePath:string) = seq {
    use sr = new StreamReader (filePath)
    while not sr.EndOfStream do
        yield sr.ReadLine ()
}

let seqFilter (f:('T -> bool)) (s:seq<'T>) = s |> Seq.filter f

let removeComments = seqFilter (fun (x:string) -> not (x.Contains "#"))
let removeEmptyLines = seqFilter (fun (x:string) -> not (x.Equals ""))

let trimSeq (sequence:seq<string>) = sequence |> removeComments |> removeEmptyLines

[<EntryPoint>]
let main args = 
    let file = readFile "BiblioCraft.cfg"
    
    let trimmedFile = trimSeq file
    
    trimmedFile |> Seq.iter (fun elem -> printfn "%s" elem)
    0

