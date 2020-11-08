namespace KtaneVideoPoker
{
	namespace Test
	{
		public class JacksOrBetter86Test: VariantTest
		{
			public override Core.Strategy GetStrategy()
			{
				return Variants.JacksOrBetter.Strategy86;
			}

			public override string GetStrategyName()
			{
				return "job86";
			}

			public override Variants.IVariant GetVariant()
			{
				return new Variants.JacksOrBetter(8, 6);
			}
		}
	}
}
