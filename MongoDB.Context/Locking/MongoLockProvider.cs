using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;

namespace MongoDB.Context.Locking
{
	public sealed class MongoLockProvider<TIdField> : IDisposable
	{
		private readonly Guid _ClientId = Guid.NewGuid();

		private readonly IMongoCollection<MongoLock<TIdField>> _Collection;
		private readonly List<MongoLockRequest<TIdField>> _LockRequests;
		private readonly List<MongoLock<TIdField>> _LocksAcquired;

		public MongoLockProvider(IEnumerable<MongoLockRequest<TIdField>> lockRequests, IMongoClient client, string databaseKey, string collectionKey)
		{
			_Collection = client.GetDatabase(databaseKey).GetCollection<MongoLock<TIdField>>(collectionKey);
			_LocksAcquired = new List<MongoLock<TIdField>>();
			_LockRequests = lockRequests.ToList();
		}

		public bool TryAcquireAll()
		{
			foreach (var request in _LockRequests)
			{
				var dbLock = _Collection.FindOneAndUpdate(
					Builders<MongoLock<TIdField>>.Filter.And(
						Builders<MongoLock<TIdField>>.Filter.Eq(z => z.DocumentId, request.DocumentId),
						Builders<MongoLock<TIdField>>.Filter.Eq(z => z.Field, request.Field)
					), 
					Builders<MongoLock<TIdField>>.Update
						.SetOnInsert(z => z.DocumentId, request.DocumentId)
						.SetOnInsert(z => z.Field, request.Field)
						.SetOnInsert(z => z.TakenBy, _ClientId)
						.SetOnInsert(z => z.TakenAt, DateTime.UtcNow), 
					new FindOneAndUpdateOptions<MongoLock<TIdField>>
					{
						IsUpsert = true, 
						ReturnDocument = ReturnDocument.After
					});

				// If there was an existing lock, we have failed to acquire all
				if (dbLock.TakenBy != _ClientId)
				{
					ReleaseLocks(_LocksAcquired);
					return false;
				}

				_LocksAcquired.Add(dbLock);
			}

			return true;
		}

		public bool TryAcquireAny(out List<MongoLock<TIdField>> acquiredLocks)
		{
			acquiredLocks = new List<MongoLock<TIdField>>();

			foreach (var request in _LockRequests)
			{
				var dbLock = _Collection.FindOneAndUpdate(
					Builders<MongoLock<TIdField>>.Filter.And(
						Builders<MongoLock<TIdField>>.Filter.Eq(z => z.DocumentId, request.DocumentId),
						Builders<MongoLock<TIdField>>.Filter.Eq(z => z.Field, request.Field)
					), 
					Builders<MongoLock<TIdField>>.Update
						.SetOnInsert(z => z.DocumentId, request.DocumentId)
						.SetOnInsert(z => z.Field, request.Field)
						.SetOnInsert(z => z.TakenBy, _ClientId)
						.SetOnInsert(z => z.TakenAt, DateTime.UtcNow), 
					new FindOneAndUpdateOptions<MongoLock<TIdField>>
					{
						IsUpsert = true, 
						ReturnDocument = ReturnDocument.After
					});

				// If there was an existing lock, we have failed to acquire this lock
				if (dbLock.TakenBy != _ClientId) continue;

				_LocksAcquired.Add(dbLock);
				acquiredLocks.Add(dbLock);
			}
			
			return acquiredLocks.Any();
		}

		private void ReleaseLocks(IEnumerable<MongoLock<TIdField>> locks)
		{
			// TODO: Potentially just destroy any locks with this client ID

			foreach (var dbLock in locks)
			{
				// Delete each lock if the clientId matches
				_Collection.FindOneAndDelete(
					Builders<MongoLock<TIdField>>.Filter.And(
						Builders<MongoLock<TIdField>>.Filter.Eq(z => z.DocumentId, dbLock.DocumentId),
						Builders<MongoLock<TIdField>>.Filter.Eq(z => z.Field, dbLock.Field),
						Builders<MongoLock<TIdField>>.Filter.Eq(z => z.TakenBy, _ClientId)
					)
				);
			}
		}

		public void Dispose()
		{
			ReleaseLocks(_LocksAcquired);
		}
	}
}