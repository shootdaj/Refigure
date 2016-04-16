using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;

namespace Refigure
{
    public static class Config
    {
        #region Hard-coded Values

        /// <summary>
        /// Stores the default values to be used for configuration.
        /// </summary>
        private static class Defaults
        {
            /// <summary>
            /// The default value for whether the constructor should assume if we're in a dev environment or not.
            /// </summary>
            public const bool InDevEnvironment = true;

            /// <summary>
            /// Gets the name of local configuration file.
            /// </summary>
            public static string DebugLocalConfigFileName
                => Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location) + ".local.config";

            /// <summary>
            /// Gets the name of the global configuration file.
            /// </summary>
            public static string DebugGlobalConfigFileName 
                => Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location) + ".config";

            /// <summary>
            /// The default global config file names that will be searched for in the AppDomain.CurrentDirectory 
            /// in increasing order of the ordering number next to the file name.
            /// </summary>
            public static readonly IReadOnlyList<Tuple<string, int>> DefaultGlobalConfigFileNames = new List<Tuple<string, int>>()
            {
                new Tuple<string, int>("AutomationBase.config", 1),
                new Tuple<string, int>("Automation.config", 2)
            };

            /// <summary>
            /// The default local config file path to be used in dev environment.
            /// </summary>
            public static string DebugLocalConfigFilePath
            {
                get
                {
                    return GetAutomationBaseDirectory().MergePath(@"CONFIG\" + DebugLocalConfigFileName);
                }
            }

            /// <summary>
            /// The default global config file path to be used in dev environment.
            /// </summary>
            public static string DebugGlobalConfigFilePath
            {
                get
                {
                    return GetAutomationBaseDirectory().MergePath(@"CONFIG\" + DebugGlobalConfigFileName);
                }
            }

            /// <summary>
            /// The default name for the directory that contains the AutomationBase project.
            /// </summary>
            public static string AutomationBaseDirectoryName
            {
                get { return "Base"; }
            }

            /// <summary>
            /// When looking for the AutomationBase directory, how many levels to recurse up.
            /// </summary>
            public static int AutomationBaseDirectoryLevelsToRecurse
            {
                get { return 5; }
            }
        }

        /// <summary>
        /// The XPaths of commonly accessed settings
        /// </summary>
        public static class XPaths
        {
            //IMPORTANT: These paths MUST end with a forward-slash (/)
            public const string AppSettings = "/configuration/appSettings/";
            public const string ConnectionStrings = "/configuration/appSettings/connectionStrings/";
            public const string Paths = "/configuration/appSettings/paths/";
            public const string RabbitMQ = "/configuration/appSettings/rabbitMQ/";
            public const string RabbitMQManagement = "/configuration/appSettings/rabbitMQ/management/";
        }

        /// <summary>
        /// Whether or not this class should automatically initialize
        /// </summary>
        public static bool AutoInitialize = true;

        #endregion

        #region CORE

        /// <summary>
        /// Whether or not this config is being loaded in a dev environment - having this set to false
        /// restrics a lot of methods that are only to be used in tests or during debugging.
        /// </summary>
        public static bool InDevEnvironment { get; private set; }

        /// <summary>
        /// Stores the XML representation of the local config file
        /// </summary>
        private static XmlDocument _localXmlDoc;

        /// <summary>
        /// Accessor for _localXmlDoc
        /// </summary>
        private static XmlDocument LocalXmlDoc
        {
            get
            {
                if (AutoInitialize) Initialize();
                if (!Initialized) throw new Exception("Configuration has not been initialized.");
                if (!InDevEnvironment) throw new Exception("This operation is only allowed in dev environment.");
                return _localXmlDoc;
            }
        }

        /// <summary>
        /// Stores the XML representation of the global config file
        /// </summary>
        private static XmlDocument _globalXmlDoc;

        /// <summary>
        /// Accessor for _globalXmlDoc
        /// </summary>
        private static XmlDocument GlobalXmlDoc
        {
            get
            {
                if (AutoInitialize) Initialize();
                if (!Initialized) throw new Exception("Configuration has not been initialized.");
                return _globalXmlDoc;
            }
        }

        #endregion

        #region C+I

        static Config()
        {

        }

        public static bool Initialized { get; set; }

        public static void Initialize(bool inDevEnvironment = Defaults.InDevEnvironment, string globalConfigFilePath = "", string localConfigFilePath = "")
        {
            if (!Initialized)
            {
                //set whether or not we are running in a development environment
                InDevEnvironment = inDevEnvironment;

                var configFileToLoad = "";
                _globalXmlDoc = new XmlDocument();

                //if anything was passed in for the global file path
                if (!string.IsNullOrEmpty(globalConfigFilePath))
                {
                    //check for existence of file
                    if (!File.Exists(globalConfigFilePath))
                        throw new Exception("Config file does not exist at provided path : " +
                                            AppDomain.CurrentDomain.BaseDirectory.MergePath(globalConfigFilePath));

                    //load file
                    configFileToLoad = globalConfigFilePath;
                }
                //if nothing was passed in for the global file path, use the default file path
                else
                {
                    foreach (string defaultGlobalConfigFileName in Defaults.DefaultGlobalConfigFileNames.OrderBy(x => x.Item2).Select(y => y.Item1))
                    {
                        //if default file does NOT exist in the current folder
                        if (!File.Exists(defaultGlobalConfigFileName))
                        {
                            //if NOT in dev env, then throw an error saying "There is no file (ther is no spoon)".
                            if (!InDevEnvironment)
                                throw new Exception("Config file does not exist at provided path: " +
                                                    AppDomain.CurrentDomain.BaseDirectory.MergePath(defaultGlobalConfigFileName));
                            //if we ARE in dev env, then set the file to load as the one in the AutomationBase\CONFIG folder 
                            else
                            {
                                configFileToLoad = Defaults.DebugGlobalConfigFilePath;
                            }
                        }
                        //if default file exists in the current folder, set it as the file to load
                        else
                        {
                            configFileToLoad = defaultGlobalConfigFileName;
                            break;
                        }
                    }
                }

                //try to load file
                try
                {
                    Debug.Print("Loading config file: " + configFileToLoad);
                    _globalXmlDoc.Load(configFileToLoad);
                }
                catch (Exception)
                {
                    throw new Exception("Could not load config file.");
                }

                //only load local config file if in dev environment
                if (InDevEnvironment)
                {
                    _localXmlDoc = new XmlDocument();

                    if (!string.IsNullOrEmpty(localConfigFilePath))
                    {
                        if (!File.Exists(localConfigFilePath))
                            throw new Exception("Config file does not exist at provided path: " +
                                                AppDomain.CurrentDomain.BaseDirectory.MergePath(localConfigFilePath));

                        configFileToLoad = localConfigFilePath;
                    }
                    else
                    {
                        configFileToLoad = File.Exists(Defaults.DebugLocalConfigFileName)
                            ? Defaults.DebugLocalConfigFileName
                            : Defaults.DebugLocalConfigFilePath;
                    }

                    try
                    {
                        Debug.Print("Loading config file: " + configFileToLoad);
                        _localXmlDoc.Load(configFileToLoad);
                    }
                    catch (Exception)
                    {
                        throw new Exception("Could not load config file.");
                    }
                }

                Initialized = true;
            }
        }

        public static void Uninitialize()
        {
            if (Initialized)
            {
                _globalXmlDoc = null;
                _localXmlDoc = null;
                Initialized = false;
            }
        }

        public static void Reinitialize(bool inDevEnv, string globalConfigFilePath = "", string localConfigFilePath = "")
        {
            Uninitialize();
            Initialize(inDevEnv, globalConfigFilePath, localConfigFilePath);
        }

        //public static void Reinitialize(string globalConfigFilePath = "")
        //{
        //	Uninitialize();
        //	Initialize(globalConfigFilePath);
        //}

        #endregion

        #region Helpers

        /// <summary>
        /// Cache for GetAutomationBaseDirectory()
        /// </summary>
        private static string _automationBaseDirectory;

        /// <summary>
        /// Tries to get the directory that contains the AutomationBase project. This will only work during debugging mode.
        /// </summary>
        /// <returns></returns>
        private static string GetAutomationBaseDirectory()
        {
            //if (!_debugMode)
            //	throw new Exception(ResErrorMsgs.AutomationBaseDirectoryOnlyInDebug);

            if (_automationBaseDirectory != null)
                return _automationBaseDirectory;

            var baseDirectoryPath = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo newCurrentDirectory = Directory.GetParent(baseDirectoryPath);

            int i = 1;
            while (!newCurrentDirectory.GetDirectories().ToList().Select(x => x.Name).Contains(Defaults.AutomationBaseDirectoryName) && i < Defaults.AutomationBaseDirectoryLevelsToRecurse)
            {
                newCurrentDirectory = Directory.GetParent(newCurrentDirectory.FullName);
                i++;
            }
            if (i < Defaults.AutomationBaseDirectoryLevelsToRecurse)
            {
                _automationBaseDirectory = newCurrentDirectory.FullName.MergePath(Defaults.AutomationBaseDirectoryName);
                return _automationBaseDirectory;
            }
            else
                throw new Exception("Could not locate AutomationBase directory.");
        }

        private static string GetAutomationRepositoryDirectory()
        {
            if (AutoInitialize)
                Initialize();
            if (!InDevEnvironment)
                throw new Exception("This operation is only allowed in dev environment.");
            return Directory.GetParent(GetAutomationBaseDirectory()).FullName;
        }

        private static string GetAutomationSolutionDirectory()
        {
            if (AutoInitialize)
                Initialize();
            if (!InDevEnvironment)
                throw new Exception("This operation is only allowed in dev environment.");
            return Directory.GetParent(GetAutomationBaseDirectory()).FullName.MergePath("Automation");
        }

        #endregion

        #region API

        //these are arranged from high level to low level functionality

        public static string Get(string key)
        {
            int multiplicity;
            string returnValue;

            if (AutoInitialize)
                Initialize();
            if (InDevEnvironment)
            {
                returnValue = GetConfigValueFromAnywhere(key, LocalXmlDoc, out multiplicity);
                if (returnValue != string.Empty) return returnValue;
            }

            returnValue = GetConfigValueFromAnywhere(key, GlobalXmlDoc, out multiplicity);
            if (returnValue != string.Empty) return returnValue;
            return string.Empty;
        }

        public static string Get(string key, string xPath)
        {
            return GetConfigValue(key, xPath);
        }

        public static bool TryGetConfigValue(string key, string xPath, out string value)
        {
            bool success = false;
            value = string.Empty;

            if (AutoInitialize)
                Initialize();
            if (InDevEnvironment)
            {
                success = TryGetLocalConfigValue(key, xPath, out value);
            }

            if (!success)
            {
                success = TryGetGlobalConfigValue(key, xPath, out value);
            }

            return success;
        }

        public static string GetConfigValue(string key, string xPath)
        {
            if (AutoInitialize)
                Initialize();
            if (InDevEnvironment)
            {
                string localValue = GetLocalConfigValue(key, xPath);
                if (localValue != string.Empty) return localValue;
            }

            string globalValue = GetGlobalConfigValue(key, xPath);
            if (globalValue != string.Empty) return globalValue;
            return string.Empty;
        }

        public static string GetRepoCodeGenDTOPath(string controlSubtypeName, string assemblyName)
        {
            return GetGenCodeDirectory().MergePath(assemblyName).MergePath(
                controlSubtypeName + @"\DTO");
        }

        public static string GetRepoCodeGenMappingPath(string controlSubtypeName, string assemblyName)
        {
            return GetGenCodeDirectory().MergePath(assemblyName).MergePath(
                controlSubtypeName + @"\Mapping");
        }

        public static string GetBinariesPath(string assemblyName)
        {
            return GetGenCodeDirectory().MergePath(assemblyName).MergePath("bin");
        }

        public static string GetGenCodeDirectory()
        {
            return Path.GetTempPath() + GetConfigValue("RepoGenCodeFolder", XPaths.Paths);
        }

        public static string GetDLLDirectory()
        {
            if (AutoInitialize)
                Initialize();
            if (!InDevEnvironment)
                throw new Exception("This operation is only allowed in dev environment.");
            return GetAutomationSolutionDirectory().MergePath("DLL");
        }

        public static bool TryGetConnectionString(string key, out string value)
        {
            string xPath = XPaths.ConnectionStrings;
            return TryGetConfigValue(key, xPath, out value);
        }

        public static string GetConnectionString(string key)
        {
            string connString;
            if (TryGetConfigValue(key, XPaths.ConnectionStrings, out connString))
            {
                return connString;
            }
            return string.Empty;
        }

        public static string GetNugetPackagesDirectory()
        {
            if (AutoInitialize)
                Initialize();
            if (!InDevEnvironment)
                throw new Exception("This operation is only allowed in dev environment.");
            return
                Directory.GetParent(GetAutomationBaseDirectory()).FullName.MergePath(Get("NugetPackagesDirectoryName"));
        }

        public static string GetPath(string key)
        {
            return GetConfigValue(key, XPaths.Paths);
        }

        public static bool TryGetLocalConfigValue(string key, string xPath, out string value)
        {
            if (AutoInitialize)
                Initialize();
            if (!InDevEnvironment)
                throw new Exception("This operation is only allowed in dev environment.");
            value = "";
            if (LocalXmlDoc == null) return false;
            value = GetConfigValueFromXml(key, xPath, LocalXmlDoc);
            return value != string.Empty;
        }

        public static string GetLocalConfigValue(string key, string xPath)
        {
            if (AutoInitialize)
                Initialize();
            if (!InDevEnvironment)
                throw new Exception("This operation is only allowed in dev environment.");
            return GetConfigValueFromXml(key, xPath, LocalXmlDoc);
        }

        public static bool TryGetGlobalConfigValue(string key, string xPath, out string value)
        {
            value = "";
            if (GlobalXmlDoc == null) return false;
            value = GetConfigValueFromXml(key, xPath, GlobalXmlDoc);
            return value != string.Empty;
        }

        public static string GetGlobalConfigValue(string key, string xPath)
        {
            return GetConfigValueFromXml(key, xPath, GlobalXmlDoc);
        }

        public static string GetConfigValueFromXml(string key, string xPath, XmlDocument xmlDoc, string entryTagName = "add", string keyAttribute = "key", string valueAttribute = "value")
        {
            string value = "";
            try
            {
                var tag =
                    xmlDoc.DocumentElement.SelectSingleNode(xPath + entryTagName + "[@" + keyAttribute + "='" + key + "']");
                if (tag == null) throw new Exception("Configuration key not found.");   //this will only be noticed during debugging
                value = tag.Attributes[valueAttribute].Value;
            }
            catch { }

            return value;
        }

        /// <summary>
        /// Tries to find and return the value of the first key found with the given name
        /// </summary>
        /// <param name="xmlDoc">XML Document to search</param>
        /// <param name="key">Key to search for</param>
        /// <param name="multiplicity">Number of total results</param>
        /// <param name="entryTagName">Entry tag name to search for</param>
        /// <param name="keyAttribute">Key attribute to search for</param>
        /// <param name="valueAttribute">Value attribute to search for</param>
        /// <returns>Value of the first key found, if any</returns>
        public static string GetConfigValueFromAnywhere(string key, XmlDocument xmlDoc, out int multiplicity,
            string entryTagName = "add", string keyAttribute = "key", string valueAttribute = "value")
        {
            List<string> values = new List<string>();
            xmlDoc.IterateThroughAllNodes((xmlNode) =>
            {
                if (xmlNode.Attributes != null && xmlNode.Attributes["key"] != null && xmlNode.Attributes["key"].Value == key)
                {
                    values.Add(xmlNode.Attributes["value"].Value);
                }
            });

            multiplicity = values.Count;
            return multiplicity > 0 ? values[0] : "";
        }

        #endregion
    }
}
