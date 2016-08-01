using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Context
{
	public abstract class AbstractMongoEntityWithId<TIdField> : AbstractMongoEntity
	{
		[BsonId]
		public TIdField _Id { get; set; }
	}
}
