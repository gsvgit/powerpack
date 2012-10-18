open Tree
open System.IO
open Microsoft.FSharp.Text

let tokenize = ref false

let usage = [ ArgInfo("--tokens", ArgType.Set tokenize, "tokenize the first file and exit") ]

let inputs = ref []

let _ = ArgParser.Parse(usage, (fun x -> inputs := !inputs @ [x]), "test... <filename> <filename>\nTests that all inputs give equivalent syntac trees")

let main() = 
  if !inputs = [] then
    Printf.eprintf "at least one input should be given\n";
  try 
    let results = 
      List.map
        (fun filename -> 
          use is = new BinaryReader(File.Open(filename, FileMode.Open))
          let lexbuf = Lexing.LexBuffer<_>.FromBinaryReader is
          if !tokenize then
            while true do 
              Printf.eprintf "tokenize - getting one token\n"
              let t = TestLexer.token lexbuf
              Printf.eprintf "tokenize - got %s, now at char %d\n" (TestParser.token_to_string t) (lexbuf.StartPos).pos_cnum;
              if t = TestParser.EOF then exit 0
            done;
          let tree = 
            try TestParser.start TestLexer.token lexbuf 
            with e -> 
              Printf.eprintf "%s(%d,%d): error: %s\n" filename (lexbuf.StartPos).pos_lnum ((lexbuf.StartPos).pos_cnum -  (lexbuf.StartPos).pos_bol) (match e with Failure s -> s | _ -> e.Message);
              exit 1
          Printf.eprintf "parsed %s ok\n" filename
          is.Close()
          (filename,tree))
        !inputs in 
    List.iter 
      (fun (filename1,tree1) -> 
        List.iter 
          (fun (filename2,tree2) -> 
            if filename1 > filename2 then 
              if tree1 <> tree2 then 
                  Printf.eprintf "file %s and file %s parsed to different results!\n" filename1 filename2;
                  let rec ptree os (Node(n,l)) = Printf.fprintf os "(%s %a)" n ptrees l
                  and ptrees os l = match l with [] -> () | [h] -> ptree os h | h::t -> Printf.fprintf os "%a %a" ptree h ptrees t in 
                  Printf.eprintf "file %s = %a\n" filename1 ptree tree1;
                  Printf.eprintf "file %s = %a\n" filename2 ptree tree2;
                  exit 1;)
          results)
      results;
  with e -> 
    Printf.eprintf "Error: %s\n" (match e with Failure s -> s | e -> e.ToString());
    exit 1

let _ = main ()