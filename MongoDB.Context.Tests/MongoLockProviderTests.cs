using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Context.Locking;
using MongoDB.Driver;
using NUnit.Framework;

namespace MongoDB.Context.Tests
{
	[TestFixture]
	public class MongoLockProviderTests
	{
		private const string DatabaseKey = "integration-tests";
		private const string CollectionKey = "locks";

		[SetUp]
		public void PreTestCleanup()
		{
			var mongoClient = new MongoClient();
			var database = mongoClient.GetDatabase(DatabaseKey);
			database.DropCollection(CollectionKey);
		}

		[Test]
		public void Should_Succeed_WhenTakingOneLockUnopposedAcquireAll()
		{
			var locksRequired = new List<MongoLockRequest<ObjectId>>
			{
				new MongoLockRequest<ObjectId>
				{
					DocumentId = ObjectId.GenerateNewId(),
					Field = "TEST"
				}
			};

			var mongoClient = new MongoClient();
			var collection = mongoClient.GetDatabase(DatabaseKey).GetCollection<MongoLock<ObjectId>>(CollectionKey);

			using (var lp = new MongoLockProvider<ObjectId>(locksRequired, mongoClient, DatabaseKey, CollectionKey))
			{
				Assert.That(lp.TryAcquireAll(), Is.True);
				Assert.That(collection.Count(z => true), Is.EqualTo(1));
			}
		}

		[Test]
		public void Should_Succeed_WhenTakingTwoLocksUnopposedAcquireAll()
		{
			var docId = ObjectId.GenerateNewId();
			var locksRequired = new List<MongoLockRequest<ObjectId>>
			{
				new MongoLockRequest<ObjectId>
				{
					DocumentId = docId,
					Field = "TEST"
				},
				new MongoLockRequest<ObjectId>
				{
					DocumentId = docId,
					Field = "TEST2"
				}
			};

			var mongoClient = new MongoClient();
			var collection = mongoClient.GetDatabase(DatabaseKey).GetCollection<MongoLock<ObjectId>>(CollectionKey);
			
			using (var lp = new MongoLockProvider<ObjectId>(locksRequired, mongoClient, DatabaseKey, CollectionKey))
			{
				Assert.That(lp.TryAcquireAll(), Is.True);
				Assert.That(collection.Count(z => true), Is.EqualTo(2));
			}

			Assert.That(collection.Count(z => true), Is.Zero);
		}

		[Test]
		public void Should_SucceedFirstThenFail_WhenTakingOneLockWithContentionAcquireAll()
		{
			var docId = ObjectId.GenerateNewId();
			var locksRequired = new List<MongoLockRequest<ObjectId>>
			{
				new MongoLockRequest<ObjectId>
				{
					DocumentId = docId,
					Field = "TEST"
				}
			};

			var mongoClient = new MongoClient();
			var collection = mongoClient.GetDatabase(DatabaseKey).GetCollection<MongoLock<ObjectId>>(CollectionKey);
			
			using (var lp = new MongoLockProvider<ObjectId>(locksRequired, mongoClient, DatabaseKey, CollectionKey))
			{
				Assert.That(lp.TryAcquireAll(), Is.True);
				Assert.That(collection.Count(z => true), Is.EqualTo(1));

				using (var lp2 = new MongoLockProvider<ObjectId>(locksRequired, mongoClient, DatabaseKey, CollectionKey))
				{
					Assert.That(lp2.TryAcquireAll(), Is.False);
					Assert.That(collection.Count(z => true), Is.EqualTo(1));
				}

				Assert.That(collection.Count(z => true), Is.EqualTo(1));
			}

			Assert.That(collection.Count(z => true), Is.Zero);
		}

		[Test]
		public void Should_ReleaseLocks_WhenDisposingAcquireAll()
		{
			var docId = ObjectId.GenerateNewId();
			var locksRequired = new List<MongoLockRequest<ObjectId>>
			{
				new MongoLockRequest<ObjectId>
				{
					DocumentId = docId,
					Field = "TEST"
				}
			};

			var mongoClient = new MongoClient();
			var collection = mongoClient.GetDatabase(DatabaseKey).GetCollection<MongoLock<ObjectId>>(CollectionKey);

			var lp = new MongoLockProvider<ObjectId>(locksRequired, mongoClient, DatabaseKey, CollectionKey);
			
			Assert.That(lp.TryAcquireAll(), Is.True);
			Assert.That(collection.Count(z => true), Is.EqualTo(1));

			lp.Dispose();

			Assert.That(collection.Count(z => true), Is.Zero);
		}

		[Test]
		public void Should_Succeed_WhenTakingOneLockAcquireAny()
		{
			var docId = ObjectId.GenerateNewId();
			var locksRequired = new List<MongoLockRequest<ObjectId>>
			{
				new MongoLockRequest<ObjectId>
				{
					DocumentId = docId,
					Field = "TEST"
				}
			};

			var mongoClient = new MongoClient();
			var collection = mongoClient.GetDatabase(DatabaseKey).GetCollection<MongoLock<ObjectId>>(CollectionKey);

			using (var lp = new MongoLockProvider<ObjectId>(locksRequired, mongoClient, DatabaseKey, CollectionKey))
			{
				List<MongoLock<ObjectId>> acquiredLocks;
				Assert.That(lp.TryAcquireAny(out acquiredLocks), Is.True);

				Assert.That(acquiredLocks.Count, Is.EqualTo(1));

				Assert.That(collection.Count(z => true), Is.EqualTo(1));
			}

			Assert.That(collection.Count(z => true), Is.Zero);
		}

		[Test]
		public void Should_Succeed_WhenTakingTwoLocksAcquireAny()
		{
			var docId = ObjectId.GenerateNewId();
			var locksRequired = new List<MongoLockRequest<ObjectId>>
			{
				new MongoLockRequest<ObjectId>
				{
					DocumentId = docId,
					Field = "TEST"
				},
				new MongoLockRequest<ObjectId>
				{
					DocumentId = docId,
					Field = "TEST2"
				}
			};

			var mongoClient = new MongoClient();
			var collection = mongoClient.GetDatabase(DatabaseKey).GetCollection<MongoLock<ObjectId>>(CollectionKey);

			using (var lp = new MongoLockProvider<ObjectId>(locksRequired, mongoClient, DatabaseKey, CollectionKey))
			{
				List<MongoLock<ObjectId>> acquiredLocks;
				Assert.That(lp.TryAcquireAny(out acquiredLocks), Is.True);

				Assert.That(acquiredLocks.Count, Is.EqualTo(2));

				Assert.That(collection.Count(z => true), Is.EqualTo(2));
			}

			Assert.That(collection.Count(z => true), Is.Zero);
		}

		[Test]
		public void Should_Fail_WhenTakingTwoLocksAlreadyTakenAcquireAny()
		{
			var docId = ObjectId.GenerateNewId();
			var locksRequired = new List<MongoLockRequest<ObjectId>>
			{
				new MongoLockRequest<ObjectId>
				{
					DocumentId = docId,
					Field = "TEST"
				},
				new MongoLockRequest<ObjectId>
				{
					DocumentId = docId,
					Field = "TEST2"
				}
			};

			var mongoClient = new MongoClient();
			var collection = mongoClient.GetDatabase(DatabaseKey).GetCollection<MongoLock<ObjectId>>(CollectionKey);

			using (var lp = new MongoLockProvider<ObjectId>(locksRequired, mongoClient, DatabaseKey, CollectionKey))
			{
				Assert.That(lp.TryAcquireAll(), Is.True);

				using (var lp2 = new MongoLockProvider<ObjectId>(locksRequired, mongoClient, DatabaseKey, CollectionKey))
				{
					Assert.That(collection.Count(z => true), Is.EqualTo(2));

					List<MongoLock<ObjectId>> acquiredLocks;
					Assert.That(lp2.TryAcquireAny(out acquiredLocks), Is.False);

					Assert.That(acquiredLocks.Count, Is.EqualTo(0));
				}

				Assert.That(collection.Count(z => true), Is.EqualTo(2));
			}

			Assert.That(collection.Count(z => true), Is.Zero);
		}

		[Test]
		public void Should_SucceedPartially_WhenTakingTwoLocksOneUncontendedOneAlreadyTakenAcquireAny()
		{
			var docId = ObjectId.GenerateNewId();
			var locksRequired = new List<MongoLockRequest<ObjectId>>
			{
				new MongoLockRequest<ObjectId>
				{
					DocumentId = docId,
					Field = "TEST"
				},
				new MongoLockRequest<ObjectId>
				{
					DocumentId = docId,
					Field = "TEST2"
				}
			};

			var mongoClient = new MongoClient();
			var collection = mongoClient.GetDatabase(DatabaseKey).GetCollection<MongoLock<ObjectId>>(CollectionKey);

			var lpRequests = new [] { locksRequired.First() };
			using (var lp = new MongoLockProvider<ObjectId>(lpRequests, mongoClient, DatabaseKey, CollectionKey))
			{
				Assert.That(lp.TryAcquireAll(), Is.True);

				using (var lp2 = new MongoLockProvider<ObjectId>(locksRequired, mongoClient, DatabaseKey, CollectionKey))
				{
					Assert.That(collection.Count(z => true), Is.EqualTo(1));

					List<MongoLock<ObjectId>> acquiredLocks;
					Assert.That(lp2.TryAcquireAny(out acquiredLocks), Is.True);

					Assert.That(acquiredLocks.Count, Is.EqualTo(1));
					Assert.That(acquiredLocks[0].DocumentId, Is.EqualTo(docId));
					Assert.That(acquiredLocks[0].Field, Is.EqualTo("TEST2"));
				}

				Assert.That(collection.Count(z => true), Is.EqualTo(1));
			}

			Assert.That(collection.Count(z => true), Is.Zero);
		}
	}
}
