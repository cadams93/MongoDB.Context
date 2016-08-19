using MongoDB.Context.Client;
using MongoDB.Context.Lcs;
using NUnit.Framework;

namespace MongoDB.Context.Tests.Lcs
{
	[TestFixture]
	public class SimpleObjectLcsTests
	{
		private LcsAlgorithm<SimpleObject> _LcsAlgorithm;
		[OneTimeSetUp]
		public void Setup()
		{
			_LcsAlgorithm = new LcsAlgorithm<SimpleObject>();
		}

		[Test]
		public void Should_NoSequence_WhenZeroItemArrayAddOneItem()
		{
			var result = _LcsAlgorithm.GetLcs(
				new SimpleObject[] { },
				new[]
				{
					new SimpleObject {Integer = 1, String = "A"}
				});

			Assert.AreEqual(0, result.Sequence.Count);
		}

		[Test]
		public void Should_OneSequence_WhenOneItemArrayItemUnmodified()
		{
			var result = _LcsAlgorithm.GetLcs(
				new[]
				{
					new SimpleObject {Integer = 1, String = "A"}
				},
				new[]
				{
					new SimpleObject {Integer = 1, String = "A"}
				});

			Assert.AreEqual(1, result.Sequence.Count);
		}

		[Test]
		public void Should_NoSequence_WhenOneItemArrayItemModified()
		{
			var result = _LcsAlgorithm.GetLcs(
				new[]
				{
					new SimpleObject {Integer = 1, String = "A"}
				},
				new[]
				{
					new SimpleObject {Integer = 1, String = "B"}
				});

			Assert.AreEqual(0, result.Sequence.Count);
		}

		[Test]
		public void Should_NoSequence_WhenOneItemArrayItemRemoved()
		{
			var result = _LcsAlgorithm.GetLcs(
				new[]
				{
					new SimpleObject {Integer = 1, String = "A"}
				},
				new SimpleObject[] {});

			Assert.AreEqual(0, result.Sequence.Count);
		}

		[Test]
		public void Should_OneSequence_WhenOneItemArrayAddOneNewItem()
		{
			var result = _LcsAlgorithm.GetLcs(
				new[]
				{
					new SimpleObject {Integer = 1, String = "A"}
				},
				new []
				{
					new SimpleObject {Integer = 1, String = "A"},
					new SimpleObject {Integer = 2, String = "B"}
				});

			Assert.AreEqual(1, result.Sequence.Count);
		}

		[Test]
		public void Should_OneSequence_WhenOneItemArrayAddTwoNewItems()
		{
			var result = _LcsAlgorithm.GetLcs(
				new[]
				{
					new SimpleObject {Integer = 1, String = "A"}
				},
				new []
				{
					new SimpleObject {Integer = 1, String = "A"},
					new SimpleObject {Integer = 2, String = "B"},
					new SimpleObject {Integer = 3, String = "C"}
				});

			Assert.AreEqual(1, result.Sequence.Count);
		}

		[Test]
		public void Should_NoSequence_WhenOneItemArrayAddOneItemAndModifyTheOriginal()
		{
			var result = _LcsAlgorithm.GetLcs(
				new[]
				{
					new SimpleObject {Integer = 1, String = "A"}
				},
				new []
				{
					new SimpleObject {Integer = 1, String = "D"},
					new SimpleObject {Integer = 2, String = "B"},
					new SimpleObject {Integer = 3, String = "C"}
				});

			Assert.AreEqual(0, result.Sequence.Count);
		}

		[Test]
		public void Should_TwoSequence_WhenTwoItemArrayUnmodified()
		{
			var result = _LcsAlgorithm.GetLcs(
				new[]
				{
					new SimpleObject {Integer = 1, String = "A"},
					new SimpleObject {Integer = 2, String = "B"}
				},
				new []
				{
					new SimpleObject {Integer = 1, String = "A"},
					new SimpleObject {Integer = 2, String = "B"}
				});

			Assert.AreEqual(2, result.Sequence.Count);
		}

		[Test]
		public void Should_NoSequence_WhenTwoItemArrayBothRemoved()
		{
			var result = _LcsAlgorithm.GetLcs(
				new[]
				{
					new SimpleObject {Integer = 1, String = "A"},
					new SimpleObject {Integer = 2, String = "B"}
				},
				new SimpleObject[] {});

			Assert.AreEqual(0, result.Sequence.Count);
		}

		[Test]
		public void Should_TwoSequence_WhenTwoItemArrayAddOneItem()
		{
			var result = _LcsAlgorithm.GetLcs(
				new[]
				{
					new SimpleObject {Integer = 1, String = "A"},
					new SimpleObject {Integer = 2, String = "B"}
				},
				new[]
				{
					new SimpleObject {Integer = 1, String = "A"},
					new SimpleObject {Integer = 2, String = "B"},
					new SimpleObject {Integer = 3, String = "C"}
				});

			Assert.AreEqual(2, result.Sequence.Count);
		}

		[Test]
		public void Should_OneSequence_WhenTwoItemArrayAddOneItemAndOneOriginalRemoved()
		{
			var result = _LcsAlgorithm.GetLcs(
				new[]
				{
					new SimpleObject {Integer = 2, String = "B"}
				},
				new[]
				{
					new SimpleObject {Integer = 1, String = "A"},
					new SimpleObject {Integer = 2, String = "B"},
					new SimpleObject {Integer = 3, String = "C"}
				});

			Assert.AreEqual(1, result.Sequence.Count);
		}
	}
}
