using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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
		}

		protected override IEnumerable<TDocument> RemoteGet(Expression<Func<TDocument, bool>> pred = null)
		{
			var compiledPred = (pred ?? (obj => true)).Compile();

			return _Entities.Where(z => compiledPred.Invoke(z));
		}

		public override IEnumerator<TDocument> GetEnumerator()
		{
			return new TrackingEntityEnumerator<TDocument, TIdField>(TrackedEntities, _Entities.AsQueryable());
		}
	}
}
