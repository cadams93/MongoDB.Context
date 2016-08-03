using System;
using System.Linq;
using NUnit.Framework;

namespace MongoDB.Context.Tests
{
	[TestFixture]
	public class ContextMultipleOperationTests : ContextTestBase
	{
		private TestEntity[] _TestEntities;

		[SetUp]
		public void Setup()
		{
			_TestEntities = GetTestEntities();
		}

		[Test]
		public void Should_OneDelete_WhenEntityDeletedAndThenModified()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var entity = ctx.TestEntities.Find().First();
				ctx.TestEntities.DeleteOnSubmit(entity);

				entity.String = "NEW VALUE";

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertDeleteCount(1);
			}
		}

		[Test]
		public void Should_OneDelete_WhenEntityModifiedAndThenDeleted()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var entity = ctx.TestEntities.Find().First();
				entity.String = "NEW VALUE";

				ctx.TestEntities.DeleteOnSubmit(entity);

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertDeleteCount(1);
			}
		}

		[Test]
		public void Should_OneInsert_WhenEntityInsertedAndThenModified()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var entity = new TestEntity
				{
					String = "INSERTED ENTITY"
				};

				ctx.TestEntities.InsertOnSubmit(entity);

				entity.String = "NEW VALUE";

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertInsertCount(1);
			}
		}

		[Test]
		public void Should_OneInsert_WhenEntityModifiedAndThenInserted()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var entity = new TestEntity
				{
					String = "INSERTED ENTITY"
				};

				entity.String = "NEW VALUE";

				ctx.TestEntities.InsertOnSubmit(entity);

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertInsertCount(1);
			}
		}

		[Test]
		public void Should_NoChange_WhenEntityInsertedAndThenDeleted()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var entity = new TestEntity
				{
					String = "INSERTED ENTITY"
				};

				ctx.TestEntities.InsertOnSubmit(entity);

				ctx.TestEntities.DeleteOnSubmit(entity);

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertNoChange();
			}
		}

		[Test]
		public void Should_NoChange_WhenEntityDeletedAndThenInserted()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var entity = ctx.TestEntities.Find().First();
				ctx.TestEntities.DeleteOnSubmit(entity);

				ctx.TestEntities.InsertOnSubmit(entity);

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertNoChange();
			}
		}

		[Test]
		public void Should_OneChange_WhenEntityDeletedAndThenInsertedAndThenModified()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var entity = ctx.TestEntities.Find().First();
				ctx.TestEntities.DeleteOnSubmit(entity);

				ctx.TestEntities.InsertOnSubmit(entity);

				entity.String = "NEW VALUE";

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertUpdateCount(1);
			}
		}

		[Test]
		public void Should_OneChange_WhenEntityDeletedAndThenModifiedAndThenInserted()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var entity = ctx.TestEntities.Find().First();
				ctx.TestEntities.DeleteOnSubmit(entity);
				
				entity.String = "NEW VALUE";

				ctx.TestEntities.InsertOnSubmit(entity);

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertUpdateCount(1);
			}
		}

		[Test]
		public void Should_NoChange_WhenEntityDeletedAndThenModifiedAndThenInsertedAndThenModifiedBackToOriginal()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var entity = ctx.TestEntities.Find().First();
				ctx.TestEntities.DeleteOnSubmit(entity);
				
				entity.String = "NEW VALUE";

				ctx.TestEntities.InsertOnSubmit(entity);

				entity.String = "OLD VALUE A";

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertNoChange();
			}
		}

		[Test]
		public void Should_NoChange_WhenEntityDeletedAndThenInsertedTwice()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var entity = ctx.TestEntities.Find().First();
				ctx.TestEntities.DeleteOnSubmit(entity);
				ctx.TestEntities.InsertOnSubmit(entity);
				ctx.TestEntities.DeleteOnSubmit(entity);
				ctx.TestEntities.InsertOnSubmit(entity);

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertNoChange();
			}
		}

		[Test]
		public void Should_OneUpdate_WhenEntityDeletedAndThenInsertedAndThenModifiedAndThenDeletedAndThenInserted()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var entity = ctx.TestEntities.Find().First();
				ctx.TestEntities.DeleteOnSubmit(entity);
				ctx.TestEntities.InsertOnSubmit(entity);

				entity.String = "NEW VALUE";

				ctx.TestEntities.DeleteOnSubmit(entity);
				ctx.TestEntities.InsertOnSubmit(entity);

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertUpdateCount(1);
			}
		}

		[Test]
		public void Should_Exception_WhenEntityInsertedWhenAlreadyExists()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var entity = ctx.TestEntities.Find().First();

				var ex = Assert.Throws<Exception>(delegate { ctx.TestEntities.InsertOnSubmit(entity); });
				Assert.That(ex.Message, Is.EqualTo("Attempting to insert an entity which already exists"));
			}
		}

		[Test]
		public void Should_Exception_WhenEntityDeletedTwice()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var entity = ctx.TestEntities.Find().First();
				ctx.TestEntities.DeleteOnSubmit(entity);

				var ex = Assert.Throws<Exception>(delegate { ctx.TestEntities.DeleteOnSubmit(entity); });
				Assert.That(ex.Message, Is.EqualTo("Attempting to delete an entity which has already been queued for deletion"));
			}
		}
	}
}
