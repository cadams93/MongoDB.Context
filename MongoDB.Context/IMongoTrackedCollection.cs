using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MongoDB.Context
{
	public interface IMongoTrackedCollection<T, TIdField> where T : AbstractMongoEntityWithId<TIdField>
	{
		void InsertOnSubmit(T entity);
		void InsertAllOnSubmit(IEnumerable<T> entities);
		void DeleteOnSubmit(T entity);
		void DeleteAllOnSubmit(IEnumerable<T> entities);

		IEnumerable<T> Find(Expression<Func<T, bool>> pred = null);
		MongoChangeSet<T, TIdField> GetChanges();
	}
}
