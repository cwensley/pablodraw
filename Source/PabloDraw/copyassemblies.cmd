
set BASEDIR=..\..
set SOLUTIONDIR=..


echo Destination: %2

call :CopyBase "%~1" "%~2" Libraries\Eto BuildOutput\net45 Eto*.*

call :CopyBase "%~1" "%~2" Libraries\lidgren Lidgren.Network\bin Lidgren.Network.*
call :CopyBase "%~1" "%~2" Libraries\Mono.Nat\src Mono.Nat\bin Mono.Nat.*
call :CopyFile "%~1" "%~2" Pablo Pablo.dll
call :CopyFile "%~1" "%~2" Pablo.Interface Pablo.Interface.dll


goto :eof


:CopyBase

echo Copying: %~4
copy "%BASEDIR%\%~3\%~4\%~1\%~5" "%~2"
goto :eof

:CopyFile

echo Copying: %~4
copy "%SOLUTIONDIR%\%~3\bin\%~1\%~4" "%~2"
goto :eof
