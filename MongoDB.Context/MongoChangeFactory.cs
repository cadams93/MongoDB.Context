using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Context.Bson.Differences;
using MongoDB.Context.Locking;
using MongoDB.Driver;

namespace MongoDB.Context
{
	public class MongoChangeFactory<TDocument, TIdField>
		where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		public MongoChange<TDocument, TIdField>[] GetMongoChangesFromChangeSet(MongoCollectionChangeSet<TDocument, TIdField> collectionChangeSet, out List<MongoLockRequest<TIdField>> locksRequired)
		{
			locksRequired = new List<MongoLockRequest<TIdField>>();

			var mongoChanges = new List<MongoChange<TDocument, TIdField>>();

			if (collectionChangeSet.Inserts != null && collectionChangeSet.Inserts.Any())
			{
				mongoChanges.AddRange(collectionChangeSet.Inserts
					.Select(z => new MongoChange<TDocument, TIdField>
					{
						Change = new InsertOneModel<TDocument>(z.Entity),
						ExecutionOrder = 1
					}));
			}

			if (collectionChangeSet.Deletes != null && collectionChangeSet.Deletes.Any())
			{
				mongoChanges.AddRange(collectionChangeSet.Deletes
					.Select(z => new MongoChange<TDocument, TIdField>
					{
						Change = new DeleteOneModel<TDocument>(Builders<TDocument>.Filter.Eq(doc => doc._Id, z.Entity._Id)),
						ExecutionOrder = 1
					}));
			}

			if (collectionChangeSet.Updates != null && collectionChangeSet.Updates.Any())
			{
				foreach (var documentUpdate in collectionChangeSet.Updates)
				{
					//var nullPullArrayFields = new HashSet<string>();
					//var pushToArrayFields = new Dictionary<string, List<KeyValuePair<int, object>>>();

					var setDocument = new BsonDocument();
					var unsetDocument = new BsonDocument();
					var pushDocument = new BsonDocument();
					var pullDocument = new BsonDocument();

					//UpdateDefinition<TDocument> update = null;
					var document = documentUpdate.Key;

					foreach (var difference in documentUpdate.Value)
					{
						var elementPath = string.Join(".", difference.FieldPath);
						locksRequired.Add(new MongoLockRequest<TIdField>
						{
							DocumentId = document._Id,
							// Allow locking up to the first Array Index specification
							Field = string.Join(".", difference.FieldPath.TakeWhile(z => !(z is int)))
						});

						var fieldDifference = difference as BsonFieldDifference<TDocument, TIdField>;
						if (fieldDifference != null)
						{
							if (fieldDifference.NewValue == null)
							{
								//update = update == null
								//	? Builders<TDocument>.Update.Unset(elementPath)
								//	: update.Unset(elementPath);

								unsetDocument.Add(new BsonElement(elementPath, string.Empty));

								continue;
							}

							//var newValue = BsonTypeMapper.MapToDotNetValue(fieldDifference.NewValue);
							//update = update == null
							//	? Builders<TDocument>.Update.Set(elementPath, newValue)
							//	: update.Set(elementPath, newValue);

							setDocument.Add(new BsonElement(elementPath, fieldDifference.NewValue));

							continue;
						}

						//var arrayDifference = difference as BsonArrayItemDifference<TDocument, TIdField>;
						//if (arrayDifference != null)
						//{
						//	var diff = difference;
						//	// Get the array element path by taking up to the first integer (array index)
						//	var arrayElementPath = string.Join(".", difference.FieldPath.TakeWhile((z, idx) => idx < diff.FieldPath.Length - 1));
						//	var arrayIdx = (int) difference.FieldPath.SkipWhile(z => !(z is int)).Take(1).Single();

						//	switch (arrayDifference.Type)
						//	{
						//		case BsonArrayItemDifferenceType.Add:

						//			pushDocument.Add(new BsonElement(arrayElementPath,
						//				new BsonDocument(new Dictionary<string, object>
						//				{
						//					{"$each", new[] {arrayDifference.ArrayItem}},
						//					{"$position", arrayIdx}
						//				})));

						//			//var newValue = BsonTypeMapper.MapToDotNetValue(arrayDifference.ArrayItem);

						//			//if (!pushToArrayFields.ContainsKey(arrayElementPath))
						//			//	pushToArrayFields.Add(arrayElementPath, new List<KeyValuePair<int, object>>());

						//			//pushToArrayFields[arrayElementPath].Add(new KeyValuePair<int, object>(arrayIdx, newValue));

						//			break;
						//		case BsonArrayItemDifferenceType.Remove:

						//			//nullPullArrayFields.Add(arrayElementPath);
						//			unsetDocument.Add(new BsonElement(elementPath, string.Empty));
						//			pullDocument.Add(new BsonElement(arrayElementPath, string.Empty));

						//			break;
						//		default:
						//			throw new ArgumentOutOfRangeException();
						//	}
						//}
					}

					if (setDocument.Any())
					{
						mongoChanges.Add(new MongoChange<TDocument, TIdField>
						{
							Change = new UpdateOneModel<TDocument>(
								Builders<TDocument>.Filter.Eq(z => z._Id, document._Id),
								new BsonDocumentUpdateDefinition<TDocument>(new BsonDocument(new BsonElement("$set", setDocument)))
							)
						});
					}

					if (unsetDocument.Any())
					{
						mongoChanges.Add(new MongoChange<TDocument, TIdField>
						{
							Change = new UpdateOneModel<TDocument>(
								Builders<TDocument>.Filter.Eq(z => z._Id, document._Id),
								new BsonDocumentUpdateDefinition<TDocument>(new BsonDocument(new BsonElement("$unset", unsetDocument)))
							)
						});
					}
					//if (update != null)
					//{
					//	mongoChanges.Add(new MongoChange<TDocument, TIdField>
					//	{
					//		Change = new UpdateOneModel<TDocument>(Builders<TDocument>.Filter.Eq(doc => doc._Id, document._Id), update),
					//		ExecutionOrder = 1
					//	});	
					//}

					//foreach (var nullPullArrayField in nullPullArrayFields)
					//{
					//	var bsonNullValue = BsonTypeMapper.MapToDotNetValue(BsonNull.Value);

					//	mongoChanges.Add(new MongoChange<TDocument, TIdField>
					//	{
					//		Change = new UpdateOneModel<TDocument>(
					//			Builders<TDocument>.Filter.Eq(doc => doc._Id, document._Id),
					//			Builders<TDocument>.Update.Pull(nullPullArrayField, bsonNullValue)
					//		),
					//		ExecutionOrder = 2
					//	});
					//}

					//foreach (var pushToArrayRequest in pushToArrayFields)
					//{
					//	mongoChanges.AddRange(pushToArrayRequest.Value
					//		.Select(pushToArrayConfig => new MongoChange<TDocument, TIdField>
					//		{
					//			Change = new UpdateOneModel<TDocument>(
					//				Builders<TDocument>.Filter.Eq(doc => doc._Id, document._Id),
					//				Builders<TDocument>.Update.PushEach(pushToArrayRequest.Key, new[] {pushToArrayConfig.Value}, position: pushToArrayConfig.Key)
					//			), 
					//			ExecutionOrder = 3
					//		}));
					//}
				}
			}

			return mongoChanges.ToArray();
		}
	}
}