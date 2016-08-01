using MongoDB.Bson;

namespace MongoDB.Context.Tests
{
	public class MockMongoContext : MongoContext
	{
		private readonly TestEntity[] _MockTestEntities;

		public MockMongoContext(TestEntity[] testEntities)
			: base(null)
		{
			_MockTestEntities = testEntities;
		}

		public override IMongoTrackedCollection<TestEntity, ObjectId> TestEntities
		{
			get { return _TestEntities ?? (_TestEntities = new MockMongoTrackedCollection<TestEntity, ObjectId>(_MockTestEntities)); }
		}
	}
}
