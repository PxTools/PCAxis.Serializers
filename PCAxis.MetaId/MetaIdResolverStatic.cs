using System;
using System.Collections.Generic;
using System.Xml;

namespace PCAxis.MetaId
{
    /// <summary>
    /// Class that encapsulates a metaid.config file containing link-definitions to metadata systems, and has methodsw that resolves a "raw" meta-id string to Links.
    /// </summary>
    public class MetaIdResolverStatic
    {
        /// <summary>
        /// Class holding format information for a link
        /// </summary>
        private class MetaLinkFormat
        {
            public MetaLinkFormat(string textFormat, string linkFormat, string linkType, string linkRelation)
            {
                this.LinkTextFormat = textFormat;
                this.LinkUrlFormat = linkFormat;
                this.LinkType = linkType;
                this.LinkRelation = linkRelation;
            }

            /// <summary>
            /// Format of the link text
            /// </summary>
            public string LinkTextFormat { get; }

            /// <summary>
            /// Format of the link (URL)
            /// </summary>
            public string LinkUrlFormat { get; }

            /// <summary>
            /// Content-type Type 
            /// </summary>
            public string LinkType { get; }
            public string LinkRelation { get; }
        }


        #region "Private fields"

        /// <summary>
        /// Configuration file
        /// </summary>
        private static XmlDocument _xdoc;

        /// <summary>
        /// Dictionary of metadata systems containing table information. 
        /// Key = Metadata system id, Value = dictionary of linkformats per language (key = language, value = linkformat-object). 
        /// </summary>
        private static readonly Dictionary<string, Dictionary<string, List<MetaLinkFormat>>> _tableLinkFormats;

        /// <summary>
        /// Dictionary of metadata systems containing variable information. 
        /// Key = Metadata system id, Value = dictionary of linkformats per language (key = language, value = linkformat-object). 
        /// </summary>
        private static readonly Dictionary<string, Dictionary<string, List<MetaLinkFormat>>> _variableLinkFormats;

        /// <summary>
        /// Dictionary of metadata systems containing value information. 
        /// Key = Metadata system id, Value = dictionary of linkformats per language (key = language, value = linkformat-object). 
        /// </summary>
        private static readonly Dictionary<string, Dictionary<string, List<MetaLinkFormat>>> _valueLinkFormats;

        /// <summary>
        /// Logging to Log4Net
        /// </summary>
        private static log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(MetaIdResolverStatic));

        /// <summary>
        /// Character that separates the systems within a META-ID 
        /// </summary>
        private static readonly char[] _systemSeparator = { ',', ' ' };

        /// <summary>
        /// Character that separates the parameters within a system META-ID
        /// </summary>
        private static readonly char[] _paramSeparator = { ':' };

        private static readonly bool _hasEntries;


        #endregion
        /// <summary>
        /// Constructor
        /// </summary>
        static MetaIdResolverStatic()
        {
            // _metadataSystems = new List<MetadataSystem>();
            _tableLinkFormats = new Dictionary<string, Dictionary<string, List<MetaLinkFormat>>>();
            _variableLinkFormats = new Dictionary<string, Dictionary<string, List<MetaLinkFormat>>>();
            _valueLinkFormats = new Dictionary<string, Dictionary<string, List<MetaLinkFormat>>>();

            _hasEntries = LoadConfiguration("metaid.config");

        }

        /// <summary>
        /// Load metadata.config file
        /// </summary>
        /// <param name="configurationFile">Path to the configuration file</param>
        /// <returns>True if the configuration file was successfully loaded, else false</returns>
        private static bool LoadConfiguration(string configurationFile)
        {
            //Todo make void and throw something if configurationFile isNotNullOrEmptry?
            if (!System.IO.File.Exists(configurationFile))
            {
                _logger.ErrorFormat("Metadata configuration file '{0}' does not exist", configurationFile);
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
        private static bool LoadConfigurationSection(string section, Dictionary<string, Dictionary<string, List<MetaLinkFormat>>> dictionaryForSection)
        {
            string xpath;
            XmlNode node;
            XmlNodeList xmlnodes;

            xpath = "/metaId/" + section;
            node = _xdoc.SelectSingleNode(xpath);

            // Find all metaSystem nodes in section
            xpath = ".//metaSystem";
            xmlnodes = node.SelectNodes(xpath);

            foreach (XmlNode sysNode in xmlnodes)
            {
                string sysId = sysNode.Attributes["id"].Value;

                if (string.IsNullOrWhiteSpace(sysId))
                {
                    throw new ApplicationException("metaSystem element must have non null id");
                }

                if (dictionaryForSection.ContainsKey(sysId))
                {
                    throw new ApplicationException("metaSystem element in Section " + section + " : Duplicate id: " + sysId);
                }

                dictionaryForSection.Add(sysId, new Dictionary<string, List<MetaLinkFormat>>()); // add system to dictionary

                // Find all language nodes for the system
                xpath = ".//links";
                XmlNodeList linksNodes = sysNode.SelectNodes(xpath);

                foreach (XmlNode linksNode in linksNodes)
                {
                    string linkType = linksNode.Attributes["type"].Value;
                    string linkRelation = linksNode.Attributes["relation"].Value;

                    // Find all language nodes for the system
                    xpath = ".//link";
                    XmlNodeList langNodes = linksNode.SelectNodes(xpath);


                    foreach (XmlNode langNode in langNodes)
                    {
                        string language = langNode.Attributes["px-lang"].Value;
                        string textFormat = langNode.Attributes["labelStringFormat"].Value;
                        string linkFormat = langNode.Attributes["urlStringFormat"].Value;


                        if (!string.IsNullOrWhiteSpace(language) && !string.IsNullOrWhiteSpace(textFormat) && !string.IsNullOrWhiteSpace(linkFormat))
                        {
                            if (!dictionaryForSection[sysId].ContainsKey(language))
                            {
                                dictionaryForSection[sysId].Add(language, new List<MetaLinkFormat>());
                            }

                            MetaLinkFormat format = new MetaLinkFormat(textFormat, linkFormat, linkType, linkRelation);


                            dictionaryForSection[sysId][language].Add(format); // Add format for this language to dictionary

                        }
                    }


                }

            }

            return true;
        }




        /// <summary>
        /// Create links
        /// </summary>
        /// <param name="metaId">META-ID</param>
        /// <param name="language">Language</param>
        /// <param name="dictionary">Dictionary containing the link formats</param>
        /// <returns></returns>
        private static List<Link> GetLinks(string metaIdList, string language, Dictionary<string, Dictionary<string, List<MetaLinkFormat>>> dictionary, string[] textParams)
        {
            List<Link> myOut = new List<Link>();

            string[] metaIds = metaIdList.Split(_systemSeparator, StringSplitOptions.RemoveEmptyEntries);

            foreach (string metaId in metaIds)
            {
                string theMetadataSystemId = "";

                foreach (string aMetadataSystemId in dictionary.Keys)
                {
                    if (metaId.StartsWith(aMetadataSystemId))
                    {
                        theMetadataSystemId = aMetadataSystemId;

                        string rawParamsString = metaId.Replace(theMetadataSystemId, "");
                        string[] linkParams = rawParamsString.Split(_paramSeparator, StringSplitOptions.RemoveEmptyEntries);
                        //char ddsfs = ':';
                        //string[] linkParams2 = rawParamsString.Split(ddsfs);


                        if (dictionary[theMetadataSystemId].ContainsKey(language))
                        {
                            // Get format object from dictionary
                            foreach (MetaLinkFormat format in dictionary[theMetadataSystemId][language])
                            {

                                Link lnk = new Link();
                                lnk.Relation = format.LinkRelation;
                                lnk.Type = format.LinkType;

                                lnk.Url = String.Format(format.LinkUrlFormat, linkParams);
                                lnk.Label = String.Format(format.LinkTextFormat, textParams);

                                myOut.Add(lnk);
                            }
                        }
                    }
                }
            }

            return myOut;
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
