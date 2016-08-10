using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using NUnit.Framework;

namespace MongoDB.Context.Tests
{
	[TestFixture]
	public class TypeMapperTests
	{
		[Test]
		public void Should_MapToString_WhenBsonString()
		{
			var type = MongoBsonTypeMapper<TestEntity, ObjectId>.GetDotNetValue(new object[] {"String"}, BsonString.Create("TEST"));
			Assert.That(type, Is.TypeOf<string>());
			Assert.That(type, Is.EqualTo("TEST"));
		}

		[Test]
		public void Should_MapToInteger_WhenBsonInteger()
		{
			var type = MongoBsonTypeMapper<TestEntity, ObjectId>.GetDotNetValue(new object[] {"Integer"}, BsonInt32.Create(1));
			Assert.That(type, Is.TypeOf<int>());
			Assert.That(type, Is.EqualTo(1));
		}

		[Test]
		public void Should_MapToNull_WhenBsonNull()
		{
			var type = MongoBsonTypeMapper<TestEntity, ObjectId>.GetDotNetValue(new object[] {"String"}, BsonNull.Value);
			Assert.That(type, Is.EqualTo(null));
		}

		[Test]
		public void Should_MapToStringArray_WhenStringArray()
		{
			var stringArray = new [] {"A", "B", "C"};
			var bsonStringArray = BsonTypeMapper.MapToBsonValue(stringArray);

			var type = MongoBsonTypeMapper<TestEntity, ObjectId>.GetDotNetValue(new object[] {"StringArray"}, bsonStringArray);
			Assert.That(type, Is.TypeOf<object[]>());
			Assert.That(type, Is.EqualTo(new []{"A", "B", "C"}));
		}

		[Test]
		public void Should_MapToSimpleObject_WhenSimpleObject()
		{
			var simpleObj = new BsonDocument
			{
				{"Integer", 1},
				{"String", "TEST"}
			};

			var type = MongoBsonTypeMapper<TestEntity, ObjectId>.GetDotNetValue(new object[] { "SimpleArray", 0 }, simpleObj);
			Assert.That(type, Is.TypeOf<SimpleObject>());
		}

		[Test]
		public void Should_MapToDictionary_WhenSimpleObjectOutsideOfClass()
		{
			var simpleObj = new BsonDocument
			{
				{"Integer", 1},
				{"String", "TEST"}
			};

			var type = MongoBsonTypeMapper<TestEntity, ObjectId>.GetDotNetValue(new object[] { "SomeOtherProperty" }, simpleObj);
			Assert.That(type, Is.TypeOf<Dictionary<string, object>>());
		}

		[Test]
		public void Should_MapToSimpleObjectArray_WhenSimpleObjectArray()
		{
			var simpleObjArray = new BsonArray
			{
				new BsonDocument
				{
					{"Integer", 1},
					{"String", "TEST"}
				}, new BsonDocument
				{
					{"Integer", 1},
					{"String", "TEST"}
				}
			};

			var type = MongoBsonTypeMapper<TestEntity, ObjectId>.GetDotNetValue(new object[] { "SimpleArray" }, simpleObjArray);
			Assert.That(type, Is.TypeOf<object[]>());
			Assert.That(((IEnumerable<object>)type).First(), Is.InstanceOf<SimpleObject>());
		}
	}
}
