
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

			foreach ( var item in Settings.overlayLocal.imageSettingsDataDictionary )
			{
				Overlay_Image_ID.Items.Add( item.Key );
			}

			foreach ( var imageType in Enum.GetValues( typeof( SettingsImage.ImageType ) ) )
			{
				Image_ImageType.Items.Add( imageType );
			}

			fontPaths = FontPaths.FindAll();

			foreach ( var installedFont in fontPaths )
			{
				Overlay_Font_FontA_Name.Items.Add( installedFont.Key );
				Overlay_Font_FontB_Name.Items.Add( installedFont.Key );
				Overlay_Font_FontC_Name.Items.Add( installedFont.Key );
				Overlay_Font_FontD_Name.Items.Add( installedFont.Key );
			}

			foreach ( var item in Settings.overlayLocal.textSettingsDataDictionary )
			{
				Overlay_Text_ID.Items.Add( item.Key );
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
				Overlay_CarNumber_Pattern.Items.Add( item.Key );
			}

			slantOptions.Add( "Normal Slant", 0 );
			slantOptions.Add( "Left Slant", 1 );
			slantOptions.Add( "Right Slant", 2 );
			slantOptions.Add( "Forward Slant", 3 );
			slantOptions.Add( "Backward Slant", 4 );

			foreach ( var item in slantOptions )
			{
				Overlay_CarNumber_Slant.Items.Add( item.Key );
			}

			initializing--;

			Initialize();
		}

		// initialize

		public void Initialize()
		{
			initializing++;

			// director

			DirectorList.Items.Clear();

			foreach ( var director in Settings.directorList )
			{
				DirectorList.Items.Add( director );

				if ( Settings.directorLocal == director )
				{
					DirectorList.SelectedItem = director;
				}
			}

			Settings.UpdateCombinedDirector();

			Director_SwitchDelay_General.Value = Settings.director.switchDelayGeneral;
			Director_SwitchDelay_RacioChatter.Value = Settings.director.switchDelayRadioChatter;

			Director_SwitchDelay_General_Override.IsChecked = Settings.director.switchDelayGeneral_Overridden;
			Director_SwitchDelay_RadioChatter_Override.IsChecked = Settings.director.switchDelayRadioChatter_Overridden;

			Director_Cameras_Practice.Text = Settings.director.camerasPractice;
			Director_Cameras_Qualifying.Text = Settings.director.camerasQualifying;
			Director_Cameras_Intro.Text = Settings.director.camerasIntro;
			Director_Cameras_Inside.Text = Settings.director.camerasInside;
			Director_Cameras_Close.Text = Settings.director.camerasClose;
			Director_Cameras_Medium.Text = Settings.director.camerasMedium;
			Director_Cameras_Far.Text = Settings.director.camerasFar;
			Director_Cameras_VeryFar.Text = Settings.director.camerasVeryFar;

			Director_Cameras_Practice_Override.IsChecked = Settings.director.camerasPractice_Overridden;
			Director_Cameras_Qualifying_Override.IsChecked = Settings.director.camerasQualifying_Overridden;
			Director_Cameras_Intro_Override.IsChecked = Settings.director.camerasIntro_Overridden;
			Director_Cameras_Inside_Override.IsChecked = Settings.director.camerasInside_Overridden;
			Director_Cameras_Close_Override.IsChecked = Settings.director.camerasClose_Overridden;
			Director_Cameras_Medium_Override.IsChecked = Settings.director.camerasMedium_Overridden;
			Director_Cameras_Far_Override.IsChecked = Settings.director.camerasFar_Overridden;
			Director_Cameras_VeryFar_Override.IsChecked = Settings.director.camerasVeryFar_Overridden;

			// overlay

			OverlayList.Items.Clear();

			foreach ( var overlay in Settings.overlayList )
			{
				OverlayList.Items.Add( overlay );

				if ( Settings.overlayLocal == overlay )
				{
					OverlayList.SelectedItem = overlay;
				}
			}

			Settings.UpdateCombinedOverlay();

			Overlay_General_Position_X.Value = Settings.overlay.position.x;
			Overlay_General_Position_Y.Value = Settings.overlay.position.y;

			Overlay_General_Size_W.Value = Settings.overlay.size.x;
			Overlay_General_Size_H.Value = Settings.overlay.size.y;

			Overlay_General_Position_Override.IsChecked = Settings.overlay.position_Overridden;
			Overlay_General_Size_Override.IsChecked = Settings.overlay.size_Overridden;

			Overlay_Font_FontA_Name.SelectedItem = fontPaths.FirstOrDefault( x => x.Value == Settings.overlay.fontPaths[ 0 ] ).Key;
			Overlay_Font_FontB_Name.SelectedItem = fontPaths.FirstOrDefault( x => x.Value == Settings.overlay.fontPaths[ 1 ] ).Key;
			Overlay_Font_FontC_Name.SelectedItem = fontPaths.FirstOrDefault( x => x.Value == Settings.overlay.fontPaths[ 2 ] ).Key;
			Overlay_Font_FontD_Name.SelectedItem = fontPaths.FirstOrDefault( x => x.Value == Settings.overlay.fontPaths[ 3 ] ).Key;

			Overlay_Font_FontA_Name_Override.IsChecked = Settings.overlay.fontNames_Overridden[ 0 ];
			Overlay_Font_FontB_Name_Override.IsChecked = Settings.overlay.fontNames_Overridden[ 1 ];
			Overlay_Font_FontC_Name_Override.IsChecked = Settings.overlay.fontNames_Overridden[ 2 ];
			Overlay_Font_FontD_Name_Override.IsChecked = Settings.overlay.fontNames_Overridden[ 3 ];

			if ( Overlay_Image_ID.SelectedIndex == -1 )
			{
				Overlay_Image_ID.SelectedIndex = 0;
			}

			InitializeOverlayImage();

			if ( Overlay_Text_ID.SelectedIndex == -1 )
			{
				Overlay_Text_ID.SelectedIndex = 0;
			}

			InitializeOverlayText();

			InitializeOverlayTranslation();

			Overlay_RaceStatus_Enable.IsChecked = Settings.overlay.raceStatusEnabled;
			Overlay_RaceStatus_Position_X.Value = (int) Settings.overlay.raceStatusPosition.x;
			Overlay_RaceStatus_Position_Y.Value = (int) Settings.overlay.raceStatusPosition.y;

			Overlay_RaceStatus_Enable_Override.IsChecked = Settings.overlay.raceStatusEnabled_Overridden;
			Overlay_RaceStatus_Position_Override.IsChecked = Settings.overlay.raceStatusPosition_Overridden;

			Overlay_Leaderboard_Enable.IsChecked = Settings.overlay.leaderboardEnabled;
			Overlay_Leaderboard_Position_X.Value = (int) Settings.overlay.leaderboardPosition.x;
			Overlay_Leaderboard_Position_Y.Value = (int) Settings.overlay.leaderboardPosition.y;
			Overlay_Leaderboard_FirstPlacePosition_X.Value = (int) Settings.overlay.leaderboardFirstPlacePosition.x;
			Overlay_Leaderboard_FirstPlacePosition_Y.Value = (int) Settings.overlay.leaderboardFirstPlacePosition.y;
			Overlay_Leaderboard_PlaceCount.Value = Settings.overlay.leaderboardPlaceCount;
			Overlay_Leaderboard_PlaceSpacing_X.Value = (int) Settings.overlay.leaderboardPlaceSpacing.x;
			Overlay_Leaderboard_PlaceSpacing_Y.Value = (int) Settings.overlay.leaderboardPlaceSpacing.y;
			Overlay_Leaderboard_UseClassColors_Enable.IsChecked = Settings.overlay.leaderboardUseClassColors;
			Overlay_Leaderboard_ClassColorStrength.Value = Settings.overlay.leaderboardClassColorStrength * 255.0f;

			Overlay_Leaderboard_Enable_Override.IsChecked = Settings.overlay.leaderboardEnabled_Overridden;
			Overlay_Leaderboard_Position_Override.IsChecked = Settings.overlay.leaderboardPosition_Overridden;
			Overlay_Leaderboard_FirstPlacePosition_Override.IsChecked = Settings.overlay.leaderboardFirstPlacePosition_Overridden;
			Overlay_Leaderboard_PlaceCount_Override.IsChecked = Settings.overlay.leaderboardPlaceCount_Overridden;
			Overlay_Leaderboard_PlaceSpacing_Override.IsChecked = Settings.overlay.leaderboardPlaceSpacing_Overridden;
			Overlay_Leaderboard_UseClassColors_Override.IsChecked = Settings.overlay.leaderboardUseClassColors_Overridden;
			Overlay_Leaderboard_ClassColorStrength_Override.IsChecked = Settings.overlay.leaderboardClassColorStrength_Overridden;

			Overlay_VoiceOf_Enable.IsChecked = Settings.overlay.voiceOfEnabled;
			Overlay_VoiceOf_Position_X.Value = (int) Settings.overlay.voiceOfPosition.x;
			Overlay_VoiceOf_Position_Y.Value = (int) Settings.overlay.voiceOfPosition.y;

			Overlay_VoiceOf_Enable_Override.IsChecked = Settings.overlay.voiceOfEnabled_Overridden;
			Overlay_VoiceOf_Position_Override.IsChecked = Settings.overlay.voiceOfPosition_Overridden;

			Overlay_Subtitle_Enable.IsChecked = Settings.overlay.subtitleEnabled;
			Overlay_Subtitle_Position_X.Value = (int) Settings.overlay.subtitlePosition.x;
			Overlay_Subtitle_Position_Y.Value = (int) Settings.overlay.subtitlePosition.y;
			Overlay_Subtitle_MaxSize_W.Value = (int) Settings.overlay.subtitleMaxSize.x;
			Overlay_Subtitle_MaxSize_H.Value = (int) Settings.overlay.subtitleMaxSize.y;
			Overlay_Subtitle_BackgroundColor_R.Value = Settings.overlay.subtitleBackgroundColor.r;
			Overlay_Subtitle_BackgroundColor_G.Value = Settings.overlay.subtitleBackgroundColor.g;
			Overlay_Subtitle_BackgroundColor_B.Value = Settings.overlay.subtitleBackgroundColor.b;
			Overlay_Subtitle_BackgroundColor_A.Value = Settings.overlay.subtitleBackgroundColor.a;
			Overlay_Subtitle_TextPadding_X.Value = Settings.overlay.subtitleTextPadding.x;
			Overlay_Subtitle_TextPadding_Y.Value = Settings.overlay.subtitleTextPadding.y;

			Subtitle_Enable_Override.IsChecked = Settings.overlay.subtitleEnabled_Overridden;
			Subtitle_Position_Override.IsChecked = Settings.overlay.subtitlePosition_Overridden;
			Subtitle_MaxSize_Override.IsChecked = Settings.overlay.subtitleMaxSize_Overridden;
			Subtitle_BackgroundColor_Override.IsChecked = Settings.overlay.subtitleBackgroundColor_Overridden;
			Subtitle_TextPadding_Override.IsChecked = Settings.overlay.subtitleTextPadding_Overridden;

			Overlay_CarNumber_OverrideEnable.IsChecked = Settings.overlay.carNumberOverrideEnabled;
			Overlay_CarNumber_ColorA_R.Value = Settings.overlay.carNumberColorA.r;
			Overlay_CarNumber_ColorA_G.Value = Settings.overlay.carNumberColorA.g;
			Overlay_CarNumber_ColorA_B.Value = Settings.overlay.carNumberColorA.b;
			Overlay_CarNumber_ColorA_A.Value = Settings.overlay.carNumberColorA.a;
			Overlay_CarNumber_ColorB_R.Value = Settings.overlay.carNumberColorB.r;
			Overlay_CarNumber_ColorB_G.Value = Settings.overlay.carNumberColorB.g;
			Overlay_CarNumber_ColorB_B.Value = Settings.overlay.carNumberColorB.b;
			Overlay_CarNumber_ColorB_A.Value = Settings.overlay.carNumberColorB.a;
			Overlay_CarNumber_ColorC_R.Value = Settings.overlay.carNumberColorC.r;
			Overlay_CarNumber_ColorC_G.Value = Settings.overlay.carNumberColorC.g;
			Overlay_CarNumber_ColorC_B.Value = Settings.overlay.carNumberColorC.b;
			Overlay_CarNumber_ColorC_A.Value = Settings.overlay.carNumberColorC.a;
			Overlay_CarNumber_Pattern.SelectedItem = patternOptions.FirstOrDefault( x => x.Value == Settings.overlay.carNumberPattern ).Key;
			Overlay_CarNumber_Slant.SelectedItem = slantOptions.FirstOrDefault( x => x.Value == Settings.overlay.carNumberSlant ).Key;

			Overlay_CarNumber_OverrideEnable_Override.IsChecked = Settings.overlay.carNumberOverrideEnabled_Overridden;
			Overlay_CarNumber_ColorA_Override.IsChecked = Settings.overlay.carNumberColorA_Overridden;
			Overlay_CarNumber_ColorB_Override.IsChecked = Settings.overlay.carNumberColorB_Overridden;
			Overlay_CarNumber_ColorC_Override.IsChecked = Settings.overlay.carNumberColorC_Overridden;
			Overlay_CarNumber_Pattern_Override.IsChecked = Settings.overlay.carNumberPattern_Overridden;
			Overlay_CarNumber_Slant_Override.IsChecked = Settings.overlay.carNumberSlant_Overridden;

			Overlay_Telemetry_PitColor_R.Value = Settings.overlay.telemetryPitColor.r;
			Overlay_Telemetry_PitColor_G.Value = Settings.overlay.telemetryPitColor.g;
			Overlay_Telemetry_PitColor_B.Value = Settings.overlay.telemetryPitColor.b;
			Overlay_Telemetry_PitColor_A.Value = Settings.overlay.telemetryPitColor.a;
			Overlay_Telemetry_OutColor_R.Value = Settings.overlay.telemetryOutColor.r;
			Overlay_Telemetry_OutColor_G.Value = Settings.overlay.telemetryOutColor.g;
			Overlay_Telemetry_OutColor_B.Value = Settings.overlay.telemetryOutColor.b;
			Overlay_Telemetry_OutColor_A.Value = Settings.overlay.telemetryOutColor.a;
			Overlay_Telemetry_IsInBetweenCars.IsChecked = Settings.overlay.telemetryIsBetweenCars;
			Overlay_Telemetry_Mode_ShowLaps.IsChecked = ( Settings.overlay.telemetryMode == 0 );
			Overlay_TelemetryMode_ShowDistance.IsChecked = ( Settings.overlay.telemetryMode == 1 );
			Overlay_Telemetry_Mode_ShowTime.IsChecked = ( Settings.overlay.telemetryMode == 2 );
			Overlay_Telemetry_NumberOfCheckpoints.Value = Settings.overlay.telemetryNumberOfCheckpoints;

			Overlay_Telemetry_PitColor_Override.IsChecked = Settings.overlay.telemetryPitColor_Overridden;
			Overlay_Telemetry_OutColor_Override.IsChecked = Settings.overlay.telemetryOutColor_Overridden;
			Overlay_Telemetry_IsInBetweenCars_Override.IsChecked = Settings.overlay.telemetryIsBetweenCars_Overridden;
			Overlay_Telemetry_Mode_Override.IsChecked = Settings.overlay.telemetryMode_Overridden;
			Overlay_Telemetry_NumberOfCheckpoints_Override.IsChecked = Settings.overlay.telemetryNumberOfCheckpoints_Overridden;

			// iracing

			iRacing_General_CommandRateLimit.Value = Settings.editor.iracingCommandRateLimit;
			iRacing_CustomPaints_Directory.Text = Settings.editor.iracingCustomPaintsDirectory;

			// editor

			Editor_Mouse_PositioningSpeedNormal.Value = Settings.editor.positioningSpeedNormal;
			Editor_Mouse_PositioningSpeedFast.Value = Settings.editor.positioningSpeedFast;
			Editor_Mouse_PositioningSpeedSlow.Value = Settings.editor.positioningSpeedSlow;

			Editor_Incidents_ScenicCameras.Text = Settings.editor.incidentsScenicCameras;
			Editor_Incidents_EditCameras.Text = Settings.editor.incidentsEditCameras;
			Editor_Incidents_OverlapMergeTime.Value = Settings.editor.incidentsOverlapMergeTime;
			Editor_Incidents_Timeout.Value = Settings.editor.incidentsTimeout;

			//

			initializing--;
		}

		public void InitializeOverlayImage()
		{
			if ( Overlay_Image_ID.SelectedIndex != -1 )
			{
				initializing++;

				var id = (string) Overlay_Image_ID.Items.GetItemAt( Overlay_Image_ID.SelectedIndex );

				Settings.UpdateCombinedOverlay();

				var settings = Settings.overlay.imageSettingsDataDictionary[ id ];

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

		public void InitializeOverlayText()
		{
			if ( Overlay_Text_ID.SelectedIndex != -1 )
			{
				initializing++;

				var id = (string) Overlay_Text_ID.Items.GetItemAt( Overlay_Text_ID.SelectedIndex );

				Settings.UpdateCombinedOverlay();

				var settings = Settings.overlay.textSettingsDataDictionary[ id ];

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

		public void InitializeOverlayTranslation()
		{
			initializing++;

			Settings.UpdateCombinedOverlay();

			L18N_ListView.Items.Clear();

			foreach ( var item in Settings.overlay.translationDictionary )
			{
				L18N_ListView.Items.Add( item );
			}

			initializing--;
		}

		//

		private void Window_Closed( object sender, EventArgs e )
		{
			Program.keepRunning = false;
		}

		// control panel

		private void ControlPanel_EnableDirector_Click( object sender, RoutedEventArgs e )
		{
			Director.isEnabled = true;
		}

		public void ControlPanel_Update()
		{
			Dispatcher.Invoke( () =>
			{
				ControlPanel_TargetCamCarIdx.Text = IRSDK.targetCamCarIdx.ToString();
				ControlPanel_TargetCamGroupNumber.Text = IRSDK.targetCamGroupNumber.ToString();
				ControlPanel_TargetCamReason.Text = IRSDK.targetCamReason;
			} );
		}

		// director

		private void Director_DirectorList_SelectionChanged( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				Settings.directorLocal = (SettingsDirector) DirectorList.Items.GetItemAt( DirectorList.SelectedIndex );

				Settings.editor.lastActiveDirectorFilePath = Settings.directorLocal.filePath;

				Settings.SaveEditor();

				Initialize();
			}
		}

		private void Director_DirectorFile_Rename_Click( object sender, EventArgs e )
		{
			var renameDirector = new RenameDirector( Settings.directorLocal.ToString() )
			{
				Owner = this
			};

			renameDirector.ShowDialog();
		}

		private void Director_DirectorFile_Create_Click( object sender, EventArgs e )
		{
			var directorFilePath = Settings.directorSettingsFolder + "My new director.xml";

			if ( File.Exists( directorFilePath ) )
			{
				MessageBox.Show( this, "Please re-name the 'My new director' director before creating a new one.", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}
			else
			{
				Settings.directorLocal = new()
				{
					filePath = directorFilePath
				};

				Settings.directorList.Add( Settings.directorLocal );

				Settings.SaveDirector();

				Settings.editor.lastActiveDirectorFilePath = Settings.directorLocal.filePath;

				Settings.SaveEditor();

				Initialize();
			}
		}

		private void Director_DirectorFile_Delete_Click( object sender, EventArgs e )
		{
			if ( Settings.directorList.Count == 1 )
			{
				MessageBox.Show( this, "You cannot delete your one and only director.", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}
			else
			{
				var result = MessageBox.Show( this, "Are you sure you want to delete this director?.", "Please Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No );

				if ( result == MessageBoxResult.Yes )
				{
					File.Delete( Settings.directorLocal.filePath );

					Settings.directorList.Remove( Settings.directorLocal );

					Settings.directorLocal = Settings.directorList[ 0 ];

					Settings.editor.lastActiveDirectorFilePath = Settings.directorLocal.filePath;

					Settings.SaveEditor();

					Initialize();
				}
			}
		}

		private void Director_Cameras_Practice_Button_Click( object sender, EventArgs e )
		{
			var cameraSelector = new CameraSelector( Director_Cameras_Practice )
			{
				Owner = this
			};

			cameraSelector.ShowDialog();
		}

		private void Director_Cameras_Qualifying_Button_Click( object sender, EventArgs e )
		{
			var cameraSelector = new CameraSelector( Director_Cameras_Qualifying )
			{
				Owner = this
			};

			cameraSelector.ShowDialog();
		}

		private void Director_Cameras_Intro_Button_Click( object sender, EventArgs e )
		{
			var cameraSelector = new CameraSelector( Director_Cameras_Intro )
			{
				Owner = this
			};

			cameraSelector.ShowDialog();
		}

		private void Director_Cameras_Inside_Button_Click( object sender, EventArgs e )
		{
			var cameraSelector = new CameraSelector( Director_Cameras_Inside )
			{
				Owner = this
			};

			cameraSelector.ShowDialog();
		}

		private void Director_Cameras_Close_Button_Click( object sender, EventArgs e )
		{
			var cameraSelector = new CameraSelector( Director_Cameras_Close )
			{
				Owner = this
			};

			cameraSelector.ShowDialog();
		}

		private void Director_Cameras_Medium_Button_Click( object sender, EventArgs e )
		{
			var cameraSelector = new CameraSelector( Director_Cameras_Medium )
			{
				Owner = this
			};

			cameraSelector.ShowDialog();
		}

		private void Director_Cameras_Far_Button_Click( object sender, EventArgs e )
		{
			var cameraSelector = new CameraSelector( Director_Cameras_Far )
			{
				Owner = this
			};

			cameraSelector.ShowDialog();
		}

		private void Director_Cameras_VeryFar_Button_Click( object sender, EventArgs e )
		{
			var cameraSelector = new CameraSelector( Director_Cameras_VeryFar )
			{
				Owner = this
			};

			cameraSelector.ShowDialog();
		}

		private void Director_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overridden = Director_Cameras_Inside_Override.IsChecked ?? false;

				if ( Settings.directorLocal.camerasInside_Overridden != overridden )
				{
					Settings.directorLocal.camerasInside_Overridden = overridden;

					Initialize();
				}
				else
				{
					var director = Settings.directorLocal.camerasInside_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.camerasInside = Director_Cameras_Inside.Text;
				}

				overridden = Director_Cameras_Close_Override.IsChecked ?? false;

				if ( Settings.directorLocal.camerasClose_Overridden != overridden )
				{
					Settings.directorLocal.camerasClose_Overridden = overridden;

					Initialize();
				}
				else
				{
					var director = Settings.directorLocal.camerasClose_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.camerasClose = Director_Cameras_Close.Text;
				}

				overridden = Director_Cameras_Medium_Override.IsChecked ?? false;

				if ( Settings.directorLocal.camerasMedium_Overridden != overridden )
				{
					Settings.directorLocal.camerasMedium_Overridden = overridden;

					Initialize();
				}
				else
				{
					var director = Settings.directorLocal.camerasMedium_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.camerasMedium = Director_Cameras_Medium.Text;
				}

				overridden = Director_Cameras_Far_Override.IsChecked ?? false;

				if ( Settings.directorLocal.camerasFar_Overridden != overridden )
				{
					Settings.directorLocal.camerasFar_Overridden = overridden;

					Initialize();
				}
				else
				{
					var director = Settings.directorLocal.camerasFar_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.camerasFar = Director_Cameras_Far.Text;
				}

				overridden = Director_Cameras_VeryFar_Override.IsChecked ?? false;

				if ( Settings.directorLocal.camerasVeryFar_Overridden != overridden )
				{
					Settings.directorLocal.camerasVeryFar_Overridden = overridden;

					Initialize();
				}
				else
				{
					var director = Settings.directorLocal.camerasVeryFar_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.camerasVeryFar = Director_Cameras_VeryFar.Text;
				}

				overridden = Director_SwitchDelay_General_Override.IsChecked ?? false;

				if ( Settings.directorLocal.switchDelayGeneral_Overridden != overridden )
				{
					Settings.directorLocal.switchDelayGeneral_Overridden = overridden;

					Initialize();
				}
				else
				{
					var director = Settings.directorLocal.switchDelayGeneral_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.switchDelayGeneral = Director_SwitchDelay_General.Value;
				}

				overridden = Director_SwitchDelay_RadioChatter_Override.IsChecked ?? false;

				if ( Settings.directorLocal.switchDelayRadioChatter_Overridden != overridden )
				{
					Settings.directorLocal.switchDelayRadioChatter_Overridden = overridden;

					Initialize();
				}
				else
				{
					var director = Settings.directorLocal.switchDelayRadioChatter_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.switchDelayRadioChatter = Director_SwitchDelay_RacioChatter.Value;
				}

				Settings.SaveDirector();
			}
		}

		// incidents

		private void Incidents_StartFrame_ValueChanged( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var number = (Number) sender;

				var listViewItem = FindVisualParent<ListViewItem>( number );

				var item = (IncidentData) Incidents_ListView.ItemContainerGenerator.ItemFromContainer( listViewItem );

				item.StartFrame = number.Value;

				IncidentScan.saveIncidentsQueued = true;

				IRSDK.targetReplayStartFrameNumberEnabled = true;
				IRSDK.targetReplayStartFrameNumber = item.StartFrame;
			}
		}

		private void Incidents_EndFrame_ValueChanged( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var number = (Number) sender;

				var listViewItem = FindVisualParent<ListViewItem>( number );

				var item = (IncidentData) Incidents_ListView.ItemContainerGenerator.ItemFromContainer( listViewItem );

				item.EndFrame = number.Value;

				IncidentScan.saveIncidentsQueued = true;

				IRSDK.targetReplayStartFrameNumberEnabled = true;
				IRSDK.targetReplayStartFrameNumber = item.EndFrame;
			}
		}

		private void Incidents_Ignore_Click( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var checkBox = (CheckBox) sender;

				var listViewItem = FindVisualParent<ListViewItem>( checkBox );

				var item = (IncidentData) Incidents_ListView.ItemContainerGenerator.ItemFromContainer( listViewItem );

				item.Ignore = checkBox.IsChecked ?? false;

				IncidentScan.saveIncidentsQueued = true;
			}
		}

		private void Incidents_ListView_MouseDoubleClick( object sender, RoutedEventArgs e )
		{
			if ( ( (FrameworkElement) e.OriginalSource ).DataContext is IncidentData item )
			{
				IRSDK.targetCamEnabled = true;
				IRSDK.targetCamFastSwitchEnabled = true;
				IRSDK.targetCamCarIdx = item.CarIdx;
				IRSDK.targetCamGroupNumber = IRSDK.GetCamGroupNumber( Settings.editor.incidentsEditCameras );

				IRSDK.targetReplayStartFrameNumberEnabled = true;
				IRSDK.targetReplayStartFrameNumber = item.StartFrame;

				if ( item.EndFrame != item.StartFrame )
				{
					IRSDK.targetReplayStartPlaying = true;

					IRSDK.targetReplayStopFrameNumberEnabled = true;
					IRSDK.targetReplayStopFrameNumber = item.EndFrame;
				}
				else
				{
					IRSDK.targetReplayStartPlaying = false;
				}
			}
		}

		private void Incidents_ScanNow_Button_Click( object sender, EventArgs e )
		{
			if ( !IRSDK.isConnected )
			{
				MessageBox.Show( this, "iRacing is not running.", "Not Yet", MessageBoxButton.OK, MessageBoxImage.Exclamation );

				return;
			}

			if ( !IRSDK.normalizedSession.isReplay )
			{
				MessageBox.Show( this, "Sorry, the incidents system does not work outside of replays.", "Not In Replay", MessageBoxButton.OK, MessageBoxImage.Exclamation );

				return;
			}

			if ( Incidents_ListView.Items.Count > 0 )
			{
				if ( MessageBox.Show( this, "Are you sure you want to clear all incidents and re-scan the replay?", "Are You Sure?", MessageBoxButton.OKCancel, MessageBoxImage.Question ) == MessageBoxResult.Cancel )
				{
					return;
				}
			}

			IncidentScan.Start();
		}

		// overlay

		private void Overlay_OverlayList_SelectionChanged( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				Settings.overlayLocal = (SettingsOverlay) OverlayList.Items.GetItemAt( OverlayList.SelectedIndex );

				Settings.editor.lastActiveOverlayFilePath = Settings.overlayLocal.filePath;

				Settings.SaveEditor();

				Initialize();

				IPC.readyToSendSettings = true;
			}
		}

		private void Overlay_OverlayFile_Rename_Click( object sender, EventArgs e )
		{
			var renameOverlay = new RenameOverlay( Settings.overlayLocal.ToString() )
			{
				Owner = this
			};

			renameOverlay.ShowDialog();
		}

		private void Overlay_OverlayFile_Create_Click( object sender, EventArgs e )
		{
			var overlayFilePath = Settings.overlaySettingsFolder + "My new overlay.xml";

			if ( File.Exists( overlayFilePath ) )
			{
				MessageBox.Show( this, "Please re-name the 'My new overlay' overlay before creating a new one.", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}
			else
			{
				Settings.overlayLocal = new()
				{
					filePath = overlayFilePath
				};

				Settings.AddMissingDictionaryItems( Settings.overlayLocal );

				Settings.overlayList.Add( Settings.overlayLocal );

				Settings.SaveOverlay();

				Settings.editor.lastActiveOverlayFilePath = Settings.overlayLocal.filePath;

				Settings.SaveEditor();

				Initialize();
			}
		}

		private void Overlay_OverlayFile_Delete_Click( object sender, EventArgs e )
		{
			if ( Settings.overlayList.Count == 1 )
			{
				MessageBox.Show( this, "You cannot delete your one and only overlay.", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}
			else
			{
				var result = MessageBox.Show( this, "Are you sure you want to delete this overlay?.", "Please Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No );

				if ( result == MessageBoxResult.Yes )
				{
					File.Delete( Settings.overlayLocal.filePath );

					Settings.overlayList.Remove( Settings.overlayLocal );

					Settings.overlayLocal = Settings.overlayList[ 0 ];

					Settings.editor.lastActiveOverlayFilePath = Settings.overlayLocal.filePath;

					Settings.SaveEditor();

					Initialize();
				}
			}
		}

		private void Overlay_General_Position_Size_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overridden = Overlay_General_Position_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.position_Overridden != overridden )
				{
					Settings.overlayLocal.position_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.position_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.position.x = Overlay_General_Position_X.Value;
					overlay.position.y = Overlay_General_Position_Y.Value;
				}

				overridden = Overlay_General_Size_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.size_Overridden != overridden )
				{
					Settings.overlayLocal.size_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.size_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.size.x = Overlay_General_Size_W.Value;
					overlay.size.y = Overlay_General_Size_H.Value;
				}

				IPC.readyToSendSettings = true;

				Settings.SaveOverlay();
			}
		}

		private void Overlay_Font_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overridden = Overlay_Font_FontA_Name_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.fontNames_Overridden[ 0 ] != overridden )
				{
					Settings.overlayLocal.fontNames_Overridden[ 0 ] = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.fontNames_Overridden[ 0 ] ? Settings.overlayLocal : Settings.overlayGlobal;

					if ( Overlay_Font_FontA_Name.SelectedItem == null )
					{
						overlay.fontPaths[ 0 ] = string.Empty;
					}
					else
					{
						var fontName = (string) Overlay_Font_FontA_Name.SelectedItem;

						overlay.fontPaths[ 0 ] = fontPaths[ fontName ];
					}
				}

				overridden = Overlay_Font_FontB_Name_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.fontNames_Overridden[ 1 ] != overridden )
				{
					Settings.overlayLocal.fontNames_Overridden[ 1 ] = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.fontNames_Overridden[ 1 ] ? Settings.overlayLocal : Settings.overlayGlobal;

					if ( Overlay_Font_FontB_Name.SelectedItem == null )
					{
						overlay.fontPaths[ 1 ] = string.Empty;
					}
					else
					{
						var fontName = (string) Overlay_Font_FontB_Name.SelectedItem;

						overlay.fontPaths[ 1 ] = fontPaths[ fontName ] ?? string.Empty;
					}
				}

				overridden = Overlay_Font_FontC_Name_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.fontNames_Overridden[ 2 ] != overridden )
				{
					Settings.overlayLocal.fontNames_Overridden[ 2 ] = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.fontNames_Overridden[ 2 ] ? Settings.overlayLocal : Settings.overlayGlobal;

					if ( Overlay_Font_FontC_Name.SelectedItem == null )
					{
						overlay.fontPaths[ 2 ] = string.Empty;
					}
					else
					{
						var fontName = (string) Overlay_Font_FontC_Name.SelectedItem;

						overlay.fontPaths[ 2 ] = fontPaths[ fontName ];
					}
				}

				overridden = Overlay_Font_FontD_Name_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.fontNames_Overridden[ 3 ] != overridden )
				{
					Settings.overlayLocal.fontNames_Overridden[ 3 ] = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.fontNames_Overridden[ 3 ] ? Settings.overlayLocal : Settings.overlayGlobal;

					if ( Overlay_Font_FontD_Name.SelectedItem == null )
					{
						overlay.fontPaths[ 3 ] = string.Empty;
					}
					else
					{
						var fontName = (string) Overlay_Font_FontD_Name.SelectedItem;

						overlay.fontPaths[ 3 ] = fontPaths[ fontName ];
					}
				}

				IPC.readyToSendSettings = true;

				Settings.SaveOverlay();
			}
		}

		private void Overlay_Image_ID_SelectionChanged( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				InitializeOverlayImage();
			}
		}

		private void Overlay_Image_FilePath_Button_Click( object sender, EventArgs e )
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

		private void Overlay_Image_Tint_Palette_Click( object sender, EventArgs e )
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

				Overlay_Image_Update( sender, e );
			}
		}

		private void Overlay_Image_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overlaySettings = Settings.overlayLocal.imageSettingsDataDictionary[ (string) Overlay_Image_ID.SelectedItem ];
				var globalSettings = Settings.overlayGlobal.imageSettingsDataDictionary[ (string) Overlay_Image_ID.SelectedItem ];

				var overridden = Image_ImageType_Override.IsChecked ?? false;

				if ( overlaySettings.imageType_Overridden != overridden )
				{
					overlaySettings.imageType_Overridden = overridden;

					InitializeOverlayImage();
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

					InitializeOverlayImage();
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

					InitializeOverlayImage();
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

					InitializeOverlayImage();
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

					InitializeOverlayImage();
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

					InitializeOverlayImage();
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

		private void Overlay_Text_ID_SelectionChanged( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				InitializeOverlayText();
			}
		}

		private void Overlay_Text_Tint_Palette_Click( object sender, EventArgs e )
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

				Overlay_Text_Update( sender, e );
			}
		}

		private void Overlay_Text_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overlaySettings = Settings.overlayLocal.textSettingsDataDictionary[ (string) Overlay_Text_ID.SelectedItem ];
				var globalSettings = Settings.overlayGlobal.textSettingsDataDictionary[ (string) Overlay_Text_ID.SelectedItem ];

				var overridden = Text_FontIndex_Override.IsChecked ?? false;

				if ( overlaySettings.fontIndex_Overridden != overridden )
				{
					overlaySettings.fontIndex_Overridden = overridden;

					InitializeOverlayText();
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

					InitializeOverlayText();
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

					InitializeOverlayText();
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

					InitializeOverlayText();
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

					InitializeOverlayText();
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

					InitializeOverlayText();
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

		private void Overlay_RaceStatus_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overridden = Overlay_RaceStatus_Enable_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.raceStatusEnabled_Overridden != overridden )
				{
					Settings.overlayLocal.raceStatusEnabled_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.raceStatusEnabled_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.raceStatusEnabled = Overlay_RaceStatus_Enable.IsChecked ?? false;
				}

				overridden = Overlay_RaceStatus_Position_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.raceStatusPosition_Overridden != overridden )
				{
					Settings.overlayLocal.raceStatusPosition_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.raceStatusPosition_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.raceStatusPosition = new Vector2( Overlay_RaceStatus_Position_X.Value, Overlay_RaceStatus_Position_Y.Value );
				}

				IPC.readyToSendSettings = true;

				Settings.SaveOverlay();
			}
		}

		private void Overlay_Leaderboard_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overridden = Overlay_Leaderboard_Enable_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.leaderboardEnabled_Overridden != overridden )
				{
					Settings.overlayLocal.leaderboardEnabled_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.leaderboardEnabled_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.leaderboardEnabled = Overlay_Leaderboard_Enable.IsChecked ?? false;
				}

				overridden = Overlay_Leaderboard_Position_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.leaderboardPosition_Overridden != overridden )
				{
					Settings.overlayLocal.leaderboardPosition_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.leaderboardPosition_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.leaderboardPosition = new Vector2( Overlay_Leaderboard_Position_X.Value, Overlay_Leaderboard_Position_Y.Value );
				}

				overridden = Overlay_Leaderboard_FirstPlacePosition_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.leaderboardFirstPlacePosition_Overridden != overridden )
				{
					Settings.overlayLocal.leaderboardFirstPlacePosition_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.leaderboardFirstPlacePosition_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.leaderboardFirstPlacePosition = new Vector2( Overlay_Leaderboard_FirstPlacePosition_X.Value, Overlay_Leaderboard_FirstPlacePosition_Y.Value );
				}

				overridden = Overlay_Leaderboard_PlaceCount_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.leaderboardPlaceCount_Overridden != overridden )
				{
					Settings.overlayLocal.leaderboardPlaceCount_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.leaderboardPlaceCount_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.leaderboardPlaceCount = (int) Overlay_Leaderboard_PlaceCount.Value;
				}

				overridden = Overlay_Leaderboard_PlaceSpacing_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.leaderboardPlaceSpacing_Overridden != overridden )
				{
					Settings.overlayLocal.leaderboardPlaceSpacing_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.leaderboardPlaceSpacing_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.leaderboardPlaceSpacing = new Vector2( Overlay_Leaderboard_PlaceSpacing_X.Value, Overlay_Leaderboard_PlaceSpacing_Y.Value );
				}

				overridden = Overlay_Leaderboard_UseClassColors_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.leaderboardUseClassColors_Overridden != overridden )
				{
					Settings.overlayLocal.leaderboardUseClassColors_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.leaderboardUseClassColors_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.leaderboardUseClassColors = Overlay_Leaderboard_UseClassColors_Enable.IsChecked ?? false;
				}

				overridden = Overlay_Leaderboard_ClassColorStrength_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.leaderboardClassColorStrength_Overridden != overridden )
				{
					Settings.overlayLocal.leaderboardClassColorStrength_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.leaderboardClassColorStrength_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.leaderboardClassColorStrength = (float) ( Overlay_Leaderboard_ClassColorStrength.Value / 255.0f );
				}

				IPC.readyToSendSettings = true;

				Settings.SaveOverlay();
			}
		}

		private void Overlay_VoiceOf_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overridden = Overlay_VoiceOf_Enable_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.voiceOfEnabled_Overridden != overridden )
				{
					Settings.overlayLocal.voiceOfEnabled_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.voiceOfEnabled_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.voiceOfEnabled = Overlay_VoiceOf_Enable.IsChecked ?? false;
				}

				overridden = Overlay_VoiceOf_Position_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.voiceOfPosition_Overridden != overridden )
				{
					Settings.overlayLocal.voiceOfPosition_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.voiceOfPosition_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.voiceOfPosition = new Vector2( Overlay_VoiceOf_Position_X.Value, Overlay_VoiceOf_Position_Y.Value );
				}

				IPC.readyToSendSettings = true;

				Settings.SaveOverlay();
			}
		}

		private void Overlay_Subtitle_OverlayBackgroundColor_Palette_Click( object sender, EventArgs e )
		{
			var color = new System.Windows.Media.Color()
			{
				ScR = Overlay_Subtitle_BackgroundColor_R.Value,
				ScG = Overlay_Subtitle_BackgroundColor_G.Value,
				ScB = Overlay_Subtitle_BackgroundColor_B.Value,
				ScA = Overlay_Subtitle_BackgroundColor_A.Value
			};

			var colorPickerDialog = new ColorPickerDialog( color )
			{
				Owner = this
			};

			var result = colorPickerDialog.ShowDialog();

			if ( result.HasValue && result.Value )
			{
				initializing++;

				Overlay_Subtitle_BackgroundColor_R.Value = colorPickerDialog.Color.ScR;
				Overlay_Subtitle_BackgroundColor_G.Value = colorPickerDialog.Color.ScG;
				Overlay_Subtitle_BackgroundColor_B.Value = colorPickerDialog.Color.ScB;
				Overlay_Subtitle_BackgroundColor_A.Value = colorPickerDialog.Color.ScA;

				initializing--;

				Overlay_Subtitle_Update( sender, e );
			}
		}

		private void Overlay_Subtitle_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overridden = Subtitle_Enable_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.subtitleEnabled_Overridden != overridden )
				{
					Settings.overlayLocal.subtitleEnabled_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.subtitleEnabled_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.subtitleEnabled = Overlay_Subtitle_Enable.IsChecked ?? false;
				}

				overridden = Subtitle_Position_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.subtitlePosition_Overridden != overridden )
				{
					Settings.overlayLocal.subtitlePosition_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.subtitlePosition_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.subtitlePosition = new Vector2( Overlay_Subtitle_Position_X.Value, Overlay_Subtitle_Position_Y.Value );
				}

				overridden = Subtitle_MaxSize_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.subtitleMaxSize_Overridden != overridden )
				{
					Settings.overlayLocal.subtitleMaxSize_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.subtitleMaxSize_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.subtitleMaxSize = new Vector2( Overlay_Subtitle_MaxSize_W.Value, Overlay_Subtitle_MaxSize_H.Value );
				}

				overridden = Subtitle_BackgroundColor_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.subtitleBackgroundColor_Overridden != overridden )
				{
					Settings.overlayLocal.subtitleBackgroundColor_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.subtitleBackgroundColor_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.subtitleBackgroundColor = new Color( Overlay_Subtitle_BackgroundColor_R.Value, Overlay_Subtitle_BackgroundColor_G.Value, Overlay_Subtitle_BackgroundColor_B.Value, Overlay_Subtitle_BackgroundColor_A.Value );
				}

				overridden = Subtitle_TextPadding_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.subtitleTextPadding_Overridden != overridden )
				{
					Settings.overlayLocal.subtitleTextPadding_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.subtitleTextPadding_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.subtitleTextPadding = new Vector2Int( Overlay_Subtitle_TextPadding_X.Value, Overlay_Subtitle_TextPadding_Y.Value );
				}

				IPC.readyToSendSettings = true;

				Settings.SaveOverlay();
			}
		}

		private void Overlay_CarNumber_ColorOverrideA_Palette_Click( object sender, EventArgs e )
		{
			var color = new System.Windows.Media.Color()
			{
				ScR = Overlay_CarNumber_ColorA_R.Value,
				ScG = Overlay_CarNumber_ColorA_G.Value,
				ScB = Overlay_CarNumber_ColorA_B.Value,
				ScA = Overlay_CarNumber_ColorA_A.Value
			};

			var colorPickerDialog = new ColorPickerDialog( color )
			{
				Owner = this
			};

			var result = colorPickerDialog.ShowDialog();

			if ( result.HasValue && result.Value )
			{
				initializing++;

				Overlay_CarNumber_ColorA_R.Value = colorPickerDialog.Color.ScR;
				Overlay_CarNumber_ColorA_G.Value = colorPickerDialog.Color.ScG;
				Overlay_CarNumber_ColorA_B.Value = colorPickerDialog.Color.ScB;
				Overlay_CarNumber_ColorA_A.Value = colorPickerDialog.Color.ScA;

				initializing--;

				Overlay_CarNumber_Update( sender, e );
			}
		}

		private void Overlay_CarNumber_ColorOverrideB_Palette_Click( object sender, EventArgs e )
		{
			var color = new System.Windows.Media.Color()
			{
				ScR = Overlay_CarNumber_ColorB_R.Value,
				ScG = Overlay_CarNumber_ColorB_G.Value,
				ScB = Overlay_CarNumber_ColorB_B.Value,
				ScA = Overlay_CarNumber_ColorB_A.Value
			};

			var colorPickerDialog = new ColorPickerDialog( color )
			{
				Owner = this
			};

			var result = colorPickerDialog.ShowDialog();

			if ( result.HasValue && result.Value )
			{
				initializing++;

				Overlay_CarNumber_ColorB_R.Value = colorPickerDialog.Color.ScR;
				Overlay_CarNumber_ColorB_G.Value = colorPickerDialog.Color.ScG;
				Overlay_CarNumber_ColorB_B.Value = colorPickerDialog.Color.ScB;
				Overlay_CarNumber_ColorB_A.Value = colorPickerDialog.Color.ScA;

				initializing--;

				Overlay_CarNumber_Update( sender, e );
			}
		}

		private void Overlay_CarNumber_ColorOverrideC_Palette_Click( object sender, EventArgs e )
		{
			var color = new System.Windows.Media.Color()
			{
				ScR = Overlay_CarNumber_ColorC_R.Value,
				ScG = Overlay_CarNumber_ColorC_G.Value,
				ScB = Overlay_CarNumber_ColorC_B.Value,
				ScA = Overlay_CarNumber_ColorC_A.Value
			};

			var colorPickerDialog = new ColorPickerDialog( color )
			{
				Owner = this
			};

			var result = colorPickerDialog.ShowDialog();

			if ( result.HasValue && result.Value )
			{
				initializing++;

				Overlay_CarNumber_ColorC_R.Value = colorPickerDialog.Color.ScR;
				Overlay_CarNumber_ColorC_G.Value = colorPickerDialog.Color.ScG;
				Overlay_CarNumber_ColorC_B.Value = colorPickerDialog.Color.ScB;
				Overlay_CarNumber_ColorC_A.Value = colorPickerDialog.Color.ScA;

				initializing--;

				Overlay_CarNumber_Update( sender, e );
			}
		}

		private void Overlay_CarNumber_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overridden = Overlay_CarNumber_OverrideEnable_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.carNumberOverrideEnabled_Overridden != overridden )
				{
					Settings.overlayLocal.carNumberOverrideEnabled_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.carNumberOverrideEnabled_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.carNumberOverrideEnabled = Overlay_CarNumber_OverrideEnable.IsChecked ?? false;
				}

				overridden = Overlay_CarNumber_ColorA_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.carNumberColorA_Overridden != overridden )
				{
					Settings.overlayLocal.carNumberColorA_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.carNumberColorA_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.carNumberColorA = new Color( Overlay_CarNumber_ColorA_R.Value, Overlay_CarNumber_ColorA_G.Value, Overlay_CarNumber_ColorA_B.Value, Overlay_CarNumber_ColorA_A.Value );
				}

				overridden = Overlay_CarNumber_ColorB_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.carNumberColorB_Overridden != overridden )
				{
					Settings.overlayLocal.carNumberColorB_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.carNumberColorB_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.carNumberColorB = new Color( Overlay_CarNumber_ColorB_R.Value, Overlay_CarNumber_ColorB_G.Value, Overlay_CarNumber_ColorB_B.Value, Overlay_CarNumber_ColorB_A.Value );
				}

				overridden = Overlay_CarNumber_ColorC_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.carNumberColorC_Overridden != overridden )
				{
					Settings.overlayLocal.carNumberColorC_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.carNumberColorC_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.carNumberColorC = new Color( Overlay_CarNumber_ColorC_R.Value, Overlay_CarNumber_ColorC_G.Value, Overlay_CarNumber_ColorC_B.Value, Overlay_CarNumber_ColorC_A.Value );
				}

				overridden = Overlay_CarNumber_Pattern_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.carNumberPattern_Overridden != overridden )
				{
					Settings.overlayLocal.carNumberPattern_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.carNumberPattern_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.carNumberPattern = patternOptions[ (string) Overlay_CarNumber_Pattern.SelectedItem ];
				}

				overridden = Overlay_CarNumber_Slant_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.carNumberSlant_Overridden != overridden )
				{
					Settings.overlayLocal.carNumberSlant_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.carNumberSlant_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.carNumberSlant = slantOptions[ (string) Overlay_CarNumber_Slant.SelectedItem ];
				}

				IPC.readyToSendSettings = true;

				Settings.SaveOverlay();

				IRSDK.normalizedData.SessionUpdate( true );
			}
		}

		private void Overlay_Telemetry_PitColor_Palette_Click( object sender, EventArgs e )
		{
			var color = new System.Windows.Media.Color()
			{
				ScR = Overlay_Telemetry_PitColor_R.Value,
				ScG = Overlay_Telemetry_PitColor_G.Value,
				ScB = Overlay_Telemetry_PitColor_B.Value,
				ScA = Overlay_Telemetry_PitColor_A.Value
			};

			var colorPickerDialog = new ColorPickerDialog( color )
			{
				Owner = this
			};

			var result = colorPickerDialog.ShowDialog();

			if ( result.HasValue && result.Value )
			{
				initializing++;

				Overlay_Telemetry_PitColor_R.Value = colorPickerDialog.Color.ScR;
				Overlay_Telemetry_PitColor_G.Value = colorPickerDialog.Color.ScG;
				Overlay_Telemetry_PitColor_B.Value = colorPickerDialog.Color.ScB;
				Overlay_Telemetry_PitColor_A.Value = colorPickerDialog.Color.ScA;

				initializing--;

				Overlay_Telemetry_Update( sender, e );
			}
		}

		private void Overlay_Telemetry_OutColor_Palette_Click( object sender, EventArgs e )
		{
			var color = new System.Windows.Media.Color()
			{
				ScR = Overlay_Telemetry_OutColor_R.Value,
				ScG = Overlay_Telemetry_OutColor_G.Value,
				ScB = Overlay_Telemetry_OutColor_B.Value,
				ScA = Overlay_Telemetry_OutColor_A.Value
			};

			var colorPickerDialog = new ColorPickerDialog( color )
			{
				Owner = this
			};

			var result = colorPickerDialog.ShowDialog();

			if ( result.HasValue && result.Value )
			{
				initializing++;

				Overlay_Telemetry_OutColor_R.Value = colorPickerDialog.Color.ScR;
				Overlay_Telemetry_OutColor_G.Value = colorPickerDialog.Color.ScG;
				Overlay_Telemetry_OutColor_B.Value = colorPickerDialog.Color.ScB;
				Overlay_Telemetry_OutColor_A.Value = colorPickerDialog.Color.ScA;

				initializing--;

				Overlay_Telemetry_Update( sender, e );
			}
		}

		private void Overlay_Telemetry_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overridden = Overlay_Telemetry_PitColor_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.telemetryPitColor_Overridden != overridden )
				{
					Settings.overlayLocal.telemetryPitColor_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.telemetryPitColor_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.telemetryPitColor = new Color( Overlay_Telemetry_PitColor_R.Value, Overlay_Telemetry_PitColor_G.Value, Overlay_Telemetry_PitColor_B.Value, Overlay_Telemetry_PitColor_A.Value );
				}

				overridden = Overlay_Telemetry_OutColor_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.telemetryOutColor_Overridden != overridden )
				{
					Settings.overlayLocal.telemetryOutColor_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.telemetryOutColor_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.telemetryOutColor = new Color( Overlay_Telemetry_OutColor_R.Value, Overlay_Telemetry_OutColor_G.Value, Overlay_Telemetry_OutColor_B.Value, Overlay_Telemetry_OutColor_A.Value );
				}

				overridden = Overlay_Telemetry_IsInBetweenCars_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.telemetryIsBetweenCars_Overridden != overridden )
				{
					Settings.overlayLocal.telemetryIsBetweenCars_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.telemetryIsBetweenCars_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.telemetryIsBetweenCars = Overlay_Telemetry_IsInBetweenCars.IsChecked ?? false;
				}

				overridden = Overlay_Telemetry_Mode_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.telemetryMode_Overridden != overridden )
				{
					Settings.overlayLocal.telemetryMode_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.telemetryMode_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.telemetryMode = 0;

					if ( Overlay_TelemetryMode_ShowDistance.IsChecked == true )
					{
						overlay.telemetryMode = 1;
					}
					else if ( Overlay_Telemetry_Mode_ShowTime.IsChecked == true )
					{
						overlay.telemetryMode = 2;
					}
				}

				overridden = Overlay_Telemetry_NumberOfCheckpoints_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.telemetryNumberOfCheckpoints_Overridden != overridden )
				{
					Settings.overlayLocal.telemetryNumberOfCheckpoints_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.telemetryNumberOfCheckpoints_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.telemetryNumberOfCheckpoints = (int) Overlay_Telemetry_NumberOfCheckpoints.Value;
				}

				IPC.readyToSendSettings = true;

				Settings.SaveOverlay();
			}
		}

		// iracing

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

				iRacing_CustomPaints_Directory.Text = Settings.editor.iracingCustomPaintsDirectory;
			}
		}

		private void iRacing_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				Settings.editor.iracingCommandRateLimit = iRacing_General_CommandRateLimit.Value;

				Settings.editor.iracingCustomPaintsDirectory = iRacing_CustomPaints_Directory.Text;

				Settings.SaveEditor();

				IRSDK.normalizedData.SessionUpdate( true );
			}
		}

		// editor

		private void Editor_Incident_ScenicCameras_Button_Click( object sender, EventArgs e )
		{
			var cameraSelector = new CameraSelector( Editor_Incidents_ScenicCameras )
			{
				Owner = this
			};

			cameraSelector.ShowDialog();
		}

		private void Editor_Incident_EditCameras_Button_Click( object sender, EventArgs e )
		{
			var cameraSelector = new CameraSelector( Editor_Incidents_EditCameras )
			{
				Owner = this
			};

			cameraSelector.ShowDialog();
		}

		private void Editor_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				Settings.editor.positioningSpeedNormal = Editor_Mouse_PositioningSpeedNormal.Value;
				Settings.editor.positioningSpeedFast = Editor_Mouse_PositioningSpeedFast.Value;
				Settings.editor.positioningSpeedSlow = Editor_Mouse_PositioningSpeedSlow.Value;

				Settings.editor.incidentsScenicCameras = Editor_Incidents_ScenicCameras.Text;
				Settings.editor.incidentsEditCameras = Editor_Incidents_EditCameras.Text;
				Settings.editor.incidentsOverlapMergeTime = Editor_Incidents_OverlapMergeTime.Value;
				Settings.editor.incidentsTimeout = Editor_Incidents_Timeout.Value;

				Settings.SaveEditor();
			}
		}

		//

		public static T? FindVisualParent<T>( UIElement element ) where T : UIElement
		{
			UIElement? parent = element;
			
			while ( parent != null )
			{
				if ( parent is T correctlyTyped )
				{
					return correctlyTyped;
				}

				parent = System.Windows.Media.VisualTreeHelper.GetParent( parent ) as UIElement;
			}

			return null;
		}
    }
}
