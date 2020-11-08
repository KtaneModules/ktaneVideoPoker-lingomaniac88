namespace KtaneVideoPoker
{
    namespace Core
    {
        public enum HandResult
        {
            FiveOfAKind,
            Flush,
            FourAces,
            FourAcesWith234,
            FourDeuces,
            FourFivesThroughKings,
            FourTwosThreesOrFours,
            FourTwosThreesOrFoursWithA234,
            FourOfAKind,
            FullHouse,
            JacksOrBetter,
            NaturalRoyalFlush,
            Nothing,
            RoyalFlush,
            Straight,
            StraightFlush,
            ThreeOfAKind,
            TwoPair,
            WildRoyalFlush
        }
    }
}

public static class HandResultExtensions
{
    public static string ToFriendlyString(this KtaneVideoPoker.Core.HandResult result)
    {
        switch (result)
        {
            case KtaneVideoPoker.Core.HandResult.FiveOfAKind:
                return "FIVE OF A KIND";
            case KtaneVideoPoker.Core.HandResult.Flush:
                return "FLUSH";
            case KtaneVideoPoker.Core.HandResult.FourAces:
                return "FOUR ACES";
            case KtaneVideoPoker.Core.HandResult.FourAcesWith234:
                return "FOUR ACES w/ 2,3,4";
            case KtaneVideoPoker.Core.HandResult.FourDeuces:
                return "FOUR DEUCES";
            case KtaneVideoPoker.Core.HandResult.FourFivesThroughKings:
                return "FOUR 5s THRU Ks";
            case KtaneVideoPoker.Core.HandResult.FourTwosThreesOrFours:
                return "FOUR 2s, 3s, 4s";
            case KtaneVideoPoker.Core.HandResult.FourTwosThreesOrFoursWithA234:
                return "FOUR 2s, 3s, 4s w/ A,2,3,4";
            case KtaneVideoPoker.Core.HandResult.FourOfAKind:
                return "FOUR OF A KIND";
            case KtaneVideoPoker.Core.HandResult.FullHouse:
                return "FULL HOUSE";
            case KtaneVideoPoker.Core.HandResult.JacksOrBetter:
                return "JACKS OR BETTER";
            case KtaneVideoPoker.Core.HandResult.NaturalRoyalFlush:
                return "NATURAL ROYAL FLUSH";
            case KtaneVideoPoker.Core.HandResult.RoyalFlush:
                return "ROYAL FLUSH";
            case KtaneVideoPoker.Core.HandResult.Straight:
                return "STRAIGHT";
            case KtaneVideoPoker.Core.HandResult.StraightFlush:
                return "STRAIGHT FLUSH";
            case KtaneVideoPoker.Core.HandResult.ThreeOfAKind:
                return "THREE OF A KIND";
            case KtaneVideoPoker.Core.HandResult.TwoPair:
                return "TWO PAIR";
            case KtaneVideoPoker.Core.HandResult.WildRoyalFlush:
                return "WILD ROYAL FLUSH";
            default:
                return "";
        }
    }
}