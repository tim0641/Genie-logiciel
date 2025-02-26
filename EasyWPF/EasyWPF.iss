
[Setup]
AppName=EasyWPF
AppVersion=1.0
DefaultDirName={pf}\EasyWPF
DefaultGroupName=EasyWPF
OutputDir=.\Output
OutputBaseFilename=EasyWPF_Setup
Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin



[Files]
Source: "C:\Users\jpvin\source\repos\Genie-logiciel\EasyWPF\bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: recursesubdirs



[Icons]
Name: "{group}\EasyWPF"; Filename: "{app}\EasyWPF.exe"

[Run]
Filename: "{app}\EasyWPF.exe"; Description: "Lancer EasyWPF"; Flags: nowait postinstall