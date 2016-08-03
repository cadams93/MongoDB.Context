using System.Linq;
using NUnit.Framework;

namespace MongoDB.Context.Tests
{
	[TestFixture]
	public class ContextUpdateTests : ContextTestBase
	{
		private TestEntity[] _TestEntities;

		[SetUp]
		public void Setup()
		{
			_TestEntities = GetTestEntities();
		}

		[Test]
		public void Should_NoUpdate_WhenNoChange()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var changes = ctx.TestEntities.GetChanges();
				changes.AssertNoChange();
			}
		}

		[Test]
		public void Should_OneSet_WhenOneRootDocumentFieldChanged()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var entity = ctx.TestEntities.Find().First();
				entity.String = "NEW VALUE";

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertUpdateCount(1);
			}
		}

		[Test]
		public void Should_OneSet_WhenTwoRootDocumentFieldsChanged()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var entity = ctx.TestEntities.Find().First();
				entity.String = "NEW VALUE";
				entity.Enum = EnumTest.Value1;

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertUpdateCount(1);
			}
		}

		[Test]
		public void Should_OneSet_WhenOneRootDocumentFieldIsChangedTwice()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var entity = ctx.TestEntities.Find().First();
				entity.String = "NEW VALUE";
				entity.String = "NEW VALUE 2";

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertUpdateCount(1);
			}
		}

		[Test]
		public void Should_NoUpdate_WhenOneRootDocumentFieldIsChangedAndThenChangedBack()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var entity = ctx.TestEntities.Find().First();
				entity.String = "NEW VALUE";
				entity.String = "OLD VALUE A";

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertNoChange();
			}
		}

		[Test]
		public void Should_OnePush_WhenAnElementIsAddedToAnArrayDocumentField()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var entity = ctx.TestEntities.Find().First();
				entity.StringArray = new[] { "OLD VALUE A1", "OLD VALUE A2", "NEW VALUE" };

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertUpdateCount(1);
			}
		}

		[Test]
		public void Should_OnePull_WhenAnElementIsRemovedFromAnArrayDocumentField()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var entity = ctx.TestEntities.Find().First();
				entity.StringArray = new[] { "OLD VALUE A1" };

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertUpdateCount(1);
			}
		}

		[Test]
		public void Should_NoUpdate_WhenOneArrayFieldIsChangedAndThenChangedBack()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var entity = ctx.TestEntities.Find().First();
				entity.StringArray = new  [] { "NEW VALUE" };
				entity.StringArray = new[] { "OLD VALUE A1", "OLD VALUE A2" };

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertNoChange();
			}
		}

		[Test]
		public void Should_OneUpdate_WhenOneSubDocumentFieldIsChanged()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var entity = ctx.TestEntities.Find().First();
				entity.SubDocument = new SubDocument { String = "NEW SUB DOCUMENT" };

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertUpdateCount(1);
			}
		}

		[Test]
		public void Should_OneUpdate_WhenTwoSubDocumentFieldsAreChanged()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var entity = ctx.TestEntities.Find().First();
				entity.SubDocument = new SubDocument { String = "NEW SUB DOCUMENT", Integer = 10 };

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertUpdateCount(1);
			}
		}

		[Test]
		public void Should_NoUpdate_WhenOneSubDocumentFieldIsChangedAndThenChangedBack()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var entity = ctx.TestEntities.Find().First();
				entity.SubDocument = new SubDocument { String = "NEW SUB DOCUMENT"};
				entity.SubDocument = new SubDocument { String = "SUB DOCUMENT A" };

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertNoChange();
			}
		}

		[Test]
		public void Should_OneUpdate_WhenOneRootDocumentFieldAndOneSubDocumentFieldIsChanged()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var entity = ctx.TestEntities.Find().First();
				entity.String = "NEW VALUE";
				entity.SubDocument = new SubDocument { String = "NEW SUB DOCUMENT" };

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertUpdateCount(1);
			}
		}

		[Test]
		public void Should_OneUpdate_WhenOneRootDocumentFieldAndTwoSubDocumentFieldsAreChanged()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var entity = ctx.TestEntities.Find().First();
				entity.String = "NEW VALUE";
				entity.SubDocument = new SubDocument { String = "NEW SUB DOCUMENT", Integer = 10 };

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertUpdateCount(1);
			}
		}

		[Test]
		public void Should_OneUpdate_WhenArrayDocumentItemModified()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var entity = ctx.TestEntities.Find().First();
				entity.SimpleArray[0].Integer = 10;

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertUpdateCount(1);
			}
		}
	}
}
