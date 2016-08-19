using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Context.Client
{
    public class MongoContext : MongoContextBase
    {
        protected MongoContext() { }
        public MongoContext(MongoClient client) : base(client) { }

        public IMongoTrackedCollection<TestEntity, ObjectId> TestEntities
        {
            get { return GetCollection<TestEntity, ObjectId>(); }
        }
    }
}
