[Setup]
AppName=iRacing-TV
AppVersion=1.69
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
SetupIconFile="C:\Users\marvi\Documents\GitHub\iRacing-TV-Controller\Assets\icon\iracing-tv.ico"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}";

[Files]
Source: "C:\Users\marvi\Desktop\iRacing-TV Controller\*"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\marvi\Desktop\iRacing-TV Controller\Assets\*"; DestDir: "{userdocs}\iRacing-TV\Assets"; Flags: ignoreversion recursesubdirs
Source: "C:\Users\marvi\Documents\GitHub\iRacing-TV-Unity\Build\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs
Source: "C:\Users\marvi\Documents\GitHub\iRacing-TV-Unity\webserver.exe"; DestDir: "{app}"; Flags: ignoreversion

Source: "C:\Users\marvi\Desktop\iRacing-TV Controller\Assets\fonts\RevolutionGothic_ExtraBold.otf"; DestDir: "{autofonts}"; FontInstall: "Revolution Gothic ExtraBold"; Flags: onlyifdoesntexist uninsneveruninstall fontisnttruetype
Source: "C:\Users\marvi\Desktop\iRacing-TV Controller\Assets\fonts\RevolutionGothic_ExtraBold_It.otf"; DestDir: "{autofonts}"; FontInstall: "Revolution Gothic ExtraBold It"; Flags: onlyifdoesntexist uninsneveruninstall fontisnttruetype

[Dirs]
Name: "{userdocs}\iRacing-TV"
Name: "{userdocs}\iRacing-TV\Assets"
Name: "{userdocs}\iRacing-TV\OverlaySettings"
Name: "{userdocs}\iRacing-TV\DirectorSettings"
Name: "{userdocs}\iRacing-TV\Incidents"
Name: "{userdocs}\iRacing-TV\SessionFlags"
Name: "{userdocs}\iRacing-TV\Subtitles"
Name: "{userdocs}\iRacing-TV\MemberImages"

[Icons]
Name: "{group}\iRacing-TV Controller"; Filename: "{app}\iRacing-TV Controller.exe"
Name: "{group}\iRacing-TV Overlay"; Filename: "{app}\iRacing-TV Overlay.exe"
Name: "{group}\iRacing-TV Webserver"; Filename: "{app}\webserver.exe"; Parameters: "-m private"
Name: "{userdesktop}\iRacing-TV Controller"; Filename: "{app}\iRacing-TV Controller.exe"; Tasks: desktopicon
Name: "{userdesktop}\iRacing-TV Overlay"; Filename: "{app}\iRacing-TV Overlay.exe"; Tasks: desktopicon
Name: "{userdesktop}\iRacing-TV Webserver"; Filename: "{app}\webserver.exe"; Parameters: "-m private"
