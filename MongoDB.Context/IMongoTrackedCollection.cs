using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MongoDB.Context
{
	public interface IMongoTrackedCollection<TDocument, TIdField> 
		where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		void InsertOnSubmit(TDocument entity);
		void InsertAllOnSubmit(IEnumerable<TDocument> entities);
		void DeleteOnSubmit(TDocument entity);
		void DeleteAllOnSubmit(IEnumerable<TDocument> entities);

		IEnumerable<TDocument> Find(Expression<Func<TDocument, bool>> pred = null);
		MongoCollectionChangeSet<TDocument, TIdField> GetChanges();
		void SubmitChanges();
	}
}
