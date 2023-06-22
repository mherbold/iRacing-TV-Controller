
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

using Dsafa.WpfColorPicker;

using Microsoft.Win32;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	public partial class MainWindow : Window
	{
		public static MainWindow Instance { get; private set; }

		public int initializing = 0;

		public SortedDictionary<string, string> fontPaths;

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

			General_Position_X.SetValue( Settings.combined.overlayPosition.x );
			General_Position_Y.SetValue( Settings.combined.overlayPosition.y );

			General_Size_W.SetValue( Settings.combined.overlaySize.x );
			General_Size_H.SetValue( Settings.combined.overlaySize.y );

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

			Editor_Mouse_Position_Normal.SetValue( Settings.editor.positioningSpeedNormal );
			Editor_Mouse_Position_Fast.SetValue( Settings.editor.positioningSpeedFast );
			Editor_Mouse_Position_Slow.SetValue( Settings.editor.positioningSpeedSlow );

			InitializeTranslation();

			RaceStatus_Overlay_Enable.IsChecked = Settings.combined.raceStatusOverlayEnabled;
			RaceStatus_OverlayPosition_X.SetValue( (int) Settings.combined.raceStatusOverlayPosition.x );
			RaceStatus_OverlayPosition_Y.SetValue( (int) Settings.combined.raceStatusOverlayPosition.y );

			RaceStatus_OverlayEnable_Override.IsChecked = Settings.combined.raceStatusOverlayEnabled_Overridden;
			RaceStatus_OverlayPosition_Override.IsChecked = Settings.combined.raceStatusOverlayPosition_Overridden;

			Leaderboard_Overlay_Enable.IsChecked = Settings.combined.leaderboardOverlayEnabled;
			Leaderboard_OverlayPosition_X.SetValue( (int) Settings.combined.leaderboardOverlayPosition.x );
			Leaderboard_OverlayPosition_Y.SetValue( (int) Settings.combined.leaderboardOverlayPosition.y );
			Leaderboard_FirstPlacePosition_X.SetValue( (int) Settings.combined.leaderboardFirstPlacePosition.x );
			Leaderboard_FirstPlacePosition_Y.SetValue( (int) Settings.combined.leaderboardFirstPlacePosition.y );
			Leaderboard_PlaceCount.Value = Settings.combined.leaderboardPlaceCount;
			Leaderboard_PlaceSpacing_X.SetValue( (int) Settings.combined.leaderboardPlaceSpacing.x );
			Leaderboard_PlaceSpacing_Y.SetValue( (int) Settings.combined.leaderboardPlaceSpacing.y );
			Leaderboard_UseClassColors_Enable.IsChecked = Settings.combined.leaderboardUseClassColors;
			Leaderboard_ClassColorStrength.Value = Settings.combined.leaderboardClassColorStrength * 255.0f;
			Leaderboard_TelemetryPitColor_R.SetValue( Settings.combined.leaderboardTelemetryPitColor.r );
			Leaderboard_TelemetryPitColor_G.SetValue( Settings.combined.leaderboardTelemetryPitColor.g );
			Leaderboard_TelemetryPitColor_B.SetValue( Settings.combined.leaderboardTelemetryPitColor.b );
			Leaderboard_TelemetryPitColor_A.SetValue( Settings.combined.leaderboardTelemetryPitColor.a );
			Leaderboard_TelemetryOutColor_R.SetValue( Settings.combined.leaderboardTelemetryOutColor.r );
			Leaderboard_TelemetryOutColor_G.SetValue( Settings.combined.leaderboardTelemetryOutColor.g );
			Leaderboard_TelemetryOutColor_B.SetValue( Settings.combined.leaderboardTelemetryOutColor.b );
			Leaderboard_TelemetryOutColor_A.SetValue( Settings.combined.leaderboardTelemetryOutColor.a );
			Leaderboard_TelemetryIsInBetweenCars.IsChecked = Settings.combined.leaderboardTelemetryIsBetweenCars;
			Leaderboard_TelemetryMode_ShowLaps.IsChecked = ( Settings.combined.leaderboardTelemetryMode == 0 );
			Leaderboard_TelemetryMode_ShowDistance.IsChecked = ( Settings.combined.leaderboardTelemetryMode == 1 );
			Leaderboard_TelemetryMode_ShowTime.IsChecked = ( Settings.combined.leaderboardTelemetryMode == 2 );
			Leaderboard_TelemetryNumberOfCheckpoints.Value = Settings.combined.leaderboardTelemetryNumberOfCheckpoints;

			Leaderboard_OverlayEnable_Override.IsChecked = Settings.combined.leaderboardOverlayEnabled_Overridden;
			Leaderboard_OverlayPosition_Override.IsChecked = Settings.combined.leaderboardOverlayPosition_Overridden;
			Leaderboard_FirstPlacePosition_Override.IsChecked = Settings.combined.leaderboardFirstPlacePosition_Overridden;
			Leaderboard_PlaceCount_Override.IsChecked = Settings.combined.leaderboardPlaceCount_Overridden;
			Leaderboard_PlaceSpacing_Override.IsChecked = Settings.combined.leaderboardPlaceSpacing_Overridden;
			Leaderboard_UseClassColors_Override.IsChecked = Settings.combined.leaderboardUseClassColors_Overridden;
			Leaderboard_ClassColorStrength_Override.IsChecked = Settings.combined.leaderboardClassColorStrength_Overridden;
			Leaderboard_TelemetryPitColor_Override.IsChecked = Settings.combined.leaderboardTelemetryPitColor_Overridden;
			Leaderboard_TelemetryOutColor_Override.IsChecked = Settings.combined.leaderboardTelemetryOutColor_Overridden;
			Leaderboard_TelemetryIsInBetweenCars_Override.IsChecked = Settings.combined.leaderboardTelemetryIsBetweenCars_Overridden;
			Leaderboard_TelemetryMode_Override.IsChecked = Settings.combined.leaderboardTelemetryMode_Overridden;
			Leaderboard_TelemetryNumberOfCheckpoints_Override.IsChecked = Settings.combined.leaderboardTelemetryNumberOfCheckpoints_Overridden;

			VoiceOf_Overlay_Enable.IsChecked = Settings.combined.voiceOfOverlayEnabled;
			VoiceOf_OverlayPosition_X.SetValue( (int) Settings.combined.voiceOfOverlayPosition.x );
			VoiceOf_OverlayPosition_Y.SetValue( (int) Settings.combined.voiceOfOverlayPosition.y );

			VoiceOf_OverlayEnable_Override.IsChecked = Settings.combined.voiceOfOverlayEnabled_Overridden;
			VoiceOf_OverlayPosition_Override.IsChecked = Settings.combined.voiceOfOverlayPosition_Overridden;

			Subtitle_Overlay_Enable.IsChecked = Settings.combined.subtitleOverlayEnabled;
			Subtitle_OverlayPosition_X.SetValue( (int) Settings.combined.subtitleOverlayPosition.x );
			Subtitle_OverlayPosition_Y.SetValue( (int) Settings.combined.subtitleOverlayPosition.y );
			Subtitle_OverlayMaxSize_W.SetValue( (int) Settings.combined.subtitleOverlayMaxSize.x );
			Subtitle_OverlayMaxSize_H.SetValue( (int) Settings.combined.subtitleOverlayMaxSize.y );
			Subtitle_OverlayBackgroundColor_R.SetValue( Settings.combined.subtitleOverlayBackgroundColor.r );
			Subtitle_OverlayBackgroundColor_G.SetValue( Settings.combined.subtitleOverlayBackgroundColor.g );
			Subtitle_OverlayBackgroundColor_B.SetValue( Settings.combined.subtitleOverlayBackgroundColor.b );
			Subtitle_OverlayBackgroundColor_A.SetValue( Settings.combined.subtitleOverlayBackgroundColor.a );
			Subtitle_TextPadding_X.SetValue( Settings.combined.subtitleTextPadding.x );
			Subtitle_TextPadding_Y.SetValue( Settings.combined.subtitleTextPadding.y );

			Subtitle_OverlayEnable_Override.IsChecked = Settings.combined.subtitleOverlayEnabled_Overridden;
			Subtitle_OverlayPosition_Override.IsChecked = Settings.combined.subtitleOverlayPosition_Overridden;
			Subtitle_OverlayMaxSize_Override.IsChecked = Settings.combined.subtitleOverlayMaxSize_Overridden;
			Subtitle_OverlayBackgroundColor_Override.IsChecked = Settings.combined.subtitleOverlayBackgroundColor_Overridden;
			Subtitle_TextPadding_Override.IsChecked = Settings.combined.subtitleTextPadding_Overridden;

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
				Image_Position_X.SetValue( (int) settings.position.x );
				Image_Position_Y.SetValue( (int) settings.position.y );
				Image_Size_W.SetValue( (int) settings.size.x );
				Image_Size_H.SetValue( (int) settings.size.y );
				Image_TintColor_R.SetValue( settings.tintColor.r );
				Image_TintColor_G.SetValue( settings.tintColor.g );
				Image_TintColor_B.SetValue( settings.tintColor.b );
				Image_TintColor_A.SetValue( settings.tintColor.a );
				Image_Border_L.SetValue( settings.border.x );
				Image_Border_T.SetValue( settings.border.y );
				Image_Border_R.SetValue( settings.border.z );
				Image_Border_B.SetValue( settings.border.w );

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
				Text_FontSize.SetValue( settings.fontSize );
				Text_Alignment.SelectedItem = settings.alignment;
				Text_Position_X.SetValue( (int) settings.position.x );
				Text_Position_Y.SetValue( (int) settings.position.y );
				Text_Size_W.SetValue( (int) settings.size.x );
				Text_Size_H.SetValue( (int) settings.size.y );
				Text_TintColor_R.SetValue( settings.tintColor.r );
				Text_TintColor_G.SetValue( settings.tintColor.g );
				Text_TintColor_B.SetValue( settings.tintColor.b );
				Text_TintColor_A.SetValue( settings.tintColor.a );

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

					overlay.overlayPosition.x = General_Position_X.GetValue();
					overlay.overlayPosition.y = General_Position_Y.GetValue();
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

					overlay.overlaySize.x = General_Size_W.GetValue();
					overlay.overlaySize.y = General_Size_H.GetValue();
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
				ScR = Image_TintColor_R.GetValue(),
				ScG = Image_TintColor_G.GetValue(),
				ScB = Image_TintColor_B.GetValue(),
				ScA = Image_TintColor_A.GetValue()
			};

			var colorPickerDialog = new ColorPickerDialog( color )
			{
				Owner = this
			};

			var result = colorPickerDialog.ShowDialog();

			if ( result.HasValue && result.Value )
			{
				Image_TintColor_R.SetValue( colorPickerDialog.Color.ScR );
				Image_TintColor_G.SetValue( colorPickerDialog.Color.ScG );
				Image_TintColor_B.SetValue( colorPickerDialog.Color.ScB );
				Image_TintColor_A.SetValue( colorPickerDialog.Color.ScA );
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

					settings.position = new Vector2( Image_Position_X.GetValue(), Image_Position_Y.GetValue() );
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

					settings.size = new Vector2( Image_Size_W.GetValue(), Image_Size_H.GetValue() );
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

					settings.tintColor = new Color( Image_TintColor_R.GetValue(), Image_TintColor_G.GetValue(), Image_TintColor_B.GetValue(), Image_TintColor_A.GetValue() );
				}

				overridden = Image_Border_Override.IsChecked ?? false;

				if ( overlaySettings.border_Overridden != overridden )
				{
					overlaySettings.border_Overridden = overridden;

					InitializeImage();
				}
				else
				{
					var settings = overlaySettings.border_Overridden? overlaySettings : globalSettings;

					settings.border = new Vector4( Image_Border_L.GetValue(), Image_Border_T.GetValue(), Image_Border_R.GetValue(), Image_Border_B.GetValue() );
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
				ScR = Text_TintColor_R.GetValue(),
				ScG = Text_TintColor_G.GetValue(),
				ScB = Text_TintColor_B.GetValue(),
				ScA = Text_TintColor_A.GetValue()
			};

			var colorPickerDialog = new ColorPickerDialog( color )
			{
				Owner = this
			};

			var result = colorPickerDialog.ShowDialog();

			if ( result.HasValue && result.Value )
			{
				Text_TintColor_R.SetValue( colorPickerDialog.Color.ScR );
				Text_TintColor_G.SetValue( colorPickerDialog.Color.ScG );
				Text_TintColor_B.SetValue( colorPickerDialog.Color.ScB );
				Text_TintColor_A.SetValue( colorPickerDialog.Color.ScA );
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

					settings.fontSize = Text_FontSize.GetValue();
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

					settings.position = new Vector2( Text_Position_X.GetValue(), Text_Position_Y.GetValue() );
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

					settings.size = new Vector2( Text_Size_W.GetValue(), Text_Size_H.GetValue() );
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

					settings.tintColor = new Color( Text_TintColor_R.GetValue(), Text_TintColor_G.GetValue(), Text_TintColor_B.GetValue(), Text_TintColor_A.GetValue() );
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

					overlay.raceStatusOverlayPosition = new Vector2( RaceStatus_OverlayPosition_X.GetValue(), RaceStatus_OverlayPosition_Y.GetValue() );
				}

				IPC.readyToSendSettings = true;

				Settings.SaveOverlay();
			}
		}

		private void Leaderboard_TelemetryPitColor_Palette_Click( object sender, EventArgs e )
		{
			var color = new System.Windows.Media.Color()
			{
				ScR = Leaderboard_TelemetryPitColor_R.GetValue(),
				ScG = Leaderboard_TelemetryPitColor_G.GetValue(),
				ScB = Leaderboard_TelemetryPitColor_B.GetValue(),
				ScA = Leaderboard_TelemetryPitColor_A.GetValue()
			};

			var colorPickerDialog = new ColorPickerDialog( color )
			{
				Owner = this
			};

			var result = colorPickerDialog.ShowDialog();

			if ( result.HasValue && result.Value )
			{
				Leaderboard_TelemetryPitColor_R.SetValue( colorPickerDialog.Color.ScR );
				Leaderboard_TelemetryPitColor_G.SetValue( colorPickerDialog.Color.ScG );
				Leaderboard_TelemetryPitColor_B.SetValue( colorPickerDialog.Color.ScB );
				Leaderboard_TelemetryPitColor_A.SetValue( colorPickerDialog.Color.ScA );
			}
		}

		private void Leaderboard_TelemetryOutColor_Palette_Click( object sender, EventArgs e )
		{
			var color = new System.Windows.Media.Color()
			{
				ScR = Leaderboard_TelemetryOutColor_R.GetValue(),
				ScG = Leaderboard_TelemetryOutColor_G.GetValue(),
				ScB = Leaderboard_TelemetryOutColor_B.GetValue(),
				ScA = Leaderboard_TelemetryOutColor_A.GetValue()
			};

			var colorPickerDialog = new ColorPickerDialog( color )
			{
				Owner = this
			};

			var result = colorPickerDialog.ShowDialog();

			if ( result.HasValue && result.Value )
			{
				Leaderboard_TelemetryOutColor_R.SetValue( colorPickerDialog.Color.ScR );
				Leaderboard_TelemetryOutColor_G.SetValue( colorPickerDialog.Color.ScG );
				Leaderboard_TelemetryOutColor_B.SetValue( colorPickerDialog.Color.ScB );
				Leaderboard_TelemetryOutColor_A.SetValue( colorPickerDialog.Color.ScA );
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

					overlay.leaderboardOverlayPosition = new Vector2( Leaderboard_OverlayPosition_X.GetValue(), Leaderboard_OverlayPosition_Y.GetValue() );
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

					overlay.leaderboardFirstPlacePosition = new Vector2( Leaderboard_FirstPlacePosition_X.GetValue(), Leaderboard_FirstPlacePosition_Y.GetValue() );
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

					overlay.leaderboardPlaceSpacing = new Vector2( Leaderboard_PlaceSpacing_X.GetValue(), Leaderboard_PlaceSpacing_Y.GetValue() );
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

				overridden = Leaderboard_TelemetryPitColor_Override.IsChecked ?? false;

				if ( Settings.overlay.leaderboardTelemetryPitColor_Overridden != overridden )
				{
					Settings.overlay.leaderboardTelemetryPitColor_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.leaderboardTelemetryPitColor_Overridden ? Settings.overlay : Settings.global;

					overlay.leaderboardTelemetryPitColor = new Color( Leaderboard_TelemetryPitColor_R.GetValue(), Leaderboard_TelemetryPitColor_G.GetValue(), Leaderboard_TelemetryPitColor_B.GetValue(), Leaderboard_TelemetryPitColor_A.GetValue() );
				}

				overridden = Leaderboard_TelemetryOutColor_Override.IsChecked ?? false;

				if ( Settings.overlay.leaderboardTelemetryOutColor_Overridden != overridden )
				{
					Settings.overlay.leaderboardTelemetryOutColor_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.leaderboardTelemetryOutColor_Overridden ? Settings.overlay : Settings.global;

					overlay.leaderboardTelemetryOutColor = new Color( Leaderboard_TelemetryOutColor_R.GetValue(), Leaderboard_TelemetryOutColor_G.GetValue(), Leaderboard_TelemetryOutColor_B.GetValue(), Leaderboard_TelemetryOutColor_A.GetValue() );
				}

				overridden = Leaderboard_TelemetryIsInBetweenCars_Override.IsChecked ?? false;

				if ( Settings.overlay.leaderboardTelemetryIsBetweenCars_Overridden != overridden )
				{
					Settings.overlay.leaderboardTelemetryIsBetweenCars_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.leaderboardTelemetryIsBetweenCars_Overridden ? Settings.overlay : Settings.global;

					overlay.leaderboardTelemetryIsBetweenCars = Leaderboard_TelemetryIsInBetweenCars.IsChecked ?? false;
				}

				overridden = Leaderboard_TelemetryMode_Override.IsChecked ?? false;

				if ( Settings.overlay.leaderboardTelemetryMode_Overridden != overridden )
				{
					Settings.overlay.leaderboardTelemetryMode_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.leaderboardTelemetryMode_Overridden ? Settings.overlay : Settings.global;

					overlay.leaderboardTelemetryMode = 0;
					
					if ( Leaderboard_TelemetryMode_ShowDistance.IsChecked == true )
					{
						overlay.leaderboardTelemetryMode = 1;
					}
					else if ( Leaderboard_TelemetryMode_ShowTime.IsChecked == true )
					{
						overlay.leaderboardTelemetryMode = 2;
					}
				}

				overridden = Leaderboard_TelemetryNumberOfCheckpoints_Override.IsChecked ?? false;

				if ( Settings.overlay.leaderboardTelemetryNumberOfCheckpoints_Overridden != overridden )
				{
					Settings.overlay.leaderboardTelemetryNumberOfCheckpoints_Overridden = overridden;

					Initialize();
				}
				else
				{
					var overlay = Settings.overlay.leaderboardTelemetryNumberOfCheckpoints_Overridden ? Settings.overlay : Settings.global;

					overlay.leaderboardTelemetryNumberOfCheckpoints = (int) Leaderboard_TelemetryNumberOfCheckpoints.Value;
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

					overlay.voiceOfOverlayPosition = new Vector2( VoiceOf_OverlayPosition_X.GetValue(), VoiceOf_OverlayPosition_Y.GetValue() );
				}

				IPC.readyToSendSettings = true;

				Settings.SaveOverlay();
			}
		}

		private void Subtitle_OverlayBackgroundColor_Palette_Click( object sender, EventArgs e )
		{
			var color = new System.Windows.Media.Color()
			{
				ScR = Subtitle_OverlayBackgroundColor_R.GetValue(),
				ScG = Subtitle_OverlayBackgroundColor_G.GetValue(),
				ScB = Subtitle_OverlayBackgroundColor_B.GetValue(),
				ScA = Subtitle_OverlayBackgroundColor_A.GetValue()
			};

			var colorPickerDialog = new ColorPickerDialog( color )
			{
				Owner = this
			};

			var result = colorPickerDialog.ShowDialog();

			if ( result.HasValue && result.Value )
			{
				Subtitle_OverlayBackgroundColor_R.SetValue( colorPickerDialog.Color.ScR );
				Subtitle_OverlayBackgroundColor_G.SetValue( colorPickerDialog.Color.ScG );
				Subtitle_OverlayBackgroundColor_B.SetValue( colorPickerDialog.Color.ScB );
				Subtitle_OverlayBackgroundColor_A.SetValue( colorPickerDialog.Color.ScA );
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

					overlay.subtitleOverlayPosition = new Vector2( Subtitle_OverlayPosition_X.GetValue(), Subtitle_OverlayPosition_Y.GetValue() );
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

					overlay.subtitleOverlayMaxSize = new Vector2( Subtitle_OverlayMaxSize_W.GetValue(), Subtitle_OverlayMaxSize_H.GetValue() );
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

					overlay.subtitleOverlayBackgroundColor = new Color( Subtitle_OverlayBackgroundColor_R.GetValue(), Subtitle_OverlayBackgroundColor_G.GetValue(), Subtitle_OverlayBackgroundColor_B.GetValue(), Subtitle_OverlayBackgroundColor_A.GetValue() );
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

					overlay.subtitleTextPadding = new Vector2Int( Subtitle_TextPadding_X.GetValue(), Subtitle_TextPadding_Y.GetValue() );
				}

				IPC.readyToSendSettings = true;

				Settings.SaveOverlay();
			}
		}

		private void Editor_Mouse_Position( object sender, EventArgs e )
		{
			if ( initializing == 0 )
			{
				Settings.editor.positioningSpeedNormal = Editor_Mouse_Position_Normal.GetValue();
				Settings.editor.positioningSpeedFast = Editor_Mouse_Position_Fast.GetValue();
				Settings.editor.positioningSpeedSlow = Editor_Mouse_Position_Slow.GetValue();

				Settings.SaveEditor();
			}
		}
	}
}
