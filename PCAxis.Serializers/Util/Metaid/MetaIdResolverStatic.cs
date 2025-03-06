using System;
using System.Collections.Generic;
using System.Configuration;
using System.Xml;

namespace PCAxis.Serializers.Util.MetaId
{
    /// <summary>
    /// Class that encapsulates a metaid.config file containing link-definitions to metadata systems, and has methodsw that resolves a "raw" meta-id string to Links.
    /// </summary>
    public static class MetaIdResolverStatic
    {

        #region "Private fields"

        /// <summary>
        /// Configuration file
        /// </summary>
        private static XmlDocument _xdoc;

        /// <summary>
        /// Dictionary of metadata systems containing table information.
        /// Key = Metadata system id, Value = dictionary of linkformats per language (key = language, value = List of linkformat-object).
        /// </summary>
        private static readonly MetaSystems _tableLinkFormats = new MetaSystems();

        /// <summary>
        /// Dictionary of metadata systems containing variable information.
        /// Key = Metadata system id, Value = dictionary of linkformats per language (key = language, value = List of linkformat-object).
        /// </summary>
        private static readonly MetaSystems _variableLinkFormats = new MetaSystems();

        /// <summary>
        /// Dictionary of metadata systems containing value information.
        /// Key = Metadata system id, Value = dictionary of linkformats per language (key = language, value = List of linkformat-object).
        /// </summary>
        private static readonly MetaSystems _valueLinkFormats = new MetaSystems();

        /// <summary>
        /// Logging to Log4Net
        /// </summary>
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(MetaIdResolverStatic));

        /// <summary>
        /// Character that separates the systems within a META-ID
        /// </summary>
        private static readonly char[] _systemSeparator = { ',', ' ' };

        /// <summary>
        /// Character that separates the parameters within a system META-ID
        /// </summary>
        private static readonly char[] _paramSeparator = { ':' };


        private static readonly bool _hasEntries = LoadConfiguration("metaid.config");

        #endregion


        /// <summary>
        /// Load metadata.config file
        /// </summary>
        /// <param name="configurationFile">Path to the configuration file</param>
        /// <returns>True if the configuration file was successfully loaded, else false</returns>
        private static bool LoadConfiguration(string configurationFile)
        {
            //It is ok to not use metaid
            if (!System.IO.File.Exists(configurationFile))
            {
                _logger.WarnFormat("Metaid configuration file '{0}' does not exist in the folder where the dlls are ...", configurationFile);
                return false;
            }

            _xdoc = new XmlDocument();
            _xdoc.Load(configurationFile);

            // Table-level
            LoadConfigurationSection("onTable", _tableLinkFormats);

            // Variable-level
            LoadConfigurationSection("onVariable", _variableLinkFormats);

            // Value-level
            LoadConfigurationSection("onValue", _valueLinkFormats);

            return true;
        }

        /// <summary>
        /// Load sub section of the configuration file
        /// </summary>
        /// <param name="section">Name of the section</param>
        /// <param name="dictionary">Dictionary to store section data in</param>
        /// <returns></returns>
        private static void LoadConfigurationSection(string section, MetaSystems dictionaryForSection)
        {
            // Find all metaSystem nodes in section
            string xpath = "/metaId/" + section + "/metaSystem";
            XmlNodeList xmlnodes = _xdoc.SelectNodes(xpath);

            foreach (XmlNode sysNode in xmlnodes)
            {
                string sysId = sysNode.Attributes["id"].Value;

                if (string.IsNullOrWhiteSpace(sysId))
                {
                    throw new ConfigurationErrorsException("MetaId config: metaSystem element in Section " + section + " : must have non null id");
                }

                if (dictionaryForSection.ContainsKey(sysId))
                {
                    throw new ConfigurationErrorsException("MetaId config: metaSystem element in Section " + section + " : Duplicate id: " + sysId);
                }

                dictionaryForSection.Add(sysId, new MetaLinkFormatsByLanguage()); // add system to dictionary

                // Find all language nodes for the system
                xpath = ".//links";
                XmlNodeList linksNodes = sysNode.SelectNodes(xpath);

                foreach (XmlNode linksNode in linksNodes)
                {
                    string linkType = linksNode.Attributes["type"].Value;
                    string linkRelation = linksNode.Attributes["relation"].Value;

                    // Find all language nodes for the system
                    xpath = ".//link";
                    XmlNodeList linkNodes = linksNode.SelectNodes(xpath);


                    foreach (XmlNode linkNode in linkNodes)
                    {
                        string pxLang = linkNode.Attributes["px-lang"].Value;
                        string labelStringFormat = linkNode.Attributes["labelStringFormat"].Value;
                        string urlStringFormat = linkNode.Attributes["urlStringFormat"].Value;

                        if (string.IsNullOrWhiteSpace(pxLang) || string.IsNullOrWhiteSpace(labelStringFormat) || string.IsNullOrWhiteSpace(urlStringFormat))
                        {
                            continue;
                        }

                        if (!dictionaryForSection[sysId].ContainsKey(pxLang))
                        {
                            dictionaryForSection[sysId].Add(pxLang, new List<MetaLinkFormat>());
                        }

                        MetaLinkFormat format = new MetaLinkFormat(labelStringFormat, urlStringFormat, linkType, linkRelation);

                        dictionaryForSection[sysId][pxLang].Add(format); // Add format for this language to dictionary
                    }


                }

            }
        }




        /// <summary>
        /// Create links
        /// </summary>
        /// <param name="metaId">META-ID</param>
        /// <param name="language">Language</param>
        /// <param name="metaSystems">Dictionary containing the link formats</param>
        /// <returns></returns>
        private static List<Link> GetLinks(string metaIdList, string language, MetaSystems metaSystems, string[] textParams)
        {
            List<Link> myOut = new List<Link>();
            if (!_hasEntries)
            {
                return myOut;
            }

            string[] metaIds = metaIdList.Split(_systemSeparator, StringSplitOptions.RemoveEmptyEntries);

            foreach (string metaId in metaIds)
            {
                string theMetadataSystemId = "";

                foreach (string aMetadataSystemId in metaSystems.Keys)
                {
                    if (!metaId.StartsWith(aMetadataSystemId))
                    {
                        break;
                    }

                    theMetadataSystemId = aMetadataSystemId;

                    string rawParamsString = metaId.Replace(theMetadataSystemId, "");
                    string[] linkParams = rawParamsString.Split(_paramSeparator, StringSplitOptions.RemoveEmptyEntries);

                    if (metaSystems[theMetadataSystemId].ContainsKey(language))
                    {
                        // Get format object from dictionary
                        foreach (MetaLinkFormat format in metaSystems[theMetadataSystemId][language])
                        {
                            Link lnk = GetFormattedLink(textParams, linkParams, format);

                            myOut.Add(lnk);
                        }
                    }

                }
            }

            return myOut;
        }

        private static Link GetFormattedLink(string[] textParams, string[] linkParams, MetaLinkFormat format)
        {
            Link link = new Link();
            link.Relation = format.LinkRelation;
            link.Type = format.LinkType;

            link.Url = String.Format(format.LinkUrlFormat, linkParams);
            link.Label = String.Format(format.LinkTextFormat, textParams);
            return link;
        }

        #region "Implementation of IMetaIdProvider"


        public static List<Link> GetTableLinks(string metaIdField, string language)
        {
            string[] textParams = new string[] { };
            return GetLinks(metaIdField, language, _tableLinkFormats, textParams);
        }

        public static List<Link> GetVariableLinks(string metaIdField, string language, string variableLabel)
        {
            string[] textParams = new string[] { variableLabel };
            return GetLinks(metaIdField, language, _variableLinkFormats, textParams);
        }

        public static List<Link> GetValueLinks(string metaIdField, string language, string variableLabel, string valueLabel)
        {
            string[] textParams = new string[] { variableLabel, valueLabel };
            return GetLinks(metaIdField, language, _valueLinkFormats, textParams);
        }
        #endregion

    }
}
