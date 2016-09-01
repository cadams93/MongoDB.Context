using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Context.Changes;

namespace MongoDB.Context.Tracking
{
	/// <summary>
	/// Provides the base funtionality for a tracked MongoDB entity collection
	/// </summary>
	/// <typeparam name="TDocument">The .NET type of the MongoDB entity</typeparam>
	/// <typeparam name="TIdField">The .NET type of the ID field for the MongoDB entity</typeparam>
	public interface IMongoTrackedCollection<TDocument, TIdField> 
		: IMongoTrackedCollection, IQueryable<TDocument>
		where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		void InsertOnSubmit(TDocument entity);
		void InsertAllOnSubmit(IEnumerable<TDocument> entities);
		void DeleteOnSubmit(TDocument entity);
		void DeleteAllOnSubmit(IEnumerable<TDocument> entities);

		IEnumerable<TDocument> FindUsingFilter(FilterDefinition<TDocument> pred = null);
		MongoCollectionChangeSet<TDocument, TIdField> GetChanges();
	}

	/// <summary>
	/// 
	/// </summary>
	public interface IMongoTrackedCollection
	{
		void SubmitChanges();
	}
}
