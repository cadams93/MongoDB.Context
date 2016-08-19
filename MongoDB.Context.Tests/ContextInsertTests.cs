using MongoDB.Context.Client;
using NUnit.Framework;

namespace MongoDB.Context.Tests
{
	[TestFixture]
	public class ContextInsertTests : ContextTestBase
	{
		[Test]
		public void Should_NoInsert_WhenNoChange()
		{
			using (var ctx = new MockMongoContext(new TestEntity[] {}))
			{
				var changes = ctx.TestEntities.GetChanges();
				changes.AssertNoChange();
			}
		}

		[Test]
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

		[Test]
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
