using MongoDB.Context.Tracking;
using System.Linq;

namespace MongoDB.Context.Tests
{
	public class MockMongoTrackedCollection<TDocument, TIdField> 
		: MongoTrackedCollection<TDocument, TIdField>
		where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		private readonly TDocument[] _Entities;

		public MockMongoTrackedCollection(TDocument[] entities)
		{
			_Entities = entities;
			CollectionQueryable = entities.AsQueryable();
		}
	}
}
