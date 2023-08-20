
using System;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace iRacingTVController
{
	public static class WebPage
	{
		public static float saveToFileTimeRemaining = 0;
		public static bool saveToFileQueued = false;

		public static void Initialize()
		{
			try
			{
				CopyFolder( Settings.editor.webpageGeneralSourceFolder, Settings.editor.webpageGeneralOutputFolder );

				var indexHtml = File.ReadAllText( Settings.editor.webpageGeneralSourceFolder + "\\index.html" );

				indexHtml = Replace( indexHtml, "title", Settings.editor.webpageTextTitle );
				indexHtml = Replace( indexHtml, "update interval", Settings.editor.webpageGeneralUpdateInterval.ToString() );

				var indexHtmlFilePath = $"{Settings.editor.webpageGeneralOutputFolder}index.html";

				File.WriteAllText( indexHtmlFilePath, indexHtml );
			}
			catch ( IOException exception )
			{
				LogFile.Write( $"Could not initialize Web Page feature:\r\n{exception.Message}\r\n" );
			}
		}

		public static void CopyFolder( string sourceFolder, string destinationFolder )
		{
			var directoryInfo = new DirectoryInfo( sourceFolder );

			if ( !directoryInfo.Exists )
			{
				throw new DirectoryNotFoundException( $"Source directory not found: {directoryInfo.FullName}" );
			}

			DirectoryInfo[] directoryInfoList = directoryInfo.GetDirectories();

			Directory.CreateDirectory( destinationFolder );

			foreach ( var file in directoryInfo.GetFiles() )
			{
				var targetFilePath = Path.Combine( destinationFolder, file.Name );

				file.CopyTo( targetFilePath, true );
			}

			foreach ( var subDirectoryInfo in directoryInfoList )
			{
				var destinationSubFolder = Path.Combine( destinationFolder, subDirectoryInfo.Name );

				CopyFolder( subDirectoryInfo.FullName, destinationSubFolder );
			}
		}

		public static void Update()
		{
			if ( !Settings.editor.webpageGeneralEnabled )
			{
				return;
			}

			saveToFileTimeRemaining = Math.Max( 0, saveToFileTimeRemaining - Program.deltaTime );

			if ( saveToFileQueued && ( saveToFileTimeRemaining == 0 ) )
			{
				saveToFileQueued = false;
				saveToFileTimeRemaining = Settings.editor.webpageGeneralUpdateInterval;

				Save();
			}
		}

		public static void Save()
		{
			try
			{
				var jsonString = JsonSerializer.Serialize( LiveData.Instance );

				var jsonDataFilePath = $"{Settings.editor.webpageGeneralOutputFolder}livedata.json";

				File.WriteAllText( jsonDataFilePath, jsonString );
			}
			catch ( IOException )
			{
				saveToFileQueued = true;
				saveToFileTimeRemaining = Settings.editor.webpageGeneralUpdateInterval;
			}
		}

		public static string Replace( string html, string key, string value )
		{
			var pattern = $"<!-{{2,}}\\s+{key}\\s+-{{2,}}>";

			return Regex.Replace( html, pattern, value );
		}
	}
}
