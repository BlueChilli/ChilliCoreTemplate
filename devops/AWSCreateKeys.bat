@echo OFF
IF [%3] EQU [] goto :noparameter
cmd /c "mkdir output" 2>NUL

echo Creating Access Keys...

cmd /s /c "__bootstrapCLI.bat %1 %2 %3 "aws iam create-access-key --user-name S3User-dev" ./output/S3User-dev_AccessKey.json" >NUL
cmd /s /c "__bootstrapCLI.bat %1 %2 %3 "aws iam create-access-key --user-name S3User-stg" ./output/S3User-stg_AccessKey.json" >NUL
cmd /s /c "__bootstrapCLI.bat %1 %2 %3 "aws iam create-access-key --user-name S3User-prod" ./output/S3User-prod_AccessKey.json" >NUL
cmd /s /c "__bootstrapCLI.bat %1 %2 %3 "aws iam create-access-key --user-name SESUser" ./output/SESUser_AccessKey.json" >NUL
cmd /s /c "AwsSmtpCredential.exe -file ./output/SESUser_AccessKey.json > ./output/SESUser_SMTPPassword.txt" >NUL

echo.
echo Keys created at ./output/

goto :end
:noparameter
@echo OFF
echo Command requires 3 parameters:
echo key secret region
goto :end
:error
@echo OFF
echo Error level: %errorlevel%
goto :end
:end
SET AWS_ACCESS_KEY_ID=
SET AWS_SECRET_ACCESS_KEY=
SET AWS_DEFAULT_REGION=