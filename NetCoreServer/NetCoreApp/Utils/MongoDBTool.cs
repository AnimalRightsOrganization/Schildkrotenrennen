using System;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace NetCoreServer.Utils
{
    public class MongoDBTool
    {
        protected MongoClient client;

        //const string connectionString = "mongodb://localhost";
        //const string connectionString = "mongodb+srv://<username>:<password>@<cluster-address>/test?w=majority";
        const string connectionString = "mongodb://localhost:27017/?readPreference=primary&directConnection=true&ssl=false";

        public MongoDBTool()
        {
            client = new MongoClient(connectionString);
            Debug.Print($"connect to: {connectionString}");
        }

        public void Connect() { }
        private void Dispose()
        {
            client = null;
        }

        public void Insert()
        {

        }
        public void Delete() { }
        public void Update() { }
        public void Query()
        {
            /*
            var search1 = client.ListDatabaseNames().ToList();
            Debug.Print($"有{search1.Count}个database");
            for (int i = 0; i < search1.Count; i++)
            {
                Debug.Print($"[{i}]---{search1[i]}");
            }
            //有4个database
            //[0]-- - admin
            //[1]-- - config
            //[2]-- - local
            //[3]-- - stickerDB
            */

            //  根据数据库名称实例化数据库
            var database = client.GetDatabase("stickerDB");

            // 根据集合名称获取集合
            var collection = database.GetCollection<BsonDocument>("Users");
            var filter = new BsonDocument();

            // 查询集合中的文档
            var search2 = Task.Run(async () => await collection.Find(filter).ToListAsync()).Result;
            // 循环遍历输出
            search2.ForEach(p =>
            {
                Debug.Print($"姓名：{p["name"]}，球衣号码：{p["number"]}");
            });

            //$lt    <   (less  than)
            //$lte   <=  (less than  or equal to)
            //$gt    >   (greater  than)
            //$gte   >=  (greater  than or equal to)
            //var filter3 = Builders<BsonDocument>.Filter.Eq("number", 10); //等于
            var filter3 = Builders<BsonDocument>.Filter.Gte("number", 10); //大于等于
            var result = collection.Find(filter3);
            Debug.Print($"result={result.CountDocuments()}");
        }
    }
}