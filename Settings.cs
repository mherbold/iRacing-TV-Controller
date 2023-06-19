
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace iRacingTVController
{
	public static class Settings
	{
		public const string SettingsFolderName = "Settings";

		public const string EditorSettingsFileName = "Editor.xml";
		public const string GlobalSettingsFileName = "Global.xml";

		public static string editorSettingsFolder = Program.documentsFolder;
		public static string overlaySettingsFolder = Program.documentsFolder + SettingsFolderName + "\\";

		public static string editorSettingsFilePath = editorSettingsFolder + EditorSettingsFileName;
		public static string globalSettingsFilePath = overlaySettingsFolder + GlobalSettingsFileName;

		public static SettingsEditor editor = new();
		public static SettingsOverlay global = new();
		public static SettingsOverlay overlay = new();

		public static List<SettingsOverlay> overlayList = new();

		public static int loading = 0;

		public static void Initialize()
		{
			UpdateOverlay( global );
			UpdateOverlay( overlay );

			if ( !Directory.Exists( Program.documentsFolder ) )
			{
				Directory.CreateDirectory( Program.documentsFolder );
			}

			if ( !Directory.Exists( overlaySettingsFolder ) )
			{
				Directory.CreateDirectory( overlaySettingsFolder );
			}

			if ( File.Exists( editorSettingsFilePath ) )
			{
				try
				{
					editor = (SettingsEditor) Load( editorSettingsFilePath, typeof( SettingsEditor ) );
				}
				catch ( Exception exception )
				{
					MessageBox.Show( $"We could not load the editor settings file '{editorSettingsFilePath}'.\r\n\r\nThe error message is as follows:\r\n\r\n{exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
				}
			}
			else
			{
				Save( editorSettingsFilePath, editor );
			}

			if ( !File.Exists( globalSettingsFilePath ) )
			{
				global.filePath = globalSettingsFilePath;

				Save( globalSettingsFilePath, global );
			}

			var overlaySettingsFilePaths = Directory.EnumerateFiles( overlaySettingsFolder );

			foreach ( var overlaySettingsFilePath in overlaySettingsFilePaths )
			{
				try
				{
					var settings = (SettingsOverlay) Load( overlaySettingsFilePath, typeof( SettingsOverlay ) );

					settings.filePath = overlaySettingsFilePath;

					UpdateOverlay( settings );

					if ( overlaySettingsFilePath == globalSettingsFilePath )
					{
						global = settings;
					}
					else
					{
						overlayList.Add( settings );

						if ( settings.filePath == editor.lastActiveOverlayFilePath )
						{
							overlay = settings;
						}
					}
				}
				catch ( Exception exception )
				{
					MessageBox.Show( $"We could not load the overlay settings file '{overlaySettingsFilePath}'.\r\n\r\nThe error message is as follows:\r\n\r\n{exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
				}
			}

			if ( overlay.filePath == string.Empty )
			{
				if ( overlayList.Count > 0 )
				{
					overlay = overlayList[ 0 ];
				}
				else
				{
					overlay.filePath = overlaySettingsFolder + "My new overlay.xml";

					overlayList.Add( overlay );

					SaveOverlay();
				}
			}

			IPC.readyToSendSettings = true;
		}

		public static void UpdateOverlay( SettingsOverlay settings )
		{
			var defaultImageSettings = new Dictionary<string, SettingsImage>() {
				{ "BlackLight", new SettingsImage(){ imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Images\\light-black.png", position = { x = 280, y = 130 } } },
				{ "Custom1", new SettingsImage() },
				{ "Custom2", new SettingsImage() },
				{ "Custom3", new SettingsImage() },
				{ "Custom4", new SettingsImage() },
				{ "CarNumber", new SettingsImage() { imageType = SettingsImage.ImageType.CarNumber, position = { x = 48, y = 10 }, size = { x = 56, y = 28 } } },
				{ "GreenLight", new SettingsImage(){ imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Images\\light-green.png", position = { x = 280, y = 130 } } },
				{ "Highlight", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Images\\current-target.png", tintColor = { a = 0.9f } } },
				{ "LeaderboardBackground", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Images\\leaderboard.png", tintColor = { a = 0.9f } } },
				{ "PositionSplitter", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Images\\position-splitter.png", position = { x = 0, y = 412 }, tintColor = { a = 0.9f } } },
				{ "RaceStatusBackground", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Images\\race-status.png", tintColor = { a = 0.9f } } },
				{ "SeriesLogo", new SettingsImage() { imageType = SettingsImage.ImageType.SeriesLogo, position = { x = 7, y = 7 }, size = { x = 305, y = 103 } } },
				{ "VoiceOfBackground", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Images\\voice-of.png", tintColor = { a = 0.9f } } },
				{ "VoiceOfCar", new SettingsImage() { imageType = SettingsImage.ImageType.Car, position = { x = 155, y = -37 }, size = { x = 320, y = 160 } } },
				{ "WhiteLight", new SettingsImage() { imageType = SettingsImage.ImageType.ImageFile, filePath = Program.documentsFolder + "Images\\light-white.png", position = { x = 280, y = 130 } } },
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
				{ "CurrentLap", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 27, alignment = Unity.TextAlignmentOptions.TopRight, position = { x = 298, y = 175 }, tintColor = { r = 0.737f, g = 0.741f, b = 0.725f } } },
				{ "DriverName", new SettingsText() { fontIndex = SettingsText.FontIndex.FontB, fontSize = 21, position = { x = 108, y = 12 }, tintColor = { r = 0.69f, g = 0.71f, b = 0.694f } } },
				{ "LapsRemaining", new SettingsText() { fontIndex = SettingsText.FontIndex.FontB, fontSize = 27, alignment = Unity.TextAlignmentOptions.TopRight, position = { x = 269, y = 125 }, tintColor = { r = 0.961f, g = 0.961f, b = 0.953f } } },
				{ "Place", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 21, alignment = Unity.TextAlignmentOptions.TopRight, position = { x = 43, y = 12 }, tintColor = { r = 0.69f, g = 0.71f, b = 0.694f } } },
				{ "SessionName", new SettingsText() { fontIndex = SettingsText.FontIndex.FontB, fontSize = 27, position = { x = 18, y = 125 }, tintColor = { r = 0.961f, g = 0.961f, b = 0.953f } } },
				{ "Speed", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 21, alignment = Unity.TextAlignmentOptions.TopRight, position = { x = 397, y = 12 }, tintColor = { r = 0.69f, g = 0.71f, b = 0.694f } } },
				{ "Subtitles", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 39, alignment = Unity.TextAlignmentOptions.Center, tintColor = { r = 0.961f, g = 0.961f, b = 0.953f } } },
				{ "Telemetry", new SettingsText() { fontIndex = SettingsText.FontIndex.FontA, fontSize = 21, alignment = Unity.TextAlignmentOptions.TopRight, position = { x = 298, y = 12 }, tintColor = { r = 0.69f, g = 0.71f, b = 0.694f } } },
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
			Save( global.filePath, global );
			Save( overlay.filePath, overlay );
		}

		public static void Save( string filePath, object settingsData )
		{
			var xmlSerializer = new XmlSerializer( settingsData.GetType() );

			var streamWriter = new StreamWriter( filePath );

			xmlSerializer.Serialize( streamWriter, settingsData );

			streamWriter.Close();
		}

		public static SettingsOverlay GetCombinedOverlay()
		{
			var combined = new SettingsOverlay
			{
				overlayPosition = overlay.overlayPosition_Overridden ? overlay.overlayPosition : global.overlayPosition,
				overlaySize = overlay.overlaySize_Overridden ? overlay.overlaySize : global.overlaySize,

				overlayPosition_Overridden = overlay.overlayPosition_Overridden,
				overlaySize_Overridden = overlay.overlaySize_Overridden,

				fontPaths = new string[ SettingsOverlay.MaxNumFonts ] {
					overlay.fontNames_Overridden[ 0 ] ? overlay.fontPaths[ 0 ] : global.fontPaths[ 0 ],
					overlay.fontNames_Overridden[ 1 ] ? overlay.fontPaths[ 1 ] : global.fontPaths[ 1 ],
					overlay.fontNames_Overridden[ 2 ] ? overlay.fontPaths[ 2 ] : global.fontPaths[ 2 ],
					overlay.fontNames_Overridden[ 3 ] ? overlay.fontPaths[ 3 ] : global.fontPaths[ 3 ]
				},

				fontNames_Overridden = new bool[ SettingsOverlay.MaxNumFonts ]
				{
					overlay.fontNames_Overridden[ 0 ],
					overlay.fontNames_Overridden[ 1 ],
					overlay.fontNames_Overridden[ 2 ],
					overlay.fontNames_Overridden[ 3 ]
				},

				leaderboardOverlayEnabled = overlay.leaderboardOverlayEnabled_Overridden ? overlay.leaderboardOverlayEnabled : global.leaderboardOverlayEnabled,
				leaderboardOverlayPosition = overlay.leaderboardOverlayPosition_Overridden ? overlay.leaderboardOverlayPosition : global.leaderboardOverlayPosition,
				leaderboardFirstPlacePosition = overlay.leaderboardFirstPlacePosition_Overridden ? overlay.leaderboardFirstPlacePosition : global.leaderboardFirstPlacePosition,
				leaderboardPlaceCount = overlay.leaderboardPlaceCount_Overridden ? overlay.leaderboardPlaceCount : global.leaderboardPlaceCount,
				leaderboardPlaceSpacing = overlay.leaderboardPlaceSpacing_Overridden ? overlay.leaderboardPlaceSpacing : global.leaderboardPlaceSpacing,

				leaderboardOverlayEnabled_Overridden = overlay.leaderboardOverlayEnabled_Overridden,
				leaderboardOverlayPosition_Overridden = overlay.leaderboardOverlayPosition_Overridden,
				leaderboardFirstPlacePosition_Overridden = overlay.leaderboardFirstPlacePosition_Overridden,
				leaderboardPlaceCount_Overridden = overlay.leaderboardPlaceCount_Overridden,
				leaderboardPlaceSpacing_Overridden = overlay.leaderboardPlaceSpacing_Overridden
			};

			foreach ( var item in overlay.imageSettingsDataDictionary )
			{
				var globalItem = global.imageSettingsDataDictionary[ item.Key ];

				combined.imageSettingsDataDictionary[ item.Key ] = new SettingsImage()
				{
					imageType = item.Value.imageType_Overridden ? item.Value.imageType : globalItem.imageType,
					filePath = item.Value.filePath_Overridden ? item.Value.filePath : globalItem.filePath,
					position = item.Value.position_Overridden ? item.Value.position : globalItem.position,
					size = item.Value.size_Overridden ? item.Value.size : globalItem.size,
					tintColor = item.Value.tintColor_Overridden ? item.Value.tintColor : globalItem.tintColor,

					imageType_Overridden = item.Value.imageType_Overridden,
					filePath_Overridden = item.Value.filePath_Overridden,
					position_Overridden = item.Value.position_Overridden,
					size_Overridden = item.Value.size_Overridden,
					tintColor_Overridden = item.Value.tintColor_Overridden
				};
			}

			foreach ( var item in overlay.textSettingsDataDictionary )
			{
				var globalItem = global.textSettingsDataDictionary[ item.Key ];

				combined.textSettingsDataDictionary[ item.Key ] = new SettingsText()
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

			foreach ( var item in overlay.translationDictionary )
			{
				var globalItem = global.translationDictionary[ item.Key ];

				combined.translationDictionary[ item.Key ] = new SettingsTranslation()
				{
					id = item.Value.id,

					translation = item.Value.translation_Overridden ? item.Value.translation : globalItem.translation,

					translation_Overridden = item.Value.translation_Overridden
				};
			}

			return combined;
		}
	}
}
