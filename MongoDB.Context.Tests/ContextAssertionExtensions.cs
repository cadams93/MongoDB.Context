using System.Linq;
using NUnit.Framework;
using MongoDB.Context.Changes;

namespace MongoDB.Context.Tests
{
	public static class ContextAssertionExtensions
	{
		public static void AssertNoChange<TDocument, TIdField>(this MongoCollectionChangeSet<TDocument, TIdField> changes) 
			where TDocument : AbstractMongoEntityWithId<TIdField>
		{
			Assert.That(changes.Inserts.Count(), Is.EqualTo(0));
			Assert.That(changes.Updates.Count(), Is.EqualTo(0));
			Assert.That(changes.Deletes.Count(), Is.EqualTo(0));
		}

		public static void AssertInsertCount<TDocument, TIdField>(this MongoCollectionChangeSet<TDocument, TIdField> changes, int expectedCount) 
			where TDocument : AbstractMongoEntityWithId<TIdField>
		{
			Assert.That(changes.Inserts.Count(), Is.EqualTo(expectedCount));
		}

		public static void AssertUpdateCount<TDocument, TIdField>(this MongoCollectionChangeSet<TDocument, TIdField> changes, int expectedCount) 
			where TDocument : AbstractMongoEntityWithId<TIdField>
		{
			Assert.That(changes.Updates.Count(), Is.EqualTo(expectedCount));
		}

		public static void AssertDeleteCount<TDocument, TIdField>(this MongoCollectionChangeSet<TDocument, TIdField> changes, int expectedCount) 
			where TDocument : AbstractMongoEntityWithId<TIdField>
		{
			Assert.That(changes.Deletes.Count(), Is.EqualTo(expectedCount));
		}

		//public static void AssertIsWriteType<T>(this WriteModel<T> change, WriteModelType expectedWriteType)
		//{
		//	WriteModelType type;
		//	if (change is InsertOneModel<T>)
		//		type = WriteModelType.InsertOne;
		//	else if (change is UpdateOneModel<T>)
		//		type = WriteModelType.UpdateOne;
		//	else if (change is UpdateManyModel<T>)
		//		type = WriteModelType.UpdateMany;
		//	else if (change is DeleteOneModel<T>)
		//		type = WriteModelType.DeleteOne;
		//	else if (change is DeleteManyModel<T>)
		//		type = WriteModelType.DeleteMany;
		//	else
		//		throw new ArgumentOutOfRangeException("expectedWriteType", expectedWriteType, null);
			
		//	Assert.AreEqual(expectedWriteType, type);
		//}

		//public static void AssertIsOperator<T>(this UpdateOneModel<T> update, OperatorType expectedOperatorType)
		//{
		//	string operatorName;
		//	switch (expectedOperatorType)
		//	{
		//		case OperatorType.Set:
		//			operatorName = "$set";
		//			break;
		//		case OperatorType.Unset:
		//			operatorName = "$unset";
		//			break;
		//		case OperatorType.Push:
		//			operatorName = "$push";
		//			break;
		//		case OperatorType.Pull:
		//			operatorName = "$pull";
		//			break;
		//		default:
		//			throw new ArgumentOutOfRangeException("expectedOperatorType", expectedOperatorType, null);
		//	}
			
		//	var updateDoc = update.Update.Render(BsonSerializer.LookupSerializer<T>(), BsonSerializer.SerializerRegistry);
		//	Assert.AreEqual(operatorName, updateDoc.Elements.Single().Name);
		//}

		//public static void AssertUpdateDocumentDefinition<T>(this UpdateOneModel<T> update, BsonDocument updateDocument)
		//{
		//	var updateDoc = update.Update.Render(BsonSerializer.LookupSerializer<T>(), BsonSerializer.SerializerRegistry);
		//	Assert.IsTrue(updateDoc.Equals(updateDocument));
		//}
	}
}
