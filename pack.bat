set basedir=%~dp0
set /p version=<%basedir%\PACKAGE_VERSION
dotnet restore %basedir%\src\CodeQualityProfile.Policy
dotnet build %basedir%\src\CodeQualityProfile.Policy /target:PackProfile /property:ProfilePackageId=%1 /property:ProfilePackageVersion=%version%