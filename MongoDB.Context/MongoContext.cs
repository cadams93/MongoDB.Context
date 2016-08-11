using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDB.Context
{
	public class MongoContext : IDisposable
	{
		protected bool SubmittingChanges;
		private readonly MongoClient _Client;
		protected readonly Dictionary<Type, IMongoTrackedCollection> CollectionCache = new Dictionary<Type, IMongoTrackedCollection>();

		protected MongoContext() {}
		public MongoContext(MongoClient client)
		{
			_Client = client;
		}

		#region Entities

		public IMongoTrackedCollection<TestEntity, ObjectId> TestEntities
		{
			get { return GetCollection<TestEntity, ObjectId>(); }
		}

		protected virtual IMongoTrackedCollection<TDocument, TIdField> GetCollection<TDocument, TIdField>()
			where TDocument : AbstractMongoEntityWithId<TIdField>
		{
			var type = typeof(IMongoTrackedCollection<TDocument, TIdField>);
			if (!CollectionCache.ContainsKey(type))
				CollectionCache.Add(type, new MongoTrackedCollection<TestEntity, ObjectId>(_Client));

			return (IMongoTrackedCollection<TDocument, TIdField>)CollectionCache[type];
		}

		#endregion

		public virtual void SubmitChanges()
		{
			if (SubmittingChanges)
				throw new Exception("Already submitting changes");

			try
			{
				SubmittingChanges = true;

				foreach (var collection in CollectionCache.Values)
					collection.SubmitChanges();
			}
			finally
			{
				SubmittingChanges = false;
			}
		}

		public void Dispose()
		{
			CollectionCache.Clear();
		}
	}
}
