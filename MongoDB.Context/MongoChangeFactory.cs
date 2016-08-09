using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Xml.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
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
						Change = new InsertOneModel<TDocument>(
							z.Entity
						),
						ExecutionOrder = 1
					}));
			}

			if (collectionChangeSet.Deletes != null && collectionChangeSet.Deletes.Any())
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

			if (collectionChangeSet.Updates != null && collectionChangeSet.Updates.Any())
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
						foreach (var change in differenceByField.Value)
						{
							var fieldChange = change as BsonFieldDifference<TDocument, TIdField>;
							if (fieldChange != null)
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
											ExecutionOrder = ++order
										});
									}

									continue;
								}

								var newValue = GetDotNetValue(fieldChange.FieldPath, fieldChange.NewValue);
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
										ExecutionOrder = ++order
									});
								}

								continue;
							}

							var arrayChange = change as BsonArrayItemDifference<TDocument, TIdField>;
							if (arrayChange != null)
							{
								switch (arrayChange.Type)
								{
									case BsonArrayItemDifferenceType.Add:
										var newValue = GetDotNetValue(arrayChange.FieldPath, arrayChange.ArrayItem);
										mongoChanges.Add(new MongoChange<TDocument, TIdField>
										{
											Change = new UpdateOneModel<TDocument>(
												Builders<TDocument>.Filter.Eq(z => z._Id, document._Id),
												Builders<TDocument>.Update.PushEach(differenceByField.Key, new[] { newValue }, position: (int)arrayChange.FieldPath.Last())
											),
											ExecutionOrder = ++order
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
											ExecutionOrder = ++order
										});
										mongoChanges.Add(new MongoChange<TDocument, TIdField>
										{
											Change = new UpdateOneModel<TDocument>(
												Builders<TDocument>.Filter.Eq(z => z._Id, document._Id),
												Builders<TDocument>.Update.Pull(differenceByField.Key, (object)null)
											),
											ExecutionOrder = ++order
										});
										break;
									default:
										throw new ArgumentOutOfRangeException();
								}

								continue;
							}

							throw new InvalidOperationException("Unable to get MongoDB change from difference type " + change.GetType().FullName);
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

			return mongoChanges.ToArray();
		}

		private static object GetDotNetValue(object[] fieldPath, BsonValue value)
		{
			// Simple type
			if (!value.IsBsonDocument && !value.IsBsonArray)
			{
				return BsonTypeMapper.MapToDotNetValue(value);
			}

			// Array (of either a simple type or document)
			if (value.IsBsonArray)
			{
				var bsonArray = value.AsBsonArray;
				return bsonArray.Select((t, idx) => GetDotNetValue(fieldPath.Concat(new object[] {idx}).ToArray(), t)).ToArray();
			}

			// Here, we are a BSON document (most likely a new .NET type)
			var type = GetTypeOfField(fieldPath);

			var requiredMembers = value.AsBsonDocument.Names.ToArray();
			var memberDict = type.GetMembers()
				.Where(z =>
					z.CustomAttributes.All(a => a.AttributeType != typeof(BsonIgnoreAttribute))
					&& (requiredMembers.Contains(z.Name) || (
						z.GetCustomAttributes(typeof(BsonElementAttribute)).Any()
						&& requiredMembers.Contains(((BsonElementAttribute)z.GetCustomAttributes(typeof(BsonElementAttribute)).Single()).ElementName))
						)
				)
				.ToDictionary(z =>
				{
					if (requiredMembers.Contains(z.Name)) return z.Name;
					var bsonElementAttribute = (BsonElementAttribute)z.GetCustomAttributes(typeof(BsonElementAttribute)).Single();
					return bsonElementAttribute.ElementName;
				});

			//Rehydrate an instance of the given type
			var instance = Activator.CreateInstance(type);
			foreach (var element in value.AsBsonDocument.Elements)
			{
				var member = memberDict[element.Name];

				var propInfo = member as PropertyInfo;
				if (propInfo != null)
				{
					propInfo.SetValue(instance, BsonTypeMapper.MapToDotNetValue(element.Value));
					continue;
				}

				var fieldInfo = member as FieldInfo;
				if (fieldInfo != null)
				{
					fieldInfo.SetValue(instance, BsonTypeMapper.MapToDotNetValue(element.Value));
					continue;
				}
			}

			return instance;
		}

		private static Type GetTypeOfField(object[] fieldPath)
		{
			var queue = new Queue<object>(fieldPath);

			Type tempType = null;
			while (queue.Any())
			{
				var field = queue.Dequeue();

				var fieldName = field as string;
				var arrayIndex = field as int?;

				if (fieldName == null && arrayIndex.HasValue)
				{
					if (tempType == null || tempType.GetInterfaces().All(z => !z.IsGenericType || z.GetGenericTypeDefinition() != typeof(IEnumerable<>)))
						throw new Exception(string.Format("Cannot find C# type for {0}", string.Join(".", fieldPath)));

					var enumerableInterface = tempType.GetInterfaces().Single(z => z.IsGenericType && z.GetGenericTypeDefinition() == typeof(IEnumerable<>));
					tempType = enumerableInterface.GetGenericArguments()[0];
					continue;
				}

				var member = (tempType ?? typeof(TDocument))
					.GetMembers()
					.SingleOrDefault(z =>
						z.CustomAttributes.All(a => a.AttributeType != typeof(BsonIgnoreAttribute))
						&& (z.Name == fieldName || (
							z.GetCustomAttributes(typeof(BsonElementAttribute)).Any()
							&& ((BsonElementAttribute)z.GetCustomAttributes(typeof(BsonElementAttribute)).Single()).ElementName == fieldName)
						)
					);

				if (member == null)
					throw new Exception(string.Format("Cannot find C# type for {0}", string.Join(".", fieldPath)));

				var propInfo  = member as PropertyInfo;
				if (propInfo != null)
				{
					tempType = propInfo.PropertyType;
					continue;
				}

				var fieldInfo = member as FieldInfo;
				if (fieldInfo != null)
				{
					tempType = fieldInfo.FieldType;
					continue;
				}

				throw new Exception(string.Format("Cannot find C# type for {0}", string.Join(".", fieldPath)));
			}

			return tempType;
		}
	}
}