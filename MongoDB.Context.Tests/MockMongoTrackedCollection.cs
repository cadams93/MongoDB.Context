using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MongoDB.Context.Tests
{
	public class MockMongoTrackedCollection<T, TIdField> 
		: MongoTrackedCollection<T, TIdField>
		where T : AbstractMongoEntityWithId<TIdField>
	{
		private readonly T[] _Entities;

		public MockMongoTrackedCollection(T[] entities)
		{
			_Entities = entities;
		}

		protected override IEnumerable<T> RemoteGet(Expression<Func<T, bool>> pred = null)
		{
			var compiledPred = (pred ?? (obj => true)).Compile();

			return _Entities.Where(z => compiledPred.Invoke(z));
		}
	}
}
