
using System.IO;

namespace iRacingTVController
{
	public static class Triggers
	{
		public static int currentSessionNumber = -1;

		public static void Update()
		{
			if ( currentSessionNumber != IRSDK.normalizedSession.sessionNumber )
			{
				currentSessionNumber = IRSDK.normalizedSession.sessionNumber;

				if ( Settings.editor.editorTriggersSessionChange != string.Empty )
				{
					if ( File.Exists( Settings.editor.editorTriggersSessionChange ) )
					{
						LogFile.Write( $"Session changed, running program '{Settings.editor.editorTriggersSessionChange}'...\r\n" );

						using var process = new System.Diagnostics.Process();

						process.StartInfo.FileName = Settings.editor.editorTriggersSessionChange;

						process.Start();
					}
					else
					{
						LogFile.Write( $"Session changed, but could not fire trigger because program '{Settings.editor.editorTriggersSessionChange}' does not exist.\r\n" );
					}
				}
			}
		}
	}
}
