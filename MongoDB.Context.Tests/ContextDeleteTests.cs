using System.Linq;
using NUnit.Framework;

namespace MongoDB.Context.Tests
{
	[TestFixture]
	public class ContextDeleteTests : ContextTestBase
	{
		[Test]
		public void Should_NoDelete_WhenNoChange()
		{
			using (var ctx = GetMongoContext())
			{
				var changes = ctx.TestEntities.GetChanges();
				changes.AssertNoChange();
			}
		}

		[Test]
		public void Should_OneDelete_WhenOneEntityIsDeleted()
		{
			using (var ctx = GetMongoContext())
			{
				var entities = ctx.TestEntities.Find();
				ctx.TestEntities.DeleteOnSubmit(entities.Take(1).Single());

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertDeleteCount(1);
			}
		}

		[Test]
		public void Should_TwoDeletes_WhenTwoEntitiesAreDeleted()
		{
			using (var ctx = GetMongoContext())
			{
				var entities = ctx.TestEntities.Find();
				ctx.TestEntities.DeleteAllOnSubmit(entities.Take(2).ToArray());

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertDeleteCount(2);
			}
		}
	}
}
