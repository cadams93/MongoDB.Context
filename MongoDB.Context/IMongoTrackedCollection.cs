using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MongoDB.Context
{
	/// <summary>
	/// Provides the base funtionality for a tracked MongoDB entity collection
	/// </summary>
	/// <typeparam name="TDocument">The .NET type of the MongoDB entity</typeparam>
	/// <typeparam name="TIdField">The .NET type of the ID field for the MongoDB entity</typeparam>
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
