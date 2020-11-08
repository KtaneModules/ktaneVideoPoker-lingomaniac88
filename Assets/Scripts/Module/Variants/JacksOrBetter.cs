using System.Collections.Generic;
using System.Linq;

namespace KtaneVideoPoker
{
    namespace Variants
    {
        public class JacksOrBetter: IVariant
        {
            public int JokerCount { get { return 0; }}

            private int FullHousePayout;
            private int FlushPayout;
            private bool BettingFiveCredits;

            public JacksOrBetter(int fullHousePayout, int flushPayout, bool bettingFiveCredits = true)
            {
                FullHousePayout = fullHousePayout;
                FlushPayout = flushPayout;
                BettingFiveCredits = bettingFiveCredits;
            }

            public Core.HandResult Evaluate(Core.Hand hand)
            {
                int straightCard = hand.GetHighestStraightCard();
                bool isFlushLike = hand.IsFlushLike();
                int matches = hand.GetPairwiseMatchCount();

                if (isFlushLike && straightCard != 0)
                {
                    if (straightCard == 1)
                    {
                        return Core.HandResult.RoyalFlush;
                    }
                    else
                    {
                        return Core.HandResult.StraightFlush;
                    }
                }
                else if (matches == 6)
                {
                    return Core.HandResult.FourOfAKind;
                }
                else if (matches == 4)
                {
                    return Core.HandResult.FullHouse;
                }
                else if (isFlushLike)
                {
                    return Core.HandResult.Flush;
                }
                else if (straightCard != 0)
                {
                    return Core.HandResult.Straight;
                }
                else if (matches == 3)
                {
                    return Core.HandResult.ThreeOfAKind;
                }
                else if (matches == 2)
                {
                    return Core.HandResult.TwoPair;
                }
                else
                {
                    var highCardCounts = new Dictionary<int, int>() {
                        {11, 0}, {12, 0}, {13, 0}, {1, 0}
                    };

                    foreach (var card in hand.Cards)
                    {
                        if (highCardCounts.ContainsKey(card.Rank))
                        {
                            highCardCounts[card.Rank]++;
                            if (highCardCounts[card.Rank] == 2)
                            {
                                return Core.HandResult.JacksOrBetter;
                            }
                        }
                    }

                    return Core.HandResult.Nothing;
                }
            }

            public Core.HandResult[] HandTypes()
            {
                return new[]
                {
                    Core.HandResult.RoyalFlush,
                    Core.HandResult.StraightFlush,
                    Core.HandResult.FourOfAKind,
                    Core.HandResult.FullHouse,
                    Core.HandResult.Flush,
                    Core.HandResult.Straight,
                    Core.HandResult.ThreeOfAKind,
                    Core.HandResult.TwoPair,
                    Core.HandResult.JacksOrBetter
                };
            }

            public int PayoutForResult(Core.HandResult result)
            {
                switch (result)
                {
                    case Core.HandResult.RoyalFlush:
                        return BettingFiveCredits ? 800 : 250;
                    case Core.HandResult.StraightFlush:
                        return 50;
                    case Core.HandResult.FourOfAKind:
                        return 25;
                    case Core.HandResult.FullHouse:
                        return FullHousePayout;
                    case Core.HandResult.Flush:
                        return FlushPayout;
                    case Core.HandResult.Straight:
                        return 4;
                    case Core.HandResult.ThreeOfAKind:
                        return 3;
                    case Core.HandResult.TwoPair:
                        return 2;
                    case Core.HandResult.JacksOrBetter:
                        return 1;
                    default:
                        return 0;
                }
            }

            // STRATEGIES

            private static IEnumerable<int> AllRanks = Enumerable.Range(1, 13);
            private static IEnumerable<int> CardIndices = Enumerable.Range(0, 5);
            private static int[] RoyalRanks = new[] {1, 10, 11, 12, 13};

            public static Core.Strategy Strategy96 = new Core.Strategy(
                (handInfo) => // 1. Royal flush
                {
                    var flushes = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 5);
                    if (!flushes.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    if (Enumerable.SequenceEqual(flushes.First().Value, RoyalRanks))
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 2. Straight flush
                {
                    var flushes = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 5);
                    if (!flushes.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var ranks = flushes.First().Value;
                    if (ranks[4] - ranks[0] == 4)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 3. Four of a kind (rule A: You may either keep or discard the kicker.)
                {
                    var quads = handInfo.RankCounts.IndexOf(4);
                    if (quads > 0)
                    {
                        var cardIndicesToKeep = handInfo.CardPositionMatrix.Values.Select(array => array[quads]);
                        return Core.RuleResult.Pass("A", cardIndicesToKeep.Select(i => 1 << i).Sum(), 31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 4. 4 to a royal flush
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Intersect(RoyalRanks).Count() == 4);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var validSuit = validSuits.First().Key;
                    var ranks = handInfo.CardPositionMatrix[validSuit].Keys.Intersect(RoyalRanks);
                    return Core.RuleResult.Pass(ranks.Select(r => 1 << handInfo.CardPositionMatrix[validSuit][r]).Sum());
                },
                (handInfo) => // 5. Full house
                {
                    var trips = handInfo.RankCounts.IndexOf(3);
                    var pair = handInfo.RankCounts.IndexOf(2);
                    if (trips > 0 && pair > 0)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 6. Flush
                {
                    var flushes = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 5);
                    if (flushes.Any())
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 7. Straight
                {
                    var ranks = AllRanks.Where(r => handInfo.RankCounts[r] == 1).ToArray();
                    if (ranks.Length == 5 && (Enumerable.SequenceEqual(ranks, RoyalRanks) || ranks[4] - ranks[0] == 4))
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 8. Three of a kind
                {
                    var trips = handInfo.RankCounts.IndexOf(3);
                    if (trips > 0)
                    {
                        var cardIndicesToKeep = CardIndices.Where(i => handInfo.Cards[i].Rank == trips);
                        return Core.RuleResult.Pass(cardIndicesToKeep.Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 9. 4 to a straight flush
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Count() == 4);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var suitAndRanks = validSuits.First();
                    var suit = suitAndRanks.Key;
                    var ranks = suitAndRanks.Value;
                    if (ranks[3] - ranks[0] <= 4)
                    {
                        return Core.RuleResult.Pass(ranks.Select(r => 1 << handInfo.CardPositionMatrix[suit][r]).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 10. Two pair
                {
                    if (handInfo.RankCounts.Count(n => n == 2) == 2)
                    {
                        var indexToDiscard = CardIndices.Where(i => handInfo.RankCounts[handInfo.Cards[i].Rank] == 1).First();
                        return Core.RuleResult.Pass(31 - (1 << indexToDiscard));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 11. High pair
                {
                    var pairs = new[] {1, 11, 12, 13}.Where(r => handInfo.RankCounts[r] == 2);
                    if (pairs.Any())
                    {
                        var rank = pairs.First();
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == rank).Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 12. 3 to a royal flush (rule B: 4 to a flush beats 3 to a royal if the latter contains a 10 and an Ace, and the unsuited card is either a 10 or a straight penalty card.)
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Intersect(RoyalRanks).Count() == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var validSuit = validSuits.First().Key;
                    if (handInfo.RanksPerSuit[validSuit].Length == 4 && handInfo.RanksPerSuit[validSuit].Except(new[] {1, 10}).Count() == 2)
                    {
                        // We have four cards with this suit, two of which are an Ace and a 10
                        var unsuitedCard = handInfo.Cards.Where(card => card.Suit != validSuit).First();
                        if (unsuitedCard.Rank >= 10 || unsuitedCard.Rank == 1)
                        {
                            return Core.RuleResult.Fail("B");
                        }
                    }
                    var ranks = handInfo.CardPositionMatrix[validSuit].Keys.Intersect(RoyalRanks);
                    return Core.RuleResult.Pass(ranks.Select(r => 1 << handInfo.CardPositionMatrix[validSuit][r]).Sum());
                },
                (handInfo) => // 13. 4 to a flush
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 4);
                    if (validSuits.Any())
                    {
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[validSuits.First().Key].Values.Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 14. Unsuited TJQK (rule C: If you also have a pair of 10s, it doesn't matter which 10 you discard.)
                {
                    var tjqkCounts = handInfo.RankCounts.GetRange(10, 4);
                    if (Enumerable.SequenceEqual(tjqkCounts, new[] {1, 1, 1, 1}))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(10, 13)).Select(i => 1 << i).Sum());
                    }
                    else if (Enumerable.SequenceEqual(tjqkCounts, new[] {2, 1, 1, 1}))
                    {
                        var indicesOfTens = CardIndices.Where(i => handInfo.Cards[i].Rank == 10).ToArray();
                        return Core.RuleResult.Pass("C", 31 - (1 << indicesOfTens[0]), 31 - (1 << indicesOfTens[1]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 15. Low pair
                {
                    var pairRank = handInfo.RankCounts.IndexOf(2);
                    if (pairRank > 0)
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == pairRank).Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                // Below here, we can assume that all ranks are different.
                (handInfo) => // 16. 4 to an open-ended straight
                {
                    int rankCount = handInfo.RankCounts.GetRange(1, 4).Sum();
                    for (int minRank = 2; minRank <= 9; minRank++)
                    {
                        rankCount += handInfo.RankCounts[minRank + 3] - handInfo.RankCounts[minRank - 1];
                        if (rankCount == 4)
                        {
                            return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(minRank, minRank + 3)).Select(i => 1 << i).Sum());
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 17. 3 to a straight flush, type 1 (the number of high cards equals or exceeds the number of gaps, except Ace low or 2-3-4)
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var suitAndRanks = validSuits.First();
                    int gapCount = suitAndRanks.Value[2] - suitAndRanks.Value[0] - 2;
                    int highCount = suitAndRanks.Value.Count(rank => rank >= 11);
                    if (highCount >= gapCount && suitAndRanks.Value[0] >= 3)
                    {
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Values.Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 18. Suited JQ (rule D: Unsuited JQKA beats suited JQ if the latter has a 9 or a flush penalty card.)
                {
                    var hasJQ = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.Keys.Where(r => r == 11 || r == 12).Count() == 2);
                    if (!hasJQ.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var jqSuit = hasJQ.First();
                    if (handInfo.RankCounts[13] == 1 && handInfo.RankCounts[1] == 1)
                    {
                        // We also have an unsuited JQKA. Does the exception apply?
                        // We know that the King and Ace aren't of the same suit as the suited JQ. Otherwise we'd have 3 to a royal.
                        var lastCard = handInfo.Cards.Where(card => card.Rank.InRange(2, 10)).First();
                        if (lastCard.Rank == 9 || lastCard.Suit == jqSuit.Key)
                        {
                            return Core.RuleResult.Fail("D");
                        }
                    }
                    return Core.RuleResult.Pass((1 << handInfo.CardPositionMatrix[jqSuit.Key][11]) + (1 << handInfo.CardPositionMatrix[jqSuit.Key][12]));
                },
                (handInfo) => // 19. Unsuited JQKA
                {
                    var ranks = new[] {1, 11, 12, 13};
                    if (ranks.All(rank => handInfo.RankCounts[rank] == 1))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank >= 11 || handInfo.Cards[i].Rank == 1).Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 20. Suited JK, QK, JA, QA, or KA
                {
                    var ranks = new[] {1, 11, 12, 13};
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.Keys.Intersect(ranks).Count() == 2);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    return Core.RuleResult.Pass(validSuits.First().Value.Where(kvp => kvp.Key >= 11 || kvp.Key == 1).Select(kvp => 1 << kvp.Value).Sum());
                },
                (handInfo) => // 21. 9JQK, TJQA, TJKA, or TQKA (rule E: 3 to a straight flush beats 4 to a straight if the former doesn’t have a straight penalty card.)
                {
                    var partialStraights = new[]
                    {
                        new[] {9, 11, 12, 13},
                        new[] {10, 11, 12, 1},
                        new[] {10, 11, 13, 1},
                        new[] {10, 12, 13, 1}
                    };
                    var matches = partialStraights.Where(partialStraight => partialStraight.Count(rank => handInfo.RankCounts[rank] == 1) == 4);
                    if (!matches.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var match = matches.First();
                    // Look for straight flush exception
                    var validPartialStraightFlushes = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3).Select(kvp => kvp.Value);
                    if (validPartialStraightFlushes.Any())
                    {
                        if (Enumerable.SequenceEqual(validPartialStraightFlushes.First(), new[] {match[1] - 4, match[0], match[1]}))
                        {
                            return Core.RuleResult.Fail("E");
                        }
                    }
                    // We're good
                    int rankToDiscard = handInfo.Cards.Select(card => card.Rank).Sum() - match.Sum();
                    return Core.RuleResult.Pass(31 - (1 << CardIndices.Where(i => handInfo.Cards[i].Rank == rankToDiscard).First()));
                },
                (handInfo) => // 22. 3 to a straight flush, type 2 (one gap with no high cards, two gaps with one high card, Ace low, or 2-3-4)
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var suitAndRanks = validSuits.First();
                    int gapCount = suitAndRanks.Value[2] - suitAndRanks.Value[0] - 2;
                    int highCount = suitAndRanks.Value.Count(rank => rank >= 11);
                    if (gapCount <= 2 && (gapCount - highCount <= 1 || suitAndRanks.Value[0] == 1))
                    {
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Values.Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 23. Unsuited JQK
                {
                    var jqkCounts = handInfo.RankCounts.GetRange(11, 3);
                    if (Enumerable.SequenceEqual(jqkCounts, new[] {1, 1, 1}))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(11, 13)).Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 24. Unsuited JQ or QK
                {
                    if (handInfo.RankCounts[12] == 1 && (handInfo.RankCounts[11] == 1 || handInfo.RankCounts[13] == 1))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(11, 13)).Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 25. Suited TJ (rule F: Unsuited JK beats suited TJ if the latter has a flush penalty card.)
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(10) && kvp.Value.ContainsKey(11));
                    if (validSuits.Any())
                    {
                        var suitAndPositions = validSuits.First();
                        if (suitAndPositions.Value.Count() >= 3 && handInfo.RankCounts[13] == 1)
                        {
                            return Core.RuleResult.Fail("F");
                        }
                        return Core.RuleResult.Pass((1 << suitAndPositions.Value[10]) + (1 << suitAndPositions.Value[11]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 26. Unsuited JK
                {
                    if (handInfo.RankCounts[11] == 1 && handInfo.RankCounts[13] == 1)
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == 11 || handInfo.Cards[i].Rank == 13).Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 27. Suited TQ (rule G: Unsuited QA beats suited TQ if the latter has a flush penalty card.)
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(10) && kvp.Value.ContainsKey(12));
                    if (validSuits.Any())
                    {
                        var suitAndPositions = validSuits.First();
                        if (suitAndPositions.Value.Count() >= 3 && handInfo.RankCounts[1] == 1)
                        {
                            return Core.RuleResult.Fail("G");
                        }
                        return Core.RuleResult.Pass((1 << suitAndPositions.Value[10]) + (1 << suitAndPositions.Value[12]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 28. Unsuited JA, QA, or KA
                {
                    if (handInfo.RankCounts[1] == 1 && handInfo.RankCounts.GetRange(11, 3).Contains(1))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank >= 11 || handInfo.Cards[i].Rank == 1).Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 29. Suited TQ (rule H: King only beats suited TK if the latter has both a 9 and a flush penalty card.)
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(10) && kvp.Value.ContainsKey(13));
                    if (validSuits.Any())
                    {
                        var suitAndPositions = validSuits.First();
                        if (suitAndPositions.Value.Count() >= 3 && handInfo.RankCounts[9] == 1)
                        {
                            return Core.RuleResult.Fail("H");
                        }
                        return Core.RuleResult.Pass((1 << suitAndPositions.Value[10]) + (1 << suitAndPositions.Value[13]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 30. Single high card
                {
                    var highCardIndices = CardIndices.Where(i => handInfo.Cards[i].Rank >= 11 || handInfo.Cards[i].Rank == 1);
                    if (highCardIndices.Any())
                    {
                        return Core.RuleResult.Pass(1 << highCardIndices.First());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 31. 3 to a straight flush, type 3
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var suitAndRanks = validSuits.First();
                    if (suitAndRanks.Value[2] - suitAndRanks.Value[0] == 4)
                    {
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Values.Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                }
            );

            public static Core.Strategy Strategy95 = new Core.Strategy(
                (handInfo) => // 1. Royal flush
                {
                    var flushes = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 5);
                    if (!flushes.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    if (Enumerable.SequenceEqual(flushes.First().Value, RoyalRanks))
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 2. Straight flush
                {
                    var flushes = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 5);
                    if (!flushes.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var ranks = flushes.First().Value;
                    if (ranks[4] - ranks[0] == 4)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 3. Four of a kind (rule A: You may either keep or discard the kicker.)
                {
                    var quads = handInfo.RankCounts.IndexOf(4);
                    if (quads > 0)
                    {
                        var cardIndicesToKeep = handInfo.CardPositionMatrix.Values.Select(array => array[quads]);
                        return Core.RuleResult.Pass("A", cardIndicesToKeep.Select(i => 1 << i).Sum(), 31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 4. 4 to a royal flush
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Intersect(RoyalRanks).Count() == 4);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var validSuit = validSuits.First().Key;
                    var ranks = handInfo.CardPositionMatrix[validSuit].Keys.Intersect(RoyalRanks);
                    return Core.RuleResult.Pass(ranks.Select(r => 1 << handInfo.CardPositionMatrix[validSuit][r]).Sum());
                },
                (handInfo) => // 5. Full house
                {
                    var trips = handInfo.RankCounts.IndexOf(3);
                    var pair = handInfo.RankCounts.IndexOf(2);
                    if (trips > 0 && pair > 0)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 6. Flush
                {
                    var flushes = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 5);
                    if (flushes.Any())
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 7. Straight
                {
                    var ranks = AllRanks.Where(r => handInfo.RankCounts[r] == 1).ToArray();
                    if (ranks.Length == 5 && (Enumerable.SequenceEqual(ranks, RoyalRanks) || ranks[4] - ranks[0] == 4))
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 8. Three of a kind
                {
                    var trips = handInfo.RankCounts.IndexOf(3);
                    if (trips > 0)
                    {
                        var cardIndicesToKeep = CardIndices.Where(i => handInfo.Cards[i].Rank == trips);
                        return Core.RuleResult.Pass(cardIndicesToKeep.Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 9. Two pair
                {
                    if (handInfo.RankCounts.Count(n => n == 2) == 2)
                    {
                        var indexToDiscard = CardIndices.Where(i => handInfo.RankCounts[handInfo.Cards[i].Rank] == 1).First();
                        return Core.RuleResult.Pass(31 - (1 << indexToDiscard));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 10. 4 to a straight flush
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Count() == 4);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var suitAndRanks = validSuits.First();
                    var suit = suitAndRanks.Key;
                    var ranks = suitAndRanks.Value;
                    if (ranks[3] - ranks[0] <= 4)
                    {
                        return Core.RuleResult.Pass(ranks.Select(r => 1 << handInfo.CardPositionMatrix[suit][r]).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 11. High pair
                {
                    var pairs = new[] {1, 11, 12, 13}.Where(r => handInfo.RankCounts[r] == 2);
                    if (pairs.Any())
                    {
                        var rank = pairs.First();
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == rank).Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 12. 3 to a royal flush
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Intersect(RoyalRanks).Count() == 3);
                    if (validSuits.Any())
                    {
                        var validSuit = validSuits.First().Key;
                        var ranks = handInfo.CardPositionMatrix[validSuit].Keys.Intersect(RoyalRanks);
                        return Core.RuleResult.Pass(ranks.Select(r => 1 << handInfo.CardPositionMatrix[validSuit][r]).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 13. 4 to a flush
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 4);
                    if (validSuits.Any())
                    {
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[validSuits.First().Key].Values.Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 14. Unsuited TJQK (rule B: If you also have a pair of 10s, it doesn't matter which 10 you discard.)
                {
                    var tjqkCounts = handInfo.RankCounts.GetRange(10, 4);
                    if (Enumerable.SequenceEqual(tjqkCounts, new[] {1, 1, 1, 1}))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(10, 13)).Select(i => 1 << i).Sum());
                    }
                    else if (Enumerable.SequenceEqual(tjqkCounts, new[] {2, 1, 1, 1}))
                    {
                        var indicesOfTens = CardIndices.Where(i => handInfo.Cards[i].Rank == 10).ToArray();
                        return Core.RuleResult.Pass("B", 31 - (1 << indicesOfTens[0]), 31 - (1 << indicesOfTens[1]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 15. Low pair
                {
                    var pairRank = handInfo.RankCounts.IndexOf(2);
                    if (pairRank > 0)
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == pairRank).Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                // Below here, we can assume that all ranks are different.
                (handInfo) => // 16. 4 to an open-ended straight
                {
                    int rankCount = handInfo.RankCounts.GetRange(1, 4).Sum();
                    for (int minRank = 2; minRank <= 9; minRank++)
                    {
                        rankCount += handInfo.RankCounts[minRank + 3] - handInfo.RankCounts[minRank - 1];
                        if (rankCount == 4)
                        {
                            return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(minRank, minRank + 3)).Select(i => 1 << i).Sum());
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 17. Suited 89J, 8TJ, 8JQ, 9TJ, 9TQ, or 9JQ
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }

                    var suitAndRanks = validSuits.First();

                    foreach (var ranksToFind in new[] {new[] {8, 9, 11}, new[] {8, 10, 11}, new[] {8, 11, 12}, new[] {9, 10, 11}, new[] {9, 10, 12}, new[] {9, 11, 12}})
                    {
                        if (Enumerable.SequenceEqual(suitAndRanks.Value, ranksToFind))
                        {
                            return Core.RuleResult.Pass(ranksToFind.Select(rank => 1 << handInfo.CardPositionMatrix[suitAndRanks.Key][rank]).Sum());
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 18. Unsuited JQKA
                {
                    var ranks = new[] {1, 11, 12, 13};
                    if (ranks.All(rank => handInfo.RankCounts[rank] == 1))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank >= 11 || handInfo.Cards[i].Rank == 1).Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 19. Suited 9JK or 9QK
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(9) && kvp.Value.ContainsKey(13) && (kvp.Value.ContainsKey(11) || kvp.Value.ContainsKey(12)));
                    if (validSuits.Any())
                    {
                        var validRanksLookup = validSuits.First().Value;
                        return Core.RuleResult.Pass(new[] {9, 11, 12, 13}.Where(rank => validRanksLookup.ContainsKey(rank)).Select(rank => 1 << validRanksLookup[rank]).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 20. Suited JQ, JK, or QK
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => Enumerable.Range(11, 3).Where(rank => kvp.Value.ContainsKey(rank)).Count() == 2);
                    if (validSuits.Any())
                    {
                        var validRanksLookup = validSuits.First().Value;
                        return Core.RuleResult.Pass(Enumerable.Range(11, 3).Where(rank => validRanksLookup.ContainsKey(rank)).Select(rank => 1 << validRanksLookup[rank]).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 21. Suited 456, 567, or 678
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }

                    var suitAndRanks = validSuits.First();

                    foreach (var ranksToFind in Enumerable.Range(4, 3).Select(lowRank => Enumerable.Range(lowRank, 3)))
                    {
                        if (Enumerable.SequenceEqual(suitAndRanks.Value, ranksToFind))
                        {
                            return Core.RuleResult.Pass(ranksToFind.Select(rank => 1 << handInfo.CardPositionMatrix[suitAndRanks.Key][rank]).Sum());
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 22. Suited JA
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(1) && kvp.Value.ContainsKey(11));
                    if (validSuits.Any())
                    {
                        var validRanksLookup = validSuits.First().Value;
                        return Core.RuleResult.Pass((1 << validRanksLookup[1]) + (1 << validRanksLookup[11]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 23. Suited 789 or 89T
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }

                    var suitAndRanks = validSuits.First();

                    foreach (var ranksToFind in new[] {7, 8}.Select(lowRank => Enumerable.Range(lowRank, 3)))
                    {
                        if (Enumerable.SequenceEqual(suitAndRanks.Value, ranksToFind))
                        {
                            return Core.RuleResult.Pass(ranksToFind.Select(rank => 1 << handInfo.CardPositionMatrix[suitAndRanks.Key][rank]).Sum());
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 24. Suited QA or KA
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(1) && (kvp.Value.ContainsKey(12) || kvp.Value.ContainsKey(13)));
                    if (validSuits.Any())
                    {
                        var validRanksLookup = validSuits.First().Value;
                        return Core.RuleResult.Pass(new[] {12, 13, 1}.Where(rank => validRanksLookup.ContainsKey(rank)).Select(rank => 1 << validRanksLookup[rank]).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 25. Suited 345
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }

                    var suitAndRanks = validSuits.First();
                    var ranksToFind = Enumerable.Range(3, 3);

                    if (Enumerable.SequenceEqual(suitAndRanks.Value, ranksToFind))
                    {
                        return Core.RuleResult.Pass(ranksToFind.Select(rank => 1 << handInfo.CardPositionMatrix[suitAndRanks.Key][rank]).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 26. Unsuited 9JQK, TJQA, TJKA, or TQKA
                {
                    var partialStraights = new[]
                    {
                        new[] {9, 11, 12, 13},
                        new[] {10, 11, 12, 1},
                        new[] {10, 11, 13, 1},
                        new[] {10, 12, 13, 1}
                    };
                    var matches = partialStraights.Where(partialStraight => partialStraight.Count(rank => handInfo.RankCounts[rank] == 1) == 4);
                    if (!matches.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var match = matches.First();
                    int rankToDiscard = handInfo.Cards.Select(card => card.Rank).Sum() - match.Sum();
                    return Core.RuleResult.Pass(31 - (1 << CardIndices.Where(i => handInfo.Cards[i].Rank == rankToDiscard).First()));
                },
                (handInfo) => // 27. Unsuited JQK
                {
                    var jqkCounts = handInfo.RankCounts.GetRange(11, 3);
                    if (Enumerable.SequenceEqual(jqkCounts, new[] {1, 1, 1}))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(11, 13)).Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 28. Unsuited JQ (rules C and D)
                {
                    if (handInfo.RankCounts[11] != 1 || handInfo.RankCounts[12] != 1)
                    {
                        return Core.RuleResult.Fail();
                    }

                    var potentialStraightFlushDraws = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (potentialStraightFlushDraws.Any())
                    {
                        var suitAndRanks = potentialStraightFlushDraws.First();
                        // Rule C: Suited 7TJ beats unsuited JQ if the fifth card is a 6 or lower.
                        if (Enumerable.SequenceEqual(suitAndRanks.Value, new[] {7, 10, 11}) && Enumerable.Range(2, 5).Select(rank => handInfo.RankCounts[rank]).Any(n => n == 1))
                        {
                            return Core.RuleResult.Fail("C");
                        }
                        // Rule D: Suited 78J or 79J beats unsuited JQ if the fifth card is an Ace.
                        if (suitAndRanks.Value[0] == 7 && suitAndRanks.Value[1].InRange(8, 9) && suitAndRanks.Value[2] == 11 && handInfo.RankCounts[1] == 1)
                        {
                            return Core.RuleResult.Fail("D");
                        }
                    }

                    return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(11, 12)).Sum(i => 1 << i));
                },
                (handInfo) => // 29. Suited A23, A24, A25, A34, A35, A45, 689, 78J[E], 79J[F], 7TJ[F], 89Q, 8TQ, or 9TK
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }

                    var suitAndRanks = validSuits.First();

                    // First, the cases without any exceptions
                    foreach (var ranksToFind in new[] {new[] {1, 2, 3}, new[] {1, 2, 4}, new[] {1, 2, 5}, new[] {1, 3, 4}, new[] {1, 3, 5}, new[] {1, 4, 5}, new[] {6, 8, 9}, new[] {8, 9, 12}, new[] {8, 10, 12}, new[] {9, 10, 13}})
                    {
                        if (Enumerable.SequenceEqual(suitAndRanks.Value, ranksToFind))
                        {
                            return Core.RuleResult.Pass(ranksToFind.Select(rank => 1 << handInfo.CardPositionMatrix[suitAndRanks.Key][rank]).Sum());
                        }
                    }

                    if (Enumerable.SequenceEqual(suitAndRanks.Value, new[] {7, 8, 11}))
                    {
                        // Rule E: Unsuited JK beats suited 78J if the fifth card is a 9 or a 10.
                        if (handInfo.RankCounts[13] == 1 && (handInfo.RankCounts[9] == 1 || handInfo.RankCounts[10] == 1))
                        {
                            return Core.RuleResult.Fail("E");
                        }

                        return Core.RuleResult.Pass(new[] {7, 8, 11}.Select(rank => 1 << handInfo.CardPositionMatrix[suitAndRanks.Key][rank]).Sum());
                    }

                    foreach (var ranksToFind in new[] {new[] {7, 9, 11}, new[] {7, 10, 11}})
                    {
                        if (Enumerable.SequenceEqual(suitAndRanks.Value, ranksToFind))
                        {
                            // Rule F: Unsuited JK beats suited 79J and 7TJ if the fifth card is an 8.
                            if (handInfo.RankCounts[13] == 1 && handInfo.RankCounts[8] == 1)
                            {
                                return Core.RuleResult.Fail("F");
                            }

                            return Core.RuleResult.Pass(ranksToFind.Select(rank => 1 << handInfo.CardPositionMatrix[suitAndRanks.Key][rank]).Sum());
                        }
                    }

                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 30. Unsuited JK (rules G and H)
                {
                    if (handInfo.RankCounts[11] != 1 || handInfo.RankCounts[13] != 1)
                    {
                        return Core.RuleResult.Fail();
                    }

                    // Rule G: Suited TJ beats unsuited JK if the former has no flush penalty cards and there are no 8s or 9s.
                    var jackSuit = handInfo.Cards.Where(card => card.Rank == 11).First().Suit;
                    if (Enumerable.SequenceEqual(handInfo.RanksPerSuit[jackSuit], new[] {10, 11}) && handInfo.RankCounts[8] == 0 && handInfo.RankCounts[9] == 0)
                    {
                        return Core.RuleResult.Fail("G");
                    }

                    var potentialStraightFlushDraws = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (potentialStraightFlushDraws.Any())
                    {
                        var suitAndRanks = potentialStraightFlushDraws.First();
                        
                        // Rule H: Suited 679 suited always beats unsuited JK and unsuited QK.
                        if (Enumerable.SequenceEqual(suitAndRanks.Value, new[] {6, 7, 9}))
                        {
                            return Core.RuleResult.Fail("H");
                        }
                    }

                    return Core.RuleResult.Pass(CardIndices.Where(i => new[] {11, 13}.Contains(handInfo.Cards[i].Rank)).Sum(i => 1 << i));
                },
                (handInfo) => // 31. Suited 78T or 79T (rule I: Jack only beats suited 78T and 79T if the fifth card is a 6.)
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }

                    var suitAndRanks = validSuits.First();

                    if (suitAndRanks.Value[0] == 7 && suitAndRanks.Value[2] == 10 && suitAndRanks.Value[1].InRange(8, 9))
                    {
                        if (handInfo.RankCounts[11] == 1 && handInfo.RankCounts[6] == 1)
                        {
                            return Core.RuleResult.Fail("I");
                        }
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Values.Sum(i => 1 << i));
                    }

                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 32. Suited TJ (rules J and K):
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(10) && kvp.Value.ContainsKey(11));
                    if (validSuits.Any())
                    {
                        var suitAndRanksLookup = validSuits.First();

                        // Rule J: Unsuited JA beats suited TJ if the latter has a flush penalty card of 6 or lower, and an 8 or a 9.
                        if (handInfo.RankCounts[1] == 1 && Enumerable.Range(2, 5).Any(rank => suitAndRanksLookup.Value.ContainsKey(rank)) && (handInfo.RankCounts[8] == 1 || handInfo.RankCounts[9] == 1))
                        {
                            return Core.RuleResult.Fail("J");
                        }

                        // Rule K: Suited 578 always bets suited TJ.
                        if (handInfo.RanksPerSuit.Any(kvp => Enumerable.SequenceEqual(kvp.Value, new[] {5, 7, 8})))
                        {
                            return Core.RuleResult.Fail("K");
                        }

                        return Core.RuleResult.Pass((1 << suitAndRanksLookup.Value[10]) + (1 << suitAndRanksLookup.Value[11]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 33. Unsuited QK (rule H: Suited 679 suited always beats unsuited JK and unsuited QK.)
                {
                    if (handInfo.RankCounts[12] != 1 || handInfo.RankCounts[13] != 1)
                    {
                        return Core.RuleResult.Fail();
                    }

                    var potentialStraightFlushDraws = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (potentialStraightFlushDraws.Any())
                    {
                        if (Enumerable.SequenceEqual(potentialStraightFlushDraws.First().Value, new[] {6, 7, 9}))
                        {
                            return Core.RuleResult.Fail("H");
                        }
                    }

                    return Core.RuleResult.Pass(CardIndices.Where(i => new[] {12, 13}.Contains(handInfo.Cards[i].Rank)).Sum(i => 1 << i));
                },
                (handInfo) => // 34. Suited 234[L][M], 235[L][M], 245[L][M], 346[L], 356[L], 457[L], 467[L], 568[L], 578[L], or 679
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }

                    var suitAndRanks = validSuits.First();

                    var rankArrays = new[]
                    {
                        new[] {2, 3, 4},
                        new[] {2, 3, 5},
                        new[] {2, 4, 5},
                        new[] {3, 4, 6},
                        new[] {3, 5, 6},
                        new[] {4, 5, 7},
                        new[] {4, 6, 7},
                        new[] {5, 6, 8},
                        new[] {5, 7, 8},
                        new[] {6, 7, 9}
                    };

                    for (int index = 0; index < rankArrays.Length; index++)
                    {
                        if (Enumerable.SequenceEqual(suitAndRanks.Value, rankArrays[index]))
                        {
                            // Rule M: Ace only beats this straight flush draw if the fifth card is a 6.
                            // It's easier to check for this first because we want to see if there's an Ace.
                            if (index < 3 && handInfo.RankCounts[1] == 1 && handInfo.RankCounts[6] == 1)
                            {
                                return Core.RuleResult.Fail("M");
                            }

                            // Rule L: Jack only beats this straight flush draw if there is a straight penalty card, unless that penalty card is a 9 or an Ace. However, if the hand doesn't contain a 2 and the penalty card has the same suit as the Jack, play the 3 to a straight flush instead.
                            int lowRank = rankArrays[index][0];
                            if (index != rankArrays.Length - 1 && handInfo.RankCounts[11] == 1 && handInfo.RankCounts[1] == 0 && (handInfo.RankCounts[lowRank - 1] == 1 || (handInfo.RankCounts[lowRank + 4] == 1 && lowRank != 5)))
                            {
                                if (handInfo.RankCounts[2] == 1 || !handInfo.RanksPerSuit.Values.Any(array => array.Length == 2))
                                {
                                    return Core.RuleResult.Fail("L");
                                }
                            }

                            return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Values.Sum(i => 1 << i));
                        }
                    }

                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 35. Unsuited JA, QA, or KA (rule N: Suited TQ beats unsuited QA if the former has no flush penalty cards.)
                {
                    if (handInfo.RankCounts[1] == 1 && handInfo.RankCounts.GetRange(11, 3).Contains(1))
                    {
                        if (handInfo.RankCounts[12] == 1)
                        {
                            var queenSuit = handInfo.Cards.Where(card => card.Rank == 12).First().Suit;
                            if (Enumerable.SequenceEqual(handInfo.RanksPerSuit[queenSuit], new[] {10, 12}) && handInfo.RankCounts[8] == 0 && handInfo.RankCounts[9] == 0)
                            {
                                return Core.RuleResult.Fail("N");
                            }
                        }
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == 1 || handInfo.Cards[i].Rank >= 11).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 36. Suited TQ or TK (rule O: King only beats suited TK if the latter has a flush penalty card.):
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(10) && kvp.Value.ContainsKey(12));
                    if (validSuits.Any())
                    {
                        var ranksLookup = validSuits.First().Value;

                        return Core.RuleResult.Pass((1 << ranksLookup[10]) + (1 << ranksLookup[12]));
                    }

                    validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(10) && kvp.Value.ContainsKey(13));
                    if (validSuits.Any())
                    {
                        var ranksLookup = validSuits.First().Value;

                        if (ranksLookup.Count() > 2)
                        {
                            return Core.RuleResult.Fail("O");
                        }

                        return Core.RuleResult.Pass((1 << ranksLookup[10]) + (1 << ranksLookup[13]));
                    }

                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 37. Single high card
                {
                    var highCardPositions = CardIndices.Where(i => handInfo.Cards[i].Rank == 1 || handInfo.Cards[i].Rank >= 11);
                    if (highCardPositions.Any())
                    {
                        int cardIndex = highCardPositions.First();
                        return Core.RuleResult.Pass(1 << cardIndex);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 38. Any other 3 to a straight flush not listed
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }

                    var suitAndRanks = validSuits.First();
                    var ranks = suitAndRanks.Value;

                    if (ranks[0].InRange(2, 6) && ranks[2] - ranks[0] == 4)
                    {
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Values.Sum(i => 1 << i));
                    }

                    return Core.RuleResult.Fail();
                }
            );

            public static Core.Strategy Strategy86 = new Core.Strategy(
                (handInfo) => // 1. Royal flush
                {
                    var flushes = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 5);
                    if (!flushes.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    if (Enumerable.SequenceEqual(flushes.First().Value, RoyalRanks))
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 2. Straight flush
                {
                    var flushes = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 5);
                    if (!flushes.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var ranks = flushes.First().Value;
                    if (ranks[4] - ranks[0] == 4)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 3. Four of a kind (rule A: You may either keep or discard the kicker.)
                {
                    var quads = handInfo.RankCounts.IndexOf(4);
                    if (quads > 0)
                    {
                        var cardIndicesToKeep = handInfo.CardPositionMatrix.Values.Select(array => array[quads]);
                        return Core.RuleResult.Pass("A", cardIndicesToKeep.Select(i => 1 << i).Sum(), 31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 4. 4 to a royal flush
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Intersect(RoyalRanks).Count() == 4);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var validSuit = validSuits.First().Key;
                    var ranks = handInfo.CardPositionMatrix[validSuit].Keys.Intersect(RoyalRanks);
                    return Core.RuleResult.Pass(ranks.Select(r => 1 << handInfo.CardPositionMatrix[validSuit][r]).Sum());
                },
                (handInfo) => // 5. Full house
                {
                    var trips = handInfo.RankCounts.IndexOf(3);
                    var pair = handInfo.RankCounts.IndexOf(2);
                    if (trips > 0 && pair > 0)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 6. Flush
                {
                    var flushes = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 5);
                    if (flushes.Any())
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 7. Straight
                {
                    var ranks = AllRanks.Where(r => handInfo.RankCounts[r] == 1).ToArray();
                    if (ranks.Length == 5 && (Enumerable.SequenceEqual(ranks, RoyalRanks) || ranks[4] - ranks[0] == 4))
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 8. Three of a kind
                {
                    var trips = handInfo.RankCounts.IndexOf(3);
                    if (trips > 0)
                    {
                        var cardIndicesToKeep = CardIndices.Where(i => handInfo.Cards[i].Rank == trips);
                        return Core.RuleResult.Pass(cardIndicesToKeep.Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 9. 4 to a straight flush
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Count() == 4);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var suitAndRanks = validSuits.First();
                    var suit = suitAndRanks.Key;
                    var ranks = suitAndRanks.Value;
                    if (ranks[3] - ranks[0] <= 4)
                    {
                        return Core.RuleResult.Pass(ranks.Select(r => 1 << handInfo.CardPositionMatrix[suit][r]).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 10. Two pair
                {
                    if (handInfo.RankCounts.Count(n => n == 2) == 2)
                    {
                        var indexToDiscard = CardIndices.Where(i => handInfo.RankCounts[handInfo.Cards[i].Rank] == 1).First();
                        return Core.RuleResult.Pass(31 - (1 << indexToDiscard));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 11. High pair
                {
                    var pairs = new[] {1, 11, 12, 13}.Where(r => handInfo.RankCounts[r] == 2);
                    if (pairs.Any())
                    {
                        var rank = pairs.First();
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == rank).Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 12. 3 to a royal flush (rule B: 4 to a flush beats 3 to a royal if the latter contains a 10 and an Ace, and the unsuited card is either a 10 or a straight penalty card.)
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Intersect(RoyalRanks).Count() == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var validSuit = validSuits.First().Key;
                    if (handInfo.RanksPerSuit[validSuit].Length == 4 && handInfo.RanksPerSuit[validSuit].Except(new[] {1, 10}).Count() == 2)
                    {
                        // We have four cards with this suit, two of which are an Ace and a 10
                        var unsuitedCard = handInfo.Cards.Where(card => card.Suit != validSuit).First();
                        if (unsuitedCard.Rank >= 10 || unsuitedCard.Rank == 1)
                        {
                            return Core.RuleResult.Fail("B");
                        }
                    }
                    var ranks = handInfo.CardPositionMatrix[validSuit].Keys.Intersect(RoyalRanks);
                    return Core.RuleResult.Pass(ranks.Select(r => 1 << handInfo.CardPositionMatrix[validSuit][r]).Sum());
                },
                (handInfo) => // 13. 4 to a flush
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 4);
                    if (validSuits.Any())
                    {
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[validSuits.First().Key].Values.Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 14. Unsuited TJQK (rule C: If you also have a pair of 10s, it doesn't matter which 10 you discard.)
                {
                    var tjqkCounts = handInfo.RankCounts.GetRange(10, 4);
                    if (Enumerable.SequenceEqual(tjqkCounts, new[] {1, 1, 1, 1}))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(10, 13)).Select(i => 1 << i).Sum());
                    }
                    else if (Enumerable.SequenceEqual(tjqkCounts, new[] {2, 1, 1, 1}))
                    {
                        var indicesOfTens = CardIndices.Where(i => handInfo.Cards[i].Rank == 10).ToArray();
                        return Core.RuleResult.Pass("C", 31 - (1 << indicesOfTens[0]), 31 - (1 << indicesOfTens[1]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 15. Low pair
                {
                    var pairRank = handInfo.RankCounts.IndexOf(2);
                    if (pairRank > 0)
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == pairRank).Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                // Below here, we can assume that all ranks are different.
                (handInfo) => // 16. 4 to an open-ended straight
                {
                    int rankCount = handInfo.RankCounts.GetRange(1, 4).Sum();
                    for (int minRank = 2; minRank <= 9; minRank++)
                    {
                        rankCount += handInfo.RankCounts[minRank + 3] - handInfo.RankCounts[minRank - 1];
                        if (rankCount == 4)
                        {
                            return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(minRank, minRank + 3)).Select(i => 1 << i).Sum());
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 17. 3 to a straight flush, type 1 (the number of high cards equals or exceeds the number of gaps, except Ace low or 2-3-4)
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var suitAndRanks = validSuits.First();
                    int gapCount = suitAndRanks.Value[2] - suitAndRanks.Value[0] - 2;
                    int highCount = suitAndRanks.Value.Count(rank => rank >= 11);
                    if (highCount >= gapCount && suitAndRanks.Value[0] >= 3)
                    {
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Values.Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 18. Suited JQ (rule D: Unsuited JQKA beats suited JQ if the latter has an 8, a 9, or a flush penalty card.)
                {
                    var hasJQ = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.Keys.Where(r => r == 11 || r == 12).Count() == 2);
                    if (!hasJQ.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var jqSuit = hasJQ.First();
                    if (handInfo.RankCounts[13] == 1 && handInfo.RankCounts[1] == 1)
                    {
                        // We also have an unsuited JQKA. Does the exception apply?
                        // We know that the King and Ace aren't of the same suit as the suited JQ. Otherwise we'd have 3 to a royal.
                        var lastCard = handInfo.Cards.Where(card => card.Rank.InRange(2, 10)).First();
                        if (lastCard.Rank == 8 || lastCard.Rank == 9 || lastCard.Suit == jqSuit.Key)
                        {
                            return Core.RuleResult.Fail("D");
                        }
                    }
                    return Core.RuleResult.Pass((1 << handInfo.CardPositionMatrix[jqSuit.Key][11]) + (1 << handInfo.CardPositionMatrix[jqSuit.Key][12]));
                },
                (handInfo) => // 19. Unsuited JQKA
                {
                    var ranks = new[] {1, 11, 12, 13};
                    if (ranks.All(rank => handInfo.RankCounts[rank] == 1))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank >= 11 || handInfo.Cards[i].Rank == 1).Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 20. Suited JK, QK, JA, QA, or KA
                {
                    var ranks = new[] {1, 11, 12, 13};
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.Keys.Intersect(ranks).Count() == 2);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    return Core.RuleResult.Pass(validSuits.First().Value.Where(kvp => kvp.Key >= 11 || kvp.Key == 1).Select(kvp => 1 << kvp.Value).Sum());
                },
                (handInfo) => // 21. 9JQK, TJQA, TJKA, or TQKA (rule E: 3 to a straight flush beats 4 to a straight if the former doesn’t have a straight penalty card.)
                {
                    var partialStraights = new[]
                    {
                        new[] {9, 11, 12, 13},
                        new[] {10, 11, 12, 1},
                        new[] {10, 11, 13, 1},
                        new[] {10, 12, 13, 1}
                    };
                    var matches = partialStraights.Where(partialStraight => partialStraight.Count(rank => handInfo.RankCounts[rank] == 1) == 4);
                    if (!matches.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var match = matches.First();
                    // Look for straight flush exception
                    var validPartialStraightFlushes = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3).Select(kvp => kvp.Value);
                    if (validPartialStraightFlushes.Any())
                    {
                        if (Enumerable.SequenceEqual(validPartialStraightFlushes.First(), new[] {match[1] - 4, match[0], match[1]}))
                        {
                            return Core.RuleResult.Fail("E");
                        }
                    }
                    // We're good
                    int rankToDiscard = handInfo.Cards.Select(card => card.Rank).Sum() - match.Sum();
                    return Core.RuleResult.Pass(31 - (1 << CardIndices.Where(i => handInfo.Cards[i].Rank == rankToDiscard).First()));
                },
                (handInfo) => // 22. 3 to a straight flush, type 2 (one gap with no high cards, two gaps with one high card, Ace low, or 2-3-4)
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var suitAndRanks = validSuits.First();
                    int gapCount = suitAndRanks.Value[2] - suitAndRanks.Value[0] - 2;
                    int highCount = suitAndRanks.Value.Count(rank => rank >= 11);
                    if (gapCount <= 2 && (gapCount - highCount <= 1 || suitAndRanks.Value[0] == 1))
                    {
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Values.Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 23. Unsuited JQK
                {
                    var jqkCounts = handInfo.RankCounts.GetRange(11, 3);
                    if (Enumerable.SequenceEqual(jqkCounts, new[] {1, 1, 1}))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(11, 13)).Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 24. Unsuited JQ or QK
                {
                    if (handInfo.RankCounts[12] == 1 && (handInfo.RankCounts[11] == 1 || handInfo.RankCounts[13] == 1))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(11, 13)).Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 25. Suited TJ (rule F: Unsuited JK beats suited TJ if the latter has a flush penalty card.)
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(10) && kvp.Value.ContainsKey(11));
                    if (validSuits.Any())
                    {
                        var suitAndPositions = validSuits.First();
                        if (suitAndPositions.Value.Count() >= 3 && handInfo.RankCounts[13] == 1)
                        {
                            return Core.RuleResult.Fail("F");
                        }
                        return Core.RuleResult.Pass((1 << suitAndPositions.Value[10]) + (1 << suitAndPositions.Value[11]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 26. Unsuited JK
                {
                    if (handInfo.RankCounts[11] == 1 && handInfo.RankCounts[13] == 1)
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == 11 || handInfo.Cards[i].Rank == 13).Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 27. Suited TQ (rule G: Unsuited QA beats suited TQ if the latter has a flush penalty card.)
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(10) && kvp.Value.ContainsKey(12));
                    if (validSuits.Any())
                    {
                        var suitAndPositions = validSuits.First();
                        if (suitAndPositions.Value.Count() >= 3 && handInfo.RankCounts[1] == 1)
                        {
                            return Core.RuleResult.Fail("G");
                        }
                        return Core.RuleResult.Pass((1 << suitAndPositions.Value[10]) + (1 << suitAndPositions.Value[12]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 28. Unsuited JA, QA, or KA
                {
                    if (handInfo.RankCounts[1] == 1 && handInfo.RankCounts.GetRange(11, 3).Contains(1))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank >= 11 || handInfo.Cards[i].Rank == 1).Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 29. Suited TQ (rule H: King only beats suited TK if the latter has both a 9 and a flush penalty card.)
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(10) && kvp.Value.ContainsKey(13));
                    if (validSuits.Any())
                    {
                        var suitAndPositions = validSuits.First();
                        if (suitAndPositions.Value.Count() >= 3 && handInfo.RankCounts[9] == 1)
                        {
                            return Core.RuleResult.Fail("H");
                        }
                        return Core.RuleResult.Pass((1 << suitAndPositions.Value[10]) + (1 << suitAndPositions.Value[13]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 30. Single high card
                {
                    var highCardIndices = CardIndices.Where(i => handInfo.Cards[i].Rank >= 11 || handInfo.Cards[i].Rank == 1);
                    if (highCardIndices.Any())
                    {
                        return Core.RuleResult.Pass(1 << highCardIndices.First());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 31. 3 to a straight flush, type 3
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var suitAndRanks = validSuits.First();
                    if (suitAndRanks.Value[2] - suitAndRanks.Value[0] == 4)
                    {
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Values.Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                }
            );

            public static Core.Strategy Strategy85 = new Core.Strategy(
                (handInfo) => // 1. Royal flush
                {
                    var flushes = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 5);
                    if (!flushes.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    if (Enumerable.SequenceEqual(flushes.First().Value, RoyalRanks))
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 2. Straight flush
                {
                    var flushes = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 5);
                    if (!flushes.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var ranks = flushes.First().Value;
                    if (ranks[4] - ranks[0] == 4)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 3. Four of a kind (rule A: You may either keep or discard the kicker.)
                {
                    var quads = handInfo.RankCounts.IndexOf(4);
                    if (quads > 0)
                    {
                        var cardIndicesToKeep = handInfo.CardPositionMatrix.Values.Select(array => array[quads]);
                        return Core.RuleResult.Pass("A", cardIndicesToKeep.Select(i => 1 << i).Sum(), 31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 4. 4 to a royal flush
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Intersect(RoyalRanks).Count() == 4);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var validSuit = validSuits.First().Key;
                    var ranks = handInfo.CardPositionMatrix[validSuit].Keys.Intersect(RoyalRanks);
                    return Core.RuleResult.Pass(ranks.Select(r => 1 << handInfo.CardPositionMatrix[validSuit][r]).Sum());
                },
                (handInfo) => // 5. Full house
                {
                    var trips = handInfo.RankCounts.IndexOf(3);
                    var pair = handInfo.RankCounts.IndexOf(2);
                    if (trips > 0 && pair > 0)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 6. Flush
                {
                    var flushes = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 5);
                    if (flushes.Any())
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 7. Straight
                {
                    var ranks = AllRanks.Where(r => handInfo.RankCounts[r] == 1).ToArray();
                    if (ranks.Length == 5 && (Enumerable.SequenceEqual(ranks, RoyalRanks) || ranks[4] - ranks[0] == 4))
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 8. Three of a kind
                {
                    var trips = handInfo.RankCounts.IndexOf(3);
                    if (trips > 0)
                    {
                        var cardIndicesToKeep = CardIndices.Where(i => handInfo.Cards[i].Rank == trips);
                        return Core.RuleResult.Pass(cardIndicesToKeep.Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 9. Two pair
                {
                    if (handInfo.RankCounts.Count(n => n == 2) == 2)
                    {
                        var indexToDiscard = CardIndices.Where(i => handInfo.RankCounts[handInfo.Cards[i].Rank] == 1).First();
                        return Core.RuleResult.Pass(31 - (1 << indexToDiscard));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 10. 4 to a straight flush
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Count() == 4);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var suitAndRanks = validSuits.First();
                    var suit = suitAndRanks.Key;
                    var ranks = suitAndRanks.Value;
                    if (ranks[3] - ranks[0] <= 4)
                    {
                        return Core.RuleResult.Pass(ranks.Select(r => 1 << handInfo.CardPositionMatrix[suit][r]).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 11. High pair
                {
                    var pairs = new[] {1, 11, 12, 13}.Where(r => handInfo.RankCounts[r] == 2);
                    if (pairs.Any())
                    {
                        var rank = pairs.First();
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == rank).Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 12. 3 to a royal flush
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Intersect(RoyalRanks).Count() == 3);
                    if (validSuits.Any())
                    {
                        var validSuit = validSuits.First().Key;
                        var ranks = handInfo.CardPositionMatrix[validSuit].Keys.Intersect(RoyalRanks);
                        return Core.RuleResult.Pass(ranks.Select(r => 1 << handInfo.CardPositionMatrix[validSuit][r]).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 13. 4 to a flush
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 4);
                    if (validSuits.Any())
                    {
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[validSuits.First().Key].Values.Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 14. Unsuited TJQK (rule B: If you also have a pair of 10s, it doesn't matter which 10 you discard.)
                {
                    var tjqkCounts = handInfo.RankCounts.GetRange(10, 4);
                    if (Enumerable.SequenceEqual(tjqkCounts, new[] {1, 1, 1, 1}))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(10, 13)).Select(i => 1 << i).Sum());
                    }
                    else if (Enumerable.SequenceEqual(tjqkCounts, new[] {2, 1, 1, 1}))
                    {
                        var indicesOfTens = CardIndices.Where(i => handInfo.Cards[i].Rank == 10).ToArray();
                        return Core.RuleResult.Pass("B", 31 - (1 << indicesOfTens[0]), 31 - (1 << indicesOfTens[1]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 15. Low pair
                {
                    var pairRank = handInfo.RankCounts.IndexOf(2);
                    if (pairRank > 0)
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == pairRank).Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                // Below here, we can assume that all ranks are different.
                (handInfo) => // 16. 4 to an open-ended straight
                {
                    int rankCount = handInfo.RankCounts.GetRange(1, 4).Sum();
                    for (int minRank = 2; minRank <= 9; minRank++)
                    {
                        rankCount += handInfo.RankCounts[minRank + 3] - handInfo.RankCounts[minRank - 1];
                        if (rankCount == 4)
                        {
                            return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(minRank, minRank + 3)).Select(i => 1 << i).Sum());
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 17. Suited 89J, 8TJ, 8JQ, 9TJ, 9TQ, or 9JQ
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }

                    var suitAndRanks = validSuits.First();

                    foreach (var ranksToFind in new[] {new[] {8, 9, 11}, new[] {8, 10, 11}, new[] {8, 11, 12}, new[] {9, 10, 11}, new[] {9, 10, 12}, new[] {9, 11, 12}})
                    {
                        if (Enumerable.SequenceEqual(suitAndRanks.Value, ranksToFind))
                        {
                            return Core.RuleResult.Pass(ranksToFind.Select(rank => 1 << handInfo.CardPositionMatrix[suitAndRanks.Key][rank]).Sum());
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 18. Unsuited JQKA
                {
                    var ranks = new[] {1, 11, 12, 13};
                    if (ranks.All(rank => handInfo.RankCounts[rank] == 1))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank >= 11 || handInfo.Cards[i].Rank == 1).Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 19. Suited 9JK or 9QK
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(9) && kvp.Value.ContainsKey(13) && (kvp.Value.ContainsKey(11) || kvp.Value.ContainsKey(12)));
                    if (validSuits.Any())
                    {
                        var validRanksLookup = validSuits.First().Value;
                        return Core.RuleResult.Pass(new[] {9, 11, 12, 13}.Where(rank => validRanksLookup.ContainsKey(rank)).Select(rank => 1 << validRanksLookup[rank]).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 20. Suited JQ, JK, or QK
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => Enumerable.Range(11, 3).Where(rank => kvp.Value.ContainsKey(rank)).Count() == 2);
                    if (validSuits.Any())
                    {
                        var validRanksLookup = validSuits.First().Value;
                        return Core.RuleResult.Pass(Enumerable.Range(11, 3).Where(rank => validRanksLookup.ContainsKey(rank)).Select(rank => 1 << validRanksLookup[rank]).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 21. Suited 456, 567, or 678
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }

                    var suitAndRanks = validSuits.First();

                    foreach (var ranksToFind in Enumerable.Range(4, 3).Select(lowRank => Enumerable.Range(lowRank, 3)))
                    {
                        if (Enumerable.SequenceEqual(suitAndRanks.Value, ranksToFind))
                        {
                            return Core.RuleResult.Pass(ranksToFind.Select(rank => 1 << handInfo.CardPositionMatrix[suitAndRanks.Key][rank]).Sum());
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 22. Suited JA
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(1) && kvp.Value.ContainsKey(11));
                    if (validSuits.Any())
                    {
                        var validRanksLookup = validSuits.First().Value;
                        return Core.RuleResult.Pass((1 << validRanksLookup[1]) + (1 << validRanksLookup[11]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 23. Suited 789 or 89T
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }

                    var suitAndRanks = validSuits.First();

                    foreach (var ranksToFind in new[] {7, 8}.Select(lowRank => Enumerable.Range(lowRank, 3)))
                    {
                        if (Enumerable.SequenceEqual(suitAndRanks.Value, ranksToFind))
                        {
                            return Core.RuleResult.Pass(ranksToFind.Select(rank => 1 << handInfo.CardPositionMatrix[suitAndRanks.Key][rank]).Sum());
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 24. Suited QA or KA
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(1) && (kvp.Value.ContainsKey(12) || kvp.Value.ContainsKey(13)));
                    if (validSuits.Any())
                    {
                        var validRanksLookup = validSuits.First().Value;
                        return Core.RuleResult.Pass(new[] {12, 13, 1}.Where(rank => validRanksLookup.ContainsKey(rank)).Select(rank => 1 << validRanksLookup[rank]).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 25. Suited 345
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }

                    var suitAndRanks = validSuits.First();
                    var ranksToFind = Enumerable.Range(3, 3);

                    if (Enumerable.SequenceEqual(suitAndRanks.Value, ranksToFind))
                    {
                        return Core.RuleResult.Pass(ranksToFind.Select(rank => 1 << handInfo.CardPositionMatrix[suitAndRanks.Key][rank]).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 26. Unsuited 9JQK, TJQA, TJKA, or TQKA
                {
                    var partialStraights = new[]
                    {
                        new[] {9, 11, 12, 13},
                        new[] {10, 11, 12, 1},
                        new[] {10, 11, 13, 1},
                        new[] {10, 12, 13, 1}
                    };
                    var matches = partialStraights.Where(partialStraight => partialStraight.Count(rank => handInfo.RankCounts[rank] == 1) == 4);
                    if (!matches.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var match = matches.First();
                    int rankToDiscard = handInfo.Cards.Select(card => card.Rank).Sum() - match.Sum();
                    return Core.RuleResult.Pass(31 - (1 << CardIndices.Where(i => handInfo.Cards[i].Rank == rankToDiscard).First()));
                },
                (handInfo) => // 27. Unsuited JQK
                {
                    var jqkCounts = handInfo.RankCounts.GetRange(11, 3);
                    if (Enumerable.SequenceEqual(jqkCounts, new[] {1, 1, 1}))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(11, 13)).Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 28. Unsuited JQ (rules C and D)
                {
                    if (handInfo.RankCounts[11] != 1 || handInfo.RankCounts[12] != 1)
                    {
                        return Core.RuleResult.Fail();
                    }

                    var potentialStraightFlushDraws = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (potentialStraightFlushDraws.Any())
                    {
                        var suitAndRanks = potentialStraightFlushDraws.First();
                        // Rule C: Suited 7TJ beats unsuited JQ if the fifth card is a 6 or lower.
                        if (Enumerable.SequenceEqual(suitAndRanks.Value, new[] {7, 10, 11}) && Enumerable.Range(2, 5).Select(rank => handInfo.RankCounts[rank]).Any(n => n == 1))
                        {
                            return Core.RuleResult.Fail("C");
                        }
                        // Rule D: Suited 78J or 79J beats unsuited JQ if the fifth card is an Ace.
                        if (suitAndRanks.Value[0] == 7 && suitAndRanks.Value[1].InRange(8, 9) && suitAndRanks.Value[2] == 11 && handInfo.RankCounts[1] == 1)
                        {
                            return Core.RuleResult.Fail("D");
                        }
                    }

                    return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(11, 12)).Sum(i => 1 << i));
                },
                (handInfo) => // 29. Suited A23, A24, A25, A34, A35, A45, 568[G], 578[G], 689, 78J[E], 79J[F], 7TJ[F], 89Q, 8TQ, or 9TK
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }

                    var suitAndRanks = validSuits.First();

                    // First, the cases without any exceptions
                    foreach (var ranksToFind in new[] {new[] {1, 2, 3}, new[] {1, 2, 4}, new[] {1, 2, 5}, new[] {1, 3, 4}, new[] {1, 3, 5}, new[] {1, 4, 5}, new[] {5, 7, 8}, new[] {6, 8, 9}, new[] {8, 9, 12}, new[] {8, 10, 12}, new[] {9, 10, 13}})
                    {
                        if (Enumerable.SequenceEqual(suitAndRanks.Value, ranksToFind))
                        {
                            return Core.RuleResult.Pass(ranksToFind.Select(rank => 1 << handInfo.CardPositionMatrix[suitAndRanks.Key][rank]).Sum());
                        }
                    }

                    if (Enumerable.SequenceEqual(suitAndRanks.Value, new[] {5, 6, 8}))
                    {
                        // Rule G: Jack only beats this straight flush draw if there is a straight penalty card of 7 or lower that doesn't match the suit of the Jack.
                        if (handInfo.RankCounts[4] == 1 && handInfo.RankCounts[11] == 1)
                        {
                            var jackFourSuits = handInfo.Cards.Where(card => card.Rank == 11 || card.Rank == 4).Select(card => card.Suit);
                            if (jackFourSuits.Distinct().Count() == 2)
                            {
                                return Core.RuleResult.Fail("G");
                            }
                        }

                        return Core.RuleResult.Pass(new[] {5, 6, 8}.Select(rank => 1 << handInfo.CardPositionMatrix[suitAndRanks.Key][rank]).Sum());
                    }

                    if (Enumerable.SequenceEqual(suitAndRanks.Value, new[] {7, 8, 11}))
                    {
                        // Rule E: Unsuited JK beats suited 78J if the fifth card is a 9 or a 10.
                        if (handInfo.RankCounts[13] == 1 && (handInfo.RankCounts[9] == 1 || handInfo.RankCounts[10] == 1))
                        {
                            return Core.RuleResult.Fail("E");
                        }

                        return Core.RuleResult.Pass(new[] {7, 8, 11}.Select(rank => 1 << handInfo.CardPositionMatrix[suitAndRanks.Key][rank]).Sum());
                    }

                    foreach (var ranksToFind in new[] {new[] {7, 9, 11}, new[] {7, 10, 11}})
                    {
                        if (Enumerable.SequenceEqual(suitAndRanks.Value, ranksToFind))
                        {
                            // Rule F: Unsuited JK beats suited 79J and 7TJ if the fifth card is an 8.
                            if (handInfo.RankCounts[13] == 1 && handInfo.RankCounts[8] == 1)
                            {
                                return Core.RuleResult.Fail("F");
                            }

                            return Core.RuleResult.Pass(ranksToFind.Select(rank => 1 << handInfo.CardPositionMatrix[suitAndRanks.Key][rank]).Sum());
                        }
                    }

                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 30. Suited TJ (rules H and I)
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(10) && kvp.Value.ContainsKey(11));
                    if (validSuits.Any())
                    {
                        var suitAndRanksLookup = validSuits.First();

                        // Rule H: Unsuited JK beats suited TJ if the latter has an 8, 9, or flush penalty card.
                        if (handInfo.RankCounts[13] == 1 && (handInfo.RankCounts[8] == 1 || handInfo.RankCounts[9] == 1 || handInfo.RanksPerSuit[suitAndRanksLookup.Key].Length >= 3))
                        {
                            return Core.RuleResult.Fail("H");
                        }

                        // Rule I: Unsuited JA beats suited TJ if the latter has a flush penalty card of 6 or lower, and an 8 or a 9.
                        if (handInfo.RankCounts[1] == 1 && suitAndRanksLookup.Value.Keys.Min() <= 6 && (handInfo.RankCounts[8] == 1 || handInfo.RankCounts[9] == 1))
                        {
                            return Core.RuleResult.Fail("I");
                        }

                        return Core.RuleResult.Pass((1 << suitAndRanksLookup.Value[10]) + (1 << suitAndRanksLookup.Value[11]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 31. Suited 234, 235, 245, 346, 356, 457, 467, 679, or 689 (rules G and J)
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }

                    var suitAndRanks = validSuits.First();

                    var rankArrays = new[]
                    {
                        new[] {2, 3, 4},
                        new[] {2, 3, 5},
                        new[] {2, 4, 5},
                        new[] {3, 4, 6},
                        new[] {3, 5, 6},
                        new[] {4, 5, 7},
                        new[] {4, 6, 7},
                        new[] {6, 7, 9},
                        new[] {6, 8, 9}
                    };

                    for (int index = 0; index < rankArrays.Length; index++)
                    {
                        if (Enumerable.SequenceEqual(suitAndRanks.Value, rankArrays[index]))
                        {
                            // Rule J: Ace only beats this straight flush draw if the fifth card is a 6 that doesn't match the suit of the Ace.
                            // It's easier to check for this first because we want to see if there's an Ace.
                            if (index < 3 && handInfo.RankCounts[1] == 1 && handInfo.RankCounts[6] == 1)
                            {
                                var aceAndSix = handInfo.Cards.Where(card => card.Rank == 1 || card.Rank == 6);
                                if (aceAndSix.Select(card => card.Suit).Distinct().Count() == 2)
                                {
                                    return Core.RuleResult.Fail("J");
                                }
                            }

                            // Rule G: Jack only beats this straight flush draw if there is a straight penalty card of 7 or lower that doesn't match the suit of the Jack.
                            int lowRank = rankArrays[index][0];
                            if (handInfo.RankCounts[11] == 1 && ((lowRank != 2 && handInfo.RankCounts[lowRank - 1] == 1) || (lowRank <= 3 && handInfo.RankCounts[lowRank + 4] == 1)) && rankArrays[index][1] != 7)
                            {
                                var jackSuit = handInfo.Cards.Where(card => card.Rank == 11).First().Suit;
                                if (handInfo.RanksPerSuit[jackSuit].Length == 1)
                                {
                                    return Core.RuleResult.Fail("G");
                                }
                            }

                            return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Values.Sum(i => 1 << i));
                        }
                    }

                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 32. Unsuited JK
                {
                    if (handInfo.RankCounts[11] == 1 && handInfo.RankCounts[13] == 1)
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => new[] {11, 13}.Contains(handInfo.Cards[i].Rank)).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 33. Suited 78T or 79T (rule K: Jack only beats suited 78T and 79T if the fifth card is a 6.)
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }

                    var suitAndRanks = validSuits.First();
                    var ranks = suitAndRanks.Value;

                    if (ranks[0] == 7 && ranks[2] == 10)
                    {
                        if (handInfo.RankCounts[11] == 1 && handInfo.RankCounts[6] == 1)
                        {
                            return Core.RuleResult.Fail("K");
                        }
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Values.Sum(i => 1 << i));
                    }

                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 34. Unsuited JA or QK
                {
                    if (handInfo.RankCounts[11] == 1 && handInfo.RankCounts[1] == 1)
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => new[] {11, 1}.Contains(handInfo.Cards[i].Rank)).Sum(i => 1 << i));
                    }
                    if (handInfo.RankCounts[12] == 1 && handInfo.RankCounts[13] == 1)
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => new[] {12, 13}.Contains(handInfo.Cards[i].Rank)).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 35. Unsuited QA or KA (rule L: Suited TQ beats unsuited QA if the former doesn't have a flush penalty card and there isn't an 8 or 9.)
                {
                    if (handInfo.RankCounts[1] == 1 && (handInfo.RankCounts[12] == 1 || handInfo.RankCounts[13] == 1))
                    {
                        if (handInfo.RankCounts[12] == 1)
                        {
                            var queenSuit = handInfo.Cards.Where(card => card.Rank == 12).First().Suit;
                            if (Enumerable.SequenceEqual(handInfo.RanksPerSuit[queenSuit], new[] {10, 12}) && handInfo.RankCounts[8] != 1 && handInfo.RankCounts[9] != 1)
                            {
                                return Core.RuleResult.Fail("L");
                            }
                        }
                        return Core.RuleResult.Pass(CardIndices.Where(i => new[] {12, 13, 1}.Contains(handInfo.Cards[i].Rank)).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 36. Suited TQ or TK (rule M: King only beats suited TK if the latter has a flush penalty card.)
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(10) && (kvp.Value.ContainsKey(12) || kvp.Value.ContainsKey(13)));
                    if (validSuits.Any())
                    {
                        var suitAndRanksLookup = validSuits.First();

                        if (suitAndRanksLookup.Value.ContainsKey(13) && suitAndRanksLookup.Value.Count() == 3)
                        {
                            return Core.RuleResult.Fail("M");
                        }

                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Suit == suitAndRanksLookup.Key && handInfo.Cards[i].Rank >= 10).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 37. Single high card
                {
                    var highCardPositions = CardIndices.Where(i => handInfo.Cards[i].Rank == 1 || handInfo.Cards[i].Rank >= 11);
                    if (highCardPositions.Any())
                    {
                        int cardIndex = highCardPositions.First();
                        var highCard = handInfo.Cards[cardIndex];
                        if (highCard.Rank == 13 && Enumerable.SequenceEqual(handInfo.RanksPerSuit[highCard.Suit], new[] {10, 13}))
                        {
                            return Core.RuleResult.Pass("O", handInfo.CardPositionMatrix[highCard.Suit].Values.Sum(i => 1 << i));
                        }
                        else
                        {
                            return Core.RuleResult.Pass(1 << cardIndex);
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 38. Any other 3 to a straight flush not listed
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }

                    var suitAndRanks = validSuits.First();
                    var ranks = suitAndRanks.Value;

                    if (ranks[0].InRange(2, 6) && ranks[2] - ranks[0] == 4)
                    {
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Values.Sum(i => 1 << i));
                    }

                    return Core.RuleResult.Fail();
                }
            );
        }
    }
}