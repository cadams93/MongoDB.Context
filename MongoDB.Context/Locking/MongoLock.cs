using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Context.Locking
{
	public sealed class MongoLock<TIdField> : AbstractMongoEntityWithId<ObjectId>
	{
		public override string DatabaseKey { get { return "test"; } }
		public override string CollectionKey { get { return "locks"; } }

		public TIdField DocumentId { get; set; }
		public string Field { get; set; }

		public Guid TakenBy { get; set; }

		[BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
		public DateTime TakenAt { get; set; }
	}
}
