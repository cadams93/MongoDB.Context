using System;
using MongoDB.Bson;

namespace MongoDB.Context.Tests
{
	public class MockMongoContext : MongoContext
	{
		private readonly TestEntity[] _TestEntities;

		public MockMongoContext(TestEntity[] testEntities)
		{
			_TestEntities = testEntities;
		}

		protected override IMongoTrackedCollection<TDocument, TIdField> GetCollection<TDocument, TIdField>()
		{
			if (typeof(TDocument) != typeof(TestEntity))
				throw new Exception("Trying to test with entities which have not been defined");

			var type = typeof(IMongoTrackedCollection<TestEntity, ObjectId>);
			if (!CollectionCache.ContainsKey(type))
				CollectionCache.Add(type, new MockMongoTrackedCollection<TestEntity, ObjectId>(_TestEntities));

			return (IMongoTrackedCollection<TDocument, TIdField>)CollectionCache[type];
		}
	}
}
