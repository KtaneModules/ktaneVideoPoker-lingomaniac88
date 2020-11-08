namespace KtaneVideoPoker
{
	using Core;

	namespace Test
	{
		public class BonusPokerTest: VariantTest
		{
			public override Core.Strategy GetStrategy()
			{
				return Variants.BonusLikePoker.StrategyBonus;
			}

			public override string GetStrategyName()
			{
				return "bonus";
			}

			public override Variants.IVariant GetVariant()
			{
				return Variants.BonusLikePoker.BonusPoker();
			}
		}
	}
}
