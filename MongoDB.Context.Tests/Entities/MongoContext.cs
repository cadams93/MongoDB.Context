using MongoDB.Bson;
using MongoDB.Context.Tracking;
using MongoDB.Driver;

namespace MongoDB.Context.Tests.Entities
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
