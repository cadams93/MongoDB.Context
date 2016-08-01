using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Driver;

namespace MongoDB.Context
{
	public class MongoTrackedCollection<TDocument, TIdField> 
		: IMongoTrackedCollection<TDocument, TIdField> 
		where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		private readonly IMongoCollection<TDocument> _Collection;
		private readonly TrackedCollection<TDocument, TIdField> _TrackedCollection = new TrackedCollection<TDocument, TIdField>();

		public MongoTrackedCollection(IMongoClient client, string databaseKey, string collectionKey)
		{
			if (client != null)
				_Collection = client.GetDatabase(databaseKey).GetCollection<TDocument>(collectionKey);
		}

		public void InsertOnSubmit(TDocument entity)
		{
			// Check if entity already tracked
			if (_TrackedCollection.Contains(entity))
			{
				var trackedEntity = _TrackedCollection[entity];

				switch (trackedEntity.State)
				{
					case EntityState.Added:
					case EntityState.ReadFromSource:
						throw new Exception("Attempting to insert an entity which already exists");
					case EntityState.Deleted:
						trackedEntity.State = EntityState.ReadFromSource;
						break;
					case EntityState.NoActionRequired:
						trackedEntity.State = EntityState.Added;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				return;
			}

			_TrackedCollection.Add(entity, EntityState.Added);
		}

		public void InsertAllOnSubmit(IEnumerable<TDocument> entities)
		{
			foreach (var entity in entities)
				InsertOnSubmit(entity);
		}

		public void DeleteOnSubmit(TDocument entity)
		{
			// Check if entity already tracked
			if (_TrackedCollection.Contains(entity))
			{
				var trackedEntity = _TrackedCollection[entity];

				switch (trackedEntity.State)
				{
					case EntityState.Added:
						trackedEntity.State = EntityState.NoActionRequired;
						break;
					case EntityState.Deleted:
						throw new Exception("Attempting to delete an entity which has already been queued for deletion");
					case EntityState.ReadFromSource:
					case EntityState.NoActionRequired:
						trackedEntity.State = EntityState.Deleted;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				return;
			}

			_TrackedCollection.Add(entity, EntityState.Deleted);
		}

		public void DeleteAllOnSubmit(IEnumerable<TDocument> entities)
		{
			foreach (var entity in entities)
				DeleteOnSubmit(entity);
		}

		public IEnumerable<TDocument> Find(Expression<Func<TDocument, bool>> pred = null)
		{
			foreach (var entity in RemoteGet(pred))
			{
				// Check if entity already tracked
				if (_TrackedCollection.Contains(entity))
				{
					//TODO: Fix this
					// Do something else
					yield break;
				}

				_TrackedCollection.Add(entity, EntityState.ReadFromSource);

				yield return entity;
			}
		}

		public MongoChangeSet<TDocument, TIdField> GetChanges()
		{
			var allTrackedEntities = _TrackedCollection.GetAllTrackedEntities().ToArray();

			var inserts = allTrackedEntities.Where(z => z.State == EntityState.Added).ToArray();
			var deletes = allTrackedEntities.Where(z => z.State == EntityState.Deleted).ToArray();

			var updates = allTrackedEntities.Where(z => z.State == EntityState.ReadFromSource)
				.ToDictionary(z => z.Entity, z => z.GetDifferences())
				.Where(z => z.Value.Any())
				.ToDictionary(z => z.Key, z => z.Value.AsEnumerable());

			return new MongoChangeSet<TDocument, TIdField>(inserts, updates, deletes);
		}

		protected virtual IEnumerable<TDocument> RemoteGet(Expression<Func<TDocument, bool>> pred = null)
		{
			return _Collection.FindSync(pred ?? (z => true)).ToList();
		}
	}
}
