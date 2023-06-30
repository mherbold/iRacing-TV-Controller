; -- Example1.iss --
; Demonstrates copying 3 files and creating an icon.

; SEE THE DOCUMENTATION FOR DETAILS ON CREATING .ISS SCRIPT FILES!

[Setup]
AppName=iRacing-TV-Unity
AppVersion=1.1
AppCopyright=Created by Marvin Herbold
AppPublisher=Marvin Herbold
AppPublisherURL=http://herboldracing.com/blog/iracing/iracing-tv-unity/
WizardStyle=modern
DefaultDirName={autopf}\iRacing-TV-Unity
DefaultGroupName=iRacing-TV-Unity
UninstallDisplayIcon={app}\iRacing-TV-Controller.exe
Compression=lzma2
SolidCompression=yes
OutputBaseFilename=iRacing-TV-Unity-Setup
OutputDir=userdocs:iRacing-TV-Unity
PrivilegesRequired=lowest
SetupIconFile="C:\Users\marvi\Desktop\iRacing-TV-Controller\Assets\iRacing-TV-Icon.ico"

[Files]
Source: "C:\Users\marvi\Desktop\iRacing-TV-Controller\*"; DestDir: "{app}"
Source: "C:\Users\marvi\Desktop\iRacing-TV-Controller\Assets\*"; DestDir: "{userdocs}\iRacing-TV-Unity\Assets"
Source: "C:\Users\marvi\Documents\GitHub\iRacing-TV-Unity\Build\*"; DestDir: "{app}"; Flags: recursesubdirs

[Dirs]
Name: "{userdocs}\iRacing-TV-Unity"
Name: "{userdocs}\iRacing-TV-Unity\Assets"
Name: "{userdocs}\iRacing-TV-Unity\OverlaySettings"
Name: "{userdocs}\iRacing-TV-Unity\DirectorSettings"
Name: "{userdocs}\iRacing-TV-Unity\IncidentScans"
Name: "{userdocs}\iRacing-TV-Unity\SessionFlags"

[Icons]
Name: "{group}\iRacing-TV-Controller"; Filename: "{app}\iRacing-TV-Controller.exe"
Name: "{group}\iRacing-TV-Overlay"; Filename: "{app}\iRacing-TV-Overlay.exe"
