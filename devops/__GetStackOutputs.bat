@echo OFF
IF [%4] EQU [] goto :noparameter

__bootstrapCLI.bat %1 %2 %3 "aws cloudformation describe-stacks --stack-name %4 --query Stacks[0].Outputs" .\output\%4.Outputs.txt
goto :end
:noparameter
@echo OFF
echo Command requires 4 parameters:
echo key secret region stackname
:end