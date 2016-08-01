using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MongoDB.Context.Tests
{
	public class ContextTestBase
	{
		protected TestEntity[] _TestEntities;

		[TestInitialize]
		public void Setup()
		{
			_TestEntities = GetTestEntities();
		}

		protected MockMongoContext GetMongoContext()
		{
			return new MockMongoContext(_TestEntities);
		}

		protected static TestEntity[] GetTestEntities()
		{
			return new[]
			{
				new TestEntity
				{
					String = "OLD VALUE A",		
					StringArray = new [] { "OLD VALUE A1", "OLD VALUE A2" },	
					SubDocument = new SubDocument
					{
						String = "SUB DOCUMENT A"
					},
					SimpleArray = new List<SimpleObject>
					{
						new SimpleObject { Integer = 1, String = "SIMPLE ARRAY ITEM A1" },
						new SimpleObject { Integer = 2, String = "SIMPLE ARRAY ITEM A2" }
					}
				},
				new TestEntity
				{
					String = "OLD VALUE B",		
					StringArray = new [] { "OLD VALUE B1", "OLD VALUE B2" },	
					SubDocument = new SubDocument
					{
						String = "SUB DOCUMENT B"
					},
					SimpleArray = new List<SimpleObject>
					{
						new SimpleObject { Integer = 10, String = "SIMPLE ARRAY ITEM B1" },
						new SimpleObject { Integer = 20, String = "SIMPLE ARRAY ITEM B2" }
					}
				}
			};
		}
	}
}
