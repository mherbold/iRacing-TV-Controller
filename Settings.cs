
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

			IPC.readyToSend = true;
		}

		public static void UpdateOverlay( SettingsOverlay settings )
		{
			var imageIdList = new string[] {
				"BlackLight",
				"Custom1",
				"Custom2",
				"Custom3",
				"Custom4",
				"CarNumber",
				"GreenLight",
				"Highlight",
				"LeaderboardBackground",
				"PositionSplitter",
				"RaceStatusBackground",
				"SeriesLogo",
				"VoiceOfBackground",
				"VoiceOfCar",
				"WhiteLight",
				"YellowLight",
			};

			foreach ( var id in imageIdList )
			{
				if ( !settings.imageSettingsDataDictionary.ContainsKey( id ) )
				{
					settings.imageSettingsDataDictionary[ id ] = new SettingsImage();
				}
			}

			var textIdList = new string[]
			{
				"CurrentLap",
				"DriverName",
				"LapsRemaining",
				"Place",
				"SessionName",
				"Speed",
				"Subtitles",
				"Telemetry",
				"Units",
				"VoiceOf",
				"VoiceOfDriverName"
			};

			foreach ( var id in textIdList )
			{
				if ( !settings.textSettingsDataDictionary.ContainsKey( id ) )
				{
					settings.textSettingsDataDictionary[ id ] = new SettingsText();
				}
			}
		}

		public static object Load( string filePath, Type type )
		{
			var xmlSerializer = new XmlSerializer( type );

			var fileStream = new FileStream( filePath, FileMode.Open );

			var settingsData = xmlSerializer.Deserialize( fileStream ) ?? throw new Exception();

			fileStream.Close();

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

				fontNames = new string[ SettingsOverlay.MaxNumFonts ] {
					overlay.fontNames_Overridden[ 0 ] ? overlay.fontNames[ 0 ] : global.fontNames[ 0 ],
					overlay.fontNames_Overridden[ 1 ] ? overlay.fontNames[ 1 ] : global.fontNames[ 1 ],
					overlay.fontNames_Overridden[ 2 ] ? overlay.fontNames[ 2 ] : global.fontNames[ 2 ],
					overlay.fontNames_Overridden[ 3 ] ? overlay.fontNames[ 3 ] : global.fontNames[ 3 ]
				},

				fontNames_Overridden = new bool[ SettingsOverlay.MaxNumFonts ]
				{
					overlay.fontNames_Overridden[ 0 ],
					overlay.fontNames_Overridden[ 1 ],
					overlay.fontNames_Overridden[ 2 ],
					overlay.fontNames_Overridden[ 3 ]
				}
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

			return combined;
		}
	}
}
