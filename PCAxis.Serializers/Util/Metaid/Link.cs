namespace PCAxis.Serializers.Util.MetaId
{
    /// <summary>
    /// Link to a metadata system. "Output"-class for meta-id.
    /// </summary>
    public class Link
    {

        /// <summary>
        /// Link text
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Link (URL)
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Content-type type like "text/html"
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// What type of information is the link to: about-statistics, statistics-homepage, definition-classification, definition-value ,,,
        /// </summary>
        public string Relation { get; set; }
    }
}
