using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Context.Bson.Differences;
using MongoDB.Context.Locking;
using MongoDB.Driver;
using NUnit.Framework;
using MongoDB.Context.Client;
using MongoDB.Context.Changes;
using MongoDB.Context.Enums;
using MongoDB.Context.Tracking;

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
			var changes = _ChangeFactory.GetMongoChangesFromChangeSet(GetMockCollectionUpdateSet(differences, out docId), out locksRequired);

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
			var changes = _ChangeFactory.GetMongoChangesFromChangeSet(GetMockCollectionUpdateSet(differences, out docId), out locksRequired);

			Assert.That(changes.Length, Is.EqualTo(1));
			Assert.That(locksRequired.Count, Is.EqualTo(1));

			Assert.That(locksRequired[0].DocumentId, Is.EqualTo(docId));
			Assert.That(locksRequired[0].Field, Is.EqualTo("TEST"));
		}

		[Test]
		public void Should_OneChange_WhenTwoFieldDifferences()
		{
			var differences = new BsonDifference<TestEntity, ObjectId>[]
			{
				new BsonFieldDifference<TestEntity, ObjectId>(new object[] { "TEST" }, BsonString.Create("OLD"), BsonString.Create("NEW")),
				new BsonFieldDifference<TestEntity, ObjectId>(new object[] { "TEST2" }, BsonString.Create("OLD"), BsonString.Create("NEW"))
			};

			ObjectId docId;
			List<MongoLockRequest<ObjectId>> locksRequired;
			var changes = _ChangeFactory.GetMongoChangesFromChangeSet(GetMockCollectionUpdateSet(differences, out docId), out locksRequired);

			Assert.That(changes.Length, Is.EqualTo(1));
			Assert.That(locksRequired.Count, Is.EqualTo(2));

			Assert.That(locksRequired[0].DocumentId, Is.EqualTo(docId));
			Assert.That(locksRequired[0].Field, Is.EqualTo("TEST"));

			Assert.That(locksRequired[1].DocumentId, Is.EqualTo(docId));
			Assert.That(locksRequired[1].Field, Is.EqualTo("TEST2"));
		}

		[Test]
		public void Should_TwoChanges_WhenOneFieldDifferenceAndOneArrayItemAdded()
		{
			var differences = new BsonDifference<TestEntity, ObjectId>[]
			{
				new BsonFieldDifference<TestEntity, ObjectId>(new object[] { "TEST" }, BsonString.Create("OLD"), BsonString.Create("NEW")),
				new BsonArrayItemDifference<TestEntity, ObjectId>(BsonArrayItemDifferenceType.Add, new object[] { "TEST2", 0 }, BsonNull.Value)
			};

			ObjectId docId;
			List<MongoLockRequest<ObjectId>> locksRequired;
			var changes = _ChangeFactory.GetMongoChangesFromChangeSet(GetMockCollectionUpdateSet(differences, out docId), out locksRequired);

			Assert.That(changes.Length, Is.EqualTo(2));
			Assert.That(locksRequired.Count, Is.EqualTo(2));

			Assert.That(locksRequired[0].DocumentId, Is.EqualTo(docId));
			Assert.That(locksRequired[0].Field, Is.EqualTo("TEST"));

			Assert.That(locksRequired[1].DocumentId, Is.EqualTo(docId));
			Assert.That(locksRequired[1].Field, Is.EqualTo("TEST2"));
		}

		[Test]
		public void Should_ThreeChanges_WhenOneFieldDifferenceAndOneArrayItemRemoved()
		{
			var differences = new BsonDifference<TestEntity, ObjectId>[]
			{
				new BsonFieldDifference<TestEntity, ObjectId>(new object[] { "TEST" }, BsonString.Create("OLD"), BsonString.Create("NEW")),
				new BsonArrayItemDifference<TestEntity, ObjectId>(BsonArrayItemDifferenceType.Remove, new object[] { "TEST2", 1 }, BsonNull.Value)
			};

			ObjectId docId;
			List<MongoLockRequest<ObjectId>> locksRequired;
			var changes = _ChangeFactory.GetMongoChangesFromChangeSet(GetMockCollectionUpdateSet(differences, out docId), out locksRequired);

			Assert.That(changes.Length, Is.EqualTo(3));
			Assert.That(locksRequired.Count, Is.EqualTo(2));

			Assert.That(locksRequired[0].DocumentId, Is.EqualTo(docId));
			Assert.That(locksRequired[0].Field, Is.EqualTo("TEST"));

			Assert.That(locksRequired[1].DocumentId, Is.EqualTo(docId));
			Assert.That(locksRequired[1].Field, Is.EqualTo("TEST2"));
		}

		[Test]
		public void Should_FourChanges_WhenOneFieldDifferenceAndOneArrayItemAddedAndOneArrayItemRemoved()
		{
			var differences = new BsonDifference<TestEntity, ObjectId>[]
			{
				new BsonFieldDifference<TestEntity, ObjectId>(new object[] { "TEST" }, BsonString.Create("OLD"), BsonString.Create("NEW")),
				new BsonArrayItemDifference<TestEntity, ObjectId>(BsonArrayItemDifferenceType.Add, new object[] { "TEST2", 0 }, BsonNull.Value),
				new BsonArrayItemDifference<TestEntity, ObjectId>(BsonArrayItemDifferenceType.Remove, new object[] { "TEST2", 1 }, BsonNull.Value)
			};

			ObjectId docId;
			List<MongoLockRequest<ObjectId>> locksRequired;
			var changes = _ChangeFactory.GetMongoChangesFromChangeSet(GetMockCollectionUpdateSet(differences, out docId), out locksRequired);

			Assert.That(changes.Length, Is.EqualTo(4));
			Assert.That(locksRequired.Count, Is.EqualTo(2));

			Assert.That(locksRequired[0].DocumentId, Is.EqualTo(docId));
			Assert.That(locksRequired[0].Field, Is.EqualTo("TEST"));

			Assert.That(locksRequired[1].DocumentId, Is.EqualTo(docId));
			Assert.That(locksRequired[1].Field, Is.EqualTo("TEST2"));
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
			var changes = _ChangeFactory.GetMongoChangesFromChangeSet(GetMockCollectionUpdateSet(differences, out docId), out locksRequired);

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
			var changes = _ChangeFactory.GetMongoChangesFromChangeSet(GetMockCollectionUpdateSet(differences, out docId), out locksRequired);

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
			var changes = _ChangeFactory.GetMongoChangesFromChangeSet(GetMockCollectionUpdateSet(differences, out docId), out locksRequired);

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
			var changes = _ChangeFactory.GetMongoChangesFromChangeSet(GetMockCollectionUpdateSet(differences, out docId), out locksRequired);

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
			var changes = _ChangeFactory.GetMongoChangesFromChangeSet(GetMockCollectionUpdateSet(differences, out docId), out locksRequired);

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
			var changes = _ChangeFactory.GetMongoChangesFromChangeSet(GetMockCollectionUpdateSet(differences, out docId), out locksRequired);

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
			var changes = _ChangeFactory.GetMongoChangesFromChangeSet(GetMockCollectionUpdateSet(differences, out docId), out locksRequired);

			Assert.That(changes.Length, Is.EqualTo(3));
			Assert.That(locksRequired.Count, Is.EqualTo(1));

			Assert.That(locksRequired[0].DocumentId, Is.EqualTo(docId));
			Assert.That(locksRequired[0].Field, Is.EqualTo("ARRAY"));
		}

		[Test]
		public void Should_OneInsert_WhenOneEntityAdded()
		{
			List<MongoLockRequest<ObjectId>> locksRequired;
			var changes = _ChangeFactory.GetMongoChangesFromChangeSet(GetMockCollectionInsertDeleteSet(1, 0), out locksRequired);

			Assert.That(changes.Length, Is.EqualTo(1));
			Assert.That(locksRequired, Is.Empty);

			Assert.That(changes[0].Change, Is.TypeOf<InsertOneModel<TestEntity>>());
		}

		[Test]
		public void Should_TwoInserts_WhenTwoEntitiesAdded()
		{
			List<MongoLockRequest<ObjectId>> locksRequired;
			var changes = _ChangeFactory.GetMongoChangesFromChangeSet(GetMockCollectionInsertDeleteSet(2, 0), out locksRequired);

			Assert.That(changes.Length, Is.EqualTo(2));
			Assert.That(locksRequired, Is.Empty);

			Assert.That(changes[0].Change, Is.TypeOf<InsertOneModel<TestEntity>>());
			Assert.That(changes[1].Change, Is.TypeOf<InsertOneModel<TestEntity>>());
		}

		[Test]
		public void Should_OneDelete_WhenOneEntityRemoved()
		{
			List<MongoLockRequest<ObjectId>> locksRequired;
			var changes = _ChangeFactory.GetMongoChangesFromChangeSet(GetMockCollectionInsertDeleteSet(0, 1), out locksRequired);

			Assert.That(changes.Length, Is.EqualTo(1));
			Assert.That(locksRequired, Is.Empty);

			Assert.That(changes[0].Change, Is.TypeOf<DeleteOneModel<TestEntity>>());
		}

		[Test]
		public void Should_TwoDeletes_WhenTwoEntitiesRemoved()
		{
			List<MongoLockRequest<ObjectId>> locksRequired;
			var changes = _ChangeFactory.GetMongoChangesFromChangeSet(GetMockCollectionInsertDeleteSet(0, 2), out locksRequired);

			Assert.That(changes.Length, Is.EqualTo(2));
			Assert.That(locksRequired, Is.Empty);

			Assert.That(changes[0].Change, Is.TypeOf<DeleteOneModel<TestEntity>>());
			Assert.That(changes[1].Change, Is.TypeOf<DeleteOneModel<TestEntity>>());
		}

		private static MongoCollectionChangeSet<TestEntity, ObjectId> GetMockCollectionUpdateSet(
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

		private static MongoCollectionChangeSet<TestEntity, ObjectId> GetMockCollectionInsertDeleteSet(int inserts, int deletes)
		{
			return new MongoCollectionChangeSet<TestEntity, ObjectId>(
				new List<TrackedEntity<TestEntity, ObjectId>>(
					Enumerable.Range(0, inserts)
					.Select(z => new TrackedEntity<TestEntity, ObjectId>(
						new TestEntity { _Id = ObjectId.GenerateNewId() }, 
						EntityState.Added
					))),
				new Dictionary<TestEntity, IEnumerable<BsonDifference<TestEntity, ObjectId>>>(),
				new List<TrackedEntity<TestEntity, ObjectId>>(
					Enumerable.Range(0, deletes)
					.Select(z => new TrackedEntity<TestEntity, ObjectId>(
						new TestEntity { _Id = ObjectId.GenerateNewId() },
						EntityState.Deleted
					)))
			);
		}
	}
}
