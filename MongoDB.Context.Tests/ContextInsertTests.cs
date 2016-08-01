using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MongoDB.Context.Tests
{
	[TestClass]
	public class ContextInsertTests : ContextTestBase
	{
		[TestMethod]
		public void Should_NoInsert_WhenNoChange()
		{
			using (var ctx = new MockMongoContext(new TestEntity[] {}))
			{
				var changes = ctx.TestEntities.GetChanges();
				changes.AssertNoChange();
			}
		}

		[TestMethod]
		public void Should_OneInsert_WhenOneEntityIsInserted()
		{
			using (var ctx = new MockMongoContext(new TestEntity[] {}))
			{
				ctx.TestEntities.InsertOnSubmit(new TestEntity
				{
					String = "INSERTED ENTITY"
				});

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertInsertCount(1);
			}
		}

		[TestMethod]
		public void Should_TwoInserts_WhenTwoEntitiesAreInserted()
		{
			using (var ctx = new MockMongoContext(new TestEntity[] {}))
			{
				ctx.TestEntities.InsertAllOnSubmit(new []
				{
					new TestEntity
					{
						String = "INSERTED ENTITY 1"
					},
					new TestEntity
					{
						String = "INSERTED ENTITY 2"
					}	
				});

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertInsertCount(2);
			}
		}
	}
}
