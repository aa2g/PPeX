using System;
using System.Reflection;

namespace PPeX.External
{
	public enum Platform
	{
		Unknown,
		Windows,
		MacOS,
		Linux
	}

	public static class PlatformHelper
	{
		public static Platform Platform { get; }

		static PlatformHelper()
		{
			Platform = CheckPlatform();
		}

		private static Platform CheckPlatform()
		{
			var pPlatform = typeof(Environment).GetProperty("Platform", BindingFlags.NonPublic | BindingFlags.Static);
			string platId = pPlatform != null ? pPlatform.GetValue(null, new object[0]).ToString() : Environment.OSVersion.Platform.ToString();
			platId = platId.ToLowerInvariant();

			if (platId.Contains("win"))
				return Platform.Windows;

			if (platId.Contains("mac") || platId.Contains("osx"))
				return Platform.MacOS;

			if (platId.Contains("lin") || platId.Contains("unix"))
				return Platform.Linux;

			return Platform.Unknown;
		}
	}
}