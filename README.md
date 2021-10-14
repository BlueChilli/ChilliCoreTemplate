# README #

Base project to clone for a new project. 

Has the Base, EmailAccount, and Api packages installed.

Has full stack example installed.

Note: DO NOT compile or run the app until steps 5 and 6!

1. Pull ChilliSourceTemplate solution and copy into a new directory (not hidden folders .git and .vs). If you have previously compiled ChilliSourceTemplate do a clean so you don't copy binaries.
2. Run ProjectSetup.bat to setup the project name and GUID's.
3. In new solution update all "ChilliSource.*" packages overwriting all files. Do not recommend updating external packages from Nuget. These will be updated in ChilliSourceTemplate. 
If you do update an external package and this tests out fine, please upate ChilliSourceTemplate.
4. Create an Azure file storage account or Aws s3 bucket and update web.config.
5. https://localhost/SolutionName/Example/ and https://localhost/SolutionName/EmailAccount/Login should be working for admin@bluechilli.com / 123456
6. Not a frecl project? Remove the rewrite rule in web.config and use rewrite rule for https in web.deploy.config. Change public controller from redirecing to index.html to the login page eg return Menu.EmailAccount_Login.Redirect();
7. Create a new repo in BitBucket
8. Run the following git commands
git init
git remote add origin https://me@bitbucket.org/bluechilli/myproject.git
git add -A
git commit -m "inflate project"
git push --set-upstream origin master
git branch develop
git checkout develop
git push --set-upstream origin develop
10. In bitbucket change the default branch from master to develop