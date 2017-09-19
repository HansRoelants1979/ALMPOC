@echo off
SET CrmDeploymentType="3"

if %CrmDeploymentType%=="1" goto :ActiveDirectory
if %CrmDeploymentType%=="2" goto :Federation
if %CrmDeploymentType%=="3" goto :Online

:ActiveDirectory
ECHO Registering plugin assembly Niko.Plugin.dll and steps
C:\Infront\CRM2016\bin\CmdPluginRegistrationTool.exe "/CRMCONNECTION:Url=https://nikogroupdev.crm4.dynamics.com; authtype=AD" "/ASSEMBLYNAME:../bin/Release/Niko.Plugin.dll; "
goto :End

:Federation
C:\Infront\CRM2016\bin\CmdPluginRegistrationTool.exe "/CRMCONNECTION:Url=https://nikogroupdev.crm4.dynamics.com;Username=infront@nikogroup.onmicrosoft.com;Password=11nFr0nt; authtype=IFD" "/ASSEMBLYNAME:../bin/Release/Niko.Plugin.dll"
goto :End

:Online
ECHO Registering plugin assembly Niko.Plugin.dll and steps
C:\Infront\CRM2016\bin\CmdPluginRegistrationTool.exe "/CRMCONNECTION:Url=https://nikogroupdev.crm4.dynamics.com;Username=infront@nikogroup.onmicrosoft.com;Password=11nFr0nt; authtype=Office365" "/ASSEMBLYNAME:../bin/Release/Niko.Plugin.dll"

:End
pause