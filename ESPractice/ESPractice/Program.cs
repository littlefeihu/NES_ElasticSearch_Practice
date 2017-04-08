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

                string indexName = "customer";
                Uri uri = new Uri("http://localhost:9200");
                ConnectionSettings settings = new ConnectionSettings(uri);
                settings.DisableDirectStreaming().DefaultIndex(indexName);
                ElasticClient client = new ElasticClient(settings);

                dynamic response = client.GetIndex(indexName);
                if (!response.IsValid)
                {
                    response = client.CreateIndex("customer");
                    Console.WriteLine(response);
                }

                var company = new Company { CompanyID = 2, Name = "IBM" };
                //创建文档并指定索引，和文档ID
                response = client.Index(company, i => i.Index(indexName).Id(company.CompanyID));
                //创建文档并指定索引，文档类型，未指定文档ID,ElasticSearch会帮你生成一个随机的ID
                response = client.Index(company, i => i.Index(indexName));
                //这里没有显示指定索引，所以使用的客户端初始化默认索引，如果没有默认索引，就会发生错误
                response = client.Index(company);

                company = new Company { CompanyID = 2, Name = "联想" };
                //修改文档
                response = client.Index(company, i => i.Id(company.CompanyID));

                company = new Company { CompanyID = 2, Name = "联想" };
                //修改指定版本的文档
                response = client.Index(company, i => i.Id(company.CompanyID).Version(1));


                //根据特定的条件来删除文档
                response = client.Delete<Company>(company.CompanyID, d => d.Index(indexName));
                //删除文档
                response = client.Delete<Company>(company.CompanyID);

                //根据名称删除索引
                response = client.DeleteIndex(indexName);

                //批量创建文档
                BulkDescriptor descriptor = new BulkDescriptor();
                descriptor.Index<Company>(op => op.Document(new Company { CompanyID = 1, Name = "IBM" }).Index("a"));
                descriptor.Index<Company>(op => op.Document(new Company { CompanyID = 2, Name = "IBM" }).Index("b"));
                descriptor.Index<Company>(op => op.Document(new Company { CompanyID = 3, Name = "IBM" }).Index("c"));

                response = client.Bulk(descriptor);

                return;

                //var mappingResponse = client.GetFieldMapping<Doc>(new string[] { "created", "updated" });


                var keyword = String.Format("*{0}*", "数据");
                QueryContainer query = new TermQuery
                {
                    Field = "docID",
                    Value = "371"
                };

                var searchRequest = new SearchRequest
                {
                    From = 0,
                    Size = 10,
                    Query = query

                };

                var searchResults = client.Search<Doc1>(searchRequest);
                Console.WriteLine(searchResults.Documents.Count);

                client.Map<Doc1>(sm => sm.Analyzer("ik_max_word"));
                var mapping = client.GetMapping<Doc1>();
                Console.WriteLine(mapping);
                searchResults = client.Search<Doc1>(s => s
                                                       .From(0)
                                                       .Size(10)
                                                       .Query(q => q
                                                            .Term(p => p.CreatedBy, "allen")
                                                       )
                                                    );



                var searchResults1 = client.Search<Doc>(s => s.Index(indexName).Type("doc1").Query(q => q.Term(p => p.CreatedBy, "allen") || q.DateRange(d => d.GreaterThanOrEquals("2017-04-02"))));

                searchResults = client.Search<Doc1>(s => s.Query(q => q.Term(p => p.CreatedBy, "allen") && q.MatchAll()));


                searchResults = client.Search<Doc1>(s => s.Query(q => (q.Term(p => p.CreatedBy, "allen") || q.Term(p => p.Name, "Foo")) && q.MatchAll()));
                searchResults = client.Search<Doc1>(s => s.Query(q => (q.Term(p => p.CreatedBy, "allen") && !q.Term(p => p.Name, "foo"))));
                searchResults = client.Search<Doc1>(s => s.Query(q => q.Term(p => p.Name, "foo")));
                // searchResults = client.Search<Doc1>(s => s.Query(q => q.Term(p => "doc1.name", "foo")));
                searchResults = client.Search<Doc1>(s => s.Query(q => q.QueryString(qs => qs.Query("*foo*"))));
                searchResults = client.Search<Doc1>(s => s.Query(q => q.Bool(bq => bq.Must(mq => mq.Term(p => p.CreatedBy, "allen"), eq => !eq.Term(p => p.Name, "foo")))));

                searchResults = client.Search<Doc1>(s => s.Query(q => q.DateRange(d => d.Field(df => df.Created).GreaterThanOrEquals("2017-04-02").LessThanOrEquals("2017-04-05"))));

                searchResults = client.Search<Doc1>(s => s.Query(q => q.Range(d => d.Field(df => df.DocID).GreaterThan(800))));

                searchResults = client.Search<Doc1>(s => s.Query(q => q.Raw(@"{""match_all"": {} }")));

                searchResults = client.Search<Doc1>(s => s.PostFilter(q => q.Raw(@"{""from"":0,""size"":10,""query"":{ ""term"":{""docID"":{ ""value"":""371""}}}}")));

                searchResults = client.Search<Doc1>();

                Doc1 d1 = new Doc1();
                d1.DocID = DateTime.Now.Millisecond;
                d1.Name = "应用程序层是一个附加层" + DateTime.Now.ToString();
                d1.Created = DateTime.Now;
                d1.Updated = DateTime.Now;
                d1.Content = @"应用程序层是一个附加层，介于领域层和UI之间，是你编排用例实现的地方，其中包含的方法几乎一一对应于表现层的用例，

                一般情况下，应用程序层和表现层一一对应，因为不同的表现层可能会有不同的用例，

                应用程序层引用领域层和基础设施层，对业务逻辑一无所知，不包含任何与业务相关的状态信息，

                应用程序层有时候需要调用外部服务，比如WCF或者WebApi,又或者是第三方的服务，这种情况一般是把对外部服务的调用封装成适配器，放在基础设置层，

                这样就把对外部服务的调用转化成了对基础设施层的调用。";
                d1.CreatedBy = "allen";
                Doc1 d2 = new Doc1();
                d2.DocID = DateTime.Now.Millisecond;
                d2.Name = "领域层包含了几乎所有的业务逻辑";
                d2.Created = DateTime.Now;
                d2.Updated = DateTime.Now;
                d2.Content = @"领域层包含了几乎所有的业务逻辑，由一组领域模型和一组服务构成，

                领域模型：

                包含数据和行为，与之相对的一个是贫血模型，什么是贫血模型，如果只是类缺少方法，对象模型并不算是贫血，如果实体的逻辑

                放在了实体类的外面，那才是真的贫血，毕竟如果把逻辑放到了实体类的外面，他实际上是违反了说，别问原则

                领域服务：

                它包含了一些逻辑上有关系并且操作多个实体的行为，";

                var d1add = client.Index(d1, i => i.Index(indexName).Type(typeof(Doc1)).Id(d1.DocID));
                Console.WriteLine("D1 Add Response: " + d1add);
                var d2add = client.Index(d2, i => i.Index(indexName).Type(typeof(Doc1)).Id(d2.DocID));
                Console.WriteLine("D2 Add Response: " + d2add);

                //var d1remove = client.Delete<Doc1>(1, d => d.Index("indexName"));

                //Console.WriteLine("D1 Reove Response: " + d1remove);

                //client.Delete(new DeleteRequest(indexName,typeof(Doc1).ToString(),1));

            }
            catch (Exception e)
            {

            }
        }
    }

    public class Company
    {
        public string Name { get; set; }
        public int CompanyID { get; set; }
    }
    public class Doc1
    {
        public int DocID;
        public string Name;
        public string Content;
        public DateTime Created;
        public DateTime Updated;
        public string CreatedBy;

    }
    public class Doc
    {
        public int DocID;
        public string Name;
        public string Content;
        public DateTime Created;
        public DateTime Updated;
        public string CreatedBy;

    }
}
