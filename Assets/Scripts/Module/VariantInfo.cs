using System;
using UnityEngine;

namespace KtaneVideoPoker
{
    public class VariantInfo
    {
        public static VariantInfo[] AllVariants = new[]
        {
            new VariantInfo("JACKS OR BETTER", "9/6 Jacks or Better", b => new Variants.JacksOrBetter(9, 6, b), Variants.JacksOrBetter.Strategy96),
            new VariantInfo("JACKS OR BETTER", "9/5 Jacks or Better", b => new Variants.JacksOrBetter(9, 5, b), Variants.JacksOrBetter.Strategy95),
            new VariantInfo("JACKS OR BETTER", "8/6 Jacks or Better", b => new Variants.JacksOrBetter(8, 6, b), Variants.JacksOrBetter.Strategy86),
            new VariantInfo("JACKS OR BETTER", "8/5 Jacks or Better", b => new Variants.JacksOrBetter(8, 5, b), Variants.JacksOrBetter.Strategy85),

            new VariantInfo("BONUS POKER", "Bonus Poker", Variants.BonusLikePoker.BonusPoker, Variants.BonusLikePoker.StrategyBonus),
            new VariantInfo("DOUBLE BONUS POKER", "Double Bonus Poker", Variants.BonusLikePoker.DoubleBonusPoker, Variants.BonusLikePoker.StrategyDoubleBonus),
            new VariantInfo("DOUBLE DOUBLE BONUS", "Double Double Bonus Poker", Variants.BonusLikePoker.DoubleDoubleBonusPoker, Variants.BonusLikePoker.StrategyDoubleDoubleBonus),
            new VariantInfo("TRIPLE DOUBLE BONUS", "Triple Double Bonus Poker", Variants.BonusLikePoker.TripleDoubleBonusPoker, Variants.BonusLikePoker.StrategyTripleDoubleBonus),

            new VariantInfo("DEUCES WILD", "Deuces Wild: Not So Ugly Ducks", Variants.DeucesWild.NotSoUglyDucks, Variants.DeucesWild.StrategyNotSoUglyDucks),
            new VariantInfo("DEUCES WILD", "Deuces Wild: Loose Deuces", Variants.DeucesWild.LooseDeuces, Variants.DeucesWild.StrategyLooseDeuces),
            new VariantInfo("DEUCES WILD", "Full Pay Deuces Wild", Variants.DeucesWild.FullPayDeucesWild, Variants.DeucesWild.StrategyFullPayDeucesWild)
        };

        /// <summary>The name of the game to display on the machine.</summary>
        public readonly string ShortName;

        /// <summary>A more specific name to be shown in the logs.</summary>
        public readonly string DetailedName;

        public readonly Variants.IVariant VariantLowBet;
        public readonly Variants.IVariant VariantMaxBet;
        public readonly Core.Strategy StrategyMaxBet;

        private VariantInfo(string shortName, string detailedName, Func<bool, Variants.IVariant> variantFactory, Core.Strategy strategyMaxBet)
        {
            ShortName = shortName;
            DetailedName = detailedName;
            VariantLowBet = variantFactory(false);
            VariantMaxBet = variantFactory(true);
            StrategyMaxBet = strategyMaxBet;
        }

        public Variants.IVariant VariantIfUsingMaxBet(bool maxBet)
        {
            return maxBet ? VariantMaxBet : VariantLowBet;
        }
    }
}
