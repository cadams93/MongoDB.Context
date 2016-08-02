using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Context
{
	public enum EnumTest
	{
		Value1 = 1,
		Value2 = 2
	}

	public class TestEntity : AbstractMongoEntityWithId<ObjectId>
	{
		public Guid Guid { get; set; }

		[BsonElement("Date")]
		public DateTime _UtcDate { get; set; }
		[BsonIgnore]
		public DateTime Date
		{
			get { return _UtcDate.ToLocalTime(); }
			set { _UtcDate = value.ToUniversalTime(); }
		}

		public string String { get; set; }
		public ObjectId? ObjectId { get; set; }
		public EnumTest Enum { get; set; }
		public string[] StringArray { get; set; }
		public List<SimpleObject> SimpleArray { get; set; }
		public SubDocument SubDocument { get; set; }
		public Dictionary<string, string> Dictionary { get; set; }
		public Dictionary<string, SimpleObject[]> SimpleDictionary { get; set; }
	}

	public class SubDocument : AbstractMongoEntity
	{
		public string String { get; set; }
		public int Integer { get; set; }
	}

	public class SimpleObject : AbstractMongoEntity, IEquatable<SimpleObject>
	{
		public int Integer { get; set; }
		public string String { get; set; }

		public bool Equals(SimpleObject other)
		{
			if (other == null) return false;
			return this.Integer == other.Integer
			       && this.String == other.String;
		}
	}
}
