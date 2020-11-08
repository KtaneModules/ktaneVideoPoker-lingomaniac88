namespace KtaneVideoPoker
{
	namespace Test
	{
		public class DoubleBonusPokerTest: VariantTest
		{
			public override Core.Strategy GetStrategy()
			{
				return Variants.BonusLikePoker.StrategyDoubleBonus;
			}

			public override string GetStrategyName()
			{
				return "doubleBonus";
			}

			public override Variants.IVariant GetVariant()
			{
				return Variants.BonusLikePoker.DoubleBonusPoker();
			}
		}
	}
}
