using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Context.Bson;

namespace MongoDB.Context.Tests
{
	[TestClass]
	public class BsonComparerTests
	{
		private BsonDocumentComparer<TestEntity, ObjectId> _Comparer;

		[TestInitialize]
		public void Setup()
		{
			_Comparer = new BsonDocumentComparer<TestEntity, ObjectId>();
		}

		[TestMethod]
		public void Should_OneChange_WhenOneFieldChange()
		{
			var left = new BsonDocument(new Dictionary<string, object>
			{
				{ "A", 1 },
				{ "B", "OLD VALUE" }
			});

			var right = new BsonDocument(new Dictionary<string, object>
			{
				{ "A", 1 },
				{ "B", "NEW VALUE" }
			});

			var differences = _Comparer.GetDifferences(left, right);
			Assert.AreEqual(1, differences.Length);
		}

		[TestMethod]
		public void Should_OneChange_WhenOneFieldRemoved()
		{
			var left = new BsonDocument(new Dictionary<string, object>
			{
				{ "A", 1 },
				{ "B", "OLD VALUE" }
			});

			var right = new BsonDocument(new Dictionary<string, object>
			{
				{ "A", 1 }
			});

			var differences = _Comparer.GetDifferences(left, right);
			Assert.AreEqual(1, differences.Length);
		}

		[TestMethod]
		public void Should_TwoChanges_WhenTwoFieldsRemoved()
		{
			var left = new BsonDocument(new Dictionary<string, object>
			{
				{ "A", 1 },
				{ "B", "OLD VALUE" }
			});

			var right = new BsonDocument();

			var differences = _Comparer.GetDifferences(left, right);
			Assert.AreEqual(2, differences.Length);
		}

		[TestMethod]
		public void Should_TwoChanges_WhenOneFieldAndAnotherAdded()
		{
			var left = new BsonDocument(new Dictionary<string, object>
			{
				{ "A", 1 },
				{ "B", "OLD VALUE" }
			});

			var right = new BsonDocument(new Dictionary<string, object>
			{
				{ "A", 1 },
				{ "C", 1 }
			});

			var differences = _Comparer.GetDifferences(left, right);
			Assert.AreEqual(2, differences.Length);
		}

		[TestMethod]
		public void Should_OneChange_WhenOneFieldAdded()
		{
			var left = new BsonDocument(new Dictionary<string, object>
			{
				{ "A", 1 },
				{ "B", "OLD VALUE" }
			});

			var right = new BsonDocument(new Dictionary<string, object>
			{
				{ "A", 1 },
				{ "B", "OLD VALUE" },
				{ "C", 1 }
			});

			var differences = _Comparer.GetDifferences(left, right);
			Assert.AreEqual(1, differences.Length);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException), "Value for field C used to be of type BsonArray, trying to set as type BsonString")]
		public void Should_ThrowException_WhenFieldTypeChangedFromArrayToString()
		{
			var left = new BsonDocument(new Dictionary<string, object>
			{
				{ "A", 1 },
				{ "B", "OLD VALUE" },
				{ "C", 
					new []
					{
						"C.1",
						"C.2"
					}
				}
			});

			var right = new BsonDocument(new Dictionary<string, object>
			{
				{ "A", 1 },
				{ "B", "OLD VALUE" },
				{ "C", 1 }
			});

			_Comparer.GetDifferences(left, right);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException), "Value for field C used to be of type BsonString, trying to set as an array")]
		public void Should_ThrowException_WhenFieldTypeChangedFromStringToArray()
		{
			var left = new BsonDocument(new Dictionary<string, object>
			{
				{ "A", 1 },
				{ "B", "OLD VALUE" },
				{ "C", 1 }
			});

			var right = new BsonDocument(new Dictionary<string, object>
			{
				{ "A", 1 },
				{ "B", "OLD VALUE" },
				{ "C", 
					new []
					{
						"C.1",
						"C.2"
					}
				}
			});

			_Comparer.GetDifferences(left, right);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException), "Value for field C used to be of type BsonInteger, trying to set as type BsonString")]
		public void Should_ThrowException_WhenFieldTypeChangedFromIntegerToString()
		{
			var left = new BsonDocument(new Dictionary<string, object>
			{
				{ "A", 1 },
				{ "B", "OLD VALUE" },
				{ "C", 1 }
			});

			var right = new BsonDocument(new Dictionary<string, object>
			{
				{ "A", 1 },
				{ "B", "OLD VALUE" },
				{ "C", "1" }
			});

			_Comparer.GetDifferences(left, right);
		}

		[TestMethod]
		public void Should_NoChange_WhenArrayUnchanged()
		{
			var left = new BsonDocument(new Dictionary<string, object>
			{
				{ "A", 1 },
				{ "B", "OLD VALUE" },
				{ "C", 
					new []
					{
						"C.1",
						"C.2"
					} 
				}
			});

			var right = new BsonDocument(new Dictionary<string, object>
			{
				{ "A", 1 },
				{ "B", "OLD VALUE" },
				{ "C", 
					new []
					{
						"C.1",
						"C.2"
					} 
				}
			});

			var differences = _Comparer.GetDifferences(left, right);
			Assert.AreEqual(0, differences.Length);
		}

		[TestMethod]
		public void Should_OneChange_WhenArrayItemAdded()
		{
			var left = new BsonDocument(new Dictionary<string, object>
			{
				{ "A", 1 },
				{ "B", "OLD VALUE" },
				{ "C", 
					new []
					{
						"C.1",
						"C.2"
					} 
				}
			});

			var right = new BsonDocument(new Dictionary<string, object>
			{
				{ "A", 1 },
				{ "B", "OLD VALUE" },
				{ "C", 
					new []
					{
						"C.1",
						"C.2",
						"C.3"
					} 
				}
			});

			var differences = _Comparer.GetDifferences(left, right);
			Assert.AreEqual(1, differences.Length);
		}

		[TestMethod]
		public void Should_OneChange_WhenArrayItemRemoved()
		{
			var left = new BsonDocument(new Dictionary<string, object>
			{
				{ "A", 1 },
				{ "B", "OLD VALUE" },
				{ "C", 
					new []
					{
						"C.1",
						"C.2"
					} 
				}
			});

			var right = new BsonDocument(new Dictionary<string, object>
			{
				{ "A", 1 },
				{ "B", "OLD VALUE" },
				{ "C", 
					new []
					{
						"C.1"
					} 
				}
			});

			var differences = _Comparer.GetDifferences(left, right);
			Assert.AreEqual(1, differences.Length);
		}

		[TestMethod]
		public void Should_OneChange_WhenArrayItemRemovedAndAnotherInserted()
		{
			var left = new BsonDocument(new Dictionary<string, object>
			{
				{ "A", 1 },
				{ "B", "OLD VALUE" },
				{ "C", 
					new []
					{
						"C.1",
						"C.2"
					} 
				}
			});

			var right = new BsonDocument(new Dictionary<string, object>
			{
				{ "A", 1 },
				{ "B", "OLD VALUE" },
				{ "C", 
					new []
					{
						"C.2",
						"C.3"
					} 
				}
			});

			var differences = _Comparer.GetDifferences(left, right);
			Assert.AreEqual(1, differences.Length);
		}
	}
}
