using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Context
{
	public abstract class AbstractMongoEntity
	{
		[BsonExtraElements]
		private IDictionary<string, object> _ExtraElements { get; set; }

		[BsonIgnore]
		public IDictionary<string, object> ExtraElements
		{
			get { return _ExtraElements ?? new Dictionary<string, object>(); }
			set { _ExtraElements = value; }
		}
	}
}
