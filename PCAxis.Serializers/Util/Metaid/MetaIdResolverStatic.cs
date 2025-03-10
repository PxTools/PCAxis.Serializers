using System;
using System.Collections.Generic;
using System.Xml;

using PCAxis.Serializers.Util.Metaid;

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

        /// <summary>
        /// Holds all data loaded from config, reshuffled so using it easy.
        /// </summary>
        private static readonly LinkTemplatesHolder holder = LoadConfiguration("metaid.config");

        #endregion


        /// <summary>
        /// Load metadata.config file
        /// </summary>
        /// <param name="configurationFile">Path to the configuration file</param>
        /// <returns>True if the configuration file was successfully loaded, else false</returns>
        private static LinkTemplatesHolder LoadConfiguration(string configurationFile)
        {
            //It is ok to not use metaid
            if (!System.IO.File.Exists(configurationFile))
            {
                _logger.WarnFormat("Metaid configuration file '{0}' does not exist in the folder where the dlls are ...", configurationFile);
                return new LinkTemplatesHolder();
            }

            _xdoc = new XmlDocument();
            _xdoc.Load(configurationFile);

            LinkTemplatesHolder holder = new LinkTemplatesHolder();

            // Table-level
            LoadConfigurationSection(AttachmentLevel.onTable, holder);

            // Variable-level
            LoadConfigurationSection(AttachmentLevel.onVariable, holder);

            // Value-level
            LoadConfigurationSection(AttachmentLevel.onValue, holder);

            return holder;
        }

        /// <summary>
        /// Load sub section of the configuration file
        /// </summary>
        /// <param name="section">Name of the section</param>
        /// <param name="dictionary">Dictionary to store section data in</param>
        /// <returns></returns>
        private static void LoadConfigurationSection(AttachmentLevel attachmentLevel, LinkTemplatesHolder holder)
        {
            // Find all metaSystem nodes in section
            string section = attachmentLevel.ToString();

            string xpath = "/metaId/" + section + "/metaSystem";
            XmlNodeList xmlnodes = _xdoc.SelectNodes(xpath);

            foreach (XmlNode sysNode in xmlnodes)
            {
                string sysId = sysNode.Attributes["id"].Value;

                // Find all language nodes for the system
                xpath = ".//relationalGroup";
                XmlNodeList relationalGroupNodes = sysNode.SelectNodes(xpath);

                foreach (XmlNode relationalGroupNode in relationalGroupNodes)
                {
                    string linkType = relationalGroupNode.Attributes["type"].Value;
                    string linkRelation = relationalGroupNode.Attributes["relation"].Value;

                    // Find all language nodes for the system
                    xpath = ".//link";
                    XmlNodeList linkNodes = relationalGroupNode.SelectNodes(xpath);

                    foreach (XmlNode linkNode in linkNodes)
                    {
                        string pxLang = linkNode.Attributes["pxLang"].Value;
                        string labelStringFormat = linkNode.Attributes["labelStringFormat"].Value;
                        string urlStringFormat = linkNode.Attributes["urlStringFormat"].Value;

                        LinkTemplate temp = new LinkTemplate(sysId, labelStringFormat, urlStringFormat, linkType, linkRelation);
                        holder.Add(attachmentLevel, pxLang, temp);
                    }
                }
            }
        }


        /// <summary>
        /// Create links
        /// </summary>
        /// <param name="metaIdList">META-ID one or more</param>
        /// <param name="templates">templates for Language and attachmentlevel</param>
        /// <param name="textParams">0,1 or 2 text parameters, depending on attachmentleve</param>
        /// <returns></returns>
        private static List<Link> GetLinks(string metaIdList, List<LinkTemplate> templates, string[] textParams)
        {
            List<Link> myOut = new List<Link>();

            if (templates.Count == 0)
            {
                return myOut;
            }

            string[] metaIds = metaIdList.Split(_systemSeparator, StringSplitOptions.RemoveEmptyEntries);

            foreach (string metaId in metaIds)
            {
                foreach (LinkTemplate template in templates)
                {
                    if (!template.Match(metaId))
                    {
                        break;
                    }

                    string rawParamsString = metaId.Replace(template.MetaSysId, "");
                    string[] linkParams = rawParamsString.Split(_paramSeparator, StringSplitOptions.RemoveEmptyEntries);

                    myOut.Add(template.GetFormattedLink(textParams, linkParams));

                }
            }

            return myOut;
        }



        #region "Implementation of IMetaIdProvider"


        public static List<Link> GetTableLinks(string metaIdField, string language)
        {
            string[] textParams = new string[] { };
            return GetLinks(metaIdField, holder.GetTemplates(AttachmentLevel.onTable, language), textParams);
        }

        public static List<Link> GetVariableLinks(string metaIdField, string language, string variableLabel)
        {
            string[] textParams = new string[] { variableLabel };
            return GetLinks(metaIdField, holder.GetTemplates(AttachmentLevel.onVariable, language), textParams);
        }

        public static List<Link> GetValueLinks(string metaIdField, string language, string variableLabel, string valueLabel)
        {
            string[] textParams = new string[] { variableLabel, valueLabel };
            return GetLinks(metaIdField, holder.GetTemplates(AttachmentLevel.onValue, language), textParams);
        }
        #endregion

    }
}
