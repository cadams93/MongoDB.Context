using System;
using MongoDB.Bson;
using MongoDB.Context.Bson;
using MongoDB.Context.Bson.Differences;

namespace MongoDB.Context
{
	public class TrackedEntity<TDocument, TIdField> 
		where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		private readonly BsonDocument _OriginalState = null;

		public TDocument Entity { get; set; }
		public EntityState State { get; set; }

		public TrackedEntity(TDocument entity, EntityState state)
		{
			Entity = entity;
			State = state;
			if (state == EntityState.ReadFromSource) _OriginalState = entity.ToBsonDocument();
		}

		public BsonDifference<TDocument, TIdField>[] GetDifferences()
		{
			switch (State)
			{
				case EntityState.Added:
				case EntityState.Deleted:
				case EntityState.NoActionRequired:
					return null;
				case EntityState.ReadFromSource:
					var comparer = new BsonDocumentComparer<TDocument, TIdField>();
					return comparer.GetDifferences(_OriginalState, this.Entity.ToBsonDocument());
				default:
					throw new InvalidOperationException("Entity state invalid");
			}
		}

		//public WriteModel<T> GetChange()
		//{
		//	switch (State)
		//	{
		//		case EntityState.Added:
		//			return new InsertOneModel<T>(Entity);
		//		case EntityState.Deleted:
		//			return new DeleteOneModel<T>(Builders<T>.Filter.Eq(z => z._Id, this.Entity._Id));
		//		case EntityState.ReadFromSource:
		//			var comparer = new BsonDocumentComparer<T, TIdField>();
		//			var differences = comparer.GetDifferences(_OriginalState, this.Entity.ToBsonDocument());
		//			var updates = differences.Select(z => z.GetMongoUpdate()).Where(z => z != null).ToArray();
		//			if (!updates.Any()) return null;
		//			return new UpdateOneModel<T>(
		//				Builders<T>.Filter.Eq(z => z._Id, this.Entity._Id), 
		//				updates.Count() > 1 ? Builders<T>.Update.Combine(updates) : updates.Single()
		//			);
		//		case EntityState.NoActionRequired:
		//			return null;
		//		default:
		//			throw new InvalidOperationException("Entity state invalid");
		//	}
		//}

		//private WriteModel<T> GetUpdateChange()
		//{
		//	var documentFilter = Builders<T>.Filter.Eq(z => z._Id, this.Entity._Id);
		//	var updates = TraverseDocumentElements(new object[] { }, _OriginalState, this.Entity.ToBsonDocument()).ToArray();

		//	return updates.Any() ? new UpdateOneModel<T>(documentFilter, updates.Count() > 1 ? Builders<T>.Update.Combine(updates) : updates.Single()) : null;
		//}

		//private static List<UpdateDefinition<T>> TraverseDocumentElements(object[] elementPath, BsonValue oldValue, BsonValue newValue)
		//{
		//	var changes = new List<UpdateDefinition<T>>();

		//	if (oldValue == newValue) return changes;

		//	var oldValueIsNull = oldValue == null || oldValue.IsBsonNull;
		//	var newValueIsNull = newValue == null || newValue.IsBsonNull;

		//	var lastElementPath = elementPath.LastOrDefault();
		//	var currentArrayIdx = lastElementPath as int?;

		//	// Replacing NULL with a new value
		//	if (oldValueIsNull && !newValueIsNull)
		//	{
		//		if (currentArrayIdx.HasValue)
		//		{
		//			// If we are in an array, add this item to the set the given idx
		//			var path = string.Join(".", elementPath.TakeWhile((obj, idx) => idx != elementPath.Count() - 1));
		//			changes.Add(Builders<T>.Update.PushEach(path, new[] { newValue }, position: currentArrayIdx.Value));
		//			return changes;
		//		}

		//		// Standard field set
		//		changes.Add(Builders<T>.Update.Set(string.Join(".", elementPath), newValue));
		//		return changes;
		//	}

		//	// Replacing old value with NULL
		//	if (newValueIsNull && !oldValueIsNull)
		//	{
		//		if (currentArrayIdx.HasValue)
		//		{
		//			// If we are in an array, remove the item from the array
		//			var path = string.Join(".", elementPath.TakeWhile((obj, idx) => idx != elementPath.Count() - 1));
		//			changes.Add(Builders<T>.Update.Pull(path, oldValue));
		//			return changes;
		//		}

		//		changes.Add(newValue == null
		//			? Builders<T>.Update.Unset(string.Join(".", elementPath)) // Unset value
		//			: Builders<T>.Update.Set(string.Join(".", elementPath), newValue)); // Set as NULL

		//		return changes;
		//	}

		//	// Old Value is not null.. and is not a complex type (Document/Array)
		//	if (!oldValueIsNull && !oldValue.IsBsonDocument && !oldValue.IsBsonArray)
		//	{
		//		changes.Add(Builders<T>.Update.Set(string.Join(".", elementPath), newValue));
		//		return changes;
		//	}

		//	if (oldValue != null && oldValue.IsBsonDocument)
		//	{
		//		var oldBsonDocument = oldValue.AsBsonDocument;
		//		var newBsonDocument = newValue != null && newValue.IsBsonDocument
		//			? newValue.AsBsonDocument
		//			: null;

		//		if (newBsonDocument == null)
		//		{
		//			if (currentArrayIdx.HasValue)
		//			{
		//				var path = string.Join(".", elementPath.TakeWhile((obj, idx) => idx != elementPath.Count() - 1));
		//				changes.Add(Builders<T>.Update.Pull(path, oldBsonDocument));
		//				return changes;
		//			}

		//			return changes;
		//		}

		//		foreach (var fieldName in oldBsonDocument.Names.Concat(newBsonDocument.Names).Distinct())
		//		{
		//			var oldFieldValue = oldBsonDocument.Contains(fieldName)
		//				? oldBsonDocument[fieldName]
		//				: null;

		//			var newFieldValue = newBsonDocument.Contains(fieldName)
		//				? newBsonDocument[fieldName]
		//				: null;

		//			var subDocumentChanges = TraverseDocumentElements(elementPath.Concat(new[] { fieldName }).ToArray(), oldFieldValue, newFieldValue);
		//			if (subDocumentChanges.Any())
		//				changes.AddRange(subDocumentChanges);
		//		}

		//		return changes;
		//	}

		//	if (oldValue != null && oldValue.IsBsonArray)
		//	{
		//		var oldArray = oldValue.AsBsonArray;
		//		var newArray = newValue.AsBsonArray;

		//		var largestIndex = oldArray.Count > newArray.Count
		//			? oldArray.Count
		//			: newArray.Count;

		//		for (var idx = 0; idx < largestIndex; idx++)
		//		{
		//			var oldArrayItem = oldArray.Count > idx
		//				? oldArray[idx]
		//				: null;

		//			var newArrayItem = newArray.Count > idx
		//				? newArray[idx]
		//				: null;

		//			var arrayChanges = TraverseDocumentElements(elementPath.Concat(new object[] { idx }).ToArray(), oldArrayItem, newArrayItem);
		//			if (arrayChanges.Any())
		//				changes.AddRange(arrayChanges);
		//		}

		//		return changes;
		//	}

		//	return changes;
		//}
	}
}
