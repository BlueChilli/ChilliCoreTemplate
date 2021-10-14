@echo OFF
IF [%5] EQU [] goto :noparameter
cmd /c "del *.temp /s /q 2>NUL" >NUL

echo Creating passwords...
cmd /c "mkdir output" 2>NUL

cmd /c "__randomKeys.bat"

SET /P runID=<./output/runid.temp
SET /p devpasswordkey=<./output/passwordkey_16_1.temp
SET /p stgpasswordkey=<./output/passwordkey_16_2.temp
SET /p prodpasswordkey=<./output/passwordkey_16_3.temp

SET /p devpasswordvalue=<./output/passwordvalue_24_1.temp
SET /p stgpasswordvalue=<./output/passwordvalue_24_2.temp
SET /p prodpasswordvalue=<./output/passwordvalue_24_3.temp

SET keypairname=KeyPair_%5_%runID%

cmd /c "del *.temp /s /q 2>NUL" >NUL

cmd /s /c "__bootstrapCLI.bat %1 %2 %3 "aws ssm put-parameter --name KEY_%devpasswordkey% --type "SecureString" --value %devpasswordvalue%"" >NUL
cmd /s /c "__bootstrapCLI.bat %1 %2 %3 "aws ssm put-parameter --name KEY_%stgpasswordkey% --type "SecureString" --value %stgpasswordvalue%"" >NUL
cmd /s /c "__bootstrapCLI.bat %1 %2 %3 "aws ssm put-parameter --name KEY_%prodpasswordkey% --type "SecureString" --value %prodpasswordvalue%"" >NUL

echo Master dev DB password: %devpasswordvalue% >>./output/DatabaseKeys_%runID%.txt
echo Master stg DB password: %stgpasswordvalue% >>./output/DatabaseKeys_%runID%.txt
echo Master prod DB password: %prodpasswordvalue% >>./output/DatabaseKeys_%runID%.txt

echo Database keys stored in file ./output/DatabaseKeys_%runID%.txt

echo.
echo Creating Key Pair...
cmd /s /c "__bootstrapCLI.bat %1 %2 %3 "aws ec2 create-key-pair --key-name %keypairname%" ./output/%keypairname%.json" >NUL
echo EC2 Key Pair stored in file ./output/%keypairname%.json
echo.
echo Extracting Key to Pem file
cmd /s /c "powershell.exe ./unpack-key.ps1 -keyFile output/%keypairname%.json"

echo.

(
   start cmd /s /c "__bootstrapCLI.bat %1 %2 %3 "aws cloudformation deploy --template-file ./dataformation.json --stack-name DataDev --capabilities CAPABILITY_NAMED_IAM --parameter-overrides ProjectName=%5 DBPasswordKey=KEY_%devpasswordkey%:1 Env=Develop" & pause"

   start cmd /s /c "__bootstrapCLI.bat %1 %2 %3 "aws cloudformation deploy --template-file ./webformation.json --stack-name WebDev --capabilities CAPABILITY_NAMED_IAM --parameter-overrides KeyPairName=%keypairname% HealthCheckTarget= MaxAutoScalingSize=1 S3Bucket=chillibucket Build=SampleSite Env=Develop" & pause"

   start cmd /s /c "__bootstrapCLI.bat %1 %2 %3 "aws cloudformation deploy --template-file ./dataformation.json --stack-name DataStg --capabilities CAPABILITY_NAMED_IAM --parameter-overrides ProjectName=%5 DBPasswordKey=KEY_%stgpasswordkey%:1 Env=Staging" & pause"

   start cmd /s /c "__bootstrapCLI.bat %1 %2 %3 "aws cloudformation deploy --template-file ./webformation.json --stack-name WebStg --capabilities CAPABILITY_NAMED_IAM --parameter-overrides KeyPairName=%keypairname% HealthCheckTarget= MaxAutoScalingSize=1 S3Bucket=chillibucket Build=SampleSite Env=Staging" & pause"

   start cmd /s /c "__bootstrapCLI.bat %1 %2 %3 "aws cloudformation deploy --template-file ./dataformation.json --stack-name DataProd --capabilities CAPABILITY_NAMED_IAM --parameter-overrides ProjectName=%5 DBPasswordKey=KEY_%prodpasswordkey%:1 Env=Production" & pause"

   start cmd /s /c "__bootstrapCLI.bat %1 %2 %3 "aws cloudformation deploy --template-file ./webformation.json --stack-name WebProd --capabilities CAPABILITY_NAMED_IAM --parameter-overrides KeyPairName=%keypairname% HealthCheckTarget= MaxAutoScalingSize=1 S3Bucket=chillibucket Build=SampleSite Env=Production" & pause"
) | pause

echo Fetching stack outputs...

(
__GetStackOutputs.bat %1 %2 %3 DataDev
__GetStackOutputs.bat %1 %2 %3 WebDev
__GetStackOutputs.bat %1 %2 %3 DataStg
__GetStackOutputs.bat %1 %2 %3 WebStg
__GetStackOutputs.bat %1 %2 %3 DataProd
__GetStackOutputs.bat %1 %2 %3 WebProd
)

goto :success
:noparameter
@echo OFF
echo Command requires 5 parameters:
echo key secret region vpcid projectname
echo - projectname MUST be lowercase (used to generate S3 buckets)
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