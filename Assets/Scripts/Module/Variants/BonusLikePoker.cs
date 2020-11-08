using System;
using System.Collections.Generic;
using System.Linq;

namespace KtaneVideoPoker
{
    namespace Variants
    {
        public class BonusLikePoker: IVariant
        {
            private class HandType
            {
                public static int RoyalFlush = 0;
                public static int StraightFlush = 1;
                public static int FourAcesWith234 = 2;
                public static int Four2s3s4sWithA234 = 3;
                public static int FourAces = 4;
                public static int Four2s3s4s = 5;
                public static int Four5sThroughKs = 6;
                public static int FullHouse = 7;
                public static int Flush = 8;
                public static int Straight = 9;
                public static int ThreeOfAKind = 10;
                public static int TwoPair = 11;
                public static int JacksOrBetter = 12;
            }

            public static BonusLikePoker BonusPoker(bool bettingFiveCredits = true)
            {
                return new BonusLikePoker(
                    false,
                    bettingFiveCredits ? 800 : 250,
                    50,
                    80,
                    40,
                    80,
                    40,
                    25,
                    8, 5, 4, 3, 2, 1
                );
            }

            public static BonusLikePoker DoubleBonusPoker(bool bettingFiveCredits = true)
            {
                return new BonusLikePoker(
                    false,
                    bettingFiveCredits ? 800 : 250,
                    50,
                    160,
                    80,
                    160,
                    80,
                    50,
                    10, 7, 5, 3, 1, 1
                );
            }

            public static BonusLikePoker DoubleDoubleBonusPoker(bool bettingFiveCredits = true)
            {
                return new BonusLikePoker(
                    true,
                    bettingFiveCredits ? 800 : 250,
                    50,
                    400,
                    160,
                    160,
                    80,
                    50,
                    9, 6, 4, 3, 1, 1
                );
            }

            public static BonusLikePoker TripleDoubleBonusPoker(bool bettingFiveCredits = true)
            {
                return new BonusLikePoker(
                    true,
                    bettingFiveCredits ? 800 : 400,
                    50,
                    bettingFiveCredits ? 800 : 400,
                    400,
                    160,
                    80,
                    50,
                    9, 7, 4, 2, 1, 1
                );
            }

            public int JokerCount { get { return 0; }}

            private bool LooksAtKickers;
            private int[] PayoutArray;

            private BonusLikePoker(bool looksAtKickers, params int[] payoutArray)
            {
                LooksAtKickers = looksAtKickers;
                PayoutArray = payoutArray;
            }

            public Core.HandResult Evaluate(Core.Hand hand)
            {
                int straightCard = hand.GetHighestStraightCard();
                bool isFlushLike = hand.IsFlushLike();
                int matches = hand.GetPairwiseMatchCount();

                if (isFlushLike && straightCard != 0)
                {
                    return straightCard == 1 ? Core.HandResult.RoyalFlush : Core.HandResult.StraightFlush;
                }
                else if (matches == 6)
                {
                    int aceThroughFourCount = hand.Cards.Count(card => card.Rank <= 4);
                    int aceCount = hand.Cards.Count(card => card.Rank == 1);
                    if (aceCount == 4)
                    {
                        return (aceThroughFourCount == 5 && LooksAtKickers) ? Core.HandResult.FourAcesWith234 : Core.HandResult.FourAces;
                    }
                    else if (aceThroughFourCount == 5)
                    {
                        return LooksAtKickers ? Core.HandResult.FourTwosThreesOrFoursWithA234 : Core.HandResult.FourTwosThreesOrFours;
                    }
                    else if (aceThroughFourCount == 4)
                    {
                        return Core.HandResult.FourTwosThreesOrFours;
                    }
                    else
                    {
                        return Core.HandResult.FourFivesThroughKings;
                    }
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
                var start = new[]
                {
                    Core.HandResult.RoyalFlush,
                    Core.HandResult.StraightFlush
                };

                var end = new[]
                {
                    Core.HandResult.FourAces,
                    Core.HandResult.FourTwosThreesOrFours,
                    Core.HandResult.FourFivesThroughKings,
                    Core.HandResult.FullHouse,
                    Core.HandResult.Flush,
                    Core.HandResult.Straight,
                    Core.HandResult.ThreeOfAKind,
                    Core.HandResult.TwoPair,
                    Core.HandResult.JacksOrBetter,
                };

                if (LooksAtKickers)
                {
                    return start.Concat(new[]
                    {
                        Core.HandResult.FourAcesWith234,
                        Core.HandResult.FourTwosThreesOrFoursWithA234
                    }).Concat(end).ToArray();
                }

                return start.Concat(end).ToArray();
            }

            public int PayoutForResult(Core.HandResult result)
            {
                int index = new List<Core.HandResult>(new[]
                {
                    Core.HandResult.RoyalFlush,
                    Core.HandResult.StraightFlush,
                    Core.HandResult.FourAcesWith234,
                    Core.HandResult.FourTwosThreesOrFoursWithA234,
                    Core.HandResult.FourAces,
                    Core.HandResult.FourTwosThreesOrFours,
                    Core.HandResult.FourFivesThroughKings,
                    Core.HandResult.FullHouse,
                    Core.HandResult.Flush,
                    Core.HandResult.Straight,
                    Core.HandResult.ThreeOfAKind,
                    Core.HandResult.TwoPair,
                    Core.HandResult.JacksOrBetter,
                }).IndexOf(result);
                return index == -1 ? 0 : PayoutArray[index];
            }

            // STRATEGIES

            private static IEnumerable<int> AllRanks = Enumerable.Range(1, 13);
            private static IEnumerable<int> CardIndices = Enumerable.Range(0, 5);
            private static int[] RoyalRanks = new[] {1, 10, 11, 12, 13};

            public static Core.Strategy StrategyBonus = new Core.Strategy(
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
                    foreach (var ranksToFind in new[] {new[] {1, 2, 3}, new[] {1, 2, 4}, new[] {1, 2, 5}, new[] {1, 3, 4}, new[] {1, 3, 5}, new[] {1, 4, 5}, new[] {8, 9, 12}, new[] {8, 10, 12}, new[] {9, 10, 13}})
                    {
                        if (Enumerable.SequenceEqual(suitAndRanks.Value, ranksToFind))
                        {
                            return Core.RuleResult.Pass(ranksToFind.Select(rank => 1 << handInfo.CardPositionMatrix[suitAndRanks.Key][rank]).Sum());
                        }
                    }

                    foreach (var ranksToFind in new[] {new[] {5, 6, 8}, new[] {5, 7, 8}, new[] {6, 8, 9}})
                    {
                        if (Enumerable.SequenceEqual(suitAndRanks.Value, ranksToFind))
                        {
                            // Rule H: Ace only beats these if there is a straight penalty card of rank 10 or lower that doesn't match the suit of the Ace.
                            if (handInfo.RankCounts[1] == 1)
                            {
                                var aceSuit = handInfo.Cards.Where(card => card.Rank == 1).First().Suit;
                                if (handInfo.RanksPerSuit[aceSuit].Count() == 1 && handInfo.RankCounts[ranksToFind[0] - 1] + handInfo.RankCounts[ranksToFind[2] + 1] == 1)
                                {
                                    return Core.RuleResult.Fail("H");
                                }
                            }

                            // Rule G: Jack only beats suited 568 if there is a straight penalty card of 7 or lower that doesn't match the suit of the Jack.
                            if (ranksToFind[0] == 5 && handInfo.RankCounts[4] == 1 && handInfo.RankCounts[11] == 1)
                            {
                                var jackFourSuits = handInfo.Cards.Where(card => card.Rank == 11 || card.Rank == 4).Select(card => card.Suit);
                                if (jackFourSuits.Distinct().Count() == 2)
                                {
                                    return Core.RuleResult.Fail("G");
                                }
                            }

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
                (handInfo) => // 30. Suited TJ (rules I and J)
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(10) && kvp.Value.ContainsKey(11));
                    if (validSuits.Any())
                    {
                        var suitAndRanksLookup = validSuits.First();

                        // Rule I: Unsuited JA beats suited TJ if the latter has a flush penalty card of 6 or lower, and a straight penalty card besides the Ace.
                        if (handInfo.RankCounts[1] == 1 && suitAndRanksLookup.Value.Keys.Min() <= 6 && handInfo.RankCounts.GetRange(7, 3).Contains(1))
                        {
                            return Core.RuleResult.Fail("I");
                        }

                        // Rule J: Unsuited JK beats suited TJ if the latter has an 8, 9, or flush penalty card.
                        if (handInfo.RankCounts[13] == 1 && (suitAndRanksLookup.Value.Count() >= 3 || handInfo.RankCounts[8] == 1 || handInfo.RankCounts[9] == 1))
                        {
                            return Core.RuleResult.Fail("J");
                        }

                        return Core.RuleResult.Pass((1 << suitAndRanksLookup.Value[10]) + (1 << suitAndRanksLookup.Value[11]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 31. Suited 234, 235, 245, 346, 356, 457, 467, 679, 78T, or 79T
                {
                    var ranksToFind = new[]
                    {
                        new[] {2, 3, 4},
                        new[] {2, 3, 5},
                        new[] {2, 4, 5},
                        new[] {3, 4, 6},
                        new[] {3, 5, 6},
                        new[] {4, 5, 7},
                        new[] {4, 6, 7},
                        new[] {6, 7, 9}
                    };

                    foreach (var ranks in ranksToFind)
                    {
                        var valid = handInfo.RanksPerSuit.Where(kvp => Enumerable.SequenceEqual(kvp.Value, ranks));
                        if (valid.Any())
                        {
                            if (handInfo.RankCounts[1] == 1)
                            {
                                var aceSuit = handInfo.Cards.Where(card => card.Rank == 1).First().Suit;
                                if (ranks[0] == 2 && !(handInfo.RanksPerSuit[aceSuit].Length == 2 && handInfo.RanksPerSuit[aceSuit][1].InRange(7, 10)))
                                {
                                    // Rule K: If the hand contains an Ace, do not play this straight flush draw unless the fifth card is a 7 through 10 and matches the suit of the Ace.
                                    return Core.RuleResult.Fail("K");
                                }
                                else
                                {
                                    if (handInfo.RanksPerSuit[aceSuit].Length == 1 && (handInfo.RankCounts[ranks[0] - 1] == 1 || handInfo.RankCounts[ranks[2] + 1] == 1))
                                    {
                                        // Ace only beats this straight flush draw if there is a straight penalty card of rank 10 or lower that doesn't match the suit of the Ace.
                                        return Core.RuleResult.Fail("H");
                                    }
                                }
                            }

                            if (handInfo.RankCounts[11] == 1 && ranks[0] != 6)
                            {
                                var jackSuit = handInfo.Cards.Where(card => card.Rank == 11).First().Suit;
                                if (handInfo.RanksPerSuit[jackSuit].Length == 1 && ((ranks[0] > 2 && handInfo.RankCounts[ranks[0] - 1] == 1) || handInfo.RankCounts[Math.Max(ranks[2] + 1, 6)] == 1))
                                {
                                    // Jack only beats this straight flush draw if there is a straight penalty card of 7 or lower that doesn't match the suit of the Jack.
                                    return Core.RuleResult.Fail("G");
                                }
                            }

                            var suit = valid.First().Key;
                            return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suit].Sum(kvp => 1 << kvp.Value));
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
                (handInfo) => // 33. Suited 78T or 79T (rule L: Jack only beats suited 78T and 79T if the fifth card is a 6 that matches the suit of the Jack.)
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
                        if (handInfo.RankCounts[1] == 1)
                        {
                            var aceSuit = handInfo.Cards.Where(card => card.Rank == 1).First().Suit;
                            if (handInfo.RanksPerSuit[aceSuit].Length == 1 && handInfo.RankCounts[6] == 1)
                            {
                                // Ace only beats this straight flush draw if there is a straight penalty card of rank 10 or lower that doesn't match the suit of the Ace.
                                return Core.RuleResult.Fail("H");
                            }
                        }

                        if (handInfo.RankCounts[11] == 1 && handInfo.RankCounts[6] == 1)
                        {
                            // Jack only beats this straight flush draw if there is a straight penalty card of 7 or lower that doesn't match the suit of the Jack.
                            return Core.RuleResult.Fail("L");
                        }

                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Values.Sum(i => 1 << i));
                    }

                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 34. Unsuited QK
                {
                    if (handInfo.RankCounts[12] == 1 && handInfo.RankCounts[13] == 1)
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => new[] {12, 13}.Contains(handInfo.Cards[i].Rank)).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 35. Unsuited JA, QA, or KA
                {
                    if (handInfo.RankCounts[1] == 0)
                    {
                        return Core.RuleResult.Fail();
                    }
                    for (int otherRank = 11; otherRank <= 13; otherRank++)
                    {
                        if (handInfo.RankCounts[otherRank] == 1)
                        {
                            return Core.RuleResult.Pass(CardIndices.Where(i => new[] {1, otherRank}.Contains(handInfo.Cards[i].Rank)).Sum(i => 1 << i));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 36. Single Ace
                {
                    if (handInfo.RankCounts[1] == 1)
                    {
                        return Core.RuleResult.Pass(1 << CardIndices.Where(i => handInfo.Cards[i].Rank == 1).First());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 37. Suited TQ or TK (rule M: King only beats suited TK if the latter has a flush penalty card.)
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
                            return Core.RuleResult.Fail("M");
                        }

                        return Core.RuleResult.Pass((1 << ranksLookup[10]) + (1 << ranksLookup[13]));
                    }

                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 38. Single Jack, Queen, or King
                {
                    int maxRank = handInfo.Cards.Max(card => card.Rank);
                    if (maxRank >= 11)
                    {
                        return Core.RuleResult.Pass(1 << CardIndices.Where(i => handInfo.Cards[i].Rank == maxRank).First());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 39. Any other 3 to a straight flush not listed
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

            public static Core.Strategy StrategyDoubleBonus = new Core.Strategy(
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
                (handInfo) => // 5. Three Aces
                {
                    if (handInfo.RankCounts[1] == 3)
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == 1).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 6. Full house
                {
                    var trips = handInfo.RankCounts.IndexOf(3);
                    var pair = handInfo.RankCounts.IndexOf(2);
                    if (trips > 0 && pair > 0)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 7. Flush
                {
                    var flushes = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 5);
                    if (flushes.Any())
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 8. Straight
                {
                    var ranks = AllRanks.Where(r => handInfo.RankCounts[r] == 1).ToArray();
                    if (ranks.Length == 5 && (Enumerable.SequenceEqual(ranks, RoyalRanks) || ranks[4] - ranks[0] == 4))
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 9. Three of a kind, except 3 Aces
                {
                    var trips = handInfo.RankCounts.IndexOf(3);
                    if (trips > 0)
                    {
                        var cardIndicesToKeep = CardIndices.Where(i => handInfo.Cards[i].Rank == trips);
                        return Core.RuleResult.Pass(cardIndicesToKeep.Select(i => 1 << i).Sum());
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
                (handInfo) => // 11. Two pair
                {
                    if (handInfo.RankCounts.Count(n => n == 2) == 2)
                    {
                        var indexToDiscard = CardIndices.Where(i => handInfo.RankCounts[handInfo.Cards[i].Rank] == 1).First();
                        return Core.RuleResult.Pass(31 - (1 << indexToDiscard));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 12. Pair of Aces
                {
                    if (handInfo.RankCounts[1] == 2)
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == 1).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 13. 4 to a flush, type 1
                {
                     var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Count() == 4);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var suitAndRanks = validSuits.First();
                    var suit = suitAndRanks.Key;
                    var ranks = suitAndRanks.Value;
                    var highFlushCards = ranks.Where(rank => rank == 1 || rank >= 11).ToArray();
                    if (highFlushCards.Count() % 3 == 0 || (highFlushCards.Max() == 1))
                    {
                        return Core.RuleResult.Pass(ranks.Select(r => 1 << handInfo.CardPositionMatrix[suit][r]).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 14. Suited TJQ or JQK
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(11) && kvp.Value.ContainsKey(12) && (kvp.Value.ContainsKey(10) || kvp.Value.ContainsKey(13)));
                    if (validSuits.Any())
                    {
                        var ranksLookup = validSuits.First().Value;
                        return Core.RuleResult.Pass(Enumerable.Range(10, 4).Where(rank => ranksLookup.ContainsKey(rank)).Sum(rank => 1 << ranksLookup[rank]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 15. Pair of Jacks, Queens, or Kings
                {
                    for (int rank = 11; rank <= 13; rank++)
                    {
                        if (handInfo.RankCounts[rank] == 2)
                        {
                            return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == rank).Select(i => 1 << i).Sum());
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 16. 4 to a flush, type 2
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Count() == 4);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var suitAndRanks = validSuits.First();
                    var suit = suitAndRanks.Key;
                    return Core.RuleResult.Pass(suitAndRanks.Value.Select(r => 1 << handInfo.CardPositionMatrix[suit][r]).Sum());
                },
                (handInfo) => // 17. 3 to a royal flush
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => RoyalRanks.Count(rank => kvp.Value.ContainsKey(rank)) >= 3);
                    if (validSuits.Any())
                    {
                        var ranksLookup = validSuits.First().Value.Where(kvp => kvp.Key == 1 || kvp.Key >= 10);
                        return Core.RuleResult.Pass(ranksLookup.Sum(kvp => 1 << kvp.Value));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 18. 4 to an open-ended straight (rule B: if there is also a pair, it doesn't matter which paired card you discard.)
                {
                    int runningTotal = handInfo.RankCounts.GetRange(1, 4).Sum(n => Math.Min(n, 1));
                    for (int lowRank = 2; lowRank <= 10; lowRank++)
                    {
                        runningTotal += Math.Min(handInfo.RankCounts[lowRank + 3], 1);
                        runningTotal -= Math.Min(handInfo.RankCounts[lowRank - 1], 1);
                        if (runningTotal == 4)
                        {
                            List<int>[] indexChoices = Enumerable.Range(0, 4).Select(i => new int[0].ToList()).ToArray();
                            for (int i = 0; i < 5; i++)
                            {
                                int arrayIndex = handInfo.Cards[i].Rank - lowRank;
                                if (arrayIndex.InRange(0, 3))
                                {
                                    indexChoices[arrayIndex].Add(i);
                                }
                            }

                            var strategies = new[] {0};

                            foreach (var choices in indexChoices)
                            {
                                strategies = choices.SelectMany(choice => strategies.Select(bitmask => bitmask + (1 << choice))).ToArray();
                            }

                            if (strategies.Length > 1)
                            {
                                return Core.RuleResult.Pass("B", strategies);
                            }
                            else
                            {
                                return Core.RuleResult.Pass(strategies);
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 19. Pair of 2s, 3s, or 4s
                {
                    for (int rank = 2; rank <= 4; rank++)
                    {
                        if (handInfo.RankCounts[rank] == 2)
                        {
                            return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == rank).Select(i => 1 << i).Sum());
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 20. 3 to a straight flush, type 1 (9TJ or 9JQ)
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(9) && kvp.Value.ContainsKey(11) && (kvp.Value.ContainsKey(10) || kvp.Value.ContainsKey(12)));
                    if (validSuits.Any())
                    {
                        var ranksLookup = validSuits.First().Value;
                        return Core.RuleResult.Pass(Enumerable.Range(9, 4).Where(rank => ranksLookup.ContainsKey(rank)).Sum(rank => 1 << ranksLookup[rank]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 21. Pair of 5s through 10s
                {
                    for (int rank = 5; rank <= 10; rank++)
                    {
                        if (handInfo.RankCounts[rank] == 2)
                        {
                            return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == rank).Select(i => 1 << i).Sum());
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                // Below here, we can assume that all ranks are different.
                (handInfo) => // 22. Unsuited JQKA
                {
                    var ranks = new[] {1, 11, 12, 13};
                    if (ranks.All(rank => handInfo.RankCounts[rank] == 1))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank >= 11 || handInfo.Cards[i].Rank == 1).Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 23. 3 to a straight flush, type 2 (The number of high cards equals the number of gaps, except Ace low or 234)
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
                (handInfo) => // 24. Unsuited 9JQK, TJQA, TJKA, or TQKA
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
                (handInfo) => // 25. 3 to a flush, type 1 (Two high cards (unless those high cards are JQ), and 6TA)
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var suitAndRanks = validSuits.First();
                    var highCards = suitAndRanks.Value.Where(rank => rank == 1 || rank >= 11).ToArray();
                    if ((highCards.Length == 2 && !Enumerable.SequenceEqual(new[] {11, 12}, highCards)))
                    {
                        if (highCards.Length == 2 && highCards[1] == 13 && highCards[0] >= 11 && handInfo.RankCounts.GetRange(2, 7).Sum() == 3)
                        {
                            return Core.RuleResult.Fail("C");
                        }
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Sum(kvp => 1 << kvp.Value));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 26. Suited JQ, JK, or QK (rule D: Suited 78T/79T beats suited QK)
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.Count(kvp2 => kvp2.Key >= 11) == 2);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var ranksLookup = validSuits.First().Value.Where(kvp => kvp.Key >= 11);
                    if (ranksLookup.Sum(kvp => kvp.Key) == 25 && handInfo.RanksPerSuit.Any(kvp => kvp.Value.Length == 3 && kvp.Value[0] == 7 && kvp.Value[2] == 10))
                    {
                        return Core.RuleResult.Fail("D");
                    }
                    return Core.RuleResult.Pass(ranksLookup.Sum(kvp => 1 << kvp.Value));
                },
                (handInfo) => // 27. Suited 346, 356, 457, 467, 568, 578, 679, 689, 78T, 79T (rule E: suited JA beats 78T and 79T)
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var suitAndRanks = validSuits.First();
                    var ranks = suitAndRanks.Value;
                    if (ranks[0].InRange(3, 7) && ranks[2] - ranks[0] == 3)
                    {
                        if (ranks[0] == 7 && handInfo.RanksPerSuit.Values.Any(rs => Enumerable.SequenceEqual(rs, new[] {1, 11})))
                        {
                            return Core.RuleResult.Fail("E");
                        }
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Sum(kvp => 1 << kvp.Value));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 28. Suited JA
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(11) && kvp.Value.ContainsKey(1));
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var ranksLookup = validSuits.First().Value;
                    return Core.RuleResult.Pass((1 << ranksLookup[11]) | (1 << ranksLookup[1]));
                },
                (handInfo) => // 29. Unsuited 89JQ (rule F: Suited QA beats unsuited 89JQ), 8TJQ, 9TJK, or 9TQK
                {
                    var partialStraights = new[]
                    {
                        new[] {8, 9, 11, 12},
                        new[] {8, 10, 11, 12},
                        new[] {9, 10, 11, 13},
                        new[] {9, 10, 12, 13}
                    };
                    var matches = partialStraights.Where(partialStraight => partialStraight.Count(rank => handInfo.RankCounts[rank] == 1) == 4);
                    if (!matches.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var match = matches.First();
                    if (match[1] == 9)
                    {
                        var queenSuit = handInfo.Cards.Where(card => card.Rank == 12).First().Suit;
                        if (handInfo.CardPositionMatrix[queenSuit].ContainsKey(1))
                        {
                            return Core.RuleResult.Fail("F");
                        }
                    }
                    int rankToDiscard = handInfo.Cards.Select(card => card.Rank).Sum() - match.Sum();
                    return Core.RuleResult.Pass(31 - (1 << CardIndices.Where(i => handInfo.Cards[i].Rank == rankToDiscard).First()));
                },
                (handInfo) => // 30. Suited 78J, 79J, or 7TJ
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var suitAndRanks = validSuits.First();
                    var ranks = suitAndRanks.Value;
                    if (ranks[0] == 7 && ranks[2] == 11)
                    {
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Sum(kvp => 1 << kvp.Value));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 31. Suited QA
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(12) && kvp.Value.ContainsKey(1));
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var ranksLookup = validSuits.First().Value;
                    return Core.RuleResult.Pass((1 << ranksLookup[12]) | (1 << ranksLookup[1]));
                },
                (handInfo) => // 32. Suited 89Q, 8TQ, or 9TK
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var suitAndRanks = validSuits.First();
                    var ranks = suitAndRanks.Value;
                    if (ranks[0].InRange(8, 9) && ranks[2] - ranks[0] == 4)
                    {
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Sum(kvp => 1 << kvp.Value));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 33. Suited KA
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(13) && kvp.Value.ContainsKey(1));
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var ranksLookup = validSuits.First().Value;
                    return Core.RuleResult.Pass((1 << ranksLookup[13]) | (1 << ranksLookup[1]));
                },
                (handInfo) => // 34. 3 to a straight flush: Ace low, 234, 235, or 245
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var suitAndRanks = validSuits.First();
                    var ranks = suitAndRanks.Value;
                    if (ranks[0] <= 2 && ranks[2] <= 5)
                    {
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Sum(kvp => 1 << kvp.Value));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 35. Unsuited A234, A235, A245, or A345
                {
                    if (handInfo.RankCounts.GetRange(1, 5).Sum() == 4)
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank <= 5).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 36. Unsuited TJQ or JQK
                {
                    if (handInfo.RankCounts[11] + handInfo.RankCounts[12] == 2 && handInfo.RankCounts[10] + handInfo.RankCounts[13] == 1)
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(10, 13)).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 37. Unsuited 789J, 78TJ, 79TJ, or 89TQ
                {
                    var partialStraights = new[]
                    {
                        new[] {7, 8, 9, 11},
                        new[] {7, 8, 10, 11},
                        new[] {7, 9, 10, 11},
                        new[] {8, 9, 10, 12}
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
                (handInfo) => // 38. Suited TJ (rule G: 3 to a flush (2TJ through 6TJ) beats suited TJ if the two cards not part of the flush draw are 7K, 8K, 8A, or 9A.)
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(10) && kvp.Value.ContainsKey(11));
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }

                    var suitAndRanksLookup = validSuits.First();

                    var nonFlushRanks = handInfo.Cards.Where(card => card.Suit != suitAndRanksLookup.Key).Select(card => card.Rank).OrderBy(n => n).ToArray();;
                    int nonFlushValues = nonFlushRanks[0] * 16 + nonFlushRanks[1];

                    if (nonFlushValues == 0x7D || nonFlushValues == 0x8D || nonFlushValues == 0x18 || nonFlushValues == 0x19)
                    {
                        return Core.RuleResult.Fail("G");
                    }

                    return Core.RuleResult.Pass((1 << suitAndRanksLookup.Value[10]) | (1 << suitAndRanksLookup.Value[11]));
                },
                (handInfo) => // 39. Unsuited JQ
                {
                    if (handInfo.RankCounts[11] + handInfo.RankCounts[12] == 2)
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(11, 12)).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 40. 3 to a flush, type 2 (1 high card, unless the high card is a King and the other two cards in the flush draw have rank 7 or lower) (rules H and I)
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var suitAndRanks = validSuits.First();
                    var ranks = suitAndRanks.Value;
                    if (ranks[0] == 1 || ranks[2] == 11 || ranks[2] == 12 || (ranks[2] == 13 && ranks[1] >= 8))
                    {
                        // Rule H: If this hand contains two high cards, neither of which is an Ace, and the other three cards have rank 8 or lower, keep the two high cards instead.
                        if (handInfo.RankCounts.GetRange(11, 3).Sum() == 2 && handInfo.RankCounts.GetRange(2, 7).Sum() == 3)
                        {
                            return Core.RuleResult.Fail("H");
                        }

                        // Rule I: Suited TQ beats 3 to a flush if the flush draw's high card is an Ace and its middle card is a 6, 7, or 8.
                        if (ranks[0] == 1 && ranks[2].InRange(6, 8) && handInfo.RanksPerSuit.Any(kvp => Enumerable.SequenceEqual(kvp.Value, new[] {10, 12})))
                        {
                            return Core.RuleResult.Fail("I");
                        }

                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Sum(kvp => 1 << kvp.Value));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 41. Unsuited JK (rules J, K, and L)
                {
                    if (handInfo.RankCounts[11] + handInfo.RankCounts[13] == 2)
                    {
                        var flushDraws = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                        if (flushDraws.Any())
                        {
                            // Rule J: 3 to a straight flush always beats unsuited JK.
                            var flushDraw = handInfo.RanksPerSuit[flushDraws.First().Key];
                            if (flushDraw[2] - flushDraw[0] == 4)
                            {
                                return Core.RuleResult.Fail("J");
                            }

                            // Rule K: 3 to a flush of type 4 beats unsuited JK if the hand contains a 9, 10, or Ace.
                            if (flushDraw[2] == 13 && flushDraw[1] <= 7 && (handInfo.RankCounts[1] == 1 || handInfo.RankCounts[9] == 1 || handInfo.RankCounts[10] == 1))
                            {
                                return Core.RuleResult.Fail("K");
                            }
                        }

                        // Rule L: If the hand also contains an Ace, a 9, and a card lower than a 9, keep the Ace as well.
                        if (handInfo.RankCounts[1] + handInfo.RankCounts[9] == 2 && handInfo.RankCounts.GetRange(2, 7).Contains(1))
                        {
                            return Core.RuleResult.Pass("L", CardIndices.Where(i => handInfo.Cards[i].Rank == 11 || handInfo.Cards[i].Rank == 13 || handInfo.Cards[i].Rank == 1).Sum(i => 1 << i));
                        }

                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == 11 || handInfo.Cards[i].Rank == 13).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 42. 3 to a flush, type 3 (78K) (rule M: Unsuited QK beats suited 78K if the fifth card is a 6 or lower.)
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => Enumerable.SequenceEqual(kvp.Value, new[] {7, 8, 13}));
                    if (validSuits.Any())
                    {
                        if (handInfo.RankCounts.GetRange(2, 5).Contains(1))
                        {
                            return Core.RuleResult.Fail("M");
                        }
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[validSuits.First().Key].Sum(kvp => 1 << kvp.Value));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 43. Suited TQ (rule N: Suited 569 and 579 beat suited TQ.)
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(10) && kvp.Value.ContainsKey(12));
                    if (validSuits.Any())
                    {
                        if (handInfo.RanksPerSuit.Any(kvp => kvp.Value.Length == 3 && kvp.Value[0] == 5 && kvp.Value[2] == 9 && kvp.Value[1].InRange(6, 7)))
                        {
                            return Core.RuleResult.Fail("N");
                        }
                        var ranksLookup = validSuits.First().Value;
                        return Core.RuleResult.Pass((1 << ranksLookup[10]) | (1 << ranksLookup[12]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 44. 3 to a straight flush: 0 high cards, 2 gaps (rules O and P)
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var suitAndRanks = validSuits.First();
                    var ranks = suitAndRanks.Value;
                    if (ranks[2] - ranks[0] == 4)
                    {
                        // Rule O: Ace only beats these straight flush draws if the straight flush draw requires discarding a straight penalty card.
                        if (handInfo.RankCounts[1] == 1 && handInfo.RankCounts.GetRange(ranks[0], 5).Sum() == 4)
                        {
                            return Core.RuleResult.Fail("O");
                        }

                        // Rule P: Jack only beats these straight flush draws if the straight flush draw requires discarding a straight penalty card that doesn't match the suit of the Jack.
                        if (handInfo.RankCounts[11] == 1)
                        {
                            var jackSuit = handInfo.Cards.First(card => card.Rank == 11).Suit;
                            if (handInfo.RankCounts.GetRange(ranks[0], 5).Sum() == 4)
                            {
                                var straightPenaltyCard = handInfo.Cards.First(card => card.Rank.InRange(ranks[0], ranks[2]) && card.Suit != suitAndRanks.Key);
                                if ((straightPenaltyCard.Suit != jackSuit || ranks[0] == 2) && !Enumerable.SequenceEqual(handInfo.RankCounts.GetRange(5, 5), new[] {1, 1, 0, 1, 1}))
                                {
                                    return Core.RuleResult.Fail("P");
                                }
                            }
                        }

                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Sum(kvp => 1 << kvp.Value));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 45. 3 to a flush, type 4 (King with two cards of rank 7 or lower) (rule Q: Unsuited QK beats these flush draws if the three non-high cards have rank 8 or lower.)
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var suitAndRanks = validSuits.First();
                    var ranks = suitAndRanks.Value;
                    if (ranks[2] == 13 && ranks[1] <= 7)
                    {
                        if (handInfo.RankCounts[12] == 1 && handInfo.RankCounts.GetRange(2, 7).Sum() == 3)
                        {
                            return Core.RuleResult.Fail("Q");
                        }
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Sum(kvp => 1 << kvp.Value));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 46. Unsuited QK (rule L)
                {
                    if (handInfo.RankCounts[12] + handInfo.RankCounts[13] == 2)
                    {
                        // Rule L: If the hand also contains an Ace, a 9, and a card lower than a 9, keep the Ace as well.
                        if (handInfo.RankCounts[1] + handInfo.RankCounts[9] == 2 && handInfo.RankCounts.GetRange(2, 7).Contains(1))
                        {
                            return Core.RuleResult.Pass("L", CardIndices.Where(i => handInfo.Cards[i].Rank == 12 || handInfo.Cards[i].Rank == 13 || handInfo.Cards[i].Rank == 1).Sum(i => 1 << i));
                        }

                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == 12 || handInfo.Cards[i].Rank == 13).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 47. Unsuited JA, QA, or KA (rule R is a bit complicated)
                {
                    if (handInfo.RankCounts[1] == 1 && handInfo.RankCounts.GetRange(11, 3).Contains(1))
                    {
                        var cardsSortedByRank = handInfo.Cards.OrderBy(card => card.Rank == 1 ? 14 : card.Rank).ToArray();

                        int score = 0;

                        if (cardsSortedByRank[0].Rank.InRange(6, 7))
                        {
                            score += 2;
                        }
                        if (cardsSortedByRank[0].Rank == 8)
                        {
                            score += 1;
                        }
                        if (cardsSortedByRank[2].Rank == 10)
                        {
                            score += 1;
                        }

                        var aceSuit = handInfo.Cards.First(card => card.Rank == 1).Suit;
                        if (cardsSortedByRank[0].Suit == aceSuit)
                        {
                            score -= 1;
                        }
                        if (cardsSortedByRank[1].Suit == aceSuit)
                        {
                            score -= 1;
                        }
                        if (cardsSortedByRank[2].Suit == aceSuit)
                        {
                            score -= 2;
                        }

                        if (score >= 0)
                        {
                            return Core.RuleResult.Fail("R");
                        }

                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == 1 || handInfo.Cards[i].Rank >= 11).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 48. Single Ace
                {
                    if (handInfo.RankCounts[1] == 1)
                    {
                        return Core.RuleResult.Pass(1 << CardIndices.Where(i => handInfo.Cards[i].Rank == 1).First());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 49. Suited TK
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(10) && kvp.Value.ContainsKey(13));
                    if (validSuits.Any())
                    {
                        var ranksLookup = validSuits.First().Value;
                        return Core.RuleResult.Pass((1 << ranksLookup[10]) | (1 << ranksLookup[13]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 50. Single Jack, Queen, or King
                {
                    if (handInfo.Cards.Any(card => card.Rank >= 11))
                    {
                        return Core.RuleResult.Pass(1 << CardIndices.First(i => handInfo.Cards[i].Rank >= 11));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 51. 4 to an inside straight (rule S: If there are two ways to play this hand as 4 to an inside straight, you may choose either one.)
                {
                    var validStrategies = new List<int>();

                    int total = handInfo.RankCounts.GetRange(1, 5).Sum();
                    for (int lowRank = 2; lowRank <= 6; lowRank++)
                    {
                        total += handInfo.RankCounts[lowRank + 4] - handInfo.RankCounts[lowRank - 1];
                        if (total == 4)
                        {
                            validStrategies.Add(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(lowRank, lowRank + 4)).Sum(i => 1 << i));
                        }
                    }

                    if (validStrategies.Count() == 2)
                    {
                        return Core.RuleResult.Pass("S", validStrategies.ToArray());
                    }
                    if (validStrategies.Count() == 1)
                    {
                        return Core.RuleResult.Pass(validStrategies[0]);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 52. 3 to a flush, type 5 (anything not previously listed)
                {
                    var validLookups = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.Count() == 3);
                    if (!validLookups.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    return Core.RuleResult.Pass(validLookups.First().Value.Sum(kvp => 1 << kvp.Value));
                }
            );

            public static Core.Strategy StrategyDoubleDoubleBonus = new Core.Strategy(
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
                (handInfo) => // 3. Four of a kind (rule A: If you have four Aces, 2s, 3s, or 4s, always go for the A-4 kicker if you don't have it already. If you have four 5s through Kings, you may either keep or discard the kicker.)
                {
                    var quads = handInfo.RankCounts.IndexOf(4);
                    if (quads > 0)
                    {
                        int kickerIndex = CardIndices.First(i => handInfo.Cards[i].Rank != quads);

                        if (quads <= 4)
                        {
                            if (handInfo.RankCounts.GetRange(1, 4).Sum() == 5)
                            {
                                return Core.RuleResult.Pass("A", 31);
                            }
                            else
                            {
                                return Core.RuleResult.Pass("A", 31 - (1 << kickerIndex));
                            }
                        }
                        else
                        {
                            return Core.RuleResult.Pass("A", 31 - (1 << kickerIndex), 31);
                        }
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
                (handInfo) => // 5. Three Aces
                {
                    if (handInfo.RankCounts[1] == 3)
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == 1).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 6. Full house
                {
                    var trips = handInfo.RankCounts.IndexOf(3);
                    var pair = handInfo.RankCounts.IndexOf(2);
                    if (trips > 0 && pair > 0)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 7. Flush
                {
                    var flushes = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 5);
                    if (flushes.Any())
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 8. Straight
                {
                    var ranks = AllRanks.Where(r => handInfo.RankCounts[r] == 1).ToArray();
                    if (ranks.Length == 5 && (Enumerable.SequenceEqual(ranks, RoyalRanks) || ranks[4] - ranks[0] == 4))
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 9. Three of a kind, except 3 Aces
                {
                    var trips = handInfo.RankCounts.IndexOf(3);
                    if (trips > 0)
                    {
                        var cardIndicesToKeep = CardIndices.Where(i => handInfo.Cards[i].Rank == trips);
                        return Core.RuleResult.Pass(cardIndicesToKeep.Select(i => 1 << i).Sum());
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
                (handInfo) => // 11. Pair of Aces
                {
                    if (handInfo.RankCounts[1] == 2)
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == 1).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 12. Two pair
                {
                    if (handInfo.RankCounts.Count(n => n == 2) == 2)
                    {
                        var indexToDiscard = CardIndices.Where(i => handInfo.RankCounts[handInfo.Cards[i].Rank] == 1).First();
                        return Core.RuleResult.Pass(31 - (1 << indexToDiscard));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 13. Suited JQK (rule B: A pair of Jacks, Queens, or Kings beats suited JQK if the hand contains a flush penalty card or a 10.)
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Count() >= 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var suitAndRanks = validSuits.First();
                    var suit = suitAndRanks.Key;
                    var ranks = suitAndRanks.Value;
                    if (ranks[ranks.Length - 3] == 11)
                    {
                        if (handInfo.RankCounts.GetRange(11, 3).Contains(2) && (ranks.Count() == 4 || handInfo.RankCounts[10] == 1))
                        {
                            return Core.RuleResult.Fail("B");
                        }
                        return Core.RuleResult.Pass(Enumerable.Range(11, 3).Select(r => 1 << handInfo.CardPositionMatrix[suit][r]).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 14. Pair of Kings
                {
                    if (handInfo.RankCounts[13] == 2)
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == 13).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 15. Suited TJQ (rule C: A pair of Jacks or Queens beats suited TJQ if the hand contains a flush penalty card, a 9, a King, or an Ace.)
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.Count() >= 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var suitAndRanks = validSuits.First();
                    var suit = suitAndRanks.Key;
                    var ranksLookup = suitAndRanks.Value;
                    if (Enumerable.Range(10, 3).All(rank => ranksLookup.ContainsKey(rank)))
                    {
                        if ((handInfo.RankCounts[11] == 2 || handInfo.RankCounts[12] == 2) && (ranksLookup.Count() == 4 || handInfo.RankCounts[1] + handInfo.RankCounts[9] + handInfo.RankCounts[13] == 1))
                        {
                            return Core.RuleResult.Fail("C");
                        }
                        return Core.RuleResult.Pass(Enumerable.Range(10, 3).Select(r => 1 << ranksLookup[r]).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 16. Pair of Jacks or Queens
                {
                    for (int rank = 11; rank <= 12; rank++)
                    {
                        if (handInfo.RankCounts[rank] == 2)
                        {
                            return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == rank).Sum(i => 1 << i));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 17. Suited TJK, TQK, JQA, JKA, or QKA
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp =>
                    {
                        var royals = kvp.Value.Intersect(RoyalRanks);
                        return royals.Count() == 3 && !(royals.Contains(10) && royals.Contains(1));
                    });
                    if (validSuits.Any())
                    {
                        var suit = validSuits.First().Key;
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Suit == suit && (handInfo.Cards[i].Rank == 1 || handInfo.Cards[i].Rank >= 10)).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 18. 4 to a flush
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.Count() == 4);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    return Core.RuleResult.Pass(validSuits.First().Value.Sum(kvp => 1 << kvp.Value));
                },
                (handInfo) => // 19. Suited TJA, TQA, TKA
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3 && kvp.Value[0] == 1 && kvp.Value[1] == 10);
                    if (validSuits.Any())
                    {
                        var suit = validSuits.First().Key;
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suit].Sum(kvp => 1 << kvp.Value));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 20. Unsuited 89TJ, 9TJQ, or TJQK (rule D: If you also have a low pair, it doesn't matter which card in the pair you discard.)
                {
                    int total = handInfo.RankCounts.GetRange(7, 4).Sum(n => Math.Min(n, 1));
                    for (int lowRank = 8; lowRank <= 10; lowRank++)
                    {
                        total += Math.Min(handInfo.RankCounts[lowRank + 3], 1);
                        total -= Math.Min(handInfo.RankCounts[lowRank - 1], 1);
                        if (total == 4)
                        {
                            for (int potentialPair = lowRank; potentialPair <= 10; potentialPair++)
                            {
                                if (handInfo.RankCounts[potentialPair] == 2)
                                {
                                    var pairIndices = CardIndices.Where(i => handInfo.Cards[i].Rank == potentialPair).ToArray();
                                    return Core.RuleResult.Pass("D", 31 - (1 << pairIndices[0]), 31 - (1 << pairIndices[1]));
                                }
                            }
                            return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(lowRank, lowRank + 3)).Sum(i => 1 << i));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 21. Low pair
                {
                    var pairRank = handInfo.RankCounts.IndexOf(2);
                    if (pairRank > 0)
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == pairRank).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                // Below here, we can assume that all ranks are different.
                (handInfo) => // 22. 4 to an open-ended straight, except JQKA
                {
                    int rankCount = handInfo.RankCounts.GetRange(1, 4).Sum();
                    for (int minRank = 2; minRank <= 7; minRank++)
                    {
                        rankCount += handInfo.RankCounts[minRank + 3] - handInfo.RankCounts[minRank - 1];
                        if (rankCount == 4)
                        {
                            return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(minRank, minRank + 3)).Select(i => 1 << i).Sum());
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 23. 3 to a straight flush, type 1 (the number of high cards equals or exceeds the number of gaps, except Ace low or 2-3-4)
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
                (handInfo) => // 24. Unsuited JQKA
                {
                    var ranks = new[] {1, 11, 12, 13};
                    if (ranks.All(rank => handInfo.RankCounts[rank] == 1))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank >= 11 || handInfo.Cards[i].Rank == 1).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 25. 2 suited high cards (rule E: 4 to a straight with 3 high cards beats 2 suited high cards if the latter has a flush penalty card.)
                {
                    var ranks = new[] {1, 11, 12, 13};
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => ranks.Count(rank => kvp.Value.ContainsKey(rank)) == 2);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }

                    var suit = validSuits.First().Key;
                    var ranksLookup = validSuits.First().Value;

                    if (ranksLookup.ContainsKey(13) || ranksLookup.ContainsKey(1))
                    {
                        var partialStraights = new[]
                        {
                            new[] {9, 11, 12, 13},
                            new[] {10, 11, 12, 1},
                            new[] {10, 11, 13, 1},
                            new[] {10, 12, 13, 1}
                        };
                        var matches = partialStraights.Where(partialStraight => partialStraight.All(rank => handInfo.RankCounts[rank] == 1));
                        if (matches.Any() && ranksLookup.Count() >= 3)
                        {
                            return Core.RuleResult.Fail("E");
                        }
                    }

                    return Core.RuleResult.Pass(ranksLookup.Where(kvp => kvp.Key == 1 || kvp.Key >= 11).Sum(kvp => 1 << kvp.Value));
                },
                (handInfo) => // 26. 9JQK, TJQA, TJKA, or TQKA
                {
                    var partialStraights = new[]
                    {
                        new[] {9, 11, 12, 13},
                        new[] {10, 11, 12, 1},
                        new[] {10, 11, 13, 1},
                        new[] {10, 12, 13, 1}
                    };
                    var matches = partialStraights.Where(partialStraight => partialStraight.All(rank => handInfo.RankCounts[rank] == 1));
                    if (!matches.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var match = matches.First();
                    int rankToDiscard = handInfo.Cards.Select(card => card.Rank).Sum() - match.Sum();
                    return Core.RuleResult.Pass(31 - (1 << CardIndices.Where(i => handInfo.Cards[i].Rank == rankToDiscard).First()));
                },
                (handInfo) => // 27. 3 to a straight flush, type 2 (one gap with no high cards, two gaps with one high card, Ace low, or 2-3-4)
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
                (handInfo) => // 28. Unsuited JQK
                {
                    var jqkCounts = handInfo.RankCounts.GetRange(11, 3);
                    if (Enumerable.SequenceEqual(jqkCounts, new[] {1, 1, 1}))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(11, 13)).Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 29. Unsuited 89JQ, 8TJQ, 9TJK, or 9TQK
                {
                    var partialStraights = new[]
                    {
                        new[] {8, 9, 11, 12},
                        new[] {8, 10, 11, 12},
                        new[] {9, 10, 11, 13},
                        new[] {9, 10, 12, 13}
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
                (handInfo) => // 30. Unsuited JQ (rule F: Ace only beats unsuited JQ if the remaining two cards' ranks are 6 and 8, 7 and 8, or anything with a 9; and neither of them match the suit of the Ace.)
                {
                    if (handInfo.RankCounts[11] != 1 || handInfo.RankCounts[12] != 1)
                    {
                        return Core.RuleResult.Fail();
                    }

                    if (handInfo.RankCounts[1] == 1 && handInfo.RanksPerSuit[handInfo.Cards.First(card => card.Rank == 1).Suit].Length == 1)
                    {
                        var lowCards = Enumerable.Range(2, 8).Where(rank => handInfo.RankCounts[rank] == 1).ToArray();
                        int lowMask = lowCards[0] * 10 + lowCards[1];
                        if (new[] {68, 78, 29, 39, 49, 59, 69, 79}.Contains(lowMask))
                        {
                            return Core.RuleResult.Fail("F");
                        }
                    }

                    return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(11, 12)).Sum(i => 1 << i));
                },
                (handInfo) => // 31. Single Ace (rules G and H)
                {
                    if (handInfo.RankCounts[1] == 1)
                    {
                        var aceSuit = handInfo.Cards.First(card => card.Rank == 1).Suit;

                        var potentialTenJack = handInfo.RanksPerSuit.Where(kvp => Enumerable.SequenceEqual(kvp.Value, Enumerable.Range(10, 2)));
                        if (potentialTenJack.Any())
                        {
                            // Rule G is determined by a complicated table.
                            var cardsSortedByRank = handInfo.Cards.OrderBy(card => card.Rank == 1 ? 14 : card.Rank).ToArray();
                            
                            int score = 0;

                            if (cardsSortedByRank[0].Rank <= 4)
                            {
                                score += 1;
                            }
                            else if (cardsSortedByRank[0].Rank >= 6)
                            {
                                score -= 1;
                            }

                            if (cardsSortedByRank[1].Rank == 7)
                            {
                                score -= 2;
                            }
                            else if (cardsSortedByRank[1].Rank == 8)
                            {
                                score -= 5;
                            }
                            else if (cardsSortedByRank[1].Rank == 9)
                            {
                                score -= 10;
                            }

                            score += 3 * cardsSortedByRank.Take(2).Count(card => card.Suit == aceSuit);

                            if (score > 0)
                            {
                                return Core.RuleResult.Fail("G");
                            }
                        }

                        var ranksWithAceSuit = handInfo.RanksPerSuit[aceSuit];
                        if (ranksWithAceSuit.Length == 3 && ranksWithAceSuit[1].InRange(2, 4) && ranksWithAceSuit[2].InRange(6, 8) && handInfo.RankCounts[13] == 1 && handInfo.RankCounts[11] + handInfo.RankCounts[12] == 1)
                        {
                            // Rule H: Unsuited JK and QK both beat Ace only if the remaining two cards both match the suit of the Ace; the lower of which is a 2, 3, or 4; and the higher of which is a 6, 7, or 8.
                            return Core.RuleResult.Fail("H");
                        }

                        return Core.RuleResult.Pass(1 << CardIndices.Where(i => handInfo.Cards[i].Rank == 1).First());
                        // return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 32. Suited TJ (rule I)
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(10) && kvp.Value.ContainsKey(11));
                    if (validSuits.Any())
                    {
                        var suitAndRanksLookup = validSuits.First();

                        // Rule I: Unsuited JK beats suited TJ if the latter has a 9 or a flush penalty card.
                        if (handInfo.RankCounts[13] == 1 && (suitAndRanksLookup.Value.Count() >= 3 || handInfo.RankCounts[9] == 1))
                        {
                            return Core.RuleResult.Fail("I");
                        }

                        return Core.RuleResult.Pass((1 << suitAndRanksLookup.Value[10]) + (1 << suitAndRanksLookup.Value[11]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 33. Unsuited JK or QK
                {
                    if (handInfo.RankCounts[13] == 1 && handInfo.RankCounts[11] + handInfo.RankCounts[12] == 1)
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank >= 11).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 34. 3 to a flush containing a 10 and King
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(10) && kvp.Value.ContainsKey(13) && kvp.Value.Count() == 3);
                    if (validSuits.Any())
                    {
                       return Core.RuleResult.Pass(validSuits.First().Value.Sum(kvp => 1 << kvp.Value));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 35. Suited TQ or TK
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(10) && kvp.Value.Keys.Max() >= 12);
                    if (validSuits.Any())
                    {
                        return Core.RuleResult.Pass(validSuits.First().Value.Where(kvp => kvp.Key >= 10).Sum(kvp => 1 << kvp.Value));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 36. Single Jack, Queen, or King
                {
                    if (handInfo.RankCounts.GetRange(11, 3).Contains(1))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank >= 11).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 37. 3 to a straight flush, type 3
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
                },
                (handInfo) => // 38. 4 to an inside straight (rule J: If there are two ways to play this hand as 4 to an inside straight, you may choose either one.)
                {
                    var validStrategies = new List<int>();

                    int total = handInfo.RankCounts.GetRange(1, 5).Sum();
                    for (int lowRank = 2; lowRank <= 6; lowRank++)
                    {
                        total += handInfo.RankCounts[lowRank + 4] - handInfo.RankCounts[lowRank - 1];
                        if (total == 4)
                        {
                            validStrategies.Add(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(lowRank, lowRank + 4)).Sum(i => 1 << i));
                        }
                    }

                    if (validStrategies.Count() == 2)
                    {
                        return Core.RuleResult.Pass("J", validStrategies.ToArray());
                    }
                    if (validStrategies.Count() == 1)
                    {
                        return Core.RuleResult.Pass(validStrategies[0]);
                    }
                    return Core.RuleResult.Fail();
                }
            );

            public static Core.Strategy StrategyTripleDoubleBonus = new Core.Strategy(
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
                (handInfo) => // 3. Four of a kind (rule A: If you have four Aces, 2s, 3s, or 4s, always go for the A-4 kicker if you don't have it already. If you have four 5s through Kings, you may either keep or discard the kicker.)
                {
                    var quads = handInfo.RankCounts.IndexOf(4);
                    if (quads > 0)
                    {
                        int kickerIndex = CardIndices.First(i => handInfo.Cards[i].Rank != quads);

                        if (quads <= 4)
                        {
                            if (handInfo.RankCounts.GetRange(1, 4).Sum() == 5)
                            {
                                return Core.RuleResult.Pass("A", 31);
                            }
                            else
                            {
                                return Core.RuleResult.Pass("A", 31 - (1 << kickerIndex));
                            }
                        }
                        else
                        {
                            return Core.RuleResult.Pass("A", 31 - (1 << kickerIndex), 31);
                        }
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
                (handInfo) => // 5. Three Aces, 2s, 3s, or 4s with A,2,3,4 kicker (rule B: If you have multiple kickers that qualify, keep only one of them.)
                {
                    var bonusRanks = handInfo.RankCounts.GetRange(1, 4);
                    if (bonusRanks.Contains(3) && bonusRanks.Sum() >= 4)
                    {
                        var rank = bonusRanks.IndexOf(3) + 1;
                        int baseStrategy = CardIndices.Where(i => handInfo.Cards[i].Rank == rank).Sum(i => 1 << i);
                        var kickerOnlyStrategies = CardIndices.Where(i => handInfo.Cards[i].Rank <= 4 && handInfo.Cards[i].Rank != rank).Select(i => 1 << i);
                        if (kickerOnlyStrategies.Count() >= 2)
                        {
                            return Core.RuleResult.Pass("B", kickerOnlyStrategies.Select(n => n + baseStrategy).ToArray());
                        }
                        return Core.RuleResult.Pass(baseStrategy + kickerOnlyStrategies.First());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 6. Three Aces, 2s, 3s, or 4s
                {
                    var bonusRanks = handInfo.RankCounts.GetRange(1, 4);
                    if (bonusRanks.Contains(3))
                    {
                        var rank = bonusRanks.IndexOf(3) + 1;
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == rank).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 7. Full house
                {
                    var trips = handInfo.RankCounts.IndexOf(3);
                    var pair = handInfo.RankCounts.IndexOf(2);
                    if (trips > 0 && pair > 0)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 8. Flush
                {
                    var flushes = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 5);
                    if (flushes.Any())
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 9. Straight
                {
                    var ranks = AllRanks.Where(r => handInfo.RankCounts[r] == 1).ToArray();
                    if (ranks.Length == 5 && (Enumerable.SequenceEqual(ranks, RoyalRanks) || ranks[4] - ranks[0] == 4))
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 10. Three 5s through Kings
                {
                    var trips = handInfo.RankCounts.IndexOf(3);
                    if (trips > 0)
                    {
                        var cardIndicesToKeep = CardIndices.Where(i => handInfo.Cards[i].Rank == trips);
                        return Core.RuleResult.Pass(cardIndicesToKeep.Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 11. 4 to a straight flush
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
                (handInfo) => // 12. Pair of Aces
                {
                    if (handInfo.RankCounts[1] == 2)
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == 1).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 13. Two pair
                {
                    if (handInfo.RankCounts.Count(n => n == 2) == 2)
                    {
                        var indexToDiscard = CardIndices.Where(i => handInfo.RankCounts[handInfo.Cards[i].Rank] == 1).First();
                        return Core.RuleResult.Pass(31 - (1 << indexToDiscard));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 14. 4 to a flush (rule C)
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.Count() == 4);
                    if (validSuits.Any())
                    {
                        var suitAndRanksLookup = validSuits.First();
                        var sortedFlushRanks = handInfo.RanksPerSuit[suitAndRanksLookup.Key];
                        if (sortedFlushRanks[1] == 10 && sortedFlushRanks[3] == 12)
                        {
                            var nonFlushCard = handInfo.Cards.First(card => card.Suit != suitAndRanksLookup.Key);
                            if (nonFlushCard.Rank.InRange(2, 7) || nonFlushCard.Rank == 10)
                            {
                                return Core.RuleResult.Fail("C");
                            }
                        }
                        return Core.RuleResult.Pass(validSuits.First().Value.Sum(kvp => 1 << kvp.Value));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 15. 3 to a royal flush (rule D: A pair of Jacks, Queens, or Kings beats 3 to a royal flush if the latter contains a 10 and an Ace.)
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.Keys.Intersect(RoyalRanks).Count() == 3);
                    if (validSuits.Any())
                    {
                        var suitAndRanksLookup = validSuits.First();
                        var suit = suitAndRanksLookup.Key;
                        var ranks = suitAndRanksLookup.Value.Keys.Intersect(RoyalRanks).OrderBy(n => n).ToArray();
                        if (ranks[1] == 10 && handInfo.RankCounts.GetRange(11, 3).Contains(2))
                        {
                            return Core.RuleResult.Fail("D");
                        }
                        return Core.RuleResult.Pass(ranks.Sum(r => 1 << handInfo.CardPositionMatrix[suit][r]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 16. Pair of 2s, 3s, 4s, Jacks, Queens, or Kings
                {
                    for (int u = 2; u <= 4; u++)
                    {
                        for (int rank = u; rank <= u + 9; rank += 9)
                        {
                            if (handInfo.RankCounts[rank] == 2)
                            {
                                return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == rank).Sum(i => 1 << i));
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 17. 4 to an open-ended straight (rule E: if there is also a pair of 5s through 10s, it doesn't matter which paired card you discard.)
                {
                    int runningTotal = handInfo.RankCounts.GetRange(1, 4).Sum(n => Math.Min(n, 1));
                    for (int lowRank = 2; lowRank <= 10; lowRank++)
                    {
                        runningTotal += Math.Min(handInfo.RankCounts[lowRank + 3], 1);
                        runningTotal -= Math.Min(handInfo.RankCounts[lowRank - 1], 1);
                        if (runningTotal == 4)
                        {
                            List<int>[] indexChoices = Enumerable.Range(0, 4).Select(i => new int[0].ToList()).ToArray();
                            for (int i = 0; i < 5; i++)
                            {
                                int arrayIndex = handInfo.Cards[i].Rank - lowRank;
                                if (arrayIndex.InRange(0, 3))
                                {
                                    indexChoices[arrayIndex].Add(i);
                                }
                            }

                            var strategies = new[] {0};

                            foreach (var choices in indexChoices)
                            {
                                strategies = choices.SelectMany(choice => strategies.Select(bitmask => bitmask + (1 << choice))).ToArray();
                            }

                            if (strategies.Length > 1)
                            {
                                return Core.RuleResult.Pass("E", strategies);
                            }
                            else
                            {
                                return Core.RuleResult.Pass(strategies);
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 18. 3 to a straight flush, type 1 (the number of high cards equals or exceeds the number of gaps, except Ace low or 2-3-4) (rule F: it's complicated, just read the manual)
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
                        if (highCount == 1)
                        {
                            if (handInfo.RankCounts[suitAndRanks.Value[0] - 1] == 2)
                            {
                                return Core.RuleResult.Fail("F");
                            }
                        }
                        if (highCount == 0)
                        {
                            int pairRank = handInfo.RankCounts.IndexOf(2);
                            if (pairRank > 0)
                            {
                                if (pairRank.InRange(suitAndRanks.Value[0], suitAndRanks.Value[2]) && handInfo.RankCounts.GetRange(suitAndRanks.Value[0] - 2, 7).Sum() == 5)
                                {
                                    return Core.RuleResult.Fail("F");
                                }
                                else if (suitAndRanks.Value[0] == pairRank + 2 || suitAndRanks.Value[2] == pairRank - 2)
                                {
                                    return Core.RuleResult.Fail("F");
                                }
                            }
                        }
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Values.Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 19. Pair of 5s through 10s
                {
                    for (int rank = 5; rank <= 10; rank++)
                    {
                        if (handInfo.RankCounts[rank] == 2)
                        {
                            return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == rank).Select(i => 1 << i).Sum());
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                // Below here, we can assume that all ranks are different.
                (handInfo) => // 20. Unsuited JQKA
                {
                    var ranks = new[] {1, 11, 12, 13};
                    if (ranks.All(rank => handInfo.RankCounts[rank] == 1))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank >= 11 || handInfo.Cards[i].Rank == 1).Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 21. 3 to a flush: two high cards (rule G: Suited JQ beats three to a flush (2JQ through 7JQ) if the other two cards have rank 7 or lower.)
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var suitAndRanks = validSuits.First();
                    var ranks = suitAndRanks.Value;

                    if ((ranks[0] == 1 && ranks[2] >= 11) || ranks[1] >= 11)
                    {
                        if (ranks[1] == 11 && ranks[2] == 12 && handInfo.RankCounts.GetRange(2, 6).Sum() == 3)
                        {
                            return Core.RuleResult.Fail("G");
                        }
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Sum(kvp => 1 << kvp.Value));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 22. Two suited high cards (rule H: If the remaining three cards form a straight flush draw of type 2B or 2C and they are all lower than the two suited high cards, keep the 3 to a straight flush instead.)
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Count(rank => rank == 1 || rank >= 11) == 2);
                    if (validSuits.Any())
                    {
                        var suit = validSuits.First().Key;
                        var highCards = new[] {11, 12, 13, 1}.Where(rank => handInfo.CardPositionMatrix[suit].ContainsKey(rank)).ToArray();

                        var potentialStraightFlushDraws = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                        if (potentialStraightFlushDraws.Any())
                        {
                            var otherCards = potentialStraightFlushDraws.First().Value;
                            if (otherCards[2] - otherCards[0] == 4 && otherCards[2].InRange(11, highCards[0] - 1))
                            {
                                return Core.RuleResult.Fail("H");
                            }
                        }

                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suit].Where(kvp => highCards.Contains(kvp.Key)).Sum(kvp => 1 << kvp.Value));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 23. 3 to a straight flush, type 2A or 2B (one gap with no high cards, Ace low, 234, 78J, 79J, or 7TJ)
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var suitAndRanks = validSuits.First();
                    int gapCount = suitAndRanks.Value[2] - suitAndRanks.Value[0] - 2;
                    int highCount = suitAndRanks.Value.Count(rank => rank >= 11);
                    if (gapCount <= 2 && (gapCount - highCount <= 1 || suitAndRanks.Value[0] == 1) && suitAndRanks.Value[0] <= 7)
                    {
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Values.Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 24. Unsuited 9JQK, TJQA, or TJKA
                {
                    var partialStraights = new[]
                    {
                        new[] {9, 11, 12, 13},
                        new[] {10, 11, 12, 1},
                        new[] {10, 11, 13, 1},
                        // new[] {1, 10, 12, 13},
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
                (handInfo) => // 25. 3 to a straight flush, type 2C (rule I: Unsuited TQKA beats suited 9TK.)
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var suitAndRanks = validSuits.First();
                    if (suitAndRanks.Value[2] - suitAndRanks.Value[0] == 4 && suitAndRanks.Value[0] >= 8)
                    {
                        if (suitAndRanks.Value[0] == 9 && handInfo.RankCounts[12] == 1 && handInfo.RankCounts[1] == 1)
                        {
                            return Core.RuleResult.Fail("I");
                        }
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Values.Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 26. Unsuited TQKA
                {
                    var ranks = new[] {1, 10, 12, 13};
                    if (ranks.All(rank => handInfo.RankCounts[rank] == 1))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => ranks.Contains(handInfo.Cards[i].Rank)).Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 27. Unsuited JQK
                {
                    if (handInfo.RankCounts.GetRange(11, 3).Sum() == 3)
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank >= 11).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 28. Unsuited 89JQ, 8TJQ, 9TJK, or 9TQK
                {
                    var partialStraights = new[]
                    {
                        new[] {8, 9, 11, 12},
                        new[] {8, 10, 11, 12},
                        new[] {9, 10, 11, 13},
                        new[] {9, 10, 12, 13}
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
                (handInfo) => // 29. Single Ace (rule J: some 3 to a flush draws beat a single Ace.)
                {
                    if (handInfo.RankCounts[1] == 0)
                    {
                        return Core.RuleResult.Fail();
                    }
                    var flushDraws = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (flushDraws.Any())
                    {
                        var flushRanks = flushDraws.First().Value;
                        if (flushRanks[0] == 1 && flushRanks[1] <= 4)
                        {
                            if (flushRanks[2] == 10 && handInfo.Cards.Count(card => card.Rank <= 4 || card.Rank >= 11) == 4)
                            {
                                return Core.RuleResult.Fail("J");
                            }
                            if (flushRanks[2].InRange(6, 9) && flushRanks[1].InRange(2, 4) && handInfo.RankCounts.GetRange(11, 3).Sum() == 2)
                            {
                                return Core.RuleResult.Fail("J");
                            }
                        }
                        else if (flushRanks[2] >= 11 && handInfo.RankCounts.GetRange(1, 4).Sum() == 4 && handInfo.Cards.Select(card => card.Suit).Distinct().Count() == 2)
                        {
                            return Core.RuleResult.Fail("J");
                        }
                    }
                    return Core.RuleResult.Pass(1 << CardIndices.First(i => handInfo.Cards[i].Rank == 1));
                },
                (handInfo) => // 30. 3 to a flush: 1 high card (rule K: suited TJ beats (2-5)(3-6)K)
                {
                    var flushDraws = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (flushDraws.Any())
                    {
                        var flushRanks = flushDraws.First().Value;
                        if (flushRanks[0] == 1 || flushRanks[2] >= 11)
                        {
                            if (flushRanks[2] == 13 && flushRanks[1] <= 6 && handInfo.RanksPerSuit.Values.Any(ranks => Enumerable.SequenceEqual(ranks, new[] {10, 11})))
                            {
                                return Core.RuleResult.Fail("K");
                            }
                            return Core.RuleResult.Pass(handInfo.CardPositionMatrix[flushDraws.First().Key].Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 31. Suited TJ
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(10) && kvp.Value.ContainsKey(11));
                    if (validSuits.Any())
                    {
                        var ranksLookup = validSuits.First().Value;
                        return Core.RuleResult.Pass((1 << ranksLookup[10]) | (1 << ranksLookup[11]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 32. Unsuited TJQ
                {
                    if (handInfo.RankCounts.GetRange(10, 3).All(n => n == 1))
                    {
                        if (handInfo.RanksPerSuit.Values.Any(ranks => ranks.Length == 3 && ranks[0] == 6 && ranks[1] == 7 && ranks[2] == 10))
                        {
                            return Core.RuleResult.Fail("L");
                        }
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(10, 12)).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 33. Unsuited JQ
                {
                    if (handInfo.RankCounts.GetRange(11, 2).All(n => n == 1))
                    {
                        if (handInfo.RanksPerSuit.Values.Any(ranks => ranks.Length == 3 && ranks[0].InRange(5, 6) && ranks[1] <= 7 && ranks[2] - ranks[0] == 4))
                        {
                            return Core.RuleResult.Fail("L");
                        }
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(11, 12)).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 34. 3 to a straight flush, type 3A
                {
                    var flushDraws = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (flushDraws.Any())
                    {
                        var flushRanks = flushDraws.First().Value;
                        if (flushRanks[2] - flushRanks[0] == 4 && flushRanks[0] >= 4)
                        {
                            return Core.RuleResult.Pass(handInfo.CardPositionMatrix[flushDraws.First().Key].Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 35. Suited TQ
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(10) && kvp.Value.ContainsKey(12));
                    if (validSuits.Any())
                    {
                        var ranksLookup = validSuits.First().Value;
                        return Core.RuleResult.Pass((1 << ranksLookup[10]) | (1 << ranksLookup[12]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 36. 3 to a straight flush, type 3B
                {
                    var flushDraws = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (flushDraws.Any())
                    {
                        var flushRanks = flushDraws.First().Value;
                        if (flushRanks[2] - flushRanks[0] == 4 && flushRanks[0] <= 3)
                        {
                            return Core.RuleResult.Pass(handInfo.CardPositionMatrix[flushDraws.First().Key].Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 37. Unsuited JK or QK
                {
                    if (handInfo.RankCounts[13] == 1 && handInfo.RankCounts[11] + handInfo.RankCounts[12] == 1)
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank >= 11).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 38. Suited TK
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(10) && kvp.Value.ContainsKey(13));
                    if (validSuits.Any())
                    {
                        var ranksLookup = validSuits.First().Value;
                        return Core.RuleResult.Pass((1 << ranksLookup[10]) | (1 << ranksLookup[13]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 39. Single Jack, Queen, or King
                {
                    if (handInfo.Cards.Any(card => card.Rank >= 11))
                    {
                        return Core.RuleResult.Pass(1 << CardIndices.First(i => handInfo.Cards[i].Rank >= 11));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 40. 3 to a flush: 0 high cards
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.Count() == 3);
                    if (validSuits.Any())
                    {
                        return Core.RuleResult.Pass(validSuits.First().Value.Sum(kvp => 1 << kvp.Value));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 41. 4 to an inside straight (rule M: If there are two ways to play this hand as 4 to an inside straight, you may choose either one.)
                {
                    var validStrategies = new List<int>();

                    int total = handInfo.RankCounts.GetRange(1, 5).Sum();
                    for (int lowRank = 2; lowRank <= 6; lowRank++)
                    {
                        total += handInfo.RankCounts[lowRank + 4] - handInfo.RankCounts[lowRank - 1];
                        if (total == 4)
                        {
                            validStrategies.Add(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(lowRank, lowRank + 4)).Sum(i => 1 << i));
                        }
                    }

                    if (validStrategies.Count() == 2)
                    {
                        return Core.RuleResult.Pass("M", validStrategies.ToArray());
                    }
                    if (validStrategies.Count() == 1)
                    {
                        return Core.RuleResult.Pass(validStrategies[0]);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 42. Suited 34 (rule N: If the hand contains a 2 and a 7, or if it contains a 5, discard everything instead.)
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => Enumerable.SequenceEqual(kvp.Value, new[] {3, 4}));
                    if (validSuits.Any())
                    {
                        if ((handInfo.RankCounts[2] == 1 && handInfo.RankCounts[7] == 1) || handInfo.RankCounts[5] == 1)
                        {
                            return Core.RuleResult.Fail("N");
                        }
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[validSuits.First().Key].Sum(kvp => 1 << kvp.Value));
                    }
                    return Core.RuleResult.Fail();
                }
            );
        }
    }
}