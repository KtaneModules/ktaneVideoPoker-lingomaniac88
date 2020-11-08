namespace KtaneVideoPoker
{
	namespace Test
	{
		public class DoubleDoubleBonusPokerTest: VariantTest
		{
			public override Core.Strategy GetStrategy()
			{
				return Variants.BonusLikePoker.StrategyDoubleDoubleBonus;
			}

			public override string GetStrategyName()
			{
				return "ddBonus";
			}

			public override Variants.IVariant GetVariant()
			{
				return Variants.BonusLikePoker.DoubleDoubleBonusPoker();
			}
		}
	}
}
