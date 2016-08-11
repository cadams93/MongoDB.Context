using System.Collections;
using System.Collections.Generic;

namespace MongoDB.Context
{
	public class TrackingEntityEnumerator<TDocument, TIdField>
		: IEnumerator<TDocument>
		where TDocument : AbstractMongoEntityWithId<TIdField>
	{
		private readonly IEnumerator<TDocument> _DefaultEnumerator;
		private readonly TrackedCollection<TDocument, TIdField> _Collection;

		public TrackingEntityEnumerator(TrackedCollection<TDocument, TIdField> collection, IEnumerable<TDocument> remoteSource)
		{
			_Collection = collection;
			_DefaultEnumerator = remoteSource.GetEnumerator();
		}

		public void Dispose()
		{
			_DefaultEnumerator.Dispose();
		}

		public bool MoveNext()
		{
			return _DefaultEnumerator.MoveNext();
		}

		public void Reset()
		{
			_DefaultEnumerator.Reset();
		}

		public TDocument Current
		{
			get
			{
				var current = _DefaultEnumerator.Current;

				// If null, return null immediately
				if (current == null) return null;

				// If we already have this entity tracked, return the tracked version
				if (_Collection.Contains(current))
					return _Collection[current].Entity;

				// Otherwise, begin tracking and return
				_Collection.Attach(current, EntityState.ReadFromSource);
				return current;
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
