using System.Collections.Generic;

namespace PCAxis.Serializers.Util.Metaid
{
    enum AttachmentLevel { onTable, onVariable, onValue }

    internal class LinkTemplatesHolder : Dictionary<string, Dictionary<string, List<LinkTemplate>>>
    {
        //string attachmentLevel, string language, List<LinkTemplate>

        public void Add(AttachmentLevel attachmentLevel, string language, LinkTemplate linkTemplate)
        {
            string level_string = attachmentLevel.ToString();

            if (!this.ContainsKey(level_string))
            {
                this[level_string] = new Dictionary<string, List<LinkTemplate>>();
            }

            if (!this[level_string].ContainsKey(language))
            {
                this[level_string][language] = new List<LinkTemplate>();
            }

            this[level_string][language].Add(linkTemplate);
        }


        public List<LinkTemplate> GetTemplates(AttachmentLevel attachmentLevel, string language)
        {
            string level_string = attachmentLevel.ToString();

            if (!this.ContainsKey(level_string))
            {
                return new List<LinkTemplate>();
            }

            if (!this[level_string].ContainsKey(language))
            {
                return new List<LinkTemplate>();
            }

            return this[level_string][language];

        }



    }
}
