using System;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDB.Context
{
	public class MongoContext : IDisposable
	{
		protected readonly MongoClient _Client;

		public MongoContext(MongoClient client)
		{
			_Client = client;
		}

		#region Entities

		protected IMongoTrackedCollection<TestEntity, ObjectId> _TestEntities;

		public virtual IMongoTrackedCollection<TestEntity, ObjectId> TestEntities
		{
			get { return _TestEntities ?? (_TestEntities = new MongoTrackedCollection<TestEntity, ObjectId>(_Client, "test", "testCollection")); }
		}

		#endregion

		public void Dispose()
		{
			_TestEntities = null;
		}
	}
}
