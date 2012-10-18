namespace FSharp.PowerPack.Unittests

open NUnit.Framework
open System.IO
open System.Diagnostics

[<AutoOpen>]
module Utils =

    let runCmdUtl redErr exe args processResult =
        try
            let proc = new Process()
            let startInfo = new ProcessStartInfo()
            startInfo.WindowStyle <- System.Diagnostics.ProcessWindowStyle.Hidden            
            startInfo.FileName <- exe
            startInfo.Arguments <- args
            startInfo.RedirectStandardOutput <- true            
            startInfo.RedirectStandardError <- redErr
            startInfo.UseShellExecute <- false
            printfn "cmd:\n %s %s" startInfo.FileName startInfo.Arguments            
            proc.StartInfo <- startInfo
            let _ = proc.Start()
            proc.WaitForExit()
            proc.StandardOutput.ReadToEnd() |> printfn "cmd: \n output: %s"
            processResult proc
        with
        | e -> Assert.Fail(e.Message)
    
    let runCmd exe args =
        try
            let procResult (proc:Process) =
                if proc.ExitCode <> 0 
                then  
                    Assert.Fail(
                        exe + " exit code = " + string proc.ExitCode 
                        + "\n Message: " + proc.StandardOutput.ReadToEnd()
                        + "\n Errors: " + proc.StandardError.ReadToEnd())
            runCmdUtl true exe args procResult
        with
        | e -> Assert.Fail(e.Message)
    
    let compareTextFiles (fs1:StreamReader) (fs2:StreamReader) =
        let mutable x = fs1.Read()
        let mutable y = fs2.Read() 
        while x = y && x <> -1 && y <> -1 do
            while (x <- fs1.Read(); x = 13) do ()
            while (y <- fs2.Read(); y = 13) do ()
        x = y

    let compareResult resultFileName compareWithFileName = 
        Assert.True(File.Exists resultFileName)
        if File.Exists compareWithFileName then 
            use crReader = new StreamReader(resultFileName)
            use stReader = new StreamReader(compareWithFileName)
            let result = compareTextFiles crReader stReader
            crReader.Close()
            stReader.Close()
            result
        else false

    let fslex args file = 
        runCmd "fslex.exe" (args + " " + file)
    let fsyacc args file =
        runCmd "fsyacc.exe" (args + " " + file)
    let compile fsc_flags files =
        runCmd "fsc.exe" (fsc_flags + " " + String.concat " " files)
    let runTest test tokenize input errOutput =
        printfn "Run test."
        try
            let args = (if tokenize then " --tokens " else " ") + String.concat " " input
            let procResult (proc:Process) =
                File.WriteAllText(errOutput,proc.StandardError.ReadToEnd())
            runCmdUtl true test args procResult
        with
        | e -> Assert.Fail(e.Message)        

[<TestFixture>]
type FsLexFsYaccTests() =
    let mainFile = "main.fs"
    let treeFile = "tree.fs"
    let baseFscFlags = "--define:NO_INSTALLED_ILX_CONFIGS  -r:System.Core.dll --nowarn:20 --define:COMPILED -r FSharp.PowerPack.dll -r FSharp.PowerPack.Compatibility.dll"
    let testPath dir file = Path.Combine(dir,file)
    let inputPath dir file = Path.Combine(Path.Combine(dir,"input"),file)
    let checkPath dir file = Path.Combine(Path.Combine(dir,"stuff"),file)


    [<Test>]
    member this.test1()=
        let dir = @"../../tests/FsYacc/test1"
        fslex ("--light-off -o " + testPath dir "test1lex.fs") (testPath dir "test1lex.fsl")
        fsyacc ("--light-off --module TestParser -o " + testPath dir "test1.fs") (testPath dir "test1.fsy")
        compile 
            (baseFscFlags + " -g -o:" + "test1.exe") 
            ([treeFile;"test1.fsi";"test1.fs";"test1lex.fs"; mainFile] |> List.map (testPath dir))
        let errOut = testPath dir "test1.input1.tokens.err"        
        runTest "test1.exe" false [inputPath dir "test1.input1"] errOut
        printfn "Compare error output."
        let chPath = checkPath dir "test1.input1.bsl"
        if compareResult errOut chPath
        then Assert.Pass()
        else Assert.Fail(errOut + " is not equal to " + chPath)

        

