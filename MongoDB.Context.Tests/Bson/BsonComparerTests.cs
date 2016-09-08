using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Context.Bson;
using NUnit.Framework;
using MongoDB.Context.Tests.Entities;

namespace MongoDB.Context.Tests.Bson
{
	[TestFixture]
	public class BsonComparerTests
	{
		private BsonDocumentComparer<TestEntity, ObjectId> _Comparer;

		[OneTimeSetUp]
		public void Setup()
		{
			_Comparer = new BsonDocumentComparer<TestEntity, ObjectId>();
		}

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
		public void Should_ThrowException_WhenFieldTypeChangedFromArrayToInteger()
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

			var ex = Assert.Throws<InvalidOperationException>(delegate { _Comparer.GetDifferences(left, right); });
			Assert.That(ex.Message, Is.EqualTo("Value for field C used to be of type Array, trying to set as type Int32"));
		}

		[Test]
		public void Should_ThrowException_WhenFieldTypeChangedFromIntegerToArray()
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

			var ex = Assert.Throws<InvalidOperationException>(delegate { _Comparer.GetDifferences(left, right); });
			Assert.That(ex.Message, Is.EqualTo("Value for field C used to be of type Int32, trying to set as type Array"));
		}

		[Test]
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

			var ex = Assert.Throws<InvalidOperationException>(delegate { _Comparer.GetDifferences(left, right); });
			Assert.That(ex.Message, Is.EqualTo("Value for field C used to be of type Int32, trying to set as type String"));
		}

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
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
			Assert.AreEqual(2, differences.Length);
		}

		[Test]
		public void Should_OneChange_WhenArrayItemRemovedAndAnotherModified()
		{
			var left = new BsonDocument(new Dictionary<string, object>
			{
				{ "A", 1 },
				{ "B", "OLD VALUE" },
				{ "C", 
					new []
					{
						new Dictionary<string, object>
						{
							{ "TEST A1", "C.1" },
							{ "TEST A2", "C.2" }
						},
						new Dictionary<string, object>
						{
							{ "TEST B1", "C.3" },
							{ "TEST B2", "C.4" }
						}
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
						new Dictionary<string, object>
						{
							{ "TEST B1", "NEW VALUE" },
							{ "TEST B2", "C.4" }
						}
					} 
				}
			});

			var differences = _Comparer.GetDifferences(left, right);
			Assert.AreEqual(2, differences.Length);
		}
	}
}
