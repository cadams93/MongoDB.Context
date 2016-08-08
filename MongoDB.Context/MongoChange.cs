using MongoDB.Driver;

namespace MongoDB.Context
{
	public class MongoChange<TDocument, TIdField> 
		where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		public WriteModel<TDocument> Change { get; set; }
		public int ExecutionOrder { get; set; }
	}
}
