@echo OFF
IF [%4] EQU [] goto :noparameter
cmd /c "mkdir output" 2>NUL

cmd /s /c "__bootstrapCLI.bat %1 %2 %3 "aws cloudformation deploy --template-file ./commonformation.json --stack-name Common --capabilities CAPABILITY_NAMED_IAM --parameter-overrides VpcId=%4""

goto :end
:noparameter
@echo OFF
echo Command requires 4 parameters:
echo key secret region vpcid
goto :end
:end
SET AWS_ACCESS_KEY_ID=
SET AWS_SECRET_ACCESS_KEY=
SET AWS_DEFAULT_REGION=