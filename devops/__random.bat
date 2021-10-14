@echo off
IF [%2] EQU [] goto :noparameter

setlocal enableDelayedExpansion
set "chars=abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"
set "name="
for /l %%N in (1 1 %1) do (
  set /a I=!random!%%62
  for %%I in (!I!) do set "name=!name!!chars:~%%I,1!"
)
endlocal & echo %name%%~x1>%2
goto :end
:noparameter
@echo Usage: __random length outputFile
:end