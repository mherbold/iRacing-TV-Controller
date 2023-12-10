
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

using Dsafa.WpfColorPicker;

using iRacingTVController.Windows;

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

		public const float UpdateIntervalTime = 1f / 15f;

		public const string StatusDisconnectedImageFileName = "Assets\\status-disconnected.png";
		public const string StatusConnectedImageFileName = "Assets\\status-connected.png";

		public readonly BitmapImage statusDisconnectedBitmapImage = new( new Uri( $"pack://application:,,,/{StatusDisconnectedImageFileName}" ) );
		public readonly BitmapImage statusConnectedBitmapImage = new( new Uri( $"pack://application:,,,/{StatusConnectedImageFileName}" ) );

		public static MainWindow Instance { get; private set; }

		public int initializing = 0;
		public float updateTimeRemaining = 0;

		public SettingsDirector.CameraType cameraType = SettingsDirector.CameraType.AutoCam;
		public NormalizedCar? normalizedCar = null;

		public ControlPanelButton[] controlPanelButton = new ControlPanelButton[ NormalizedData.MaxNumCars ];

		public bool masterOn;
		public bool raceStatusOn;
		public bool leaderboardOn;
		public bool trackMapOn;
		public bool pitLaneOn;
		public bool startLightsOn;
		public bool voiceOfOn;
		public bool chyronOn;
		public bool subtitlesOn;
		public bool introOn;

		public bool[] customLayerOn = new bool[ 6 ];

		public SortedDictionary<string, string> fontOptions;
		public SortedDictionary<string, int> patternOptions = new();
		public SortedDictionary<string, int> slantOptions = new();
		public SortedDictionary<string, int> leaderboardMultiClassOffsetTypeOptions = new();
		public SortedDictionary<string, int> inAnimationOptions = new();
		public SortedDictionary<string, int> outAnimationOptions = new();
		public SortedDictionary<string, int> formatOptions = new();
		public SortedDictionary<string, int> capitalizationOptions = new();
		public SortedDictionary<string, Guid> controllerOptions = new();
		public SortedDictionary<string, string> audioCaptureDeviceOptions = new();
		public SortedDictionary<string, string> audioRenderDeviceOptions = new();

		static MainWindow()
		{
			Instance = new MainWindow();
		}

		private MainWindow()
		{
			Instance = this;

			initializing++;

			InitializeComponent();

			LogFile.Write( $"Creating window {Title}...\r\n" );

			fontOptions = FontPaths.FindAll();

			initializing--;
		}

		// initialize

		public void Initialize()
		{
			try
			{
				initializing++;

				// control panel

				for ( int i = 0; i < 8; i++ )
				{
					for ( int j = 0; j < 8; j++ )
					{
						var buttonIndex = ( j + i * 8 );

						var grid = new Grid();

						if ( buttonIndex != ( NormalizedData.MaxNumCars - 1 ) )
						{
							grid.Visibility = Visibility.Hidden;
						}

						grid.SetValue( Grid.RowProperty, i );
						grid.SetValue( Grid.ColumnProperty, j );

						if ( buttonIndex <= ( NormalizedData.MaxNumCars - 1 ) )
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
							label1.Padding = new Thickness( 6, 3, 6, 3 );

							grid.Children.Add( label1 );

							var label2 = new Label();

							label2.HorizontalAlignment = HorizontalAlignment.Right;
							label2.VerticalAlignment = VerticalAlignment.Top;
							label2.FontSize = 10;
							label2.Foreground = System.Windows.Media.Brushes.Gray;
							label2.IsHitTestVisible = false;
							label2.Padding = new Thickness( 6, 3, 6, 3 );

							grid.Children.Add( label2 );

							var label3 = new Label();

							label3.HorizontalAlignment = HorizontalAlignment.Left;
							label3.VerticalAlignment = VerticalAlignment.Bottom;
							label3.FontSize = 10;
							label3.Foreground = System.Windows.Media.Brushes.Gray;
							label3.IsHitTestVisible = false;
							label3.Padding = new Thickness( 6, 3, 6, 3 );

							grid.Children.Add( label3 );

							var label4 = new Label();

							label4.HorizontalAlignment = HorizontalAlignment.Right;
							label4.VerticalAlignment = VerticalAlignment.Bottom;
							label4.FontSize = 10;
							label4.Foreground = System.Windows.Media.Brushes.Gray;
							label4.IsHitTestVisible = false;
							label4.Padding = new Thickness( 6, 3, 6, 3 );

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

							if ( buttonIndex == ( NormalizedData.MaxNumCars - 1 ) )
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
					Director_Rules_Rule14_Camera.Items.Add( item );
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

				foreach ( var fontOption in fontOptions )
				{
					Overlay_Font_FontA_Name.Items.Add( fontOption.Key );
					Overlay_Font_FontB_Name.Items.Add( fontOption.Key );
					Overlay_Font_FontC_Name.Items.Add( fontOption.Key );
					Overlay_Font_FontD_Name.Items.Add( fontOption.Key );
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

				leaderboardMultiClassOffsetTypeOptions.Add( "From Bottom Left", 0 );
				leaderboardMultiClassOffsetTypeOptions.Add( "From Top Left", 1 );

				foreach ( var item in leaderboardMultiClassOffsetTypeOptions )
				{
					Overlay_Leaderboard_MultiClassOffset_Type.Items.Add( item.Key );
				}

				inAnimationOptions.Add( "Static (Use For Editing)", 0 );
				inAnimationOptions.Add( "Fade", 1 );
				inAnimationOptions.Add( "Drop Down", 2 );
				inAnimationOptions.Add( "Drop Up", 3 );
				inAnimationOptions.Add( "Slide Left", 4 );
				inAnimationOptions.Add( "Slide Right", 5 );
				inAnimationOptions.Add( "Leave", 6 );
				inAnimationOptions.Add( "Approach", 7 );
				inAnimationOptions.Add( "Conveyor", 8 );

				foreach ( var item in inAnimationOptions )
				{
					Overlay_Intro_LeftInAnimationNumber.Items.Add( item.Key );
					Overlay_Intro_RightInAnimationNumber.Items.Add( item.Key );
				}

				outAnimationOptions.Add( "Static (Use For Editing)", 0 );
				outAnimationOptions.Add( "Fade", 1 );
				outAnimationOptions.Add( "Drop Down", 2 );
				outAnimationOptions.Add( "Drop Up", 3 );
				outAnimationOptions.Add( "Slide Left", 4 );
				outAnimationOptions.Add( "Slide Right", 5 );
				outAnimationOptions.Add( "Leave", 6 );
				outAnimationOptions.Add( "Approach", 7 );
				outAnimationOptions.Add( "Conveyor", 8 );
				outAnimationOptions.Add( "Fall Down", 9 );

				foreach ( var item in outAnimationOptions )
				{
					Overlay_Intro_LeftOutAnimationNumber.Items.Add( item.Key );
					Overlay_Intro_RightOutAnimationNumber.Items.Add( item.Key );
				}

				// editor

				formatOptions.Add( "Lastname", 0 );
				formatOptions.Add( "Las", 1 );
				formatOptions.Add( "F Las", 2 );
				formatOptions.Add( "Firstname", 3 );
				formatOptions.Add( "Fir", 4 );
				formatOptions.Add( "Fir L", 5 );
				formatOptions.Add( "F. Lastname", 6 );
				formatOptions.Add( "Firstname L.", 7 );
				formatOptions.Add( "Lastname, F", 8 );
				formatOptions.Add( "Firstname Lastname", 9 );

				foreach ( var item in formatOptions )
				{
					iRacing_DriverNames_FormatOption.Items.Add( item.Key );
				}

				capitalizationOptions.Add( "Leave Names Alone", 0 );
				capitalizationOptions.Add( "Change From All Uppercase To Uppercase First Letter Only", 1 );
				capitalizationOptions.Add( "Change To All Uppercase Always", 2 );

				foreach ( var item in capitalizationOptions )
				{
					iRacing_DriverNames_CapitalizationOption.Items.Add( item.Key );
				}

				controllerOptions = Controller.FindAll();

				foreach ( var item in controllerOptions )
				{
					Editor_SteamVr_ControllerOption.Items.Add( item.Key );
				}

				audioCaptureDeviceOptions = SpeechToText.FindAll();

				foreach ( var item in audioCaptureDeviceOptions )
				{
					Editor_SpeechToText_AudioCaptureDeviceOption.Items.Add( item.Key );
				}

				audioRenderDeviceOptions = PushToTalk.FindAll();

				foreach ( var item in audioRenderDeviceOptions )
				{
					Editor_PushToTalk_AudioRenderDeviceOption.Items.Add( item.Key );
				}

				//

				initializing--;

				//

				Update();
			}
			catch ( Exception exception )
			{
				LogFile.WriteException( exception );

				throw;
			}
		}

		// update

		public void Update( TextBox textBoxInput, string text, CheckBox overrideCheckBox, bool isGlobal, bool isOverridden, Button? extraButton = null )
		{
			textBoxInput.IsEnabled = isGlobal || isOverridden;
			textBoxInput.Text = text;
			overrideCheckBox.IsEnabled = !isGlobal;
			overrideCheckBox.IsChecked = isOverridden;

			if ( extraButton != null )
			{
				extraButton.IsEnabled = isGlobal || isOverridden;
			}
		}

		public void Update( Decimal decimalInput, float value, CheckBox overrideCheckBox, bool isGlobal, bool isOverridden )
		{
			decimalInput.IsEnabled = isGlobal || isOverridden;
			decimalInput.Value = value;
			overrideCheckBox.IsEnabled = !isGlobal;
			overrideCheckBox.IsChecked = isOverridden;
		}

		public void Update( Slider sliderInput, float value, CheckBox overrideCheckBox, bool isGlobal, bool isOverridden )
		{
			sliderInput.IsEnabled = isGlobal || isOverridden;
			sliderInput.Value = value;
			overrideCheckBox.IsEnabled = !isGlobal;
			overrideCheckBox.IsChecked = isOverridden;
		}

		public void Update( CheckBox checkBoxInput, bool value, CheckBox overrideCheckBox, bool isGlobal, bool isOverridden )
		{
			checkBoxInput.IsEnabled = isGlobal || isOverridden;
			checkBoxInput.IsChecked = value;
			overrideCheckBox.IsEnabled = !isGlobal;
			overrideCheckBox.IsChecked = isOverridden;
		}

		public void Update( RadioButton radioButtonInput, bool value, CheckBox overrideCheckBox, bool isGlobal, bool isOverridden )
		{
			radioButtonInput.IsEnabled = isGlobal || isOverridden;
			radioButtonInput.IsChecked = value;
			overrideCheckBox.IsEnabled = !isGlobal;
			overrideCheckBox.IsChecked = isOverridden;
		}

		public void Update( ComboBox comboBoxInput, object value, CheckBox overrideCheckBox, bool isGlobal, bool isOverridden )
		{
			comboBoxInput.IsEnabled = isGlobal || isOverridden;
			comboBoxInput.SelectedItem = value;
			overrideCheckBox.IsEnabled = !isGlobal;
			overrideCheckBox.IsChecked = isOverridden;
		}

		public void Update( Number numberInput, int value, CheckBox overrideCheckBox, bool isGlobal, bool isOverridden )
		{
			numberInput.IsEnabled = isGlobal || isOverridden;
			numberInput.Value = value;
			overrideCheckBox.IsEnabled = !isGlobal;
			overrideCheckBox.IsChecked = isOverridden;
		}

		public void Update( Number numberInput1, Number numberInput2, Vector2Int value, CheckBox overrideCheckBox, bool isGlobal, bool isOverridden )
		{
			numberInput1.IsEnabled = isGlobal || isOverridden;
			numberInput2.IsEnabled = isGlobal || isOverridden;
			numberInput1.Value = value.x;
			numberInput2.Value = value.y;
			overrideCheckBox.IsEnabled = !isGlobal;
			overrideCheckBox.IsChecked = isOverridden;
		}

		public void Update( Number numberInput1, Number numberInput2, Vector2 value, CheckBox overrideCheckBox, bool isGlobal, bool isOverridden )
		{
			numberInput1.IsEnabled = isGlobal || isOverridden;
			numberInput2.IsEnabled = isGlobal || isOverridden;
			numberInput1.Value = (int) value.x;
			numberInput2.Value = (int) value.y;
			overrideCheckBox.IsEnabled = !isGlobal;
			overrideCheckBox.IsChecked = isOverridden;
		}

		public void Update( Decimal decimalInput1, Decimal decimalInput2, Vector2 value, CheckBox overrideCheckBox, bool isGlobal, bool isOverridden )
		{
			decimalInput1.IsEnabled = isGlobal || isOverridden;
			decimalInput2.IsEnabled = isGlobal || isOverridden;
			decimalInput1.Value = value.x;
			decimalInput2.Value = value.y;
			overrideCheckBox.IsEnabled = !isGlobal;
			overrideCheckBox.IsChecked = isOverridden;
		}

		public void Update( Decimal decimalInput1, Decimal decimalInput2, Decimal decimalInput3, Decimal decimalInput4, Color value, CheckBox overrideCheckBox, bool isGlobal, bool isOverridden, Button? extraButton = null )
		{
			decimalInput1.IsEnabled = isGlobal || isOverridden;
			decimalInput2.IsEnabled = isGlobal || isOverridden;
			decimalInput3.IsEnabled = isGlobal || isOverridden;
			decimalInput4.IsEnabled = isGlobal || isOverridden;
			decimalInput1.Value = value.r;
			decimalInput2.Value = value.g;
			decimalInput3.Value = value.b;
			decimalInput4.Value = value.a;
			overrideCheckBox.IsEnabled = !isGlobal;
			overrideCheckBox.IsChecked = isOverridden;

			if ( extraButton != null )
			{
				extraButton.IsEnabled = isGlobal || isOverridden;
			}
		}

		public void Update( Decimal decimalInput1, Decimal decimalInput2, Decimal decimalInput3, Decimal decimalInput4, Vector4 value, CheckBox overrideCheckBox, bool isGlobal, bool isOverridden )
		{
			decimalInput1.IsEnabled = isGlobal || isOverridden;
			decimalInput2.IsEnabled = isGlobal || isOverridden;
			decimalInput3.IsEnabled = isGlobal || isOverridden;
			decimalInput4.IsEnabled = isGlobal || isOverridden;
			decimalInput1.Value = value.x;
			decimalInput2.Value = value.y;
			decimalInput3.Value = value.z;
			decimalInput4.Value = value.w;
			overrideCheckBox.IsEnabled = !isGlobal;
			overrideCheckBox.IsChecked = isOverridden;
		}

		public void Update()
		{
			initializing++;

			Settings.UpdateCombinedDirector();
			Settings.UpdateCombinedOverlay();

			var directorIsGlobal = Settings.directorLocal.filePath == Settings.relativeGlobalDirectorSettingsFilePath;
			var overlayIsGlobal = Settings.overlayLocal.filePath == Settings.relativeGlobalOverlaySettingsFilePath;

			// control panel

			ControlPanel_Master_Button.IsChecked = masterOn = true;
			ControlPanel_RaceStatus_Button.IsChecked = raceStatusOn = Settings.overlay.raceStatusEnabled;
			ControlPanel_Leaderboard_Button.IsChecked = leaderboardOn = Settings.overlay.leaderboardEnabled;
			ControlPanel_TrackMap_Button.IsChecked = trackMapOn = Settings.overlay.trackMapEnabled;
			ControlPanel_PitLane_Button.IsChecked = pitLaneOn = Settings.overlay.pitLaneEnabled;
			ControlPanel_StartLights_Button.IsChecked = startLightsOn = Settings.overlay.startLightsEnabled;
			ControlPanel_VoiceOf_Button.IsChecked = voiceOfOn = Settings.overlay.voiceOfEnabled;
			ControlPanel_Chyron_Button.IsChecked = chyronOn = Settings.overlay.chyronEnabled;
			ControlPanel_Subtitles_Button.IsChecked = subtitlesOn = Settings.overlay.subtitleEnabled;
			ControlPanel_Intro_Button.IsChecked = introOn = Settings.overlay.introEnabled;

			ControlPanel_C1_Button.IsChecked = customLayerOn[ 0 ] = Settings.overlay.imageSettingsDataDictionary[ "CustomLayer1" ].imageType != SettingsImage.ImageType.None;
			ControlPanel_C2_Button.IsChecked = customLayerOn[ 1 ] = Settings.overlay.imageSettingsDataDictionary[ "CustomLayer2" ].imageType != SettingsImage.ImageType.None;
			ControlPanel_C3_Button.IsChecked = customLayerOn[ 2 ] = Settings.overlay.imageSettingsDataDictionary[ "CustomLayer3" ].imageType != SettingsImage.ImageType.None;
			ControlPanel_C4_Button.IsChecked = customLayerOn[ 3 ] = Settings.overlay.imageSettingsDataDictionary[ "CustomLayer4" ].imageType != SettingsImage.ImageType.None;
			ControlPanel_C5_Button.IsChecked = customLayerOn[ 4 ] = Settings.overlay.imageSettingsDataDictionary[ "CustomLayer5" ].imageType != SettingsImage.ImageType.None;
			ControlPanel_C6_Button.IsChecked = customLayerOn[ 5 ] = Settings.overlay.imageSettingsDataDictionary[ "CustomLayer6" ].imageType != SettingsImage.ImageType.None;

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

			Update( Director_Cameras_Practice, Settings.director.camerasPractice, Director_Cameras_Practice_Override, directorIsGlobal, Settings.director.camerasPractice_Overridden );
			Update( Director_Cameras_Qualifying, Settings.director.camerasQualifying, Director_Cameras_Qualifying_Override, directorIsGlobal, Settings.director.camerasQualifying_Overridden );
			Update( Director_Cameras_Intro, Settings.director.camerasIntro, Director_Cameras_Intro_Override, directorIsGlobal, Settings.director.camerasIntro_Overridden );
			Update( Director_Cameras_Scenic, Settings.director.camerasScenic, Director_Cameras_Scenic_Override, directorIsGlobal, Settings.director.camerasScenic_Overridden );
			Update( Director_Cameras_Pits, Settings.director.camerasPits, Director_Cameras_Pits_Override, directorIsGlobal, Settings.director.camerasPits_Overridden );
			Update( Director_Cameras_StartFinish, Settings.director.camerasStartFinish, Director_Cameras_StartFinish_Override, directorIsGlobal, Settings.director.camerasStartFinish_Overridden );
			Update( Director_Cameras_Reverse, Settings.director.camerasReverse, Director_Cameras_Reverse_Override, directorIsGlobal, Settings.director.camerasReverse_Overridden );
			Update( Director_Cameras_Inside, Settings.director.camerasInside, Director_Cameras_Inside_Override, directorIsGlobal, Settings.director.camerasInside_Overridden );
			Update( Director_Cameras_Close, Settings.director.camerasClose, Director_Cameras_Close_Override, directorIsGlobal, Settings.director.camerasClose_Overridden );
			Update( Director_Cameras_Medium, Settings.director.camerasMedium, Director_Cameras_Medium_Override, directorIsGlobal, Settings.director.camerasMedium_Overridden );
			Update( Director_Cameras_Far, Settings.director.camerasFar, Director_Cameras_Far_Override, directorIsGlobal, Settings.director.camerasFar_Overridden );
			Update( Director_Cameras_VeryFar, Settings.director.camerasVeryFar, Director_Cameras_VeryFar_Override, directorIsGlobal, Settings.director.camerasVeryFar_Overridden );
			Update( Director_Cameras_Custom1, Settings.director.camerasCustom1, Director_Cameras_Custom1_Override, directorIsGlobal, Settings.director.camerasCustom1_Overridden );
			Update( Director_Cameras_Custom2, Settings.director.camerasCustom2, Director_Cameras_Custom2_Override, directorIsGlobal, Settings.director.camerasCustom2_Overridden );
			Update( Director_Cameras_Custom3, Settings.director.camerasCustom3, Director_Cameras_Custom3_Override, directorIsGlobal, Settings.director.camerasCustom3_Overridden );
			Update( Director_Cameras_Custom4, Settings.director.camerasCustom4, Director_Cameras_Custom4_Override, directorIsGlobal, Settings.director.camerasCustom4_Overridden );
			Update( Director_Cameras_Custom5, Settings.director.camerasCustom5, Director_Cameras_Custom5_Override, directorIsGlobal, Settings.director.camerasCustom5_Overridden );
			Update( Director_Cameras_Custom6, Settings.director.camerasCustom6, Director_Cameras_Custom6_Override, directorIsGlobal, Settings.director.camerasCustom6_Overridden );

			Update( Director_SwitchDelay_Director, Settings.director.switchDelayDirector, Director_SwitchDelay_Director_Override, directorIsGlobal, Settings.director.switchDelayDirector_Overridden );
			Update( Director_SwitchDelay_iRacing, Settings.director.switchDelayIracing, Director_SwitchDelay_iRacing_Override, directorIsGlobal, Settings.director.switchDelayIracing_Overridden );
			Update( Director_SwitchDelay_RadioChatter, Settings.director.switchDelayRadioChatter, Director_SwitchDelay_RadioChatter_Override, directorIsGlobal, Settings.director.switchDelayRadioChatter_Overridden );
			Update( Director_SwitchDelay_NotInRace, Settings.director.switchDelayNotInRace, Director_SwitchDelay_NotInRace_Override, directorIsGlobal, Settings.director.switchDelayNotInRace_Overridden );

			Update( Director_Heat_MaxGapTime, Settings.director.heatMaxGapTime, Director_Heat_MaxGapTime_Override, directorIsGlobal, Settings.director.heatMaxGapTime_Overridden );
			Update( Director_Heat_OvertakeBonus, Settings.director.heatOvertakeBonus, Director_Heat_OvertakeBonus_Override, directorIsGlobal, Settings.director.heatOvertakeBonus_Overridden );
			Update( Director_Heat_PositionBattle, Settings.director.heatPositionBattle, Director_Heat_PositionBattle_Override, directorIsGlobal, Settings.director.heatPositionBattle_Overridden );
			Update( Director_Heat_Bias, Settings.director.heatBias, Director_Heat_Bias_Override, directorIsGlobal, Settings.director.heatBias_Overridden );

			Update( Director_PreferredCar_UserIds, Settings.director.preferredCarUserIds, Director_PreferredCar_UserIds_Override, directorIsGlobal, Settings.director.preferredCarUserIds_Overridden );
			Update( Director_PreferredCar_CarNumbers, Settings.director.preferredCarCarNumbers, Director_PreferredCar_CarNumbers_Override, directorIsGlobal, Settings.director.preferredCarCarNumbers_Overridden );
			Update( Director_PreferredCar_LockOnEnabled, Settings.director.preferredCarLockOnEnabled, Director_PreferredCar_LockOnEnabled_Override, directorIsGlobal, Settings.director.preferredCarLockOnEnabled_Overridden );
			Update( Director_PreferredCar_LockOnMinimumHeat, Settings.director.preferredCarLockOnMinimumHeat, Director_PreferredCar_LockOnMinimumHeat_Override, directorIsGlobal, Settings.director.preferredCarLockOnMinimumHeat_Overridden );

			Update( Director_Rules_Rule1_Enabled, Settings.director.rule1_Enabled, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );
			Update( Director_Rules_Rule1_Camera, Settings.director.rule1_Camera, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );
			Update( Director_Rules_Rule2_Enabled, Settings.director.rule2_Enabled, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );
			Update( Director_Rules_Rule2_Camera, Settings.director.rule2_Camera, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );
			Update( Director_Rules_Rule3_Enabled, Settings.director.rule3_Enabled, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );
			Update( Director_Rules_Rule3_Camera, Settings.director.rule3_Camera, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );
			Update( Director_Rules_Rule4_Enabled, Settings.director.rule4_Enabled, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );
			Update( Director_Rules_Rule4_Camera, Settings.director.rule4_Camera, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );
			Update( Director_Rules_Rule5_Enabled, Settings.director.rule5_Enabled, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );
			Update( Director_Rules_Rule5_Camera, Settings.director.rule5_Camera, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );
			Update( Director_Rules_Rule6_Enabled, Settings.director.rule6_Enabled, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );
			Update( Director_Rules_Rule6_Camera, Settings.director.rule6_Camera, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );
			Update( Director_Rules_Rule7_Enabled, Settings.director.rule7_Enabled, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );
			Update( Director_Rules_Rule7_Camera, Settings.director.rule7_Camera, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );
			Update( Director_Rules_Rule8_Enabled, Settings.director.rule8_Enabled, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );
			Update( Director_Rules_Rule8_Camera, Settings.director.rule8_Camera, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );
			Update( Director_Rules_Rule9_Enabled, Settings.director.rule9_Enabled, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );
			Update( Director_Rules_Rule9_Camera, Settings.director.rule9_Camera, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );
			Update( Director_Rules_Rule10_Enabled, Settings.director.rule10_Enabled, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );
			Update( Director_Rules_Rule10_Camera, Settings.director.rule10_Camera, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );
			Update( Director_Rules_Rule11_Enabled, Settings.director.rule11_Enabled, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );
			Update( Director_Rules_Rule11_Camera, Settings.director.rule11_Camera, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );
			Update( Director_Rules_Rule12_Enabled, Settings.director.rule12_Enabled, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );
			Update( Director_Rules_Rule12_Camera, Settings.director.rule12_Camera, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );
			Update( Director_Rules_Rule13_Enabled, Settings.director.rule13_Enabled, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );
			Update( Director_Rules_Rule13_Camera, Settings.director.rule13_Camera, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );
			Update( Director_Rules_Rule14_Enabled, Settings.director.rule14_Enabled, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );
			Update( Director_Rules_Rule14_Camera, Settings.director.rule14_Camera, Director_Rules_Override, directorIsGlobal, Settings.director.rules_Overridden );

			Update( Director_AutoCam_Inside_Minimum, Settings.director.autoCamInsideMinimum, Director_AutoCam_Override, directorIsGlobal, Settings.director.autoCam_Overridden );
			Update( Director_AutoCam_Inside_Maximum, Settings.director.autoCamInsideMaximum, Director_AutoCam_Override, directorIsGlobal, Settings.director.autoCam_Overridden );
			Update( Director_AutoCam_Close_Maximum, Settings.director.autoCamCloseMaximum, Director_AutoCam_Override, directorIsGlobal, Settings.director.autoCam_Overridden );
			Update( Director_AutoCam_Medium_Maximum, Settings.director.autoCamMediumMaximum, Director_AutoCam_Override, directorIsGlobal, Settings.director.autoCam_Overridden );
			Update( Director_AutoCam_Far_Maximum, Settings.director.autoCamFarMaximum, Director_AutoCam_Override, directorIsGlobal, Settings.director.autoCam_Overridden );

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

			Overlay_ShowBorders.IsChecked = Settings.overlay.showBorders;

			// overlay - general

			Update( Overlay_General_Position_X, Overlay_General_Position_Y, Settings.overlay.position, Overlay_General_Position_Override, overlayIsGlobal, Settings.overlay.position_Overridden );
			Update( Overlay_General_Size_W, Overlay_General_Size_H, Settings.overlay.size, Overlay_General_Size_Override, overlayIsGlobal, Settings.overlay.size_Overridden );

			// overlay - fonts

			Update( Overlay_Font_FontA_Name, Settings.overlay.fontNames[ 0 ], Overlay_Font_FontA_Name_Override, overlayIsGlobal, Settings.overlay.fontNames_Overridden[ 0 ] );
			Update( Overlay_Font_FontB_Name, Settings.overlay.fontNames[ 1 ], Overlay_Font_FontB_Name_Override, overlayIsGlobal, Settings.overlay.fontNames_Overridden[ 1 ] );
			Update( Overlay_Font_FontC_Name, Settings.overlay.fontNames[ 2 ], Overlay_Font_FontC_Name_Override, overlayIsGlobal, Settings.overlay.fontNames_Overridden[ 2 ] );
			Update( Overlay_Font_FontD_Name, Settings.overlay.fontNames[ 3 ], Overlay_Font_FontD_Name_Override, overlayIsGlobal, Settings.overlay.fontNames_Overridden[ 3 ] );

			// overlay - images

			if ( Overlay_Image_ID.SelectedIndex == -1 )
			{
				Overlay_Image_ID.SelectedIndex = 0;
			}

			UpdateOverlayImage();

			// overlay - text

			if ( Overlay_Text_ID.SelectedIndex == -1 )
			{
				Overlay_Text_ID.SelectedIndex = 0;
			}

			UpdateOverlayText();

			// overlay - localization

			UpdateOverlayTranslation();

			// overlay - start lights

			Update( Overlay_StartLights_Enable, Settings.overlay.startLightsEnabled, Overlay_StartLights_Enable_Override, overlayIsGlobal, Settings.overlay.startLightsEnabled_Overridden );
			Update( Overlay_StartLights_Position_X, Overlay_StartLights_Position_Y, Settings.overlay.startLightsPosition, Overlay_StartLights_Position_Override, overlayIsGlobal, Settings.overlay.startLightsPosition_Overridden );

			// overlay - race status

			Update( Overlay_RaceStatus_Enable, Settings.overlay.raceStatusEnabled, Overlay_RaceStatus_Enable_Override, overlayIsGlobal, Settings.overlay.raceStatusEnabled_Overridden );
			Update( Overlay_RaceStatus_Position_X, Overlay_RaceStatus_Position_Y, Settings.overlay.raceStatusPosition, Overlay_RaceStatus_Position_Override, overlayIsGlobal, Settings.overlay.raceStatusPosition_Overridden );

			// overlay - leaderboard

			Update( Overlay_Leaderboard_Enable, Settings.overlay.leaderboardEnabled, Overlay_Leaderboard_Enable_Override, overlayIsGlobal, Settings.overlay.leaderboardEnabled_Overridden );
			Update( Overlay_Leaderboard_Position_X, Overlay_Leaderboard_Position_Y, Settings.overlay.leaderboardPosition, Overlay_Leaderboard_Position_Override, overlayIsGlobal, Settings.overlay.leaderboardPosition_Overridden );
			Update( Overlay_Leaderboard_FirstSlotPosition_X, Overlay_Leaderboard_FirstSlotPosition_Y, Settings.overlay.leaderboardFirstSlotPosition, Overlay_Leaderboard_FirstSlotPosition_Override, overlayIsGlobal, Settings.overlay.leaderboardFirstSlotPosition_Overridden );
			Update( Overlay_Leaderboard_SlotCount, Settings.overlay.leaderboardSlotCount, Overlay_Leaderboard_SlotCount_Override, overlayIsGlobal, Settings.overlay.leaderboardSlotCount_Overridden );
			Update( Overlay_Leaderboard_SlotSpacing_X, Overlay_Leaderboard_SlotSpacing_Y, Settings.overlay.leaderboardSlotSpacing, Overlay_Leaderboard_SlotSpacing_Override, overlayIsGlobal, Settings.overlay.leaderboardSlotSpacing_Overridden );
			Update( Overlay_Leaderboard_SeparateBoards, Settings.overlay.leaderboardSeparateBoards, Overlay_Leaderboard_SeparateBoards_Override, overlayIsGlobal, Settings.overlay.leaderboardSeparateBoards_Overridden );
			Update( Overlay_Leaderboard_MultiClassOffset_X, Overlay_Leaderboard_MultiClassOffset_Y, Settings.overlay.leaderboardMultiClassOffset, Overlay_Leaderboard_MultiClassOffset_Override, overlayIsGlobal, Settings.overlay.leaderboardMultiClassOffset_Overridden );
			Update( Overlay_Leaderboard_MultiClassOffset_Type, leaderboardMultiClassOffsetTypeOptions.FirstOrDefault( x => x.Value == Settings.overlay.leaderboardMultiClassOffsetType ).Key, Overlay_Leaderboard_MultiClassOffset_Override, overlayIsGlobal, Settings.overlay.leaderboardMultiClassOffset_Overridden );

			// overlay - track map

			Update( Overlay_TrackMap_Enable, Settings.overlay.trackMapEnabled, Overlay_TrackMap_Enable_Override, overlayIsGlobal, Settings.overlay.trackMapEnabled_Overridden );
			Update( Overlay_TrackMap_Reverse, Settings.overlay.trackMapReverse, Overlay_TrackMap_Reverse_Override, overlayIsGlobal, Settings.overlay.trackMapReverse_Overridden );
			Update( Overlay_TrackMap_Position_X, Overlay_TrackMap_Position_Y, Settings.overlay.trackMapPosition, Overlay_TrackMap_Position_Override, overlayIsGlobal, Settings.overlay.trackMapPosition_Overridden );
			Update( Overlay_TrackMap_Size_W, Overlay_TrackMap_Size_H, Settings.overlay.trackMapSize, Overlay_TrackMap_Size_Override, overlayIsGlobal, Settings.overlay.trackMapSize_Overridden );
			Update( Overlay_TrackMap_TextureFilePath, Settings.overlay.trackMapTextureFilePath, Overlay_TrackMap_TextureFilePath_Override, overlayIsGlobal, Settings.overlay.trackMapTextureFilePath_Overridden, Overlay_TrackMap_TextureFilePath_Button );
			Update( Overlay_TrackMap_LineThickness, Settings.overlay.trackMapLineThickness, Overlay_TrackMap_LineThickness_Override, overlayIsGlobal, Settings.overlay.trackMapLineThickness_Overridden );
			Update( Overlay_TrackMap_LineColor_R, Overlay_TrackMap_LineColor_G, Overlay_TrackMap_LineColor_B, Overlay_TrackMap_LineColor_A, Settings.overlay.trackMapLineColor, Overlay_TrackMap_LineColor_Override, overlayIsGlobal, Settings.overlay.trackMapLineColor_Overridden );
			Update( Overlay_TrackMap_StartFinishOffset, Settings.overlay.trackMapStartFinishOffset, Overlay_TrackMap_StartFinishOffset_Override, overlayIsGlobal, Settings.overlay.trackMapStartFinishOffset_Overridden );

			// overlay - pit lane

			Update( Overlay_PitLane_Enable, Settings.overlay.pitLaneEnabled, Overlay_PitLane_Enable_Override, overlayIsGlobal, Settings.overlay.pitLaneEnabled_Overridden );
			Update( Overlay_PitLane_Position_X, Overlay_PitLane_Position_Y, Settings.overlay.pitLanePosition, Overlay_PitLane_Position_Override, overlayIsGlobal, Settings.overlay.pitLanePosition_Overridden );
			Update( Overlay_PitLane_Length, Settings.overlay.pitLaneLength, Overlay_PitLane_Length_Override, overlayIsGlobal, Settings.overlay.pitLaneLength_Overridden );

			// overlay - voice of

			Update( Overlay_VoiceOf_Enable, Settings.overlay.voiceOfEnabled, Overlay_VoiceOf_Enable_Override, overlayIsGlobal, Settings.overlay.voiceOfEnabled_Overridden );
			Update( Overlay_VoiceOf_Position_X, Overlay_VoiceOf_Position_Y, Settings.overlay.voiceOfPosition, Overlay_VoiceOf_Position_Override, overlayIsGlobal, Settings.overlay.voiceOfPosition_Overridden );

			// overlay - chyron

			Update( Overlay_Chyron_Enable, Settings.overlay.chyronEnabled, Overlay_Chyron_Enable_Override, overlayIsGlobal, Settings.overlay.chyronEnabled_Overridden );
			Update( Overlay_Chyron_Position_X, Overlay_Chyron_Position_Y, Settings.overlay.chyronPosition, Overlay_Chyron_Position_Override, overlayIsGlobal, Settings.overlay.chyronPosition_Overridden );

			// overlay - subtitle

			Update( Overlay_Subtitle_Enable, Settings.overlay.subtitleEnabled, Overlay_Subtitle_Enable_Override, overlayIsGlobal, Settings.overlay.subtitleEnabled_Overridden );
			Update( Overlay_Subtitle_Position_X, Overlay_Subtitle_Position_Y, Settings.overlay.subtitlePosition, Overlay_Subtitle_Position_Override, overlayIsGlobal, Settings.overlay.subtitlePosition_Overridden );
			Update( Overlay_Subtitle_MaxSize_W, Overlay_Subtitle_MaxSize_H, Settings.overlay.subtitleMaxSize, Overlay_Subtitle_MaxSize_Override, overlayIsGlobal, Settings.overlay.subtitleMaxSize_Overridden );
			Update( Overlay_Subtitle_TextPadding_X, Overlay_Subtitle_TextPadding_Y, Settings.overlay.subtitleTextPadding, Overlay_Subtitle_TextPadding_Override, overlayIsGlobal, Settings.overlay.subtitleTextPadding_Overridden );

			// overlay - car number

			Update( Overlay_CarNumber_OverrideEnable, Settings.overlay.carNumberOverrideEnabled, Overlay_CarNumber_OverrideEnable_Override, overlayIsGlobal, Settings.overlay.carNumberOverrideEnabled_Overridden );
			Update( Overlay_CarNumber_ColorA_R, Overlay_CarNumber_ColorA_G, Overlay_CarNumber_ColorA_B, Overlay_CarNumber_ColorA_A, Settings.overlay.carNumberColorA, Overlay_CarNumber_ColorA_Override, overlayIsGlobal, Settings.overlay.carNumberColorA_Overridden, Overlay_CarNumber_ColorA_Palette );
			Update( Overlay_CarNumber_ColorB_R, Overlay_CarNumber_ColorB_G, Overlay_CarNumber_ColorB_B, Overlay_CarNumber_ColorB_A, Settings.overlay.carNumberColorB, Overlay_CarNumber_ColorB_Override, overlayIsGlobal, Settings.overlay.carNumberColorB_Overridden, Overlay_CarNumber_ColorB_Palette );
			Update( Overlay_CarNumber_ColorC_R, Overlay_CarNumber_ColorC_G, Overlay_CarNumber_ColorC_B, Overlay_CarNumber_ColorC_A, Settings.overlay.carNumberColorC, Overlay_CarNumber_ColorC_Override, overlayIsGlobal, Settings.overlay.carNumberColorC_Overridden, Overlay_CarNumber_ColorC_Palette );
			Update( Overlay_CarNumber_Pattern, patternOptions.FirstOrDefault( x => x.Value == Settings.overlay.carNumberPattern ).Key, Overlay_CarNumber_Pattern_Override, overlayIsGlobal, Settings.overlay.carNumberPattern_Overridden );
			Update( Overlay_CarNumber_Slant, slantOptions.FirstOrDefault( x => x.Value == Settings.overlay.carNumberSlant ).Key, Overlay_CarNumber_Slant_Override, overlayIsGlobal, Settings.overlay.carNumberSlant_Overridden );

			// overlay - telemetry

			Update( Overlay_Telemetry_PitColor_R, Overlay_Telemetry_PitColor_G, Overlay_Telemetry_PitColor_B, Overlay_Telemetry_PitColor_A, Settings.overlay.telemetryPitColor, Overlay_Telemetry_PitColor_Override, overlayIsGlobal, Settings.overlay.telemetryPitColor_Overridden, Overlay_Telemetry_PitColor_Palette );
			Update( Overlay_Telemetry_OutColor_R, Overlay_Telemetry_OutColor_G, Overlay_Telemetry_OutColor_B, Overlay_Telemetry_OutColor_A, Settings.overlay.telemetryOutColor, Overlay_Telemetry_OutColor_Override, overlayIsGlobal, Settings.overlay.telemetryOutColor_Overridden, Overlay_Telemetry_OutColor_Palette );
			Update( Overlay_Telemetry_IsInBetweenCars, Settings.overlay.telemetryIsBetweenCars, Overlay_Telemetry_IsInBetweenCars_Override, overlayIsGlobal, Settings.overlay.telemetryIsBetweenCars_Overridden );
			Update( Overlay_Telemetry_Mode_ShowLaps, ( Settings.overlay.telemetryMode == 0 ), Overlay_Telemetry_Mode_Override, overlayIsGlobal, Settings.overlay.telemetryMode_Overridden );
			Update( Overlay_TelemetryMode_ShowDistance, ( Settings.overlay.telemetryMode == 1 ), Overlay_Telemetry_Mode_Override, overlayIsGlobal, Settings.overlay.telemetryMode_Overridden );
			Update( Overlay_Telemetry_Mode_ShowTime, ( Settings.overlay.telemetryMode == 2 ), Overlay_Telemetry_Mode_Override, overlayIsGlobal, Settings.overlay.telemetryMode_Overridden );
			Update( Overlay_Telemetry_ShowAsNegativeNumbers, Settings.overlay.telemetryShowAsNegativeNumbers, Overlay_Telemetry_ShowAsNegativeNumbers_Override, overlayIsGlobal, Settings.overlay.telemetryShowAsNegativeNumbers_Overridden );

			// overlay - intro

			Update( Overlay_Intro_Enable, Settings.overlay.introEnabled, Overlay_Intro_Enable_Override, overlayIsGlobal, Settings.overlay.introEnabled_Overridden );
			Update( Overlay_Intro_LeftPosition_X, Overlay_Intro_LeftPosition_Y, Settings.overlay.introLeftPosition, Overlay_Intro_LeftPosition_Override, overlayIsGlobal, Settings.overlay.introLeftPosition_Overridden );
			Update( Overlay_Intro_LeftScale, Settings.overlay.introLeftScale, Overlay_Intro_LeftScale_Override, overlayIsGlobal, Settings.overlay.introLeftScale_Overridden );
			Update( Overlay_Intro_RightPosition_X, Overlay_Intro_RightPosition_Y, Settings.overlay.introRightPosition, Overlay_Intro_RightPosition_Override, overlayIsGlobal, Settings.overlay.introRightPosition_Overridden );
			Update( Overlay_Intro_RightScale, Settings.overlay.introRightScale, Overlay_Intro_RightScale_Override, overlayIsGlobal, Settings.overlay.introRightScale_Overridden );
			Update( Overlay_Intro_LeftStartTime, Settings.overlay.introLeftStartTime, Overlay_Intro_StartTime_Override, overlayIsGlobal, Settings.overlay.introStartTime_Overridden );
			Update( Overlay_Intro_RightStartTime, Settings.overlay.introRightStartTime, Overlay_Intro_StartTime_Override, overlayIsGlobal, Settings.overlay.introStartTime_Overridden );
			Update( Overlay_Intro_StartInterval, Settings.overlay.introStartInterval, Overlay_Intro_StartInterval_Override, overlayIsGlobal, Settings.overlay.introStartInterval_Overridden );
			Update( Overlay_Intro_LeftInAnimationNumber, inAnimationOptions.FirstOrDefault( x => x.Value == Settings.overlay.introLeftInAnimationNumber ).Key, Overlay_Intro_InAnimationNumber_Override, overlayIsGlobal, Settings.overlay.introInAnimationNumber_Overridden );
			Update( Overlay_Intro_RightInAnimationNumber, inAnimationOptions.FirstOrDefault( x => x.Value == Settings.overlay.introRightInAnimationNumber ).Key, Overlay_Intro_InAnimationNumber_Override, overlayIsGlobal, Settings.overlay.introInAnimationNumber_Overridden );
			Update( Overlay_Intro_LeftOutAnimationNumber, outAnimationOptions.FirstOrDefault( x => x.Value == Settings.overlay.introLeftOutAnimationNumber ).Key, Overlay_Intro_OutAnimationNumber_Override, overlayIsGlobal, Settings.overlay.introOutAnimationNumber_Overridden );
			Update( Overlay_Intro_RightOutAnimationNumber, outAnimationOptions.FirstOrDefault( x => x.Value == Settings.overlay.introRightOutAnimationNumber ).Key, Overlay_Intro_OutAnimationNumber_Override, overlayIsGlobal, Settings.overlay.introOutAnimationNumber_Overridden );
			Update( Overlay_Intro_InTime, Settings.overlay.introInTime, Overlay_Intro_InTime_Override, overlayIsGlobal, Settings.overlay.introInTime_Overridden );
			Update( Overlay_Intro_HoldTime, Settings.overlay.introHoldTime, Overlay_Intro_HoldTime_Override, overlayIsGlobal, Settings.overlay.introHoldTime_Overridden );
			Update( Overlay_Intro_OutTime, Settings.overlay.introOutTime, Overlay_Intro_OutTime_Override, overlayIsGlobal, Settings.overlay.introOutTime_Overridden );

			// overlay - hud

			Update( Overlay_Hud_Enable, Settings.overlay.hudEnabled, Overlay_Hud_Enable_Override, overlayIsGlobal, Settings.overlay.hudEnabled_Overridden );
			Update( Overlay_Hud_Position_X, Overlay_Hud_Position_Y, Settings.overlay.hudPosition, Overlay_Hud_Position_Override, overlayIsGlobal, Settings.overlay.hudPosition_Overridden );
			Update( Overlay_Hud_SpeechToTextPosition_X, Overlay_Hud_SpeechToTextPosition_Y, Settings.overlay.hudSpeechToTextPosition, Overlay_Hud_SpeechToTextPosition_Override, overlayIsGlobal, Settings.overlay.hudSpeechToTextPosition_Overridden );
			Update( Overlay_Hud_SpeechToTextMaxSize_W, Overlay_Hud_SpeechToTextMaxSize_H, Settings.overlay.hudSpeechToTextMaxSize, Overlay_Hud_SpeechToTextMaxSize_Override, overlayIsGlobal, Settings.overlay.hudSpeechToTextMaxSize_Overridden );
			Update( Overlay_Hud_SpeechToTextTextPadding_X, Overlay_Hud_SpeechToTextTextPadding_Y, Settings.overlay.hudSpeechToTextTextPadding, Overlay_Hud_SpeechToTextTextPadding_Override, overlayIsGlobal, Settings.overlay.hudSpeechToTextTextPadding_Overridden );

			// overlay - trainer

			Update( Overlay_Trainer_Enable, Settings.overlay.trainerEnabled, Overlay_Trainer_Enable_Override, overlayIsGlobal, Settings.overlay.trainerEnabled_Overridden );
			Update( Overlay_Trainer_Position_X, Overlay_Trainer_Position_Y, Settings.overlay.trainerPosition, Overlay_Trainer_Position_Override, overlayIsGlobal, Settings.overlay.trainerPosition_Overridden );
			Update( Overlay_Trainer_Size_W, Overlay_Trainer_Size_H, Settings.overlay.trainerSize, Overlay_Trainer_Size_Override, overlayIsGlobal, Settings.overlay.trainerSize_Overridden );
			Update( Overlay_Trainer_SpeedScale, Settings.overlay.trainerSpeedScale, Overlay_Trainer_SpeedScale_Override, overlayIsGlobal, Settings.overlay.trainerSpeedScale_Overridden );

			// web page

			WebPage_General_Enabled.IsChecked = Settings.editor.webpageGeneralEnabled;
			WebPage_General_SourceFolder.Text = Settings.editor.webpageGeneralSourceFolder;
			WebPage_General_OutputFolder.Text = Settings.editor.webpageGeneralOutputFolder;

			// iracing

			iRacing_General_CommandRateLimit.Value = Settings.editor.iracingGeneralCommandRateLimit;

			iRacing_Account_Username.Text = Settings.editor.iracingAccountUsername;
			iRacing_Account_Password.Password = Settings.editor.iracingAccountPassword;

			iRacing_CustomPaints_Directory.Text = Settings.editor.iracingCustomPaintsDirectory;
			iRacing_CustomPaints_AiRosterFile.Text = Settings.editor.iracingCustomPaintsAiRosterFile;

			iRacing_DriverNames_Suffixes.Text = Settings.editor.iracingDriverNamesSuffixes;
			iRacing_DriverNames_FormatOption.SelectedItem = formatOptions.FirstOrDefault( x => x.Value == Settings.editor.iracingDriverNameFormatOption ).Key;
			iRacing_DriverNames_CapitalizationOption.SelectedItem = capitalizationOptions.FirstOrDefault( x => x.Value == Settings.editor.iracingDriverNameCapitalizationOption ).Key;

			// editor

			Editor_AlwaysOnTop.IsChecked = Settings.editor.editorAlwaysOnTop;

			Editor_Mouse_PositioningSpeedNormal.Value = Settings.editor.editorMousePositioningSpeedNormal;
			Editor_Mouse_PositioningSpeedFast.Value = Settings.editor.editorMousePositioningSpeedFast;
			Editor_Mouse_PositioningSpeedSlow.Value = Settings.editor.editorMousePositioningSpeedSlow;

			Editor_Incidents_ScenicCameras.Text = Settings.editor.editorIncidentsScenicCameras;
			Editor_Incidents_EditCameras.Text = Settings.editor.editorIncidentsEditCameras;
			Editor_Incidents_OverlapMergeTime.Value = Settings.editor.editorIncidentsOverlapMergeTime;
			Editor_Incidents_Timeout.Value = Settings.editor.editorIncidentsTimeout;

			Editor_SteamVr_Enabled.IsChecked = Settings.editor.editorSteamVrEnabled;
			Editor_SteamVr_ControllerOption.SelectedItem = controllerOptions.FirstOrDefault( x => x.Value == Settings.editor.editorSteamVrControllerGuid ).Key;
			Editor_SteamVr_Width.Value = Settings.editor.editorSteamVrWidth;
			Editor_SteamVr_Position_X.Value = Settings.editor.editorSteamVrPosition.x;
			Editor_SteamVr_Position_Y.Value = Settings.editor.editorSteamVrPosition.y;
			Editor_SteamVr_Position_Z.Value = Settings.editor.editorSteamVrPosition.z;
			Editor_SteamVr_Curvature.Value = Settings.editor.editorSteamVrCurvature;

			Editor_SpeechToText_Enabled.IsChecked = Settings.editor.editorSpeechToTextEnabled;
			Editor_SpeechToText_AudioCaptureDeviceOption.SelectedItem = audioCaptureDeviceOptions.FirstOrDefault( x => x.Value == Settings.editor.editorSpeechToTextAudioCaptureDeviceId ).Key;
			Editor_SpeechToText_CognitiveServicesKey.Text = Settings.editor.editorSpeechToTextCognitiveServiceKey;
			Editor_SpeechToText_CognitiveServicesRegion.Text = Settings.editor.editorSpeechToTextCognitiveServiceRegion;
			Editor_SpeechToText_Language.Text = Settings.editor.editorSpeechToTextLanguage;
			Editor_SpeechToText_PotatoFilterEnabled.IsChecked = Settings.editor.editorSpeechToTextPotatoFilterEnabled;

			Editor_PushToTalk_MuteEnabled.IsChecked = Settings.editor.editorPushToTalkMuteEnabled;
			Editor_PushToTalk_AudioRenderDeviceOption.SelectedItem = audioRenderDeviceOptions.FirstOrDefault( x => x.Value == Settings.editor.editorPushToTalkAudioRenderDeviceId ).Key;

			Editor_Triggers_SessionChange.Text = Settings.editor.editorTriggersSessionChange;

			Editor_Startup_EnableDirector.IsChecked = Settings.editor.editorStartupEnableDirector;

			Editor_ControlPanel_SortByCarNumber.IsChecked = Settings.editor.editorControlPanelSortByCarNumber;

			// Editor_WebcamStreaming_Enabled.IsChecked = Settings.editor.editorWebcamStreamingEnabled;
			// Editor_WebcamStreaming_WebserverURL.Text = Settings.editor.editorWebcamStreamingWebserverURL;

			// turn on/off topmost window attribute

			Topmost = Settings.editor.editorAlwaysOnTop;

			//

			initializing--;
		}

		public void UpdateOverlayImage()
		{
			if ( Overlay_Image_ID.SelectedIndex != -1 )
			{
				initializing++;

				var id = (string) Overlay_Image_ID.Items.GetItemAt( Overlay_Image_ID.SelectedIndex );

				Settings.UpdateCombinedOverlay();

				var overlayIsGlobal = Settings.overlayLocal.filePath == Settings.relativeGlobalOverlaySettingsFilePath;

				var settings = Settings.overlay.imageSettingsDataDictionary[ id ];

				Update( Image_ImageType, settings.imageType, Image_ImageType_Override, overlayIsGlobal, settings.imageType_Overridden );
				Update( Image_FilePath, settings.filePath, Image_FilePath_Override, overlayIsGlobal, settings.filePath_Overridden, Image_FilePath_Button );
				Update( Image_Position_X, Image_Position_Y, settings.position, Image_Position_Override, overlayIsGlobal, settings.position_Overridden );
				Update( Image_Size_W, Image_Size_H, settings.size, Image_Size_Override, overlayIsGlobal, settings.size_Overridden );
				Update( Image_TintColor_R, Image_TintColor_G, Image_TintColor_B, Image_TintColor_A, settings.tintColor, Image_TintColor_Override, overlayIsGlobal, settings.tintColor_Overridden, Image_TintColor_Palette );
				Update( Image_Border_L, Image_Border_T, Image_Border_R, Image_Border_B, settings.border, Image_Border_Override, overlayIsGlobal, settings.border_Overridden );
				Update( Image_Frames_W, Image_Frames_H, settings.frameSize, Image_Frames_Override, overlayIsGlobal, settings.frames_Overridden );
				Update( Image_Frames_Count, settings.frameCount, Image_Frames_Override, overlayIsGlobal, settings.frames_Overridden );
				Update( Image_AnimationSpeed, settings.animationSpeed, Image_AnimationSpeed_Override, overlayIsGlobal, settings.animationSpeed_Overridden );
				Update( Image_TilingEnabled, settings.tilingEnabled, Image_TilingEnabled_Override, overlayIsGlobal, settings.tilingEnabled_Overridden );
				Update( Image_UseClassColors, settings.useClassColors, Image_UseClassColors_Override, overlayIsGlobal, settings.useClassColors_Overridden );
				Update( Image_ClassColorStrength, settings.classColorStrength * 255.0f, Image_ClassColorStrength_Override, overlayIsGlobal, settings.classColorStrength_Overridden );

				initializing--;
			}
		}

		public void UpdateOverlayText()
		{
			if ( Overlay_Text_ID.SelectedIndex != -1 )
			{
				initializing++;

				var id = (string) Overlay_Text_ID.Items.GetItemAt( Overlay_Text_ID.SelectedIndex );

				Settings.UpdateCombinedOverlay();

				var overlayIsGlobal = Settings.overlayLocal.filePath == Settings.relativeGlobalOverlaySettingsFilePath;

				var settings = Settings.overlay.textSettingsDataDictionary[ id ];

				Update( Text_FontIndex, settings.fontIndex, Text_FontIndex_Override, overlayIsGlobal, settings.fontIndex_Overridden );
				Update( Text_FontSize, settings.fontSize, Text_FontSize_Override, overlayIsGlobal, settings.fontSize_Overridden );
				Update( Text_Alignment, settings.alignment, Text_Alignment_Override, overlayIsGlobal, settings.alignment_Overridden );
				Update( Text_Position_X, Text_Position_Y, settings.position, Text_Position_Override, overlayIsGlobal, settings.position_Overridden );
				Update( Text_Size_W, Text_Size_H, settings.size, Text_Size_Override, overlayIsGlobal, settings.size_Overridden );
				Update( Text_TintColor_R, Text_TintColor_G, Text_TintColor_B, Text_TintColor_A, settings.tintColor, Text_TintColor_Override, overlayIsGlobal, settings.tintColor_Overridden, Text_TintColor_Palette );
				Update( Text_UseClassColors, settings.useClassColors, Text_UseClassColors_Override, overlayIsGlobal, settings.useClassColors_Overridden );
				Update( Text_ClassColorStrength, settings.classColorStrength * 255.0f, Text_ClassColorStrength_Override, overlayIsGlobal, settings.classColorStrength_Overridden );
				Update( Text_AllowOverflow, settings.allowOverflow, Text_AllowOverflow_Override, overlayIsGlobal, settings.allowOverflow_Overridden );

				initializing--;
			}
		}

		public void UpdateOverlayTranslation()
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

		public void ControlPanel_Update()
		{
			updateTimeRemaining = Math.Max( 0, updateTimeRemaining - Program.deltaTime );

			if ( updateTimeRemaining == 0 )
			{
				updateTimeRemaining = UpdateIntervalTime;

				Dispatcher.Invoke( () =>
				{
					if ( IRSDK.isConnected )
					{
						ControlPanel_RaceStatus_SimFrame.Text = IRSDK.normalizedData.replayFrameNum.ToString();
						ControlPanel_RaceStatus_SessionName.Text = IRSDK.normalizedSession.sessionName;
						ControlPanel_RaceStatus_SessionType.Text = IRSDK.normalizedSession.sessionType;
						ControlPanel_RaceStatus_SessionState.Text = IRSDK.normalizedData.sessionState.ToString();
						ControlPanel_RaceStatus_SimTime.Text = Program.GetTimeString( IRSDK.normalizedData.sessionTime, false );
						ControlPanel_RaceStatus_RemainingTime.Text = Program.GetTimeString( IRSDK.normalizedData.sessionTimeRemaining, false );

						if ( IRSDK.normalizedData.sessionLapsRemaining == 32767 )
						{
							ControlPanel_RaceStatus_RemainingLaps.Text = "---";
						}
						else
						{
							ControlPanel_RaceStatus_RemainingLaps.Text = IRSDK.normalizedData.sessionLapsRemaining.ToString();
						}

						ConnectionStatusImage.Source = statusConnectedBitmapImage;
					}
					else
					{
						ControlPanel_RaceStatus_SimFrame.Text = string.Empty;
						ControlPanel_RaceStatus_SessionName.Text = string.Empty;
						ControlPanel_RaceStatus_SessionType.Text = string.Empty;
						ControlPanel_RaceStatus_SessionState.Text = string.Empty;
						ControlPanel_RaceStatus_SimTime.Text = string.Empty;
						ControlPanel_RaceStatus_RemainingTime.Text = string.Empty;
						ControlPanel_RaceStatus_RemainingLaps.Text = string.Empty;

						ConnectionStatusImage.Source = statusDisconnectedBitmapImage;
					}

					if ( IRSDK.targetCamEnabled )
					{
						ControlPanel_CameraControl_Enable_Button.Content = "🎥 Enabled";
						ControlPanel_CameraControl_Enable_Button.BorderBrush = System.Windows.Media.Brushes.Green;
						ControlPanel_CameraControl_Enable_Button.BorderThickness = new Thickness( 3.0 );

						if ( Director.isEnabled )
						{
							ControlPanel_CameraControl_Director_Button.BorderBrush = System.Windows.Media.Brushes.Green;
							ControlPanel_CameraControl_Director_Button.BorderThickness = new Thickness( 3.0 );

							ControlPanel_CameraControl_Manual_Button.BorderBrush = System.Windows.Media.Brushes.Gray;
							ControlPanel_CameraControl_Manual_Button.BorderThickness = new Thickness( 0.5 );
						}
						else
						{
							ControlPanel_CameraControl_Director_Button.BorderBrush = System.Windows.Media.Brushes.Gray;
							ControlPanel_CameraControl_Director_Button.BorderThickness = new Thickness( 0.5 );

							ControlPanel_CameraControl_Manual_Button.BorderBrush = System.Windows.Media.Brushes.Green;
							ControlPanel_CameraControl_Manual_Button.BorderThickness = new Thickness( 3.0 );
						}

						if ( Director.isHolding )
						{
							ControlPanel_CameraControl_Hold_Button.BorderBrush = System.Windows.Media.Brushes.Green;
							ControlPanel_CameraControl_Hold_Button.BorderThickness = new Thickness( 3.0 );
						}
						else
						{
							ControlPanel_CameraControl_Hold_Button.BorderBrush = System.Windows.Media.Brushes.Gray;
							ControlPanel_CameraControl_Hold_Button.BorderThickness = new Thickness( 0.5 );
						}

						ControlPanel_CameraControl_Reset_Button.BorderBrush = System.Windows.Media.Brushes.Gray;
						ControlPanel_CameraControl_Reset_Button.BorderThickness = new Thickness( 0.5 );

						var normalizedCar = IRSDK.normalizedData.FindNormalizedCarByCarIdx( IRSDK.targetCamCarIdx );

						if ( normalizedCar != null )
						{
							ControlPanel_CameraControl_CarNumber.Text = normalizedCar.carNumber;
							ControlPanel_CameraControl_DriverName.Text = normalizedCar.userName;
						}
						else
						{
							ControlPanel_CameraControl_CarNumber.Text = string.Empty;
							ControlPanel_CameraControl_DriverName.Text = string.Empty;
						}

						ControlPanel_CameraControl_CameraGroup.Text = IRSDK.GetCamGroupName( IRSDK.targetCamGroupNumber );
						ControlPanel_CameraControl_Reason.Text = IRSDK.targetCamReason;
						ControlPanel_CameraControl_Timer.Text = $"{IRSDK.cameraSwitchWaitTimeRemaining:0.0}";
					}
					else
					{
						ControlPanel_CameraControl_Enable_Button.Content = "🎥 Enable";
						ControlPanel_CameraControl_Enable_Button.BorderBrush = System.Windows.Media.Brushes.Gray;
						ControlPanel_CameraControl_Enable_Button.BorderThickness = new Thickness( 0.5 );

						ControlPanel_CameraControl_Director_Button.BorderBrush = System.Windows.Media.Brushes.Gray;
						ControlPanel_CameraControl_Director_Button.BorderThickness = new Thickness( 0.5 );

						ControlPanel_CameraControl_Manual_Button.BorderBrush = System.Windows.Media.Brushes.Gray;
						ControlPanel_CameraControl_Manual_Button.BorderThickness = new Thickness( 0.5 );

						ControlPanel_CameraControl_Hold_Button.BorderBrush = System.Windows.Media.Brushes.Gray;
						ControlPanel_CameraControl_Hold_Button.BorderThickness = new Thickness( 0.5 );

						ControlPanel_CameraControl_Reset_Button.BorderBrush = System.Windows.Media.Brushes.Gray;
						ControlPanel_CameraControl_Reset_Button.BorderThickness = new Thickness( 0.5 );

						ControlPanel_CameraControl_CarNumber.Text = string.Empty;
						ControlPanel_CameraControl_DriverName.Text = string.Empty;
						ControlPanel_CameraControl_CameraGroup.Text = string.Empty;
						ControlPanel_CameraControl_Reason.Text = string.Empty;
						ControlPanel_CameraControl_Timer.Text = string.Empty;
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

					var sortedNormalizedCars = Settings.editor.editorControlPanelSortByCarNumber ? IRSDK.normalizedData.carNumberSortedNormalizedCars : IRSDK.normalizedData.relativeLapPositionSortedNormalizedCars;

					foreach ( var normalizedCar in sortedNormalizedCars )
					{
						ControlPanelButton cpb;

						if ( normalizedCar.carIdx == 0 )
						{
							cpb = controlPanelButton[ 63 ];

							if ( IRSDK.targetCamEnabled && ( IRSDK.targetCamCarIdx == normalizedCar.carIdx ) )
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
								cpb.label[ 4 ].Content = $"↓ {normalizedCar.displayedName} ↓";
							}
							else
							{
								cpb.label[ 4 ].Content = normalizedCar.displayedName;
							}

							var label = $"P{normalizedCar.leaderboardIndex}";

							if ( normalizedCar.hasCrossedStartLine )
							{
								label += " S";
							}

							if ( normalizedCar.hasCrossedFinishLine )
							{
								label += " F";
							}

							cpb.label[ 0 ].Content = label;
							cpb.label[ 1 ].Content = $"#{normalizedCar.carNumber}";

							if ( normalizedCar.heatTotal > 0.025f )
							{
								cpb.label[ 2 ].Content = $"{normalizedCar.heat:0.0}+{normalizedCar.heatBonus:0.0}+{normalizedCar.heatBias:0.0}";
							}
							else
							{
								cpb.label[ 2 ].Content = string.Empty;
							}

							var backgroundButton = System.Windows.Media.Brushes.White;
							var foregroundLabels = System.Windows.Media.Brushes.Black;

							if ( normalizedCar.heatTotal > 0 )
							{
								var t = (int) Math.Floor( normalizedCar.heatTotal % 1 * 127 );

								var r = 0;
								var g = 0;
								var b = 0;

								if ( normalizedCar.heatTotal < 1 )
								{
									r = 255 - t;
									g = 255;
									b = 255;
								}
								else if ( normalizedCar.heatTotal < 2 )
								{
									r = 127 + t;
									g = 255;
									b = 255 - t;
								}
								else if ( normalizedCar.heatTotal < 3 )
								{
									r = 255;
									g = 255 - t;
									b = 127;
								}
								else if ( normalizedCar.heatTotal < 4 )
								{
									r = 255;
									g = 127 - t;
									b = 127 - t;
								}
								else
								{
									r = 255;
									g = 0;
									b = 0;
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

							if ( IRSDK.targetCamEnabled && ( IRSDK.targetCamCarIdx == normalizedCar.carIdx ) )
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

					UpdateCameraButton( ControlPanel_Camera_Scenic_Button, cameraType == SettingsDirector.CameraType.Scenic );
					UpdateCameraButton( ControlPanel_Camera_Pits_Button, cameraType == SettingsDirector.CameraType.Pits );
					UpdateCameraButton( ControlPanel_Camera_StartFinish_Button, cameraType == SettingsDirector.CameraType.StartFinish );
					UpdateCameraButton( ControlPanel_Camera_Reverse_Button, cameraType == SettingsDirector.CameraType.Reverse );

					UpdateCameraButton( ControlPanel_Camera_Inside_Button, cameraType == SettingsDirector.CameraType.Inside );
					UpdateCameraButton( ControlPanel_Camera_Close_Button, cameraType == SettingsDirector.CameraType.Close );
					UpdateCameraButton( ControlPanel_Camera_Medium_Button, cameraType == SettingsDirector.CameraType.Medium );
					UpdateCameraButton( ControlPanel_Camera_Far_Button, cameraType == SettingsDirector.CameraType.Far );
					UpdateCameraButton( ControlPanel_Camera_VeryFar_Button, cameraType == SettingsDirector.CameraType.VeryFar );

					UpdateCameraButton( ControlPanel_Camera_AutoCam_Button, cameraType == SettingsDirector.CameraType.AutoCam );

					UpdateCameraButton( ControlPanel_Camera_C1_Button, cameraType == SettingsDirector.CameraType.Custom1 );
					UpdateCameraButton( ControlPanel_Camera_C2_Button, cameraType == SettingsDirector.CameraType.Custom2 );
					UpdateCameraButton( ControlPanel_Camera_C3_Button, cameraType == SettingsDirector.CameraType.Custom3 );
					UpdateCameraButton( ControlPanel_Camera_C4_Button, cameraType == SettingsDirector.CameraType.Custom4 );
					UpdateCameraButton( ControlPanel_Camera_C5_Button, cameraType == SettingsDirector.CameraType.Custom5 );
					UpdateCameraButton( ControlPanel_Camera_C6_Button, cameraType == SettingsDirector.CameraType.Custom6 );
				} );
			}
		}

		private void UpdateCameraButton( Button button, bool isActive )
		{
			if ( IRSDK.targetCamEnabled && isActive )
			{
				button.BorderBrush = System.Windows.Media.Brushes.Green;
				button.BorderThickness = new Thickness( 3.0 );
			}
			else
			{
				button.BorderBrush = System.Windows.Media.Brushes.Gray;
				button.BorderThickness = new Thickness( 0.5 );
			}
		}

		private void SetManualCamera( SettingsDirector.CameraType newCameraType )
		{
			if ( IRSDK.targetCamEnabled )
			{
				cameraType = newCameraType;

				if ( normalizedCar == null )
				{
					normalizedCar = IRSDK.normalizedData.FindNormalizedCarByCarIdx( IRSDK.camCarIdx ) ?? IRSDK.normalizedData.normalizedCars[ 0 ];
				}

				Director.isEnabled = false;
				Director.targetCamCarIdx = normalizedCar.carIdx;
				Director.targetCamType = cameraType;

				Director.UpdateAutoCamType( normalizedCar );

				IRSDK.targetCamFastSwitchEnabled = true;
				IRSDK.targetCamSlowSwitchEnabled = false;
				IRSDK.targetCamCarIdx = normalizedCar.carIdx;
				IRSDK.targetCamGroupNumber = Director.GetCamGroupNumber( normalizedCar, cameraType );
				IRSDK.targetCamReason = "Manual camera control.";
			}
		}

		private void ControlPanel_CameraControl_Enable_Button_Click( object sender, RoutedEventArgs e )
		{
			IRSDK.targetCamEnabled = !IRSDK.targetCamEnabled;
		}

		private void ControlPanel_CameraControl_Director_Button_Click( object sender, RoutedEventArgs e )
		{
			Director.isEnabled = true;

			IRSDK.targetCamFastSwitchEnabled = true;

			normalizedCar = null;
		}

		private void ControlPanel_CameraControl_Manual_Button_Click( object sender, RoutedEventArgs e )
		{
			Director.isEnabled = false;

			IRSDK.targetCamFastSwitchEnabled = true;
		}

		private void ControlPanel_CameraControl_Hold_Button_Click( object sender, RoutedEventArgs e )
		{
			Director.isHolding = !Director.isHolding;
		}

		private void ControlPanel_CameraControl_Reset_Button_Click( object sender, RoutedEventArgs e )
		{
			Director.isHolding = false;

			IRSDK.cameraSwitchWaitTimeRemaining = 0;
		}

		private void ControlPanel_Camera_Scenic_Button_Click( object sender, RoutedEventArgs e )
		{
			SetManualCamera( SettingsDirector.CameraType.Scenic );
		}

		private void ControlPanel_Camera_Pits_Button_Click( object sender, RoutedEventArgs e )
		{
			SetManualCamera( SettingsDirector.CameraType.Pits );
		}

		private void ControlPanel_Camera_StartFinish_Button_Click( object sender, RoutedEventArgs e )
		{
			SetManualCamera( SettingsDirector.CameraType.StartFinish );
		}

		private void ControlPanel_Camera_Reverse_Button_Click( object sender, RoutedEventArgs e )
		{
			SetManualCamera( SettingsDirector.CameraType.Reverse );
		}

		private void ControlPanel_Camera_Inside_Button_Click( object sender, RoutedEventArgs e )
		{
			SetManualCamera( SettingsDirector.CameraType.Inside );
		}

		private void ControlPanel_Camera_Close_Button_Click( object sender, RoutedEventArgs e )
		{
			SetManualCamera( SettingsDirector.CameraType.Close );
		}

		private void ControlPanel_Camera_Medium_Button_Click( object sender, RoutedEventArgs e )
		{
			SetManualCamera( SettingsDirector.CameraType.Medium );
		}

		private void ControlPanel_Camera_Far_Button_Click( object sender, RoutedEventArgs e )
		{
			SetManualCamera( SettingsDirector.CameraType.Far );
		}

		private void ControlPanel_Camera_VeryFar_Button_Click( object sender, RoutedEventArgs e )
		{
			SetManualCamera( SettingsDirector.CameraType.VeryFar );
		}

		private void ControlPanel_Camera_AutoCam_Button_Click( object sender, RoutedEventArgs e )
		{
			SetManualCamera( SettingsDirector.CameraType.AutoCam );
		}

		private void ControlPanel_Camera_C1_Button_Click( object sender, RoutedEventArgs e )
		{
			SetManualCamera( SettingsDirector.CameraType.Custom1 );
		}

		private void ControlPanel_Camera_C2_Button_Click( object sender, RoutedEventArgs e )
		{
			SetManualCamera( SettingsDirector.CameraType.Custom2 );
		}

		private void ControlPanel_Camera_C3_Button_Click( object sender, RoutedEventArgs e )
		{
			SetManualCamera( SettingsDirector.CameraType.Custom3 );
		}

		private void ControlPanel_Camera_C4_Button_Click( object sender, RoutedEventArgs e )
		{
			SetManualCamera( SettingsDirector.CameraType.Custom4 );
		}

		private void ControlPanel_Camera_C5_Button_Click( object sender, RoutedEventArgs e )
		{
			SetManualCamera( SettingsDirector.CameraType.Custom5 );
		}

		private void ControlPanel_Camera_C6_Button_Click( object sender, RoutedEventArgs e )
		{
			SetManualCamera( SettingsDirector.CameraType.Custom6 );
		}

		private void ControlPanel_Grid_Button_Click( object sender, RoutedEventArgs e )
		{
			if ( IRSDK.targetCamEnabled )
			{
				var button = (Button) sender;

				int carIdx = ( button.Name == "PACE" ) ? 0 : int.Parse( button.Name[ 1.. ] );

				normalizedCar = IRSDK.normalizedData.FindNormalizedCarByCarIdx( carIdx );

				SetManualCamera( cameraType );
			}
		}

		private void ControlPanel_ToggleButton_Updated( object sender, RoutedEventArgs e )
		{
			masterOn = ControlPanel_Master_Button.IsChecked ?? false;
			raceStatusOn = ControlPanel_RaceStatus_Button.IsChecked ?? false;
			leaderboardOn = ControlPanel_Leaderboard_Button.IsChecked ?? false;
			trackMapOn = ControlPanel_TrackMap_Button.IsChecked ?? false;
			pitLaneOn = ControlPanel_PitLane_Button.IsChecked ?? false;
			startLightsOn = ControlPanel_StartLights_Button.IsChecked ?? false;
			voiceOfOn = ControlPanel_VoiceOf_Button.IsChecked ?? false;
			chyronOn = ControlPanel_Chyron_Button.IsChecked ?? false;
			subtitlesOn = ControlPanel_Subtitles_Button.IsChecked ?? false;
			introOn = ControlPanel_Intro_Button.IsChecked ?? false;

			customLayerOn[ 0 ] = ControlPanel_C1_Button.IsChecked ?? false;
			customLayerOn[ 1 ] = ControlPanel_C2_Button.IsChecked ?? false;
			customLayerOn[ 2 ] = ControlPanel_C3_Button.IsChecked ?? false;
			customLayerOn[ 3 ] = ControlPanel_C4_Button.IsChecked ?? false;
			customLayerOn[ 4 ] = ControlPanel_C5_Button.IsChecked ?? false;
			customLayerOn[ 5 ] = ControlPanel_C6_Button.IsChecked ?? false;
		}

		// director

		private void Director_DirectorList_SelectionChanged( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				Settings.directorLocal = (SettingsDirector) DirectorList.Items.GetItemAt( DirectorList.SelectedIndex );

				Settings.editor.lastActiveDirectorFilePath = Settings.directorLocal.filePath;

				Settings.saveEditorToFileQueued = true;

				Update();
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
			var directorFilePath = Settings.GetRelativePath( Settings.directorSettingsFolder + "My new director.xml" );

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

				Settings.saveDirectorToFileQueued = true;

				Settings.editor.lastActiveDirectorFilePath = Settings.directorLocal.filePath;

				Settings.saveEditorToFileQueued = true;

				Update();
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

					Settings.saveEditorToFileQueued = true;

					Update();
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

		private void Director_Cameras_Scenic_Button_Click( object sender, EventArgs e )
		{
			var cameraSelector = new CameraSelector( Director_Cameras_Scenic )
			{
				Owner = this
			};

			cameraSelector.ShowDialog();
		}

		private void Director_Cameras_Pits_Button_Click( object sender, EventArgs e )
		{
			var cameraSelector = new CameraSelector( Director_Cameras_Pits )
			{
				Owner = this
			};

			cameraSelector.ShowDialog();
		}

		private void Director_Cameras_StartFinish_Button_Click( object sender, EventArgs e )
		{
			var cameraSelector = new CameraSelector( Director_Cameras_StartFinish )
			{
				Owner = this
			};

			cameraSelector.ShowDialog();
		}

		private void Director_Cameras_Reverse_Button_Click( object sender, EventArgs e )
		{
			var cameraSelector = new CameraSelector( Director_Cameras_Reverse )
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

		private void Director_Cameras_Custom1_Button_Click( object sender, EventArgs e )
		{
			var cameraSelector = new CameraSelector( Director_Cameras_Custom1 )
			{
				Owner = this
			};

			cameraSelector.ShowDialog();
		}

		private void Director_Cameras_Custom2_Button_Click( object sender, EventArgs e )
		{
			var cameraSelector = new CameraSelector( Director_Cameras_Custom2 )
			{
				Owner = this
			};

			cameraSelector.ShowDialog();
		}

		private void Director_Cameras_Custom3_Button_Click( object sender, EventArgs e )
		{
			var cameraSelector = new CameraSelector( Director_Cameras_Custom3 )
			{
				Owner = this
			};

			cameraSelector.ShowDialog();
		}

		private void Director_Cameras_Custom4_Button_Click( object sender, EventArgs e )
		{
			var cameraSelector = new CameraSelector( Director_Cameras_Custom4 )
			{
				Owner = this
			};

			cameraSelector.ShowDialog();
		}

		private void Director_Cameras_Custom5_Button_Click( object sender, EventArgs e )
		{
			var cameraSelector = new CameraSelector( Director_Cameras_Custom5 )
			{
				Owner = this
			};

			cameraSelector.ShowDialog();
		}

		private void Director_Cameras_Custom6_Button_Click( object sender, EventArgs e )
		{
			var cameraSelector = new CameraSelector( Director_Cameras_Custom6 )
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

					Update();
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

					Update();
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

					Update();
				}
				else
				{
					var director = Settings.directorLocal.camerasIntro_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.camerasIntro = Director_Cameras_Intro.Text;
				}

				overridden = Director_Cameras_Scenic_Override.IsChecked ?? false;

				if ( Settings.directorLocal.camerasScenic_Overridden != overridden )
				{
					Settings.directorLocal.camerasScenic_Overridden = overridden;

					Update();
				}
				else
				{
					var director = Settings.directorLocal.camerasScenic_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.camerasScenic = Director_Cameras_Scenic.Text;
				}

				overridden = Director_Cameras_Pits_Override.IsChecked ?? false;

				if ( Settings.directorLocal.camerasPits_Overridden != overridden )
				{
					Settings.directorLocal.camerasPits_Overridden = overridden;

					Update();
				}
				else
				{
					var director = Settings.directorLocal.camerasPits_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.camerasPits = Director_Cameras_Pits.Text;
				}

				overridden = Director_Cameras_StartFinish_Override.IsChecked ?? false;

				if ( Settings.directorLocal.camerasStartFinish_Overridden != overridden )
				{
					Settings.directorLocal.camerasStartFinish_Overridden = overridden;

					Update();
				}
				else
				{
					var director = Settings.directorLocal.camerasStartFinish_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.camerasStartFinish = Director_Cameras_StartFinish.Text;
				}

				overridden = Director_Cameras_Reverse_Override.IsChecked ?? false;

				if ( Settings.directorLocal.camerasReverse_Overridden != overridden )
				{
					Settings.directorLocal.camerasReverse_Overridden = overridden;

					Update();
				}
				else
				{
					var director = Settings.directorLocal.camerasReverse_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.camerasReverse = Director_Cameras_Reverse.Text;
				}

				overridden = Director_Cameras_Inside_Override.IsChecked ?? false;

				if ( Settings.directorLocal.camerasInside_Overridden != overridden )
				{
					Settings.directorLocal.camerasInside_Overridden = overridden;

					Update();
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

					Update();
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

					Update();
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

					Update();
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

					Update();
				}
				else
				{
					var director = Settings.directorLocal.camerasVeryFar_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.camerasVeryFar = Director_Cameras_VeryFar.Text;
				}

				overridden = Director_Cameras_Custom1_Override.IsChecked ?? false;

				if ( Settings.directorLocal.camerasCustom1_Overridden != overridden )
				{
					Settings.directorLocal.camerasCustom1_Overridden = overridden;

					Update();
				}
				else
				{
					var director = Settings.directorLocal.camerasCustom1_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.camerasCustom1 = Director_Cameras_Custom1.Text;
				}

				overridden = Director_Cameras_Custom2_Override.IsChecked ?? false;

				if ( Settings.directorLocal.camerasCustom2_Overridden != overridden )
				{
					Settings.directorLocal.camerasCustom2_Overridden = overridden;

					Update();
				}
				else
				{
					var director = Settings.directorLocal.camerasCustom2_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.camerasCustom2 = Director_Cameras_Custom2.Text;
				}

				overridden = Director_Cameras_Custom3_Override.IsChecked ?? false;

				if ( Settings.directorLocal.camerasCustom3_Overridden != overridden )
				{
					Settings.directorLocal.camerasCustom3_Overridden = overridden;

					Update();
				}
				else
				{
					var director = Settings.directorLocal.camerasCustom3_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.camerasCustom3 = Director_Cameras_Custom3.Text;
				}

				overridden = Director_Cameras_Custom4_Override.IsChecked ?? false;

				if ( Settings.directorLocal.camerasCustom4_Overridden != overridden )
				{
					Settings.directorLocal.camerasCustom4_Overridden = overridden;

					Update();
				}
				else
				{
					var director = Settings.directorLocal.camerasCustom4_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.camerasCustom4 = Director_Cameras_Custom4.Text;
				}

				overridden = Director_Cameras_Custom5_Override.IsChecked ?? false;

				if ( Settings.directorLocal.camerasCustom5_Overridden != overridden )
				{
					Settings.directorLocal.camerasCustom5_Overridden = overridden;

					Update();
				}
				else
				{
					var director = Settings.directorLocal.camerasCustom5_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.camerasCustom5 = Director_Cameras_Custom5.Text;
				}

				overridden = Director_Cameras_Custom6_Override.IsChecked ?? false;

				if ( Settings.directorLocal.camerasCustom6_Overridden != overridden )
				{
					Settings.directorLocal.camerasCustom6_Overridden = overridden;

					Update();
				}
				else
				{
					var director = Settings.directorLocal.camerasCustom6_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.camerasCustom6 = Director_Cameras_Custom6.Text;
				}

				overridden = Director_SwitchDelay_Director_Override.IsChecked ?? false;

				if ( Settings.directorLocal.switchDelayDirector_Overridden != overridden )
				{
					Settings.directorLocal.switchDelayDirector_Overridden = overridden;

					Update();
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

					Update();
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

					Update();
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

					Update();
				}
				else
				{
					var director = Settings.directorLocal.switchDelayNotInRace_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.switchDelayNotInRace = Director_SwitchDelay_NotInRace.Value;
				}

				overridden = Director_Heat_MaxGapTime_Override.IsChecked ?? false;

				if ( Settings.directorLocal.heatMaxGapTime_Overridden != overridden )
				{
					Settings.directorLocal.heatMaxGapTime_Overridden = overridden;

					Update();
				}
				else
				{
					var director = Settings.directorLocal.heatMaxGapTime_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.heatMaxGapTime = Director_Heat_MaxGapTime.Value;
				}

				overridden = Director_Heat_OvertakeBonus_Override.IsChecked ?? false;

				if ( Settings.directorLocal.heatOvertakeBonus_Overridden != overridden )
				{
					Settings.directorLocal.heatOvertakeBonus_Overridden = overridden;

					Update();
				}
				else
				{
					var director = Settings.directorLocal.heatOvertakeBonus_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.heatOvertakeBonus = Director_Heat_OvertakeBonus.Value;
				}

				overridden = Director_Heat_PositionBattle_Override.IsChecked ?? false;

				if ( Settings.directorLocal.heatPositionBattle_Overridden != overridden )
				{
					Settings.directorLocal.heatPositionBattle_Overridden = overridden;

					Update();
				}
				else
				{
					var director = Settings.directorLocal.heatPositionBattle_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.heatPositionBattle = Director_Heat_PositionBattle.Value;
				}

				overridden = Director_Heat_Bias_Override.IsChecked ?? false;

				if ( Settings.directorLocal.heatBias_Overridden != overridden )
				{
					Settings.directorLocal.heatBias_Overridden = overridden;

					Update();
				}
				else
				{
					var director = Settings.directorLocal.heatBias_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.heatBias = (float) Director_Heat_Bias.Value;
				}

				overridden = Director_PreferredCar_UserIds_Override.IsChecked ?? false;

				if ( Settings.directorLocal.preferredCarUserIds_Overridden != overridden )
				{
					Settings.directorLocal.preferredCarUserIds_Overridden = overridden;

					Update();
				}
				else
				{
					var director = Settings.directorLocal.preferredCarUserIds_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.preferredCarUserIds = Director_PreferredCar_UserIds.Text;
				}

				overridden = Director_PreferredCar_CarNumbers_Override.IsChecked ?? false;

				if ( Settings.directorLocal.preferredCarCarNumbers_Overridden != overridden )
				{
					Settings.directorLocal.preferredCarCarNumbers_Overridden = overridden;

					Update();
				}
				else
				{
					var director = Settings.directorLocal.preferredCarCarNumbers_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.preferredCarCarNumbers = Director_PreferredCar_CarNumbers.Text;
				}

				overridden = Director_PreferredCar_LockOnEnabled_Override.IsChecked ?? false;

				if ( Settings.directorLocal.preferredCarLockOnEnabled_Overridden != overridden )
				{
					Settings.directorLocal.preferredCarLockOnEnabled_Overridden = overridden;

					Update();
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

					Update();
				}
				else
				{
					var director = Settings.directorLocal.preferredCarLockOnMinimumHeat_Overridden ? Settings.directorLocal : Settings.directorGlobal;

					director.preferredCarLockOnMinimumHeat = Director_PreferredCar_LockOnMinimumHeat.Value;
				}

				Settings.saveDirectorToFileQueued = true;
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

					Update();
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

					director.rule14_Enabled = Director_Rules_Rule13_Enabled.IsChecked ?? false;
					director.rule14_Camera = (SettingsDirector.CameraType) Director_Rules_Rule13_Camera.SelectedItem;
				}

				Settings.saveDirectorToFileQueued = true;
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

					Update();
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

				Settings.saveDirectorToFileQueued = true;
			}
		}

		// session flags

		private void SessionFlags_Edit_Button_Click( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var button = (Button) sender;

				var listViewItem = FindVisualParent<ListViewItem>( button );

				var item = (SessionFlagsData) SessionFlags_ListView.ItemContainerGenerator.ItemFromContainer( listViewItem );

				var editSessionFlagsData = new EditSessionFlagsData( item )
				{
					Owner = this
				};

				editSessionFlagsData.ShowDialog();
			}
		}

		private void SessionFlags_AddAtCurrentFrame_Button_Click( object sender, EventArgs e )
		{
			if ( !IRSDK.isConnected )
			{
				MessageBox.Show( this, "iRacing is not running.", "Not Yet", MessageBoxButton.OK, MessageBoxImage.Exclamation );

				return;
			}

			if ( !IRSDK.normalizedSession.isReplay )
			{
				MessageBox.Show( this, "Sorry, you should add session flags manually only during replays.", "Not In Replay", MessageBoxButton.OK, MessageBoxImage.Exclamation );

				return;
			}

			SessionFlagsPlayback.AddAtCurrentFrame( 0 );
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

				IncidentPlayback.saveToFileQueued = true;

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

				IncidentPlayback.saveToFileQueued = true;

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

				IncidentPlayback.saveToFileQueued = true;
			}
		}

		private void Incidents_ListView_MouseDoubleClick( object sender, RoutedEventArgs e )
		{
			if ( ( (FrameworkElement) e.OriginalSource ).DataContext is IncidentData item )
			{
				Director.isEnabled = false;

				IRSDK.targetCamEnabled = true;
				IRSDK.targetCamFastSwitchEnabled = true;
				IRSDK.targetCamCarIdx = item.CarIdx;
				IRSDK.targetCamGroupNumber = IRSDK.GetCamGroupNumber( Settings.editor.editorIncidentsEditCameras, false );

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

				SubtitlePlayback.saveToFileQueued = true;
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

				SubtitlePlayback.saveToFileQueued = true;
			}
		}

		private void Subtitles_ListView_MouseDoubleClick( object sender, RoutedEventArgs e )
		{
			// TODO: Needs fixing to work with session times instead of frame numbers

			/*
			if ( ( (FrameworkElement) e.OriginalSource ).DataContext is SubtitleData item )
			{
				IRSDK.targetReplayStartFrameNumberEnabled = true;
				IRSDK.targetReplayStartFrameNumber = item.StartTime;

				if ( item.EndTime != item.StartTime )
				{
					IRSDK.targetReplayStartPlaying = true;

					IRSDK.targetReplayStopFrameNumberEnabled = true;
					IRSDK.targetReplayStopFrameNumber = item.EndTime;
				}
				else
				{
					IRSDK.targetReplayStartPlaying = false;
				}
			}
			*/
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

		private void Overlay_ShowBorders_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				Settings.overlayLocal.showBorders = Overlay_ShowBorders.IsChecked ?? false;

				Settings.saveOverlayToFileQueued = true;

				Update();

				IPC.readyToSendSettings = true;
			}
		}

		private void Overlay_OverlayList_SelectionChanged( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				Settings.overlayLocal = (SettingsOverlay) OverlayList.Items.GetItemAt( OverlayList.SelectedIndex );

				Settings.editor.lastActiveOverlayFilePath = Settings.overlayLocal.filePath;

				Settings.saveEditorToFileQueued = true;

				Update();

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
			var overlayFilePath = Settings.GetRelativePath( Settings.overlaySettingsFolder + "My new overlay.xml" );

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

				Settings.FixSettings( Settings.overlayLocal );

				Settings.overlayList.Add( Settings.overlayLocal );

				Settings.saveOverlayToFileQueued = true;

				Settings.editor.lastActiveOverlayFilePath = Settings.overlayLocal.filePath;

				Settings.saveEditorToFileQueued = true;

				Update();
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

					Settings.saveEditorToFileQueued = true;

					Update();
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

					Update();
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

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.size_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.size.x = Overlay_General_Size_W.Value;
					overlay.size.y = Overlay_General_Size_H.Value;
				}

				IPC.readyToSendSettings = true;

				Settings.saveOverlayToFileQueued = true;
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

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.fontNames_Overridden[ 0 ] ? Settings.overlayLocal : Settings.overlayGlobal;

					if ( Overlay_Font_FontA_Name.SelectedItem == null )
					{
						overlay.fontNames[ 0 ] = string.Empty;
						overlay.fontPaths[ 0 ] = string.Empty;
					}
					else
					{
						var fontName = (string) Overlay_Font_FontA_Name.SelectedItem;

						overlay.fontNames[ 0 ] = fontName;
						overlay.fontPaths[ 0 ] = fontOptions[ fontName ];
					}
				}

				overridden = Overlay_Font_FontB_Name_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.fontNames_Overridden[ 1 ] != overridden )
				{
					Settings.overlayLocal.fontNames_Overridden[ 1 ] = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.fontNames_Overridden[ 1 ] ? Settings.overlayLocal : Settings.overlayGlobal;

					if ( Overlay_Font_FontB_Name.SelectedItem == null )
					{
						overlay.fontNames[ 1 ] = string.Empty;
						overlay.fontPaths[ 1 ] = string.Empty;
					}
					else
					{
						var fontName = (string) Overlay_Font_FontB_Name.SelectedItem;

						overlay.fontNames[ 1 ] = fontName;
						overlay.fontPaths[ 1 ] = fontOptions[ fontName ];
					}
				}

				overridden = Overlay_Font_FontC_Name_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.fontNames_Overridden[ 2 ] != overridden )
				{
					Settings.overlayLocal.fontNames_Overridden[ 2 ] = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.fontNames_Overridden[ 2 ] ? Settings.overlayLocal : Settings.overlayGlobal;

					if ( Overlay_Font_FontC_Name.SelectedItem == null )
					{
						overlay.fontNames[ 2 ] = string.Empty;
						overlay.fontPaths[ 2 ] = string.Empty;
					}
					else
					{
						var fontName = (string) Overlay_Font_FontC_Name.SelectedItem;

						overlay.fontNames[ 2 ] = fontName;
						overlay.fontPaths[ 2 ] = fontOptions[ fontName ];
					}
				}

				overridden = Overlay_Font_FontD_Name_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.fontNames_Overridden[ 3 ] != overridden )
				{
					Settings.overlayLocal.fontNames_Overridden[ 3 ] = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.fontNames_Overridden[ 3 ] ? Settings.overlayLocal : Settings.overlayGlobal;

					if ( Overlay_Font_FontD_Name.SelectedItem == null )
					{
						overlay.fontNames[ 3 ] = string.Empty;
						overlay.fontPaths[ 3 ] = string.Empty;
					}
					else
					{
						var fontName = (string) Overlay_Font_FontD_Name.SelectedItem;

						overlay.fontNames[ 3 ] = fontName;
						overlay.fontPaths[ 3 ] = fontOptions[ fontName ];
					}
				}

				IPC.readyToSendSettings = true;

				Settings.saveOverlayToFileQueued = true;
			}
		}

		private void Overlay_Image_ID_SelectionChanged( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				UpdateOverlayImage();
			}
		}

		private void Overlay_Image_FilePath_Button_Click( object sender, EventArgs e )
		{
			string currentFilePath = Image_FilePath.Text;

			currentFilePath = Settings.GetFullPath( currentFilePath );

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
				Image_FilePath.Text = Settings.GetRelativePath( openFileDialog.FileName );
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
				var id = (string) Overlay_Image_ID.SelectedItem;

				var overlaySettings = Settings.overlayLocal.imageSettingsDataDictionary[ id ];
				var globalSettings = Settings.overlayGlobal.imageSettingsDataDictionary[ id ];

				var overridden = Image_ImageType_Override.IsChecked ?? false;

				if ( overlaySettings.imageType_Overridden != overridden )
				{
					overlaySettings.imageType_Overridden = overridden;

					UpdateOverlayImage();
				}
				else
				{
					var settings = overlaySettings.imageType_Overridden ? overlaySettings : globalSettings;

					settings.imageType = (SettingsImage.ImageType) Image_ImageType.SelectedItem;
				}

				{
					var settings = overlaySettings.imageType_Overridden ? overlaySettings : globalSettings;

					if ( id == "CustomLayer1" )
					{
						ControlPanel_C1_Button.IsChecked = customLayerOn[ 0 ] = settings.imageType != SettingsImage.ImageType.None;
					}

					if ( id == "CustomLayer2" )
					{
						ControlPanel_C2_Button.IsChecked = customLayerOn[ 1 ] = settings.imageType != SettingsImage.ImageType.None;
					}

					if ( id == "CustomLayer3" )
					{
						ControlPanel_C3_Button.IsChecked = customLayerOn[ 2 ] = settings.imageType != SettingsImage.ImageType.None;
					}

					if ( id == "CustomLayer4" )
					{
						ControlPanel_C4_Button.IsChecked = customLayerOn[ 3 ] = settings.imageType != SettingsImage.ImageType.None;
					}

					if ( id == "CustomLayer5" )
					{
						ControlPanel_C5_Button.IsChecked = customLayerOn[ 4 ] = settings.imageType != SettingsImage.ImageType.None;
					}

					if ( id == "CustomLayer6" )
					{
						ControlPanel_C6_Button.IsChecked = customLayerOn[ 5 ] = settings.imageType != SettingsImage.ImageType.None;
					}
				}

				overridden = Image_FilePath_Override.IsChecked ?? false;

				if ( overlaySettings.filePath_Overridden != overridden )
				{
					overlaySettings.filePath_Overridden = overridden;

					UpdateOverlayImage();
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

					UpdateOverlayImage();
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

					UpdateOverlayImage();
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

					UpdateOverlayImage();
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

					UpdateOverlayImage();
				}
				else
				{
					var settings = overlaySettings.border_Overridden ? overlaySettings : globalSettings;

					settings.border = new Vector4( Image_Border_L.Value, Image_Border_T.Value, Image_Border_R.Value, Image_Border_B.Value );
				}

				overridden = Image_Frames_Override.IsChecked ?? false;

				if ( overlaySettings.frames_Overridden != overridden )
				{
					overlaySettings.frames_Overridden = overridden;

					UpdateOverlayImage();
				}
				else
				{
					var settings = overlaySettings.frames_Overridden ? overlaySettings : globalSettings;

					settings.frameSize = new Vector2( Image_Frames_W.Value, Image_Frames_H.Value );
					settings.frameCount = Image_Frames_Count.Value;
				}

				overridden = Image_AnimationSpeed_Override.IsChecked ?? false;

				if ( overlaySettings.animationSpeed_Overridden != overridden )
				{
					overlaySettings.animationSpeed_Overridden = overridden;

					UpdateOverlayImage();
				}
				else
				{
					var settings = overlaySettings.animationSpeed_Overridden ? overlaySettings : globalSettings;

					settings.animationSpeed = Image_AnimationSpeed.Value;
				}

				overridden = Image_TilingEnabled_Override.IsChecked ?? false;

				if ( overlaySettings.tilingEnabled_Overridden != overridden )
				{
					overlaySettings.tilingEnabled_Overridden = overridden;

					UpdateOverlayImage();
				}
				else
				{
					var settings = overlaySettings.tilingEnabled_Overridden ? overlaySettings : globalSettings;

					settings.tilingEnabled = Image_TilingEnabled.IsChecked ?? false;
				}

				overridden = Image_UseClassColors_Override.IsChecked ?? false;

				if ( overlaySettings.useClassColors_Overridden != overridden )
				{
					overlaySettings.useClassColors_Overridden = overridden;

					UpdateOverlayImage();
				}
				else
				{
					var settings = overlaySettings.useClassColors_Overridden ? overlaySettings : globalSettings;

					settings.useClassColors = Image_UseClassColors.IsChecked ?? false;
				}

				overridden = Image_ClassColorStrength_Override.IsChecked ?? false;

				if ( overlaySettings.classColorStrength_Overridden != overridden )
				{
					overlaySettings.classColorStrength_Overridden = overridden;

					UpdateOverlayImage();
				}
				else
				{
					var settings = overlaySettings.classColorStrength_Overridden ? overlaySettings : globalSettings;

					settings.classColorStrength = (float) ( Image_ClassColorStrength.Value / 255.0f );
				}

				IPC.readyToSendSettings = true;

				Settings.saveOverlayToFileQueued = true;
			}
		}

		private void Overlay_Text_ID_SelectionChanged( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				UpdateOverlayText();
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

					UpdateOverlayText();
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

					UpdateOverlayText();
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

					UpdateOverlayText();
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

					UpdateOverlayText();
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

					UpdateOverlayText();
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

					UpdateOverlayText();
				}
				else
				{
					var settings = overlaySettings.tintColor_Overridden ? overlaySettings : globalSettings;

					settings.tintColor = new Color( Text_TintColor_R.Value, Text_TintColor_G.Value, Text_TintColor_B.Value, Text_TintColor_A.Value );
				}

				overridden = Text_UseClassColors_Override.IsChecked ?? false;

				if ( overlaySettings.useClassColors_Overridden != overridden )
				{
					overlaySettings.useClassColors_Overridden = overridden;

					UpdateOverlayText();
				}
				else
				{
					var settings = overlaySettings.useClassColors_Overridden ? overlaySettings : globalSettings;

					settings.useClassColors = Text_UseClassColors.IsChecked ?? false;
				}

				overridden = Text_ClassColorStrength_Override.IsChecked ?? false;

				if ( overlaySettings.classColorStrength_Overridden != overridden )
				{
					overlaySettings.classColorStrength_Overridden = overridden;

					UpdateOverlayText();
				}
				else
				{
					var settings = overlaySettings.classColorStrength_Overridden ? overlaySettings : globalSettings;

					settings.classColorStrength = (float) ( Text_ClassColorStrength.Value / 255.0f );
				}

				overridden = Text_AllowOverflow_Override.IsChecked ?? false;

				if ( overlaySettings.allowOverflow_Overridden != overridden )
				{
					overlaySettings.allowOverflow_Overridden = overridden;

					UpdateOverlayText();
				}
				else
				{
					var settings = overlaySettings.allowOverflow_Overridden ? overlaySettings : globalSettings;

					settings.allowOverflow = Text_AllowOverflow.IsChecked ?? false;
				}

				IPC.readyToSendSettings = true;

				Settings.saveOverlayToFileQueued = true;
			}
		}

		private void Overlay_StartLights_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overridden = Overlay_StartLights_Enable_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.startLightsEnabled_Overridden != overridden )
				{
					Settings.overlayLocal.startLightsEnabled_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.startLightsEnabled_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.startLightsEnabled = Overlay_StartLights_Enable.IsChecked ?? false;
				}

				overridden = Overlay_StartLights_Position_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.startLightsPosition_Overridden != overridden )
				{
					Settings.overlayLocal.startLightsPosition_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.startLightsPosition_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.startLightsPosition = new Vector2( Overlay_StartLights_Position_X.Value, Overlay_StartLights_Position_Y.Value );
				}

				IPC.readyToSendSettings = true;

				Settings.saveOverlayToFileQueued = true;
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

					Update();
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

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.raceStatusPosition_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.raceStatusPosition = new Vector2( Overlay_RaceStatus_Position_X.Value, Overlay_RaceStatus_Position_Y.Value );
				}

				IPC.readyToSendSettings = true;

				Settings.saveOverlayToFileQueued = true;
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

					Update();
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

					Update();
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

					Update();
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

					Update();
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

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.leaderboardSlotSpacing_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.leaderboardSlotSpacing = new Vector2( Overlay_Leaderboard_SlotSpacing_X.Value, Overlay_Leaderboard_SlotSpacing_Y.Value );
				}

				overridden = Overlay_Leaderboard_SeparateBoards_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.leaderboardSeparateBoards_Overridden != overridden )
				{
					Settings.overlayLocal.leaderboardSeparateBoards_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.leaderboardSeparateBoards_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.leaderboardSeparateBoards = Overlay_Leaderboard_SeparateBoards.IsChecked ?? false;
				}

				overridden = Overlay_Leaderboard_MultiClassOffset_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.leaderboardMultiClassOffset_Overridden != overridden )
				{
					Settings.overlayLocal.leaderboardMultiClassOffset_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.leaderboardMultiClassOffset_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.leaderboardMultiClassOffset = new Vector2( Overlay_Leaderboard_MultiClassOffset_X.Value, Overlay_Leaderboard_MultiClassOffset_Y.Value );
					overlay.leaderboardMultiClassOffsetType = leaderboardMultiClassOffsetTypeOptions[ (string) Overlay_Leaderboard_MultiClassOffset_Type.SelectedItem ];
				}

				IPC.readyToSendSettings = true;

				Settings.saveOverlayToFileQueued = true;
			}
		}

		private void Overlay_TrackMap_TextureFilePath_Button_Click( object sender, EventArgs e )
		{
			string currentFilePath = Overlay_TrackMap_TextureFilePath.Text;

			currentFilePath = Settings.GetFullPath( currentFilePath );

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
				Overlay_TrackMap_TextureFilePath.Text = Settings.GetRelativePath( openFileDialog.FileName );
			}
		}

		private void Overlay_TrackMap_LineColor_Palette_Click( object sender, EventArgs e )
		{
			var color = new System.Windows.Media.Color()
			{
				ScR = Overlay_TrackMap_LineColor_R.Value,
				ScG = Overlay_TrackMap_LineColor_G.Value,
				ScB = Overlay_TrackMap_LineColor_B.Value,
				ScA = Overlay_TrackMap_LineColor_A.Value
			};

			var colorPickerDialog = new ColorPickerDialog( color )
			{
				Owner = this
			};

			var result = colorPickerDialog.ShowDialog();

			if ( result.HasValue && result.Value )
			{
				initializing++;

				Overlay_TrackMap_LineColor_R.Value = colorPickerDialog.Color.ScR;
				Overlay_TrackMap_LineColor_G.Value = colorPickerDialog.Color.ScG;
				Overlay_TrackMap_LineColor_B.Value = colorPickerDialog.Color.ScB;
				Overlay_TrackMap_LineColor_A.Value = colorPickerDialog.Color.ScA;

				initializing--;

				Overlay_TrackMap_Update( sender, e );
			}
		}

		private void Overlay_TrackMap_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overridden = Overlay_TrackMap_Enable_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.trackMapEnabled_Overridden != overridden )
				{
					Settings.overlayLocal.trackMapEnabled_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.trackMapEnabled_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.trackMapEnabled = Overlay_TrackMap_Enable.IsChecked ?? false;
				}

				overridden = Overlay_TrackMap_Reverse_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.trackMapReverse_Overridden != overridden )
				{
					Settings.overlayLocal.trackMapReverse_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.trackMapReverse_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.trackMapReverse = Overlay_TrackMap_Reverse.IsChecked ?? false;
				}

				overridden = Overlay_TrackMap_Position_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.trackMapPosition_Overridden != overridden )
				{
					Settings.overlayLocal.trackMapPosition_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.trackMapPosition_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.trackMapPosition.x = Overlay_TrackMap_Position_X.Value;
					overlay.trackMapPosition.y = Overlay_TrackMap_Position_Y.Value;
				}

				overridden = Overlay_TrackMap_Size_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.trackMapSize_Overridden != overridden )
				{
					Settings.overlayLocal.trackMapSize_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.trackMapSize_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.trackMapSize.x = Overlay_TrackMap_Size_W.Value;
					overlay.trackMapSize.y = Overlay_TrackMap_Size_H.Value;
				}

				overridden = Overlay_TrackMap_TextureFilePath_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.trackMapTextureFilePath_Overridden != overridden )
				{
					Settings.overlayLocal.trackMapTextureFilePath_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.trackMapTextureFilePath_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.trackMapTextureFilePath = Overlay_TrackMap_TextureFilePath.Text;
				}

				overridden = Overlay_TrackMap_LineThickness_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.trackMapLineThickness_Overridden != overridden )
				{
					Settings.overlayLocal.trackMapLineThickness_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.trackMapLineThickness_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.trackMapLineThickness = Overlay_TrackMap_LineThickness.Value;
				}

				overridden = Overlay_TrackMap_LineColor_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.trackMapLineColor_Overridden != overridden )
				{
					Settings.overlayLocal.trackMapLineColor_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.trackMapLineColor_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.trackMapLineColor.r = Overlay_TrackMap_LineColor_R.Value;
					overlay.trackMapLineColor.g = Overlay_TrackMap_LineColor_G.Value;
					overlay.trackMapLineColor.b = Overlay_TrackMap_LineColor_B.Value;
					overlay.trackMapLineColor.a = Overlay_TrackMap_LineColor_A.Value;
				}

				overridden = Overlay_TrackMap_StartFinishOffset_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.trackMapStartFinishOffset_Overridden != overridden )
				{
					Settings.overlayLocal.trackMapStartFinishOffset_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.trackMapStartFinishOffset_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.trackMapStartFinishOffset = (int) Overlay_TrackMap_StartFinishOffset.Value;
				}

				IPC.readyToSendSettings = true;

				Settings.saveOverlayToFileQueued = true;
			}
		}

		private void Overlay_PitLane_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overridden = Overlay_PitLane_Enable_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.pitLaneEnabled_Overridden != overridden )
				{
					Settings.overlayLocal.pitLaneEnabled_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.pitLaneEnabled_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.pitLaneEnabled = Overlay_PitLane_Enable.IsChecked ?? false;
				}

				overridden = Overlay_PitLane_Position_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.pitLanePosition_Overridden != overridden )
				{
					Settings.overlayLocal.pitLanePosition_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.pitLanePosition_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.pitLanePosition.x = Overlay_PitLane_Position_X.Value;
					overlay.pitLanePosition.y = Overlay_PitLane_Position_Y.Value;
				}

				overridden = Overlay_PitLane_Length_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.pitLaneLength_Overridden != overridden )
				{
					Settings.overlayLocal.pitLaneLength_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.pitLaneLength_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.pitLaneLength = Overlay_PitLane_Length.Value;
				}

				IPC.readyToSendSettings = true;

				Settings.saveOverlayToFileQueued = true;
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

					Update();
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

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.voiceOfPosition_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.voiceOfPosition = new Vector2( Overlay_VoiceOf_Position_X.Value, Overlay_VoiceOf_Position_Y.Value );
				}

				IPC.readyToSendSettings = true;

				Settings.saveOverlayToFileQueued = true;
			}
		}

		private void Overlay_Chyron_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overridden = Overlay_Chyron_Enable_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.chyronEnabled_Overridden != overridden )
				{
					Settings.overlayLocal.chyronEnabled_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.chyronEnabled_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.chyronEnabled = Overlay_Chyron_Enable.IsChecked ?? false;
				}

				overridden = Overlay_Chyron_Position_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.chyronPosition_Overridden != overridden )
				{
					Settings.overlayLocal.chyronPosition_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.chyronPosition_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.chyronPosition = new Vector2( Overlay_Chyron_Position_X.Value, Overlay_Chyron_Position_Y.Value );
				}

				IPC.readyToSendSettings = true;

				Settings.saveOverlayToFileQueued = true;
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

					Update();
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

					Update();
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

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.subtitleMaxSize_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.subtitleMaxSize = new Vector2( Overlay_Subtitle_MaxSize_W.Value, Overlay_Subtitle_MaxSize_H.Value );
				}

				overridden = Overlay_Subtitle_TextPadding_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.subtitleTextPadding_Overridden != overridden )
				{
					Settings.overlayLocal.subtitleTextPadding_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.subtitleTextPadding_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.subtitleTextPadding = new Vector2Int( Overlay_Subtitle_TextPadding_X.Value, Overlay_Subtitle_TextPadding_Y.Value );
				}

				IPC.readyToSendSettings = true;

				Settings.saveOverlayToFileQueued = true;
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

					Update();
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

					Update();
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

					Update();
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

					Update();
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

					Update();
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

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.carNumberSlant_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.carNumberSlant = slantOptions[ (string) Overlay_CarNumber_Slant.SelectedItem ];
				}

				IPC.readyToSendSettings = true;

				Settings.UpdateCombinedOverlay();

				Settings.saveOverlayToFileQueued = true;

				IRSDK.normalizedData.SessionUpdate();
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

					Update();
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

					Update();
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

					Update();
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

					Update();
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

				overridden = Overlay_Telemetry_ShowAsNegativeNumbers_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.telemetryShowAsNegativeNumbers_Overridden != overridden )
				{
					Settings.overlayLocal.telemetryShowAsNegativeNumbers_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.telemetryShowAsNegativeNumbers_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.telemetryShowAsNegativeNumbers = Overlay_Telemetry_ShowAsNegativeNumbers.IsChecked ?? false;
				}

				IPC.readyToSendSettings = true;

				Settings.saveOverlayToFileQueued = true;
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

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.introEnabled_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.introEnabled = Overlay_Intro_Enable.IsChecked ?? false;
				}

				overridden = Overlay_Intro_LeftPosition_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.introLeftPosition_Overridden != overridden )
				{
					Settings.overlayLocal.introLeftPosition_Overridden = overridden;

					Update();
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

					Update();
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

					Update();
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

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.introRightScale_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.introRightScale = Overlay_Intro_RightScale.Value;
				}

				overridden = Overlay_Intro_StartTime_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.introStartTime_Overridden != overridden )
				{
					Settings.overlayLocal.introStartTime_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.introStartTime_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.introLeftStartTime = Overlay_Intro_LeftStartTime.Value;
					overlay.introRightStartTime = Overlay_Intro_RightStartTime.Value;
				}

				overridden = Overlay_Intro_StartInterval_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.introStartInterval_Overridden != overridden )
				{
					Settings.overlayLocal.introStartInterval_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.introStartInterval_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.introStartInterval = Overlay_Intro_StartInterval.Value;
				}

				overridden = Overlay_Intro_InAnimationNumber_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.introInAnimationNumber_Overridden != overridden )
				{
					Settings.overlayLocal.introInAnimationNumber_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.introInAnimationNumber_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.introLeftInAnimationNumber = inAnimationOptions[ (string) Overlay_Intro_LeftInAnimationNumber.SelectedItem ];
					overlay.introRightInAnimationNumber = inAnimationOptions[ (string) Overlay_Intro_RightInAnimationNumber.SelectedItem ];
				}

				overridden = Overlay_Intro_OutAnimationNumber_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.introOutAnimationNumber_Overridden != overridden )
				{
					Settings.overlayLocal.introOutAnimationNumber_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.introOutAnimationNumber_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.introLeftOutAnimationNumber = outAnimationOptions[ (string) Overlay_Intro_LeftOutAnimationNumber.SelectedItem ];
					overlay.introRightOutAnimationNumber = outAnimationOptions[ (string) Overlay_Intro_RightOutAnimationNumber.SelectedItem ];
				}

				overridden = Overlay_Intro_InTime_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.introInTime_Overridden != overridden )
				{
					Settings.overlayLocal.introInTime_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.introInTime_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.introInTime = Overlay_Intro_InTime.Value;
				}

				overridden = Overlay_Intro_HoldTime_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.introHoldTime_Overridden != overridden )
				{
					Settings.overlayLocal.introHoldTime_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.introHoldTime_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.introHoldTime = Overlay_Intro_HoldTime.Value;
				}

				overridden = Overlay_Intro_OutTime_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.introOutTime_Overridden != overridden )
				{
					Settings.overlayLocal.introOutTime_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.introOutTime_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.introOutTime = Overlay_Intro_OutTime.Value;
				}

				IPC.readyToSendSettings = true;

				Settings.saveOverlayToFileQueued = true;
			}
		}

		private void Overlay_Hud_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overridden = Overlay_Hud_Enable_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.hudEnabled_Overridden != overridden )
				{
					Settings.overlayLocal.hudEnabled_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.hudEnabled_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.hudEnabled = Overlay_Hud_Enable.IsChecked ?? false;
				}

				overridden = Overlay_Hud_Position_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.hudPosition_Overridden != overridden )
				{
					Settings.overlayLocal.hudPosition_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.hudPosition_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.hudPosition = new Vector2( Overlay_Hud_Position_X.Value, Overlay_Hud_Position_Y.Value );
				}

				overridden = Overlay_Hud_SpeechToTextPosition_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.hudSpeechToTextPosition_Overridden != overridden )
				{
					Settings.overlayLocal.hudSpeechToTextPosition_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.hudSpeechToTextPosition_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.hudSpeechToTextPosition = new Vector2( Overlay_Hud_SpeechToTextPosition_X.Value, Overlay_Hud_SpeechToTextPosition_Y.Value );
				}

				overridden = Overlay_Hud_SpeechToTextMaxSize_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.hudSpeechToTextMaxSize_Overridden != overridden )
				{
					Settings.overlayLocal.hudSpeechToTextMaxSize_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.hudSpeechToTextMaxSize_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.hudSpeechToTextMaxSize = new Vector2( Overlay_Hud_SpeechToTextMaxSize_W.Value, Overlay_Hud_SpeechToTextMaxSize_H.Value );
				}

				overridden = Overlay_Hud_SpeechToTextTextPadding_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.hudSpeechToTextTextPadding_Overridden != overridden )
				{
					Settings.overlayLocal.hudSpeechToTextTextPadding_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.hudSpeechToTextTextPadding_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.hudSpeechToTextTextPadding = new Vector2Int( Overlay_Hud_SpeechToTextTextPadding_X.Value, Overlay_Hud_SpeechToTextTextPadding_Y.Value );
				}

				IPC.readyToSendSettings = true;

				Settings.saveOverlayToFileQueued = true;
			}
		}

		private void Overlay_Trainer_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				var overridden = Overlay_Trainer_Enable_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.trainerEnabled_Overridden != overridden )
				{
					Settings.overlayLocal.trainerEnabled_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.trainerEnabled_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.trainerEnabled = Overlay_Trainer_Enable.IsChecked ?? false;
				}

				overridden = Overlay_Trainer_Position_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.trainerPosition_Overridden != overridden )
				{
					Settings.overlayLocal.trainerPosition_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.trainerPosition_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.trainerPosition.x = Overlay_Trainer_Position_X.Value;
					overlay.trainerPosition.y = Overlay_Trainer_Position_Y.Value;
				}

				overridden = Overlay_Trainer_Size_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.trainerSize_Overridden != overridden )
				{
					Settings.overlayLocal.trainerSize_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.trainerSize_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.trainerSize.x = Overlay_Trainer_Size_W.Value;
					overlay.trainerSize.y = Overlay_Trainer_Size_H.Value;
				}

				overridden = Overlay_Trainer_SpeedScale_Override.IsChecked ?? false;

				if ( Settings.overlayLocal.trainerSpeedScale_Overridden != overridden )
				{
					Settings.overlayLocal.trainerSpeedScale_Overridden = overridden;

					Update();
				}
				else
				{
					var overlay = Settings.overlayLocal.trainerSpeedScale_Overridden ? Settings.overlayLocal : Settings.overlayGlobal;

					overlay.trainerSpeedScale = Overlay_Trainer_SpeedScale.Value;
				}

				IPC.readyToSendSettings = true;

				Settings.saveOverlayToFileQueued = true;
			}
		}

		// web page

		private void WebPage_General_SourceFolder_Button_Click( object sender, EventArgs e )
		{
			var commonOpenFileDialog = new CommonOpenFileDialog
			{
				Title = "Choose the Folder to Load Web Page Files From",
				IsFolderPicker = true,
				InitialDirectory = Settings.editor.webpageGeneralSourceFolder,
				AddToMostRecentlyUsedList = false,
				AllowNonFileSystemItems = false,
				DefaultDirectory = Settings.editor.webpageGeneralSourceFolder,
				EnsureFileExists = true,
				EnsurePathExists = true,
				EnsureReadOnly = false,
				EnsureValidNames = true,
				Multiselect = false,
				ShowPlacesList = true
			};

			if ( commonOpenFileDialog.ShowDialog() == CommonFileDialogResult.Ok )
			{
				Settings.editor.webpageGeneralSourceFolder = commonOpenFileDialog.FileName;

				WebPage_General_SourceFolder.Text = Settings.editor.webpageGeneralSourceFolder;
			}
		}

		private void WebPage_General_OutputFolder_Button_Click( object sender, EventArgs e )
		{
			var commonOpenFileDialog = new CommonOpenFileDialog
			{
				Title = "Choose the Folder to Save Web Page Files To",
				IsFolderPicker = true,
				InitialDirectory = Settings.editor.webpageGeneralOutputFolder,
				AddToMostRecentlyUsedList = false,
				AllowNonFileSystemItems = false,
				DefaultDirectory = Settings.editor.webpageGeneralOutputFolder,
				EnsureFileExists = true,
				EnsurePathExists = true,
				EnsureReadOnly = false,
				EnsureValidNames = true,
				Multiselect = false,
				ShowPlacesList = true
			};

			if ( commonOpenFileDialog.ShowDialog() == CommonFileDialogResult.Ok )
			{
				Settings.editor.webpageGeneralOutputFolder = commonOpenFileDialog.FileName;

				WebPage_General_OutputFolder.Text = Settings.editor.webpageGeneralOutputFolder;
			}
		}

		private void WebPage_General_ReloadTemplate( object sender, EventArgs e )
		{
			WebPage.Initialize();
		}

		private void WebPage_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				Settings.editor.webpageGeneralEnabled = WebPage_General_Enabled.IsChecked ?? false;
				Settings.editor.webpageGeneralSourceFolder = WebPage_General_SourceFolder.Text;
				Settings.editor.webpageGeneralOutputFolder = WebPage_General_OutputFolder.Text;

				Settings.saveEditorToFileQueued = true;
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

		private void iRacing_CustomPaintsAiRosterFile_Button_Click( object sender, EventArgs e )
		{
			string currentFilePath = iRacing_CustomPaints_AiRosterFile.Text;

			var openFileDialog = new OpenFileDialog()
			{
				Title = "Select an AI Roster File",
				Filter = "AI Roster Files (*.json)|*.json|All files (*.*)|*.*",
				InitialDirectory = ( currentFilePath == string.Empty ) ? Program.aiRosterFolder : Path.GetDirectoryName( currentFilePath ),
				FileName = currentFilePath,
				ValidateNames = true,
				CheckPathExists = true,
				CheckFileExists = true
			};

			if ( openFileDialog.ShowDialog() == true )
			{
				iRacing_CustomPaints_AiRosterFile.Text = openFileDialog.FileName;

				IRSDK.ReloadAiRoster();
			}
		}

		private void iRacing_DataApi_Connect( object sender, EventArgs e )
		{
			DataApi.Initialize( true );

			IRSDK.normalizedData.SessionUpdate();

			TrackMap.Initialize();
		}

		private void iRacing_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				Settings.editor.iracingGeneralCommandRateLimit = iRacing_General_CommandRateLimit.Value;

				Settings.editor.iracingAccountUsername = iRacing_Account_Username.Text;
				Settings.editor.iracingAccountPassword = iRacing_Account_Password.Password;

				Settings.editor.iracingCustomPaintsDirectory = iRacing_CustomPaints_Directory.Text;
				Settings.editor.iracingCustomPaintsAiRosterFile = iRacing_CustomPaints_AiRosterFile.Text;

				Settings.editor.iracingDriverNamesSuffixes = iRacing_DriverNames_Suffixes.Text;
				Settings.editor.iracingDriverNameFormatOption = formatOptions[ (string) iRacing_DriverNames_FormatOption.SelectedItem ];
				Settings.editor.iracingDriverNameCapitalizationOption = capitalizationOptions[ (string) iRacing_DriverNames_CapitalizationOption.SelectedItem ];

				Settings.saveEditorToFileQueued = true;

				IRSDK.normalizedData.SessionUpdate();
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

		private void Editor_Triggers_SessionChange_Button_Click( object sender, EventArgs e )
		{
			string currentFilePath = Editor_Triggers_SessionChange.Text;

			currentFilePath = Settings.GetFullPath( currentFilePath );

			var openFileDialog = new OpenFileDialog()
			{
				Title = "Select a Program to Run",
				Filter = "Program Files (*.exe)|*.exe|All files (*.*)|*.*",
				InitialDirectory = ( currentFilePath == string.Empty ) ? Program.documentsFolder : Path.GetDirectoryName( currentFilePath ),
				FileName = currentFilePath,
				ValidateNames = true,
				CheckPathExists = true,
				CheckFileExists = true
			};

			if ( openFileDialog.ShowDialog() == true )
			{
				Editor_Triggers_SessionChange.Text = Settings.GetRelativePath( openFileDialog.FileName );
			}
		}

		private void Editor_Update( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				Settings.editor.editorAlwaysOnTop = Editor_AlwaysOnTop.IsChecked ?? false;

				Settings.editor.editorMousePositioningSpeedNormal = Editor_Mouse_PositioningSpeedNormal.Value;
				Settings.editor.editorMousePositioningSpeedFast = Editor_Mouse_PositioningSpeedFast.Value;
				Settings.editor.editorMousePositioningSpeedSlow = Editor_Mouse_PositioningSpeedSlow.Value;

				Settings.editor.editorIncidentsScenicCameras = Editor_Incidents_ScenicCameras.Text;
				Settings.editor.editorIncidentsEditCameras = Editor_Incidents_EditCameras.Text;
				Settings.editor.editorIncidentsOverlapMergeTime = Editor_Incidents_OverlapMergeTime.Value;
				Settings.editor.editorIncidentsTimeout = Editor_Incidents_Timeout.Value;

				Settings.editor.editorSteamVrEnabled = Editor_SteamVr_Enabled.IsChecked ?? false;
				Settings.editor.editorSteamVrWidth = Editor_SteamVr_Width.Value;
				Settings.editor.editorSteamVrPosition = new Vector3( Editor_SteamVr_Position_X.Value, Editor_SteamVr_Position_Y.Value, Editor_SteamVr_Position_Z.Value );
				Settings.editor.editorSteamVrCurvature = Editor_SteamVr_Curvature.Value;

				if ( Editor_SteamVr_ControllerOption.SelectedItem != null )
				{
					Settings.editor.editorSteamVrControllerGuid = controllerOptions[ (string) Editor_SteamVr_ControllerOption.SelectedItem ];

					Controller.Initialize();
				}

				Settings.editor.editorSpeechToTextEnabled = Editor_SpeechToText_Enabled.IsChecked ?? false;
				Settings.editor.editorSpeechToTextCognitiveServiceKey = Editor_SpeechToText_CognitiveServicesKey.Text;
				Settings.editor.editorSpeechToTextCognitiveServiceRegion = Editor_SpeechToText_CognitiveServicesRegion.Text;
				Settings.editor.editorSpeechToTextLanguage = Editor_SpeechToText_Language.Text;
				Settings.editor.editorSpeechToTextPotatoFilterEnabled = Editor_SpeechToText_PotatoFilterEnabled.IsChecked ?? false;

				if ( Editor_SpeechToText_AudioCaptureDeviceOption.SelectedItem != null )
				{
					Settings.editor.editorSpeechToTextAudioCaptureDeviceId = audioCaptureDeviceOptions[ (string) Editor_SpeechToText_AudioCaptureDeviceOption.SelectedItem ];
				}

				SpeechToText.Initialize();

				Settings.editor.editorPushToTalkMuteEnabled = Editor_PushToTalk_MuteEnabled.IsChecked ?? false;

				if ( Editor_PushToTalk_AudioRenderDeviceOption.SelectedItem != null )
				{
					Settings.editor.editorPushToTalkAudioRenderDeviceId = audioRenderDeviceOptions[ (string) Editor_PushToTalk_AudioRenderDeviceOption.SelectedItem ];
				}

				PushToTalk.Initialize();

				Settings.editor.editorTriggersSessionChange = Editor_Triggers_SessionChange.Text;

				Settings.editor.editorStartupEnableDirector = Editor_Startup_EnableDirector.IsChecked ?? false;

				Settings.editor.editorControlPanelSortByCarNumber = Editor_ControlPanel_SortByCarNumber.IsChecked ?? false;

//				Settings.editor.editorWebcamStreamingEnabled = Editor_WebcamStreaming_Enabled.IsChecked ?? false;
//				Settings.editor.editorWebcamStreamingWebserverURL = Editor_WebcamStreaming_WebserverURL.Text;

				Settings.saveEditorToFileQueued = true;

				Update();
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
