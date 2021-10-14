@echo OFF

: set PARAMn and count parameters
SET /a paramcount=1
:paramloop
SET "param%paramcount%=%~1"
IF DEFINED param%paramcount% SET /a paramcount+=1&shift&GOTO paramloop
SET /a paramcount -=1

IF [%param10%] EQU [] goto :noparameter

echo "AWS.Key=%param1%"
echo "AWS.SecretKey=%param2%"
echo "AWS.Region=%param3%"
echo "Size=%param4%"
echo "AgentURL=%param5%"
echo "Pat=%param6%"
echo "Pool=%param7%"
echo "Agent=%param8%"
echo "VPCID=%param9%"
echo "SubnetId=%param10%"

cmd /c "del *.temp /s /q 2>NUL" >NUL

echo Creating passwords...
cmd /c "mkdir output" 2>NUL

cmd /c "__randomKeys.bat"
SET /P runID=<./output/runid.temp
SET keypairname=KeyPair_ci_%runID%

cmd /c "del *.temp /s /q 2>NUL" >NUL


echo.
echo Creating Key Pair...
cmd /s /c "__bootstrapCLI.bat %param1% %param2% %param3% "aws ec2 create-key-pair --key-name %keypairname%" ./output/%keypairname%.json" >NUL
echo EC2 Key Pair stored in file ./output/%keypairname%.json

echo.
echo Extracting Key to Pem file
cmd /s /c "powershell.exe ./unpack-key.ps1 -keyFile output/%keypairname%.json"

echo.
(
   start cmd /s /c "__bootstrapCLI.bat %param1% %param2% %param3% "aws cloudformation deploy --template-file ./cicdformation.json --stack-name BuildServer --capabilities CAPABILITY_NAMED_IAM --parameter-overrides KeyPairName=%keypairname% Size=%param4% AgentURL=%param5% Pat=%param6% Pool=%param7% Agent=%param8% VpcId=%param9% SubnetId=%param10%" & pause"

) | pause

echo Fetching stack outputs...

(
  __bootstrapCLI.bat  %param1% %param2% %param3% "aws cloudformation describe-stacks --stack-name BuildServer --query Stacks[0].Outputs" .\output\CI_Outputs.txt
)

goto :success
:noparameter
@echo OFF
echo Command requires 8 parameters:
echo key secret region size agent_url pat pool agent vpcid subnetId
goto :end
:error
@echo OFF
echo Error level: %errorlevel%
goto :end
:success
@echo OFF
echo.
echo Finished.
:end
SET AWS_ACCESS_KEY_ID=
SET AWS_SECRET_ACCESS_KEY=
SET AWS_DEFAULT_REGION=