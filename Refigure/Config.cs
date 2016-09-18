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

		public static DateTime GetAsDateTime(string settingName, string notFoundExceptionMsg = null,
			string cantParseDateExceptionMsg = null)
		{
			DateTime outputValue;

			if (DateTime.TryParse(Get(settingName, notFoundExceptionMsg), out outputValue))
			{
				return outputValue;
			}

			throw new Exception(cantParseDateExceptionMsg);
		}

		public static int? GetAsIntSilent(string settingName)
		{
			var stringValue = Get(settingName);
			int outValue;
			return !string.IsNullOrEmpty(stringValue) && int.TryParse(stringValue, out outValue) ? (int?) outValue : null;
		}

		public static bool? GetAsBoolSilent(string settingName)
		{
			var stringValue = Get(settingName);
			bool outValue;
			return !string.IsNullOrEmpty(stringValue) && bool.TryParse(stringValue, out outValue) ? (bool?)outValue : null;
		}

		public static void Set(string settingName, string value)
		{
			var config = ConfigurationManager.OpenExeConfiguration(System.Reflection.Assembly.GetCallingAssembly().Location);

			if (config.AppSettings.Settings[settingName] != null && !string.IsNullOrEmpty(config.AppSettings.Settings[settingName]?.Value))
			{
				config.AppSettings.Settings[settingName].Value = value;
			}
			else
			{
				config.AppSettings.Settings.Add(settingName, value);
			}

			config.Save(ConfigurationSaveMode.Minimal);
		}
	}
}
