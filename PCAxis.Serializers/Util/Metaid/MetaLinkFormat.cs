namespace PCAxis.Serializers.Util.MetaId
{
    /// <summary>
    /// Class holding format information for a link
    /// </summary>
    internal class MetaLinkFormat
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
}
