
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

using Dsafa.WpfColorPicker;

using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	public partial class MainWindow : Window
	{
		public static MainWindow Instance { get; private set; }

		public int initializing = 0;

		public SortedDictionary<string, string> fontPaths;

		public SortedDictionary<string, int> patternOptions = new();
		public SortedDictionary<string, int> slantOptions = new();

		static MainWindow()
		{
			Instance = new MainWindow();
		}

		private MainWindow()
		{
			Instance = this;

			initializing++;

			try
			{
				InitializeComponent();

				Program.Initialize();
			}
			catch ( Exception exception )
			{
				LogFile.WriteException( exception );

				throw;
			}

			foreach ( var item in Settings.overlay.imageSettingsDataDictionary )
			{
				Image_ID.Items.Add( item.Key );
			}

			foreach ( var imageType in Enum.GetValues( typeof( SettingsImage.ImageType ) ) )
			{
				Image_ImageType.Items.Add( imageType );
			}

			fontPaths = FontPaths.FindAll();

			foreach ( var installedFont in fontPaths )
			{
				Font_FontA_Name.Items.Add( installedFont.Key );
				Font_FontB_Name.Items.Add( installedFont.Key );
				Font_FontC_Name.Items.Add( installedFont.Key );
				Font_FontD_Name.Items.Add( installedFont.Key );
			}

			foreach ( var item in Settings.overlay.textSettingsDataDictionary )
			{
				Text_ID.Items.Add( item.Key );
			}

			foreach ( var fontIndex in Enum.GetValues( typeof( SettingsText.FontIndex ) ) )
			{
				Text_FontIndex.Items.Add( fontIndex );
			}

			foreach ( var textAlignmentOption in Enum.GetValues( typeof( TextAlignmentOptions ) ) )
			{
				Text_Alignment.Items.Add( textAlignmentOption );
			}

			patternOptions.Add( "914", 37 );
			patternOptions.Add( "Aardvark", 11 );
			patternOptions.Add( "Aero", 1 );
			patternOptions.Add( "Air Millhouse", 42 );
			patternOptions.Add( "Antique Oil", 46 );
			patternOptions.Add( "Arial", 0 );
			patternOptions.Add( "Batavia", 39 );
			patternOptions.Add( "Bauer", 24 );
			patternOptions.Add( "Bauhaus Alt", 41 );
			patternOptions.Add( "Bavuese", 18 );
			patternOptions.Add( "Bell Gothic", 2 );
			patternOptions.Add( "Berlin Sans", 20 );
			patternOptions.Add( "Bernard", 25 );
			patternOptions.Add( "Biondi", 29 );
			patternOptions.Add( "Blackoak", 13 );
			patternOptions.Add( "Bodoni Poster", 45 );
			patternOptions.Add( "Bolt", 3 );
			patternOptions.Add( "Brittanic Bold", 50 );
			patternOptions.Add( "Brushwhacker", 10 );
			patternOptions.Add( "Carnegie Way", 40 );
			patternOptions.Add( "CBrown", 55 );
			patternOptions.Add( "Convecta Alt", 38 );
			patternOptions.Add( "Convecta Alt-2", 54 );
			patternOptions.Add( "Convecta", 19 );
			patternOptions.Add( "Cricket", 22 );
			patternOptions.Add( "Demonized", 33 );
			patternOptions.Add( "Eras Bold", 51 );
			patternOptions.Add( "Foolio", 4 );
			patternOptions.Add( "Harabara", 56 );
			patternOptions.Add( "Harlow Solid", 49 );
			patternOptions.Add( "Hattenschweiler", 26 );
			patternOptions.Add( "He is Dead Jim", 48 );
			patternOptions.Add( "Humanist", 5 );
			patternOptions.Add( "Idea", 12 );
			patternOptions.Add( "Impact", 35 );
			patternOptions.Add( "Incised 901", 44 );
			patternOptions.Add( "Infinite Justice", 14 );
			patternOptions.Add( "Kimberly", 32 );
			patternOptions.Add( "Lithograph", 6 );
			patternOptions.Add( "Matt Rolfe", 53 );
			patternOptions.Add( "Microgamma", 7 );
			patternOptions.Add( "Minion", 47 );
			patternOptions.Add( "Motherlode", 21 );
			patternOptions.Add( "Okayd", 8 );
			patternOptions.Add( "Omni Custom", 31 );
			patternOptions.Add( "Omni", 30 );
			patternOptions.Add( "Pepsi", 36 );
			patternOptions.Add( "Piazza Black", 52 );
			patternOptions.Add( "Pilsen", 57 );
			patternOptions.Add( "Rhino", 23 );
			patternOptions.Add( "Sofachrome", 15 );
			patternOptions.Add( "SportsNight Alt", 28 );
			patternOptions.Add( "SportsNight", 27 );
			patternOptions.Add( "Swiss 721 Bold", 43 );
			patternOptions.Add( "Walldoc", 17 );
			patternOptions.Add( "Wide Latin", 34 );
			patternOptions.Add( "Winsdor", 9 );
			patternOptions.Add( "Yikes", 16 );

			foreach ( var item in patternOptions )
			{
				CarNumber_Pattern.Items.Add( item.Key );
			}

			slantOptions.Add( "Normal Slant", 0 );
			slantOptions.Add( "Left Slant", 1 );
			slantOptions.Add( "Right Slant", 2 );
			slantOptions.Add( "Forward Slant", 3 );
			slantOptions.Add( "Backward Slant", 4 );

			foreach ( var item in slantOptions )
			{
				CarNumber_Slant.Items.Add( item.Key );
			}

			initializing--;

			Initialize();
		}

		public void Initialize()
		{
			initializing++;

			OverlayList.Items.Clear();

			foreach ( var overlay in Settings.overlayList )
			{
				OverlayList.Items.Add( overlay );

				if ( Settings.overlay == overlay )
				{
					OverlayList.SelectedItem = overlay;
				}
			}

			Settings.UpdateCombinedOverlay();

			General_Position_X.Value = Settings.combined.overlayPosition.x;
			General_Position_Y.Value = Settings.combined.overlayPosition.y;

			General_Size_W.Value = Settings.combined.overlaySize.x;
			General_Size_H.Value = Settings.combined.overlaySize.y;

			General_Position_Override.IsChecked = Settings.combined.overlayPosition_Overridden;
			General_Size_Override.IsChecked = Settings.combined.overlaySize_Overridden;

			Font_FontA_Name.SelectedItem = fontPaths.FirstOrDefault( x => x.Value == Settings.combined.fontPaths[ 0 ] ).Key;
			Font_FontB_Name.SelectedItem = fontPaths.FirstOrDefault( x => x.Value == Settings.combined.fontPaths[ 1 ] ).Key;
			Font_FontC_Name.SelectedItem = fontPaths.FirstOrDefault( x => x.Value == Settings.combined.fontPaths[ 2 ] ).Key;
			Font_FontD_Name.SelectedItem = fontPaths.FirstOrDefault( x => x.Value == Settings.combined.fontPaths[ 3 ] ).Key;

			Font_FontA_Name_Override.IsChecked = Settings.combined.fontNames_Overridden[ 0 ];
			Font_FontB_Name_Override.IsChecked = Settings.combined.fontNames_Overridden[ 1 ];
			Font_FontC_Name_Override.IsChecked = Settings.combined.fontNames_Overridden[ 2 ];
			Font_FontD_Name_Override.IsChecked = Settings.combined.fontNames_Overridden[ 3 ];

			if ( Image_ID.SelectedIndex == -1 )
			{
				Image_ID.SelectedIndex = 0;
			}

			InitializeImage();

			if ( Text_ID.SelectedIndex == -1 )
			{
				Text_ID.SelectedIndex = 0;
			}

			InitializeText();

			InitializeTranslation();

			RaceStatus_Overlay_Enable.IsChecked = Settings.combined.raceStatusOverlayEnabled;
			RaceStatus_OverlayPosition_X.Value = (int) Settings.combined.raceStatusOverlayPosition.x;
			RaceStatus_OverlayPosition_Y.Value = (int) Settings.combined.raceStatusOverlayPosition.y;

			RaceStatus_OverlayEnable_Override.IsChecked = Settings.combined.raceStatusOverlayEnabled_Overridden;
			RaceStatus_OverlayPosition_Override.IsChecked = Settings.combined.raceStatusOverlayPosition_Overridden;

			Leaderboard_Overlay_Enable.IsChecked = Settings.combined.leaderboardOverlayEnabled;
			Leaderboard_OverlayPosition_X.Value = (int) Settings.combined.leaderboardOverlayPosition.x;
			Leaderboard_OverlayPosition_Y.Value = (int) Settings.combined.leaderboardOverlayPosition.y;
			Leaderboard_FirstPlacePosition_X.Value = (int) Settings.combined.leaderboardFirstPlacePosition.x;
			Leaderboard_FirstPlacePosition_Y.Value = (int) Settings.combined.leaderboardFirstPlacePosition.y;
			Leaderboard_PlaceCount.Value = Settings.combined.leaderboardPlaceCount;
			Leaderboard_PlaceSpacing_X.Value = (int) Settings.combined.leaderboardPlaceSpacing.x;
			Leaderboard_PlaceSpacing_Y.Value = (int) Settings.combined.leaderboardPlaceSpacing.y;
			Leaderboard_UseClassColors_Enable.IsChecked = Settings.combined.leaderboardUseClassColors;
			Leaderboard_ClassColorStrength.Value = Settings.combined.leaderboardClassColorStrength * 255.0f;

			Leaderboard_OverlayEnable_Override.IsChecked = Settings.combined.leaderboardOverlayEnabled_Overridden;
			Leaderboard_OverlayPosition_Override.IsChecked = Settings.combined.leaderboardOverlayPosition_Overridden;
			Leaderboard_FirstPlacePosition_Override.IsChecked = Settings.combined.leaderboardFirstPlacePosition_Overridden;
			Leaderboard_PlaceCount_Override.IsChecked = Settings.combined.leaderboardPlaceCount_Overridden;
			Leaderboard_PlaceSpacing_Override.IsChecked = Settings.combined.leaderboardPlaceSpacing_Overridden;
			Leaderboard_UseClassColors_Override.IsChecked = Settings.combined.leaderboardUseClassColors_Overridden;
			Leaderboard_ClassColorStrength_Override.IsChecked = Settings.combined.leaderboardClassColorStrength_Overridden;

			VoiceOf_Overlay_Enable.IsChecked = Settings.combined.voiceOfOverlayEnabled;
			VoiceOf_OverlayPosition_X.Value = (int) Settings.combined.voiceOfOverlayPosition.x;
			VoiceOf_OverlayPosition_Y.Value = (int) Settings.combined.voiceOfOverlayPosition.y;

			VoiceOf_OverlayEnable_Override.IsChecked = Settings.combined.voiceOfOverlayEnabled_Overridden;
			VoiceOf_OverlayPosition_Override.IsChecked = Settings.combined.voiceOfOverlayPosition_Overridden;

			Subtitle_Overlay_Enable.IsChecked = Settings.combined.subtitleOverlayEnabled;
			Subtitle_OverlayPosition_X.Value = (int) Settings.combined.subtitleOverlayPosition.x;
			Subtitle_OverlayPosition_Y.Value = (int) Settings.combined.subtitleOverlayPosition.y;
			Subtitle_OverlayMaxSize_W.Value = (int) Settings.combined.subtitleOverlayMaxSize.x;
			Subtitle_OverlayMaxSize_H.Value = (int) Settings.combined.subtitleOverlayMaxSize.y;
			Subtitle_OverlayBackgroundColor_R.Value = Settings.combined.subtitleOverlayBackgroundColor.r;
			Subtitle_OverlayBackgroundColor_G.Value = Settings.combined.subtitleOverlayBackgroundColor.g;
			Subtitle_OverlayBackgroundColor_B.Value = Settings.combined.subtitleOverlayBackgroundColor.b;
			Subtitle_OverlayBackgroundColor_A.Value = Settings.combined.subtitleOverlayBackgroundColor.a;
			Subtitle_TextPadding_X.Value = Settings.combined.subtitleTextPadding.x;
			Subtitle_TextPadding_Y.Value = Settings.combined.subtitleTextPadding.y;

			Subtitle_OverlayEnable_Override.IsChecked = Settings.combined.subtitleOverlayEnabled_Overridden;
			Subtitle_OverlayPosition_Override.IsChecked = Settings.combined.subtitleOverlayPosition_Overridden;
			Subtitle_OverlayMaxSize_Override.IsChecked = Settings.combined.subtitleOverlayMaxSize_Overridden;
			Subtitle_OverlayBackgroundColor_Override.IsChecked = Settings.combined.subtitleOverlayBackgroundColor_Overridden;
			Subtitle_TextPadding_Override.IsChecked = Settings.combined.subtitleTextPadding_Overridden;

			CarNumber_Override_Enable.IsChecked = Settings.combined.carNumberOverrideEnabled;
			CarNumber_ColorA_R.Value = Settings.combined.carNumberColorA.r;
			CarNumber_ColorA_G.Value = Settings.combined.carNumberColorA.g;
			CarNumber_ColorA_B.Value = Settings.combined.carNumberColorA.b;
			CarNumber_ColorA_A.Value = Settings.combined.carNumberColorA.a;
			CarNumber_ColorB_R.Value = Settings.combined.carNumberColorB.r;
			CarNumber_ColorB_G.Value = Settings.combined.carNumberColorB.g;
			CarNumber_ColorB_B.Value = Settings.combined.carNumberColorB.b;
			CarNumber_ColorB_A.Value = Settings.combined.carNumberColorB.a;
			CarNumber_ColorC_R.Value = Settings.combined.carNumberColorC.r;
			CarNumber_ColorC_G.Value = Settings.combined.carNumberColorC.g;
			CarNumber_ColorC_B.Value = Settings.combined.carNumberColorC.b;
			CarNumber_ColorC_A.Value = Settings.combined.carNumberColorC.a;
			CarNumber_Pattern.SelectedItem = patternOptions.FirstOrDefault( x => x.Value == Settings.combined.carNumberPattern ).Key;
			CarNumber_Slant.SelectedItem = slantOptions.FirstOrDefault( x => x.Value == Settings.combined.carNumberSlant ).Key;

			CarNumber_Override_Enable_Override.IsChecked = Settings.combined.carNumberOverrideEnabled_Overridden;
			CarNumber_ColorA_Override.IsChecked = Settings.combined.carNumberColorA_Overridden;
			CarNumber_ColorB_Override.IsChecked = Settings.combined.carNumberColorB_Overridden;
			CarNumber_ColorC_Override.IsChecked = Settings.combined.carNumberColorC_Overridden;
			CarNumber_Pattern_Override.IsChecked = Settings.combined.carNumberPattern_Overridden;
			CarNumber_Slant_Override.IsChecked = Settings.combined.carNumberSlant_Overridden;

			Telemetry_PitColor_R.Value = Settings.combined.telemetryPitColor.r;
			Telemetry_PitColor_G.Value = Settings.combined.telemetryPitColor.g;
			Telemetry_PitColor_B.Value = Settings.combined.telemetryPitColor.b;
			Telemetry_PitColor_A.Value = Settings.combined.telemetryPitColor.a;

			Telemetry_PitColor_R.Value = Settings.combined.telemetryPitColor.r;
			Telemetry_PitColor_G.Value = Settings.combined.telemetryPitColor.g;
			Telemetry_PitColor_B.Value = Settings.combined.telemetryPitColor.b;
			Telemetry_PitColor_A.Value = Settings.combined.telemetryPitColor.a;
			Telemetry_OutColor_R.Value = Settings.combined.telemetryOutColor.r;
			Telemetry_OutColor_G.Value = Settings.combined.telemetryOutColor.g;
			Telemetry_OutColor_B.Value = Settings.combined.telemetryOutColor.b;
			Telemetry_OutColor_A.Value = Settings.combined.telemetryOutColor.a;
			Telemetry_IsInBetweenCars.IsChecked = Settings.combined.telemetryIsBetweenCars;
			Telemetry_Mode_ShowLaps.IsChecked = ( Settings.combined.telemetryMode == 0 );
			TelemetryMode_ShowDistance.IsChecked = ( Settings.combined.telemetryMode == 1 );
			Telemetry_Mode_ShowTime.IsChecked = ( Settings.combined.telemetryMode == 2 );
			Telemetry_NumberOfCheckpoints.Value = Settings.combined.telemetryNumberOfCheckpoints;

			Telemetry_PitColor_Override.IsChecked = Settings.combined.telemetryPitColor_Overridden;
			Telemetry_OutColor_Override.IsChecked = Settings.combined.telemetryOutColor_Overridden;
			Telemetry_IsInBetweenCars_Override.IsChecked = Settings.combined.telemetryIsBetweenCars_Overridden;
			Telemetry_Mode_Override.IsChecked = Settings.combined.telemetryMode_Overridden;
			Telemetry_NumberOfCheckpoints_Override.IsChecked = Settings.combined.telemetryNumberOfCheckpoints_Overridden;

			iRacing_CustomPaintsDirectory.Text = Settings.editor.iracingCustomPaintsDirectory;

			Editor_Mouse_Position_Normal.Value = Settings.editor.positioningSpeedNormal;
			Editor_Mouse_Position_Fast.Value = Settings.editor.positioningSpeedFast;
			Editor_Mouse_Position_Slow.Value = Settings.editor.positioningSpeedSlow;

			initializing--;
		}

		public void InitializeImage()
		{
			if ( Image_ID.SelectedIndex != -1 )
			{
				initializing++;

				var id = (string) Image_ID.Items.GetItemAt( Image_ID.SelectedIndex );

				Settings.UpdateCombinedOverlay();

				var settings = Settings.combined.imageSettingsDataDictionary[ id ];

				Image_ImageType.SelectedItem = settings.imageType;
				Image_FilePath.Text = settings.filePath;
				Image_Position_X.Value = (int) settings.position.x;
				Image_Position_Y.Value = (int) settings.position.y;
				Image_Size_W.Value = (int) settings.size.x;
				Image_Size_H.Value = (int) settings.size.y;
				Image_TintColor_R.Value = settings.tintColor.r;
				Image_TintColor_G.Value = settings.tintColor.g;
				Image_TintColor_B.Value = settings.tintColor.b;
				Image_TintColor_A.Value = settings.tintColor.a;
				Image_Border_L.Value = settings.border.x;
				Image_Border_T.Value = settings.border.y;
				Image_Border_R.Value = settings.border.z;
				Image_Border_B.Value = settings.border.w;

				Image_ImageType_Override.IsChecked = settings.imageType_Overridden;
				Image_FilePath_Override.IsChecked = settings.filePath_Overridden;
				Image_Position_Override.IsChecked = settings.position_Overridden;
				Image_Size_Override.IsChecked = settings.size_Overridden;
				Image_TintColor_Override.IsChecked = settings.tintColor_Overridden;
				Image_Border_Override.IsChecked = settings.border_Overridden;

				initializing--;
			}
		}

		public void InitializeText()
		{
			if ( Text_ID.SelectedIndex != -1 )
			{
				initializing++;

				var id = (string) Text_ID.Items.GetItemAt( Text_ID.SelectedIndex );

				Settings.UpdateCombinedOverlay();

				var settings = Settings.combined.textSettingsDataDictionary[ id ];

				Text_FontIndex.SelectedItem = settings.fontIndex;
				Text_FontSize.Value = settings.fontSize;
				Text_Alignment.SelectedItem = settings.alignment;
				Text_Position_X.Value = (int) settings.position.x;
				Text_Position_Y.Value = (int) settings.position.y;
				Text_Size_W.Value = (int) settings.size.x;
				Text_Size_H.Value = (int) settings.size.y;
				Text_TintColor_R.Value = settings.tintColor.r;
				Text_TintColor_G.Value = settings.tintColor.g;
				Text_TintColor_B.Value = settings.tintColor.b;
				Text_TintColor_A.Value = settings.tintColor.a;

				Text_FontIndex_Override.IsChecked = settings.fontIndex_Overridden;
				Text_FontSize_Override.IsChecked = settings.fontSize_Overridden;
				Text_Alignment_Override.IsChecked = settings.alignment_Overridden;
				Text_Position_Override.IsChecked = settings.position_Overridden;
				Text_Size_Override.IsChecked = settings.size_Overridden;
				Text_TintColor_Override.IsChecked = settings.tintColor_Overridden;

				initializing--;
			}
		}

		public void InitializeTranslation()
		{
			initializing++;

			Settings.UpdateCombinedOverlay();

			L18N_ListView.Items.Clear();

			foreach ( var item in Settings.combined.translationDictionary )
			{
				L18N_ListView.Items.Add( item );
			}

			initializing--;
		}

		private void Window_Closed( object sender, EventArgs e )
		{
			Program.keepRunning = false;
		}

		private void OverlayList_SelectionChanged( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				Settings.overlay = (SettingsOverlay) OverlayList.Items.GetItemAt( OverlayList.SelectedIndex );

				Settings.editor.lastActiveOverlayFilePath = Settings.overlay.filePath;

				Settings.SaveEditor();

				Initialize();

				IPC.readyToSendSettings = true;
			}
		}

		private void OverlayFile_Rename_Click( object sender, EventArgs e )
		{
			var renameOverlay = new RenameOverlay( Settings.overlay.ToString() )
			{
				Owner = this
			};

			renameOverlay.ShowDialog();
		}

		private void OverlayFile_Create_Click( object sender, EventArgs e )
		{
			var overlayFilePath = Settings.overlaySettingsFolder + "My new overlay.xml";

			if ( File.Exists( overlayFilePath ) )
			{
				MessageBox.Show( "Please re-name the 'My new overlay' overlay before creating a new one.", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}
			else
			{
				Settings.overlay = new()
				{
					filePath = overlayFilePath
				};

				Settings.UpdateOverlay( Settings.overlay );

				Settings.overlayList.Add( Settings.overlay );

				Settings.SaveOverlay();

				Settings.editor.lastActiveOverlayFilePath = Settings.overlay.filePath;

				Settings.SaveEditor();

				Initialize();
			}
		}

		private void OverlayFile_Delete_Click( object sender, EventArgs e )
		{
			if ( Settings.overlayList.Count == 1 )
			{
				MessageBox.Show( "You cannot delete your one and only overlay.", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}
			else
			{
				var result = MessageBox.Show( "Are you sure you want to delete this overlay?.", "Please Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No );

				if ( result == MessageBoxResult.Yes )
				{
					File.Delete( Settings.overlay.filePath );

					Settings.overlayList.Remove( Settings.overlay );

					Settings.overlay = Settings.overlayList[ 0 ];

					Settings.editor.lastActiveOverlayFilePath = Settings.overlay.filePath;

					Settings.SaveEditor();

					Initialize();
				}
			}
		}

		private void General_Position_Size_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overridden = General_Position_Override.IsChecked ?? false;

				if ( Settings.overlay.overlayPosition_Overridden != overridden )
				{
					Settings.overlay.overlayPosition_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.overlayPosition_Overridden ? Settings.overlay : Settings.global;

					overlay.overlayPosition.x = General_Position_X.Value;
					overlay.overlayPosition.y = General_Position_Y.Value;
				}

				overridden = General_Size_Override.IsChecked ?? false;

				if ( Settings.overlay.overlaySize_Overridden != overridden )
				{
					Settings.overlay.overlaySize_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.overlaySize_Overridden ? Settings.overlay : Settings.global;

					overlay.overlaySize.x = General_Size_W.Value;
					overlay.overlaySize.y = General_Size_H.Value;
				}

				IPC.readyToSendSettings = true;

				Settings.SaveOverlay();
			}
		}

		private void Font_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overridden = Font_FontA_Name_Override.IsChecked ?? false;

				if ( Settings.overlay.fontNames_Overridden[ 0 ] != overridden )
				{
					Settings.overlay.fontNames_Overridden[ 0 ] = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.fontNames_Overridden[ 0 ] ? Settings.overlay : Settings.global;

					if ( Font_FontA_Name.SelectedItem == null )
					{
						overlay.fontPaths[ 0 ] = string.Empty;
					}
					else
					{
						var fontName = (string) Font_FontA_Name.SelectedItem;

						overlay.fontPaths[ 0 ] = fontPaths[ fontName ];
					}
				}

				overridden = Font_FontB_Name_Override.IsChecked ?? false;

				if ( Settings.overlay.fontNames_Overridden[ 1 ] != overridden )
				{
					Settings.overlay.fontNames_Overridden[ 1 ] = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.fontNames_Overridden[ 1 ] ? Settings.overlay : Settings.global;

					if ( Font_FontB_Name.SelectedItem == null )
					{
						overlay.fontPaths[ 1 ] = string.Empty;
					}
					else
					{
						var fontName = (string) Font_FontB_Name.SelectedItem;

						overlay.fontPaths[ 1 ] = fontPaths[ fontName ] ?? string.Empty;
					}
				}

				overridden = Font_FontC_Name_Override.IsChecked ?? false;

				if ( Settings.overlay.fontNames_Overridden[ 2 ] != overridden )
				{
					Settings.overlay.fontNames_Overridden[ 2 ] = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.fontNames_Overridden[ 2 ] ? Settings.overlay : Settings.global;

					if ( Font_FontC_Name.SelectedItem == null )
					{
						overlay.fontPaths[ 2 ] = string.Empty;
					}
					else
					{
						var fontName = (string) Font_FontC_Name.SelectedItem;

						overlay.fontPaths[ 2 ] = fontPaths[ fontName ];
					}
				}

				overridden = Font_FontD_Name_Override.IsChecked ?? false;

				if ( Settings.overlay.fontNames_Overridden[ 3 ] != overridden )
				{
					Settings.overlay.fontNames_Overridden[ 3 ] = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.fontNames_Overridden[ 3 ] ? Settings.overlay : Settings.global;

					if ( Font_FontD_Name.SelectedItem == null )
					{
						overlay.fontPaths[ 3 ] = string.Empty;
					}
					else
					{
						var fontName = (string) Font_FontD_Name.SelectedItem;

						overlay.fontPaths[ 3 ] = fontPaths[ fontName ];
					}
				}

				IPC.readyToSendSettings = true;

				Settings.SaveOverlay();
			}
		}

		private void Image_ID_SelectionChanged( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				InitializeImage();
			}
		}

		private void Image_FilePath_Button_Click( object sender, EventArgs e )
		{
			string currentFilePath = Image_FilePath.Text;

			var openFileDialog = new OpenFileDialog()
			{
				Title = "Select an Image File",
				Filter = "Image Files (*.jpg; *.png; *.tif; *.bmp)|*.jpg;*.png;*.tif;*.bmp|All files (*.*)|*.*",
				InitialDirectory = ( currentFilePath == string.Empty ) ? Program.documentsFolder : Path.GetDirectoryName( currentFilePath ),
				FileName = currentFilePath,
				ValidateNames = true,
				CheckPathExists = true,
				CheckFileExists = true
			};

			if ( openFileDialog.ShowDialog() == true )
			{
				Image_FilePath.Text = openFileDialog.FileName;
			}
		}

		private void Image_Tint_Palette_Click( object sender, EventArgs e )
		{
			var color = new System.Windows.Media.Color()
			{
				ScR = Image_TintColor_R.Value,
				ScG = Image_TintColor_G.Value,
				ScB = Image_TintColor_B.Value,
				ScA = Image_TintColor_A.Value
			};

			var colorPickerDialog = new ColorPickerDialog( color )
			{
				Owner = this
			};

			var result = colorPickerDialog.ShowDialog();

			if ( result.HasValue && result.Value )
			{
				initializing++;

				Image_TintColor_R.Value = colorPickerDialog.Color.ScR;
				Image_TintColor_G.Value = colorPickerDialog.Color.ScG;
				Image_TintColor_B.Value = colorPickerDialog.Color.ScB;
				Image_TintColor_A.Value = colorPickerDialog.Color.ScA;

				initializing--;

				Image_Update( sender, e );
			}
		}

		private void Image_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overlaySettings = Settings.overlay.imageSettingsDataDictionary[ (string) Image_ID.SelectedItem ];
				var globalSettings = Settings.global.imageSettingsDataDictionary[ (string) Image_ID.SelectedItem ];

				var overridden = Image_ImageType_Override.IsChecked ?? false;

				if ( overlaySettings.imageType_Overridden != overridden )
				{
					overlaySettings.imageType_Overridden = overridden;

					InitializeImage();
				}
				else
				{
					var settings = overlaySettings.imageType_Overridden ? overlaySettings : globalSettings;

					settings.imageType = (SettingsImage.ImageType) Image_ImageType.SelectedItem;
				}

				overridden = Image_FilePath_Override.IsChecked ?? false;

				if ( overlaySettings.filePath_Overridden != overridden )
				{
					overlaySettings.filePath_Overridden = overridden;

					InitializeImage();
				}
				else
				{
					var settings = overlaySettings.filePath_Overridden ? overlaySettings : globalSettings;

					settings.filePath = Image_FilePath.Text;
				}

				overridden = Image_Position_Override.IsChecked ?? false;

				if ( overlaySettings.position_Overridden != overridden )
				{
					overlaySettings.position_Overridden = overridden;

					InitializeImage();
				}
				else
				{
					var settings = overlaySettings.position_Overridden ? overlaySettings : globalSettings;

					settings.position = new Vector2( Image_Position_X.Value, Image_Position_Y.Value );
				}

				overridden = Image_Size_Override.IsChecked ?? false;

				if ( overlaySettings.size_Overridden != overridden )
				{
					overlaySettings.size_Overridden = overridden;

					InitializeImage();
				}
				else
				{
					var settings = overlaySettings.size_Overridden ? overlaySettings : globalSettings;

					settings.size = new Vector2( Image_Size_W.Value, Image_Size_H.Value );
				}

				overridden = Image_TintColor_Override.IsChecked ?? false;

				if ( overlaySettings.tintColor_Overridden != overridden )
				{
					overlaySettings.tintColor_Overridden = overridden;

					InitializeImage();
				}
				else
				{
					var settings = overlaySettings.tintColor_Overridden ? overlaySettings : globalSettings;

					settings.tintColor = new Color( Image_TintColor_R.Value, Image_TintColor_G.Value, Image_TintColor_B.Value, Image_TintColor_A.Value );
				}

				overridden = Image_Border_Override.IsChecked ?? false;

				if ( overlaySettings.border_Overridden != overridden )
				{
					overlaySettings.border_Overridden = overridden;

					InitializeImage();
				}
				else
				{
					var settings = overlaySettings.border_Overridden ? overlaySettings : globalSettings;

					settings.border = new Vector4( Image_Border_L.Value, Image_Border_T.Value, Image_Border_R.Value, Image_Border_B.Value );
				}

				IPC.readyToSendSettings = true;

				Settings.SaveOverlay();
			}
		}

		private void Text_ID_SelectionChanged( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				InitializeText();
			}
		}

		private void Text_Tint_Palette_Click( object sender, EventArgs e )
		{
			var color = new System.Windows.Media.Color()
			{
				ScR = Text_TintColor_R.Value,
				ScG = Text_TintColor_G.Value,
				ScB = Text_TintColor_B.Value,
				ScA = Text_TintColor_A.Value
			};

			var colorPickerDialog = new ColorPickerDialog( color )
			{
				Owner = this
			};

			var result = colorPickerDialog.ShowDialog();

			if ( result.HasValue && result.Value )
			{
				initializing++;

				Text_TintColor_R.Value = colorPickerDialog.Color.ScR;
				Text_TintColor_G.Value = colorPickerDialog.Color.ScG;
				Text_TintColor_B.Value = colorPickerDialog.Color.ScB;
				Text_TintColor_A.Value = colorPickerDialog.Color.ScA;

				initializing--;

				Text_Update( sender, e );
			}
		}

		private void Text_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overlaySettings = Settings.overlay.textSettingsDataDictionary[ (string) Text_ID.SelectedItem ];
				var globalSettings = Settings.global.textSettingsDataDictionary[ (string) Text_ID.SelectedItem ];

				var overridden = Text_FontIndex_Override.IsChecked ?? false;

				if ( overlaySettings.fontIndex_Overridden != overridden )
				{
					overlaySettings.fontIndex_Overridden = overridden;

					InitializeText();
				}
				else
				{
					var settings = overlaySettings.fontIndex_Overridden ? overlaySettings : globalSettings;

					settings.fontIndex = (SettingsText.FontIndex) Text_FontIndex.SelectedItem;
				}

				overridden = Text_FontSize_Override.IsChecked ?? false;

				if ( overlaySettings.fontSize_Overridden != overridden )
				{
					overlaySettings.fontSize_Overridden = overridden;

					InitializeText();
				}
				else
				{
					var settings = overlaySettings.fontSize_Overridden ? overlaySettings : globalSettings;

					settings.fontSize = Text_FontSize.Value;
				}

				overridden = Text_Alignment_Override.IsChecked ?? false;

				if ( overlaySettings.alignment_Overridden != overridden )
				{
					overlaySettings.alignment_Overridden = overridden;

					InitializeText();
				}
				else
				{
					var settings = overlaySettings.alignment_Overridden ? overlaySettings : globalSettings;

					settings.alignment = (TextAlignmentOptions) Text_Alignment.SelectedItem;
				}

				overridden = Text_Position_Override.IsChecked ?? false;

				if ( overlaySettings.position_Overridden != overridden )
				{
					overlaySettings.position_Overridden = overridden;

					InitializeText();
				}
				else
				{
					var settings = overlaySettings.position_Overridden ? overlaySettings : globalSettings;

					settings.position = new Vector2( Text_Position_X.Value, Text_Position_Y.Value );
				}

				overridden = Text_Size_Override.IsChecked ?? false;

				if ( overlaySettings.size_Overridden != overridden )
				{
					overlaySettings.size_Overridden = overridden;

					InitializeText();
				}
				else
				{
					var settings = overlaySettings.size_Overridden ? overlaySettings : globalSettings;

					settings.size = new Vector2( Text_Size_W.Value, Text_Size_H.Value );
				}

				overridden = Text_TintColor_Override.IsChecked ?? false;

				if ( overlaySettings.tintColor_Overridden != overridden )
				{
					overlaySettings.tintColor_Overridden = overridden;

					InitializeText();
				}
				else
				{
					var settings = overlaySettings.tintColor_Overridden ? overlaySettings : globalSettings;

					settings.tintColor = new Color( Text_TintColor_R.Value, Text_TintColor_G.Value, Text_TintColor_B.Value, Text_TintColor_A.Value );
				}

				IPC.readyToSendSettings = true;

				Settings.SaveOverlay();
			}
		}

		private void RaceStatus_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overridden = RaceStatus_OverlayEnable_Override.IsChecked ?? false;

				if ( Settings.overlay.raceStatusOverlayEnabled_Overridden != overridden )
				{
					Settings.overlay.raceStatusOverlayEnabled_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.raceStatusOverlayEnabled_Overridden ? Settings.overlay : Settings.global;

					overlay.raceStatusOverlayEnabled = RaceStatus_Overlay_Enable.IsChecked ?? false;
				}

				overridden = RaceStatus_OverlayPosition_Override.IsChecked ?? false;

				if ( Settings.overlay.raceStatusOverlayPosition_Overridden != overridden )
				{
					Settings.overlay.raceStatusOverlayPosition_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.raceStatusOverlayPosition_Overridden ? Settings.overlay : Settings.global;

					overlay.raceStatusOverlayPosition = new Vector2( RaceStatus_OverlayPosition_X.Value, RaceStatus_OverlayPosition_Y.Value );
				}

				IPC.readyToSendSettings = true;

				Settings.SaveOverlay();
			}
		}

		private void Leaderboard_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overridden = Leaderboard_OverlayEnable_Override.IsChecked ?? false;

				if ( Settings.overlay.leaderboardOverlayEnabled_Overridden != overridden )
				{
					Settings.overlay.leaderboardOverlayEnabled_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.leaderboardOverlayEnabled_Overridden ? Settings.overlay : Settings.global;

					overlay.leaderboardOverlayEnabled = Leaderboard_Overlay_Enable.IsChecked ?? false;
				}

				overridden = Leaderboard_OverlayPosition_Override.IsChecked ?? false;

				if ( Settings.overlay.leaderboardOverlayPosition_Overridden != overridden )
				{
					Settings.overlay.leaderboardOverlayPosition_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.leaderboardOverlayPosition_Overridden ? Settings.overlay : Settings.global;

					overlay.leaderboardOverlayPosition = new Vector2( Leaderboard_OverlayPosition_X.Value, Leaderboard_OverlayPosition_Y.Value );
				}

				overridden = Leaderboard_FirstPlacePosition_Override.IsChecked ?? false;

				if ( Settings.overlay.leaderboardFirstPlacePosition_Overridden != overridden )
				{
					Settings.overlay.leaderboardFirstPlacePosition_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.leaderboardFirstPlacePosition_Overridden ? Settings.overlay : Settings.global;

					overlay.leaderboardFirstPlacePosition = new Vector2( Leaderboard_FirstPlacePosition_X.Value, Leaderboard_FirstPlacePosition_Y.Value );
				}

				overridden = Leaderboard_PlaceCount_Override.IsChecked ?? false;

				if ( Settings.overlay.leaderboardPlaceCount_Overridden != overridden )
				{
					Settings.overlay.leaderboardPlaceCount_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.leaderboardPlaceCount_Overridden ? Settings.overlay : Settings.global;

					overlay.leaderboardPlaceCount = (int) Leaderboard_PlaceCount.Value;
				}

				overridden = Leaderboard_PlaceSpacing_Override.IsChecked ?? false;

				if ( Settings.overlay.leaderboardPlaceSpacing_Overridden != overridden )
				{
					Settings.overlay.leaderboardPlaceSpacing_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.leaderboardPlaceSpacing_Overridden ? Settings.overlay : Settings.global;

					overlay.leaderboardPlaceSpacing = new Vector2( Leaderboard_PlaceSpacing_X.Value, Leaderboard_PlaceSpacing_Y.Value );
				}

				overridden = Leaderboard_UseClassColors_Override.IsChecked ?? false;

				if ( Settings.overlay.leaderboardUseClassColors_Overridden != overridden )
				{
					Settings.overlay.leaderboardUseClassColors_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.leaderboardUseClassColors_Overridden ? Settings.overlay : Settings.global;

					overlay.leaderboardUseClassColors = Leaderboard_UseClassColors_Enable.IsChecked ?? false;
				}

				overridden = Leaderboard_ClassColorStrength_Override.IsChecked ?? false;

				if ( Settings.overlay.leaderboardClassColorStrength_Overridden != overridden )
				{
					Settings.overlay.leaderboardClassColorStrength_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.leaderboardClassColorStrength_Overridden ? Settings.overlay : Settings.global;

					overlay.leaderboardClassColorStrength = (float) ( Leaderboard_ClassColorStrength.Value / 255.0f );
				}

				IPC.readyToSendSettings = true;

				Settings.SaveOverlay();
			}
		}

		private void VoiceOf_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overridden = VoiceOf_OverlayEnable_Override.IsChecked ?? false;

				if ( Settings.overlay.voiceOfOverlayEnabled_Overridden != overridden )
				{
					Settings.overlay.voiceOfOverlayEnabled_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.voiceOfOverlayEnabled_Overridden ? Settings.overlay : Settings.global;

					overlay.voiceOfOverlayEnabled = VoiceOf_Overlay_Enable.IsChecked ?? false;
				}

				overridden = VoiceOf_OverlayPosition_Override.IsChecked ?? false;

				if ( Settings.overlay.voiceOfOverlayPosition_Overridden != overridden )
				{
					Settings.overlay.voiceOfOverlayPosition_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.voiceOfOverlayPosition_Overridden ? Settings.overlay : Settings.global;

					overlay.voiceOfOverlayPosition = new Vector2( VoiceOf_OverlayPosition_X.Value, VoiceOf_OverlayPosition_Y.Value );
				}

				IPC.readyToSendSettings = true;

				Settings.SaveOverlay();
			}
		}

		private void Subtitle_OverlayBackgroundColor_Palette_Click( object sender, EventArgs e )
		{
			var color = new System.Windows.Media.Color()
			{
				ScR = Subtitle_OverlayBackgroundColor_R.Value,
				ScG = Subtitle_OverlayBackgroundColor_G.Value,
				ScB = Subtitle_OverlayBackgroundColor_B.Value,
				ScA = Subtitle_OverlayBackgroundColor_A.Value
			};

			var colorPickerDialog = new ColorPickerDialog( color )
			{
				Owner = this
			};

			var result = colorPickerDialog.ShowDialog();

			if ( result.HasValue && result.Value )
			{
				initializing++;

				Subtitle_OverlayBackgroundColor_R.Value = colorPickerDialog.Color.ScR;
				Subtitle_OverlayBackgroundColor_G.Value = colorPickerDialog.Color.ScG;
				Subtitle_OverlayBackgroundColor_B.Value = colorPickerDialog.Color.ScB;
				Subtitle_OverlayBackgroundColor_A.Value = colorPickerDialog.Color.ScA;

				initializing--;

				Subtitle_Update( sender, e );
			}
		}

		private void Subtitle_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overridden = Subtitle_OverlayEnable_Override.IsChecked ?? false;

				if ( Settings.overlay.subtitleOverlayEnabled_Overridden != overridden )
				{
					Settings.overlay.subtitleOverlayEnabled_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.subtitleOverlayEnabled_Overridden ? Settings.overlay : Settings.global;

					overlay.subtitleOverlayEnabled = Subtitle_Overlay_Enable.IsChecked ?? false;
				}

				overridden = Subtitle_OverlayPosition_Override.IsChecked ?? false;

				if ( Settings.overlay.subtitleOverlayPosition_Overridden != overridden )
				{
					Settings.overlay.subtitleOverlayPosition_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.subtitleOverlayPosition_Overridden ? Settings.overlay : Settings.global;

					overlay.subtitleOverlayPosition = new Vector2( Subtitle_OverlayPosition_X.Value, Subtitle_OverlayPosition_Y.Value );
				}

				overridden = Subtitle_OverlayMaxSize_Override.IsChecked ?? false;

				if ( Settings.overlay.subtitleOverlayMaxSize_Overridden != overridden )
				{
					Settings.overlay.subtitleOverlayMaxSize_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.subtitleOverlayMaxSize_Overridden ? Settings.overlay : Settings.global;

					overlay.subtitleOverlayMaxSize = new Vector2( Subtitle_OverlayMaxSize_W.Value, Subtitle_OverlayMaxSize_H.Value );
				}

				overridden = Subtitle_OverlayBackgroundColor_Override.IsChecked ?? false;

				if ( Settings.overlay.subtitleOverlayBackgroundColor_Overridden != overridden )
				{
					Settings.overlay.subtitleOverlayBackgroundColor_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.subtitleOverlayBackgroundColor_Overridden ? Settings.overlay : Settings.global;

					overlay.subtitleOverlayBackgroundColor = new Color( Subtitle_OverlayBackgroundColor_R.Value, Subtitle_OverlayBackgroundColor_G.Value, Subtitle_OverlayBackgroundColor_B.Value, Subtitle_OverlayBackgroundColor_A.Value );
				}

				overridden = Subtitle_TextPadding_Override.IsChecked ?? false;

				if ( Settings.overlay.subtitleTextPadding_Overridden != overridden )
				{
					Settings.overlay.subtitleTextPadding_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.subtitleTextPadding_Overridden ? Settings.overlay : Settings.global;

					overlay.subtitleTextPadding = new Vector2Int( Subtitle_TextPadding_X.Value, Subtitle_TextPadding_Y.Value );
				}

				IPC.readyToSendSettings = true;

				Settings.SaveOverlay();
			}
		}

		private void CarNumber_ColorOverrideA_Palette_Click( object sender, EventArgs e )
		{
			var color = new System.Windows.Media.Color()
			{
				ScR = CarNumber_ColorA_R.Value,
				ScG = CarNumber_ColorA_G.Value,
				ScB = CarNumber_ColorA_B.Value,
				ScA = CarNumber_ColorA_A.Value
			};

			var colorPickerDialog = new ColorPickerDialog( color )
			{
				Owner = this
			};

			var result = colorPickerDialog.ShowDialog();

			if ( result.HasValue && result.Value )
			{
				initializing++;

				CarNumber_ColorA_R.Value = colorPickerDialog.Color.ScR;
				CarNumber_ColorA_G.Value = colorPickerDialog.Color.ScG;
				CarNumber_ColorA_B.Value = colorPickerDialog.Color.ScB;
				CarNumber_ColorA_A.Value = colorPickerDialog.Color.ScA;

				initializing--;

				CarNumber_Update( sender, e );
			}
		}

		private void CarNumber_ColorOverrideB_Palette_Click( object sender, EventArgs e )
		{
			var color = new System.Windows.Media.Color()
			{
				ScR = CarNumber_ColorB_R.Value,
				ScG = CarNumber_ColorB_G.Value,
				ScB = CarNumber_ColorB_B.Value,
				ScA = CarNumber_ColorB_A.Value
			};

			var colorPickerDialog = new ColorPickerDialog( color )
			{
				Owner = this
			};

			var result = colorPickerDialog.ShowDialog();

			if ( result.HasValue && result.Value )
			{
				initializing++;

				CarNumber_ColorB_R.Value = colorPickerDialog.Color.ScR;
				CarNumber_ColorB_G.Value = colorPickerDialog.Color.ScG;
				CarNumber_ColorB_B.Value = colorPickerDialog.Color.ScB;
				CarNumber_ColorB_A.Value = colorPickerDialog.Color.ScA;

				initializing--;

				CarNumber_Update( sender, e );
			}
		}

		private void CarNumber_ColorOverrideC_Palette_Click( object sender, EventArgs e )
		{
			var color = new System.Windows.Media.Color()
			{
				ScR = CarNumber_ColorC_R.Value,
				ScG = CarNumber_ColorC_G.Value,
				ScB = CarNumber_ColorC_B.Value,
				ScA = CarNumber_ColorC_A.Value
			};

			var colorPickerDialog = new ColorPickerDialog( color )
			{
				Owner = this
			};

			var result = colorPickerDialog.ShowDialog();

			if ( result.HasValue && result.Value )
			{
				initializing++;

				CarNumber_ColorC_R.Value = colorPickerDialog.Color.ScR;
				CarNumber_ColorC_G.Value = colorPickerDialog.Color.ScG;
				CarNumber_ColorC_B.Value = colorPickerDialog.Color.ScB;
				CarNumber_ColorC_A.Value = colorPickerDialog.Color.ScA;

				initializing--;

				CarNumber_Update( sender, e );
			}
		}

		private void CarNumber_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overridden = CarNumber_Override_Enable_Override.IsChecked ?? false;

				if ( Settings.overlay.carNumberOverrideEnabled_Overridden != overridden )
				{
					Settings.overlay.carNumberOverrideEnabled_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.carNumberOverrideEnabled_Overridden ? Settings.overlay : Settings.global;

					overlay.carNumberOverrideEnabled = CarNumber_Override_Enable.IsChecked ?? false;
				}

				overridden = CarNumber_ColorA_Override.IsChecked ?? false;

				if ( Settings.overlay.carNumberColorA_Overridden != overridden )
				{
					Settings.overlay.carNumberColorA_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.carNumberColorA_Overridden ? Settings.overlay : Settings.global;

					overlay.carNumberColorA = new Color( CarNumber_ColorA_R.Value, CarNumber_ColorA_G.Value, CarNumber_ColorA_B.Value, CarNumber_ColorA_A.Value );
				}

				overridden = CarNumber_ColorB_Override.IsChecked ?? false;

				if ( Settings.overlay.carNumberColorB_Overridden != overridden )
				{
					Settings.overlay.carNumberColorB_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.carNumberColorB_Overridden ? Settings.overlay : Settings.global;

					overlay.carNumberColorB = new Color( CarNumber_ColorB_R.Value, CarNumber_ColorB_G.Value, CarNumber_ColorB_B.Value, CarNumber_ColorB_A.Value );
				}

				overridden = CarNumber_ColorC_Override.IsChecked ?? false;

				if ( Settings.overlay.carNumberColorC_Overridden != overridden )
				{
					Settings.overlay.carNumberColorC_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.carNumberColorC_Overridden ? Settings.overlay : Settings.global;

					overlay.carNumberColorC = new Color( CarNumber_ColorC_R.Value, CarNumber_ColorC_G.Value, CarNumber_ColorC_B.Value, CarNumber_ColorC_A.Value );
				}

				overridden = CarNumber_Pattern_Override.IsChecked ?? false;

				if ( Settings.overlay.carNumberPattern_Overridden != overridden )
				{
					Settings.overlay.carNumberPattern_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.carNumberPattern_Overridden ? Settings.overlay : Settings.global;

					overlay.carNumberPattern = patternOptions[ (string) CarNumber_Pattern.SelectedItem ];
				}

				overridden = CarNumber_Slant_Override.IsChecked ?? false;

				if ( Settings.overlay.carNumberSlant_Overridden != overridden )
				{
					Settings.overlay.carNumberSlant_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.carNumberSlant_Overridden ? Settings.overlay : Settings.global;

					overlay.carNumberSlant = slantOptions[ (string) CarNumber_Slant.SelectedItem ];
				}

				IPC.readyToSendSettings = true;

				Settings.SaveOverlay();

				IRSDK.normalizedData.SessionUpdate( true );
			}
		}

		private void Telemetry_PitColor_Palette_Click( object sender, EventArgs e )
		{
			var color = new System.Windows.Media.Color()
			{
				ScR = Telemetry_PitColor_R.Value,
				ScG = Telemetry_PitColor_G.Value,
				ScB = Telemetry_PitColor_B.Value,
				ScA = Telemetry_PitColor_A.Value
			};

			var colorPickerDialog = new ColorPickerDialog( color )
			{
				Owner = this
			};

			var result = colorPickerDialog.ShowDialog();

			if ( result.HasValue && result.Value )
			{
				initializing++;

				Telemetry_PitColor_R.Value = colorPickerDialog.Color.ScR;
				Telemetry_PitColor_G.Value = colorPickerDialog.Color.ScG;
				Telemetry_PitColor_B.Value = colorPickerDialog.Color.ScB;
				Telemetry_PitColor_A.Value = colorPickerDialog.Color.ScA;

				initializing--;

				Telemetry_Update( sender, e );
			}
		}

		private void Telemetry_OutColor_Palette_Click( object sender, EventArgs e )
		{
			var color = new System.Windows.Media.Color()
			{
				ScR = Telemetry_OutColor_R.Value,
				ScG = Telemetry_OutColor_G.Value,
				ScB = Telemetry_OutColor_B.Value,
				ScA = Telemetry_OutColor_A.Value
			};

			var colorPickerDialog = new ColorPickerDialog( color )
			{
				Owner = this
			};

			var result = colorPickerDialog.ShowDialog();

			if ( result.HasValue && result.Value )
			{
				initializing++;

				Telemetry_OutColor_R.Value = colorPickerDialog.Color.ScR;
				Telemetry_OutColor_G.Value = colorPickerDialog.Color.ScG;
				Telemetry_OutColor_B.Value = colorPickerDialog.Color.ScB;
				Telemetry_OutColor_A.Value = colorPickerDialog.Color.ScA;

				initializing--;

				Telemetry_Update( sender, e );
			}
		}

		private void Telemetry_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overridden = Telemetry_PitColor_Override.IsChecked ?? false;

				if ( Settings.overlay.telemetryPitColor_Overridden != overridden )
				{
					Settings.overlay.telemetryPitColor_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.telemetryPitColor_Overridden ? Settings.overlay : Settings.global;

					overlay.telemetryPitColor = new Color( Telemetry_PitColor_R.Value, Telemetry_PitColor_G.Value, Telemetry_PitColor_B.Value, Telemetry_PitColor_A.Value );
				}

				overridden = Telemetry_OutColor_Override.IsChecked ?? false;

				if ( Settings.overlay.telemetryOutColor_Overridden != overridden )
				{
					Settings.overlay.telemetryOutColor_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.telemetryOutColor_Overridden ? Settings.overlay : Settings.global;

					overlay.telemetryOutColor = new Color( Telemetry_OutColor_R.Value, Telemetry_OutColor_G.Value, Telemetry_OutColor_B.Value, Telemetry_OutColor_A.Value );
				}

				overridden = Telemetry_IsInBetweenCars_Override.IsChecked ?? false;

				if ( Settings.overlay.telemetryIsBetweenCars_Overridden != overridden )
				{
					Settings.overlay.telemetryIsBetweenCars_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.telemetryIsBetweenCars_Overridden ? Settings.overlay : Settings.global;

					overlay.telemetryIsBetweenCars = Telemetry_IsInBetweenCars.IsChecked ?? false;
				}

				overridden = Telemetry_Mode_Override.IsChecked ?? false;

				if ( Settings.overlay.telemetryMode_Overridden != overridden )
				{
					Settings.overlay.telemetryMode_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.telemetryMode_Overridden ? Settings.overlay : Settings.global;

					overlay.telemetryMode = 0;

					if ( TelemetryMode_ShowDistance.IsChecked == true )
					{
						overlay.telemetryMode = 1;
					}
					else if ( Telemetry_Mode_ShowTime.IsChecked == true )
					{
						overlay.telemetryMode = 2;
					}
				}

				overridden = Telemetry_NumberOfCheckpoints_Override.IsChecked ?? false;

				if ( Settings.overlay.telemetryNumberOfCheckpoints_Overridden != overridden )
				{
					Settings.overlay.telemetryNumberOfCheckpoints_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.telemetryNumberOfCheckpoints_Overridden ? Settings.overlay : Settings.global;

					overlay.telemetryNumberOfCheckpoints = (int) Telemetry_NumberOfCheckpoints.Value;
				}

				IPC.readyToSendSettings = true;

				Settings.SaveOverlay();
			}
		}

		private void ScanNow_Button_Click( object sender, RoutedEventArgs e )
		{
			if ( !IRSDK.isConnected )
			{
				MessageBox.Show( "iRacing is not running.", "Not Yet", MessageBoxButton.OK, MessageBoxImage.Exclamation );

				return;
			}

			if ( !IRSDK.normalizedSession.isReplay )
			{
				MessageBox.Show( "Sorry, the incidents system does not work outside of replays.", "Not In Replay", MessageBoxButton.OK, MessageBoxImage.Exclamation );

				return;
			}

			IncidentScan.Start();
		}

		private void iRacing_CustomPaintsDirectory_Button_Click( object sender, EventArgs e )
		{
			var commonOpenFileDialog = new CommonOpenFileDialog
			{
				Title = "Choose the iRacing Custom Paints Folder",
				IsFolderPicker = true,
				InitialDirectory = Settings.editor.iracingCustomPaintsDirectory,
				AddToMostRecentlyUsedList = false,
				AllowNonFileSystemItems = false,
				DefaultDirectory = Settings.editor.iracingCustomPaintsDirectory,
				EnsureFileExists = true,
				EnsurePathExists = true,
				EnsureReadOnly = false,
				EnsureValidNames = true,
				Multiselect = false,
				ShowPlacesList = true
			};

			if ( commonOpenFileDialog.ShowDialog() == CommonFileDialogResult.Ok )
			{
				Settings.editor.iracingCustomPaintsDirectory = commonOpenFileDialog.FileName;

				iRacing_CustomPaintsDirectory.Text = Settings.editor.iracingCustomPaintsDirectory;
			}
		}

		private void iRacing_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				Settings.editor.iracingCustomPaintsDirectory = iRacing_CustomPaintsDirectory.Text;

				Settings.SaveEditor();

				IRSDK.normalizedData.SessionUpdate( true );
			}
		}

		private void Editor_Mouse_Position( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				Settings.editor.positioningSpeedNormal = Editor_Mouse_Position_Normal.Value;
				Settings.editor.positioningSpeedFast = Editor_Mouse_Position_Fast.Value;
				Settings.editor.positioningSpeedSlow = Editor_Mouse_Position_Slow.Value;

				Settings.SaveEditor();
			}
		}
	}
}
