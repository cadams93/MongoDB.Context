using System.Collections.Generic;
using System.Linq;
using MongoDB.Context.Bson.Differences;

namespace MongoDB.Context
{
	public class MongoChangeSet<TDocument, TIdField> 
		where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		public TrackedEntity<TDocument, TIdField>[] Inserts { get; private set; }
		public Dictionary<TDocument, BsonDifference<TDocument, TIdField>[]> Updates { get; private set; }
		public TrackedEntity<TDocument, TIdField>[] Deletes { get; private set; }

		public MongoChangeSet(
			IEnumerable<TrackedEntity<TDocument, TIdField>> inserts, 
			Dictionary<TDocument, IEnumerable<BsonDifference<TDocument, TIdField>>> updates,
			IEnumerable<TrackedEntity<TDocument, TIdField>> deletes)
		{
			Inserts = inserts.ToArray();
			Updates = updates.ToDictionary(z => z.Key, z => z.Value.ToArray());
			Deletes = deletes.ToArray();
		}
	}
}
