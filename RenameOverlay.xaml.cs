
using System.IO;
using System.Windows;

namespace iRacingTVController
{
	public partial class RenameOverlay : Window
	{
		public string overlayName;

		public RenameOverlay( string overlayName )
		{
			InitializeComponent();

			this.overlayName = overlayName;

			OverlayName.Text = this.overlayName;
		}

		private void Rename_Click( object sender, RoutedEventArgs e )
		{
			var overlayName = OverlayName.Text.Trim();

			var lowercaseOverlayName = overlayName.ToLower();
			var lowercaseGlobalOverlayName = Settings.global.ToString().ToLower();

			if ( overlayName == string.Empty )
			{
				MessageBox.Show( "You need to give the overlay a name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}
			else if ( lowercaseOverlayName == lowercaseGlobalOverlayName )
			{
				MessageBox.Show( "You can't use that name, it is reserved.", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}
			else
			{
				var overlayFilePath = Settings.overlaySettingsFolder + overlayName + ".xml";

				if ( Settings.overlay.filePath == overlayFilePath )
				{
					Close();
				}
				else if ( File.Exists( overlayFilePath ) )
				{
					MessageBox.Show( "There is already an existing overlay with that name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
				}
				else
				{
					File.Delete( Settings.overlay.filePath );

					Settings.overlay.filePath = overlayFilePath;

					Settings.SaveOverlay();

					Settings.editor.lastActiveOverlayFilePath = Settings.overlay.filePath;

					Settings.SaveEditor();

					MainWindow.Instance.Initialize();

					Close();
				}
			}
		}

		private void Cancel_Click( object sender, RoutedEventArgs e )
		{
			Close();
		}
	}
}
