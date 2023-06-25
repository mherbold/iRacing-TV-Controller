
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

					SaveOverlay();
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

					SaveDirector();
				}
			}

			IPC.readyToSendSettings = true;
		}

		public static void AddMissingDictionaryItems( SettingsOverlay settings )
		{
			var defaultImageSettings = new Dictionary<string, SettingsImage>() {
				{ "BlackLight", new SettingsImage(){ imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Images\\light-black.png", position = { x = 280, y = 130 } } },
				{ "CarNumber", new SettingsImage() { imageType = SettingsImage.ImageType.CarNumber, position = { x = 48, y = 10 }, size = { x = 56, y = 28 } } },
				{ "CheckeredFlag", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Images\\flag-checkered.png" } },
				{ "Custom1", new SettingsImage() },
				{ "Custom2", new SettingsImage() },
				{ "Custom3", new SettingsImage() },
				{ "Custom4", new SettingsImage() },
				{ "GreenFlag", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Images\\flag-green.png" } },
				{ "GreenLight", new SettingsImage(){ imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Images\\light-green.png", position = { x = 280, y = 130 } } },
				{ "Highlight", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Images\\current-target.png" } },
				{ "LeaderboardBackground", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Images\\leaderboard.png", size = { x = 319, y = 8 }, border = { x = 32, y = 32, z = 32, w = 32 } } },
				{ "PositionSplitter", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Images\\position-splitter.png" } },
				{ "RaceStatusBackground", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Images\\race-status.png" } },
				{ "SeriesLogo", new SettingsImage() { imageType = SettingsImage.ImageType.SeriesLogo, position = { x = 7, y = 7 }, size = { x = 305, y = 103 } } },
				{ "VoiceOfBackground", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Images\\voice-of.png" } },
				{ "VoiceOfCar", new SettingsImage() { imageType = SettingsImage.ImageType.Car, position = { x = 155, y = -37 }, size = { x = 320, y = 160 } } },
				{ "WhiteLight", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Images\\light-white.png", position = { x = 280, y = 130 } } },
				{ "YellowFlag", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Images\\flag-yellow.png" } },
				{ "YellowLight", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Images\\light-yellow.png", position = { x = 280, y = 130 } } },
			};

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
				{ "LapsRemaining", new SettingsText() { fontIndex = SettingsText.FontIndex.FontB, fontSize = 27, alignment = TextAlignmentOptions.TopRight, position = { x = 269, y = 125 }, tintColor = { r = 0.961f, g = 0.961f, b = 0.953f } } },
				{ "Place", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 21, alignment = TextAlignmentOptions.TopRight, position = { x = 43, y = 12 }, tintColor = { r = 0.69f, g = 0.71f, b = 0.694f } } },
				{ "SessionName", new SettingsText() { fontIndex = SettingsText.FontIndex.FontB, fontSize = 27, position = { x = 18, y = 125 }, tintColor = { r = 0.961f, g = 0.961f, b = 0.953f } } },
				{ "Speed", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 21, alignment = TextAlignmentOptions.TopRight, position = { x = 397, y = 12 }, tintColor = { r = 0.69f, g = 0.71f, b = 0.694f } } },
				{ "Subtitles", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 39, alignment = TextAlignmentOptions.Center, tintColor = { r = 0.961f, g = 0.961f, b = 0.953f } } },
				{ "Telemetry", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 21, alignment = TextAlignmentOptions.TopRight, position = { x = 298, y = 12 }, tintColor = { r = 0.69f, g = 0.71f, b = 0.694f } } },
				{ "Units", new SettingsText() { fontIndex = SettingsText.FontIndex.FontB, fontSize = 27, position = { x = 18, y = 175 }, tintColor = { r = 0.737f, g = 0.741f, b = 0.725f } } },
				{ "VoiceOf", new SettingsText() { fontIndex = SettingsText.FontIndex.FontB, fontSize = 30, position = { x = 30, y = 10 }, tintColor = { r = 0.737f, g = 0.741f, b = 0.725f } } },
				{ "VoiceOfDriverName", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 30, position = { x = 30, y = 41 }, tintColor = { r = 0.137f, g = 0.122f, b = 0.125f } } },
			};

			foreach ( var item in defaultTextSettings )
			{
				if ( !settings.textSettingsDataDictionary.ContainsKey( item.Key ) )
				{
					settings.textSettingsDataDictionary[ item.Key ] = item.Value;
				}
			}

			var defaultTranslationSettings = new Dictionary<string, SettingsTranslation>()
			{
				{ "Pit", new SettingsTranslation() { translation = "PIT" } },
				{ "Out", new SettingsTranslation() { translation = "OUT" } },
				{ "LapsAbbreviation", new SettingsTranslation() { translation = "L" } },
				{ "MetersAbbreviation", new SettingsTranslation() { translation = "M" } },
				{ "FeetAbbreviation", new SettingsTranslation() { translation = "FT" } },
				{ "KPH", new SettingsTranslation() { translation = "KPH" } },
				{ "MPH", new SettingsTranslation() { translation = "MPH" } },
				{ "FinalLap", new SettingsTranslation() { translation = "FINAL LAP" } },
				{ "ToGo", new SettingsTranslation() { translation = "TO GO" } },
				{ "Time", new SettingsTranslation() { translation = "TIME" } },
				{ "Lap", new SettingsTranslation() { translation = "LAP" } },
				{ "VoiceOf", new SettingsTranslation() { translation = "VOICE OF" } }
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
			Save( editorSettingsFilePath, editor );
		}

		public static void SaveOverlay()
		{
			Save( overlayGlobal.filePath, overlayGlobal );
			Save( overlayLocal.filePath, overlayLocal );

			UpdateCombinedOverlay();
		}

		public static void SaveDirector()
		{
			Save( directorGlobal.filePath, directorGlobal );
			Save( directorLocal.filePath, directorLocal );

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
				leaderboardFirstPlacePosition = overlayLocal.leaderboardFirstPlacePosition_Overridden ? overlayLocal.leaderboardFirstPlacePosition : overlayGlobal.leaderboardFirstPlacePosition,
				leaderboardPlaceCount = overlayLocal.leaderboardPlaceCount_Overridden ? overlayLocal.leaderboardPlaceCount : overlayGlobal.leaderboardPlaceCount,
				leaderboardPlaceSpacing = overlayLocal.leaderboardPlaceSpacing_Overridden ? overlayLocal.leaderboardPlaceSpacing : overlayGlobal.leaderboardPlaceSpacing,
				leaderboardUseClassColors = overlayLocal.leaderboardUseClassColors_Overridden ? overlayLocal.leaderboardUseClassColors : overlayGlobal.leaderboardUseClassColors,
				leaderboardClassColorStrength = overlayLocal.leaderboardClassColorStrength_Overridden ? overlayLocal.leaderboardClassColorStrength : overlayGlobal.leaderboardClassColorStrength,
				telemetryPitColor = overlayLocal.telemetryPitColor_Overridden ? overlayLocal.telemetryPitColor : overlayGlobal.telemetryPitColor,
				telemetryOutColor = overlayLocal.telemetryOutColor_Overridden ? overlayLocal.telemetryOutColor : overlayGlobal.telemetryOutColor,
				telemetryIsBetweenCars = overlayLocal.telemetryIsBetweenCars_Overridden ? overlayLocal.telemetryIsBetweenCars : overlayGlobal.telemetryIsBetweenCars,
				telemetryMode = overlayLocal.telemetryMode_Overridden ? overlayLocal.telemetryMode : overlayGlobal.telemetryMode,
				telemetryNumberOfCheckpoints = overlayLocal.telemetryNumberOfCheckpoints_Overridden ? overlayLocal.telemetryNumberOfCheckpoints : overlayGlobal.telemetryNumberOfCheckpoints,

				leaderboardEnabled_Overridden = overlayLocal.leaderboardEnabled_Overridden,
				leaderboardPosition_Overridden = overlayLocal.leaderboardPosition_Overridden,
				leaderboardFirstPlacePosition_Overridden = overlayLocal.leaderboardFirstPlacePosition_Overridden,
				leaderboardPlaceCount_Overridden = overlayLocal.leaderboardPlaceCount_Overridden,
				leaderboardPlaceSpacing_Overridden = overlayLocal.leaderboardPlaceSpacing_Overridden,
				leaderboardUseClassColors_Overridden = overlayLocal.leaderboardUseClassColors_Overridden,
				leaderboardClassColorStrength_Overridden = overlayLocal.leaderboardClassColorStrength_Overridden,
				telemetryPitColor_Overridden = overlayLocal.telemetryPitColor_Overridden,
				telemetryOutColor_Overridden = overlayLocal.telemetryOutColor_Overridden,
				telemetryIsBetweenCars_Overridden = overlayLocal.telemetryIsBetweenCars_Overridden,
				telemetryMode_Overridden = overlayLocal.telemetryMode_Overridden,
				telemetryNumberOfCheckpoints_Overridden = overlayLocal.telemetryNumberOfCheckpoints_Overridden,

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

				directorCarLength = overlayLocal.directorCarLength_Overridden ? overlayLocal.directorCarLength : overlayGlobal.directorCarLength,
				directorHeatFalloff = overlayLocal.directorHeatFalloff_Overridden ? overlayLocal.directorHeatFalloff : overlayGlobal.directorHeatFalloff,
				directorHeatBias = overlayLocal.directorHeatBias_Overridden ? overlayLocal.directorHeatBias : overlayGlobal.directorHeatBias,

				directorCarLength_Overridden = overlayLocal.directorCarLength_Overridden,
				directorHeatFalloff_Overridden = overlayLocal.directorHeatFalloff_Overridden,
				directorHeatBias_Overridden = overlayLocal.directorHeatBias_Overridden,
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
			};
		}
	}
}
