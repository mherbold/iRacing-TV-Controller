
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using Dsafa.WpfColorPicker;

using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	public partial class MainWindow : Window
	{
		public class ControlPanelButton
		{
			public Grid grid;
			public Button button;
			public Label[] label;

			public ControlPanelButton( Grid grid, Button button, Label label1, Label label2, Label label3, Label label4, Label label5 )
			{
				this.grid = grid;
				this.button = button;

				label = new Label[ 5 ];

				label[ 0 ] = label1;
				label[ 1 ] = label2;
				label[ 2 ] = label3;
				label[ 3 ] = label4;
				label[ 4 ] = label5;
			}
		}

		public const string StatusDisconnectedImageFileName = "Assets\\status-disconnected.png";
		public const string StatusConnectedImageFileName = "Assets\\status-connected.png";

		public readonly BitmapImage statusDisconnectedBitmapImage = new( new Uri( $"pack://application:,,,/{StatusDisconnectedImageFileName}" ) );
		public readonly BitmapImage statusConnectedBitmapImage = new( new Uri( $"pack://application:,,,/{StatusConnectedImageFileName}" ) );

		public static MainWindow Instance { get; private set; }

		public int initializing = 0;

		public SettingsDirector.CameraType cameraType = SettingsDirector.CameraType.AutoCam;
		public NormalizedCar? normalizedCar;

		public ControlPanelButton[] controlPanelButton = new ControlPanelButton[ 64 ];

		public SortedDictionary<string, string> fontPaths;

		public SortedDictionary<string, int> patternOptions = new();
		public SortedDictionary<string, int> slantOptions = new();
		public SortedDictionary<string, int> animationOptions = new();
		public SortedDictionary<string, int> capitalizationOptions = new();

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

			// control panel

			UpdateManualCamera();

			for ( int i = 0; i < 8; i++ )
			{
				for ( int j = 0; j < 8; j++ )
				{
					var buttonIndex = ( j + i * 8 );

					var grid = new Grid();

					if ( buttonIndex != 63 )
					{
						grid.Visibility = Visibility.Hidden;
					}

					grid.SetValue( Grid.RowProperty, i );
					grid.SetValue( Grid.ColumnProperty, j );

					if ( buttonIndex <= 63 )
					{
						var button = new Button();

						button.Click += ControlPanel_Grid_Button_Click;

						grid.Children.Add( button );

						var label1 = new Label();

						label1.HorizontalAlignment = HorizontalAlignment.Left;
						label1.VerticalAlignment = VerticalAlignment.Top;
						label1.FontSize = 10;
						label1.Foreground = System.Windows.Media.Brushes.Gray;
						label1.IsHitTestVisible = false;

						grid.Children.Add( label1 );

						var label2 = new Label();

						label2.HorizontalAlignment = HorizontalAlignment.Right;
						label2.VerticalAlignment = VerticalAlignment.Top;
						label2.FontSize = 10;
						label2.Foreground = System.Windows.Media.Brushes.Gray;
						label2.IsHitTestVisible = false;

						grid.Children.Add( label2 );

						var label3 = new Label();

						label3.HorizontalAlignment = HorizontalAlignment.Left;
						label3.VerticalAlignment = VerticalAlignment.Bottom;
						label3.FontSize = 10;
						label3.Foreground = System.Windows.Media.Brushes.Gray;
						label3.IsHitTestVisible = false;

						grid.Children.Add( label3 );

						var label4 = new Label();

						label4.HorizontalAlignment = HorizontalAlignment.Right;
						label4.VerticalAlignment = VerticalAlignment.Bottom;
						label4.FontSize = 10;
						label4.Foreground = System.Windows.Media.Brushes.Gray;
						label4.IsHitTestVisible = false;

						grid.Children.Add( label4 );

						var label5 = new Label();

						label5.HorizontalAlignment = HorizontalAlignment.Stretch;
						label5.VerticalAlignment = VerticalAlignment.Center;
						label5.FontSize = 10;
						label5.Foreground = System.Windows.Media.Brushes.White;
						label5.Background = System.Windows.Media.Brushes.Black;
						label5.Padding = new Thickness( 5, 0, 5, 1 );
						label5.IsHitTestVisible = false;
						label5.FontWeight = FontWeights.Bold;
						label5.Margin = new Thickness( 5, 0, 5, 0 );
						label5.HorizontalContentAlignment = HorizontalAlignment.Center;

						grid.Children.Add( label5 );

						ControlPanel_ButtonGrid.Children.Add( grid );

						if ( buttonIndex == 63 )
						{
							button.Name = "PACE";

							label5.Content = "PACE CAR";
						}

						controlPanelButton[ buttonIndex ] = new ControlPanelButton( grid, button, label1, label2, label3, label4, label5 );
					}
				}
			}

			// director

			foreach ( var item in Enum.GetValues( typeof( SettingsDirector.CameraType ) ) )
			{
				Director_Rules_Rule1_Camera.Items.Add( item );
				Director_Rules_Rule2_Camera.Items.Add( item );
				Director_Rules_Rule3_Camera.Items.Add( item );
				Director_Rules_Rule4_Camera.Items.Add( item );
				Director_Rules_Rule5_Camera.Items.Add( item );
				Director_Rules_Rule6_Camera.Items.Add( item );
				Director_Rules_Rule7_Camera.Items.Add( item );
				Director_Rules_Rule8_Camera.Items.Add( item );
				Director_Rules_Rule9_Camera.Items.Add( item );
				Director_Rules_Rule10_Camera.Items.Add( item );
				Director_Rules_Rule11_Camera.Items.Add( item );
				Director_Rules_Rule12_Camera.Items.Add( item );
				Director_Rules_Rule13_Camera.Items.Add( item );
			}

			// overlay

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

			animationOptions.Add( "Static (Use For Editing)", 0 );
			animationOptions.Add( "Animation #1", 1 );

			foreach ( var item in animationOptions )
			{
				Overlay_Intro_AnimationNumber.Items.Add( item.Key );
			}

			// editor

			capitalizationOptions.Add( "Leave Names Alone", 0 );
			capitalizationOptions.Add( "Change From All Uppercase To Uppercase First Letter Only", 1 );
			capitalizationOptions.Add( "Change To All Uppercase Always", 1 );

			foreach ( var item in capitalizationOptions )
			{
				iRacing_DriverNames_CapitalizationOption.Items.Add( item.Key );
			}

			//

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

			Director_SwitchDelay_Director.Value = Settings.director.switchDelayDirector;
			Director_SwitchDelay_iRacing.Value = Settings.director.switchDelayIracing;
			Director_SwitchDelay_RadioChatter.Value = Settings.director.switchDelayRadioChatter;
			Director_SwitchDelay_NotInRace.Value = Settings.director.switchDelayNotInRace;

			Director_SwitchDelay_Director_Override.IsChecked = Settings.director.switchDelayDirector_Overridden;
			Director_SwitchDelay_iRacing_Override.IsChecked = Settings.director.switchDelayIracing_Overridden;
			Director_SwitchDelay_RadioChatter_Override.IsChecked = Settings.director.switchDelayRadioChatter_Overridden;
			Director_SwitchDelay_NotInRace_Override.IsChecked = Settings.director.switchDelayNotInRace_Overridden;

			Director_Heat_CarLength.Value = Settings.director.heatCarLength;
			Director_Heat_Falloff.Value = Settings.director.heatFalloff;
			Director_Heat_Bias.Value = Settings.director.heatBias;

			Director_Heat_CarLength_Override.IsChecked = Settings.director.heatCarLength_Overridden;
			Director_Heat_Falloff_Override.IsChecked = Settings.director.heatFalloff_Overridden;
			Director_Heat_Bias_Override.IsChecked = Settings.director.heatBias_Overridden;

			Director_PreferredCar_Number.Text = Settings.director.preferredCarNumber;
			Director_PreferredCar_LockOnEnabled.IsChecked = Settings.director.preferredCarLockOnEnabled;
			Director_PreferredCar_LockOnMinimumHeat.Value = Settings.director.preferredCarLockOnMinimumHeat;

			Director_PreferredCar_Number_Override.IsChecked = Settings.director.preferredCarNumber_Overridden;
			Director_PreferredCar_LockOnEnabled_Override.IsChecked = Settings.director.preferredCarLockOnEnabled_Overridden;
			Director_PreferredCar_LockOnMinimumHeat_Override.IsChecked = Settings.director.preferredCarLockOnMinimumHeat_Overridden;

			Director_Rules_Rule1_Enabled.IsChecked = Settings.director.rule1_Enabled;
			Director_Rules_Rule1_Camera.SelectedItem = Settings.director.rule1_Camera;
			Director_Rules_Rule2_Enabled.IsChecked = Settings.director.rule2_Enabled;
			Director_Rules_Rule2_Camera.SelectedItem = Settings.director.rule2_Camera;
			Director_Rules_Rule3_Enabled.IsChecked = Settings.director.rule3_Enabled;
			Director_Rules_Rule3_Camera.SelectedItem = Settings.director.rule3_Camera;
			Director_Rules_Rule4_Enabled.IsChecked = Settings.director.rule4_Enabled;
			Director_Rules_Rule4_Camera.SelectedItem = Settings.director.rule4_Camera;
			Director_Rules_Rule5_Enabled.IsChecked = Settings.director.rule5_Enabled;
			Director_Rules_Rule5_Camera.SelectedItem = Settings.director.rule5_Camera;
			Director_Rules_Rule6_Enabled.IsChecked = Settings.director.rule6_Enabled;
			Director_Rules_Rule6_Camera.SelectedItem = Settings.director.rule6_Camera;
			Director_Rules_Rule7_Enabled.IsChecked = Settings.director.rule7_Enabled;
			Director_Rules_Rule7_Camera.SelectedItem = Settings.director.rule7_Camera;
			Director_Rules_Rule8_Enabled.IsChecked = Settings.director.rule8_Enabled;
			Director_Rules_Rule8_Camera.SelectedItem = Settings.director.rule8_Camera;
			Director_Rules_Rule9_Enabled.IsChecked = Settings.director.rule9_Enabled;
			Director_Rules_Rule9_Camera.SelectedItem = Settings.director.rule9_Camera;
			Director_Rules_Rule10_Enabled.IsChecked = Settings.director.rule10_Enabled;
			Director_Rules_Rule10_Camera.SelectedItem = Settings.director.rule10_Camera;
			Director_Rules_Rule11_Enabled.IsChecked = Settings.director.rule11_Enabled;
			Director_Rules_Rule11_Camera.SelectedItem = Settings.director.rule11_Camera;
			Director_Rules_Rule12_Enabled.IsChecked = Settings.director.rule12_Enabled;
			Director_Rules_Rule12_Camera.SelectedItem = Settings.director.rule12_Camera;
			Director_Rules_Rule13_Enabled.IsChecked = Settings.director.rule13_Enabled;
			Director_Rules_Rule13_Camera.SelectedItem = Settings.director.rule13_Camera;

			Director_Rules_Override.IsChecked = Settings.director.rules_Overridden;

			Director_AutoCam_Inside_Minimum.Value = Settings.director.autoCamInsideMinimum;
			Director_AutoCam_Inside_Maximum.Value = Settings.director.autoCamInsideMaximum;
			Director_AutoCam_Close_Maximum.Value = Settings.director.autoCamCloseMaximum;
			Director_AutoCam_Medium_Maximum.Value = Settings.director.autoCamMediumMaximum;
			Director_AutoCam_Far_Maximum.Value = Settings.director.autoCamFarMaximum;

			Director_AutoCam_Override.IsChecked = Settings.director.autoCam_Overridden;

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

			// overlay - general

			Overlay_General_Position_X.Value = Settings.overlay.position.x;
			Overlay_General_Position_Y.Value = Settings.overlay.position.y;

			Overlay_General_Size_W.Value = Settings.overlay.size.x;
			Overlay_General_Size_H.Value = Settings.overlay.size.y;

			Overlay_General_Position_Override.IsChecked = Settings.overlay.position_Overridden;
			Overlay_General_Size_Override.IsChecked = Settings.overlay.size_Overridden;

			// overlay - fonts

			Overlay_Font_FontA_Name.SelectedItem = fontPaths.FirstOrDefault( x => x.Value == Settings.overlay.fontPaths[ 0 ] ).Key;
			Overlay_Font_FontB_Name.SelectedItem = fontPaths.FirstOrDefault( x => x.Value == Settings.overlay.fontPaths[ 1 ] ).Key;
			Overlay_Font_FontC_Name.SelectedItem = fontPaths.FirstOrDefault( x => x.Value == Settings.overlay.fontPaths[ 2 ] ).Key;
			Overlay_Font_FontD_Name.SelectedItem = fontPaths.FirstOrDefault( x => x.Value == Settings.overlay.fontPaths[ 3 ] ).Key;

			Overlay_Font_FontA_Name_Override.IsChecked = Settings.overlay.fontNames_Overridden[ 0 ];
			Overlay_Font_FontB_Name_Override.IsChecked = Settings.overlay.fontNames_Overridden[ 1 ];
			Overlay_Font_FontC_Name_Override.IsChecked = Settings.overlay.fontNames_Overridden[ 2 ];
			Overlay_Font_FontD_Name_Override.IsChecked = Settings.overlay.fontNames_Overridden[ 3 ];

			// overlay - images

			if ( Overlay_Image_ID.SelectedIndex == -1 )
			{
				Overlay_Image_ID.SelectedIndex = 0;
			}

			InitializeOverlayImage();

			// overlay - text

			if ( Overlay_Text_ID.SelectedIndex == -1 )
			{
				Overlay_Text_ID.SelectedIndex = 0;
			}

			InitializeOverlayText();

			// overlay - localization

			InitializeOverlayTranslation();

			// overlay - race status

			Overlay_RaceStatus_Enable.IsChecked = Settings.overlay.raceStatusEnabled;
			Overlay_RaceStatus_Position_X.Value = (int) Settings.overlay.raceStatusPosition.x;
			Overlay_RaceStatus_Position_Y.Value = (int) Settings.overlay.raceStatusPosition.y;

			Overlay_RaceStatus_Enable_Override.IsChecked = Settings.overlay.raceStatusEnabled_Overridden;
			Overlay_RaceStatus_Position_Override.IsChecked = Settings.overlay.raceStatusPosition_Overridden;

			// overlay - leaderboard

			Overlay_Leaderboard_Enable.IsChecked = Settings.overlay.leaderboardEnabled;
			Overlay_Leaderboard_Position_X.Value = (int) Settings.overlay.leaderboardPosition.x;
			Overlay_Leaderboard_Position_Y.Value = (int) Settings.overlay.leaderboardPosition.y;
			Overlay_Leaderboard_FirstSlotPosition_X.Value = (int) Settings.overlay.leaderboardFirstSlotPosition.x;
			Overlay_Leaderboard_FirstSlotPosition_Y.Value = (int) Settings.overlay.leaderboardFirstSlotPosition.y;
			Overlay_Leaderboard_SlotCount.Value = Settings.overlay.leaderboardSlotCount;
			Overlay_Leaderboard_SlotSpacing_X.Value = (int) Settings.overlay.leaderboardSlotSpacing.x;
			Overlay_Leaderboard_SlotSpacing_Y.Value = (int) Settings.overlay.leaderboardSlotSpacing.y;
			Overlay_Leaderboard_UseClassColors_Enable.IsChecked = Settings.overlay.leaderboardUseClassColors;
			Overlay_Leaderboard_ClassColorStrength.Value = Settings.overlay.leaderboardClassColorStrength * 255.0f;

			Overlay_Leaderboard_Enable_Override.IsChecked = Settings.overlay.leaderboardEnabled_Overridden;
			Overlay_Leaderboard_Position_Override.IsChecked = Settings.overlay.leaderboardPosition_Overridden;
			Overlay_Leaderboard_FirstSlotPosition_Override.IsChecked = Settings.overlay.leaderboardFirstSlotPosition_Overridden;
			Overlay_Leaderboard_SlotCount_Override.IsChecked = Settings.overlay.leaderboardSlotCount_Overridden;
			Overlay_Leaderboard_SlotSpacing_Override.IsChecked = Settings.overlay.leaderboardSlotSpacing_Overridden;
			Overlay_Leaderboard_UseClassColors_Override.IsChecked = Settings.overlay.leaderboardUseClassColors_Overridden;
			Overlay_Leaderboard_ClassColorStrength_Override.IsChecked = Settings.overlay.leaderboardClassColorStrength_Overridden;

			// overlay - voice of

			Overlay_VoiceOf_Enable.IsChecked = Settings.overlay.voiceOfEnabled;
			Overlay_VoiceOf_Position_X.Value = (int) Settings.overlay.voiceOfPosition.x;
			Overlay_VoiceOf_Position_Y.Value = (int) Settings.overlay.voiceOfPosition.y;

			Overlay_VoiceOf_Enable_Override.IsChecked = Settings.overlay.voiceOfEnabled_Overridden;
			Overlay_VoiceOf_Position_Override.IsChecked = Settings.overlay.voiceOfPosition_Overridden;

			// overlay - subtitle

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

			Overlay_Subtitle_Enable_Override.IsChecked = Settings.overlay.subtitleEnabled_Overridden;
			Overlay_Subtitle_Position_Override.IsChecked = Settings.overlay.subtitlePosition_Overridden;
			Overlay_Subtitle_MaxSize_Override.IsChecked = Settings.overlay.subtitleMaxSize_Overridden;
			Overlay_Subtitle_BackgroundColor_Override.IsChecked = Settings.overlay.subtitleBackgroundColor_Overridden;
			Overlay_Subtitle_TextPadding_Override.IsChecked = Settings.overlay.subtitleTextPadding_Overridden;

			// overlay - car number

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

			// overlay - telemetry

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

			// overlay - intro

			Overlay_Intro_Enable.IsChecked = Settings.overlay.introEnabled;
			Overlay_Intro_StartTime.Value = Settings.overlay.introStartTime;
			Overlay_Intro_RowInterval.Value = Settings.overlay.introRowInterval;
			Overlay_Intro_AnimationNumber.SelectedItem = animationOptions.FirstOrDefault( x => x.Value == Settings.overlay.introAnimationNumber ).Key;
			Overlay_Intro_AnimationSpeed.Value = Settings.overlay.introAnimationSpeed;
			Overlay_Intro_LeftPosition_X.Value = Settings.overlay.introLeftPosition.x;
			Overlay_Intro_LeftPosition_Y.Value = Settings.overlay.introLeftPosition.y;
			Overlay_Intro_LeftScale.Value = Settings.overlay.introLeftScale;
			Overlay_Intro_RightPosition_X.Value = Settings.overlay.introRightPosition.x;
			Overlay_Intro_RightPosition_Y.Value = Settings.overlay.introRightPosition.y;
			Overlay_Intro_RightScale.Value = Settings.overlay.introRightScale;

			Overlay_Intro_Enable_Override.IsChecked = Settings.overlay.introEnabled_Overridden;
			Overlay_Intro_StartTime_Override.IsChecked = Settings.overlay.introStartTime_Overridden;
			Overlay_Intro_RowInterval_Override.IsChecked = Settings.overlay.introRowInterval_Overridden;
			Overlay_Intro_AnimationNumber_Override.IsChecked = Settings.overlay.introAnimationNumber_Overridden;
			Overlay_Intro_AnimationSpeed_Override.IsChecked = Settings.overlay.introAnimationSpeed_Overridden;
			Overlay_Intro_LeftPosition_Override.IsChecked = Settings.overlay.introLeftPosition_Overridden;
			Overlay_Intro_LeftScale_Override.IsChecked = Settings.overlay.introLeftScale_Overridden;
			Overlay_Intro_RightPosition_Override.IsChecked = Settings.overlay.introRightPosition_Overridden;
			Overlay_Intro_RightScale_Override.IsChecked = Settings.overlay.introRightScale_Overridden;

			// iracing

			iRacing_General_CommandRateLimit.Value = Settings.editor.iracingGeneralCommandRateLimit;

			iRacing_CustomPaints_Directory.Text = Settings.editor.iracingCustomPaintsDirectory;

			iRacing_DriverNames_Suffixes.Text = Settings.editor.iracingDriverNamesSuffixes;
			iRacing_DriverNames_CapitalizationOption.SelectedItem = capitalizationOptions.FirstOrDefault( x => x.Value == Settings.editor.iracingDriverNameCapitalizationOption ).Key;

			// editor

			Editor_Mouse_PositioningSpeedNormal.Value = Settings.editor.editorMousePositioningSpeedNormal;
			Editor_Mouse_PositioningSpeedFast.Value = Settings.editor.editorMousePositioningSpeedFast;
			Editor_Mouse_PositioningSpeedSlow.Value = Settings.editor.editorMousePositioningSpeedSlow;

			Editor_Incidents_ScenicCameras.Text = Settings.editor.editorIncidentsScenicCameras;
			Editor_Incidents_EditCameras.Text = Settings.editor.editorIncidentsEditCameras;
			Editor_Incidents_OverlapMergeTime.Value = Settings.editor.editorIncidentsOverlapMergeTime;
			Editor_Incidents_Timeout.Value = Settings.editor.editorIncidentsTimeout;

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
			Director.isEnabled = !Director.isEnabled;

			ControlPanel_EnableDirector_Button.Content = ( Director.isEnabled ) ? "🎥 Disable Director" : "🎥 Enable Director";
		}

		public void ControlPanel_Update()
		{
			Dispatcher.Invoke( () =>
			{
				if ( IRSDK.isConnected )
				{
					Status.Content = $"{IRSDK.normalizedSession.sessionName} - {IRSDK.normalizedData.sessionState}";
					FrameNumber.Content = IRSDK.normalizedData.replayFrameNum;

					ConnectionStatusImage.Source = statusConnectedBitmapImage;
				}
				else
				{
					Status.Content = string.Empty;
					FrameNumber.Content = string.Empty;

					ConnectionStatusImage.Source = statusDisconnectedBitmapImage;
				}

				if ( Director.isEnabled )
				{
					DirectorStatusImage.Source = statusConnectedBitmapImage;

					var normalizedCar = IRSDK.normalizedData.FindNormalizedCarByCarIdx( IRSDK.targetCamCarIdx );

					if ( normalizedCar != null )
					{
						ControlPanel_TargetCamCarNumber.Text = $"#{normalizedCar.carNumber}";
						ControlPanel_TargetDriverName.Text = normalizedCar.userName;
					}
					else
					{
						ControlPanel_TargetCamCarNumber.Text = string.Empty;
						ControlPanel_TargetDriverName.Text = string.Empty;
					}

					ControlPanel_TargetCamGroupNumber.Text = IRSDK.GetCamGroupName( IRSDK.targetCamGroupNumber );
					ControlPanel_TargetCamReason.Text = IRSDK.targetCamReason;
					ControlPanel_CameraSwitchTimer.Text = $"{IRSDK.cameraSwitchWaitTimeRemaining:0.0}";
				}
				else
				{
					DirectorStatusImage.Source = statusDisconnectedBitmapImage;

					ControlPanel_TargetCamCarNumber.Text = string.Empty;
					ControlPanel_TargetDriverName.Text = string.Empty;
					ControlPanel_TargetCamGroupNumber.Text = string.Empty;
					ControlPanel_TargetCamReason.Text = string.Empty;
					ControlPanel_CameraSwitchTimer.Text = string.Empty;
				}

				var widthScale = ActualWidth / 860;
				var heightScale = ActualHeight / 550;

				var fontSize = 10 * Math.Min( widthScale, heightScale );

				foreach ( var cpb in controlPanelButton )
				{
					for ( var i = 0; i < 5; i++ )
					{
						cpb.label[ i ].FontSize = fontSize;
					}
				}

				int controlPanelButtonIndex_Front = 0;
				int controlPanelButtonIndex_Back = 62;

				foreach ( var normalizedCar in IRSDK.normalizedData.relativeLapPositionSortedNormalizedCars )
				{
					ControlPanelButton cpb;

					if ( normalizedCar.carIdx == 0 )
					{
						cpb = controlPanelButton[ 63 ];

						if ( Director.isOverridden && ( IRSDK.targetCamCarIdx == normalizedCar.carIdx ) )
						{
							if ( ( Program.stopwatch.ElapsedMilliseconds / 500 % 2 ) == 0 )
							{
								cpb.button.BorderBrush = System.Windows.Media.Brushes.DeepSkyBlue;
							}
							else
							{
								cpb.button.BorderBrush = System.Windows.Media.Brushes.SkyBlue;
							}

							cpb.button.BorderThickness = new Thickness( 3.0 );
						}
						else if ( normalizedCar.carIdx == IRSDK.normalizedData.camCarIdx )
						{
							cpb.button.BorderBrush = System.Windows.Media.Brushes.Green;
							cpb.button.BorderThickness = new Thickness( 3.0 );
						}
						else
						{
							cpb.button.BorderBrush = System.Windows.Media.Brushes.DarkGray;
							cpb.button.BorderThickness = new Thickness( 0.5 );
						}

						cpb.button.Background = System.Windows.Media.Brushes.White;

						continue;
					}

					if ( normalizedCar.isOnPitRoad || ( normalizedCar.outOfCarTimer >= 10 ) )
					{
						cpb = controlPanelButton[ controlPanelButtonIndex_Back-- ];
					}
					else
					{
						cpb = controlPanelButton[ controlPanelButtonIndex_Front++ ];
					}

					if ( normalizedCar.includeInLeaderboard )
					{
						cpb.grid.Visibility = Visibility.Visible;

						cpb.button.Name = $"P{normalizedCar.carIdx}";

						cpb.label[ 4 ].Background = System.Windows.Media.Brushes.Black;
						cpb.label[ 4 ].Foreground = new System.Windows.Media.SolidColorBrush( System.Windows.Media.Color.FromArgb( 255, (byte) ( 128 + normalizedCar.classColor.r * 127 ), (byte) ( 128 + normalizedCar.classColor.g * 127 ), (byte) ( 128 + normalizedCar.classColor.b * 127 ) ) );

						if ( normalizedCar.lapPositionRelativeToClassLeader >= 1 )
						{
							cpb.label[ 4 ].Content = $"↓ {normalizedCar.abbrevName} ↓";
						}
						else
						{
							cpb.label[ 4 ].Content = normalizedCar.abbrevName;
						}

						cpb.label[ 0 ].Content = $"P{normalizedCar.leaderboardIndex}";
						cpb.label[ 1 ].Content = $"#{normalizedCar.carNumber}";

						if ( normalizedCar.attackingHeat > 0 )
						{
							cpb.label[ 2 ].Content = $"{normalizedCar.attackingHeat:0.0}";
						}
						else
						{
							cpb.label[ 2 ].Content = string.Empty;
						}

						var heat = normalizedCar.attackingHeat;

						var backgroundButton = System.Windows.Media.Brushes.White;
						var foregroundLabels = System.Windows.Media.Brushes.Black;

						if ( heat > 0 )
						{
							var r = 0;
							var g = 0;
							var b = 0;

							if ( heat < 1 )
							{
								r = g = 255;
								b = (int) Math.Round( 255 - ( heat - 0 ) * 255 * 0.5 );
							}
							else if ( heat < 2 )
							{
								r = 255;
								g = (int) Math.Round( 255 - ( heat - 1 ) * 255 * 0.5 );
								b = 128;
							}
							else if ( heat < 3 )
							{
								r = 255;
								g = b = (int) Math.Round( 128 - ( heat - 2 ) * 255 * 0.5 );
							}
							else
							{
								r = 255;
								g = b = 0;
							}

							backgroundButton = new System.Windows.Media.SolidColorBrush( System.Windows.Media.Color.FromArgb( 255, (byte) r, (byte) g, (byte) b ) );
						}

						cpb.button.Background = backgroundButton;

						cpb.label[ 0 ].Foreground = foregroundLabels;
						cpb.label[ 1 ].Foreground = foregroundLabels;
						cpb.label[ 2 ].Foreground = foregroundLabels;

						if ( normalizedCar.isOutOfCar )
						{
							cpb.label[ 3 ].Content = "OUT";
							cpb.label[ 3 ].Foreground = System.Windows.Media.Brushes.Red;
						}
						else if ( normalizedCar.isOnPitRoad )
						{
							cpb.label[ 3 ].Content = "PIT";
							cpb.label[ 3 ].Foreground = System.Windows.Media.Brushes.Black;
						}
						else
						{
							cpb.label[ 3 ].Content = string.Empty;
							cpb.label[ 3 ].Foreground = System.Windows.Media.Brushes.Black;
						}

						if ( Director.isOverridden && ( IRSDK.targetCamCarIdx == normalizedCar.carIdx ) )
						{
							if ( ( Program.stopwatch.ElapsedMilliseconds / 500 % 2 ) == 0 )
							{
								cpb.button.BorderBrush = System.Windows.Media.Brushes.DeepSkyBlue;
							}
							else
							{
								cpb.button.BorderBrush = System.Windows.Media.Brushes.SkyBlue;
							}

							cpb.button.BorderThickness = new Thickness( 3.0 );
						}
						else if ( normalizedCar.carIdx == IRSDK.normalizedData.camCarIdx )
						{
							cpb.button.BorderBrush = System.Windows.Media.Brushes.Green;
							cpb.button.BorderThickness = new Thickness( 3.0 );
						}
						else
						{
							cpb.button.BorderBrush = System.Windows.Media.Brushes.DarkGray;
							cpb.button.BorderThickness = new Thickness( 0.5 );
						}
					}
					else
					{
						cpb.grid.Visibility = Visibility.Hidden;
					}
				}

				while ( controlPanelButtonIndex_Front <= controlPanelButtonIndex_Back )
				{
					var cpb = controlPanelButton[ controlPanelButtonIndex_Front++ ];

					cpb.grid.Visibility = Visibility.Hidden;
				}

				UpdateCameraButton( ControlPanel_Camera_Intro_Button, cameraType == SettingsDirector.CameraType.Intro );
				UpdateCameraButton( ControlPanel_Camera_Inside_Button, cameraType == SettingsDirector.CameraType.Inside );
				UpdateCameraButton( ControlPanel_Camera_Close_Button, cameraType == SettingsDirector.CameraType.Close );
				UpdateCameraButton( ControlPanel_Camera_Medium_Button, cameraType == SettingsDirector.CameraType.Medium );
				UpdateCameraButton( ControlPanel_Camera_Far_Button, cameraType == SettingsDirector.CameraType.Far );
				UpdateCameraButton( ControlPanel_Camera_VeryFar_Button, cameraType == SettingsDirector.CameraType.VeryFar );
				UpdateCameraButton( ControlPanel_Camera_AutoCam_Button, cameraType == SettingsDirector.CameraType.AutoCam );
			} );
		}

		private void UpdateCameraButton( Button button, bool isActive )
		{
			if ( isActive )
			{
				if ( Director.isOverridden && ( ( Program.stopwatch.ElapsedMilliseconds / 500 % 2 ) == 0 ) )
				{
					button.BorderBrush = System.Windows.Media.Brushes.DeepSkyBlue;
				}
				else
				{
					button.BorderBrush = System.Windows.Media.Brushes.SkyBlue;
				}

				button.BorderThickness = new Thickness( 3.0 );
			}
			else
			{
				button.BorderBrush = System.Windows.Media.Brushes.Gray;
				button.BorderThickness = new Thickness( 0.5 );
			}
		}

		private void UpdateManualCamera()
		{
			if ( Director.isOverridden && ( normalizedCar != null ) )
			{
				IRSDK.targetCamEnabled = true;
				IRSDK.targetCamFastSwitchEnabled = true;
				IRSDK.targetCamSlowSwitchEnabled = false;
				IRSDK.targetCamCarIdx = normalizedCar.carIdx;
				IRSDK.targetCamGroupNumber = Director.GetCamGroupNumber( normalizedCar, cameraType );
				IRSDK.targetCamReason = "Manual override.";
			}
		}

		private void ControlPanel_Camera_Intro_Button_Click( object sender, RoutedEventArgs e )
		{
			if ( Director.isOverridden && ( cameraType == SettingsDirector.CameraType.Intro ) )
			{
				Director.isOverridden = false;
			}
			else
			{
				cameraType = SettingsDirector.CameraType.Intro;

				UpdateManualCamera();
			}
		}

		private void ControlPanel_Camera_Inside_Button_Click( object sender, RoutedEventArgs e )
		{
			if ( Director.isOverridden && ( cameraType == SettingsDirector.CameraType.Inside ) )
			{
				Director.isOverridden = false;
			}
			else
			{
				cameraType = SettingsDirector.CameraType.Inside;

				UpdateManualCamera();
			}
		}

		private void ControlPanel_Camera_Close_Button_Click( object sender, RoutedEventArgs e )
		{
			if ( Director.isOverridden && ( cameraType == SettingsDirector.CameraType.Close ) )
			{
				Director.isOverridden = false;
			}
			else
			{
				cameraType = SettingsDirector.CameraType.Close;

				UpdateManualCamera();
			}
		}

		private void ControlPanel_Camera_Medium_Button_Click( object sender, RoutedEventArgs e )
		{
			if ( Director.isOverridden && ( cameraType == SettingsDirector.CameraType.Medium ) )
			{
				Director.isOverridden = false;
			}
			else
			{
				cameraType = SettingsDirector.CameraType.Medium;

				UpdateManualCamera();
			}
		}

		private void ControlPanel_Camera_Far_Button_Click( object sender, RoutedEventArgs e )
		{
			if ( Director.isOverridden && ( cameraType == SettingsDirector.CameraType.Far ) )
			{
				Director.isOverridden = false;
			}
			else
			{
				cameraType = SettingsDirector.CameraType.Far;

				UpdateManualCamera();
			}
		}

		private void ControlPanel_Camera_VeryFar_Button_Click( object sender, RoutedEventArgs e )
		{
			if ( Director.isOverridden && ( cameraType == SettingsDirector.CameraType.VeryFar ) )
			{
				Director.isOverridden = false;
			}
			else
			{
				cameraType = SettingsDirector.CameraType.VeryFar;

				UpdateManualCamera();
			}
		}

		private void ControlPanel_Camera_AutoCam_Button_Click( object sender, RoutedEventArgs e )
		{
			if ( Director.isOverridden && ( cameraType == SettingsDirector.CameraType.AutoCam ) )
			{
				Director.isOverridden = false;
			}
			else
			{
				cameraType = SettingsDirector.CameraType.AutoCam;

				UpdateManualCamera();
			}
		}

		private void ControlPanel_Grid_Button_Click( object sender, RoutedEventArgs e )
		{
			var button = (Button) sender;

			int carIdx = ( button.Name == "PACE" ) ? 0 : int.Parse( button.Name[ 1.. ] );

			if ( Director.isOverridden && ( normalizedCar != null ) && ( normalizedCar.carIdx == carIdx ) )
			{
				Director.isOverridden = false;
			}
			else
			{
				Director.isOverridden = true;

				normalizedCar = IRSDK.normalizedData.FindNormalizedCarByCarIdx( carIdx );

				UpdateManualCamera();
			}
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
				var overridden = Director_Cameras_Practice_Override.IsChecked ?? false;

				if ( Settings.directorLocal.camerasPractice_Overridden != overridden )
				{
					Settings.directorLocal.camerasPractice_Overridden = overridden;

					Initialize();
				}
				else
				{
					var director = Settings.directorLocal.camerasPractice_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.camerasPractice = Director_Cameras_Practice.Text;
				}

				overridden = Director_Cameras_Qualifying_Override.IsChecked ?? false;

				if ( Settings.directorLocal.camerasQualifying_Overridden != overridden )
				{
					Settings.directorLocal.camerasQualifying_Overridden = overridden;

					Initialize();
				}
				else
				{
					var director = Settings.directorLocal.camerasQualifying_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.camerasQualifying = Director_Cameras_Qualifying.Text;
				}

				overridden = Director_Cameras_Intro_Override.IsChecked ?? false;

				if ( Settings.directorLocal.camerasIntro_Overridden != overridden )
				{
					Settings.directorLocal.camerasIntro_Overridden = overridden;

					Initialize();
				}
				else
				{
					var director = Settings.directorLocal.camerasIntro_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.camerasIntro = Director_Cameras_Intro.Text;
				}

				overridden = Director_Cameras_Inside_Override.IsChecked ?? false;

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

				overridden = Director_SwitchDelay_Director_Override.IsChecked ?? false;

				if ( Settings.directorLocal.switchDelayDirector_Overridden != overridden )
				{
					Settings.directorLocal.switchDelayDirector_Overridden = overridden;

					Initialize();
				}
				else
				{
					var director = Settings.directorLocal.switchDelayDirector_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.switchDelayDirector = Director_SwitchDelay_Director.Value;
				}

				overridden = Director_SwitchDelay_iRacing_Override.IsChecked ?? false;

				if ( Settings.directorLocal.switchDelayIracing_Overridden != overridden )
				{
					Settings.directorLocal.switchDelayIracing_Overridden = overridden;

					Initialize();
				}
				else
				{
					var director = Settings.directorLocal.switchDelayIracing_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.switchDelayIracing = Director_SwitchDelay_iRacing.Value;
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

					director.switchDelayRadioChatter = Director_SwitchDelay_RadioChatter.Value;
				}

				overridden = Director_SwitchDelay_NotInRace_Override.IsChecked ?? false;

				if ( Settings.directorLocal.switchDelayNotInRace_Overridden != overridden )
				{
					Settings.directorLocal.switchDelayNotInRace_Overridden = overridden;

					Initialize();
				}
				else
				{
					var director = Settings.directorLocal.switchDelayNotInRace_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.switchDelayNotInRace = Director_SwitchDelay_NotInRace.Value;
				}

				overridden = Director_Heat_CarLength_Override.IsChecked ?? false;

				if ( Settings.directorLocal.heatCarLength_Overridden != overridden )
				{
					Settings.directorLocal.heatCarLength_Overridden = overridden;

					Initialize();
				}
				else
				{
					var director = Settings.directorLocal.heatCarLength_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.heatCarLength = Director_Heat_CarLength.Value;
				}

				overridden = Director_Heat_Falloff_Override.IsChecked ?? false;

				if ( Settings.directorLocal.heatFalloff_Overridden != overridden )
				{
					Settings.directorLocal.heatFalloff_Overridden = overridden;

					Initialize();
				}
				else
				{
					var director = Settings.directorLocal.heatFalloff_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.heatFalloff = Director_Heat_Falloff.Value;
				}

				overridden = Director_Heat_Bias_Override.IsChecked ?? false;

				if ( Settings.directorLocal.heatBias_Overridden != overridden )
				{
					Settings.directorLocal.heatBias_Overridden = overridden;

					Initialize();
				}
				else
				{
					var director = Settings.directorLocal.heatBias_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.heatBias = (float) Director_Heat_Bias.Value;
				}

				overridden = Director_PreferredCar_Number_Override.IsChecked ?? false;

				if ( Settings.directorLocal.preferredCarNumber_Overridden != overridden )
				{
					Settings.directorLocal.preferredCarNumber_Overridden = overridden;

					Initialize();
				}
				else
				{
					var director = Settings.directorLocal.preferredCarNumber_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.preferredCarNumber = Director_PreferredCar_Number.Text;
				}

				overridden = Director_PreferredCar_LockOnEnabled_Override.IsChecked ?? false;

				if ( Settings.directorLocal.preferredCarLockOnEnabled_Overridden != overridden )
				{
					Settings.directorLocal.preferredCarLockOnEnabled_Overridden = overridden;

					Initialize();
				}
				else
				{
					var director = Settings.directorLocal.preferredCarLockOnEnabled_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.preferredCarLockOnEnabled = Director_PreferredCar_LockOnEnabled.IsChecked ?? false;
				}

				overridden = Director_PreferredCar_LockOnMinimumHeat_Override.IsChecked ?? false;

				if ( Settings.directorLocal.preferredCarLockOnMinimumHeat_Overridden != overridden )
				{
					Settings.directorLocal.preferredCarLockOnMinimumHeat_Overridden = overridden;

					Initialize();
				}
				else
				{
					var director = Settings.directorLocal.preferredCarLockOnMinimumHeat_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.preferredCarLockOnMinimumHeat = Director_PreferredCar_LockOnMinimumHeat.Value;
				}

				Settings.SaveDirector();
			}
		}

		private void Director_Rules_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overridden = Director_Rules_Override.IsChecked ?? false;

				if ( Settings.directorLocal.rules_Overridden != overridden )
				{
					Settings.directorLocal.rules_Overridden = overridden;

					Initialize();
				}
				else
				{
					var director = Settings.directorLocal.rules_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.rule1_Enabled = Director_Rules_Rule1_Enabled.IsChecked ?? false;
					director.rule1_Camera = (SettingsDirector.CameraType) Director_Rules_Rule1_Camera.SelectedItem;

					director.rule2_Enabled = Director_Rules_Rule2_Enabled.IsChecked ?? false;
					director.rule2_Camera = (SettingsDirector.CameraType) Director_Rules_Rule2_Camera.SelectedItem;

					director.rule3_Enabled = Director_Rules_Rule3_Enabled.IsChecked ?? false;
					director.rule3_Camera = (SettingsDirector.CameraType) Director_Rules_Rule3_Camera.SelectedItem;

					director.rule4_Enabled = Director_Rules_Rule4_Enabled.IsChecked ?? false;
					director.rule4_Camera = (SettingsDirector.CameraType) Director_Rules_Rule4_Camera.SelectedItem;

					director.rule5_Enabled = Director_Rules_Rule5_Enabled.IsChecked ?? false;
					director.rule5_Camera = (SettingsDirector.CameraType) Director_Rules_Rule5_Camera.SelectedItem;

					director.rule6_Enabled = Director_Rules_Rule6_Enabled.IsChecked ?? false;
					director.rule6_Camera = (SettingsDirector.CameraType) Director_Rules_Rule6_Camera.SelectedItem;

					director.rule7_Enabled = Director_Rules_Rule7_Enabled.IsChecked ?? false;
					director.rule7_Camera = (SettingsDirector.CameraType) Director_Rules_Rule7_Camera.SelectedItem;

					director.rule8_Enabled = Director_Rules_Rule8_Enabled.IsChecked ?? false;
					director.rule8_Camera = (SettingsDirector.CameraType) Director_Rules_Rule8_Camera.SelectedItem;

					director.rule9_Enabled = Director_Rules_Rule9_Enabled.IsChecked ?? false;
					director.rule9_Camera = (SettingsDirector.CameraType) Director_Rules_Rule9_Camera.SelectedItem;

					director.rule10_Enabled = Director_Rules_Rule10_Enabled.IsChecked ?? false;
					director.rule10_Camera = (SettingsDirector.CameraType) Director_Rules_Rule10_Camera.SelectedItem;

					director.rule11_Enabled = Director_Rules_Rule11_Enabled.IsChecked ?? false;
					director.rule11_Camera = (SettingsDirector.CameraType) Director_Rules_Rule11_Camera.SelectedItem;

					director.rule12_Enabled = Director_Rules_Rule12_Enabled.IsChecked ?? false;
					director.rule12_Camera = (SettingsDirector.CameraType) Director_Rules_Rule12_Camera.SelectedItem;

					director.rule13_Enabled = Director_Rules_Rule13_Enabled.IsChecked ?? false;
					director.rule13_Camera = (SettingsDirector.CameraType) Director_Rules_Rule13_Camera.SelectedItem;
				}

				Settings.SaveDirector();
			}
		}

		private void Director_AutoCam_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overridden = Director_AutoCam_Override.IsChecked ?? false;

				if ( Settings.directorLocal.autoCam_Overridden != overridden )
				{
					Settings.directorLocal.autoCam_Overridden = overridden;

					Initialize();
				}
				else
				{
					var director = Settings.directorLocal.autoCam_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.autoCamInsideMinimum = Director_AutoCam_Inside_Minimum.Value;
					director.autoCamInsideMaximum = Director_AutoCam_Inside_Maximum.Value;
					director.autoCamCloseMaximum = Director_AutoCam_Close_Maximum.Value;
					director.autoCamMediumMaximum = Director_AutoCam_Medium_Maximum.Value;
					director.autoCamFarMaximum = Director_AutoCam_Far_Maximum.Value;
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

				IncidentPlayback.saveIncidentsQueued = true;

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

				IncidentPlayback.saveIncidentsQueued = true;

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

				IncidentPlayback.saveIncidentsQueued = true;
			}
		}

		private void Incidents_ListView_MouseDoubleClick( object sender, RoutedEventArgs e )
		{
			if ( ( (FrameworkElement) e.OriginalSource ).DataContext is IncidentData item )
			{
				IRSDK.targetCamEnabled = true;
				IRSDK.targetCamFastSwitchEnabled = true;
				IRSDK.targetCamCarIdx = item.CarIdx;
				IRSDK.targetCamGroupNumber = IRSDK.GetCamGroupNumber( Settings.editor.editorIncidentsEditCameras );

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

			IncidentPlayback.Start();
		}

		private void Incidents_Clear_Button_Click( object sender, EventArgs e )
		{
			if ( Incidents_ListView.Items.Count > 0 )
			{
				if ( MessageBox.Show( this, "Are you sure you want to clear all incidents?", "Are You Sure?", MessageBoxButton.OKCancel, MessageBoxImage.Question ) == MessageBoxResult.Cancel )
				{
					return;
				}
			}

			IncidentPlayback.Clear();
		}

		// subtitles

		private void Subtitles_Text_TextChanged( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var textBox = (TextBox) sender;

				var listViewItem = FindVisualParent<ListViewItem>( textBox );

				var item = (SubtitleData) Subtitles_ListView.ItemContainerGenerator.ItemFromContainer( listViewItem );

				item.Text = textBox.Text;

				SubtitlePlayback.saveSubtitlesQueued = true;
			}
		}

		private void Subtitles_Ignore_Click( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var checkBox = (CheckBox) sender;

				var listViewItem = FindVisualParent<ListViewItem>( checkBox );

				var item = (SubtitleData) Subtitles_ListView.ItemContainerGenerator.ItemFromContainer( listViewItem );

				item.Ignore = checkBox.IsChecked ?? false;

				SubtitlePlayback.saveSubtitlesQueued = true;
			}
		}

		private void Subtitles_ListView_MouseDoubleClick( object sender, RoutedEventArgs e )
		{
			if ( ( (FrameworkElement) e.OriginalSource ).DataContext is SubtitleData item )
			{
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

		private void Subtitles_Import_Button_Click( object sender, EventArgs e )
		{
			if ( !IRSDK.isConnected )
			{
				MessageBox.Show( this, "iRacing is not running.", "Not Yet", MessageBoxButton.OK, MessageBoxImage.Exclamation );

				return;
			}

			if ( !IRSDK.normalizedSession.isReplay )
			{
				MessageBox.Show( this, "Sorry, the subtitles system does not work outside of replays.", "Not In Replay", MessageBoxButton.OK, MessageBoxImage.Exclamation );

				return;
			}

			if ( Subtitles_ListView.Items.Count > 0 )
			{
				if ( MessageBox.Show( this, "Are you sure you want to clear all subtitles and import them from iRacing-STT-VR?", "Are You Sure?", MessageBoxButton.OKCancel, MessageBoxImage.Question ) == MessageBoxResult.Cancel )
				{
					return;
				}
			}

			if ( !SubtitlePlayback.Import() )
			{
				MessageBox.Show( this, "Sorry, we were not able to find any iRacing-STT-VR chat log matching this session ID.", "Chat Log Not Found", MessageBoxButton.OK, MessageBoxImage.Error );
			}
		}

		private void Subtitles_Clear_Button_Click( object sender, EventArgs e )
		{
			if ( Subtitles_ListView.Items.Count > 0 )
			{
				if ( MessageBox.Show( this, "Are you sure you want to clear all subtitles?", "Are You Sure?", MessageBoxButton.OKCancel, MessageBoxImage.Question ) == MessageBoxResult.Cancel )
				{
					return;
				}
			}

			SubtitlePlayback.Clear();
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

				overridden = Overlay_Leaderboard_FirstSlotPosition_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.leaderboardFirstSlotPosition_Overridden != overridden )
				{
					Settings.overlayLocal.leaderboardFirstSlotPosition_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.leaderboardFirstSlotPosition_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.leaderboardFirstSlotPosition = new Vector2( Overlay_Leaderboard_FirstSlotPosition_X.Value, Overlay_Leaderboard_FirstSlotPosition_Y.Value );
				}

				overridden = Overlay_Leaderboard_SlotCount_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.leaderboardSlotCount_Overridden != overridden )
				{
					Settings.overlayLocal.leaderboardSlotCount_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.leaderboardSlotCount_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.leaderboardSlotCount = (int) Overlay_Leaderboard_SlotCount.Value;
				}

				overridden = Overlay_Leaderboard_SlotSpacing_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.leaderboardSlotSpacing_Overridden != overridden )
				{
					Settings.overlayLocal.leaderboardSlotSpacing_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.leaderboardSlotSpacing_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.leaderboardSlotSpacing = new Vector2( Overlay_Leaderboard_SlotSpacing_X.Value, Overlay_Leaderboard_SlotSpacing_Y.Value );
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
				var overridden = Overlay_Subtitle_Enable_Override.IsChecked ?? false;

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

				overridden = Overlay_Subtitle_Position_Override.IsChecked ?? false;

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

				overridden = Overlay_Subtitle_MaxSize_Override.IsChecked ?? false;

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

				overridden = Overlay_Subtitle_BackgroundColor_Override.IsChecked ?? false;

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

				overridden = Overlay_Subtitle_TextPadding_Override.IsChecked ?? false;

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

		private void Overlay_CarNumber_ColorA_Palette_Click( object sender, EventArgs e )
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

		private void Overlay_CarNumber_ColorB_Palette_Click( object sender, EventArgs e )
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

		private void Overlay_CarNumber_ColorC_Palette_Click( object sender, EventArgs e )
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

		private void Overlay_Intro_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overridden = Overlay_Intro_Enable_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.introEnabled_Overridden != overridden )
				{
					Settings.overlayLocal.introEnabled_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.introEnabled_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.introEnabled = Overlay_Intro_Enable.IsChecked ?? false;
				}

				overridden = Overlay_Intro_StartTime_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.introStartTime_Overridden != overridden )
				{
					Settings.overlayLocal.introStartTime_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.introStartTime_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.introStartTime = Overlay_Intro_StartTime.Value;
				}

				overridden = Overlay_Intro_RowInterval_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.introRowInterval_Overridden != overridden )
				{
					Settings.overlayLocal.introRowInterval_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.introRowInterval_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.introRowInterval = Overlay_Intro_RowInterval.Value;
				}

				overridden = Overlay_Intro_AnimationNumber_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.introAnimationNumber_Overridden != overridden )
				{
					Settings.overlayLocal.introAnimationNumber_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.introAnimationNumber_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.introAnimationNumber = animationOptions[ (string) Overlay_Intro_AnimationNumber.SelectedItem ];
				}

				overridden = Overlay_Intro_AnimationSpeed_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.introAnimationSpeed_Overridden != overridden )
				{
					Settings.overlayLocal.introAnimationSpeed_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.introAnimationSpeed_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.introAnimationSpeed = Overlay_Intro_AnimationSpeed.Value;
				}

				overridden = Overlay_Intro_LeftPosition_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.introLeftPosition_Overridden != overridden )
				{
					Settings.overlayLocal.introLeftPosition_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.introLeftPosition_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.introLeftPosition = new Vector2( Overlay_Intro_LeftPosition_X.Value, Overlay_Intro_LeftPosition_Y.Value );
				}

				overridden = Overlay_Intro_LeftScale_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.introLeftScale_Overridden != overridden )
				{
					Settings.overlayLocal.introLeftScale_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.introLeftScale_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.introLeftScale = Overlay_Intro_LeftScale.Value;
				}

				overridden = Overlay_Intro_RightPosition_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.introRightPosition_Overridden != overridden )
				{
					Settings.overlayLocal.introRightPosition_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.introRightPosition_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.introRightPosition = new Vector2( Overlay_Intro_RightPosition_X.Value, Overlay_Intro_RightPosition_Y.Value );
				}

				overridden = Overlay_Intro_RightScale_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.introRightScale_Overridden != overridden )
				{
					Settings.overlayLocal.introRightScale_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlayLocal.introRightScale_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.introRightScale = Overlay_Intro_RightScale.Value;
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
				Settings.editor.iracingGeneralCommandRateLimit = iRacing_General_CommandRateLimit.Value;

				Settings.editor.iracingCustomPaintsDirectory = iRacing_CustomPaints_Directory.Text;

				Settings.editor.iracingDriverNamesSuffixes = iRacing_DriverNames_Suffixes.Text;
				Settings.editor.iracingDriverNameCapitalizationOption = capitalizationOptions[ (string) iRacing_DriverNames_CapitalizationOption.SelectedItem ];

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
				Settings.editor.editorMousePositioningSpeedNormal = Editor_Mouse_PositioningSpeedNormal.Value;
				Settings.editor.editorMousePositioningSpeedFast = Editor_Mouse_PositioningSpeedFast.Value;
				Settings.editor.editorMousePositioningSpeedSlow = Editor_Mouse_PositioningSpeedSlow.Value;

				Settings.editor.editorIncidentsScenicCameras = Editor_Incidents_ScenicCameras.Text;
				Settings.editor.editorIncidentsEditCameras = Editor_Incidents_EditCameras.Text;
				Settings.editor.editorIncidentsOverlapMergeTime = Editor_Incidents_OverlapMergeTime.Value;
				Settings.editor.editorIncidentsTimeout = Editor_Incidents_Timeout.Value;

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
