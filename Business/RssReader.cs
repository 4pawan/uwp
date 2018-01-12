using System.ServiceModel.Syndication;

namespace UnicornWidget.Business
{
    public class RssReader
    {
        private string feedUrl;

        public RssReader(string feedUrl)
        {
            this.feedUrl = feedUrl;
        }

        public SyndicationFeed GetSyndicationFeed()
        {
            try
            {
                using (var reader = System.Xml.XmlReader.Create(feedUrl))
                {
                    return SyndicationFeed.Load(reader);
                }
            }
            catch (System.Exception)
            {
                return null;
            }
        }
    }
}