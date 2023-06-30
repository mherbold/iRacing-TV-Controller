
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace iRacingTVController
{
	public class FontPaths
	{
		public static readonly string globalFontsFolder = Environment.GetFolderPath( Environment.SpecialFolder.Fonts );
		public static readonly string localFontsFolder = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ) + "\\Microsoft\\Windows\\Fonts\\";

		public static SortedDictionary<string, string> FindAll()
		{
			SortedDictionary<string, string> fontPaths = new();

			FindInFolder( globalFontsFolder, fontPaths );
			FindInFolder( localFontsFolder, fontPaths );

			return fontPaths;
		}

		public static void FindInFolder( string path, SortedDictionary<string, string> fontPaths )
		{
			var fontsInDirectory = Directory.EnumerateFiles( path );

			foreach ( var fontPath in fontsInDirectory )
			{
				var fontFamilies = Fonts.GetFontFamilies( fontPath );

				if ( fontFamilies.Count == 1 )
				{
					var fontFamily = fontFamilies.First();

					var fontName = fontFamily.Source.Split( "#" ).Last();

					if ( !fontPaths.ContainsKey( fontName ) )
					{
						fontPaths[ fontName ] = fontPath;
					}
				}
			}
		}
	}
}
