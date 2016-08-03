﻿using System.Linq;
using NUnit.Framework;

namespace MongoDB.Context.Tests
{
	[TestFixture]
	public class ContextDeleteTests : ContextTestBase
	{
		private TestEntity[] _TestEntities;

		[SetUp]
		public void Setup()
		{
			_TestEntities = GetTestEntities();
		}

		[Test]
		public void Should_NoDelete_WhenNoChange()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var changes = ctx.TestEntities.GetChanges();
				changes.AssertNoChange();
			}
		}

		[Test]
		public void Should_OneDelete_WhenOneEntityIsDeleted()
		{
			using (var ctx = new MockMongoContext(_TestEntities))
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
			using (var ctx = new MockMongoContext(_TestEntities))
			{
				var entities = ctx.TestEntities.Find();
				ctx.TestEntities.DeleteAllOnSubmit(entities.Take(2).ToArray());

				var changes = ctx.TestEntities.GetChanges();
				changes.AssertDeleteCount(2);
			}
		}
	}
}
