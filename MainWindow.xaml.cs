
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

using Dsafa.WpfColorPicker;

using Microsoft.Win32;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	public partial class MainWindow : Window
	{
		public static MainWindow Instance { get; private set; }

		public int initializing = 0;

		public RenameOverlay? renameOverlay;
		public ColorPicker? colorPicker;

		static MainWindow()
		{
			Instance = new MainWindow();
		}

		private MainWindow()
		{
			Instance = this;

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

			initializing++;

			foreach ( var item in Settings.overlay.imageSettingsDataDictionary )
			{
				Image_ID.Items.Add( item.Key );
			}

			foreach ( var imageType in Enum.GetValues( typeof( SettingsImage.ImageType ) ) )
			{
				Image_ImageType.Items.Add( imageType );
			}

			foreach ( var item in Fonts.SystemFontFamilies )
			{
				Font_FontA_Name.Items.Add( item.ToString() );
				Font_FontB_Name.Items.Add( item.ToString() );
				Font_FontC_Name.Items.Add( item.ToString() );
				Font_FontD_Name.Items.Add( item.ToString() );
			}

			foreach ( var items in Settings.overlay.textSettingsDataDictionary )
			{
				Text_ID.Items.Add( items.Key );
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

			var combined = Settings.GetCombinedOverlay();

			General_Position_X.SetValue( combined.overlayPosition.x );
			General_Position_Y.SetValue( combined.overlayPosition.y );

			General_Size_W.SetValue( combined.overlaySize.x );
			General_Size_H.SetValue( combined.overlaySize.y );

			General_Position_Override.IsChecked = combined.overlayPosition_Overridden;
			General_Size_Override.IsChecked = combined.overlaySize_Overridden;

			foreach ( var item in Font_FontA_Name.Items )
			{
				if ( item.ToString() == combined.fontNames[ 0 ] )
				{
					Font_FontA_Name.SelectedItem = item;
					break;
				}
			}

			foreach ( var item in Font_FontB_Name.Items )
			{
				if ( item.ToString() == combined.fontNames[ 1 ] )
				{
					Font_FontB_Name.SelectedItem = item;
					break;
				}
			}

			foreach ( var item in Font_FontC_Name.Items )
			{
				if ( item.ToString() == combined.fontNames[ 2 ] )
				{
					Font_FontC_Name.SelectedItem = item;
					break;
				}
			}

			foreach ( var item in Font_FontD_Name.Items )
			{
				if ( item.ToString() == combined.fontNames[ 3 ] )
				{
					Font_FontD_Name.SelectedItem = item;
					break;
				}
			}

			Font_FontA_Name_Override.IsChecked = combined.fontNames_Overridden[ 0 ];
			Font_FontB_Name_Override.IsChecked = combined.fontNames_Overridden[ 1 ];
			Font_FontC_Name_Override.IsChecked = combined.fontNames_Overridden[ 2 ];
			Font_FontD_Name_Override.IsChecked = combined.fontNames_Overridden[ 3 ];

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

			initializing--;
		}

		public void InitializeImage()
		{
			if ( Image_ID.SelectedIndex != -1 )
			{
				initializing++;

				var id = (string) Image_ID.Items.GetItemAt( Image_ID.SelectedIndex );

				var combined = Settings.GetCombinedOverlay();

				var settings = combined.imageSettingsDataDictionary[ id ];

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

				Image_ImageType_Override.IsChecked = settings.imageType_Overridden;
				Image_FilePath_Override.IsChecked = settings.filePath_Overridden;
				Image_Position_Override.IsChecked = settings.position_Overridden;
				Image_Size_Override.IsChecked = settings.size_Overridden;
				Image_TintColor_Override.IsChecked = settings.tintColor_Overridden;

				initializing--;
			}
		}

		public void InitializeText()
		{
			if ( Text_ID.SelectedIndex != -1 )
			{
				initializing++;

				var id = (string) Text_ID.Items.GetItemAt( Text_ID.SelectedIndex );

				var combined = Settings.GetCombinedOverlay();

				var settings = combined.textSettingsDataDictionary[ id ];

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
			}
		}

		private void OverlayFile_Rename_Click( object sender, EventArgs e )
		{
			renameOverlay = new( Settings.overlay.ToString() )
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

				IPC.readyToSend = true;

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

					overlay.fontNames[ 0 ] = Font_FontA_Name.SelectedItem?.ToString() ?? string.Empty;
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

					overlay.fontNames[ 1 ] = Font_FontB_Name.SelectedItem?.ToString() ?? string.Empty;
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

					overlay.fontNames[ 2 ] = Font_FontC_Name.SelectedItem?.ToString() ?? string.Empty;
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

					overlay.fontNames[ 3 ] = Font_FontD_Name.SelectedItem?.ToString() ?? string.Empty;
				}

				IPC.readyToSend = true;

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

					settings.tintColor = new Unity.Color( Image_TintColor_R.GetValue(), Image_TintColor_G.GetValue(), Image_TintColor_B.GetValue(), Image_TintColor_A.GetValue() );
				}

				IPC.readyToSend = true;

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

					settings.tintColor = new Unity.Color( Text_TintColor_R.GetValue(), Text_TintColor_G.GetValue(), Text_TintColor_B.GetValue(), Text_TintColor_A.GetValue() );
				}

				IPC.readyToSend = true;

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
