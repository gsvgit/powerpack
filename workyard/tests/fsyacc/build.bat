SET FSP_BASE=%~d0%~p0..\..\..\Debug\bin

setlocal
if EXIST build.ok DEL /f /q build.ok

call %~d0%~p0scripts\config.bat
@if ERRORLEVEL 1 goto Error

if NOT "%FSC:NOTAVAIL=X%" == "%FSC%" ( 
  REM Skipping test for FSI.EXE
  goto Skip
)

REM Just copy local all binaries for tests
xcopy /Y %FSP_BASE%"\*.dll"
xcopy /Y %FSP_BASE%"\*.exe"

set fsp_flag=-r FSharp.PowerPack.dll 
set fsp_compat_flag=-r FSharp.PowerPack.Compatibility.dll

REM UNICODE test1-unicode

"%FSLEX%" --light-off -o test1lex.fs test1lex.fsl
@if ERRORLEVEL 1 goto Error

"%FSYACC%" --light-off --module TestParser -o test1.fs test1.fsy
@if ERRORLEVEL 1 goto Error

"%FSC%" %fsc_flags% %fsp_flag% -g -o:test1%ILX_SUFFIX%.exe tree.fs test1.fsi test1.fs test1lex.fs main.fs
@if ERRORLEVEL 1 goto Error

"%FSYACC%" --light-off --module TestParser -o test2.fs test2.fsy
@if ERRORLEVEL 1 goto Error

"%FSC%" %fsc_flags% %fsp_flag% -g -o:test2%ILX_SUFFIX%.exe tree.fs test2.fsi test2.fs test1lex.fs main.fs
@if ERRORLEVEL 1 goto Error

"%FSLEX%" --light-off --unicode -o test1-unicode-lex.fs test1-unicode-lex.fsl
@if ERRORLEVEL 1 goto Error

"%FSYACC%" --light-off --module TestParser -o test1-unicode.fs test1-unicode.fsy
@if ERRORLEVEL 1 goto Error

"%FSC%" %fsc_flags% %fsp_flag% -g -o:test1-unicode%ILX_SUFFIX%.exe tree.fs test1-unicode.fsi test1-unicode.fs test1-unicode-lex.fs main-unicode.fs
@if ERRORLEVEL 1 goto Error



"%FSLEX%" -o test1lex.fs test1lex.fsl
@if ERRORLEVEL 1 goto Error

"%FSYACC%" --module TestParser -o test1.fs test1.fsy
@if ERRORLEVEL 1 goto Error

"%FSC%" %fsc_flags% %fsp_flag% -g -o:test1%ILX_SUFFIX%.exe tree.fs test1.fsi test1.fs test1lex.fs main.fs
@if ERRORLEVEL 1 goto Error

"%FSYACC%" --module TestParser -o test1compat.fs --ml-compatibility test1.fsy
@if ERRORLEVEL 1 goto Error

"%FSC%" %fsc_flags% %fsp_flag% %fsp_compat_flag% -g -o:test1compat%ILX_SUFFIX%.exe tree.fs test1compat.fsi test1compat.fs test1lex.fs main.fs
@if ERRORLEVEL 1 goto Error

"%FSYACC%" --module TestParser -o test2.fs test2.fsy
@if ERRORLEVEL 1 goto Error

"%FSC%" %fsc_flags% %fsp_flag% -g -o:test2%ILX_SUFFIX%.exe tree.fs test2.fsi test2.fs test1lex.fs main.fs
@if ERRORLEVEL 1 goto Error

"%FSYACC%" --module TestParser -o test2compat.fs --ml-compatibility test2.fsy
@if ERRORLEVEL 1 goto Error

"%FSC%" %fsc_flags% %fsp_flag% %fsp_compat_flag% -g -o:test2compat%ILX_SUFFIX%.exe tree.fs test2compat.fsi test2compat.fs test1lex.fs main.fs
@if ERRORLEVEL 1 goto Error

"%FSLEX%" --unicode -o test1-unicode-lex.fs test1-unicode-lex.fsl
@if ERRORLEVEL 1 goto Error

"%FSYACC%" --module TestParser -o test1-unicode.fs test1-unicode.fsy
@if ERRORLEVEL 1 goto Error

"%FSC%" %fsc_flags% %fsp_flag% -g -o:test1-unicode%ILX_SUFFIX%.exe tree.fs test1-unicode.fsi test1-unicode.fs test1-unicode-lex.fs main-unicode.fs
@if ERRORLEVEL 1 goto Error



:Ok
echo Passed fsharp %~f0 ok.
echo. > build.ok
endlocal
exit /b 0


:Skip
echo Skipped %~f0
endlocal
exit /b 0


:Error
call scripts\ChompErr.bat %ERRORLEVEL% %~f0
endlocal
exit /b %ERRORLEVEL%
