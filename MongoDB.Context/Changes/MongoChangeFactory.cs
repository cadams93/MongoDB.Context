using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Context.Bson.Differences;
using MongoDB.Context.Locking;
using MongoDB.Driver;

namespace MongoDB.Context.Changes
{
	public class MongoChangeFactory<TDocument, TIdField>
		where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		public MongoChange<TDocument, TIdField>[] GetMongoChangesFromChangeSet(
			MongoCollectionChangeSet<TDocument, TIdField> collectionChangeSet, 
			out List<MongoLockRequest<TIdField>> locksRequired)
		{
			locksRequired = new List<MongoLockRequest<TIdField>>();

			var mongoChanges = new List<MongoChange<TDocument, TIdField>>();

			if (collectionChangeSet.Inserts != null && collectionChangeSet.Inserts.Any())
                QueueInserts(collectionChangeSet, mongoChanges);

			if (collectionChangeSet.Deletes != null && collectionChangeSet.Deletes.Any())
                QueueDeletes(collectionChangeSet, mongoChanges);

			if (collectionChangeSet.Updates != null && collectionChangeSet.Updates.Any())
                QueueUpdates(collectionChangeSet, locksRequired, mongoChanges);

			return mongoChanges.ToArray();
		}

        private static void QueueInserts(MongoCollectionChangeSet<TDocument, TIdField> collectionChangeSet, 
            List<MongoChange<TDocument, TIdField>> mongoChanges)
        {
            mongoChanges.AddRange(collectionChangeSet.Inserts
                .Select(z => new MongoChange<TDocument, TIdField>
                {
                    Change = new InsertOneModel<TDocument>(
                        z.Entity
                    ),
                    ExecutionOrder = 1
                }));
        }

        private static void QueueUpdates(MongoCollectionChangeSet<TDocument, TIdField> collectionChangeSet, 
            List<MongoLockRequest<TIdField>> locksRequired, List<MongoChange<TDocument, TIdField>> mongoChanges)
        {
            foreach (var documentUpdate in collectionChangeSet.Updates)
            {
                var document = documentUpdate.Key;

                var differenceByFields = documentUpdate.Value
                    .GroupBy(z => string.Join(".", z.FieldPath.TakeWhile(p => !(p is int))))
                    .ToDictionary(z => z.Key, z => z.ToArray());

                UpdateDefinition<TDocument> fieldUpdates = null;
                foreach (var differenceByField in differenceByFields)
                {
                    locksRequired.Add(new MongoLockRequest<TIdField>
                    {
                        DocumentId = document._Id,
                        Field = differenceByField.Key
                    });

                    // We have to be mindful of the order of operations to ensure index references are correct
                    // Iterate over the changes in order in which they were added to the collection
                    var order = 1;
                    var nullPullRequired = false;
                    foreach (var change in differenceByField.Value)
                    {
                        var arrayChange = change as BsonArrayItemDifference<TDocument, TIdField>;

                        // If this change is NOT another array item removal, we need to pull all nulls from the array
                        if (nullPullRequired && (arrayChange == null || arrayChange.Type != BsonArrayItemDifferenceType.Remove))
                        {
                            QueueArrayNullPull(mongoChanges, document, differenceByField, ++order);
                            nullPullRequired = false;
                        }

                        if (arrayChange != null)
                        {
                            QueueArrayItemChange(mongoChanges, document, differenceByField, arrayChange, ++order, ref nullPullRequired);
                            continue;
                        }

                        var fieldChange = change as BsonFieldDifference<TDocument, TIdField>;
                        if (fieldChange != null)
                        {
                            QueueDocumentFieldChange(mongoChanges, document, fieldChange, ++order, ref fieldUpdates);
                            continue;
                        }

                        throw new InvalidOperationException("Unable to get MongoDB change from difference type " + change.GetType().FullName);
                    }

                    // Do the last NULL pull
                    if (nullPullRequired)
                    {
                        QueueArrayNullPull(mongoChanges, document, differenceByField, ++order);
                        nullPullRequired = false;
                    }
                }

                if (fieldUpdates == null) continue;

                mongoChanges.Add(new MongoChange<TDocument, TIdField>
                {
                    Change = new UpdateOneModel<TDocument>(
                        Builders<TDocument>.Filter.Eq(z => z._Id, document._Id),
                        fieldUpdates
                    ),
                    ExecutionOrder = 1
                });
            }
        }

        private static void QueueDocumentFieldChange(List<MongoChange<TDocument, TIdField>> mongoChanges, TDocument document, 
            BsonFieldDifference<TDocument, TIdField> fieldChange, int order, ref UpdateDefinition<TDocument> fieldUpdates)
        {
            var elementPath = string.Join(".", fieldChange.FieldPath);
            var isRootFieldChange = fieldChange.FieldPath.All(z => !(z is int));

            if (fieldChange.NewValue == null)
            {
                if (isRootFieldChange)
                {
                    fieldUpdates = fieldUpdates == null
                        ? Builders<TDocument>.Update.Unset(elementPath)
                        : fieldUpdates.Unset(elementPath);
                }
                else
                {
                    mongoChanges.Add(new MongoChange<TDocument, TIdField>
                    {
                        Change = new UpdateOneModel<TDocument>(
                            Builders<TDocument>.Filter.Eq(z => z._Id, document._Id),
                            Builders<TDocument>.Update.Unset(elementPath)
                        ),
                        ExecutionOrder = order
                    });
                }
            }
            else
            {
                var newValue = BsonTypeMapper<TDocument, TIdField>
                    .GetDotNetValue(fieldChange.FieldPath, fieldChange.NewValue);

                if (isRootFieldChange)
                {
                    fieldUpdates = fieldUpdates == null
                        ? Builders<TDocument>.Update.Set(elementPath, newValue)
                        : fieldUpdates.Set(elementPath, newValue);
                }
                else
                {
                    mongoChanges.Add(new MongoChange<TDocument, TIdField>
                    {
                        Change = new UpdateOneModel<TDocument>(
                            Builders<TDocument>.Filter.Eq(z => z._Id, document._Id),
                            Builders<TDocument>.Update.Set(elementPath, newValue)
                        ),
                        ExecutionOrder = order
                    });
                }
            }
        }

        private static void QueueArrayItemChange(List<MongoChange<TDocument, TIdField>> mongoChanges, TDocument document, 
            KeyValuePair<string, BsonDifference<TDocument, TIdField>[]> differenceByField, 
            BsonArrayItemDifference<TDocument, TIdField> arrayChange, int order, ref bool nullPullRequired)
        {
            switch (arrayChange.Type)
            {
                case BsonArrayItemDifferenceType.Add:
                    var newValue = BsonTypeMapper<TDocument, TIdField>.GetDotNetValue(arrayChange.FieldPath, arrayChange.ArrayItem);
                    mongoChanges.Add(new MongoChange<TDocument, TIdField>
                    {
                        Change = new UpdateOneModel<TDocument>(
                            Builders<TDocument>.Filter.Eq(z => z._Id, document._Id),
                            Builders<TDocument>.Update.PushEach(differenceByField.Key, new[] { newValue },
                                position: (int)arrayChange.FieldPath.Last())
                        ),
                        ExecutionOrder = order
                    });
                    break;
                case BsonArrayItemDifferenceType.Remove:
                    var elementPath = string.Join(".", arrayChange.FieldPath);
                    mongoChanges.Add(new MongoChange<TDocument, TIdField>
                    {
                        Change = new UpdateOneModel<TDocument>(
                            Builders<TDocument>.Filter.Eq(z => z._Id, document._Id),
                            Builders<TDocument>.Update.Unset(elementPath)
                        ),
                        ExecutionOrder = order
                    });
                    nullPullRequired = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void QueueArrayNullPull(List<MongoChange<TDocument, TIdField>> mongoChanges, TDocument document, 
            KeyValuePair<string, BsonDifference<TDocument, TIdField>[]> differenceByField, int order)
        {
            mongoChanges.Add(new MongoChange<TDocument, TIdField>
            {
                Change = new UpdateOneModel<TDocument>(
                    Builders<TDocument>.Filter.Eq(z => z._Id, document._Id),
                    Builders<TDocument>.Update.Pull(differenceByField.Key, (object)null)
                ),
                ExecutionOrder = order
            });
        }

        private static void QueueDeletes(MongoCollectionChangeSet<TDocument, TIdField> collectionChangeSet, 
            List<MongoChange<TDocument, TIdField>> mongoChanges)
        {
            mongoChanges.AddRange(collectionChangeSet.Deletes
                .Select(z => new MongoChange<TDocument, TIdField>
                {
                    Change = new DeleteOneModel<TDocument>(
                        Builders<TDocument>.Filter.Eq(doc => doc._Id, z.Entity._Id)
                    ),
                    ExecutionOrder = 1
                }));
        }
	}
}