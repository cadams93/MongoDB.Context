using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MongoDB.Context.Tracking
{
	public class TrackingMongoEnumerator<TDocument, TIdField>
		: TrackingMongoEntityBase<TDocument, TIdField>, IEnumerator<TDocument>
		where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		private IEnumerator<TDocument> _DefaultEnumerator;
		private readonly IQueryable<TDocument> _Queryable;

		public TrackingMongoEnumerator(TrackedCollection<TDocument, TIdField> collection, IQueryable<TDocument> remoteSource) : base(collection)
		{
			_Queryable = remoteSource;
		}

		public void Dispose()
		{
			if (_DefaultEnumerator != null)
				_DefaultEnumerator.Dispose();
		}

		public bool MoveNext()
		{
			if (_DefaultEnumerator == null)
				_DefaultEnumerator = _Queryable.GetEnumerator();

			return _DefaultEnumerator.MoveNext();
		}

		public void Reset()
		{
			if (_DefaultEnumerator != null)
				_DefaultEnumerator.Reset();
		}

		public TDocument Current
		{
			get
			{
				if (_DefaultEnumerator == null)
					throw new Exception();

				var current = _DefaultEnumerator.Current;

				// If null, return null immediately
				if (current == null) return null;

				return TrackEntityIfRequired(current);
			}
		}

		public IEnumerable<TDocument> Iterate()
		{
			while (this.MoveNext())
				yield return this.Current;
		}

		object IEnumerator.Current
		{
			get { return Current; }
		}
	}
}
