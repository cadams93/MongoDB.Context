using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoDB.Context
{
	public abstract class MongoContextBase : IDisposable
	{
		protected bool SubmittingChanges;
		private readonly MongoClient _Client;
		protected readonly Dictionary<Type, IMongoTrackedCollection> CollectionCache = new Dictionary<Type, IMongoTrackedCollection>();

		protected MongoContextBase() {}
		public MongoContextBase(MongoClient client)
		{
			_Client = client;
		}

		protected virtual IMongoTrackedCollection<TDocument, TIdField> GetCollection<TDocument, TIdField>()
			where TDocument : AbstractMongoEntityWithId<TIdField>
		{
			var type = typeof(IMongoTrackedCollection<TDocument, TIdField>);
			if (!CollectionCache.ContainsKey(type))
				CollectionCache.Add(type, new MongoTrackedCollection<TDocument, TIdField>(_Client));

			return (IMongoTrackedCollection<TDocument, TIdField>)CollectionCache[type];
		}

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
