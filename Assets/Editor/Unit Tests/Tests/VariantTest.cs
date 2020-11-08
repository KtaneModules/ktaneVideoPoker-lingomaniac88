using NUnit.Framework;
using System.Linq;
using UnityEngine;

namespace KtaneVideoPoker
{
	using Core;

	namespace Test
	{
		public abstract class VariantTest
		{
			private static string TestDataFolder = "Assets/Editor/Unit Tests/Data/";

			public abstract Strategy GetStrategy();

			public abstract string GetStrategyName();

			public abstract Variants.IVariant GetVariant();

			[Test]
			public void GenerateAnalysis()
			{
				var analysis = Analyzer.Analyzer.Analyze(GetVariant());
				Debug.Log(analysis.ExpectedReturn);

				analysis.WriteToFile(TestDataFolder + GetStrategyName() + ".analysis");

				/*var strategyArray = analysis.OptimalStrategies.ToArray();
				foreach (uint s in analysis.OptimalStrategies.Distinct().OrderByDescending(n => n))
				{
					Debug.LogFormat("{0}: {1}", s, strategyArray.Count(n => n == s));
				}

				var payoutArray = analysis.ExpectedPayouts.ToArray();
				foreach (long payout in analysis.ExpectedPayouts.Distinct().OrderByDescending(n => n))
				{
					Debug.LogFormat("{0}: {1}", payout, payoutArray.Count(n => n == payout));
				}*/
			}

			[Test]
			public void StrategyTest()
			{
				var variant = GetVariant();

				Analyzer.AnalysisResult analysis;
				try
				{
					analysis = Analyzer.AnalysisResult.LoadFromFile(TestDataFolder + GetStrategyName() + ".analysis");
				}
				catch (System.IO.IOException)
				{
					Debug.LogFormat("Analysis file could not be loaded. Generating on the fly...");
					analysis = Analyzer.Analyzer.Analyze(variant);
				}
				

				var strategy = GetStrategy();

				int handsProcessed = 0;
				int failureCount = 0;
				int failuresDiscardingAll = 0;

				foreach (var cards in Enumerable.Range(0, Util.StandardDeckSize + variant.JokerCount).Select(Card.CreateWithId).Combinations(5))
				{
					handsProcessed++;
					var hand = new Hand(cards);
					var result = strategy.Evaluate(hand);
					var optimal = analysis.OptimalStrategies[Util.CombinedId(Util.StandardDeckSize + variant.JokerCount, cards.Select(card => card.Id))];
					if ((uint) result.Strategies.Select(i => 1 << i).Sum() != optimal)
					{
						failureCount++;

						if (result.RuleIndex == -1)
						{
							failuresDiscardingAll++;
						}
						else
						{
							Debug.LogFormat("Failed hand [{0}]. Analysis expected [{1}], strategy said [{2}] by rule {3}{4}{5}", cards.Join(","), Enumerable.Range(0, 32).Where(i => (optimal & (1 << i)) != 0).Join(","), result.Strategies.Join(","), result.ExtraRules.Join(","), result.ExtraRules.Length > 0 ? "-" : "", result.RuleIndex + 1);
						}
					}
				}

				if (failuresDiscardingAll > 0)
				{
					Debug.LogFormat("{0} failures from rule fallthroughs", failuresDiscardingAll);
				}
				Assert.AreEqual(0, failureCount);
			}
		}
	}
}
