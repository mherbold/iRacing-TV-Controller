
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
			var lowercaseGlobalOverlayName = Settings.overlayGlobal.ToString().ToLower();

			if ( overlayName == string.Empty )
			{
				MessageBox.Show( this, "You need to give the overlay a name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}
			else if ( lowercaseOverlayName == lowercaseGlobalOverlayName )
			{
				MessageBox.Show( this, "You can't use that name, it is reserved.", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}
			else
			{
				var overlayFilePath = Settings.overlaySettingsFolder + overlayName + ".xml";

				if ( Settings.overlayLocal.filePath == overlayFilePath )
				{
					Close();
				}
				else if ( File.Exists( overlayFilePath ) )
				{
					MessageBox.Show( this, "There is already an existing overlay with that name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
				}
				else
				{
					File.Delete( Settings.overlayLocal.filePath );

					Settings.overlayLocal.filePath = overlayFilePath;

					Settings.saveOverlayToFileQueued = true;

					Settings.editor.lastActiveOverlayFilePath = Settings.overlayLocal.filePath;

					Settings.saveEditorToFileQueued = true;

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
