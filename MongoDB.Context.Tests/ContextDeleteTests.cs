using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MongoDB.Context.Tests
{
	[TestClass]
	public class ContextDeleteTests : ContextTestBase
	{
		[TestMethod]
		public void Should_NoDelete_WhenNoChange()
		{
			using (var ctx = GetMongoContext())
			{
				var changes = ctx.TestEntities.GetChanges();
				changes.AssertNoChange();
			}
		}

		[TestMethod]
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

		[TestMethod]
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
