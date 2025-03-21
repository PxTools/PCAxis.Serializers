using System;
using System.Configuration;

using PCAxis.Serializers.Util.MetaId;


namespace PCAxis.Serializers.Util.Metaid
{
    /// <summary>
    /// Class with template for a link
    /// </summary>
    internal class LinkTemplate
    {
        private readonly string _metaSysId;

        public LinkTemplate(string metaSysId, string textFormat, string linkFormat, string linkType, string linkRelation)
        {
            if (string.IsNullOrWhiteSpace(metaSysId))
            {
                throw new ConfigurationErrorsException("MetaId config: metaSystem element must have non null id");
            }

            if (string.IsNullOrWhiteSpace(linkFormat))
            {
                throw new ConfigurationErrorsException("MetaId config: urlStringFormat must have non null value");
            }

            this.LinkTextFormat = textFormat;
            this.LinkUrlFormat = linkFormat;
            this.LinkType = linkType;
            this.LinkRelation = linkRelation;
            this._metaSysId = metaSysId;
        }


        public bool Match(string metaIdString)
        {
            return metaIdString.StartsWith(MetaSysId);
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

        public string MetaSysId => _metaSysId;

        /// <summary>
        /// Returns a completed/ready/formatted Link
        /// </summary>
        /// <param name="textParams"></param>
        /// <param name="linkParams"></param>
        /// <returns></returns>
        public Link GetFormattedLink(string[] textParams, string[] linkParams, string metaId)
        {
            Link link = new Link();
            link.Relation = this.LinkRelation;
            link.Type = this.LinkType;
            link.Url = FormatText(this.LinkUrlFormat, linkParams);
            link.Label = FormatText(this.LinkTextFormat, textParams);
            link.MetaId = metaId;
            return link;
        }


        private static string FormatText(string text, string[] formatParams)
        {
            if (String.IsNullOrEmpty(text))
            {
                return text;
            }
            try
            {
                return String.Format(text, formatParams);
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}
