cls
SET DIR=%~dp0%
IF NOT EXIST "%DIR%log" MKDIR "%DIR%log"
"%PROGRAMFILES(X86)%\MSBuild\14.0\Bin\msbuild.exe" /target:UpdateNuGet /v:d "%DIR%/restorepackages.proj" /logger:FileLogger,Microsoft.Build.Engine;LogFile=%DIR%/log/updatenuget.log
pause