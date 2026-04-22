using System.Collections.Generic;

namespace PCAxis.Serializers.Util.MetaId
{
    /// <summary>
    /// Class that encapsulates a metaid.config file containing link-definitions to metadata systems,
    /// and has methods that resolves a "raw" meta-id string to Links.
    /// This is a static wrapper.
    /// </summary>
    public static class MetaIdResolverStatic
    {

        /// <summary>
        /// Logging to Log4Net
        /// </summary>
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(MetaIdResolverStatic));

        /// <summary>
        /// Character that separates the systems within a META-ID
        /// </summary>
        public static readonly char[] _systemSeparator = { ',', ' ' };

        /// <summary>
        /// Character that separates the parameters within a system META-ID
        /// </summary>
        private static readonly char[] _paramSeparator = { ':' };


        private static readonly MetaIdResolver Instance = new MetaIdResolver();

        public static List<Link> GetTableLinks(string metaIdField, string language)
        {
            return Instance.GetTableLinks(metaIdField, language);
        }

        public static List<Link> GetVariableLinks(string metaIdField, string language, string variableLabel)
        {
            return Instance.GetVariableLinks(metaIdField, language, variableLabel);
        }

        public static List<Link> GetValueLinks(string metaIdField, string language, string variableLabel, string valueLabel)
        {
            return Instance.GetValueLinks(metaIdField, language, variableLabel, valueLabel);
        }

    }
}
