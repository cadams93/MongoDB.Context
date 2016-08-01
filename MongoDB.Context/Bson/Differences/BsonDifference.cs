using MongoDB.Driver;

namespace MongoDB.Context.Bson.Differences
{
	public abstract class BsonDifference<TDocument, TIdField>
		   where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		public abstract UpdateDefinition<TDocument> GetMongoUpdate();
	}
}
