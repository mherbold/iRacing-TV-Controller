
using System;

namespace iRacingTVController
{
	[Serializable]
	public class SettingsTranslation
	{
		public string id = string.Empty;

		public string translation = string.Empty;

		public bool translation_Overridden = false;

		public string Translation
		{
			get
			{
				return translation;
			}
			
			set
			{
				translation = value;

				if ( Settings.loading == 0 )
				{
					var settings = ( translation_Overridden ) ? Settings.overlayLocal : Settings.overlayGlobal;

					settings.translationDictionary[ id ].translation = translation;

					IPC.readyToSendSettings = true;

					Settings.saveOverlayToFileQueued = true;
				}
			}
		}

		public bool Translation_IsEnabled
		{
			get
			{
				return Settings.overlayLocal.translationDictionary[ id ].translation_Overridden;
			}
		}

		public bool Translation_Overridden
		{
			get
			{
				return translation_Overridden;
			}
			
			set
			{
				translation_Overridden = value;

				if ( Settings.loading == 0 )
				{
					Settings.overlayLocal.translationDictionary[ id ].translation_Overridden = value;

					MainWindow.Instance.UpdateOverlayTranslation();

					IPC.readyToSendSettings = true;

					Settings.saveOverlayToFileQueued = true;
				}
			}
		}

		public bool Translation_Overridden_IsEnabled
		{
			get
			{
				return true;
			}
		}
	}
}
