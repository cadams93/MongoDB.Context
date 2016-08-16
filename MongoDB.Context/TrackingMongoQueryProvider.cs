using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MongoDB.Context
{
	public class TrackingMongoQueryProvider<TDocument, TIdField> 
		: TrackingMongoEntityBase<TDocument, TIdField>, IQueryProvider 
		where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		private readonly IQueryProvider _Provider;

		public TrackingMongoQueryProvider(TrackedCollection<TDocument, TIdField> collection, IQueryProvider provider) : base(collection)
		{
			_Provider = provider;
		}

		public IQueryable CreateQuery(Expression expression)
		{
			return _Provider.CreateQuery(expression);
		}

		public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
		{
			return _Provider.CreateQuery<TElement>(expression);
		}

		public object Execute(Expression expression)
		{
			var result = _Provider.Execute(expression);
			if (result == null) return null;

			var resultType = result.GetType();

			// We are not returning a type of TDocument, or which implements IEnumerable<TDocument> (ie. projection has been applied)
			// Do not track!
			if (resultType != typeof(TDocument) &&
				!resultType.GetInterfaces().Any(z => z.IsGenericType && z.GetGenericTypeDefinition() == typeof(IEnumerable<>) && z.GetGenericArguments()[0] == typeof(TDocument)))
			{
				return result;
			}

			// We are returning some structure which implements IEnumerable<TDocument>
			if (resultType.GetInterfaces()
				.Any(z => z.IsGenericType && z.GetGenericTypeDefinition() == typeof(IEnumerable<>) && z.GetGenericArguments()[0] == typeof(TDocument)))
			{
				var enumerable = new List<TDocument>();

				foreach (var item in (IEnumerable)result)
				{
					var docFromEnumerable = item as TDocument;
					if (docFromEnumerable == null) continue;

					enumerable.Add(TrackEntityIfRequired(docFromEnumerable));
				}

				return enumerable;
			}

			// We are returning just one TDocument (ie. First, Single, etc)
			var doc = result as TDocument;
			return doc == null ? null : TrackEntityIfRequired(doc);
		}

		public TResult Execute<TResult>(Expression expression)
		{
			var result = _Provider.Execute<TResult>(expression);

			// We are not returning a type of TDocument, or which implements IEnumerable<TDocument> (ie. projection has been applied)
			// Do not track!
			if (typeof(TResult) != typeof(TDocument) &&
				!typeof(TResult).GetInterfaces().Any(z => z.IsGenericType && z.GetGenericTypeDefinition() == typeof(IEnumerable<>) && z.GetGenericArguments()[0] == typeof(TDocument)))
			{
				return result;
			}

			// We are returning some structure which implements IEnumerable<TDocument>
			if (typeof(TResult).GetInterfaces()
				.Any(z => z.IsGenericType && z.GetGenericTypeDefinition() == typeof(IEnumerable<>) && z.GetGenericArguments()[0] == typeof(TDocument)))
			{
				var enumerable = new List<TDocument>();

				foreach (var item in (IEnumerable)result)
				{
					var docFromEnumerable = item as TDocument;
					if (docFromEnumerable == null) continue;

					enumerable.Add(TrackEntityIfRequired(docFromEnumerable));
				}

				return (TResult)Convert.ChangeType(enumerable, typeof(TResult));
			}

			// We are returning just one TDocument (ie. First, Single, etc)
			var doc = result as TDocument;
			if (doc == null) return default(TResult);

			return (TResult)Convert.ChangeType(TrackEntityIfRequired(doc), typeof(TResult));
		}
	}
}
