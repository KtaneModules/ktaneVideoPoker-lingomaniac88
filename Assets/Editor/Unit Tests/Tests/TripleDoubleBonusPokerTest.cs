namespace KtaneVideoPoker
{
	namespace Test
	{
		public class TripleDoubleBonusPokerTest: VariantTest
		{
			public override Core.Strategy GetStrategy()
			{
				return Variants.BonusLikePoker.StrategyTripleDoubleBonus;
			}

			public override string GetStrategyName()
			{
				return "tdBonus";
			}

			public override Variants.IVariant GetVariant()
			{
				return Variants.BonusLikePoker.TripleDoubleBonusPoker();
			}
		}
	}
}
