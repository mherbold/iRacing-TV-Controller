
using System;
using System.Collections.Generic;
using System.IO;
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
		public const string GlobalSettingsFileName = "Global.xml";

		public static string editorSettingsFolder = Program.documentsFolder;
		public static string overlaySettingsFolder = Program.documentsFolder + OverlaySettingsFolderName + "\\";
		public static string directorSettingsFolder = Program.documentsFolder + DirectorSettingsFolderName + "\\";

		public static string editorSettingsFilePath = editorSettingsFolder + EditorSettingsFileName;
		public static string globalOverlaySettingsFilePath = overlaySettingsFolder + GlobalSettingsFileName;
		public static string globalDirectorSettingsFilePath = directorSettingsFolder + GlobalSettingsFileName;

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
			AddMissingDictionaryItems( overlayGlobal );
			AddMissingDictionaryItems( overlayLocal );

			if ( !Directory.Exists( Program.documentsFolder ) )
			{
				Directory.CreateDirectory( Program.documentsFolder );
			}

			if ( !Directory.Exists( overlaySettingsFolder ) )
			{
				Directory.CreateDirectory( overlaySettingsFolder );
			}

			if ( !Directory.Exists( directorSettingsFolder ) )
			{
				Directory.CreateDirectory( directorSettingsFolder );
			}

			// editor

			if ( File.Exists( editorSettingsFilePath ) )
			{
				try
				{
					editor = (SettingsEditor) Load( editorSettingsFilePath, typeof( SettingsEditor ) );
				}
				catch ( Exception exception )
				{
					MessageBox.Show( MainWindow.Instance, $"We could not load the editor settings file '{editorSettingsFilePath}'.\r\n\r\nThe error message is as follows:\r\n\r\n{exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
				}
			}
			else
			{
				Save( editorSettingsFilePath, editor );
			}

			// overlay

			if ( !File.Exists( globalOverlaySettingsFilePath ) )
			{
				overlayGlobal.filePath = globalOverlaySettingsFilePath;

				Save( globalOverlaySettingsFilePath, overlayGlobal );
			}

			var overlaySettingsFilePaths = Directory.EnumerateFiles( overlaySettingsFolder );

			foreach ( var overlaySettingsFilePath in overlaySettingsFilePaths )
			{
				try
				{
					var settings = (SettingsOverlay) Load( overlaySettingsFilePath, typeof( SettingsOverlay ) );

					settings.filePath = overlaySettingsFilePath;

					AddMissingDictionaryItems( settings );

					if ( overlaySettingsFilePath == globalOverlaySettingsFilePath )
					{
						overlayGlobal = settings;
					}
					else
					{
						overlayList.Add( settings );

						if ( settings.filePath == editor.lastActiveOverlayFilePath )
						{
							overlayLocal = settings;
						}
					}
				}
				catch ( Exception exception )
				{
					MessageBox.Show( MainWindow.Instance, $"We could not load the overlay settings file '{overlaySettingsFilePath}'.\r\n\r\nThe error message is as follows:\r\n\r\n{exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
				}
			}

			if ( overlayLocal.filePath == string.Empty )
			{
				if ( overlayList.Count > 0 )
				{
					overlayLocal = overlayList[ 0 ];
				}
				else
				{
					overlayLocal.filePath = overlaySettingsFolder + "My new overlay.xml";

					overlayList.Add( overlayLocal );

					saveOverlayToFileQueued = true;
				}
			}

			// director

			if ( !File.Exists( globalDirectorSettingsFilePath ) )
			{
				directorGlobal.filePath = globalDirectorSettingsFilePath;

				Save( globalDirectorSettingsFilePath, directorGlobal );
			}

			var directorSettingsFilePaths = Directory.EnumerateFiles( directorSettingsFolder );

			foreach ( var directorSettingsFilePath in directorSettingsFilePaths )
			{
				try
				{
					var settings = (SettingsDirector) Load( directorSettingsFilePath, typeof( SettingsDirector ) );

					settings.filePath = directorSettingsFilePath;

					if ( directorSettingsFilePath == globalDirectorSettingsFilePath )
					{
						directorGlobal = settings;
					}
					else
					{
						directorList.Add( settings );

						if ( settings.filePath == editor.lastActiveDirectorFilePath )
						{
							directorLocal = settings;
						}
					}
				}
				catch ( Exception exception )
				{
					MessageBox.Show( MainWindow.Instance, $"We could not load the director settings file '{directorSettingsFilePath}'.\r\n\r\nThe error message is as follows:\r\n\r\n{exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
				}
			}

			if ( directorLocal.filePath == string.Empty )
			{
				if ( directorList.Count > 0 )
				{
					directorLocal = directorList[ 0 ];
				}
				else
				{
					directorLocal.filePath = directorSettingsFolder + "My new director.xml";

					directorList.Add( directorLocal );

					saveDirectorToFileQueued = true;
				}
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

		public static void AddMissingDictionaryItems( SettingsOverlay settings )
		{
			var defaultImageSettings = new Dictionary<string, SettingsImage>() {
				{ "BlackLight", new SettingsImage(){ imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\light-black.png", position = { x = 280, y = 130 } } },
				{ "CarNumber", new SettingsImage() { imageType = SettingsImage.ImageType.CarNumber, position = { x = 48, y = 10 }, size = { x = 56, y = 28 } } },
				{ "CheckeredFlag", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\flag-checkered.png" } },
				{ "Custom1", new SettingsImage() },
				{ "Custom2", new SettingsImage() },
				{ "Custom3", new SettingsImage() },
				{ "Custom4", new SettingsImage() },
				{ "GreenFlag", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\flag-green.png" } },
				{ "GreenLight", new SettingsImage(){ imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\light-green.png", position = { x = 280, y = 130 } } },
				{ "Highlight", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\current-target.png" } },
				{ "IntroBackground", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\leaderboard.png", position = { x = 44, y = 155 }, size = { x = 1826, y = 660 }, border = { x = 32, y = 32, z = 32, w = 32 } } },
				{ "IntroDriverBackground", new SettingsImage() { imageType = SettingsImage.ImageType.None, position = { x = -203, y = -177 }, size = { x = 408, y = 336 } } },
				{ "IntroDriverCar", new SettingsImage() { imageType = SettingsImage.ImageType.Car, position = { x = -254, y = -113 }, size = { x = 508, y = 272 } } },
				{ "IntroDriverCarNumber", new SettingsImage() { imageType = SettingsImage.ImageType.CarNumber, position = { x = 97, y = 73 }, size = { x = 73, y = 52 } } },
				{ "IntroDriverHelmet", new SettingsImage() { imageType = SettingsImage.ImageType.Helmet, position = { x = -175, y = -159 }, size = { x = 100, y = 100 } } },
				{ "IntroDriverSuit", new SettingsImage() { imageType = SettingsImage.ImageType.Driver, position = { x = 13, y = -125 }, size = { x = 150, y = 250 } } },
				{ "IntroStatsBackground", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\leaderboard.png", position = { x = -178, y = 53 }, size = { x = 357, y = 90 }, border = { x = 32, y = 32, z = 32, w = 32 } } },
				{ "LeaderboardBackground", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\leaderboard.png", size = { x = 319, y = 8 }, border = { x = 32, y = 32, z = 32, w = 32 } } },
				{ "PositionSplitter", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\position-splitter.png" } },
				{ "RaceStatusBackground", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\race-status.png" } },
				{ "SeriesLogo", new SettingsImage() { imageType = SettingsImage.ImageType.SeriesLogo, position = { x = 7, y = 7 }, size = { x = 305, y = 103 } } },
				{ "StartLightsGo", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\lights-go.png" } },
				{ "StartLightsReady", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\lights-ready.png" } },
				{ "StartLightsSet", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\lights-set.png" } },
				{ "VoiceOfBackground", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\voice-of.png" } },
				{ "VoiceOfCar", new SettingsImage() { imageType = SettingsImage.ImageType.Car, position = { x = 220, y = -48 }, size = { x = 400, y = 200 } } },
				{ "WhiteLight", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\light-white.png", position = { x = 280, y = 130 } } },
				{ "YellowFlag", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\flag-yellow.png" } },
				{ "YellowLight", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Assets\\light-yellow.png", position = { x = 280, y = 130 } } },
			};

			var oldImageSettingNames = new Dictionary<string, string>()
			{
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

			var defaultTextSettings = new Dictionary<string, SettingsText>()
			{
				{ "CurrentLap", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 27, alignment = TextAlignmentOptions.TopRight, position = { x = 298, y = 175 }, tintColor = { r = 0.737f, g = 0.741f, b = 0.725f } } },
				{ "DriverName", new SettingsText() { fontIndex = SettingsText.FontIndex.FontB, fontSize = 21, position = { x = 108, y = 12 }, tintColor = { r = 0.69f, g = 0.71f, b = 0.694f } } },
				{ "IntroStatsPosition", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 50, position = { x = -151, y = 68 } } },
				{ "IntroStatsQualifyingTime", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 27, alignment = TextAlignmentOptions.Top, position = { x = 0, y = 99 }, tintColor = { r = 0.306f, g = 0.832f, b = 1 } } },
				{ "IntroStatsDriverName", new SettingsText() { fontIndex = SettingsText.FontIndex.FontB, fontSize = 27, alignment = TextAlignmentOptions.Top, position = { x = 0, y = 70 } } },
				{ "LapsRemaining", new SettingsText() { fontIndex = SettingsText.FontIndex.FontB, fontSize = 27, alignment = TextAlignmentOptions.TopRight, position = { x = 269, y = 125 }, tintColor = { r = 0.961f, g = 0.961f, b = 0.953f } } },
				{ "Position", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 21, alignment = TextAlignmentOptions.TopRight, position = { x = 43, y = 12 }, tintColor = { r = 0.69f, g = 0.71f, b = 0.694f } } },
				{ "SessionName", new SettingsText() { fontIndex = SettingsText.FontIndex.FontB, fontSize = 27, position = { x = 18, y = 125 }, tintColor = { r = 0.961f, g = 0.961f, b = 0.953f } } },
				{ "Speed", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 21, alignment = TextAlignmentOptions.TopRight, position = { x = 397, y = 12 }, tintColor = { r = 0.69f, g = 0.71f, b = 0.694f } } },
				{ "Subtitles", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 39, alignment = TextAlignmentOptions.Center, tintColor = { r = 0.961f, g = 0.961f, b = 0.953f } } },
				{ "Telemetry", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 21, alignment = TextAlignmentOptions.TopRight, position = { x = 298, y = 12 }, tintColor = { r = 0.69f, g = 0.71f, b = 0.694f } } },
				{ "Units", new SettingsText() { fontIndex = SettingsText.FontIndex.FontB, fontSize = 27, position = { x = 18, y = 175 }, tintColor = { r = 0.737f, g = 0.741f, b = 0.725f } } },
				{ "VoiceOf", new SettingsText() { fontIndex = SettingsText.FontIndex.FontB, fontSize = 30, position = { x = 30, y = 10 }, tintColor = { r = 0.434f, g = 0.434f, b = 0.434f } } },
				{ "VoiceOfDriverName", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 38, position = { x = 30, y = 41 }, tintColor = { r = 0.137f, g = 0.122f, b = 0.125f } } },
			};

			var oldTextSettingNames = new Dictionary<string, string>()
			{
				{ "Place", "Position" }
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
				{ "FeetAbbreviation", new SettingsTranslation() { translation = "FT" } },
				{ "FinalLap", new SettingsTranslation() { translation = "FINAL LAP" } },
				{ "KPH", new SettingsTranslation() { translation = "KPH" } },
				{ "Lap", new SettingsTranslation() { translation = "LAP" } },
				{ "LapsAbbreviation", new SettingsTranslation() { translation = "L" } },
				{ "MetersAbbreviation", new SettingsTranslation() { translation = "M" } },
				{ "MPH", new SettingsTranslation() { translation = "MPH" } },
				{ "Out", new SettingsTranslation() { translation = "OUT" } },
				{ "Pit", new SettingsTranslation() { translation = "PIT" } },
				{ "Time", new SettingsTranslation() { translation = "TIME" } },
				{ "ToGo", new SettingsTranslation() { translation = "TO GO" } },
				{ "VoiceOf", new SettingsTranslation() { translation = "VOICE OF" } },
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
				Save( editorSettingsFilePath, editor );
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
				Save( overlayGlobal.filePath, overlayGlobal );
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
				Save( directorGlobal.filePath, directorGlobal );
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
			var xmlSerializer = new XmlSerializer( settingsData.GetType() );

			var streamWriter = new StreamWriter( filePath );

			xmlSerializer.Serialize( streamWriter, settingsData );

			streamWriter.Close();
		}

		public static void UpdateCombinedOverlay()
		{
			overlay = new SettingsOverlay
			{
				position = overlayLocal.position_Overridden ? overlayLocal.position : overlayGlobal.position,
				size = overlayLocal.size_Overridden ? overlayLocal.size : overlayGlobal.size,

				position_Overridden = overlayLocal.position_Overridden,
				size_Overridden = overlayLocal.size_Overridden,

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

				raceStatusEnabled = overlayLocal.raceStatusEnabled_Overridden ? overlayLocal.raceStatusEnabled : overlayGlobal.raceStatusEnabled,
				raceStatusPosition = overlayLocal.raceStatusPosition_Overridden ? overlayLocal.raceStatusPosition : overlayGlobal.raceStatusPosition,

				raceStatusEnabled_Overridden = overlayLocal.raceStatusEnabled_Overridden,
				raceStatusPosition_Overridden = overlayLocal.raceStatusPosition_Overridden,

				leaderboardEnabled = overlayLocal.leaderboardEnabled_Overridden ? overlayLocal.leaderboardEnabled : overlayGlobal.leaderboardEnabled,
				leaderboardPosition = overlayLocal.leaderboardPosition_Overridden ? overlayLocal.leaderboardPosition : overlayGlobal.leaderboardPosition,
				leaderboardFirstSlotPosition = overlayLocal.leaderboardFirstSlotPosition_Overridden ? overlayLocal.leaderboardFirstSlotPosition : overlayGlobal.leaderboardFirstSlotPosition,
				leaderboardSlotCount = overlayLocal.leaderboardSlotCount_Overridden ? overlayLocal.leaderboardSlotCount : overlayGlobal.leaderboardSlotCount,
				leaderboardSlotSpacing = overlayLocal.leaderboardSlotSpacing_Overridden ? overlayLocal.leaderboardSlotSpacing : overlayGlobal.leaderboardSlotSpacing,
				leaderboardUseClassColors = overlayLocal.leaderboardUseClassColors_Overridden ? overlayLocal.leaderboardUseClassColors : overlayGlobal.leaderboardUseClassColors,
				leaderboardClassColorStrength = overlayLocal.leaderboardClassColorStrength_Overridden ? overlayLocal.leaderboardClassColorStrength : overlayGlobal.leaderboardClassColorStrength,

				leaderboardEnabled_Overridden = overlayLocal.leaderboardEnabled_Overridden,
				leaderboardPosition_Overridden = overlayLocal.leaderboardPosition_Overridden,
				leaderboardFirstSlotPosition_Overridden = overlayLocal.leaderboardFirstSlotPosition_Overridden,
				leaderboardSlotCount_Overridden = overlayLocal.leaderboardSlotCount_Overridden,
				leaderboardSlotSpacing_Overridden = overlayLocal.leaderboardSlotSpacing_Overridden,
				leaderboardUseClassColors_Overridden = overlayLocal.leaderboardUseClassColors_Overridden,
				leaderboardClassColorStrength_Overridden = overlayLocal.leaderboardClassColorStrength_Overridden,

				voiceOfEnabled = overlayLocal.voiceOfEnabled_Overridden ? overlayLocal.voiceOfEnabled : overlayGlobal.voiceOfEnabled,
				voiceOfPosition = overlayLocal.voiceOfPosition_Overridden ? overlayLocal.voiceOfPosition : overlayGlobal.voiceOfPosition,

				voiceOfEnabled_Overridden = overlayLocal.voiceOfEnabled_Overridden,
				voiceOfPosition_Overridden = overlayLocal.voiceOfPosition_Overridden,

				subtitleEnabled = overlayLocal.subtitleEnabled_Overridden ? overlayLocal.subtitleEnabled : overlayGlobal.subtitleEnabled,
				subtitlePosition = overlayLocal.subtitlePosition_Overridden ? overlayLocal.subtitlePosition : overlayGlobal.subtitlePosition,
				subtitleMaxSize = overlayLocal.subtitleMaxSize_Overridden ? overlayLocal.subtitleMaxSize : overlayGlobal.subtitleMaxSize,
				subtitleBackgroundColor = overlayLocal.subtitleBackgroundColor_Overridden ? overlayLocal.subtitleBackgroundColor : overlayGlobal.subtitleBackgroundColor,
				subtitleTextPadding = overlayLocal.subtitleTextPadding_Overridden ? overlayLocal.subtitleTextPadding : overlayGlobal.subtitleTextPadding,

				subtitleEnabled_Overridden = overlayLocal.subtitleEnabled_Overridden,
				subtitlePosition_Overridden = overlayLocal.subtitlePosition_Overridden,
				subtitleMaxSize_Overridden = overlayLocal.subtitleMaxSize_Overridden,
				subtitleBackgroundColor_Overridden = overlayLocal.subtitleBackgroundColor_Overridden,
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
				telemetryNumberOfCheckpoints = overlayLocal.telemetryNumberOfCheckpoints_Overridden ? overlayLocal.telemetryNumberOfCheckpoints : overlayGlobal.telemetryNumberOfCheckpoints,

				telemetryPitColor_Overridden = overlayLocal.telemetryPitColor_Overridden,
				telemetryOutColor_Overridden = overlayLocal.telemetryOutColor_Overridden,
				telemetryIsBetweenCars_Overridden = overlayLocal.telemetryIsBetweenCars_Overridden,
				telemetryMode_Overridden = overlayLocal.telemetryMode_Overridden,
				telemetryNumberOfCheckpoints_Overridden = overlayLocal.telemetryNumberOfCheckpoints_Overridden,

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

				startLightsEnabled = overlayLocal.startLightsEnabled_Overridden ? overlayLocal.startLightsEnabled : overlayGlobal.startLightsEnabled,
				startLightsPosition = overlayLocal.startLightsPosition_Overridden ? overlayLocal.startLightsPosition : overlayGlobal.startLightsPosition,

				startLightsEnabled_Overridden = overlayLocal.startLightsEnabled_Overridden,
				startLightsPosition_Overridden = overlayLocal.startLightsPosition_Overridden,
			};

			foreach ( var item in overlayLocal.imageSettingsDataDictionary )
			{
				var globalItem = overlayGlobal.imageSettingsDataDictionary[ item.Key ];

				overlay.imageSettingsDataDictionary[ item.Key ] = new SettingsImage()
				{
					imageType = item.Value.imageType_Overridden ? item.Value.imageType : globalItem.imageType,
					filePath = item.Value.filePath_Overridden ? item.Value.filePath : globalItem.filePath,
					position = item.Value.position_Overridden ? item.Value.position : globalItem.position,
					size = item.Value.size_Overridden ? item.Value.size : globalItem.size,
					tintColor = item.Value.tintColor_Overridden ? item.Value.tintColor : globalItem.tintColor,
					border = item.Value.border_Overridden ? item.Value.border : globalItem.border,

					imageType_Overridden = item.Value.imageType_Overridden,
					filePath_Overridden = item.Value.filePath_Overridden,
					position_Overridden = item.Value.position_Overridden,
					size_Overridden = item.Value.size_Overridden,
					tintColor_Overridden = item.Value.tintColor_Overridden,
					border_Overridden = item.Value.border_Overridden
				};
			}

			foreach ( var item in overlayLocal.textSettingsDataDictionary )
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

					fontIndex_Overridden = item.Value.fontIndex_Overridden,
					fontSize_Overridden = item.Value.fontSize_Overridden,
					alignment_Overridden = item.Value.alignment_Overridden,
					position_Overridden = item.Value.position_Overridden,
					size_Overridden = item.Value.size_Overridden,
					tintColor_Overridden = item.Value.tintColor_Overridden
				};
			}

			foreach ( var item in overlayLocal.translationDictionary )
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

		public static void UpdateCombinedDirector()
		{
			director = new SettingsDirector()
			{
				camerasPractice = directorLocal.camerasPractice_Overridden ? directorLocal.camerasPractice : directorGlobal.camerasPractice,
				camerasQualifying = directorLocal.camerasQualifying_Overridden ? directorLocal.camerasQualifying : directorGlobal.camerasQualifying,
				camerasIntro = directorLocal.camerasIntro_Overridden ? directorLocal.camerasIntro : directorGlobal.camerasIntro,
				camerasInside = directorLocal.camerasInside_Overridden ? directorLocal.camerasInside : directorGlobal.camerasInside,
				camerasClose = directorLocal.camerasClose_Overridden ? directorLocal.camerasClose : directorGlobal.camerasClose,
				camerasMedium = directorLocal.camerasMedium_Overridden ? directorLocal.camerasMedium : directorGlobal.camerasMedium,
				camerasFar = directorLocal.camerasFar_Overridden ? directorLocal.camerasFar : directorGlobal.camerasFar,
				camerasVeryFar = directorLocal.camerasVeryFar_Overridden ? directorLocal.camerasVeryFar : directorGlobal.camerasVeryFar,

				camerasPractice_Overridden = directorLocal.camerasPractice_Overridden,
				camerasQualifying_Overridden = directorLocal.camerasQualifying_Overridden,
				camerasIntro_Overridden = directorLocal.camerasIntro_Overridden,
				camerasInside_Overridden = directorLocal.camerasInside_Overridden,
				camerasClose_Overridden = directorLocal.camerasClose_Overridden,
				camerasMedium_Overridden = directorLocal.camerasMedium_Overridden,
				camerasFar_Overridden = directorLocal.camerasFar_Overridden,
				camerasVeryFar_Overridden = directorLocal.camerasVeryFar_Overridden,

				switchDelayDirector = directorLocal.switchDelayDirector_Overridden ? directorLocal.switchDelayDirector : directorGlobal.switchDelayDirector,
				switchDelayIracing = directorLocal.switchDelayIracing_Overridden ? directorLocal.switchDelayIracing : directorGlobal.switchDelayIracing,
				switchDelayRadioChatter = directorLocal.switchDelayRadioChatter_Overridden ? directorLocal.switchDelayRadioChatter : directorGlobal.switchDelayRadioChatter,
				switchDelayNotInRace = directorLocal.switchDelayNotInRace_Overridden ? directorLocal.switchDelayNotInRace : directorGlobal.switchDelayNotInRace,

				switchDelayDirector_Overridden = directorLocal.switchDelayDirector_Overridden,
				switchDelayIracing_Overridden = directorLocal.switchDelayIracing_Overridden,
				switchDelayRadioChatter_Overridden = directorLocal.switchDelayRadioChatter_Overridden,
				switchDelayNotInRace_Overridden = directorLocal.switchDelayNotInRace_Overridden,

				heatCarLength = directorLocal.heatCarLength_Overridden ? directorLocal.heatCarLength : directorGlobal.heatCarLength,
				heatFalloff = directorLocal.heatFalloff_Overridden ? directorLocal.heatFalloff : directorGlobal.heatFalloff,
				heatBias = directorLocal.heatBias_Overridden ? directorLocal.heatBias : directorGlobal.heatBias,

				heatCarLength_Overridden = directorLocal.heatCarLength_Overridden,
				heatFalloff_Overridden = directorLocal.heatFalloff_Overridden,
				heatBias_Overridden = directorLocal.heatBias_Overridden,

				preferredCarNumber = directorLocal.preferredCarNumber_Overridden ? directorLocal.preferredCarNumber : directorGlobal.preferredCarNumber,
				preferredCarLockOnEnabled = directorLocal.preferredCarLockOnEnabled_Overridden ? directorLocal.preferredCarLockOnEnabled : directorGlobal.preferredCarLockOnEnabled,
				preferredCarLockOnMinimumHeat = directorLocal.preferredCarLockOnMinimumHeat_Overridden ? directorLocal.preferredCarLockOnMinimumHeat : directorGlobal.preferredCarLockOnMinimumHeat,

				preferredCarNumber_Overridden = directorLocal.preferredCarNumber_Overridden,
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
				rule13_Enabled = directorLocal.rules_Overridden ? directorLocal.rule13_Enabled : directorGlobal.rule13_Enabled,
				rule13_Camera = directorLocal.rules_Overridden ? directorLocal.rule13_Camera : directorGlobal.rule13_Camera,

				rules_Overridden = directorLocal.rules_Overridden,

				autoCamInsideMinimum = directorLocal.autoCam_Overridden ? directorLocal.autoCamInsideMinimum : directorGlobal.autoCamInsideMinimum,
				autoCamInsideMaximum = directorLocal.autoCam_Overridden ? directorLocal.autoCamInsideMaximum : directorGlobal.autoCamInsideMaximum,
				autoCamCloseMaximum = directorLocal.autoCam_Overridden ? directorLocal.autoCamCloseMaximum : directorGlobal.autoCamCloseMaximum,
				autoCamMediumMaximum = directorLocal.autoCam_Overridden ? directorLocal.autoCamMediumMaximum : directorGlobal.autoCamMediumMaximum,
				autoCamFarMaximum = directorLocal.autoCam_Overridden ? directorLocal.autoCamFarMaximum : directorGlobal.autoCamFarMaximum,

				autoCam_Overridden = directorLocal.autoCam_Overridden
			};
		}
	}
}
