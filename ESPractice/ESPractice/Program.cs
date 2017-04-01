using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPractice
{
    class Program
    {
        static void Main(string[] args)
        {
         try
          {
            string indexName = "idx";
            Uri uri = new Uri("http://localhost:9200");
            ConnectionSettings settings = new ConnectionSettings(uri);
            settings.PrettyJson()
                    .DefaultIndex(indexName);

            ElasticClient client = new ElasticClient(settings);
            client.CreateIndex(indexName);

            Doc d1 = new Doc();
            d1.DocID = DateTime.Now.Millisecond;
            d1.Name = "foo"+DateTime.Now.ToString();

            Doc d2 = new Doc();
            d2.DocID = DateTime.Now.Millisecond;
            d2.Name = "bar" + DateTime.Now.ToString();

            var d1add = client.Index(d1, i => i.Index(indexName).Type(typeof(Doc)).Id(d1.DocID));
            Console.WriteLine("D1 Add Response: " + d1add);
            var d2add = client.Index(d2, i => i.Index(indexName).Type(typeof(Doc)).Id(d2.DocID));
            Console.WriteLine("D2 Add Response: " + d2add);

            var d1remove = client.Delete<Doc>(1, d => d.Index("indexName"));
        
            Console.WriteLine("D1 Reove Response: " + d1remove);
        }
        catch (Exception e)
        {
      
        }
        }
    }
    public class Doc
    {
        public int DocID;
        public string Name;
    }
}
