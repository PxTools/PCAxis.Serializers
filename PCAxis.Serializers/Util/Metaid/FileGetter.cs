using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace PCAxis.Serializers.Util.MetaId
{
    internal class FileGetter : IFileGetter
    {

        const string DefaultConfigFilename = "metaid.config";
        /// <summary>
        /// Logging to Log4Net
        /// </summary>
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(FileGetter));


        XDocument IFileGetter.ReadConfig(string configurationFile)
        {

            string configFile = configurationFile ?? DefaultConfigFilename;

            //It is ok to not use metaid
            if (!File.Exists(configFile))
            {
                var fullPath = Path.GetFullPath(configFile);
                if (configFile.Equals(DefaultConfigFilename))
                {
                    _logger.WarnFormat("Metaid configuration file '{0}' does not exist in the folder where the dlls are. So any metaids will not be replaced.", fullPath);
                    return null;
                }
                else
                {
                    string errMess = $"Metaid configuration file  '{fullPath}' not found.";
                    _logger.Error(errMess);
                    throw new FileNotFoundException(errMess);
                }
            }

            return XDocument.Load(configFile);
        }

        Dictionary<string, string> IFileGetter.ReadLabelsfile(string labelsFilePath)
        {
            Dictionary<string, string> myOut = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(labelsFilePath))
            {
                return myOut;
            }

            if (!File.Exists(labelsFilePath))
            {
                var fullPath = Path.GetFullPath(labelsFilePath);
                string errMess = $"Labelfile  '{fullPath}' not found.";
                _logger.Error(errMess);
                throw new FileNotFoundException(errMess);
            }


            char[] keySeparator = { ' ' };

            foreach (var line in File.ReadLines(labelsFilePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(keySeparator, 2, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != 2)
                {
                    Console.Error.WriteLine($"Ignoring bad line: '{line}'");
                    continue;
                }

                myOut[parts[0]] = parts[1];
            }

            return myOut;
        }
    }
}
