; =====================================================================
; Inno Setup Script for WinTweakStudio
; Open this file in Inno Setup Compiler and click 'Build -> Compile'
; =====================================================================

[Setup]
AppId={{9F8A8D5B-3B12-4C67-B9C0-8E15D4398959}
AppName=WinTweakStudio
AppVersion=1.0.0
AppPublisher=ShadownCore
DefaultDirName={autopf}\WinTweakStudio
DefaultGroupName=WinTweakStudio
SetupIconFile=d:\MyProject\WinTweakStudio\Icon\App_Icon.ico
UninstallDisplayIcon={app}\WinTweakStudio.exe
Compression=lzma2/ultra64
SolidCompression=yes
OutputDir=.\InstallerOutput
OutputBaseFilename=WinTweakStudio_Setup_v1.0
PrivilegesRequired=admin

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "d:\MyProject\WinTweakStudio\bin\Debug\net8.0-windows\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\WinTweakStudio"; Filename: "{app}\WinTweakStudio.exe"
Name: "{autodesktop}\WinTweakStudio"; Filename: "{app}\WinTweakStudio.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\WinTweakStudio.exe"; Description: "{cm:LaunchProgram,WinTweakStudio}"; Flags: nowait postinstall skipifsilent
