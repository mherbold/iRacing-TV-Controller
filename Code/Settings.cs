﻿
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	public static class Settings
	{
		public const int SaveToFileIntervalTime = 1;

		public const string OverlaySettingsFolderName = "OverlaySettings";
		public const string DirectorSettingsFolderName = "DirectorSettings";

		public const string EditorSettingsFileName = "Editor.xml";

		public static string editorSettingsFolder = Program.documentsFolder;
		public static string overlaySettingsFolder = Program.documentsFolder + OverlaySettingsFolderName + "\\";
		public static string directorSettingsFolder = Program.documentsFolder + DirectorSettingsFolderName + "\\";

		public static string relativeEditorSettingsFilePath = GetRelativePath( editorSettingsFolder + EditorSettingsFileName );

		public static SettingsEditor editor = new();

		public static SettingsOverlay overlayGlobal = new();
		public static SettingsOverlay overlayLocal = new();
		public static SettingsOverlay overlay = new();

		public static SettingsDirector directorGlobal = new();
		public static SettingsDirector directorLocal = new();
		public static SettingsDirector director = new();

		public static List<SettingsOverlay> overlayList = new();
		public static List<SettingsDirector> directorList = new();

		public static int loading = 0;

		public static float saveEditorToFileTimeRemaining = 0;
		public static bool saveEditorToFileQueued = false;

		public static float saveOverlayToFileTimeRemaining = 0;
		public static bool saveOverlayToFileQueued = false;

		public static float saveDirectorToFileTimeRemaining = 0;
		public static bool saveDirectorToFileQueued = false;

		public static void Initialize()
		{
			LogFile.Write( "Initializing settings...\r\n" );

			// initialize default settings

			FixSettings( overlayGlobal );
			FixSettings( overlayLocal );

			// create directories

			if ( !Directory.Exists( Program.documentsFolder ) )
			{
				LogFile.Write( $"Directory {Program.documentsFolder} does not exist, creating it...\r\n" );

				Directory.CreateDirectory( Program.documentsFolder );
			}

			if ( !Directory.Exists( overlaySettingsFolder ) )
			{
				LogFile.Write( $"Directory {overlaySettingsFolder} does not exist, creating it...\r\n" );

				Directory.CreateDirectory( overlaySettingsFolder );
			}

			if ( !Directory.Exists( directorSettingsFolder ) )
			{
				LogFile.Write( $"Directory {directorSettingsFolder} does not exist, creating it...\r\n" );

				Directory.CreateDirectory( directorSettingsFolder );
			}

			// delete obsolete files

			var obsoleteFilePath = $"{overlaySettingsFolder}Global.xml";

			if ( File.Exists( obsoleteFilePath ) )
			{
				var renamedFilePath = $"{overlaySettingsFolder}Global (obsolete - delete me).xml";

				File.Move( obsoleteFilePath, renamedFilePath );
			}

			obsoleteFilePath = $"{directorSettingsFolder}Global.xml";

			if ( File.Exists( obsoleteFilePath ) )
			{
				var renamedFilePath = $"{directorSettingsFolder}Global (obsolete - delete me).xml";

				File.Move( obsoleteFilePath, renamedFilePath );
			}

			// editor

			if ( File.Exists( relativeEditorSettingsFilePath ) )
			{
				try
				{
					editor = (SettingsEditor) Load( relativeEditorSettingsFilePath, typeof( SettingsEditor ) );
				}
				catch ( Exception exception )
				{
					MessageBox.Show( MainWindow.Instance, $"We could not load the editor settings file '{relativeEditorSettingsFilePath}'.\r\n\r\nThe error message is as follows:\r\n\r\n{exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
				}
			}
			else
			{
				Save( relativeEditorSettingsFilePath, editor );
			}

			// web page folder

			if ( !Directory.Exists( editor.webpageGeneralOutputFolder ) )
			{
				LogFile.Write( $"Directory {editor.webpageGeneralOutputFolder} does not exist, creating it...\r\n" );

				Directory.CreateDirectory( editor.webpageGeneralOutputFolder );
			}

			// overlay

			var overlaySettingsFilePaths = Directory.EnumerateFiles( overlaySettingsFolder );

			foreach ( var overlaySettingsFilePath in overlaySettingsFilePaths )
			{
				try
				{
					var relativeOverlaySettingsFilePath = GetRelativePath( overlaySettingsFilePath );

					var settings = (SettingsOverlay) Load( relativeOverlaySettingsFilePath, typeof( SettingsOverlay ) );

					settings.filePath = relativeOverlaySettingsFilePath;

					FixSettings( settings );

					overlayList.Add( settings );

					if ( settings.filePath == editor.lastActiveOverlayFilePath )
					{
						overlayLocal = settings;
					}
				}
				catch ( Exception exception )
				{
					MessageBox.Show( MainWindow.Instance, $"We could not load the overlay settings file '{overlaySettingsFilePath}'.\r\n\r\nThe error message is as follows:\r\n\r\n{exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
				}
			}

			if ( overlayList.Count == 0 )
			{
				overlayLocal.filePath = GetRelativePath( overlaySettingsFolder + "My new overlay.xml" );

				overlayList.Add( overlayLocal );

				saveOverlayToFileQueued = true;
			}

			if ( overlayLocal.filePath == SettingsOverlay.defaultFilePath )
			{
				overlayLocal = overlayList[ 0 ];
			}

			// director

			var directorSettingsFilePaths = Directory.EnumerateFiles( directorSettingsFolder );

			foreach ( var directorSettingsFilePath in directorSettingsFilePaths )
			{
				try
				{
					var relativeDirectorSettingsFilePath = GetRelativePath( directorSettingsFilePath );

					var settings = (SettingsDirector) Load( relativeDirectorSettingsFilePath, typeof( SettingsDirector ) );

					settings.filePath = relativeDirectorSettingsFilePath;

					directorList.Add( settings );

					if ( settings.filePath == editor.lastActiveDirectorFilePath )
					{
						directorLocal = settings;
					}
				}
				catch ( Exception exception )
				{
					MessageBox.Show( MainWindow.Instance, $"We could not load the director settings file '{directorSettingsFilePath}'.\r\n\r\nThe error message is as follows:\r\n\r\n{exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
				}
			}

			if ( directorList.Count == 0 )
			{
				directorLocal.filePath = GetRelativePath( directorSettingsFolder + "My new director.xml" );

				directorList.Add( directorLocal );

				saveDirectorToFileQueued = true;
			}

			if ( directorLocal.filePath == SettingsDirector.defaultFilePath )
			{
				directorLocal = directorList[ 0 ];
			}

			IPC.readyToSendSettings = true;
		}

		public static void Update()
		{
			saveEditorToFileTimeRemaining = Math.Max( 0, saveEditorToFileTimeRemaining - Program.deltaTime );

			if ( saveEditorToFileQueued && ( saveEditorToFileTimeRemaining == 0 ) )
			{
				saveEditorToFileQueued = false;
				saveEditorToFileTimeRemaining = SaveToFileIntervalTime;

				SaveEditor();
			}

			saveOverlayToFileTimeRemaining = Math.Max( 0, saveOverlayToFileTimeRemaining - Program.deltaTime );

			if ( saveOverlayToFileQueued && ( saveOverlayToFileTimeRemaining == 0 ) )
			{
				saveOverlayToFileQueued = false;
				saveOverlayToFileTimeRemaining = SaveToFileIntervalTime;

				SaveOverlay();
			}

			saveDirectorToFileTimeRemaining = Math.Max( 0, saveDirectorToFileTimeRemaining - Program.deltaTime );

			if ( saveDirectorToFileQueued && ( saveDirectorToFileTimeRemaining == 0 ) )
			{
				saveDirectorToFileQueued = false;
				saveDirectorToFileTimeRemaining = SaveToFileIntervalTime;

				SaveDirector();
			}
		}

		public static void FixSettings( SettingsOverlay settings )
		{
			LogFile.Write( $"Fixing up overlay settings for {settings.filePath}...\r\n" );

			settings.trackMapTextureFilePath = GetRelativePath( settings.trackMapTextureFilePath );

			var defaultFontNames = new string[]
			{
				"Revolution Gothic ExtraBold",
				"Revolution Gothic It ExtraBold",
				"Arial",
				"Arial"
			};

			for ( var fontIndex = 0; fontIndex < settings.fontNames.Length; fontIndex++ )
			{
				if ( ( settings.fontNames[ fontIndex ] == null ) || ( settings.fontNames[ fontIndex ] == string.Empty ) )
				{
					settings.fontNames[ fontIndex ] = defaultFontNames[ fontIndex ];
				}
			}

			for ( var fontIndex = 0; fontIndex < settings.fontPaths.Length; fontIndex++ )
			{
				if ( !MainWindow.Instance.fontOptions.ContainsKey( settings.fontNames[ fontIndex ] ) )
				{
					settings.fontNames[ fontIndex ] = MainWindow.Instance.fontOptions.First().Key;
				}

				settings.fontPaths[ fontIndex ] = MainWindow.Instance.fontOptions[ settings.fontNames[ fontIndex ] ];
			}

			var defaultImageSettings = new Dictionary<string, SettingsImage>() {
				{ "BattleChyronBackground", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\generic-background.png", size = { x = 500, y = 178 }, border = { x = 24, y = 24, z = 24, w = 24 } } },
				{ "BattleChyronLayer1", new SettingsImage() { imageType = SettingsImage.ImageType.Car, position = { x = 227, y = -2 }, size = { x = 270, y = 149 } } },
				{ "BattleChyronLayer2", new SettingsImage() { imageType = SettingsImage.ImageType.MemberClubRegion, position = { x = 264, y = 121 }, size = { x = 200, y = 50 } } },
				{ "BattleChyronLayer3", new SettingsImage() { imageType = SettingsImage.ImageType.Helmet, position = { x = -36, y = -48 }, size = { x = 80, y = 80 } } },
				{ "BattleChyronLayer4", new SettingsImage() { imageType = SettingsImage.ImageType.CarNumber, position = { x = 7, y = 26 }, size = { x = 90, y = 62 } } },
				{ "BattleChyronLayer5", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\battle.png", position = { x = 452, y = 9 }, size = { x = 160, y = 160 } } },
				{ "BattleChyronLayer6", new SettingsImage() { imageType = SettingsImage.ImageType.None } },
				{ "BattleChyronLayer7", new SettingsImage() { imageType = SettingsImage.ImageType.None } },
				{ "ChyronBackground", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\generic-background.png", size = { x = 500, y = 178 }, border = { x = 24, y = 24, z = 24, w = 24 } } },
				{ "ChyronLayer1", new SettingsImage() { imageType = SettingsImage.ImageType.Car, position = { x = 227, y = -2 }, size = { x = 270, y = 149 } } },
				{ "ChyronLayer2", new SettingsImage() { imageType = SettingsImage.ImageType.MemberClubRegion, position = { x = 264, y = 121 }, size = { x = 200, y = 50 } } },
				{ "ChyronLayer3", new SettingsImage() { imageType = SettingsImage.ImageType.Helmet, position = { x = -36, y = -48 }, size = { x = 80, y = 80 } } },
				{ "ChyronLayer4", new SettingsImage() { imageType = SettingsImage.ImageType.CarNumber, position = { x = 7, y = 26 }, size = { x = 90, y = 62 } } },
				{ "ChyronLayer5", new SettingsImage() { imageType = SettingsImage.ImageType.None } },
				{ "ChyronLayer6", new SettingsImage() { imageType = SettingsImage.ImageType.None } },
				{ "ChyronLayer7", new SettingsImage() { imageType = SettingsImage.ImageType.None } },
				{ "Custom1Layer1", new SettingsImage() },
				{ "Custom1Layer2", new SettingsImage() },
				{ "Custom1Layer3", new SettingsImage() },
				{ "Custom2Layer1", new SettingsImage() },
				{ "Custom2Layer2", new SettingsImage() },
				{ "Custom2Layer3", new SettingsImage() },
				{ "Custom3Layer1", new SettingsImage() },
				{ "Custom3Layer2", new SettingsImage() },
				{ "Custom3Layer3", new SettingsImage() },
				{ "Custom4Layer1", new SettingsImage() },
				{ "Custom4Layer2", new SettingsImage() },
				{ "Custom4Layer3", new SettingsImage() },
				{ "Custom5Layer1", new SettingsImage() },
				{ "Custom5Layer2", new SettingsImage() },
				{ "Custom5Layer3", new SettingsImage() },
				{ "Custom6Layer1", new SettingsImage() },
				{ "Custom6Layer2", new SettingsImage() },
				{ "Custom6Layer3", new SettingsImage() },
				{ "HudBackground", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\hud-background.png" } },
				{ "HudSpeechToTextBackground", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\generic-background.png", border = { x = 24, y = 24, z = 24, w = 24 } } },
				{ "HudSpotterIndicatorLeft", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\spotter-indicator.png", position = { x = 50, y = -50 }, tintColor = { r = 1, g = 1, b = 0 } } },
				{ "HudSpotterIndicatorRight", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\spotter-indicator.png", position = { x = 550, y = -50 }, tintColor = { r = 1, g = 1, b = 0 } } },
				{ "IntroBackground", new SettingsImage() { imageType = SettingsImage.ImageType.None, filePath = Program.documentsFolder + "Assets\\default\\generic-background.png", position = { x = 44, y = 155 }, size = { x = 1826, y = 660 }, border = { x = 24, y = 24, z = 24, w = 24 } } },
				{ "IntroLayer1", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\generic-background.png", position = { x = -203, y = -177 }, size = { x = 408, y = 336 }, border = { x = 24, y = 24, z = 24, w = 24 } } },
				{ "IntroLayer2", new SettingsImage() { imageType = SettingsImage.ImageType.MemberClubRegion, position = { x = -73, y = -170 }, size = { x = 256, y = 64 } } },
				{ "IntroLayer3", new SettingsImage() { imageType = SettingsImage.ImageType.Car, position = { x = -214, y = -94 }, size = { x = 442, y = 221 } } },
				{ "IntroLayer4", new SettingsImage() { imageType = SettingsImage.ImageType.Helmet, position = { x = 105, y = -104 }, size = { x = 80, y = 80 } } },
				{ "IntroLayer5", new SettingsImage() { imageType = SettingsImage.ImageType.None, filePath = Program.documentsFolder + "Assets\\default\\generic-background.png", position = { x = -178, y = 53 }, size = { x = 357, y = 90 }, border = { x = 24, y = 24, z = 24, w = 24 } } },
				{ "IntroLayer6", new SettingsImage() { imageType = SettingsImage.ImageType.CarNumber, position = { x = 100, y = 84 }, size = { x = 94, y = 63 } } },
				{ "LeaderboardBackground", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\generic-background.png", size = { x = 319, y = 49 }, border = { x = 24, y = 24, z = 24, w = 24 }, useClassColors = true } },
				{ "LeaderboardCurrentTarget", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\leaderboard-current-target.png" } },
				{ "LeaderboardLayer1", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\leaderboard-heading.png", position = { x = 0, y = -3 }, useClassColors = true } },
				{ "LeaderboardLayer2", new SettingsImage() { imageType = SettingsImage.ImageType.None } },
				{ "LeaderboardPositionBackground", new SettingsImage() { imageType = SettingsImage.ImageType.None } },
				{ "LeaderboardPositionLayer1", new SettingsImage() { imageType = SettingsImage.ImageType.CarNumber, position = { x = 48, y = 10 }, size = { x = 56, y = 28 } } },
				{ "LeaderboardPositionLayer2", new SettingsImage() { imageType = SettingsImage.ImageType.None } },
				{ "LeaderboardPositionLayer3", new SettingsImage() { imageType = SettingsImage.ImageType.None } },
				{ "LeaderboardPositionPreferredCar", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\leaderboard-preferred-car.png", position = { x = 5, y = 5 }, tintColor = { r = 0.75f, g = 0.5f, b = 0.25f, a = 0.35f } } },
				{ "LeaderboardPositionSplitter", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\leaderboard-position-splitter.png", useClassColors = true } },
				{ "PitLaneBackground", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\pit-lane-background.png", position = { y = -9 }, size = { x = 440, y = 19 }, border = { x = 75, y = 2, z = 2, w = 2 }, tintColor = { a = 0.5f } } },
				{ "PitLaneCarCurrentTarget", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\track-map-target.png", size = { x = 32, y = 32 }, frameSize = { x = 64, y = 64 }, frameCount = 8, animationSpeed = 15, tintColor = { r = 57f / 255, g = 181f / 255, b = 74f / 255, a = 1 } } },
				{ "PitLaneCarLayer1", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\track-map-car.png", size = { x = 16, y = 16 } } },
				{ "PitLaneCarLayer2", new SettingsImage() { imageType = SettingsImage.ImageType.CarNumber, position = { x = 0, y = -35 }, size = { x = 40, y = 40 } } },
				{ "RaceResultBackground", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\generic-background.png", position = { x = -512, y = 0 }, size = { x = 1024, y = 104 }, border = { x = 24, y = 24, z = 24, w = 24 }, useClassColors = true } },
				{ "RaceResultLayer1", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\leaderboard-heading.png", position = { x = -512, y = 0 }, size = { x = 1024, y = 72 }, border = { x = 24, y = 24, z = 24, w = 24 }, useClassColors = true } },
				{ "RaceResultLayer2", new SettingsImage() { imageType = SettingsImage.ImageType.None } },
				{ "RaceResultPositionBackground", new SettingsImage() { imageType = SettingsImage.ImageType.None } },
				{ "RaceResultPositionLayer1", new SettingsImage() { imageType = SettingsImage.ImageType.CarNumber, position = { x = -357, y = 10 }, size = { x = 90, y = 50 } } },
				{ "RaceResultPositionLayer2", new SettingsImage() { imageType = SettingsImage.ImageType.None } },
				{ "RaceResultPositionLayer3", new SettingsImage() { imageType = SettingsImage.ImageType.None } },
				{ "RaceResultPositionPreferredCar", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\leaderboard-preferred-car.png", position = { x = -512, y = 4 }, size = { x = 1024, y = 64 }, tintColor = { r = 0.75f, g = 0.5f, b = 0.25f, a = 0.35f } } },
				{ "RaceStatusBackground", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\race-status-background.png" } },
				{ "RaceStatusBlackLight", new SettingsImage(){ imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\race-status-light-black.png", position = { x = 280, y = 130 } } },
				{ "RaceStatusCheckeredFlagLayer1", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\race-status-checkered-flag.png", size = { x = 319, y = 219 }, frameSize = { x = 319, y = 219 }, frameCount = 35, animationSpeed = 24 } },
				{ "RaceStatusCheckeredFlagLayer2", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\race-status-checkered-flag-text.png", tintColor = { r = 0.9f, g = 0.9f, b = 0.9f } } },
				{ "RaceStatusGreenFlagLayer1", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\race-status-white-flag.png", size = { x = 319, y = 219 }, tintColor = { r = 0.212f, g = 0.871f, b = 0.212f }, frameSize = { x = 319, y = 219 }, frameCount = 35, animationSpeed = 24 } },
				{ "RaceStatusGreenFlagLayer2", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\race-status-green-flag-text.png" } },
				{ "RaceStatusGreenLight", new SettingsImage(){ imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\race-status-light-green.png", position = { x = 280, y = 130 } } },
				{ "RaceStatusLayer1", new SettingsImage() { imageType = SettingsImage.ImageType.SeriesLogo, position = { x = 7, y = 7 }, size = { x = 305, y = 103 } } },
				{ "RaceStatusOneToGreen", new SettingsImage() { imageType = SettingsImage.ImageType.None } },
				{ "RaceStatusWhiteLight", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\race-status-light-white.png", position = { x = 280, y = 130 } } },
				{ "RaceStatusYellowFlagLayer1", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\race-status-white-flag.png", size = { x = 319, y = 219 }, tintColor = { r = 1, g = 1, b = 0 }, frameSize = { x = 319, y = 219 }, frameCount = 35, animationSpeed = 24 } },
				{ "RaceStatusYellowFlagLayer2", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\race-status-yellow-flag-text.png" } },
				{ "RaceStatusYellowLight", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\race-status-light-yellow.png", position = { x = 280, y = 130 } } },
				{ "StartLightsGo", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\start-lights-go.png" } },
				{ "StartLightsReady", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\start-lights-ready.png" } },
				{ "StartLightsSet", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\start-lights-set.png" } },
				{ "SubtitlesBackground", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\generic-background.png", border = { x = 24, y = 24, z = 24, w = 24 } } },
				{ "TrackMapCarCurrentTarget", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\track-map-target.png", size = { x = 32, y = 32 }, position = { x = 0, y = 0 }, frameSize = { x = 64, y = 64 }, frameCount = 8, animationSpeed = 15, tintColor = { r = 57f / 255, g = 181f / 255, b = 74f / 255, a = 1 } } },
				{ "TrackMapCarLayer1", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\track-map-car.png", size = { x = 16, y = 16 } } },
				{ "TrackMapCarLayer2", new SettingsImage() { imageType = SettingsImage.ImageType.CarNumber, position = { x = 0, y = -35 }, size = { x = 40, y = 40 } } },
				{ "TrackMapPaceCar", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\track-map-pace-car.png", size = { x = 16, y = 16 }, frameSize = { x = 32, y = 32 }, frameCount = 12, animationSpeed = 15 } },
				{ "TrackMapStartFinishLine", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\track-map-car.png", size = { x = 8, y = 8 }, tintColor = { r = 1, g = 0, b = 0, a = 1 } } },
				{ "TrainerBackground", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\generic-background.png", position = { x = -10, y = -10 }, size = { x = 660, y = 148 }, border = { x = 24, y = 24, z = 24, w = 24 }, tintColor = { a = 0.25f } } },
				{ "VoiceOfBackground", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\default\\voice-of-background.png" } },
				{ "VoiceOfLayer1", new SettingsImage() { imageType = SettingsImage.ImageType.Car, position = { x = 220, y = -48 }, size = { x = 400, y = 200 } } },
				{ "VoiceOfLayer2", new SettingsImage() { imageType = SettingsImage.ImageType.None } },
				{ "VoiceOfLayer3", new SettingsImage() { imageType = SettingsImage.ImageType.None } },
			};

			var oldImageSettingNames = new Dictionary<string, string>()
			{
				{ "BlackLight", "RaceStatusBlackLight" },
				{ "CarNumber", "LeaderboardPositionLayer1" },
				{ "CheckeredFlag", "RaceStatusCheckeredFlag" },
				{ "Custom1", "Custom1Layer1" },
				{ "Custom2", "Custom2Layer1" },
				{ "Custom3", "Custom3Layer1" },
				{ "Custom4", "Custom4Layer1" },
				{ "CustomLayer1", "Custom1Layer1" },
				{ "CustomLayer2", "Custom2Layer1" },
				{ "CustomLayer3", "Custom3Layer1" },
				{ "CustomLayer4", "Custom4Layer1" },
				{ "CustomLayer5", "Custom5Layer1" },
				{ "CustomLayer6", "Custom6Layer1" },
				{ "GreenFlag", "RaceStatusGreenFlag" },
				{ "GreenLight", "RaceStatusGreenLight" },
				{ "Highlight", "LeaderboardCurrentTarget" },
				{ "IntroDriverBackground", "IntroLayer1" },
				{ "IntroDriverCar", "IntroLayer3" },
				{ "IntroDriverCarNumber", "IntroLayer6" },
				{ "IntroDriverHelmet", "IntroLayer4" },
				{ "IntroDriverSuit", "IntroLayer2" },
				{ "IntroStatsBackground", "IntroLayer5" },
				{ "LeaderboardPositionPreferredDriver", "LeaderboardPositionPreferredCar" },
				{ "PositionSplitter", "LeaderboardPositionSplitter" },
				{ "RaceStatusSeriesLogo", "RaceStatusLayer1" },
				{ "SeriesLogo", "RaceStatusLayer1" },
				{ "TrackMapCurrentTarget", "TrackMapCarCurrentTarget" },
				{ "VoiceOfCar", "VoiceOfLayer1" },
				{ "WhiteLight", "RaceStatusWhiteLight" },
				{ "YellowFlag", "RaceStatusYellowFlag" },
				{ "YellowLight", "RaceStatusYellowLight" }
			};

			foreach ( var item in oldImageSettingNames )
			{
				if ( settings.imageSettingsDataDictionary.ContainsKey( item.Key ) )
				{
					settings.imageSettingsDataDictionary[ item.Value ] = settings.imageSettingsDataDictionary[ item.Key ];

					settings.imageSettingsDataDictionary.Remove( item.Key );
				}
			}

			foreach ( var item in defaultImageSettings )
			{
				if ( !settings.imageSettingsDataDictionary.ContainsKey( item.Key ) )
				{
					settings.imageSettingsDataDictionary[ item.Key ] = item.Value;
				}
			}

			foreach ( var item in settings.imageSettingsDataDictionary )
			{
				item.Value.filePath = GetRelativePath( item.Value.filePath );
			}

			var defaultTextSettings = new Dictionary<string, SettingsText>()
			{
				{ "BattleChyronTextLayer1", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 40, position = { x = 53, y = 97 }, alignment = TextAlignmentOptions.Top, tintColor = { b = 0 }, content = SettingsText.Content.Driver_Position_Ordinal } },
				{ "BattleChyronTextLayer10", new SettingsText() { fontIndex = SettingsText.FontIndex.None } },
				{ "BattleChyronTextLayer11", new SettingsText() { fontIndex = SettingsText.FontIndex.None } },
				{ "BattleChyronTextLayer12", new SettingsText() { fontIndex = SettingsText.FontIndex.None } },
				{ "BattleChyronTextLayer13", new SettingsText() { fontIndex = SettingsText.FontIndex.None } },
				{ "BattleChyronTextLayer14", new SettingsText() { fontIndex = SettingsText.FontIndex.None } },
				{ "BattleChyronTextLayer15", new SettingsText() { fontIndex = SettingsText.FontIndex.None } },
				{ "BattleChyronTextLayer2", new SettingsText() { fontIndex = SettingsText.FontIndex.FontB, fontSize = 42, position = { x = 100, y = 19 }, content = SettingsText.Content.Driver_GivenName } },
				{ "BattleChyronTextLayer3", new SettingsText() { fontIndex = SettingsText.FontIndex.FontB, fontSize = 25, position = { x = 100, y = 60 }, tintColor = { r = 0.5f, g = 0.5f, b = 0.5f }, content = SettingsText.Content.Driver_FamilyName } },
				{ "BattleChyronTextLayer4", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 36, position = { x = 100, y = 119 }, content = SettingsText.Content.Driver_License } },
				{ "BattleChyronTextLayer5", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 36, position = { x = 100, y = 84 }, content = SettingsText.Content.Driver_Rating } },
				{ "BattleChyronTextLayer6", new SettingsText() { fontIndex = SettingsText.FontIndex.None } },
				{ "BattleChyronTextLayer7", new SettingsText() { fontIndex = SettingsText.FontIndex.None } },
				{ "BattleChyronTextLayer8", new SettingsText() { fontIndex = SettingsText.FontIndex.None } },
				{ "BattleChyronTextLayer9", new SettingsText() { fontIndex = SettingsText.FontIndex.None } },
				{ "ChyronTextLayer1", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 40, position = { x = 53, y = 97 }, alignment = TextAlignmentOptions.Top, tintColor = { b = 0 }, content = SettingsText.Content.Driver_Position_Ordinal } },
				{ "ChyronTextLayer10", new SettingsText() { fontIndex = SettingsText.FontIndex.None } },
				{ "ChyronTextLayer11", new SettingsText() { fontIndex = SettingsText.FontIndex.None } },
				{ "ChyronTextLayer12", new SettingsText() { fontIndex = SettingsText.FontIndex.None } },
				{ "ChyronTextLayer13", new SettingsText() { fontIndex = SettingsText.FontIndex.None } },
				{ "ChyronTextLayer14", new SettingsText() { fontIndex = SettingsText.FontIndex.None } },
				{ "ChyronTextLayer15", new SettingsText() { fontIndex = SettingsText.FontIndex.None } },
				{ "ChyronTextLayer2", new SettingsText() { fontIndex = SettingsText.FontIndex.FontB, fontSize = 42, position = { x = 100, y = 19 }, content = SettingsText.Content.Driver_GivenName } },
				{ "ChyronTextLayer3", new SettingsText() { fontIndex = SettingsText.FontIndex.FontB, fontSize = 25, position = { x = 100, y = 60 }, tintColor = { r = 0.5f, g = 0.5f, b = 0.5f }, content = SettingsText.Content.Driver_FamilyName } },
				{ "ChyronTextLayer4", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 36, position = { x = 100, y = 119 }, content = SettingsText.Content.Driver_License } },
				{ "ChyronTextLayer5", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 36, position = { x = 100, y = 84 }, content = SettingsText.Content.Driver_Rating } },
				{ "ChyronTextLayer6", new SettingsText() { fontIndex = SettingsText.FontIndex.None } },
				{ "ChyronTextLayer7", new SettingsText() { fontIndex = SettingsText.FontIndex.None } },
				{ "ChyronTextLayer8", new SettingsText() { fontIndex = SettingsText.FontIndex.None } },
				{ "ChyronTextLayer9", new SettingsText() { fontIndex = SettingsText.FontIndex.None } },
				{ "Custom1TextLayer1", new SettingsText() },
				{ "Custom1TextLayer2", new SettingsText() },
				{ "Custom1TextLayer3", new SettingsText() },
				{ "Custom2TextLayer1", new SettingsText() },
				{ "Custom2TextLayer2", new SettingsText() },
				{ "Custom2TextLayer3", new SettingsText() },
				{ "Custom3TextLayer1", new SettingsText() },
				{ "Custom3TextLayer2", new SettingsText() },
				{ "Custom3TextLayer3", new SettingsText() },
				{ "Custom4TextLayer1", new SettingsText() },
				{ "Custom4TextLayer2", new SettingsText() },
				{ "Custom4TextLayer3", new SettingsText() },
				{ "Custom5TextLayer1", new SettingsText() },
				{ "Custom5TextLayer2", new SettingsText() },
				{ "Custom5TextLayer3", new SettingsText() },
				{ "Custom6TextLayer1", new SettingsText() },
				{ "Custom6TextLayer2", new SettingsText() },
				{ "Custom6TextLayer3", new SettingsText() },
				{ "HudSpeechToText", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 30, alignment = TextAlignmentOptions.Center, tintColor = { r = 0.961f, g = 0.961f, b = 0.953f } } },
				{ "HudTextLayer1", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 22, alignment = TextAlignmentOptions.TopLeft, position = { x = 21, y = 25 }, content = SettingsText.Content.Player_FuelRemainingInLaps } },
				{ "HudTextLayer2", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 22, alignment = TextAlignmentOptions.TopLeft, position = { x = 125, y = 25 }, content = SettingsText.Content.Driver_LapsBehindClassLeader } },
				{ "HudTextLayer3", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 22, alignment = TextAlignmentOptions.TopLeft, position = { x = 444, y = 25 }, content = SettingsText.Content.Player_RPM } },
				{ "HudTextLayer4", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 22, alignment = TextAlignmentOptions.TopLeft, position = { x = 541, y = 25 }, content = SettingsText.Content.Driver_Speed } },
				{ "HudTextLayer5", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 22, alignment = TextAlignmentOptions.TopLeft, position = { x = 652, y = 25 }, content = SettingsText.Content.Driver_Gear } },
				{ "HudTextLayer6", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 22, alignment = TextAlignmentOptions.TopLeft, position = { x = 228, y = 25 }, content = SettingsText.Content.Driver_CarInFront_GapTime } },
				{ "HudTextLayer7", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 22, alignment = TextAlignmentOptions.TopLeft, position = { x = 335, y = 25 }, content = SettingsText.Content.Driver_CarBehind_GapTime } },
				{ "HudTextLayer8", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 22, alignment = TextAlignmentOptions.Center, position = { x = 960, y = 540 }, content = SettingsText.Content.Driver_LapDelta } },
				{ "HudTextLayer9", new SettingsText() },
				{ "HudTextLayer10", new SettingsText() },
				{ "HudTextLayer11", new SettingsText() },
				{ "HudTextLayer12", new SettingsText() },
				{ "HudTextLayer13", new SettingsText() },
				{ "HudTextLayer14", new SettingsText() },
				{ "HudTextLayer15", new SettingsText() },
				{ "HudTextLayer16", new SettingsText() },
				{ "IntroDriverTextLayer1", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 76, position = { x = -180, y = -167 }, content = SettingsText.Content.Driver_QualifyPosition_WithP } },
				{ "IntroDriverTextLayer2", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 32, position = { x = -179, y = 46 }, tintColor = { r = 0.306f, g = 0.832f, b = 1 }, content = SettingsText.Content.Driver_QualifyTime } },
				{ "IntroDriverTextLayer3", new SettingsText() { fontIndex = SettingsText.FontIndex.FontB, fontSize = 32, position = { x = -179, y = 110 }, content = SettingsText.Content.Driver_Name } },
				{ "IntroDriverTextLayer4", new SettingsText() { fontIndex = SettingsText.FontIndex.None, fontSize = 50, alignment = TextAlignmentOptions.TopRight, position = { x = 177, y = 86 }, content = SettingsText.Content.Driver_CarNumber } },
				{ "IntroDriverTextLayer5", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 27, position = { x = -125, y = 81 }, tintColor = { b = 0, a = 0.75f }, content = SettingsText.Content.Driver_License } },
				{ "IntroDriverTextLayer6", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 27, position = { x = -179, y = 81 }, tintColor = { b = 0, a = 0.75f }, content = SettingsText.Content.Driver_Rating } },
				{ "LeaderboardPositionCurrentTargetTextLayer1", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 21, alignment = TextAlignmentOptions.TopRight, position = { x = 397, y = 12 }, tintColor = { r = 0.69f, g = 0.71f, b = 0.694f }, content = SettingsText.Content.Driver_Speed } },
				{ "LeaderboardPositionTextLayer1", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 21, alignment = TextAlignmentOptions.TopRight, position = { x = 43, y = 12 }, tintColor = { r = 0.69f, g = 0.71f, b = 0.694f }, content = SettingsText.Content.Driver_Position } },
				{ "LeaderboardPositionTextLayer2", new SettingsText() { fontIndex = SettingsText.FontIndex.None, fontSize = 21, alignment = TextAlignmentOptions.Top, position = { x = 76, y = 12 }, tintColor = { r = 0.69f, g = 0.71f, b = 0.694f }, content = SettingsText.Content.Driver_CarNumber } },
				{ "LeaderboardPositionTextLayer3", new SettingsText() { fontIndex = SettingsText.FontIndex.FontB, fontSize = 21, position = { x = 108, y = 12 }, tintColor = { r = 0.69f, g = 0.71f, b = 0.694f }, useClassColors = true, content = SettingsText.Content.Driver_Name } },
				{ "LeaderboardPositionTextLayer4", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 21, alignment = TextAlignmentOptions.TopRight, position = { x = 298, y = 12 }, tintColor = { r = 0.69f, g = 0.71f, b = 0.694f }, content = SettingsText.Content.Driver_Telemetry } },
				{ "LeaderboardTextLayer1", new SettingsText() { fontIndex = SettingsText.FontIndex.FontB, fontSize = 27, alignment = TextAlignmentOptions.Top, position = { x = 159, y = 5 }, tintColor = { r = 0.137f, g = 0.122f, b = 0.125f }, content = SettingsText.Content.Leaderboard_ClassName } },
				{ "LeaderboardTextLayer2", new SettingsText() { fontIndex = SettingsText.FontIndex.None, fontSize = 27, alignment = TextAlignmentOptions.Top, position = { x = 159, y = 5 }, tintColor = { r = 0.137f, g = 0.122f, b = 0.125f }, content = SettingsText.Content.Leaderboard_ClassNameShort } },
				{ "PitLaneCarTextLayer1", new SettingsText() { fontIndex = SettingsText.FontIndex.None, fontSize = 21, alignment = TextAlignmentOptions.Top, position = { x = 0, y = -35 }, tintColor = { r = 0.69f, g = 0.71f, b = 0.694f }, content = SettingsText.Content.Driver_CarNumber } },
				{ "RaceResultPositionTextLayer1", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 60, alignment = TextAlignmentOptions.TopRight, position = { x = -384, y = 0 }, content = SettingsText.Content.Driver_Position_Ordinal } },
				{ "RaceResultPositionTextLayer2", new SettingsText() { fontIndex = SettingsText.FontIndex.None, fontSize = 21, alignment = TextAlignmentOptions.Top, position = { x = 76, y = 12 }, tintColor = { r = 0.69f, g = 0.71f, b = 0.694f }, content = SettingsText.Content.Driver_CarNumber } },
				{ "RaceResultPositionTextLayer3", new SettingsText() { fontIndex = SettingsText.FontIndex.FontB, fontSize = 60, position = { x = -242, y = 0 }, useClassColors = true, content = SettingsText.Content.Driver_FullName } },
				{ "RaceResultPositionTextLayer4", new SettingsText() { fontIndex = SettingsText.FontIndex.FontC, fontSize = 60, alignment = TextAlignmentOptions.TopRight, position = { x = 449, y = 0 }, tintColor = { r = 0.69f, g = 0.71f, b = 0.694f }, content = SettingsText.Content.Driver_License } },
				{ "RaceResultTextLayer1", new SettingsText() { fontIndex = SettingsText.FontIndex.FontB, fontSize = 50, alignment = TextAlignmentOptions.Top, position = { x = 0, y = 6 }, tintColor = { r = 0.137f, g = 0.122f, b = 0.125f }, content = SettingsText.Content.Leaderboard_ClassName } },
				{ "RaceResultTextLayer2", new SettingsText() { fontIndex = SettingsText.FontIndex.None, fontSize = 50, alignment = TextAlignmentOptions.Top, position = { x = 0, y = 6 }, tintColor = { r = 0.137f, g = 0.122f, b = 0.125f }, content = SettingsText.Content.Leaderboard_ClassNameShort } },
				{ "RaceStatusTextLayer1", new SettingsText() { fontIndex = SettingsText.FontIndex.FontB, fontSize = 27, position = { x = 18, y = 125 }, tintColor = { r = 0.961f, g = 0.961f, b = 0.953f }, content = SettingsText.Content.Session_Name } },
				{ "RaceStatusTextLayer2", new SettingsText() { fontIndex = SettingsText.FontIndex.FontB, fontSize = 27, alignment = TextAlignmentOptions.TopRight, position = { x = 269, y = 125 }, tintColor = { r = 0.961f, g = 0.961f, b = 0.953f }, content = SettingsText.Content.Session_LapsRemaining } },
				{ "RaceStatusTextLayer3", new SettingsText() { fontIndex = SettingsText.FontIndex.FontB, fontSize = 27, position = { x = 18, y = 175 }, tintColor = { r = 0.737f, g = 0.741f, b = 0.725f }, content = SettingsText.Content.Translation_Units } },
				{ "RaceStatusTextLayer4", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 27, alignment = TextAlignmentOptions.TopRight, position = { x = 298, y = 175 }, tintColor = { r = 0.737f, g = 0.741f, b = 0.725f }, content = SettingsText.Content.Session_CurrentLap } },
				{ "Subtitles", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 35, alignment = TextAlignmentOptions.Center, tintColor = { r = 0.961f, g = 0.961f, b = 0.953f } } },
				{ "TrackMapCarTextLayer1", new SettingsText() { fontIndex = SettingsText.FontIndex.None, fontSize = 21, alignment = TextAlignmentOptions.Top, position = { x = 0, y = -35 }, tintColor = { r = 0.69f, g = 0.71f, b = 0.694f }, content = SettingsText.Content.Driver_CarNumber } },
				{ "TrainerMessage", new SettingsText() { fontIndex = SettingsText.FontIndex.FontB, fontSize = 21, alignment = TextAlignmentOptions.TopLeft, position = { x = 10, y = 10 }, tintColor = { r = 0.69f, g = 0.71f, b = 0.694f } } },
				{ "TrainerCountdown", new SettingsText() { fontIndex = SettingsText.FontIndex.FontC, fontSize = 21, alignment = TextAlignmentOptions.Top, position = { x = 10, y = 10 }, tintColor = { r = 0.69f, g = 0.71f, b = 0.694f } } },
				{ "VoiceOfTextLayer1", new SettingsText() { fontIndex = SettingsText.FontIndex.FontB, fontSize = 30, position = { x = 30, y = 10 }, tintColor = { r = 0.434f, g = 0.434f, b = 0.434f }, content = SettingsText.Content.Translation_VoiceOf } },
				{ "VoiceOfTextLayer2", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 38, position = { x = 30, y = 41 }, tintColor = { r = 0.137f, g = 0.122f, b = 0.125f }, content = SettingsText.Content.Driver_Name } },
			};

			var oldTextSettingNames = new Dictionary<string, string>()
			{
				{ "ChyronDriverName", "ChyronTextLayer1" },
				{ "ChyronGear", "ChyronTextLayer5" },
				{ "ChyronGearLabel", "ChyronTextLayer4" },
				{ "ChyronHometown", "ChyronTextLayer13" },
				{ "ChyronHometownLabel", "ChyronTextLayer12" },
				{ "ChyronLicense", "ChyronTextLayer11" },
				{ "ChyronLicenseLabel", "ChyronTextLayer10" },
				{ "ChyronRandom", "ChyronTextLayer15" },
				{ "ChyronRandomLabel", "ChyronTextLayer14" },
				{ "ChyronRating", "ChyronTextLayer9" },
				{ "ChyronRatingLabel", "ChyronTextLayer8" },
				{ "ChyronRPM", "ChyronTextLayer7" },
				{ "ChyronRPMLabel", "ChyronTextLayer6" },
				{ "ChyronSpeed", "ChyronTextLayer3" },
				{ "ChyronSpeedLabel", "ChyronTextLayer2" },
				{ "CurrentLap", "RaceStatusTextLayer4" },
				{ "DriverName", "LeaderboardPositionTextLayer3" },
				{ "HudFuel", "HudTextLayer1" },
				{ "HudGapTimeBack", "HudTextLayer7" },
				{ "HudGapTimeFront", "HudTextLayer6" },
				{ "HudGear", "HudTextLayer5" },
				{ "HudLapDelta", "HudTextLayer8" },
				{ "HudLapsToLeader", "HudTextLayer2" },
				{ "HudRPM", "HudTextLayer3" },
				{ "HudSpeed", "HudTextLayer4" },
				{ "IntroCarNumber", "IntroDriverTextLayer4" },
				{ "IntroDriverName", "IntroDriverTextLayer3" },
				{ "IntroLicense", "IntroDriverTextLayer5" },
				{ "IntroQualifyingTime", "IntroDriverTextLayer2" },
				{ "IntroRating", "IntroDriverTextLayer6" },
				{ "IntroStartingGridPosition", "IntroDriverTextLayer1" },
				{ "IntroStatsDriverName", "IntroDriverTextLayer3" },
				{ "IntroStatsPosition", "IntroDriverTextLayer1" },
				{ "IntroStatsQualifyingTime", "IntroDriverTextLayer2" },
				{ "LapsRemaining", "RaceStatusTextLayer2" },
				{ "LeaderboardClassName", "LeaderboardTextLayer1" },
				{ "LeaderboardClassNameShort", "LeaderboardTextLayer2" },
				{ "LeaderboardCurrentTargetSpeed", "LeaderboardPositionCurrentTargetTextLayer1" },
				{ "LeaderboardPosition", "LeaderboardPositionTextLayer1" },
				{ "LeaderboardPositionCarNumber", "LeaderboardPositionTextLayer2" },
				{ "LeaderboardPositionDriverName", "LeaderboardPositionTextLayer3" },
				{ "LeaderboardPositionTelemetry", "LeaderboardPositionTextLayer4" },
				{ "PitLaneCarNumber", "PitLaneCarTextLayer1" },
				{ "Place", "LeaderboardPositionTextLayer1" },
				{ "Position", "LeaderboardPositionTextLayer1" },
				{ "RaceStatusCurrentLap", "RaceStatusTextLayer4" },
				{ "RaceStatusLapsRemaining", "RaceStatusTextLayer2" },
				{ "RaceStatusSessionName", "RaceStatusTextLayer1" },
				{ "RaceStatusUnits", "RaceStatusTextLayer3" },
				{ "SessionName", "RaceStatusTextLayer1" },
				{ "Speed", "LeaderboardPositionCurrentTargetTextLayer1" },
				{ "Telemetry", "LeaderboardPositionTextLayer4" },
				{ "TrackMapCarNumber", "TrackMapCarTextLayer1" },
				{ "Units", "RaceStatusTextLayer3" },
				{ "VoiceOf", "VoiceOfTextLayer1" },
				{ "VoiceOfDriverName", "VoiceOfTextLayer2" },
			};

			foreach ( var item in oldTextSettingNames )
			{
				if ( settings.textSettingsDataDictionary.ContainsKey( item.Key ) )
				{
					settings.textSettingsDataDictionary[ item.Value ] = settings.textSettingsDataDictionary[ item.Key ];

					settings.textSettingsDataDictionary.Remove( item.Key );
				}
			}

			foreach ( var item in defaultTextSettings )
			{
				if ( !settings.textSettingsDataDictionary.ContainsKey( item.Key ) )
				{
					settings.textSettingsDataDictionary[ item.Key ] = item.Value;
				}
			}

			var defaultTranslationSettings = new Dictionary<string, SettingsTranslation>()
			{
				{ "DidNotQualify", new SettingsTranslation() { translation = "DNQ" } },
				{ "FEATURE", new SettingsTranslation() { translation = "RACE" } },
				{ "FeetAbbreviation", new SettingsTranslation() { translation = "FT" } },
				{ "FinalLap", new SettingsTranslation() { translation = "FINAL LAP" } },
				{ "Gear", new SettingsTranslation() { translation = "Gear" } },
				{ "HEAT 1", new SettingsTranslation() { translation = "HEAT 1" } },
				{ "HEAT 2", new SettingsTranslation() { translation = "HEAT 2" } },
				{ "iRating", new SettingsTranslation() { translation = "iRating" } },
				{ "KPH", new SettingsTranslation() { translation = "KPH" } },
				{ "Lap", new SettingsTranslation() { translation = "LAP" } },
				{ "LapsAbbreviation", new SettingsTranslation() { translation = "L" } },
				{ "License", new SettingsTranslation() { translation = "License" } },
				{ "MetersAbbreviation", new SettingsTranslation() { translation = "M" } },
				{ "MPH", new SettingsTranslation() { translation = "MPH" } },
				{ "Out", new SettingsTranslation() { translation = "OUT" } },
				{ "Pit", new SettingsTranslation() { translation = "PIT" } },
				{ "PRACTICE", new SettingsTranslation() { translation = "PRACTICE" } },
				{ "QUALIFY", new SettingsTranslation() { translation = "QUALIFY" } },
				{ "RPM", new SettingsTranslation() { translation = "RPM" } },
				{ "Speed", new SettingsTranslation() { translation = "Speed" } },
				{ "Time", new SettingsTranslation() { translation = "TIME" } },
				{ "ToGo", new SettingsTranslation() { translation = "TO GO" } },
				{ "VoiceOf", new SettingsTranslation() { translation = "VOICE OF" } },
				{ "WARMUP", new SettingsTranslation() { translation = "WARM UP" } },
			};

			foreach ( var item in defaultTranslationSettings )
			{
				if ( !settings.translationDictionary.ContainsKey( item.Key ) )
				{
					item.Value.id = item.Key;

					settings.translationDictionary[ item.Key ] = item.Value;
				}
			}
		}

		public static object Load( string filePath, Type type )
		{
			loading++;

			LogFile.Write( $"Loading {filePath}...\r\n" );

			var xmlSerializer = new XmlSerializer( type );

			var fileStream = new FileStream( filePath, FileMode.Open );

			var settingsData = xmlSerializer.Deserialize( fileStream ) ?? throw new Exception();

			fileStream.Close();

			loading--;

			return settingsData;
		}

		public static void SaveEditor()
		{
			try
			{
				Save( relativeEditorSettingsFilePath, editor );
			}
			catch ( IOException )
			{
				saveEditorToFileQueued = true;
				saveEditorToFileTimeRemaining = SaveToFileIntervalTime;
			}
		}

		public static void SaveOverlay()
		{
			try
			{
				Save( overlayLocal.filePath, overlayLocal );
			}
			catch ( IOException )
			{
				saveOverlayToFileQueued = true;
				saveOverlayToFileTimeRemaining = SaveToFileIntervalTime;
			}

			UpdateCombinedOverlay();
		}

		public static void SaveDirector()
		{
			try
			{
				Save( directorLocal.filePath, directorLocal );
			}
			catch ( IOException )
			{
				saveDirectorToFileQueued = true;
				saveDirectorToFileTimeRemaining = SaveToFileIntervalTime;
			}

			UpdateCombinedDirector();
		}

		public static void Save( string filePath, object settingsData )
		{
			LogFile.Write( $"Saving {filePath}...\r\n" );

			var xmlSerializer = new XmlSerializer( settingsData.GetType() );

			var streamWriter = new StreamWriter( filePath );

			xmlSerializer.Serialize( streamWriter, settingsData );

			streamWriter.Close();
		}

		public static void UpdateCombinedOverlay()
		{
			overlay = new SettingsOverlay
			{
				showBorders = overlayLocal.showBorders,

				position = overlayLocal.position_Overridden ? overlayLocal.position : overlayGlobal.position,
				size = overlayLocal.size_Overridden ? overlayLocal.size : overlayGlobal.size,
				driverCsvFilePath = overlayLocal.driverCsvFilePath_Overridden ? overlayLocal.driverCsvFilePath : overlayGlobal.driverCsvFilePath,
				stringsCsvFilePath = overlayLocal.stringsCsvFilePath_Overridden ? overlayLocal.stringsCsvFilePath : overlayGlobal.stringsCsvFilePath,
				trainerCsvFilePath = overlayLocal.trainerCsvFilePath_Overridden ? overlayLocal.trainerCsvFilePath : overlayGlobal.trainerCsvFilePath,

				position_Overridden = overlayLocal.position_Overridden,
				size_Overridden = overlayLocal.size_Overridden,
				driverCsvFilePath_Overridden = overlayLocal.driverCsvFilePath_Overridden,
				stringsCsvFilePath_Overridden = overlayLocal.stringsCsvFilePath_Overridden,
				trainerCsvFilePath_Overridden = overlayLocal.trainerCsvFilePath_Overridden,

				fontNames = new string[ SettingsOverlay.MaxNumFonts ] {
					overlayLocal.fontNames_Overridden[ 0 ] ? overlayLocal.fontNames[ 0 ] : overlayGlobal.fontNames[ 0 ],
					overlayLocal.fontNames_Overridden[ 1 ] ? overlayLocal.fontNames[ 1 ] : overlayGlobal.fontNames[ 1 ],
					overlayLocal.fontNames_Overridden[ 2 ] ? overlayLocal.fontNames[ 2 ] : overlayGlobal.fontNames[ 2 ],
					overlayLocal.fontNames_Overridden[ 3 ] ? overlayLocal.fontNames[ 3 ] : overlayGlobal.fontNames[ 3 ]
				},

				fontPaths = new string[ SettingsOverlay.MaxNumFonts ] {
					overlayLocal.fontNames_Overridden[ 0 ] ? overlayLocal.fontPaths[ 0 ] : overlayGlobal.fontPaths[ 0 ],
					overlayLocal.fontNames_Overridden[ 1 ] ? overlayLocal.fontPaths[ 1 ] : overlayGlobal.fontPaths[ 1 ],
					overlayLocal.fontNames_Overridden[ 2 ] ? overlayLocal.fontPaths[ 2 ] : overlayGlobal.fontPaths[ 2 ],
					overlayLocal.fontNames_Overridden[ 3 ] ? overlayLocal.fontPaths[ 3 ] : overlayGlobal.fontPaths[ 3 ]
				},

				fontNames_Overridden = new bool[ SettingsOverlay.MaxNumFonts ]
				{
					overlayLocal.fontNames_Overridden[ 0 ],
					overlayLocal.fontNames_Overridden[ 1 ],
					overlayLocal.fontNames_Overridden[ 2 ],
					overlayLocal.fontNames_Overridden[ 3 ]
				},

				startLightsEnabled = overlayLocal.startLightsEnabled_Overridden ? overlayLocal.startLightsEnabled : overlayGlobal.startLightsEnabled,
				startLightsPosition = overlayLocal.startLightsPosition_Overridden ? overlayLocal.startLightsPosition : overlayGlobal.startLightsPosition,

				startLightsEnabled_Overridden = overlayLocal.startLightsEnabled_Overridden,
				startLightsPosition_Overridden = overlayLocal.startLightsPosition_Overridden,

				raceStatusEnabled = overlayLocal.raceStatusEnabled_Overridden ? overlayLocal.raceStatusEnabled : overlayGlobal.raceStatusEnabled,
				raceStatusPosition = overlayLocal.raceStatusPosition_Overridden ? overlayLocal.raceStatusPosition : overlayGlobal.raceStatusPosition,

				raceStatusEnabled_Overridden = overlayLocal.raceStatusEnabled_Overridden,
				raceStatusPosition_Overridden = overlayLocal.raceStatusPosition_Overridden,

				leaderboardEnabled = overlayLocal.leaderboardEnabled_Overridden ? overlayLocal.leaderboardEnabled : overlayGlobal.leaderboardEnabled,
				leaderboardPosition = overlayLocal.leaderboardPosition_Overridden ? overlayLocal.leaderboardPosition : overlayGlobal.leaderboardPosition,
				leaderboardFirstSlotPosition = overlayLocal.leaderboardFirstSlotPosition_Overridden ? overlayLocal.leaderboardFirstSlotPosition : overlayGlobal.leaderboardFirstSlotPosition,
				leaderboardSlotCount = overlayLocal.leaderboardSlotCount_Overridden ? overlayLocal.leaderboardSlotCount : overlayGlobal.leaderboardSlotCount,
				leaderboardSlotSpacing = overlayLocal.leaderboardSlotSpacing_Overridden ? overlayLocal.leaderboardSlotSpacing : overlayGlobal.leaderboardSlotSpacing,
				leaderboardSeparateBoards = overlayLocal.leaderboardSeparateBoards_Overridden ? overlayLocal.leaderboardSeparateBoards : overlayGlobal.leaderboardSeparateBoards,
				leaderboardMultiClassOffset = overlayLocal.leaderboardMultiClassOffset_Overridden ? overlayLocal.leaderboardMultiClassOffset : overlayGlobal.leaderboardMultiClassOffset,
				leaderboardMultiClassOffsetType = overlayLocal.leaderboardMultiClassOffset_Overridden ? overlayLocal.leaderboardMultiClassOffsetType : overlayGlobal.leaderboardMultiClassOffsetType,

				leaderboardEnabled_Overridden = overlayLocal.leaderboardEnabled_Overridden,
				leaderboardPosition_Overridden = overlayLocal.leaderboardPosition_Overridden,
				leaderboardFirstSlotPosition_Overridden = overlayLocal.leaderboardFirstSlotPosition_Overridden,
				leaderboardSlotCount_Overridden = overlayLocal.leaderboardSlotCount_Overridden,
				leaderboardSlotSpacing_Overridden = overlayLocal.leaderboardSlotSpacing_Overridden,
				leaderboardSeparateBoards_Overridden = overlayLocal.leaderboardSeparateBoards_Overridden,
				leaderboardMultiClassOffset_Overridden = overlayLocal.leaderboardMultiClassOffset_Overridden,

				raceResultEnabled = overlayLocal.raceResultEnabled_Overridden ? overlayLocal.raceResultEnabled : overlayGlobal.raceResultEnabled,
				raceResultPosition = overlayLocal.raceResultPosition_Overridden ? overlayLocal.raceResultPosition : overlayGlobal.raceResultPosition,
				raceResultFirstSlotPosition = overlayLocal.raceResultFirstSlotPosition_Overridden ? overlayLocal.raceResultFirstSlotPosition : overlayGlobal.raceResultFirstSlotPosition,
				raceResultSlotCount = overlayLocal.raceResultSlotCount_Overridden ? overlayLocal.raceResultSlotCount : overlayGlobal.raceResultSlotCount,
				raceResultSlotSpacing = overlayLocal.raceResultSlotSpacing_Overridden ? overlayLocal.raceResultSlotSpacing : overlayGlobal.raceResultSlotSpacing,
				raceResultStartTime = overlayLocal.raceResultStartTime_Overridden ? overlayLocal.raceResultStartTime : overlayGlobal.raceResultStartTime,
				raceResultInterval = overlayLocal.raceResultInterval_Overridden ? overlayLocal.raceResultInterval : overlayGlobal.raceResultInterval,

				raceResultEnabled_Overridden = overlayLocal.raceResultEnabled_Overridden,
				raceResultPosition_Overridden = overlayLocal.raceResultPosition_Overridden,
				raceResultFirstSlotPosition_Overridden = overlayLocal.raceResultFirstSlotPosition_Overridden,
				raceResultSlotCount_Overridden = overlayLocal.raceResultSlotCount_Overridden,
				raceResultSlotSpacing_Overridden = overlayLocal.raceResultSlotSpacing_Overridden,
				raceResultStartTime_Overridden = overlayLocal.raceResultStartTime_Overridden,
				raceResultInterval_Overridden = overlayLocal.raceResultInterval_Overridden,

				trackMapEnabled = overlayLocal.trackMapEnabled_Overridden ? overlayLocal.trackMapEnabled : overlayGlobal.trackMapEnabled,
				trackMapReverse = overlayLocal.trackMapReverse_Overridden ? overlayLocal.trackMapReverse : overlayGlobal.trackMapReverse,
				trackMapPosition = overlayLocal.trackMapPosition_Overridden ? overlayLocal.trackMapPosition : overlayGlobal.trackMapPosition,
				trackMapSize = overlayLocal.trackMapSize_Overridden ? overlayLocal.trackMapSize : overlayGlobal.trackMapSize,
				trackMapTextureFilePath = overlayLocal.trackMapTextureFilePath_Overridden ? overlayLocal.trackMapTextureFilePath : overlayGlobal.trackMapTextureFilePath,
				trackMapLineThickness = overlayLocal.trackMapLineThickness_Overridden ? overlayLocal.trackMapLineThickness : overlayGlobal.trackMapLineThickness,
				trackMapLineColor = overlayLocal.trackMapLineColor_Overridden ? overlayLocal.trackMapLineColor : overlayGlobal.trackMapLineColor,
				trackMapStartFinishOffset = overlayLocal.trackMapStartFinishOffset_Overridden ? overlayLocal.trackMapStartFinishOffset : overlayGlobal.trackMapStartFinishOffset,

				trackMapEnabled_Overridden = overlayLocal.trackMapEnabled_Overridden,
				trackMapReverse_Overridden = overlayLocal.trackMapReverse_Overridden,
				trackMapPosition_Overridden = overlayLocal.trackMapPosition_Overridden,
				trackMapSize_Overridden = overlayLocal.trackMapSize_Overridden,
				trackMapTextureFilePath_Overridden = overlayLocal.trackMapTextureFilePath_Overridden,
				trackMapLineThickness_Overridden = overlayLocal.trackMapLineThickness_Overridden,
				trackMapLineColor_Overridden = overlayLocal.trackMapLineColor_Overridden,
				trackMapStartFinishOffset_Overridden = overlayLocal.trackMapStartFinishOffset_Overridden,

				pitLaneEnabled = overlayLocal.pitLaneEnabled_Overridden ? overlayLocal.pitLaneEnabled : overlayGlobal.pitLaneEnabled,
				pitLanePosition = overlayLocal.pitLanePosition_Overridden ? overlayLocal.pitLanePosition : overlayGlobal.pitLanePosition,
				pitLaneLength = overlayLocal.pitLaneLength_Overridden ? overlayLocal.pitLaneLength : overlayGlobal.pitLaneLength,

				pitLaneEnabled_Overridden = overlayLocal.pitLaneEnabled_Overridden,
				pitLanePosition_Overridden = overlayLocal.pitLanePosition_Overridden,
				pitLaneLength_Overridden = overlayLocal.pitLaneLength_Overridden,

				voiceOfEnabled = overlayLocal.voiceOfEnabled_Overridden ? overlayLocal.voiceOfEnabled : overlayGlobal.voiceOfEnabled,
				voiceOfPosition = overlayLocal.voiceOfPosition_Overridden ? overlayLocal.voiceOfPosition : overlayGlobal.voiceOfPosition,

				voiceOfEnabled_Overridden = overlayLocal.voiceOfEnabled_Overridden,
				voiceOfPosition_Overridden = overlayLocal.voiceOfPosition_Overridden,

				chyronEnabled = overlayLocal.chyronEnabled_Overridden ? overlayLocal.chyronEnabled : overlayGlobal.chyronEnabled,
				chyronShowDuringPractice = overlayLocal.chyronShowDuringPractice_Overridden ? overlayLocal.chyronShowDuringPractice : overlayGlobal.chyronShowDuringPractice,
				chyronShowDuringQualifying = overlayLocal.chyronShowDuringQualifying_Overridden ? overlayLocal.chyronShowDuringQualifying : overlayGlobal.chyronShowDuringQualifying,
				chyronShowDuringRace = overlayLocal.chyronShowDuringRace_Overridden ? overlayLocal.chyronShowDuringRace : overlayGlobal.chyronShowDuringRace,
				chyronPosition = overlayLocal.chyronPosition_Overridden ? overlayLocal.chyronPosition : overlayGlobal.chyronPosition,
				chyronDelay = overlayLocal.chyronDelay_Overridden ? overlayLocal.chyronDelay : overlayGlobal.chyronDelay,

				chyronEnabled_Overridden = overlayLocal.chyronEnabled_Overridden,
				chyronShowDuringPractice_Overridden = overlayLocal.chyronShowDuringPractice_Overridden,
				chyronShowDuringQualifying_Overridden = overlayLocal.chyronShowDuringQualifying_Overridden,
				chyronShowDuringRace_Overridden = overlayLocal.chyronShowDuringRace_Overridden,
				chyronPosition_Overridden = overlayLocal.chyronPosition_Overridden,
				chyronDelay_Overridden = overlayLocal.chyronDelay_Overridden,

				battleChyronEnabled = overlayLocal.battleChyronEnabled_Overridden ? overlayLocal.battleChyronEnabled : overlayGlobal.battleChyronEnabled,
				battleChyronPosition = overlayLocal.battleChyronPosition_Overridden ? overlayLocal.battleChyronPosition : overlayGlobal.battleChyronPosition,
				battleChyronDistance = overlayLocal.battleChyronDistance_Overridden ? overlayLocal.battleChyronDistance : overlayGlobal.battleChyronDistance,
				battleChyronDelay = overlayLocal.battleChyronDelay_Overridden ? overlayLocal.battleChyronDelay : overlayGlobal.battleChyronDelay,

				battleChyronEnabled_Overridden = overlayLocal.battleChyronEnabled_Overridden,
				battleChyronPosition_Overridden = overlayLocal.battleChyronPosition_Overridden,
				battleChyronDistance_Overridden = overlayLocal.battleChyronDistance_Overridden,
				battleChyronDelay_Overridden = overlayLocal.battleChyronDelay_Overridden,

				subtitleEnabled = overlayLocal.subtitleEnabled_Overridden ? overlayLocal.subtitleEnabled : overlayGlobal.subtitleEnabled,
				subtitlePosition = overlayLocal.subtitlePosition_Overridden ? overlayLocal.subtitlePosition : overlayGlobal.subtitlePosition,
				subtitleMaxSize = overlayLocal.subtitleMaxSize_Overridden ? overlayLocal.subtitleMaxSize : overlayGlobal.subtitleMaxSize,
				subtitleTextPadding = overlayLocal.subtitleTextPadding_Overridden ? overlayLocal.subtitleTextPadding : overlayGlobal.subtitleTextPadding,

				subtitleEnabled_Overridden = overlayLocal.subtitleEnabled_Overridden,
				subtitlePosition_Overridden = overlayLocal.subtitlePosition_Overridden,
				subtitleMaxSize_Overridden = overlayLocal.subtitleMaxSize_Overridden,
				subtitleTextPadding_Overridden = overlayLocal.subtitleTextPadding_Overridden,

				carNumberOverrideEnabled = overlayLocal.carNumberOverrideEnabled_Overridden ? overlayLocal.carNumberOverrideEnabled : overlayGlobal.carNumberOverrideEnabled,
				carNumberColorA = overlayLocal.carNumberColorA_Overridden ? overlayLocal.carNumberColorA : overlayGlobal.carNumberColorA,
				carNumberColorB = overlayLocal.carNumberColorB_Overridden ? overlayLocal.carNumberColorB : overlayGlobal.carNumberColorB,
				carNumberColorC = overlayLocal.carNumberColorC_Overridden ? overlayLocal.carNumberColorC : overlayGlobal.carNumberColorC,
				carNumberPattern = overlayLocal.carNumberPattern_Overridden ? overlayLocal.carNumberPattern : overlayGlobal.carNumberPattern,
				carNumberSlant = overlayLocal.carNumberSlant_Overridden ? overlayLocal.carNumberSlant : overlayGlobal.carNumberSlant,

				carNumberOverrideEnabled_Overridden = overlayLocal.carNumberOverrideEnabled_Overridden,
				carNumberColorA_Overridden = overlayLocal.carNumberColorA_Overridden,
				carNumberColorB_Overridden = overlayLocal.carNumberColorB_Overridden,
				carNumberColorC_Overridden = overlayLocal.carNumberColorC_Overridden,
				carNumberPattern_Overridden = overlayLocal.carNumberPattern_Overridden,
				carNumberSlant_Overridden = overlayLocal.carNumberSlant_Overridden,

				telemetryPitColor = overlayLocal.telemetryPitColor_Overridden ? overlayLocal.telemetryPitColor : overlayGlobal.telemetryPitColor,
				telemetryOutColor = overlayLocal.telemetryOutColor_Overridden ? overlayLocal.telemetryOutColor : overlayGlobal.telemetryOutColor,
				telemetryIsBetweenCars = overlayLocal.telemetryIsBetweenCars_Overridden ? overlayLocal.telemetryIsBetweenCars : overlayGlobal.telemetryIsBetweenCars,
				telemetryMode = overlayLocal.telemetryMode_Overridden ? overlayLocal.telemetryMode : overlayGlobal.telemetryMode,
				telemetryShowAsNegativeNumbers = overlayLocal.telemetryShowAsNegativeNumbers_Overridden ? overlayLocal.telemetryShowAsNegativeNumbers : overlayGlobal.telemetryShowAsNegativeNumbers,

				telemetryPitColor_Overridden = overlayLocal.telemetryPitColor_Overridden,
				telemetryOutColor_Overridden = overlayLocal.telemetryOutColor_Overridden,
				telemetryIsBetweenCars_Overridden = overlayLocal.telemetryIsBetweenCars_Overridden,
				telemetryMode_Overridden = overlayLocal.telemetryMode_Overridden,
				telemetryShowAsNegativeNumbers_Overridden = overlayLocal.telemetryShowAsNegativeNumbers_Overridden,

				introEnabled = overlayLocal.introEnabled_Overridden ? overlayLocal.introEnabled : overlayGlobal.introEnabled,
				introLeftPosition = overlayLocal.introLeftPosition_Overridden ? overlayLocal.introLeftPosition : overlayGlobal.introLeftPosition,
				introLeftScale = overlayLocal.introLeftScale_Overridden ? overlayLocal.introLeftScale : overlayGlobal.introLeftScale,
				introRightPosition = overlayLocal.introRightPosition_Overridden ? overlayLocal.introRightPosition : overlayGlobal.introRightPosition,
				introRightScale = overlayLocal.introRightScale_Overridden ? overlayLocal.introRightScale : overlayGlobal.introRightScale,
				introLeftStartTime = overlayLocal.introStartTime_Overridden ? overlayLocal.introLeftStartTime : overlayGlobal.introLeftStartTime,
				introRightStartTime = overlayLocal.introStartTime_Overridden ? overlayLocal.introRightStartTime : overlayGlobal.introRightStartTime,
				introStartInterval = overlayLocal.introStartInterval_Overridden ? overlayLocal.introStartInterval : overlayGlobal.introStartInterval,
				introLeftInAnimationNumber = overlayLocal.introInAnimationNumber_Overridden ? overlayLocal.introLeftInAnimationNumber : overlayGlobal.introLeftInAnimationNumber,
				introRightInAnimationNumber = overlayLocal.introInAnimationNumber_Overridden ? overlayLocal.introRightInAnimationNumber : overlayGlobal.introRightInAnimationNumber,
				introLeftOutAnimationNumber = overlayLocal.introOutAnimationNumber_Overridden ? overlayLocal.introLeftOutAnimationNumber : overlayGlobal.introLeftOutAnimationNumber,
				introRightOutAnimationNumber = overlayLocal.introOutAnimationNumber_Overridden ? overlayLocal.introRightOutAnimationNumber : overlayGlobal.introRightOutAnimationNumber,
				introInTime = overlayLocal.introInTime_Overridden ? overlayLocal.introInTime : overlayGlobal.introInTime,
				introHoldTime = overlayLocal.introHoldTime_Overridden ? overlayLocal.introHoldTime : overlayGlobal.introHoldTime,
				introOutTime = overlayLocal.introOutTime_Overridden ? overlayLocal.introOutTime : overlayGlobal.introOutTime,

				introEnabled_Overridden = overlayLocal.introEnabled_Overridden,
				introLeftPosition_Overridden = overlayLocal.introLeftPosition_Overridden,
				introLeftScale_Overridden = overlayLocal.introLeftScale_Overridden,
				introRightPosition_Overridden = overlayLocal.introRightPosition_Overridden,
				introRightScale_Overridden = overlayLocal.introRightScale_Overridden,
				introStartTime_Overridden = overlayLocal.introStartTime_Overridden,
				introStartInterval_Overridden = overlayLocal.introStartInterval_Overridden,
				introInAnimationNumber_Overridden = overlayLocal.introInAnimationNumber_Overridden,
				introOutAnimationNumber_Overridden = overlayLocal.introOutAnimationNumber_Overridden,
				introInTime_Overridden = overlayLocal.introInTime_Overridden,
				introHoldTime_Overridden = overlayLocal.introHoldTime_Overridden,
				introOutTime_Overridden = overlayLocal.introOutTime_Overridden,

				hudEnabled = overlayLocal.hudEnabled_Overridden ? overlayLocal.hudEnabled : overlayGlobal.hudEnabled,
				hudPosition = overlayLocal.hudPosition_Overridden ? overlayLocal.hudPosition : overlayGlobal.hudPosition,
				hudSpeechToTextPosition = overlayLocal.hudSpeechToTextPosition_Overridden ? overlayLocal.hudSpeechToTextPosition : overlayGlobal.hudSpeechToTextPosition,
				hudSpeechToTextMaxSize = overlayLocal.hudSpeechToTextMaxSize_Overridden ? overlayLocal.hudSpeechToTextMaxSize : overlayGlobal.hudSpeechToTextMaxSize,
				hudSpeechToTextTextPadding = overlayLocal.hudSpeechToTextTextPadding_Overridden ? overlayLocal.hudSpeechToTextTextPadding : overlayGlobal.hudSpeechToTextTextPadding,
				hudLocalWebcamPosition = overlayLocal.hudLocalWebcamPosition_Overridden ? overlayLocal.hudLocalWebcamPosition : overlayGlobal.hudLocalWebcamPosition,
				hudLocalWebcamSize = overlayLocal.hudLocalWebcamSize_Overridden ? overlayLocal.hudLocalWebcamSize : overlayGlobal.hudLocalWebcamSize,
				hudRemoteWebcamPosition = overlayLocal.hudRemoteWebcamPosition_Overridden ? overlayLocal.hudRemoteWebcamPosition : overlayGlobal.hudRemoteWebcamPosition,
				hudRemoteWebcamSize = overlayLocal.hudRemoteWebcamSize_Overridden ? overlayLocal.hudRemoteWebcamSize : overlayGlobal.hudRemoteWebcamSize,

				hudEnabled_Overridden = overlayLocal.hudEnabled_Overridden,
				hudPosition_Overridden = overlayLocal.hudPosition_Overridden,
				hudSpeechToTextPosition_Overridden = overlayLocal.hudSpeechToTextPosition_Overridden,
				hudSpeechToTextMaxSize_Overridden = overlayLocal.hudSpeechToTextMaxSize_Overridden,
				hudSpeechToTextTextPadding_Overridden = overlayLocal.hudSpeechToTextTextPadding_Overridden,
				hudLocalWebcamPosition_Overridden = overlayLocal.hudLocalWebcamPosition_Overridden,
				hudLocalWebcamSize_Overridden = overlayLocal.hudLocalWebcamSize_Overridden,
				hudRemoteWebcamPosition_Overridden = overlayLocal.hudRemoteWebcamPosition_Overridden,
				hudRemoteWebcamSize_Overridden = overlayLocal.hudRemoteWebcamSize_Overridden,

				trainerEnabled = overlayLocal.trainerEnabled_Overridden ? overlayLocal.trainerEnabled : overlayGlobal.trainerEnabled,
				trainerPosition = overlayLocal.trainerPosition_Overridden ? overlayLocal.trainerPosition : overlayGlobal.trainerPosition,

				trainerEnabled_Overridden = overlayLocal.trainerEnabled_Overridden,
				trainerPosition_Overridden = overlayLocal.trainerPosition_Overridden,
			};

			foreach ( var item in overlayLocal.imageSettingsDataDictionary )
			{
				if ( overlayGlobal.imageSettingsDataDictionary.ContainsKey( item.Key ) )
				{
					var globalItem = overlayGlobal.imageSettingsDataDictionary[ item.Key ];

					overlay.imageSettingsDataDictionary[ item.Key ] = new SettingsImage()
					{
						imageType = item.Value.imageType_Overridden ? item.Value.imageType : globalItem.imageType,
						fallbackType = item.Value.fallbackType_Overridden ? item.Value.fallbackType : globalItem.fallbackType,
						filePath = item.Value.filePath_Overridden ? item.Value.filePath : globalItem.filePath,
						position = item.Value.position_Overridden ? item.Value.position : globalItem.position,
						size = item.Value.size_Overridden ? item.Value.size : globalItem.size,
						tintColor = item.Value.tintColor_Overridden ? item.Value.tintColor : globalItem.tintColor,
						border = item.Value.border_Overridden ? item.Value.border : globalItem.border,
						frameSize = item.Value.frames_Overridden ? item.Value.frameSize : globalItem.frameSize,
						frameCount = item.Value.frames_Overridden ? item.Value.frameCount : globalItem.frameCount,
						animationSpeed = item.Value.animationSpeed_Overridden ? item.Value.animationSpeed : globalItem.animationSpeed,
						tilingEnabled = item.Value.tilingEnabled_Overridden ? item.Value.tilingEnabled : globalItem.tilingEnabled,
						useClassColors = item.Value.useClassColors_Overridden ? item.Value.useClassColors : globalItem.useClassColors,
						classColorStrength = item.Value.classColorStrength_Overridden ? item.Value.classColorStrength : globalItem.classColorStrength,

						imageType_Overridden = item.Value.imageType_Overridden,
						fallbackType_Overridden = item.Value.fallbackType_Overridden,
						filePath_Overridden = item.Value.filePath_Overridden,
						position_Overridden = item.Value.position_Overridden,
						size_Overridden = item.Value.size_Overridden,
						tintColor_Overridden = item.Value.tintColor_Overridden,
						border_Overridden = item.Value.border_Overridden,
						frames_Overridden = item.Value.frames_Overridden,
						animationSpeed_Overridden = item.Value.animationSpeed_Overridden,
						tilingEnabled_Overridden = item.Value.tilingEnabled_Overridden,
						useClassColors_Overridden = item.Value.useClassColors_Overridden,
						classColorStrength_Overridden = item.Value.classColorStrength_Overridden
					};
				}
			}

			foreach ( var item in overlayLocal.textSettingsDataDictionary )
			{
				if ( overlayGlobal.textSettingsDataDictionary.ContainsKey( item.Key ) )
				{
					var globalItem = overlayGlobal.textSettingsDataDictionary[ item.Key ];

					overlay.textSettingsDataDictionary[ item.Key ] = new SettingsText()
					{
						fontIndex = item.Value.fontIndex_Overridden ? item.Value.fontIndex : globalItem.fontIndex,
						fontSize = item.Value.fontSize_Overridden ? item.Value.fontSize : globalItem.fontSize,
						alignment = item.Value.alignment_Overridden ? item.Value.alignment : globalItem.alignment,
						position = item.Value.position_Overridden ? item.Value.position : globalItem.position,
						size = item.Value.size_Overridden ? item.Value.size : globalItem.size,
						tintColor = item.Value.tintColor_Overridden ? item.Value.tintColor : globalItem.tintColor,
						useClassColors = item.Value.useClassColors_Overridden ? item.Value.useClassColors : globalItem.useClassColors,
						classColorStrength = item.Value.classColorStrength_Overridden ? item.Value.classColorStrength : globalItem.classColorStrength,
						allowOverflow = item.Value.allowOverflow_Overridden ? item.Value.allowOverflow : globalItem.allowOverflow,
						content = item.Value.content_Overridden ? item.Value.content : globalItem.content,
						csvProperty = item.Value.csvProperty_Overridden ? item.Value.csvProperty : globalItem.csvProperty,

						fontIndex_Overridden = item.Value.fontIndex_Overridden,
						fontSize_Overridden = item.Value.fontSize_Overridden,
						alignment_Overridden = item.Value.alignment_Overridden,
						position_Overridden = item.Value.position_Overridden,
						size_Overridden = item.Value.size_Overridden,
						tintColor_Overridden = item.Value.tintColor_Overridden,
						useClassColors_Overridden = item.Value.useClassColors_Overridden,
						classColorStrength_Overridden = item.Value.classColorStrength_Overridden,
						allowOverflow_Overridden = item.Value.allowOverflow_Overridden,
						content_Overridden = item.Value.content_Overridden,
						csvProperty_Overridden = item.Value.csvProperty_Overridden,
					};
				}
			}

			foreach ( var item in overlayLocal.translationDictionary )
			{
				if ( overlayGlobal.translationDictionary.ContainsKey( item.Key ) )
				{
					var globalItem = overlayGlobal.translationDictionary[ item.Key ];

					overlay.translationDictionary[ item.Key ] = new SettingsTranslation()
					{
						id = item.Value.id,

						translation = item.Value.translation_Overridden ? item.Value.translation : globalItem.translation,

						translation_Overridden = item.Value.translation_Overridden
					};
				}
			}
		}

		public static void UpdateCombinedDirector()
		{
			director = new SettingsDirector()
			{
				camerasPractice = directorLocal.camerasPractice_Overridden ? directorLocal.camerasPractice : directorGlobal.camerasPractice,
				camerasQualifying = directorLocal.camerasQualifying_Overridden ? directorLocal.camerasQualifying : directorGlobal.camerasQualifying,
				camerasIntro = directorLocal.camerasIntro_Overridden ? directorLocal.camerasIntro : directorGlobal.camerasIntro,

				camerasScenic = directorLocal.camerasScenic_Overridden ? directorLocal.camerasScenic : directorGlobal.camerasScenic,
				camerasPits = directorLocal.camerasPits_Overridden ? directorLocal.camerasPits : directorGlobal.camerasPits,
				camerasStartFinish = directorLocal.camerasStartFinish_Overridden ? directorLocal.camerasStartFinish : directorGlobal.camerasStartFinish,
				camerasReverse = directorLocal.camerasReverse_Overridden ? directorLocal.camerasReverse : directorGlobal.camerasReverse,

				camerasInside = directorLocal.camerasInside_Overridden ? directorLocal.camerasInside : directorGlobal.camerasInside,
				camerasClose = directorLocal.camerasClose_Overridden ? directorLocal.camerasClose : directorGlobal.camerasClose,
				camerasMedium = directorLocal.camerasMedium_Overridden ? directorLocal.camerasMedium : directorGlobal.camerasMedium,
				camerasFar = directorLocal.camerasFar_Overridden ? directorLocal.camerasFar : directorGlobal.camerasFar,
				camerasVeryFar = directorLocal.camerasVeryFar_Overridden ? directorLocal.camerasVeryFar : directorGlobal.camerasVeryFar,

				camerasCustom1 = directorLocal.camerasCustom1_Overridden ? directorLocal.camerasCustom1 : directorGlobal.camerasCustom1,
				camerasCustom2 = directorLocal.camerasCustom2_Overridden ? directorLocal.camerasCustom2 : directorGlobal.camerasCustom2,
				camerasCustom3 = directorLocal.camerasCustom3_Overridden ? directorLocal.camerasCustom3 : directorGlobal.camerasCustom3,
				camerasCustom4 = directorLocal.camerasCustom4_Overridden ? directorLocal.camerasCustom4 : directorGlobal.camerasCustom4,
				camerasCustom5 = directorLocal.camerasCustom5_Overridden ? directorLocal.camerasCustom5 : directorGlobal.camerasCustom5,
				camerasCustom6 = directorLocal.camerasCustom6_Overridden ? directorLocal.camerasCustom6 : directorGlobal.camerasCustom6,

				camerasPractice_Overridden = directorLocal.camerasPractice_Overridden,
				camerasQualifying_Overridden = directorLocal.camerasQualifying_Overridden,
				camerasIntro_Overridden = directorLocal.camerasIntro_Overridden,

				camerasScenic_Overridden = directorLocal.camerasScenic_Overridden,
				camerasPits_Overridden = directorLocal.camerasPits_Overridden,
				camerasStartFinish_Overridden = directorLocal.camerasStartFinish_Overridden,
				camerasReverse_Overridden = directorLocal.camerasReverse_Overridden,

				camerasInside_Overridden = directorLocal.camerasInside_Overridden,
				camerasClose_Overridden = directorLocal.camerasClose_Overridden,
				camerasMedium_Overridden = directorLocal.camerasMedium_Overridden,
				camerasFar_Overridden = directorLocal.camerasFar_Overridden,
				camerasVeryFar_Overridden = directorLocal.camerasVeryFar_Overridden,

				camerasCustom1_Overridden = directorLocal.camerasCustom1_Overridden,
				camerasCustom2_Overridden = directorLocal.camerasCustom2_Overridden,
				camerasCustom3_Overridden = directorLocal.camerasCustom3_Overridden,
				camerasCustom4_Overridden = directorLocal.camerasCustom4_Overridden,
				camerasCustom5_Overridden = directorLocal.camerasCustom5_Overridden,
				camerasCustom6_Overridden = directorLocal.camerasCustom6_Overridden,

				switchDelayDirector = directorLocal.switchDelayDirector_Overridden ? directorLocal.switchDelayDirector : directorGlobal.switchDelayDirector,
				switchDelayIracing = directorLocal.switchDelayIracing_Overridden ? directorLocal.switchDelayIracing : directorGlobal.switchDelayIracing,
				switchDelayRadioChatter = directorLocal.switchDelayRadioChatter_Overridden ? directorLocal.switchDelayRadioChatter : directorGlobal.switchDelayRadioChatter,
				switchDelayNotInRace = directorLocal.switchDelayNotInRace_Overridden ? directorLocal.switchDelayNotInRace : directorGlobal.switchDelayNotInRace,

				switchDelayDirector_Overridden = directorLocal.switchDelayDirector_Overridden,
				switchDelayIracing_Overridden = directorLocal.switchDelayIracing_Overridden,
				switchDelayRadioChatter_Overridden = directorLocal.switchDelayRadioChatter_Overridden,
				switchDelayNotInRace_Overridden = directorLocal.switchDelayNotInRace_Overridden,

				heatMaxGapTime = directorLocal.heatMaxGapTime_Overridden ? directorLocal.heatMaxGapTime : directorGlobal.heatMaxGapTime,
				heatOvertakeBonus = directorLocal.heatOvertakeBonus_Overridden ? directorLocal.heatOvertakeBonus : directorGlobal.heatOvertakeBonus,
				heatPositionBattle = directorLocal.heatPositionBattle_Overridden ? directorLocal.heatPositionBattle : directorGlobal.heatPositionBattle,
				heatBias = directorLocal.heatBias_Overridden ? directorLocal.heatBias : directorGlobal.heatBias,

				heatMaxGapTime_Overridden = directorLocal.heatMaxGapTime_Overridden,
				heatOvertakeBonus_Overridden = directorLocal.heatOvertakeBonus_Overridden,
				heatPositionBattle_Overridden = directorLocal.heatPositionBattle_Overridden,
				heatBias_Overridden = directorLocal.heatBias_Overridden,

				preferredCarUserIds = directorLocal.preferredCarUserIds_Overridden ? directorLocal.preferredCarUserIds : directorGlobal.preferredCarUserIds,
				preferredCarCarNumbers = directorLocal.preferredCarCarNumbers_Overridden ? directorLocal.preferredCarCarNumbers : directorGlobal.preferredCarCarNumbers,
				preferredCarLockOnEnabled = directorLocal.preferredCarLockOnEnabled_Overridden ? directorLocal.preferredCarLockOnEnabled : directorGlobal.preferredCarLockOnEnabled,
				preferredCarLockOnMinimumHeat = directorLocal.preferredCarLockOnMinimumHeat_Overridden ? directorLocal.preferredCarLockOnMinimumHeat : directorGlobal.preferredCarLockOnMinimumHeat,

				preferredCarUserIds_Overridden = directorLocal.preferredCarUserIds_Overridden,
				preferredCarCarNumbers_Overridden = directorLocal.preferredCarCarNumbers_Overridden,
				preferredCarLockOnEnabled_Overridden = directorLocal.preferredCarLockOnEnabled_Overridden,
				preferredCarLockOnMinimumHeat_Overridden = directorLocal.preferredCarLockOnMinimumHeat_Overridden,

				rule1_Enabled = directorLocal.rules_Overridden ? directorLocal.rule1_Enabled : directorGlobal.rule1_Enabled,
				rule1_Camera = directorLocal.rules_Overridden ? directorLocal.rule1_Camera : directorGlobal.rule1_Camera,
				rule2_Enabled = directorLocal.rules_Overridden ? directorLocal.rule2_Enabled : directorGlobal.rule2_Enabled,
				rule2_Camera = directorLocal.rules_Overridden ? directorLocal.rule2_Camera : directorGlobal.rule2_Camera,
				rule3_Enabled = directorLocal.rules_Overridden ? directorLocal.rule3_Enabled : directorGlobal.rule3_Enabled,
				rule3_Camera = directorLocal.rules_Overridden ? directorLocal.rule3_Camera : directorGlobal.rule3_Camera,
				rule4_Enabled = directorLocal.rules_Overridden ? directorLocal.rule4_Enabled : directorGlobal.rule4_Enabled,
				rule4_Camera = directorLocal.rules_Overridden ? directorLocal.rule4_Camera : directorGlobal.rule4_Camera,
				rule5_Enabled = directorLocal.rules_Overridden ? directorLocal.rule5_Enabled : directorGlobal.rule5_Enabled,
				rule5_Camera = directorLocal.rules_Overridden ? directorLocal.rule5_Camera : directorGlobal.rule5_Camera,
				rule6_Enabled = directorLocal.rules_Overridden ? directorLocal.rule6_Enabled : directorGlobal.rule6_Enabled,
				rule6_Camera = directorLocal.rules_Overridden ? directorLocal.rule6_Camera : directorGlobal.rule6_Camera,
				rule7_Enabled = directorLocal.rules_Overridden ? directorLocal.rule7_Enabled : directorGlobal.rule7_Enabled,
				rule7_Camera = directorLocal.rules_Overridden ? directorLocal.rule7_Camera : directorGlobal.rule7_Camera,
				rule8_Enabled = directorLocal.rules_Overridden ? directorLocal.rule8_Enabled : directorGlobal.rule8_Enabled,
				rule8_Camera = directorLocal.rules_Overridden ? directorLocal.rule8_Camera : directorGlobal.rule8_Camera,
				rule9_Enabled = directorLocal.rules_Overridden ? directorLocal.rule9_Enabled : directorGlobal.rule9_Enabled,
				rule9_Camera = directorLocal.rules_Overridden ? directorLocal.rule9_Camera : directorGlobal.rule9_Camera,
				rule10_Enabled = directorLocal.rules_Overridden ? directorLocal.rule10_Enabled : directorGlobal.rule10_Enabled,
				rule10_Camera = directorLocal.rules_Overridden ? directorLocal.rule10_Camera : directorGlobal.rule10_Camera,
				rule11_Enabled = directorLocal.rules_Overridden ? directorLocal.rule11_Enabled : directorGlobal.rule11_Enabled,
				rule11_Camera = directorLocal.rules_Overridden ? directorLocal.rule11_Camera : directorGlobal.rule11_Camera,
				rule12_Enabled = directorLocal.rules_Overridden ? directorLocal.rule12_Enabled : directorGlobal.rule12_Enabled,
				rule12_Camera = directorLocal.rules_Overridden ? directorLocal.rule12_Camera : directorGlobal.rule12_Camera,
				rule14_Enabled = directorLocal.rules_Overridden ? directorLocal.rule14_Enabled : directorGlobal.rule14_Enabled,
				rule14_Camera = directorLocal.rules_Overridden ? directorLocal.rule14_Camera : directorGlobal.rule14_Camera,

				rules_Overridden = directorLocal.rules_Overridden,

				autoCamInsideMinimum = directorLocal.autoCam_Overridden ? directorLocal.autoCamInsideMinimum : directorGlobal.autoCamInsideMinimum,
				autoCamInsideMaximum = directorLocal.autoCam_Overridden ? directorLocal.autoCamInsideMaximum : directorGlobal.autoCamInsideMaximum,
				autoCamCloseMaximum = directorLocal.autoCam_Overridden ? directorLocal.autoCamCloseMaximum : directorGlobal.autoCamCloseMaximum,
				autoCamMediumMaximum = directorLocal.autoCam_Overridden ? directorLocal.autoCamMediumMaximum : directorGlobal.autoCamMediumMaximum,
				autoCamFarMaximum = directorLocal.autoCam_Overridden ? directorLocal.autoCamFarMaximum : directorGlobal.autoCamFarMaximum,

				autoCam_Overridden = directorLocal.autoCam_Overridden
			};
		}

		public static string GetFullPath( string relativePath )
		{
			if ( Path.IsPathFullyQualified( relativePath ) )
			{
				return relativePath;
			}
			else
			{
				return Path.GetFullPath( relativePath, Program.documentsFolder );
			}
		}

		public static string GetRelativePath( string fullPath )
		{
			if ( fullPath == string.Empty )
			{
				return string.Empty;
			}
			else
			{
				return Path.GetRelativePath( Program.documentsFolder, fullPath );
			}
		}
	}
}
