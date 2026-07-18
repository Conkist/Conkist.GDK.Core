using System;
using UnityEngine;

namespace Conkist.GDK.Utils
{
	public static class ImageStringConverter
	{
		/// <summary>
		/// Use "Conkist/Tools/String Image Converter" to get string image representation
		/// </summary>
		public static Texture2D ImageFromString(string source, int width, int height)
		{
			var bytes = Convert.FromBase64String(source);
			var texture = new Texture2D(width, height);
			texture.LoadImage(bytes);
			return texture;
		}
	}
}