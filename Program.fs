
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

let getFiles (filePath:string) = Directory.GetFiles(filePath, @"*.cfg", SearchOption.AllDirectories)

let seqFilter (f:('T -> bool)) (s:seq<'T>) = s |> Seq.filter f

let removeComments = seqFilter (fun (x:string) -> not (x.Contains "#"))
let removeEmptyLines = seqFilter (fun (x:string) -> not (x.Equals ""))

let trimSeq (sequence:seq<string>) = sequence |> removeComments |> removeEmptyLines

let getBlock (s:string) (sq:seq<string>) = 
    let index = sq |> Seq.findIndex (fun (x:string) -> x.Equals (s + " {"))
    let newsq = sq |> Seq.skip (index + 1)
    let indexEnd = newsq |> Seq.findIndex (fun (x:string) -> x.Equals "}")
    newsq |> Seq.take indexEnd 
    
let getValuesFromString (deli : char[] ) (s : string) =
    let index = s.IndexOf ":"
    let cleanString = s.Substring (index + 1) 
    let values = cleanString.Split deli
    values
    
let getValuesCFG = getValuesFromString [| '=' |]
    
let getDictionaryFromSeq sq =
    let dict = Dictionary<string,string>()
    sq |> Seq.iter (fun (x: string) -> dict.Add((getValuesCFG x).[0], (getValuesCFG x).[1]))
    dict
    
let rec getNameFromFile (f:string) = 
    match f.StartsWith "./" with
    | true -> getNameFromFile (f.Substring 2)
    | false -> (f.Split [|'.'|]).[0]
    
let printCollection targetSet =
    targetSet |> Seq.iter (fun x -> printfn "%O" x)
    targetSet |> Seq.length |> printfn "%d"  
    
    
let getDictionaryFromFile (f:string) =
    let file = readFile f
    let trimmedFile = trimSeq file
    let blocks = getBlock "block" trimmedFile
    let items = getBlock "item" trimmedFile
    let modDict = new Dictionary<string,Dictionary<string,string>>()
    modDict.Add("blocks", getDictionaryFromSeq blocks)    
    modDict.Add("items", getDictionaryFromSeq items) 
    modDict
    
[<EntryPoint>]
let main args = 

    let files = getFiles "."
    let modsDict = new Dictionary<string,Dictionary<string,Dictionary<string,string>>>()
    files |> Seq.iter (fun x ->  modsDict.Add(getNameFromFile x, getDictionaryFromFile x))

    0

