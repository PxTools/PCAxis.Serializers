using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;


namespace PCAxis.Serializers.Util.MetaId
{
    /// <summary>
    /// Class that encapsulates a metaid.config file containing link-definitions to metadata systems,
    /// and has methods that resolves a "raw" meta-id string to Links.
    /// The non-static version, which may become dependency injected in the future. 
    /// </summary>
    internal class MetaIdResolver
    {
        #region Private fields

        /// <summary>
        /// Configuration file (LINQ to XML)
        /// </summary>
        private readonly XDocument _xdoc;

        /// <summary>
        /// Logging to Log4Net
        /// </summary>
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(MetaIdResolver));

        /// <summary>
        /// Character that separates the systems within a META-ID
        /// </summary>
        public static readonly char[] _systemSeparator = { ',', ' ' };

        /// <summary>
        /// Character that separates the parameters within a system META-ID
        /// </summary>
        private static readonly char[] _paramSeparator = { ':' };

        /// <summary>
        /// Holds all data loaded from config, reshuffled so using it easy.
        /// </summary>
        private readonly LinkTemplatesHolder holder;

        /// <summary>
        /// Get the root folder for labels files.
        /// May be given onthe metaid-element (the xml-root), if so it is used, else the dll (AppContext.BaseDirectory) folder is used.
        /// </summary>
        private readonly string _LabelFilesFolder;

        #endregion

        //Used by the static version
        internal MetaIdResolver() : this(null, new FileGetter()) { }

        //now, only used directly by tests. 
        internal MetaIdResolver(string configurationFile, IFileGetter filereader)
        {
            this.holder = new LinkTemplatesHolder();

            this._xdoc = filereader.ReadConfig(configurationFile);
            if (this._xdoc == null)
            {
                return;
            }
            //for "manual" labels files.
            _LabelFilesFolder = this._xdoc.Root?.Attribute("labelFilesFolder")?.Value ?? AppContext.BaseDirectory;

            // Table-level
            LoadConfigurationSection(AttachmentLevel.onTable, this.holder, filereader);

            // Variable-level
            LoadConfigurationSection(AttachmentLevel.onVariable, this.holder, filereader);

            // Value-level
            LoadConfigurationSection(AttachmentLevel.onValue, this.holder, filereader);

        }

        /// <summary>
        /// Load sub section of the configuration file
        /// </summary>
        /// <param name="attachmentLevel">Name of the section</param>
        /// <param name="holder">To store section data in</param>
        /// <returns></returns>
        private void LoadConfigurationSection(AttachmentLevel attachmentLevel, LinkTemplatesHolder holder, IFileGetter filereader)
        {
            string sectionName = attachmentLevel.ToString();

            var metaSystems = this._xdoc
                .Root?
                .Element(sectionName)?
                .Elements("metaSystem");

            if (metaSystems == null)
                return;

            foreach (var sysNode in metaSystems)
            {
                string sysId = (string)sysNode.Attribute("id");
                if (string.IsNullOrEmpty(sysId))
                {
                    continue;
                }

                foreach (var relationalGroup in sysNode.Elements("relationalGroup"))
                {
                    string linkType = (string)relationalGroup.Attribute("type");
                    string linkRelation = (string)relationalGroup.Attribute("relation");

                    foreach (var linkNode in relationalGroup.Elements("link"))
                    {
                        string pxLang = (string)linkNode.Attribute("pxLang");
                        if (string.IsNullOrEmpty(pxLang))
                        {
                            continue;
                        }

                        string labelsFilename = GetLabelsFilename(linkNode.Attribute("labelsFile"));

                        Dictionary<string, string> labelsFile = filereader.ReadLabelsfile(labelsFilename);

                        string labelStringFormat = (string)linkNode.Attribute("labelStringFormat");

                        string urlStringFormat = (string)linkNode.Attribute("urlStringFormat");

                        LinkTemplate template = new LinkTemplate(
                            sysId,
                            labelStringFormat,
                            labelsFile,
                            urlStringFormat,
                            linkType,
                            linkRelation);

                        holder.Add(attachmentLevel, pxLang, template);
                    }
                }
            }
        }

        #region Labels file


        private string GetLabelsFilename(XAttribute labelsFileAttr)
        {
            string filename = labelsFileAttr?.Value;
            if (string.IsNullOrEmpty(filename))
            {
                return "";
            }
            return Path.Combine(this._LabelFilesFolder, filename);
        }


        #endregion

        #region Link creation

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
                        continue;
                    }

                    string rawParamsString = metaId.Replace(template.MetaSysId, "");

                    string[] linkParams = rawParamsString.Split(_paramSeparator, StringSplitOptions.RemoveEmptyEntries);

                    myOut.Add(template.GetFormattedLink(textParams, linkParams, metaId));
                }
            }

            return myOut;
        }

        #endregion

        #region IMetaIdProvider

        public List<Link> GetTableLinks(string metaIdField, string language)
        {
            return GetLinks(metaIdField, this.holder.GetTemplates(AttachmentLevel.onTable, language), Array.Empty<string>());
        }

        public List<Link> GetVariableLinks(string metaIdField, string language, string variableLabel)
        {
            return GetLinks(metaIdField, this.holder.GetTemplates(AttachmentLevel.onVariable, language), new[] { variableLabel });
        }

        public List<Link> GetValueLinks(string metaIdField, string language, string variableLabel, string valueLabel)
        {
            return GetLinks(metaIdField, this.holder.GetTemplates(AttachmentLevel.onValue, language), new[] { variableLabel, valueLabel });
        }

        #endregion
    }
}
