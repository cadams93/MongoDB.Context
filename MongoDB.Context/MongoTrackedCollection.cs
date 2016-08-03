using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Context.Locking;
using MongoDB.Driver;

namespace MongoDB.Context
{
	public class MongoTrackedCollection<TDocument, TIdField> 
		: IMongoTrackedCollection<TDocument, TIdField> 
		where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		private readonly IMongoClient _Client = null;
		private readonly IMongoCollection<TDocument> _Collection = null;
		private readonly TrackedCollection<TDocument, TIdField> _TrackedCollection = new TrackedCollection<TDocument, TIdField>();

		public MongoTrackedCollection(IMongoClient client, string databaseKey, string collectionKey)
		{
			if (client != null)
			{
				_Collection = client.GetDatabase(databaseKey).GetCollection<TDocument>(collectionKey);
				_Client = client;
			}
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

		public void SubmitChanges()
		{
			var allTrackedEntities = _TrackedCollection.GetAllTrackedEntities().ToArray();

			var inserts = allTrackedEntities.Where(z => z.State == EntityState.Added).ToArray();
			var deletes = allTrackedEntities.Where(z => z.State == EntityState.Deleted).ToArray();

			var updates = allTrackedEntities.Where(z => z.State == EntityState.ReadFromSource)
				.ToDictionary(z => z.Entity, z => z.GetDifferences())
				.Where(z => z.Value.Any())
				.ToDictionary(z => z.Key, z => z.Value.AsEnumerable());

			var insertModels = inserts.Select(inserrt => new InsertOneModel<TDocument>(inserrt.Entity)).ToArray();
			var deleteModels = deletes.Select(delete => new DeleteOneModel<TDocument>(Builders<TDocument>.Filter.Eq(z => z._Id, delete.Entity._Id))).ToArray();

			var lockRequests = updates.SelectMany(z => z.Value.Select(x => new MongoLockRequest<TIdField> { DocumentId = z.Key._Id, Field = x.RootDocumentField })).ToArray();

			var updateModels = updates.Select(update =>
				new UpdateOneModel<TDocument>(
					Builders<TDocument>.Filter.Eq(z => z._Id, update.Key._Id),
					Builders<TDocument>.Update.Combine(update.Value.Select(diff => diff.GetMongoUpdate())))
				)
				.ToArray();

			// TODO: Check results here
			if (deleteModels.Any()) _Collection.BulkWrite(deleteModels);
			if (insertModels.Any()) _Collection.BulkWrite(insertModels);

			if (updateModels.Any())
			{
				// TODO: More complex locking - handle lock failures
				using (var lp = new MongoLockProvider<TIdField>(_Client, "test", "lock"))
				{
					List<MongoLock<TIdField>> acquiredLocks;
					var success = lp.TryAcquireAll(lockRequests, out acquiredLocks);
					if (success)
					{
						_Collection.BulkWrite(updateModels);
					}
				}
			}
		}

		protected virtual IEnumerable<TDocument> RemoteGet(Expression<Func<TDocument, bool>> pred = null)
		{
			return _Collection.FindSync(pred ?? (z => true)).ToList();
		}
	}
}
