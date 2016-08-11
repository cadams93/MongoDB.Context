using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Context
{
	public abstract class AbstractMongoEntityWithId<TIdField> : AbstractMongoEntity
	{
		public abstract string DatabaseKey { get; }
		public abstract string CollectionKey { get; }

		[BsonId]
		public TIdField _Id { get; set; }
	}
}
