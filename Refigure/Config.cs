using System;
using System.Configuration;

namespace Refigure
{
	public class Config
	{
		public static string Get(string settingName, string exceptionMessage = null)
		{
			var value = ConfigurationManager.AppSettings[settingName];
			if (exceptionMessage != null && string.IsNullOrEmpty(value))
				throw new Exception(exceptionMessage);
			return value;
		}

		public static int GetAsInt(string settingName, string exceptionMessage = null)
		{
			return int.Parse(Get(settingName, exceptionMessage));
		}
	}
}
