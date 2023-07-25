[Setup]
AppName=iRacing-TV
AppVersion=1.14
AppCopyright=Created by Marvin Herbold
AppPublisher=Marvin Herbold
AppPublisherURL=https://herboldracing.com/iracing-tv
WizardStyle=modern
DefaultDirName={autopf}\iRacing-TV
DefaultGroupName=iRacing-TV
UninstallDisplayIcon={app}\iRacing-TV Controller.exe
Compression=lzma2
SolidCompression=yes
OutputBaseFilename=iRacing-TV-Setup
OutputDir=userdocs:iRacing-TV
PrivilegesRequired=lowest
SetupIconFile="C:\Users\marvi\Desktop\iRacing-TV Controller\Assets\iRacing-TV-Icon.ico"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}";

[Files]
Source: "C:\Users\marvi\Desktop\iRacing-TV Controller\*"; DestDir: "{app}"
Source: "C:\Users\marvi\Desktop\iRacing-TV Controller\Assets\*"; DestDir: "{userdocs}\iRacing-TV\Assets"
Source: "C:\Users\marvi\Documents\GitHub\iRacing-TV-Unity\Build\*"; DestDir: "{app}"; Flags: recursesubdirs

Source: "C:\Users\marvi\Desktop\iRacing-TV Controller\Assets\RevolutionGothic_ExtraBold.otf"; DestDir: "{autofonts}"; FontInstall: "Revolution Gothic ExtraBold"; Flags: onlyifdoesntexist uninsneveruninstall fontisnttruetype
Source: "C:\Users\marvi\Desktop\iRacing-TV Controller\Assets\RevolutionGothic_ExtraBold_It.otf"; DestDir: "{autofonts}"; FontInstall: "Revolution Gothic ExtraBold It"; Flags: onlyifdoesntexist uninsneveruninstall fontisnttruetype

[Dirs]
Name: "{userdocs}\iRacing-TV"
Name: "{userdocs}\iRacing-TV\Assets"
Name: "{userdocs}\iRacing-TV\OverlaySettings"
Name: "{userdocs}\iRacing-TV\DirectorSettings"
Name: "{userdocs}\iRacing-TV\Incidents"
Name: "{userdocs}\iRacing-TV\SessionFlags"
Name: "{userdocs}\iRacing-TV\Subtitles"

[Icons]
Name: "{group}\iRacing-TV Controller"; Filename: "{app}\iRacing-TV Controller.exe"
Name: "{group}\iRacing-TV Overlay"; Filename: "{app}\iRacing-TV Overlay.exe"
Name: "{userdesktop}\iRacing-TV Controller"; Filename: "{app}\iRacing-TV Controller.exe"; Tasks: desktopicon
Name: "{userdesktop}\iRacing-TV Overlay"; Filename: "{app}\iRacing-TV Overlay.exe"; Tasks: desktopicon
