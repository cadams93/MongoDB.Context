﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Context.Locking;
using MongoDB.Driver;

namespace MongoDB.Context
{
	/// <summary>
	/// Base implementation of a tracked collection of MongoDB entities
	/// Includes the ability to track, untrack and get changes since tracking began
	/// </summary>
	/// <typeparam name="TDocument">The .NET type of the MongoDB entity</typeparam>
	/// <typeparam name="TIdField">The .NET type of the ID field for the MongoDB entity</typeparam>
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

		/// <summary>
		/// Insert the given entity into the current context
		/// </summary>
		/// <param name="entity"></param>
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

			_TrackedCollection.Attach(entity, EntityState.Added);
		}

		/// <summary>
		/// Inserts the given entities into the current context (multi version of <see cref="InsertOnSubmit"/>)
		/// </summary>
		/// <param name="entities"></param>
		public void InsertAllOnSubmit(IEnumerable<TDocument> entities)
		{
			foreach (var entity in entities)
				InsertOnSubmit(entity);
		}

		/// <summary>
		/// Remove the given entity from the current context
		/// </summary>
		/// <param name="entity"></param>
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

			_TrackedCollection.Attach(entity, EntityState.Deleted);
		}

		/// <summary>
		/// Deletes the given entities from the current context (multi version of <see cref="DeleteOnSubmit"/>)
		/// </summary>
		/// <param name="entities"></param>
		public void DeleteAllOnSubmit(IEnumerable<TDocument> entities)
		{
			foreach (var entity in entities)
				DeleteOnSubmit(entity);
		}

		/// <summary>
		/// Get documents from the remote source (MongoDB collection)
		/// If the document has already been read into the context, the already tracked document is returned
		/// </summary>
		/// <param name="pred"></param>
		/// <returns></returns>
		public IEnumerable<TDocument> Find(Expression<Func<TDocument, bool>> pred = null)
		{
			foreach (var entity in RemoteGet(pred))
			{
				// Check if entity already tracked
				if (_TrackedCollection.Contains(entity._Id))
				{
					yield return _TrackedCollection[entity._Id].Entity;
					continue;
				}

				_TrackedCollection.Attach(entity, EntityState.ReadFromSource);

				yield return entity;
			}
		}

		/// <summary>
		/// Get the changes required to synchronise the current context with MongoDB, according to the tracked state of each entity
		/// </summary>
		/// <returns></returns>
		public MongoCollectionChangeSet<TDocument, TIdField> GetChanges()
		{
			var allTrackedEntities = _TrackedCollection.GetAllTrackedEntities().ToArray();

			var inserts = allTrackedEntities.Where(z => z.State == EntityState.Added).ToArray();
			var deletes = allTrackedEntities.Where(z => z.State == EntityState.Deleted).ToArray();

			var updates = allTrackedEntities.Where(z => z.State == EntityState.ReadFromSource)
				.ToDictionary(z => z.Entity, z => z.GetDifferences())
				.Where(z => z.Value.Any())
				.ToDictionary(z => z.Key, z => z.Value.AsEnumerable());

			return new MongoCollectionChangeSet<TDocument, TIdField>(inserts, updates, deletes);
		}

		/// <summary>
		/// Synchronises the tracked entities with MongoDB
		/// </summary>
		public void SubmitChanges()
		{
			var changeSet = GetChanges();
			var changeFactory = new MongoChangeFactory<TDocument, TIdField>();

			List<MongoLockRequest<TIdField>> locksRequired;

			var changes = changeFactory.GetMongoChangesFromChangeSet(changeSet, out locksRequired);
			if (!changes.Any()) return;

			if (locksRequired.Any())
			{
				using (var lp = new MongoLockProvider<TIdField>(locksRequired, _Client, "test", "locks"))
				{
					var success = lp.TryAcquireAll();
					if (!success)
						throw new Exception("Failed to acquire all required locks");

					// Updates requiring locks (plus Inserts and Deletes in same context)
					SubmitChangesImpl(changes);
					return;
				}
			}

			// Inserts and Deletes only
			SubmitChangesImpl(changes);
		}

		private void SubmitChangesImpl(IEnumerable<MongoChange<TDocument, TIdField>> changes)
		{
			var mongoChanges = changes.ToArray();

			var deletes = mongoChanges.Where(z => z.Change.ModelType == WriteModelType.DeleteOne).ToArray();
			if (deletes.Any())
			{
				var deleteResults = BulkWriteChanges(deletes);
			}

			var inserts = mongoChanges.Where(z => z.Change.ModelType == WriteModelType.InsertOne).ToArray();
			if (inserts.Any())
			{
				var insertResults = BulkWriteChanges(inserts);
			}

			var updates = mongoChanges.Where(z => z.Change.ModelType == WriteModelType.UpdateOne).ToArray();
			if (updates.Any())
			{
				var updateResults = BulkWriteChanges(updates);
			}

			// Cleanup the state of all of the tracked objects
			_TrackedCollection.CleanupEntityStateAfterSubmit();
		}

		private Dictionary<int, BulkWriteResult<TDocument>> BulkWriteChanges(IEnumerable<MongoChange<TDocument, TIdField>> changesForOperation, bool stopOnFailure = true)
		{
			var bulkWriteResults = new Dictionary<int, BulkWriteResult<TDocument>>();

			foreach (var mongoChangeGroup in changesForOperation.GroupBy(z => z.ExecutionOrder))
			{
				var result = _Collection.BulkWrite(mongoChangeGroup.Select(z => z.Change));
				bulkWriteResults.Add(mongoChangeGroup.Key, result);

				if (stopOnFailure && result.ProcessedRequests.Count() != result.RequestCount) return bulkWriteResults;
			}

			return bulkWriteResults;
		}

		protected virtual IEnumerable<TDocument> RemoteGet(Expression<Func<TDocument, bool>> pred = null)
		{
			return _Collection.FindSync(pred ?? (z => true)).ToList();
		}
	}
}
