using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Context.Bson.Differences;
using MongoDB.Context.Locking;
using NUnit.Framework;

namespace MongoDB.Context.Tests
{
	[TestFixture]
	public class ChangeFactoryTests
	{
		private MongoChangeFactory<TestEntity, ObjectId> _ChangeFactory;

		[SetUp]
		public void Setup()
		{
			_ChangeFactory = new MongoChangeFactory<TestEntity, ObjectId>();
		}

		[Test]
		public void Should_NoChange_WhenNoDifference()
		{
			var differences = new BsonDifference<TestEntity, ObjectId>[] {};

			ObjectId docId;
			List<MongoLockRequest<ObjectId>> locksRequired;
			var changes = _ChangeFactory.GetMongoChangesFromChangeSet(GetMockCollectionChangeSet(differences, out docId), out locksRequired);

			Assert.That(changes, Is.Empty);
			Assert.That(locksRequired, Is.Empty);
		}

		[Test]
		public void Should_OneChange_WhenOneFieldDifference()
		{
			var differences = new BsonDifference<TestEntity, ObjectId>[]
			{
				new BsonFieldDifference<TestEntity, ObjectId>(new object[] { "TEST" }, BsonString.Create("OLD"), BsonString.Create("NEW")) 
			};

			ObjectId docId;
			List<MongoLockRequest<ObjectId>> locksRequired;
			var changes = _ChangeFactory.GetMongoChangesFromChangeSet(GetMockCollectionChangeSet(differences, out docId), out locksRequired);

			Assert.That(changes.Length, Is.EqualTo(1));
			Assert.That(locksRequired.Count, Is.EqualTo(1));
			Assert.That(locksRequired[0].DocumentId, Is.EqualTo(docId));
			Assert.That(locksRequired[0].Field, Is.EqualTo("TEST"));
		}

		[Test]
		public void Should_TwoChanges_WhenOneArrayItemRemoved()
		{
			var differences = new BsonDifference<TestEntity, ObjectId>[]
			{
				new BsonArrayItemDifference<TestEntity, ObjectId>(BsonArrayItemDifferenceType.Remove, new object[] { "ARRAY", 0 }, BsonNull.Value)
			};

			ObjectId docId;
			List<MongoLockRequest<ObjectId>> locksRequired;
			var changes = _ChangeFactory.GetMongoChangesFromChangeSet(GetMockCollectionChangeSet(differences, out docId), out locksRequired);

			Assert.That(changes.Length, Is.EqualTo(2));
			Assert.That(locksRequired.Count, Is.EqualTo(1));

			Assert.That(locksRequired[0].DocumentId, Is.EqualTo(docId));
			Assert.That(locksRequired[0].Field, Is.EqualTo("ARRAY"));
		}

		[Test]
		public void Should_ThreeChanges_WhenTwoArrayItemsRemoved()
		{
			var differences = new BsonDifference<TestEntity, ObjectId>[]
			{
				new BsonArrayItemDifference<TestEntity, ObjectId>(BsonArrayItemDifferenceType.Remove, new object[] { "ARRAY", 0 }, BsonNull.Value), 
				new BsonArrayItemDifference<TestEntity, ObjectId>(BsonArrayItemDifferenceType.Remove, new object[] { "ARRAY", 1 }, BsonNull.Value) 
			};

			ObjectId docId;
			List<MongoLockRequest<ObjectId>> locksRequired;
			var changes = _ChangeFactory.GetMongoChangesFromChangeSet(GetMockCollectionChangeSet(differences, out docId), out locksRequired);

			Assert.That(changes.Length, Is.EqualTo(3));
			Assert.That(locksRequired.Count, Is.EqualTo(1));

			Assert.That(locksRequired[0].DocumentId, Is.EqualTo(docId));
			Assert.That(locksRequired[0].Field, Is.EqualTo("ARRAY"));
		}

		[Test]
		public void Should_FiveChanges_WhenTwoArrayItemsRemovedAndOneInsertedInBetween()
		{
			var differences = new BsonDifference<TestEntity, ObjectId>[]
			{
				new BsonArrayItemDifference<TestEntity, ObjectId>(BsonArrayItemDifferenceType.Remove, new object[] { "ARRAY", 0 }, BsonNull.Value), 
				new BsonArrayItemDifference<TestEntity, ObjectId>(BsonArrayItemDifferenceType.Add, new object[] { "ARRAY", 1 }, BsonNull.Value),
				new BsonArrayItemDifference<TestEntity, ObjectId>(BsonArrayItemDifferenceType.Remove, new object[] { "ARRAY", 2 }, BsonNull.Value) 
			};

			ObjectId docId;
			List<MongoLockRequest<ObjectId>> locksRequired;
			var changes = _ChangeFactory.GetMongoChangesFromChangeSet(GetMockCollectionChangeSet(differences, out docId), out locksRequired);

			Assert.That(changes.Length, Is.EqualTo(5));
			Assert.That(locksRequired.Count, Is.EqualTo(1));

			Assert.That(locksRequired[0].DocumentId, Is.EqualTo(docId));
			Assert.That(locksRequired[0].Field, Is.EqualTo("ARRAY"));
		}

		[Test]
		public void Should_FourChanges_WhenOneArrayItemRemovedAndTwoInsertedAfter()
		{
			var differences = new BsonDifference<TestEntity, ObjectId>[]
			{
				new BsonArrayItemDifference<TestEntity, ObjectId>(BsonArrayItemDifferenceType.Remove, new object[] { "ARRAY", 0 }, BsonNull.Value), 
				new BsonArrayItemDifference<TestEntity, ObjectId>(BsonArrayItemDifferenceType.Add, new object[] { "ARRAY", 1 }, BsonNull.Value),
				new BsonArrayItemDifference<TestEntity, ObjectId>(BsonArrayItemDifferenceType.Add, new object[] { "ARRAY", 2 }, BsonNull.Value) 
			};

			ObjectId docId;
			List<MongoLockRequest<ObjectId>> locksRequired;
			var changes = _ChangeFactory.GetMongoChangesFromChangeSet(GetMockCollectionChangeSet(differences, out docId), out locksRequired);

			Assert.That(changes.Length, Is.EqualTo(4));
			Assert.That(locksRequired.Count, Is.EqualTo(1));

			Assert.That(locksRequired[0].DocumentId, Is.EqualTo(docId));
			Assert.That(locksRequired[0].Field, Is.EqualTo("ARRAY"));
		}

		[Test]
		public void Should_FourChanges_WhenThreeArrayItemsRemoved()
		{
			var differences = new BsonDifference<TestEntity, ObjectId>[]
			{
				new BsonArrayItemDifference<TestEntity, ObjectId>(BsonArrayItemDifferenceType.Remove, new object[] { "ARRAY", 0 }, BsonNull.Value), 
				new BsonArrayItemDifference<TestEntity, ObjectId>(BsonArrayItemDifferenceType.Remove, new object[] { "ARRAY", 1 }, BsonNull.Value),
				new BsonArrayItemDifference<TestEntity, ObjectId>(BsonArrayItemDifferenceType.Remove, new object[] { "ARRAY", 2 }, BsonNull.Value) 
			};

			ObjectId docId;
			List<MongoLockRequest<ObjectId>> locksRequired;
			var changes = _ChangeFactory.GetMongoChangesFromChangeSet(GetMockCollectionChangeSet(differences, out docId), out locksRequired);

			Assert.That(changes.Length, Is.EqualTo(4));
			Assert.That(locksRequired.Count, Is.EqualTo(1));

			Assert.That(locksRequired[0].DocumentId, Is.EqualTo(docId));
			Assert.That(locksRequired[0].Field, Is.EqualTo("ARRAY"));
		}

		[Test]
		public void Should_TwoChanges_WhenTwoArrayItemsAdded()
		{
			var differences = new BsonDifference<TestEntity, ObjectId>[]
			{
				new BsonArrayItemDifference<TestEntity, ObjectId>(BsonArrayItemDifferenceType.Add, new object[] { "ARRAY", 0 }, BsonNull.Value), 
				new BsonArrayItemDifference<TestEntity, ObjectId>(BsonArrayItemDifferenceType.Add, new object[] { "ARRAY", 1 }, BsonNull.Value)
			};

			ObjectId docId;
			List<MongoLockRequest<ObjectId>> locksRequired;
			var changes = _ChangeFactory.GetMongoChangesFromChangeSet(GetMockCollectionChangeSet(differences, out docId), out locksRequired);

			Assert.That(changes.Length, Is.EqualTo(2));
			Assert.That(locksRequired.Count, Is.EqualTo(1));

			Assert.That(locksRequired[0].DocumentId, Is.EqualTo(docId));
			Assert.That(locksRequired[0].Field, Is.EqualTo("ARRAY"));
		}

		[Test]
		public void Should_ThreeChanges_WhenThreeArrayItemsAdded()
		{
			var differences = new BsonDifference<TestEntity, ObjectId>[]
			{
				new BsonArrayItemDifference<TestEntity, ObjectId>(BsonArrayItemDifferenceType.Add, new object[] { "ARRAY", 0 }, BsonNull.Value), 
				new BsonArrayItemDifference<TestEntity, ObjectId>(BsonArrayItemDifferenceType.Add, new object[] { "ARRAY", 1 }, BsonNull.Value),
				new BsonArrayItemDifference<TestEntity, ObjectId>(BsonArrayItemDifferenceType.Add, new object[] { "ARRAY", 2 }, BsonNull.Value) 
			};

			ObjectId docId;
			List<MongoLockRequest<ObjectId>> locksRequired;
			var changes = _ChangeFactory.GetMongoChangesFromChangeSet(GetMockCollectionChangeSet(differences, out docId), out locksRequired);

			Assert.That(changes.Length, Is.EqualTo(3));
			Assert.That(locksRequired.Count, Is.EqualTo(1));

			Assert.That(locksRequired[0].DocumentId, Is.EqualTo(docId));
			Assert.That(locksRequired[0].Field, Is.EqualTo("ARRAY"));
		}

		private static MongoCollectionChangeSet<TestEntity, ObjectId> GetMockCollectionChangeSet(
			IEnumerable<BsonDifference<TestEntity, ObjectId>> differences, out ObjectId docId)
		{
			docId = ObjectId.GenerateNewId();
			return new MongoCollectionChangeSet<TestEntity, ObjectId>(
				new List<TrackedEntity<TestEntity, ObjectId>>(),
				new Dictionary<TestEntity, IEnumerable<BsonDifference<TestEntity, ObjectId>>>
				{
					{ new TestEntity { _Id = docId }, differences }
				}, 
				new List<TrackedEntity<TestEntity, ObjectId>>()
			);
		}
	}
}
