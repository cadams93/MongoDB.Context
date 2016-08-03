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
		private readonly List<MongoLock<TIdField>> _LocksAcquired;

		public MongoLockProvider(IMongoClient client, string databaseKey, string collectionKey)
		{
			_Collection = client.GetDatabase(databaseKey).GetCollection<MongoLock<TIdField>>(collectionKey);
			_LocksAcquired = new List<MongoLock<TIdField>>();
		}

		public bool TryAcquireAll(IEnumerable<MongoLockRequest<TIdField>> requests, out List<MongoLock<TIdField>> acquiredLocks)
		{
			acquiredLocks = new List<MongoLock<TIdField>>();

			foreach (var request in requests)
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

				acquiredLocks.Add(dbLock);
			}

			_LocksAcquired.AddRange(acquiredLocks);

			return true;
		}

		public bool TryAcquireAny(IEnumerable<MongoLockRequest<TIdField>> requests, out List<MongoLock<TIdField>> acquiredLocks)
		{
			acquiredLocks = new List<MongoLock<TIdField>>();

			foreach (var request in requests)
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

				acquiredLocks.Add(dbLock);
			}

			_LocksAcquired.AddRange(acquiredLocks);

			return acquiredLocks.Any();
		}

		private void ReleaseLocks(IEnumerable<MongoLock<TIdField>> locks)
		{
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