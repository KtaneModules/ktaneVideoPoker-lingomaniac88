using NUnit.Framework;
using System.Linq;
using UnityEngine;

namespace KtaneVideoPoker
{
	using Core;

	namespace Test
	{
		public class AllAmerican40Test: VariantTest
		{
			public override Core.Strategy GetStrategy()
			{
				return Variants.AllAmerican.Strategy40;
			}

			public override string GetStrategyName()
			{
				return "aa40";
			}

			public override Variants.IVariant GetVariant()
			{
				return new Variants.AllAmerican(40);
			}

			[Test]
			public void JackAceTest()
			{
				var variant = GetVariant();

				Analyzer.AnalysisResult analysis;
				try
				{
					analysis = Analyzer.AnalysisResult.LoadFromFile("Assets/Scripts/Module/Test/Data/" + GetStrategyName() + ".analysis");
				}
				catch (System.IO.IOException)
				{
					Debug.LogFormat("Analysis file could not be loaded. Generating on the fly...");
					analysis = Analyzer.Analyzer.Analyze(variant);
				}

				foreach (var cards in Enumerable.Range(0, Util.StandardDeckSize + variant.JokerCount).Select(Card.CreateWithId).Combinations(5))
				{
					var hand = new Hand(cards);
					var handInfo = new DetailedHandInfo(hand);
					if (!(handInfo.RankCounts[11] == 1 && handInfo.RankCounts[12] == 0 && handInfo.RankCounts[13] == 0 && handInfo.RankCounts[1] == 1 && handInfo.RankCounts.Max() == 1))
					{
						continue;
					}
					var jackAndAce = handInfo.Cards.Where(card => card.Rank % 10 == 1).ToArray();
					if (jackAndAce[0].Suit == jackAndAce[1].Suit)
					{
						continue;
					}

					var optimal = analysis.OptimalStrategies[Util.CombinedId(Util.StandardDeckSize + variant.JokerCount, cards.Select(card => card.Id))];

					// int mask = 0b0000_0001_0001_0111_0001_0111_0111_1110;
					if ((optimal | 0x0117177E) == 0x0117177E)
					{
						Debug.LogFormat("Hand=[{0}] Optimal=[{1}]", cards.Join(","), Enumerable.Range(0, 32).Where(i => (optimal & (1 << i)) != 0).Join(","));
					}
				}
			}
		}
	}
}
