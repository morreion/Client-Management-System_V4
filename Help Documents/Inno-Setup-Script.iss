; -- Inno Setup Script for Client Management System V4 --
; Download Inno Setup from: https://jrsoftware.org/isdl.php

[Setup]
AppId={{D3B3E5C1-4D3A-4FBA-B7C1-309859A30E5B}
AppName=Client Management System V4
AppVersion=1.0
AppPublisher=Private Clinic
DefaultDirName={autopf}\Client Management System V4
DefaultGroupName=Client Management System V4
AllowNoIcons=yes
; Specify the path to your icon
SetupIconFile=..\Images\Health_Solutions-Icon -2.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\bin\Release\net8.0-windows\win-x64\publish\Client-Management-System_V4.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\bin\Release\net8.0-windows\win-x64\publish\client_mgmt_schema.sql"; DestDir: "{app}"; Flags: ignoreversion
; Include the runtimes folder (REQUIRED for SQLite and native dependencies)
Source: "..\bin\Release\net8.0-windows\win-x64\publish\runtimes\*"; DestDir: "{app}\runtimes"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Client Management System V4"; Filename: "{app}\Client-Management-System_V4.exe"; IconFilename: "{app}\Client-Management-System_V4.exe"; WorkingDir: "{app}"
Name: "{autodesktop}\Client Management System V4"; Filename: "{app}\Client-Management-System_V4.exe"; Tasks: desktopicon; IconFilename: "{app}\Client-Management-System_V4.exe"; WorkingDir: "{app}"

[Run]
Filename: "{app}\Client-Management-System_V4.exe"; Description: "{cm:LaunchProgram,Client Management System V4}"; Flags: nowait postinstall skipifsilent; WorkingDir: "{app}"
