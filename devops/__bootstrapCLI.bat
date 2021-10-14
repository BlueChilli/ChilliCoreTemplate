@echo OFF
SET AWS_ACCESS_KEY_ID=%1
SET AWS_SECRET_ACCESS_KEY=%2
SET AWS_DEFAULT_REGION=%3

IF [%4] EQU [] goto :noparameter

:: Remove quotes
SET __bootstrapCLICMD=###%4###
SET __bootstrapCLICMD=%__bootstrapCLICMD:"###=%
SET __bootstrapCLICMD=%__bootstrapCLICMD:###"=%
SET __bootstrapCLICMD=%__bootstrapCLICMD:###=%

IF [%5] EQU [] goto :runcommand
%__bootstrapCLICMD% 1>%5
goto :end

:runcommand
@echo ON
%__bootstrapCLICMD%
@echo OFF
goto :end

:noparameter
@echo OFF
echo Command requires 4 parameters:
echo key secret region awscommand (outputFile)
echo  - outputFile is optional.
:end
@echo OFF
SET AWS_ACCESS_KEY_ID=
SET AWS_SECRET_ACCESS_KEY=
SET AWS_DEFAULT_REGION=
SET __bootstrapCLICMD=