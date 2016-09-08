using System;
using System.Threading;
using MongoDB.Bson;
using MongoDB.Context.Tests.Entities;
using MongoDB.Context.Tracking;

namespace MongoDB.Context.Tests
{
	public class MockMongoContext : MongoContext
	{
		// Used for concurrent .SubmitChanges() call testing
		public readonly ManualResetEvent WithinSubmitEvent = new ManualResetEvent(false);

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

		public override void SubmitChanges()
		{
			if (SubmittingChanges)
				throw new Exception("Already submitting changes");

			try
			{
				SubmittingChanges = true;

				WithinSubmitEvent.WaitOne();

				foreach (var collection in CollectionCache.Values)
					collection.SubmitChanges();
			}
			finally
			{
				SubmittingChanges = false;
			}
		}
	}
}
