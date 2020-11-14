using System;
using System.Collections.Generic;
using System.Linq;

namespace KtaneVideoPoker
{
    namespace Variants
    {
        public class DeucesWild: IVariant
        {
            public static DeucesWild NotSoUglyDucks(bool bettingFiveCredits = true)
            {
                return new DeucesWild(
                    bettingFiveCredits ? 800 : 250,
                    200, 25, 16, 10,
                    4, 4, 3, 2, 1
                );
            }

            public static DeucesWild LooseDeuces(bool bettingFiveCredits = true)
            {
                return new DeucesWild(
                    bettingFiveCredits ? 800 : 250,
                    500, 25, 15, 8,
                    4, 3, 2, 2, 1
                );
            }

            public static DeucesWild FullPayDeucesWild(bool bettingFiveCredits = true)
            {
                return new DeucesWild(
                    bettingFiveCredits ? 800 : 250,
                    200, 25, 15, 9,
                    5, 3, 2, 2, 1
                );
            }

            public int JokerCount { get { return 0; }}

            private int[] PayoutArray;

            private DeucesWild(params int[] payoutArray)
            {
                PayoutArray = payoutArray;
            }

            public Core.HandResult Evaluate(Core.Hand hand)
            {
                var newHand = new Core.Hand(hand.Cards.Select(card => card.Rank == 2 ? new Core.Card((int)card.Suit) : card));

                int straightCard = newHand.GetHighestStraightCard();
                bool isFlushLike = newHand.IsFlushLike();
                int matches = newHand.GetPairwiseMatchCount();
                int jokerCount = newHand.GetJokerCount();

                if (straightCard == 1 && isFlushLike && jokerCount == 0)
                {
                    return Core.HandResult.NaturalRoyalFlush;
                }
                else if (jokerCount == 4)
                {
                    return Core.HandResult.FourDeuces;
                }
                else if (straightCard == 1 && isFlushLike)
                {
                    return Core.HandResult.WildRoyalFlush;
                }
                else if (matches == new[] {10, 6, 3, 1}[jokerCount])
                {
                    return Core.HandResult.FiveOfAKind;
                }
                else if (straightCard != 0 && isFlushLike)
                {
                    return Core.HandResult.StraightFlush;
                }
                else if (matches == new[] {6, 3, 1, 0}[jokerCount])
                {
                    return Core.HandResult.FourOfAKind;
                }
                else if (matches == new[] {4, 2, -1, -1}[jokerCount])
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
                else if (matches == new[] {3, 1, 0, -1}[jokerCount])
                {
                    return Core.HandResult.ThreeOfAKind;
                }
                else
                {
                    return Core.HandResult.Nothing;
                }
            }

            public Core.HandResult[] HandTypes()
            {
                return new[]
                {
                    Core.HandResult.NaturalRoyalFlush,
                    Core.HandResult.FourDeuces,
                    Core.HandResult.WildRoyalFlush,
                    Core.HandResult.FiveOfAKind,
                    Core.HandResult.StraightFlush,
                    Core.HandResult.FourOfAKind,
                    Core.HandResult.FullHouse,
                    Core.HandResult.Flush,
                    Core.HandResult.Straight,
                    Core.HandResult.ThreeOfAKind
                };
            }

            public int PayoutForResult(Core.HandResult result)
            {
                int index = HandTypes().ToList().IndexOf(result);
                return index == -1 ? 0 : PayoutArray[index];
            }

            // STRATEGIES

            private static IEnumerable<int> AllRanks = Enumerable.Range(1, 13);
            private static IEnumerable<int> CardIndices = Enumerable.Range(0, 5);
            private static int[] RoyalRanks = new[] {1, 10, 11, 12, 13};

            public static Core.Strategy StrategyNotSoUglyDucks = new Core.DeucesWildStrategy(0,
                (handInfo) => // 1. [0W] Natural royal flush
                {
                    if (handInfo.WildCount == 0 && handInfo.RanksPerSuit.Values.Any(ranks => Enumerable.SequenceEqual(ranks, RoyalRanks)))
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 2. [0W] 4 to a natural royal flush
                {
                    if (handInfo.WildCount == 0)
                    {
                        var validSuits = handInfo.CardPositionMatrix.Where(kvp => RoyalRanks.Count(rank => kvp.Value.ContainsKey(rank)) == 4);
                        if (validSuits.Any())
                        {
                            var ranksLookup = validSuits.First().Value;
                            var ranks = ranksLookup.Keys.Intersect(RoyalRanks);
                            return Core.RuleResult.Pass(ranks.Sum(rank => 1 << ranksLookup[rank]));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 3. [0W] Straight flush
                {
                    if (handInfo.WildCount == 0)
                    {
                        var validSuits = handInfo.RanksPerSuit.Values.Where(ranks => ranks.Length == 5);
                        if (validSuits.Any())
                        {
                            var ranks = validSuits.First();
                            if (ranks[4] - ranks[0] == 4)
                            {
                                return Core.RuleResult.Pass(31);
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 4. [0W] Four of a kind
                {
                    var quads = handInfo.RankCounts.IndexOf(4);
                    if (quads > 0 && handInfo.WildCount == 0)
                    {
                        var indexToDiscard = CardIndices.First(i => handInfo.Cards[i].Rank != quads);
                        return Core.RuleResult.Pass(31 - (1 << indexToDiscard));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 5. [0W] Full house
                {
                    var trips = handInfo.RankCounts.IndexOf(3);
                    var pair = handInfo.RankCounts.IndexOf(2);
                    if (trips > 0 && pair > 0 && handInfo.WildCount == 0)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 6. [0W] Flush
                {
                    var flushes = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 5);
                    if (flushes.Any() && handInfo.WildCount == 0)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 7. [0W] Straight
                {
                    var ranks = AllRanks.Where(r => handInfo.RankCounts[r] == 1).ToArray();
                    if (ranks.Length == 5 && (Enumerable.SequenceEqual(ranks, RoyalRanks) || ranks[4] - ranks[0] == 4) && handInfo.WildCount == 0)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 8. [0W] Three of a kind
                {
                    var trips = handInfo.RankCounts.IndexOf(3);
                    if (trips > 0 && handInfo.WildCount == 0)
                    {
                        var cardIndicesToKeep = CardIndices.Where(i => handInfo.Cards[i].Rank == trips);
                        return Core.RuleResult.Pass(cardIndicesToKeep.Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 9. [0W] 4 to a straight flush
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Count() == 4);
                    if (!validSuits.Any() || handInfo.WildCount > 0)
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
                (handInfo) => // 10. [0W] 3 to a natural royal flush
                {
                    if (handInfo.WildCount == 0)
                    {
                        var validSuits = handInfo.CardPositionMatrix.Where(kvp => RoyalRanks.Count(rank => kvp.Value.ContainsKey(rank)) == 3);
                        if (validSuits.Any())
                        {
                            var ranksLookup = validSuits.First().Value;
                            var ranks = ranksLookup.Keys.Intersect(RoyalRanks);
                            return Core.RuleResult.Pass(ranks.Sum(rank => 1 << ranksLookup[rank]));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 11. [0W] 4 to a flush
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Count() == 4);
                    if (validSuits.Any() && handInfo.WildCount == 0)
                    {
                        var suit = validSuits.First().Key;
                        var indexToDiscard = CardIndices.First(i => handInfo.Cards[i].Suit != suit);
                        return Core.RuleResult.Pass(31 - (1 << indexToDiscard));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 12. [0W] Two pair
                {
                    if (handInfo.RankCounts.Count(n => n == 2) == 2 && handInfo.WildCount == 0)
                    {
                        var indexToDiscard = CardIndices.First(i => handInfo.RankCounts[handInfo.Cards[i].Rank] == 1);
                        return Core.RuleResult.Pass(31 - (1 << indexToDiscard));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 13. [0W] Suited 567, 678, 789, 89T, or 9TJ
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.Count() == 3);
                    if (validSuits.Any() && handInfo.WildCount == 0)
                    {
                        var ranksLookup = validSuits.First().Value;
                        int max = ranksLookup.Keys.Max();
                        int min = ranksLookup.Keys.Min();
                        if (max - min == 2 && min >= 5)
                        {
                            return Core.RuleResult.Pass(ranksLookup.Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 14. [0W] One pair
                {
                    int pairRank = handInfo.RankCounts.IndexOf(2);
                    if (pairRank >= 0 && handInfo.WildCount == 0)
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == pairRank).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 15. [0W] 4 to an open-ended straight
                {
                    if (handInfo.WildCount > 0)
                    {
                        return Core.RuleResult.Fail();
                    }
                    int rankCount = handInfo.RankCounts.GetRange(3, 4).Sum();
                    for (int minRank = 4; minRank <= 10; minRank++)
                    {
                        rankCount += handInfo.RankCounts[minRank + 3] - handInfo.RankCounts[minRank - 1];
                        if (rankCount == 4)
                        {
                            return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(minRank, minRank + 3)).Select(i => 1 << i).Sum());
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 16. [0W] 3 to a straight flush, except Ace low
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.Count() == 3);
                    if (validSuits.Any() && handInfo.WildCount == 0)
                    {
                        var ranksLookup = validSuits.First().Value;
                        int max = ranksLookup.Keys.Max();
                        int min = ranksLookup.Keys.Min();
                        if (max - min <= 4 && min != 1)
                        {
                            return Core.RuleResult.Pass(ranksLookup.Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 17. [0W] Suited TJ, TQ, or JQ
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => Enumerable.Range(10, 3).Count(rank => kvp.Value.ContainsKey(rank)) == 2);
                    if (validSuits.Any() && handInfo.WildCount == 0)
                    {
                        var ranksLookup = validSuits.First().Value;
                        return Core.RuleResult.Pass(ranksLookup.Where(kvp => kvp.Key.InRange(10, 12)).Sum(kvp => 1 << kvp.Value));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 18. [0W] 4 to a straight (rules A and B)
                {
                    if (handInfo.WildCount > 0)
                    {
                        return Core.RuleResult.Fail();
                    }

                    var validStrategies = new List<int>();

                    int total = handInfo.RankCounts.GetRange(2, 5).Sum();
                    for (int lowRank = 3; lowRank <= 9; lowRank++)
                    {
                        total += handInfo.RankCounts[lowRank + 4] - handInfo.RankCounts[lowRank - 1];
                        if (total == 4)
                        {
                            if (lowRank == 3 && handInfo.RanksPerSuit.Values.Any(ranks => ranks.Length == 3 && ranks[0] == 1 && ranks[1] == 3 && ranks[2] <= 5) && handInfo.RankCounts[6] + handInfo.RankCounts[7] == 2)
                            {
                                return Core.RuleResult.Fail("B");
                            }
                            if (lowRank == 4 && handInfo.RanksPerSuit.Values.Any(ranks => Enumerable.SequenceEqual(ranks, new[] {1, 4, 5})))
                            {
                                return Core.RuleResult.Fail("B");
                            }
                            if (lowRank == 6 || (lowRank == 7 && handInfo.RankCounts[10] == 0))
                            {
                                var highCard = handInfo.Cards.First(card => card.Rank == lowRank + 4);
                                if (Enumerable.SequenceEqual(handInfo.RanksPerSuit[highCard.Suit], new[] {highCard.Rank, 13}))
                                {
                                    return Core.RuleResult.Fail("B");
                                }
                            }
                            validStrategies.Add(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(lowRank, lowRank + 4)).Sum(i => 1 << i));
                        }
                    }

                    if (new[] {10, 11, 12, 13, 1}.Count(rank => handInfo.RankCounts[rank] == 1) == 4)
                    {
                        validStrategies.Add(CardIndices.Where(i => handInfo.Cards[i].Rank == 1 || handInfo.Cards[i].Rank >= 10).Sum(i => 1 << i));
                    }

                    // Rule A: If there are two ways to play this hand as 4 to an inside straight, you may choose either one.
                    if (validStrategies.Count() == 2)
                    {
                        return Core.RuleResult.Pass("A", validStrategies.ToArray());
                    }
                    if (validStrategies.Count() == 1)
                    {
                        return Core.RuleResult.Pass(validStrategies[0]);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 19. [0W] Suited A34, A35, or A45
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.Count() == 3);
                    if (validSuits.Any() && handInfo.WildCount == 0)
                    {
                        var ranksLookup = validSuits.First().Value;
                        int max = ranksLookup.Keys.Max();
                        int min = ranksLookup.Keys.Min();
                        if (max <= 5 && min == 1)
                        {
                            return Core.RuleResult.Pass(ranksLookup.Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 20. [0W] Suited TK, JK, or QK (rule C: It gets weird)
                {
                    if (handInfo.WildCount > 0)
                    {
                        return Core.RuleResult.Fail();
                    }

                    // Keys are a single number whose hex digits are the ranks of the flush draw in order
                    // Value arrays contain arrays of length 3. The first two are the ranks to find,
                    // and the last is equal to 1 if the cards must be suited, 0 if it's optional.
                    var invalidOthers = new Dictionary<int, int[][]>
                    {
                        {
                            0x3AD, new[]
                            {
                                new[] {4, 11, 0},
                                new[] {4, 12, 0},
                                new[] {4, 1, 0},
                                new[] {5, 12, 0},
                                new[] {6, 12, 1},
                                new[] {9, 1, 0}
                            }
                        },
                        {
                            0x4AD, new[]
                            {
                                new[] {3, 11, 0},
                                new[] {3, 12, 0},
                                new[] {3, 1, 0},
                                new[] {9, 1, 0},
                            }
                        },
                        {
                            0x5AD, new[]
                            {
                                new[] {3, 12, 0},
                                new[] {9, 1, 0}
                            }
                        },
                        {
                            0x6AD, new[]
                            {
                                new[] {3, 12, 1},
                                new[] {9, 1, 1}
                            }
                        },
                        {
                            0x3BD, new[]
                            {
                                new[] {4, 10, 0},
                                new[] {4, 12, 0},
                                new[] {4, 1, 0},
                                new[] {5, 12, 0},
                                new[] {5, 1, 0},
                                new[] {6, 12, 0},
                                new[] {7, 12, 1},
                                new[] {8, 12, 1},
                                new[] {9, 1, 0}
                            }
                        },
                        {
                            0x4BD, new[]
                            {
                                new[] {3, 10, 0},
                                new[] {3, 12, 0},
                                new[] {3, 1, 0},
                                new[] {5, 12, 0},
                                new[] {6, 12, 1},
                                new[] {9, 1, 0}
                            }
                        },
                        {
                            0x5BD, new[]
                            {
                                new[] {3, 12, 0},
                                new[] {3, 1, 0},
                                new[] {4, 12, 0},
                                new[] {9, 1, 0}
                            }
                        },
                        {
                            0x6BD, new[]
                            {
                                new[] {3, 12, 0},
                                new[] {4, 12, 1},
                                new[] {9, 1, 0}
                            }
                        },
                        {
                            0x7BD, new[]
                            {
                                new[] {3, 12, 1},
                                new[] {9, 1, 1}
                            }
                        },
                        {
                            0x8BD, new[]
                            {
                                new[] {3, 12, 0},
                                new[] {9, 1, 0}
                            }
                        },
                        {
                            0x3CD, new[]
                            {
                                new[] {4, 10, 0},
                                new[] {4, 11, 0},
                                new[] {4, 1, 0},
                                new[] {5, 10, 0},
                                new[] {5, 11, 0},
                                new[] {5, 1, 0},
                                new[] {6, 10, 1},
                                new[] {6, 11, 0},
                                new[] {6, 1, 0},
                                new[] {7, 11, 1},
                                new[] {8, 11, 1},
                                new[] {9, 1, 0}
                            }
                        },
                        {
                            0x4CD, new[]
                            {
                                new[] {3, 10, 0},
                                new[] {3, 11, 0},
                                new[] {3, 1, 0},
                                new[] {5, 11, 0},
                                new[] {5, 1, 0},
                                new[] {6, 11, 1},
                                new[] {6, 1, 1},
                                new[] {9, 1, 0}
                            }
                        },
                        {
                            0x5CD, new[]
                            {
                                new[] {3, 10, 0},
                                new[] {3, 11, 0},
                                new[] {3, 1, 0},
                                new[] {4, 11, 0},
                                new[] {4, 1, 0},
                                new[] {9, 1, 0}
                            }
                        },
                        {
                            0x6CD, new[]
                            {
                                new[] {3, 10, 1},
                                new[] {3, 11, 0},
                                new[] {3, 1, 0},
                                new[] {4, 11, 1},
                                new[] {4, 1, 1},
                                new[] {9, 1, 0}
                            }
                        },
                        {
                            0x7CD, new[]
                            {
                                new[] {3, 11, 1},
                                new[] {9, 1, 0}
                            }
                        },
                        {
                            0x8CD, new[]
                            {
                                new[] {3, 11, 0},
                                new[] {9, 1, 0}
                            }
                        }
                    };

                    if (handInfo.RankCounts[13] == 1)
                    {
                        var kingSuit = handInfo.Cards.First(card => card.Rank == 13).Suit;
                        if (handInfo.CardPositionMatrix[kingSuit].Keys.Any(rank => rank.InRange(10, 12)))
                        {
                            var suitedRanks = handInfo.RanksPerSuit[kingSuit];
                            if (suitedRanks.Length == 3)
                            {
                                int key = 0;
                                foreach (int rank in suitedRanks)
                                {
                                    key = 16 * key + rank;
                                }
                                if (invalidOthers.ContainsKey(key))
                                {
                                    foreach (var invalidCase in invalidOthers[key])
                                    {
                                        var ranksNeededToFail = invalidCase.Take(2);
                                        bool requiresSuitMatch = invalidCase.Last() == 1;
                                        var cardsNeededToFail = handInfo.Cards.Where(card => ranksNeededToFail.Contains(card.Rank));
                                        if (cardsNeededToFail.Count() == 2 && (!requiresSuitMatch || cardsNeededToFail.Select(card => card.Suit).Distinct().Count() == 1))
                                        {
                                            return Core.RuleResult.Fail("C");
                                        }
                                    }
                                }
                            }
                            return Core.RuleResult.Pass(handInfo.CardPositionMatrix[kingSuit].Where(kvp => kvp.Key >= 10).Sum(kvp => 1 << kvp.Value));
                        }
                    }

                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 21. [0W] Suited 67, 78, 89, 9T, or TA (rule D: It gets weird)
                {
                    if (handInfo.WildCount > 0)
                    {
                        return Core.RuleResult.Fail();
                    }

                    // Keys are the lower rank in the straight/royal flush draw.
                    // Value arrays contain arrays of length 4. The first three are the ranks to find,
                    // and the last's absolute value is the minimum number of suits needed.
                    // Negative numbers have the added stipulation that the highest two cards are not suited.
                    var validOthers = new Dictionary<int, int[][]>
                    {
                        {
                            6, new[]
                            {
                                new[] {3, 11, 12, 3},
                                new[] {3, 11, 13, 3},
                                new[] {9, 11, 12, 3},
                                new[] {9, 11, 13, 3},
                                new[] {9, 11, 1, 4},
                                new[] {10, 11, 12, 4},
                                new[] {10, 11, 13, 4},
                                new[] {10, 11, 1, 3},
                                new[] {10, 12, 13, 4},
                                new[] {10, 12, 1, 3},
                                new[] {10, 13, 1, 3},
                                new[] {11, 12, 13, 4},
                                new[] {11, 12, 1, 3},
                                new[] {11, 13, 1, 3},
                                new[] {12, 13, 1, 3}
                            }
                        },
                        {
                            7, new[]
                            {
                                new[] {3, 10, 12, 4},
                                new[] {3, 11, 12, 3},
                                new[] {3, 11, 13, 3},
                                new[] {3, 11, 1, -3},
                                new[] {3, 12, 13, 3},
                                new[] {3, 12, 1, 2},
                                new[] {3, 13, 1, 2},
                                new[] {4, 12, 13, 3},
                                new[] {4, 12, 1, -3},
                                new[] {4, 13, 1, 4},
                                new[] {10, 12, 13, 4},
                                new[] {10, 12, 1, 4},
                                new[] {10, 13, 1, 4},
                                new[] {11, 12, 13, 4},
                                new[] {11, 12, 1, 3},
                                new[] {11, 13, 1, 3},
                                new[] {12, 13, 1, 3}
                            }
                        },
                        {
                            8, new[]
                            {
                                new[] {3, 4, 12, 4},
                                new[] {3, 4, 13, 2},
                                new[] {3, 4, 1, 3},
                                new[] {3, 5, 13, 4},
                                new[] {3, 12, 13, 4},
                                new[] {3, 12, 1, 4},
                                new[] {3, 13, 1, 2},
                                new[] {4, 5, 13, 3},
                                new[] {4, 5, 1, 4},
                                new[] {4, 12, 13, 3},
                                new[] {4, 12, 1, -3},
                                new[] {4, 13, 1, 2},
                                new[] {5, 13, 1, -3},
                                new[] {12, 13, 1, 4}
                            }
                        },
                        {
                            9, new[]
                            {
                                new[] {3, 4, 5, 3},
                                new[] {3, 4, 6, 3},
                                new[] {3, 4, 1, 3},
                                new[] {3, 5, 6, 3},
                                new[] {3, 5, 13, 3},
                                new[] {3, 5, 1, 3},
                                new[] {3, 6, 1, 3},
                                new[] {4, 5, 6, 3},
                                new[] {4, 5, 7, 4},
                                new[] {4, 5, 13, 2},
                                new[] {4, 5, 1, 3},
                                new[] {4, 6, 1, 2},
                                new[] {4, 13, 1, -3},
                                new[] {5, 6, 1, 2},
                                new[] {5, 7, 1, 3},
                                new[] {5, 13, 1, 3}
                            }
                        },
                        {
                            1, new[]
                            {
                                new[] {5, 7, 9, 4}
                            }
                        }
                    };

                    foreach (int r1 in validOthers.Keys)
                    {
                        int r2 = (r1 == 1) ? 10 : r1 + 1;
                        var validDraws = handInfo.RanksPerSuit.Where(kvp => Enumerable.SequenceEqual(kvp.Value, new[] {r1, r2}));
                        if (validDraws.Any())
                        {
                            var suit = validDraws.First().Key;
                            foreach (var candidates in validOthers[r1])
                            {
                                var neededRanks = candidates.Take(3);
                                var minimumDistinctSuits = Math.Abs(candidates.Last());
                                bool highestTwoCardsMustHaveDistinctSuits = candidates.Last() < 0;
                                if (neededRanks.All(rank => handInfo.RankCounts[rank] == 1) && handInfo.Cards.Select(card => card.Suit).Distinct().Count() >= minimumDistinctSuits && (!highestTwoCardsMustHaveDistinctSuits || handInfo.Cards.Where(card => card.Rank == 1 || card.Rank >= 11).Select(card => card.Suit).Distinct().Count() == 2))
                                {
                                    return Core.RuleResult.Pass("D", CardIndices.Where(i => handInfo.Cards[i].Suit == suit).Sum(i => 1 << i));
                                }
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 22. [1W] Wild royal flush
                {
                    if (handInfo.WildCount == 1 && handInfo.RanksPerSuit.Values.Any(array => array.Length == 4) && handInfo.RankCounts[1] + handInfo.RankCounts.GetRange(10, 4).Sum() == 4)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 23. [1W] Five of a kind
                {
                    if (handInfo.WildCount == 1 && handInfo.RankCounts.Contains(4))
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 24. [1W] Straight flush
                {
                    if (handInfo.WildCount == 1)
                    {
                        var valid = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 4);
                        if (valid.Any())
                        {
                            var ranks = valid.First().Value;
                            if (ranks.Last() - ranks.First() <= 4)
                            {
                                return Core.RuleResult.Pass(31);
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 25. [1W] Four of a kind
                {
                    if (handInfo.WildCount == 1 && handInfo.RankCounts.Contains(3))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].IsJoker || handInfo.RankCounts[handInfo.Cards[i].Rank] == 3).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 26. [1W] 4 to a wild royal flush
                {
                    if (handInfo.WildCount == 1)
                    {
                        var validRanks = handInfo.CardPositionMatrix.Where(kvp => RoyalRanks.Count(rank => kvp.Value.ContainsKey(rank)) == 3);
                        if (validRanks.Any())
                        {
                            var lookup = validRanks.First().Value;
                            return Core.RuleResult.Pass(handInfo.WildPositions.Sum(i => 1 << i) + lookup.Where(kvp => kvp.Key == 1 || kvp.Key >= 10).Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 27. [1W] Full house
                {
                    if (handInfo.WildCount == 1 && handInfo.RankCounts.Count(n => n == 2) == 2)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 28. [1W] Flush
                {
                    if (handInfo.WildCount == 1 && handInfo.RanksPerSuit.Values.Any(ranks => ranks.Length == 4))
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 29. [1W] 4 to a straight flush: 0 or 1 gaps, except Ace low, W346, and W356
                {
                    if (handInfo.WildCount == 1)
                    {
                        var valid = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                        if (valid.Any())
                        {
                            var suitAndRanks = valid.First();
                            var ranks = suitAndRanks.Value;
                            if (ranks[0] >= 3 && (ranks[2] - ranks[0] == 2 || (ranks[2] - ranks[0] == 3 && ranks[0] >= 4)))
                            {
                                return Core.RuleResult.Pass(31 - (1 << CardIndices.First(i => !handInfo.Cards[i].IsJoker && handInfo.Cards[i].Suit != suitAndRanks.Key)));
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 30. [1W] Straight
                {
                    var ranks = AllRanks.Where(r => handInfo.RankCounts[r] == 1).ToArray();
                    if (ranks.Length == 4 && (ranks.Intersect(RoyalRanks).Count() == 4 || ranks[3] - ranks[0] <= 4) && handInfo.WildCount == 1)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 31. [1W] 4 to a straight flush: W346, W356, or 2 gaps (except Ace low)
                {
                    if (handInfo.WildCount == 1)
                    {
                        var valid = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                        if (valid.Any())
                        {
                            var suitAndRanks = valid.First();
                            var ranks = suitAndRanks.Value;
                            if (ranks[0] >= 3 && ranks[2] - ranks[0] <= 4)
                            {
                                return Core.RuleResult.Pass(31 - (1 << CardIndices.First(i => !handInfo.Cards[i].IsJoker && handInfo.Cards[i].Suit != suitAndRanks.Key)));
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 32. [1W] Three of a kind
                {
                    if (handInfo.WildCount == 1 && handInfo.RankCounts.Contains(2))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].IsJoker || handInfo.RankCounts[handInfo.Cards[i].Rank] == 2).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 33. [1W] 4 to a straight flush: Ace low
                {
                    if (handInfo.WildCount == 1)
                    {
                        var valid = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                        if (valid.Any())
                        {
                            var suitAndRanks = valid.First();
                            var ranks = suitAndRanks.Value;
                            if (ranks[2] <= 5)
                            {
                                return Core.RuleResult.Pass(31 - (1 << CardIndices.First(i => !handInfo.Cards[i].IsJoker && handInfo.Cards[i].Suit != suitAndRanks.Key)));
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 34. [1W] 3 to a wild royal flush: WTJ, WTQ, WTK, WJQ, WJK, or WQK
                {
                    if (handInfo.WildCount == 1)
                    {
                        var validRanks = handInfo.CardPositionMatrix.Where(kvp => Enumerable.Range(10, 4).Count(rank => kvp.Value.ContainsKey(rank)) == 2);
                        if (validRanks.Any())
                        {
                            var lookup = validRanks.First().Value;
                            return Core.RuleResult.Pass(handInfo.WildPositions.Sum(i => 1 << i) + lookup.Where(kvp => kvp.Key >= 10).Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 35. [1W] 3 to a straight flush: W67, W78, W89, or W9T
                {
                    if (handInfo.WildCount == 1)
                    {
                        for (int r = 6; r <= 9; r++)
                        {
                            var validRanks = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(r) && kvp.Value.ContainsKey(r + 1));
                            if (validRanks.Any())
                            {
                                var lookup = validRanks.First().Value;
                                return Core.RuleResult.Pass(handInfo.WildPositions.Sum(i => 1 << i) + lookup.Where(kvp => kvp.Key.InRange(r, r + 1)).Sum(kvp => 1 << kvp.Value));
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 36. [1W] 3 to a wild royal flush: WTA, WJA, or WQA (rule E: W8T > WTA if the fifth card is a King)
                {
                    if (handInfo.WildCount == 1)
                    {
                        var validRanks = handInfo.CardPositionMatrix.Where(kvp => Enumerable.Range(10, 3).Any(rank => kvp.Value.ContainsKey(rank)) && kvp.Value.ContainsKey(1));
                        if (validRanks.Any())
                        {
                            var lookup = validRanks.First().Value;
                            if (lookup.ContainsKey(10) && lookup.ContainsKey(8) && handInfo.RankCounts[13] == 1)
                            {
                                return Core.RuleResult.Fail("E");
                            }
                            return Core.RuleResult.Pass(handInfo.WildPositions.Sum(i => 1 << i) + lookup.Where(kvp => kvp.Key.InRange(10, 12) || kvp.Key == 1).Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 37. [1W] 3 to a straight flush: W8T (rule F: W57 == W8T)
                {
                    if (handInfo.WildCount == 1)
                    {
                        var validRanks = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(8) && kvp.Value.ContainsKey(10));
                        if (validRanks.Any())
                        {
                            var lookup = validRanks.First().Value;
                            int strategy = handInfo.WildPositions.Sum(i => 1 << i) + lookup.Where(kvp => kvp.Key == 8 || kvp.Key == 10).Sum(kvp => 1 << kvp.Value);
                            if (handInfo.RanksPerSuit.Any(kvp => Enumerable.SequenceEqual(kvp.Value, new[] {5, 7})))
                            {
                                int otherStrategy = 31 + (1 << handInfo.WildPositions[0]) - strategy;
                                return Core.RuleResult.Pass("F", strategy, otherStrategy);
                            }
                            return Core.RuleResult.Pass(handInfo.WildPositions.Sum(i => 1 << i) + lookup.Where(kvp => kvp.Key == 8 || kvp.Key == 10).Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 38. [1W] 3 to a wild royal flush: WKA
                {
                    if (handInfo.WildCount == 1)
                    {
                        var validRanks = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(1) && kvp.Value.ContainsKey(13));
                        if (validRanks.Any())
                        {
                            var lookup = validRanks.First().Value;
                            return Core.RuleResult.Pass(handInfo.WildPositions.Sum(i => 1 << i) + lookup.Where(kvp => kvp.Key == 1 || kvp.Key == 13).Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 39. [1W] 3 to a straight flush: W57, W68, W79, W9J (rule G: If multiple draws are possible, then either is acceptable.)
                {
                    if (handInfo.WildCount == 1)
                    {
                        var strategies = new List<int>();
                        for (int r = 5; r <= 9; r++)
                        {
                            if (r == 8)
                            {
                                continue;
                            }
                            var validRanks = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(r) && kvp.Value.ContainsKey(r + 2));
                            if (validRanks.Any())
                            {
                                var lookup = validRanks.First().Value;
                                int strategy = handInfo.WildPositions.Sum(i => 1 << i) + lookup.Where(kvp => kvp.Key == r || kvp.Key == r + 2).Sum(kvp => 1 << kvp.Value);
                                strategies.Add(strategy);
                            }
                        }
                        if (strategies.Count() == 1)
                        {
                            return Core.RuleResult.Pass(strategies[0]);
                        }
                        if (strategies.Count() > 1)
                        {
                            return Core.RuleResult.Pass("G", strategies.ToArray());
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 40. [1W] 3 to a straight flush: W56
                {
                    if (handInfo.WildCount == 1)
                    {
                        var validRanks = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(5) && kvp.Value.ContainsKey(6));
                        if (validRanks.Any())
                        {
                            var lookup = validRanks.First().Value;
                            return Core.RuleResult.Pass(handInfo.WildPositions.Sum(i => 1 << i) + lookup.Where(kvp => kvp.Key.InRange(5, 6)).Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 41. [1W] 3 to a straight flush: W45 (rule H: It gets weird)
                {
                    if (handInfo.WildCount == 1)
                    {
                        var validRanks = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(4) && kvp.Value.ContainsKey(5));
                        if (validRanks.Any())
                        {
                            var suitAndRanksLookup = validRanks.First();
                            var suit = suitAndRanksLookup.Key;
                            if (handInfo.RanksPerSuit[suit].Length == 3)
                            {
                                var fifthCard = handInfo.Cards.First(card => !card.IsJoker && card.Suit != suit);
                                if (new[] {1, 3, 6, 7, 8}.Contains(fifthCard.Rank))
                                {
                                    return Core.RuleResult.Fail("H");
                                }
                                else if (handInfo.RankCounts[13] == 1 && handInfo.RankCounts[10] == 0)
                                {
                                    return Core.RuleResult.Fail("H");
                                }
                            }
                            else
                            {
                                var threes = handInfo.Cards.Where(card => card.Rank == 3 && !card.IsJoker);
                                if (threes.Any())
                                {
                                    var three = threes.First();
                                    var suitedWithThree = handInfo.CardPositionMatrix[three.Suit];
                                    if (suitedWithThree.ContainsKey(9) || suitedWithThree.ContainsKey(11) || new[] {8, 12, 13}.Any(rank => handInfo.RankCounts[rank] == 1))
                                    {
                                        return Core.RuleResult.Fail("H");
                                    }
                                }
                                else if (handInfo.RankCounts[1] == 1 && handInfo.RankCounts[6] + handInfo.RankCounts[7] == 1)
                                {
                                    return Core.RuleResult.Fail("H");
                                }
                            }

                            var lookup = suitAndRanksLookup.Value;
                            return Core.RuleResult.Pass(handInfo.WildPositions.Sum(i => 1 << i) + lookup.Where(kvp => kvp.Key.InRange(4, 5)).Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 42. [1W] 4 to a straight: W567, W678, W789, W89T, W9TJ, WTJQ (rule I: it gets a bit weird)
                {
                    var exceptions = new int[][]
                    {
                        new[] {12, 13, 1},
                        new[] {3, -13, 1},
                        new[] {3, -4},
                        new int[0],
                        new int[0],
                        new[] {3}
                    };
                    if (handInfo.WildCount == 1)
                    {
                        int total = handInfo.RankCounts.GetRange(4, 3).Sum();
                        for (int lowRank = 5; lowRank <= 10; lowRank++)
                        {
                            total += handInfo.RankCounts[lowRank + 2] - handInfo.RankCounts[lowRank - 1];
                            if (total == 3)
                            {
                                var fifthCard = handInfo.Cards.First(card => !card.IsJoker && !card.Rank.InRange(lowRank, lowRank + 2));
                                foreach (int exception in exceptions[lowRank - 5])
                                {
                                    if (handInfo.RankCounts[Math.Abs(exception)] == 1 && (exception > 0 || handInfo.RanksPerSuit[fifthCard.Suit].Length > 1))
                                    {
                                        return Core.RuleResult.Fail("I");
                                    }
                                }
                                return Core.RuleResult.Pass(handInfo.WildPositions.Sum(i => 1 << i) + CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(lowRank, lowRank + 2)).Sum(i => 1 << i));
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 43. [1W] One deuce
                {
                    if (handInfo.WildCount == 1)
                    {
                        return Core.RuleResult.Pass(1 << handInfo.WildPositions[0]);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 44. [2W] Wild royal flush
                {
                    if (handInfo.WildCount == 2 && handInfo.RanksPerSuit.Values.Any(array => array.Length == 3) && handInfo.RankCounts[1] + handInfo.RankCounts.GetRange(10, 4).Sum() == 3)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 45. [2W] Five of a kind
                {
                    if (handInfo.WildCount == 2 && handInfo.RankCounts.Contains(3))
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 46. [2W] Straight flush
                {
                    if (handInfo.WildCount == 2)
                    {
                        var valid = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                        if (valid.Any())
                        {
                            var ranks = valid.First().Value;
                            if (ranks.Last() - ranks.First() <= 4)
                            {
                                return Core.RuleResult.Pass(31);
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 47. [2W] Four of a kind
                {
                    if (handInfo.WildCount == 2 && handInfo.RankCounts.Contains(2))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].IsJoker || handInfo.RankCounts[handInfo.Cards[i].Rank] == 2).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 48. [2W] 4 to a wild royal flush
                {
                    if (handInfo.WildCount == 2)
                    {
                        var validRanks = handInfo.CardPositionMatrix.Where(kvp => RoyalRanks.Count(rank => kvp.Value.ContainsKey(rank)) == 2);
                        if (validRanks.Any())
                        {
                            var lookup = validRanks.First().Value;
                            return Core.RuleResult.Pass(handInfo.WildPositions.Sum(i => 1 << i) + lookup.Where(kvp => kvp.Key == 1 || kvp.Key >= 10).Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 49. [2W] 4 to a straight flush: WW45, WW56, WW57, WW67, WW68, WW78, WW79, WW89, WW8T, WW9T, WW9J
                {
                    if (handInfo.WildCount == 2)
                    {
                        var valid = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.Count() >= 2);
                        if (valid.Any())
                        {
                            var lookup = valid.First();
                            var ranks = handInfo.RanksPerSuit[lookup.Key];
                            for (int j = 0; j < ranks.Length - 1; j++)
                            {
                                if (ranks[j] >= 4 && (ranks[j + 1] - ranks[j] == 1 || (ranks[j + 1] - ranks[j] == 2 && ranks[j] >= 5)))
                                {
                                    return Core.RuleResult.Pass(handInfo.WildPositions.Sum(i => 1 << i) + (1 << lookup.Value[ranks[j]]) + (1 << lookup.Value[ranks[j + 1]]));
                                }
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 50. [2W] Two deuces
                {
                    if (handInfo.WildCount == 2)
                    {
                        return Core.RuleResult.Pass(handInfo.WildPositions.Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 51. [3W] Wild royal flush
                {
                    if (handInfo.WildCount == 3 && handInfo.RanksPerSuit.Values.Any(array => array.Length == 2) && handInfo.RankCounts[1] + handInfo.RankCounts.GetRange(10, 4).Sum() == 2)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 52. [3W] Five of a kind
                {
                    if (handInfo.WildCount == 3 && handInfo.RankCounts.Contains(2))
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 53. [3W] Three deuces
                {
                    if (handInfo.WildCount == 3)
                    {
                        return Core.RuleResult.Pass(handInfo.WildPositions.Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 54. [4W] Four deuces (rule J: You may keep or discard the non-deuce)
                {
                    if (handInfo.WildCount == 4)
                    {
                        return Core.RuleResult.Pass("J", handInfo.WildPositions.Sum(i => 1 << i), 31);
                    }
                    return Core.RuleResult.Fail();
                }
            );

            public static Core.Strategy StrategyLooseDeuces = new Core.DeucesWildStrategy(0,
                (handInfo) => // 1. [0W] Natural royal flush
                {
                    if (handInfo.WildCount == 0 && handInfo.RanksPerSuit.Values.Any(ranks => Enumerable.SequenceEqual(ranks, RoyalRanks)))
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 2. [0W] 4 to a natural royal flush
                {
                    if (handInfo.WildCount == 0)
                    {
                        var validSuits = handInfo.CardPositionMatrix.Where(kvp => RoyalRanks.Count(rank => kvp.Value.ContainsKey(rank)) == 4);
                        if (validSuits.Any())
                        {
                            var ranksLookup = validSuits.First().Value;
                            var ranks = ranksLookup.Keys.Intersect(RoyalRanks);
                            return Core.RuleResult.Pass(ranks.Sum(rank => 1 << ranksLookup[rank]));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 3. [0W] Straight flush
                {
                    if (handInfo.WildCount == 0)
                    {
                        var validSuits = handInfo.RanksPerSuit.Values.Where(ranks => ranks.Length == 5);
                        if (validSuits.Any())
                        {
                            var ranks = validSuits.First();
                            if (ranks[4] - ranks[0] == 4)
                            {
                                return Core.RuleResult.Pass(31);
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 4. [0W] Four of a kind
                {
                    var quads = handInfo.RankCounts.IndexOf(4);
                    if (quads > 0 && handInfo.WildCount == 0)
                    {
                        var indexToDiscard = CardIndices.First(i => handInfo.Cards[i].Rank != quads);
                        return Core.RuleResult.Pass(31 - (1 << indexToDiscard));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 5. [0W] Full house
                {
                    var trips = handInfo.RankCounts.IndexOf(3);
                    var pair = handInfo.RankCounts.IndexOf(2);
                    if (trips > 0 && pair > 0 && handInfo.WildCount == 0)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 6. [0W] Flush
                {
                    var flushes = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 5);
                    if (flushes.Any() && handInfo.WildCount == 0)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 7. [0W] Straight
                {
                    var ranks = AllRanks.Where(r => handInfo.RankCounts[r] == 1).ToArray();
                    if (ranks.Length == 5 && (Enumerable.SequenceEqual(ranks, RoyalRanks) || ranks[4] - ranks[0] == 4) && handInfo.WildCount == 0)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 8. [0W] Three of a kind
                {
                    var trips = handInfo.RankCounts.IndexOf(3);
                    if (trips > 0 && handInfo.WildCount == 0)
                    {
                        var cardIndicesToKeep = CardIndices.Where(i => handInfo.Cards[i].Rank == trips);
                        return Core.RuleResult.Pass(cardIndicesToKeep.Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 9. [0W] 3 to a natural royal flush (rule A: suited 9TJQ beats suited TJQ)
                {
                    if (handInfo.WildCount == 0)
                    {
                        var validSuits = handInfo.CardPositionMatrix.Where(kvp => RoyalRanks.Count(rank => kvp.Value.ContainsKey(rank)) == 3);
                        if (validSuits.Any())
                        {
                            var suitAndRanksLookup = validSuits.First();
                            if (Enumerable.SequenceEqual(handInfo.RanksPerSuit[suitAndRanksLookup.Key], Enumerable.Range(9, 4)))
                            {
                                return Core.RuleResult.Fail("A");
                            }
                            var ranksLookup = suitAndRanksLookup.Value;
                            var ranks = ranksLookup.Keys.Intersect(RoyalRanks);
                            return Core.RuleResult.Pass(ranks.Sum(rank => 1 << ranksLookup[rank]));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 10. [0W] 4 to a straight flush
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Count() == 4);
                    if (!validSuits.Any() || handInfo.WildCount > 0)
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
                (handInfo) => // 11. [0W] One pair (rule B: If you have two pairs, only keep one of them. It doesn't matter which one you keep.)
                {
                    if (handInfo.WildCount == 0)
                    {
                        var validStrategies = new List<int>();
                        for (int r = 1; r <= 13; r++)
                        {
                            if (r == 2)
                            {
                                continue;
                            }
                            if (handInfo.RankCounts[r] == 2)
                            {
                                validStrategies.Add(CardIndices.Where(i => handInfo.Cards[i].Rank == r).Sum(i => 1 << i));
                            }
                        }
                        if (validStrategies.Count() >= 2)
                        {
                            return Core.RuleResult.Pass("B", validStrategies.ToArray());
                        }
                        if (validStrategies.Count() == 1)
                        {
                            return Core.RuleResult.Pass(validStrategies.First());
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 12. [0W] 4 to an open-ended straight (rule C: If 4 to a flush is also possible, then either play is acceptable.)
                {
                    if (handInfo.WildCount > 0)
                    {
                        return Core.RuleResult.Fail();
                    }
                    int rankCount = handInfo.RankCounts.GetRange(3, 4).Sum();
                    for (int minRank = 4; minRank <= 10; minRank++)
                    {
                        rankCount += handInfo.RankCounts[minRank + 3] - handInfo.RankCounts[minRank - 1];
                        if (rankCount == 4)
                        {
                            int straightStrategy = CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(minRank, minRank + 3)).Select(i => 1 << i).Sum();
                            var fourToAFlushSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.Count() == 4);
                            if (fourToAFlushSuits.Any())
                            {
                                return Core.RuleResult.Pass("C", straightStrategy, fourToAFlushSuits.First().Value.Sum(kvp => 1 << kvp.Value));
                            }
                            return Core.RuleResult.Pass(straightStrategy);
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 13. [0W] 4 to a flush
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Count() == 4);
                    if (validSuits.Any() && handInfo.WildCount == 0)
                    {
                        var suit = validSuits.First().Key;
                        var indexToDiscard = CardIndices.First(i => handInfo.Cards[i].Suit != suit);
                        return Core.RuleResult.Pass(31 - (1 << indexToDiscard));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 14. [0W] 3 to a straight flush: 0 or 1 gap, except Ace low, 346, or 356
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Count() == 3);
                    if (validSuits.Any() && handInfo.WildCount == 0)
                    {
                        var suitAndRanks = validSuits.First();
                        var ranks = suitAndRanks.Value;
                        if (Enumerable.SequenceEqual(ranks, new[] {3, 4, 5}) || (ranks[0] >= 4 && ranks[2] - ranks[0] <= 3))
                        {
                            return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 15. [0W] Suited TJ (rule D: 8TJQ and 9TJK beat suited TJ if there's suited 7TJ)
                {
                    if (handInfo.WildCount == 0 && handInfo.RankCounts[10] + handInfo.RankCounts[11] == 2)
                    {
                        var tenCard = handInfo.Cards.First(card => card.Rank == 10);
                        var jackCard = handInfo.Cards.First(card => card.Rank == 11);
                        if (tenCard.Suit == jackCard.Suit)
                        {
                            if (handInfo.Cards.Any(card => card.Rank == 7 && card.Suit == tenCard.Suit))
                            {
                                if (handInfo.RankCounts[8] + handInfo.RankCounts[12] == 2 || handInfo.RankCounts[9] + handInfo.RankCounts[13] == 2)
                                {
                                    return Core.RuleResult.Fail("D");
                                }
                            }
                            return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(10, 11)).Sum(i => 1 << i));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 16. [0W] 3 to a straight flush: 346, 356, or 2 gaps, except Ace low (rules E and F)
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Count() == 3);
                    if (validSuits.Any() && handInfo.WildCount == 0)
                    {
                        var suitAndRanks = validSuits.First();
                        var ranks = suitAndRanks.Value;
                        if (ranks[0] >= 3 && ranks[2] - ranks[0] <= 4)
                        {
                            // Rule E: If this straight flush draw has a straight penalty card, keep 4 to a straight
                            if (handInfo.Cards.Any(card => card.Suit != suitAndRanks.Key && card.Rank.InRange(ranks[0], Math.Max(ranks[0] + 4, ranks[2]))))
                            {
                                return Core.RuleResult.Fail("E");
                            }
                            // Rule F: Suited TQ and JQ beat 3 to a straight flush if the sum of the ranks of the straight flush draw's cards is 21 or less.
                            if (ranks.Sum() <= 21 && handInfo.RanksPerSuit.Values.Any(rs => rs.Length == 2 && rs[0] >= 10 && rs[1] == 12))
                            {
                                return Core.RuleResult.Fail("F");
                            }
                            return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 17. [0W] Suited TQ or JQ (rules G and H)
                {
                    if (handInfo.WildCount == 0)
                    {
                        var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.
                        Count() >= 2 && kvp.Value.Count(kvp2 => kvp2.Key.InRange(10, 12)) == 2);
                        if (validSuits.Any())
                        {
                            var sevenToAce = handInfo.RankCounts.GetRange(7, 7);
                            sevenToAce.Add(handInfo.RankCounts[1]);

                            int total = sevenToAce.Take(5).Sum();
                            for (int i = 0; i < 3; i++)
                            {
                                total += sevenToAce[i + 5] - sevenToAce[i];
                                if (total == 4)
                                {
                                    return Core.RuleResult.Fail("G");
                                }
                            }

                            var cardsSortedByRank = handInfo.Cards.OrderBy(card => card.Rank).ToArray();
                            if (cardsSortedByRank[0].Rank == 6 && cardsSortedByRank[1].Rank == 7 && cardsSortedByRank[3].Rank == 10 && cardsSortedByRank.Take(2).Any(card => card.Suit == cardsSortedByRank[3].Suit))
                            {
                                return Core.RuleResult.Fail("H");
                            }

                            var suitAndRanksLookup = validSuits.First();
                            return Core.RuleResult.Pass(suitAndRanksLookup.Value.Where(kvp => kvp.Key.InRange(10, 12)).Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 18. [0W] 4 to a straight (rule I)
                {
                    if (handInfo.WildCount > 0)
                    {
                        return Core.RuleResult.Fail();
                    }

                    var validStrategies = new List<int>();

                    int total = handInfo.RankCounts.GetRange(2, 5).Sum();
                    for (int lowRank = 3; lowRank <= 9; lowRank++)
                    {
                        total += handInfo.RankCounts[lowRank + 4] - handInfo.RankCounts[lowRank - 1];
                        if (total == 4)
                        {
                            validStrategies.Add(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(lowRank, lowRank + 4)).Sum(i => 1 << i));
                        }
                    }

                    if (new[] {10, 11, 12, 13, 1}.Count(rank => handInfo.RankCounts[rank] == 1) == 4)
                    {
                        validStrategies.Add(CardIndices.Where(i => handInfo.Cards[i].Rank == 1 || handInfo.Cards[i].Rank >= 10).Sum(i => 1 << i));
                    }

                    // Rule I: If there are two ways to play this hand as 4 to an inside straight, you may choose either one.
                    if (validStrategies.Count() == 2)
                    {
                        return Core.RuleResult.Pass("I", validStrategies.ToArray());
                    }
                    if (validStrategies.Count() == 1)
                    {
                        return Core.RuleResult.Pass(validStrategies[0]);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 19. [0W] Suited TK, JK, or QK (rule J: it gets complicated)
                {
                    var validOthers = new[]
                    {
                        new[] // Valid with TK
                        {
                            0x345, 0x346, 0x347, 0x348, 0x356, 0x357, 0x358, 0x3359, 0x367, 0x368, 0x2369, 0x378, 0x379, 0x389, 0x456, 0x457, 0x458, 0x459, 0x467, 0x468, 0x469, 0x478, 0x479, 0x3147, 0x489, 0x3148, 0x567, 0x568, 0x569, 0x3156, 0x578, 0x579, 0x257B, 0x157, 0x589, 0x358B, 0x158, 0x267B, 0x367C, 0x167, 0x268B, 0x168, 0x278C, 0x178
                        },
                        new[] // Valid with JK
                        {
                            0x345, 0x346, 0x356, 0x456, 0x347, 0x357, 0x457, 0x367, 0x467, 0x567, 0x348, 0x358, 0x458, 0x368, 0x468, 0x568, 0x378, 0x478, 0x578, 0x678, 0x3459, 0x2469, 0x2569, 0x2379, 0x479, 0x579, 0x679, 0x2389, 0x489, 0x2589, 0x2689, 0x257A, 0x267A, 0x358A, 0x68A, 0x3157, 0x2167, 0x3158, 0x2168, 0x178
                        },
                        new[] // Valid with QK
                        {
                            0x346, 0x356, 0x456, 0x347, 0x357, 0x457, 0x367, 0x467, 0x567, 0x348, 0x358, 0x458, 0x368, 0x468, 0x568, 0x378, 0x478, 0x578, 0x678, 0x569, 0x2479, 0x579, 0x679, 0x2489, 0x589, 0x689, 0x789, 0x367A, 0x78A, 0x2178
                        }
                    };
                    if (handInfo.WildCount == 0 && handInfo.RankCounts[13] == 1)
                    {
                        var king = handInfo.Cards.First(card => card.Rank == 13);
                        var rankLookup = handInfo.CardPositionMatrix[king.Suit];
                        for (int otherRoyalRank = 10; otherRoyalRank <= 12; otherRoyalRank++)
                        {
                            if (rankLookup.ContainsKey(otherRoyalRank))
                            {
                                if (rankLookup.Count() > 2)
                                {
                                    // TODO: Implement this
                                    if (otherRoyalRank == 10)
                                    {
                                        if (new[] {5, 7, 8}.All(r => handInfo.RankCounts[r] == 1))
                                        {
                                            var lowCards = handInfo.Cards.Where(card => card.Rank <= 8);
                                            if (lowCards.Select(card => card.Suit).Distinct().Count() == 2 && rankLookup.ContainsKey(8))
                                            {
                                                return Core.RuleResult.Fail("J");
                                            }
                                            return Core.RuleResult.Pass((1 << rankLookup[10]) + (1 << rankLookup[13]));
                                        }
                                    }
                                    else if (otherRoyalRank == 11)
                                    {
                                        if (new[] {6, 7, 8}.All(r => handInfo.RankCounts[r] == 1))
                                        {
                                            if (handInfo.Cards.Select(card => card.Suit).Distinct().Count() == 2)
                                            {
                                                return Core.RuleResult.Fail("J");
                                            }
                                            return Core.RuleResult.Pass((1 << rankLookup[11]) + (1 << rankLookup[13]));
                                        }
                                    }
                                    return Core.RuleResult.Fail("J");
                                }
                                foreach (int mask in validOthers[otherRoyalRank - 10])
                                {
                                    int minExtraSuits = mask >> 12;
                                    var otherRanks = new[] {(mask >> 8) % 16, (mask >> 4) % 16, mask % 16};
                                    var validOtherCards = handInfo.Cards.Where(card => otherRanks.Contains(card.Rank));
                                    if (validOtherCards.Count() == 3 && validOtherCards.Select(card => card.Suit).Distinct().Count() >= minExtraSuits)
                                    {
                                        return Core.RuleResult.Pass(rankLookup.Sum(kvp => 1 << kvp.Value));
                                    }
                                }
                                // Super special exceptions: 58TJK, where no card matches the suit of the 8
                                if (new[] {5, 8, 10, 11, 13}.All(r => handInfo.RankCounts[r] == 1))
                                {
                                    var eightCard = handInfo.Cards.First(card => card.Rank == 8);
                                    if (handInfo.RanksPerSuit[eightCard.Suit].Length == 1)
                                    {
                                        return Core.RuleResult.Pass(rankLookup.Sum(kvp => 1 << kvp.Value));
                                    }
                                }
                            }
                        }
                        return Core.RuleResult.Fail("J");
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 20. [1W] Wild royal flush
                {
                    if (handInfo.WildCount == 1 && handInfo.RanksPerSuit.Values.Any(array => array.Length == 4) && handInfo.RankCounts[1] + handInfo.RankCounts.GetRange(10, 4).Sum() == 4)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 21. [1W] Five of a kind
                {
                    if (handInfo.WildCount == 1 && handInfo.RankCounts.Contains(4))
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 22. [1W] Straight flush
                {
                    if (handInfo.WildCount == 1)
                    {
                        var valid = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 4);
                        if (valid.Any())
                        {
                            var ranks = valid.First().Value;
                            if (ranks.Last() - ranks.First() <= 4)
                            {
                                return Core.RuleResult.Pass(31);
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 23. [1W] Four of a kind
                {
                    if (handInfo.WildCount == 1 && handInfo.RankCounts.Contains(3))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].IsJoker || handInfo.RankCounts[handInfo.Cards[i].Rank] == 3).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 24. [1W] 4 to a wild royal flush
                {
                    if (handInfo.WildCount == 1)
                    {
                        var validRanks = handInfo.CardPositionMatrix.Where(kvp => RoyalRanks.Count(rank => kvp.Value.ContainsKey(rank)) == 3);
                        if (validRanks.Any())
                        {
                            var lookup = validRanks.First().Value;
                            return Core.RuleResult.Pass(handInfo.WildPositions.Sum(i => 1 << i) + lookup.Where(kvp => kvp.Key == 1 || kvp.Key >= 10).Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 25. [1W] Full house
                {
                    if (handInfo.WildCount == 1 && handInfo.RankCounts.Count(n => n == 2) == 2)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 26. [1W] 4 to a straight flush: W567 to W9TJ
                {
                    if (handInfo.WildCount == 1)
                    {
                        var valid = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length >= 3);
                        if (valid.Any())
                        {
                            var suitAndRanks = valid.First();
                            var ranks = suitAndRanks.Value;
                            for (int i = 0; i < ranks.Length - 2; i++)
                            {
                                if (ranks[i + 2] - ranks[i] == 2 && ranks[i].InRange(5, 9))
                                {
                                    int indexToDiscard = CardIndices.First(j => !handInfo.Cards[j].IsJoker && !(handInfo.Cards[j].Suit == suitAndRanks.Key && handInfo.Cards[j].Rank.InRange(ranks[i], ranks[i] + 2)));
                                    return Core.RuleResult.Pass(31 - (1 << indexToDiscard));
                                }
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 27. [1W] Flush
                {
                    if (handInfo.WildCount == 1 && handInfo.RanksPerSuit.Values.Any(ranks => ranks.Length == 4))
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 28. [1W] Straight
                {
                    var ranks = AllRanks.Where(r => handInfo.RankCounts[r] == 1).ToArray();
                    if (ranks.Length == 4 && (ranks.Intersect(RoyalRanks).Count() == 4 || ranks[3] - ranks[0] <= 4) && handInfo.WildCount == 1)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 29. [1W] 4 to a straight flush: 0 or 1 gaps, except Ace low, W346, or W356
                {
                    if (handInfo.WildCount == 1)
                    {
                        var valid = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                        if (valid.Any())
                        {
                            var suitAndRanks = valid.First();
                            var ranks = suitAndRanks.Value;
                            if (ranks[2] - ranks[0] <= 3 && (ranks[2] - ranks[0] == 2 || ranks[2] >= 7))
                            {
                                return Core.RuleResult.Pass(31 - (1 << CardIndices.First(i => !handInfo.Cards[i].IsJoker && handInfo.Cards[i].Suit != suitAndRanks.Key)));
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 30. [1W] Three of a kind
                {
                    if (handInfo.WildCount == 1 && handInfo.RankCounts.Contains(2))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].IsJoker || handInfo.RankCounts[handInfo.Cards[i].Rank] == 2).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 31. [1W] 4 to a straight flush: everything else
                {
                    if (handInfo.WildCount == 1)
                    {
                        var valid = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                        if (valid.Any())
                        {
                            var suitAndRanks = valid.First();
                            var ranks = suitAndRanks.Value;
                            if (ranks[2] - ranks[0] <= 4)
                            {
                                return Core.RuleResult.Pass(31 - (1 << CardIndices.First(i => !handInfo.Cards[i].IsJoker && handInfo.Cards[i].Suit != suitAndRanks.Key)));
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 32. [1W] 3 to a wild royal flush: WTJ, WTQ, WTK, WJQ, WJK, or WQK
                {
                    if (handInfo.WildCount == 1)
                    {
                        var validRanks = handInfo.CardPositionMatrix.Where(kvp => Enumerable.Range(10, 4).Count(rank => kvp.Value.ContainsKey(rank)) == 2);
                        if (validRanks.Any())
                        {
                            var lookup = validRanks.First().Value;
                            return Core.RuleResult.Pass(handInfo.WildPositions.Sum(i => 1 << i) + lookup.Where(kvp => kvp.Key >= 10).Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 33. [1W] W67 through W9T (rule K: it's complicated)
                {
                    if (handInfo.WildCount == 1)
                    {
                        // Each array at the lowest level is {firstRank, secondRank, mustBeUnsuited}.
                        // Negative numbers indicate stars.
                        var acceptable = new[]
                        {
                            new[] // W67
                            {
                                new[] {3, 11, 0},
                                new[] {9, 11, 0},
                                new[] {9, 12, 1},
                                new[] {10, 11, 0},
                                new[] {10, 12, 0},
                                new[] {10, 13, 0},
                                new[] {10, 1, 0},
                                new[] {-11, -12, 0},
                                new[] {-11, -13, 0},
                                new[] {-11, -1, 0},
                                new[] {-12, -13, 0},
                                new[] {12, 1, 0},
                                new[] {13, 1, 0}
                            },
                            new[] // W78
                            {
                                new[] {3, 11, 0},
                                new[] {3, 12, 0},
                                new[] {3, 13, 0},
                                new[] {3, 1, 0},
                                new[] {4, 12, 0},
                                new[] {4, 13, 0},
                                new[] {10, 12, 0},
                                new[] {10, 13, 0},
                                new[] {10, 1, 0},
                                new[] {11, 12, 0},
                                new[] {11, 13, 0},
                                new[] {11, 1, 0},
                                new[] {-12, -13, 1},
                                new[] {-12, -1, 0},
                                new[] {13, 1, 0}
                            },
                            new[] // W89
                            {
                                new[] {3, 4, 0},
                                new[] {3, 12, 0},
                                new[] {3, 13, 0},
                                new[] {3, 1, 0},
                                new[] {4, 5, 1},
                                new[] {4, 12, 0},
                                new[] {4, 13, 0},
                                new[] {4, 1, 0},
                                new[] {5, 13, 0},
                                new[] {5, 1, 0},
                                new[] {12, 13, 1},
                                new[] {12, 1, 0},
                                new[] {13, 1, 0}
                            },
                            new[] // W9T
                            {
                                new[] {3, 4, 0},
                                new[] {3, 5, 0},
                                new[] {3, 6, 0},
                                new[] {3, 13, 0},
                                new[] {3, 1, 0},
                                new[] {-4, -5, 0},
                                new[] {4, 6, 0},
                                new[] {4, 7, 0},
                                new[] {4, 13, 0},
                                new[] {-4, 1, 0},
                                new[] {5, 6, 0},
                                new[] {5, 7, 0},
                                new[] {5, 12, 0},
                                new[] {5, 13, 0},
                                new[] {-5, -1, 0},
                                new[] {6, 1, 0},
                                new[] {7, 1, 0},
                                new[] {13, 1, 0},
                            }
                        };

                        for (int lowRank = 6; lowRank < 6 + acceptable.Length; lowRank++)
                        {
                            var validDraws = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(lowRank) && kvp.Value.ContainsKey(lowRank + 1));
                            if (validDraws.Any())
                            {
                                var suitAndRanksLookup = validDraws.First();
                                // Check the other cards
                                foreach (var candidate in acceptable[lowRank - 6])
                                {
                                    if (new[] {0, 1}.All((index) =>
                                    {
                                        int r = candidate[index];
                                        return handInfo.RankCounts[Math.Abs(r)] == 1 && !suitAndRanksLookup.Value.ContainsKey(r);
                                    }))
                                    {
                                        var otherCards = handInfo.Cards.Where(card => !card.IsJoker && !card.Rank.InRange(lowRank, lowRank + 1));
                                        if (candidate[2] == 0 || otherCards.Select(card => card.Suit).Distinct().Count() == 2)
                                        {
                                            int strategy = (1 << handInfo.WildPositions[0]) + Enumerable.Range(lowRank, 2).Sum(r => 1 << suitAndRanksLookup.Value[r]);
                                            return Core.RuleResult.Pass(strategy);
                                        }
                                    }
                                }
                            }
                        }
                        return Core.RuleResult.Fail("K");
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 34. [1W] Deuce only
                {
                    if (handInfo.WildCount == 1)
                    {
                        return Core.RuleResult.Pass(1 << handInfo.WildPositions[0]);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 35. [2W] Wild royal flush
                {
                    if (handInfo.WildCount == 2 && handInfo.RanksPerSuit.Values.Any(array => array.Length == 3) && handInfo.RankCounts[1] + handInfo.RankCounts.GetRange(10, 4).Sum() == 3)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 36. [2W] Five of a kind
                {
                    if (handInfo.WildCount == 2 && handInfo.RankCounts.Contains(3))
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 37. [2W] Straight flush
                {
                    if (handInfo.WildCount == 2)
                    {
                        var valid = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                        if (valid.Any())
                        {
                            var ranks = valid.First().Value;
                            if (ranks.Last() - ranks.First() <= 4)
                            {
                                return Core.RuleResult.Pass(31);
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 38. [2W] Four of a kind
                {
                    if (handInfo.WildCount == 2 && handInfo.RankCounts.Contains(2))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].IsJoker || handInfo.RankCounts[handInfo.Cards[i].Rank] == 2).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 39. [2W] 4 to a wild royal flush
                {
                    if (handInfo.WildCount == 2)
                    {
                        var validRanks = handInfo.CardPositionMatrix.Where(kvp => RoyalRanks.Count(rank => kvp.Value.ContainsKey(rank)) == 2);
                        if (validRanks.Any())
                        {
                            var lookup = validRanks.First().Value;
                            return Core.RuleResult.Pass(handInfo.WildPositions.Sum(i => 1 << i) + lookup.Where(kvp => kvp.Key == 1 || kvp.Key >= 10).Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 40. [2W] Two deuces
                {
                    if (handInfo.WildCount == 2)
                    {
                        return Core.RuleResult.Pass(handInfo.WildPositions.Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 41. [3+W] Keep all deuces
                {
                    if (handInfo.WildCount == 3)
                    {
                        return Core.RuleResult.Pass(handInfo.WildPositions.Sum(i => 1 << i));
                    }
                    if (handInfo.WildCount == 4)
                    {
                        return Core.RuleResult.Pass("L", handInfo.WildPositions.Sum(i => 1 << i), 31);
                    }
                    return Core.RuleResult.Fail();
                }
            );

            public static Core.Strategy StrategyFullPayDeucesWild = new Core.DeucesWildStrategy(0,
                (handInfo) => // 1. [0W] Natural royal flush
                {
                    if (handInfo.WildCount == 0 && handInfo.RanksPerSuit.Values.Any(ranks => Enumerable.SequenceEqual(ranks, RoyalRanks)))
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 2. [0W] 4 to a natural royal flush
                {
                    if (handInfo.WildCount == 0)
                    {
                        var validSuits = handInfo.CardPositionMatrix.Where(kvp => RoyalRanks.Count(rank => kvp.Value.ContainsKey(rank)) == 4);
                        if (validSuits.Any())
                        {
                            var ranksLookup = validSuits.First().Value;
                            var ranks = ranksLookup.Keys.Intersect(RoyalRanks);
                            return Core.RuleResult.Pass(ranks.Sum(rank => 1 << ranksLookup[rank]));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 3. [0W] Straight flush
                {
                    if (handInfo.WildCount == 0)
                    {
                        var validSuits = handInfo.RanksPerSuit.Values.Where(ranks => ranks.Length == 5);
                        if (validSuits.Any())
                        {
                            var ranks = validSuits.First();
                            if (ranks[4] - ranks[0] == 4)
                            {
                                return Core.RuleResult.Pass(31);
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 4. [0W] Four of a kind
                {
                    var quads = handInfo.RankCounts.IndexOf(4);
                    if (quads > 0 && handInfo.WildCount == 0)
                    {
                        var indexToDiscard = CardIndices.First(i => handInfo.Cards[i].Rank != quads);
                        return Core.RuleResult.Pass(31 - (1 << indexToDiscard));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 5. [0W] Full house
                {
                    var trips = handInfo.RankCounts.IndexOf(3);
                    var pair = handInfo.RankCounts.IndexOf(2);
                    if (trips > 0 && pair > 0 && handInfo.WildCount == 0)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 6. [0W] Flush
                {
                    var flushes = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 5);
                    if (flushes.Any() && handInfo.WildCount == 0)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 7. [0W] Straight
                {
                    var ranks = AllRanks.Where(r => handInfo.RankCounts[r] == 1).ToArray();
                    if (ranks.Length == 5 && (Enumerable.SequenceEqual(ranks, RoyalRanks) || ranks[4] - ranks[0] == 4) && handInfo.WildCount == 0)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 8. [0W] Three of a kind
                {
                    var trips = handInfo.RankCounts.IndexOf(3);
                    if (trips > 0 && handInfo.WildCount == 0)
                    {
                        var cardIndicesToKeep = CardIndices.Where(i => handInfo.Cards[i].Rank == trips);
                        return Core.RuleResult.Pass(cardIndicesToKeep.Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 9. [0W] 4 to a straight flush, except 8TJQ (rule A: Suited 8TJQ beats suited TJQ if the fifth card is a King.)
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Count() == 4);
                    if (!validSuits.Any() || handInfo.WildCount > 0)
                    {
                        return Core.RuleResult.Fail();
                    }
                    var suitAndRanks = validSuits.First();
                    var suit = suitAndRanks.Key;
                    var ranks = suitAndRanks.Value;
                    if (ranks[3] - ranks[0] <= 4)
                    {
                        if (Enumerable.SequenceEqual(ranks, new[] {8, 10, 11, 12}))
                        {
                            if (handInfo.RankCounts[13] == 1)
                            {
                                return Core.RuleResult.Pass("A", 31 - (1 << CardIndices.First(i => handInfo.Cards[i].Rank == 13)));
                            }
                            else
                            {
                                return Core.RuleResult.Fail();
                            }
                        }
                        return Core.RuleResult.Pass(ranks.Select(r => 1 << handInfo.CardPositionMatrix[suit][r]).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 10. [0W] 3 to a natural royal flush
                {
                    if (handInfo.WildCount == 0)
                    {
                        var validSuits = handInfo.CardPositionMatrix.Where(kvp => RoyalRanks.Count(rank => kvp.Value.ContainsKey(rank)) == 3);
                        if (validSuits.Any())
                        {
                            var suitAndRanksLookup = validSuits.First();
                            var ranksLookup = suitAndRanksLookup.Value;
                            var ranks = ranksLookup.Keys.Intersect(RoyalRanks);
                            return Core.RuleResult.Pass(ranks.Sum(rank => 1 << ranksLookup[rank]));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 11. [0W] One pair (rule B: If you have two pairs, only keep one of them. It doesn't matter which one you keep.)
                {
                    if (handInfo.WildCount == 0)
                    {
                        var validStrategies = new List<int>();
                        for (int r = 1; r <= 13; r++)
                        {
                            if (r == 2)
                            {
                                continue;
                            }
                            if (handInfo.RankCounts[r] == 2)
                            {
                                validStrategies.Add(CardIndices.Where(i => handInfo.Cards[i].Rank == r).Sum(i => 1 << i));
                            }
                        }
                        if (validStrategies.Count() >= 2)
                        {
                            return Core.RuleResult.Pass("B", validStrategies.ToArray());
                        }
                        if (validStrategies.Count() == 1)
                        {
                            return Core.RuleResult.Pass(validStrategies.First());
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 12. [0W] 4 to an open-ended straight (rule C: If 4 to a flush is also possible, then either play is acceptable.)
                {
                    if (handInfo.WildCount > 0)
                    {
                        return Core.RuleResult.Fail();
                    }
                    int rankCount = handInfo.RankCounts.GetRange(3, 4).Sum();
                    for (int minRank = 4; minRank <= 10; minRank++)
                    {
                        rankCount += handInfo.RankCounts[minRank + 3] - handInfo.RankCounts[minRank - 1];
                        if (rankCount == 4)
                        {
                            int straightStrategy = CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(minRank, minRank + 3)).Select(i => 1 << i).Sum();
                            var fourToAFlushSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.Count() == 4);
                            if (fourToAFlushSuits.Any())
                            {
                                return Core.RuleResult.Pass("C", straightStrategy, fourToAFlushSuits.First().Value.Sum(kvp => 1 << kvp.Value));
                            }
                            return Core.RuleResult.Pass(straightStrategy);
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 13. [0W] 4 to a flush
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Count() == 4);
                    if (validSuits.Any() && handInfo.WildCount == 0)
                    {
                        var suit = validSuits.First().Key;
                        var indexToDiscard = CardIndices.First(i => handInfo.Cards[i].Suit != suit);
                        return Core.RuleResult.Pass(31 - (1 << indexToDiscard));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 14. [0W] 3 to a straight flush: 0 or 1 gap, except Ace low, 346, or 356
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Count() == 3);
                    if (validSuits.Any() && handInfo.WildCount == 0)
                    {
                        var suitAndRanks = validSuits.First();
                        var ranks = suitAndRanks.Value;
                        if (Enumerable.SequenceEqual(ranks, new[] {3, 4, 5}) || (ranks[0] >= 4 && ranks[2] - ranks[0] <= 3))
                        {
                            return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 15. [0W] Suited TJ (rule D: Suited 7TJ beats suited TJ if the remaining cards' ranks are 3Q, 4Q, 5Q, 6Q, QA, or KA)
                {
                    if (handInfo.WildCount == 0 && handInfo.RankCounts[10] + handInfo.RankCounts[11] == 2)
                    {
                        var tenCard = handInfo.Cards.First(card => card.Rank == 10);
                        var jackCard = handInfo.Cards.First(card => card.Rank == 11);
                        if (tenCard.Suit == jackCard.Suit)
                        {
                            if (handInfo.Cards.Any(card => card.Rank == 7 && card.Suit == tenCard.Suit))
                            {
                                if ((handInfo.RankCounts[12] == 1 && handInfo.RankCounts.GetRange(1, 6).Contains(1)) || handInfo.RankCounts[13] + handInfo.RankCounts[1] == 2)
                                {
                                    return Core.RuleResult.Fail("D");
                                }
                            }
                            return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(10, 11)).Sum(i => 1 << i));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 16. [0W] 3 to a straight flush: 2 gaps (except Ace low), 346, or 356 (rule E)
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Count() == 3);
                    if (validSuits.Any() && handInfo.WildCount == 0)
                    {
                        var suitAndRanks = validSuits.First();
                        var ranks = suitAndRanks.Value;
                        
                        if (ranks[0] >= 3 && ranks[2] - ranks[0] <= 4)
                        {
                            if (ranks[2] <= 7 && handInfo.RanksPerSuit.Values.Any(arr => arr.Length == 2 && arr[0] >= 10 && arr[1] == 12))
                            {
                                return Core.RuleResult.Fail("F");
                            }

                            int sfStrategy = handInfo.CardPositionMatrix[suitAndRanks.Key].Sum(kvp => 1 << kvp.Value);

                            if (handInfo.Cards.Any(card => card.Rank.InRange(ranks[0], ranks[0] + 4) && card.Suit != suitAndRanks.Key))
                            {
                                var validStrategies = new List<int>();

                                int total = handInfo.RankCounts.GetRange(2, 5).Sum();
                                for (int lowRank = 3; lowRank <= 9; lowRank++)
                                {
                                    total += handInfo.RankCounts[lowRank + 4] - handInfo.RankCounts[lowRank - 1];
                                    if (total == 4)
                                    {
                                        validStrategies.Add(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(lowRank, lowRank + 4)).Sum(i => 1 << i));
                                    }
                                }

                                if (new[] {10, 11, 12, 13, 1}.Count(rank => handInfo.RankCounts[rank] == 1) == 4)
                                {
                                    validStrategies.Add(CardIndices.Where(i => handInfo.Cards[i].Rank == 1 || handInfo.Cards[i].Rank >= 10).Sum(i => 1 << i));
                                }

                                validStrategies.Add(sfStrategy);
                                if (validStrategies.Count() >= 2)
                                {
                                    return Core.RuleResult.Pass("E", validStrategies.ToArray());
                                }
                            }
                            return Core.RuleResult.Pass(sfStrategy);
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 17. [0W] 4 to a straight (rules G, H, I)
                {
                    if (handInfo.WildCount > 0)
                    {
                        return Core.RuleResult.Fail();
                    }

                    var validStrategies = new List<int>();

                    int total = handInfo.RankCounts.GetRange(2, 5).Sum();
                    for (int lowRank = 3; lowRank <= 9; lowRank++)
                    {
                        total += handInfo.RankCounts[lowRank + 4] - handInfo.RankCounts[lowRank - 1];
                        if (total == 4)
                        {
                            validStrategies.Add(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(lowRank, lowRank + 4)).Sum(i => 1 << i));
                        }
                    }

                    if (new[] {10, 11, 12, 13, 1}.Count(rank => handInfo.RankCounts[rank] == 1) == 4)
                    {
                        validStrategies.Add(CardIndices.Where(i => handInfo.Cards[i].Rank == 1 || handInfo.Cards[i].Rank >= 10).Sum(i => 1 << i));
                    }

                    if (handInfo.RankCounts[12] == 1)
                    {
                        var queen = handInfo.Cards.First(card => card.Rank == 12);
                        var ranksWithQueenSuit = handInfo.RanksPerSuit[queen.Suit];
                        if (ranksWithQueenSuit.Length == 2 && ranksWithQueenSuit[1] == 12 && ranksWithQueenSuit[0] >= 10)
                        {
                            int tjqCount = handInfo.RankCounts.GetRange(10, 3).Sum();
                            int eightLow = handInfo.RankCounts[8] + handInfo.RankCounts[9] + tjqCount;
                            int nineLow = handInfo.RankCounts[9] + tjqCount + handInfo.RankCounts[13];
                            int tenLow = tjqCount + handInfo.RankCounts[13] + handInfo.RankCounts[1];
                            if (new[] {eightLow, nineLow, tenLow}.Min() == 2)
                            {
                                return Core.RuleResult.Fail("H");
                            }
                        }

                        if (ranksWithQueenSuit.Contains(10))
                        {
                            if (handInfo.RankCounts[6] == 1 && handInfo.RankCounts[7] == 1 && handInfo.RankCounts[8] == 1)
                            {
                                return Core.RuleResult.Fail("I");
                            }
                        }
                    }

                    // Rule G: If there are two ways to play this hand as 4 to an inside straight, you may choose either one.
                    if (validStrategies.Count() == 2)
                    {
                        return Core.RuleResult.Pass("G", validStrategies.ToArray());
                    }
                    if (validStrategies.Count() == 1)
                    {
                        return Core.RuleResult.Pass(validStrategies[0]);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 18. [0W] Suited TQ or JQ
                {
                    if (handInfo.WildCount == 0)
                    {
                        var valid = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length >= 2 && kvp.Value.Last() == 12 && kvp.Value[kvp.Value.Length - 2] >= 10);
                        if (valid.Any())
                        {
                            var suitAndRanks = valid.First();
                            var suit = suitAndRanks.Key;
                            var ranks = suitAndRanks.Value;

                            return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Suit == suit && handInfo.Cards[i].Rank >= 10).Sum(i => 1 << i));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 19. [0W] Suited TK, JK, or QK (rule J: it gets complicated)
                {
                    if (handInfo.WildCount == 0 && handInfo.RankCounts[13] == 1)
                    {
                        var king = handInfo.Cards.First(card => card.Rank == 13);

                        for (int lowRoyal = 10; lowRoyal <= 12; lowRoyal++)
                        {
                            // This check also ensures that we don't have a flush penalty card
                            if (Enumerable.SequenceEqual(handInfo.RanksPerSuit[king.Suit], new[] {lowRoyal, 13}))
                            {
                                int[] rankBitmasks = handInfo.CardPositionMatrix.Select(kvp => kvp.Value.Sum(kvp2 => 1 << kvp2.Key)).OrderBy(n => n).ToArray();
                                // We have this royal flush draw, now check for exceptions
                                if (handInfo.RankCounts.GetRange(10, 4).Sum() > 2)
                                {
                                    // We have a blatant straight penalty card
                                    return Core.RuleResult.Fail("J");
                                }
                                else if (handInfo.RankCounts[1] == 1 || handInfo.RankCounts[9] == 1)
                                {
                                    // We have a borderline straight penalty card
                                    if (Helpers.FPDW19.PenaltyExceptions.Any(seq => Enumerable.SequenceEqual(seq, rankBitmasks)))
                                    {
                                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[king.Suit].Sum(kvp => 1 << kvp.Value));
                                    }
                                    return Core.RuleResult.Fail("J");
                                }
                                else
                                {
                                    // No penalty card
                                    if (Helpers.FPDW19.NoPenaltyExceptions.Any(seq => Enumerable.SequenceEqual(seq, rankBitmasks)))
                                    {
                                        return Core.RuleResult.Fail("J");
                                    }
                                    return Core.RuleResult.Pass(handInfo.CardPositionMatrix[king.Suit].Sum(kvp => 1 << kvp.Value));
                                }
                            }
                        }
                    }

                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 20. [1W] Wild royal flush
                {
                    if (handInfo.WildCount == 1 && handInfo.RanksPerSuit.Values.Any(array => array.Length == 4) && handInfo.RankCounts[1] + handInfo.RankCounts.GetRange(10, 4).Sum() == 4)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 21. [1W] Five of a kind
                {
                    if (handInfo.WildCount == 1 && handInfo.RankCounts.Contains(4))
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 22. [1W] Straight flush
                {
                    if (handInfo.WildCount == 1)
                    {
                        var valid = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 4);
                        if (valid.Any())
                        {
                            var ranks = valid.First().Value;
                            if (ranks.Last() - ranks.First() <= 4)
                            {
                                return Core.RuleResult.Pass(31);
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 23. [1W] Four of a kind
                {
                    if (handInfo.WildCount == 1 && handInfo.RankCounts.Contains(3))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].IsJoker || handInfo.RankCounts[handInfo.Cards[i].Rank] == 3).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 24. [1W] 4 to a wild royal flush
                {
                    if (handInfo.WildCount == 1)
                    {
                        var validRanks = handInfo.CardPositionMatrix.Where(kvp => RoyalRanks.Count(rank => kvp.Value.ContainsKey(rank)) == 3);
                        if (validRanks.Any())
                        {
                            var lookup = validRanks.First().Value;
                            return Core.RuleResult.Pass(handInfo.WildPositions.Sum(i => 1 << i) + lookup.Where(kvp => kvp.Key == 1 || kvp.Key >= 10).Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 25. [1W] Full house
                {
                    if (handInfo.WildCount == 1 && handInfo.RankCounts.Count(n => n == 2) == 2)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 26. [1W] 4 to a straight flush: W567 to W9TJ
                {
                    if (handInfo.WildCount == 1)
                    {
                        var valid = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length >= 3);
                        if (valid.Any())
                        {
                            var suitAndRanks = valid.First();
                            var ranks = suitAndRanks.Value;
                            for (int i = 0; i < ranks.Length - 2; i++)
                            {
                                if (ranks[i + 2] - ranks[i] == 2 && ranks[i].InRange(5, 9))
                                {
                                    int indexToDiscard = CardIndices.First(j => !handInfo.Cards[j].IsJoker && !(handInfo.Cards[j].Suit == suitAndRanks.Key && handInfo.Cards[j].Rank.InRange(ranks[i], ranks[i] + 2)));
                                    return Core.RuleResult.Pass(31 - (1 << indexToDiscard));
                                }
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 27. [1W] Three of a kind
                {
                    if (handInfo.WildCount == 1 && handInfo.RankCounts.Contains(2))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].IsJoker || handInfo.RankCounts[handInfo.Cards[i].Rank] == 2).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 28. [1W] Flush
                {
                    if (handInfo.WildCount == 1 && handInfo.RanksPerSuit.Values.Any(ranks => ranks.Length == 4))
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 29. [1W] Straight
                {
                    var ranks = AllRanks.Where(r => handInfo.RankCounts[r] == 1).ToArray();
                    if (ranks.Length == 4 && (ranks.Intersect(RoyalRanks).Count() == 4 || ranks[3] - ranks[0] <= 4) && handInfo.WildCount == 1)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 30. [1W] 4 to a straight flush: everything else
                {
                    if (handInfo.WildCount == 1)
                    {
                        var valid = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                        if (valid.Any())
                        {
                            var suitAndRanks = valid.First();
                            var ranks = suitAndRanks.Value;
                            if (ranks[2] - ranks[0] <= 4)
                            {
                                return Core.RuleResult.Pass(31 - (1 << CardIndices.First(i => !handInfo.Cards[i].IsJoker && handInfo.Cards[i].Suit != suitAndRanks.Key)));
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 31. [1W] 3 to a wild royal flush: WTJ, WTQ, WTK, WJQ, WJK, or WQK
                {
                    if (handInfo.WildCount == 1)
                    {
                        var validRanks = handInfo.CardPositionMatrix.Where(kvp => Enumerable.Range(10, 4).Count(rank => kvp.Value.ContainsKey(rank)) == 2);
                        if (validRanks.Any())
                        {
                            var lookup = validRanks.First().Value;
                            return Core.RuleResult.Pass(handInfo.WildPositions.Sum(i => 1 << i) + lookup.Where(kvp => kvp.Key >= 10).Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 32. [1W] 3 to a straight flush: W67, W78, W89, or W9T
                {
                    if (handInfo.WildCount == 1)
                    {
                        for (int r = 6; r <= 9; r++)
                        {
                            var validRanks = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(r) && kvp.Value.ContainsKey(r + 1));
                            if (validRanks.Any())
                            {
                                var lookup = validRanks.First().Value;
                                return Core.RuleResult.Pass(handInfo.WildPositions.Sum(i => 1 << i) + lookup.Where(kvp => kvp.Key.InRange(r, r + 1)).Sum(kvp => 1 << kvp.Value));
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 33. [1W] 3 to a wild royal flush: WTA, WJA, WQA, or WKA
                {
                    if (handInfo.WildCount == 1 && handInfo.RankCounts[1] == 1)
                    {
                        var ace = handInfo.Cards.First(card => !card.IsJoker && card.Rank == 1);
                        var positions = handInfo.CardPositionMatrix[ace.Suit];

                        var acceptable = new[]
                        {
                            new[]
                            {
                                -0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x45, 0x46, 0x47, 0x48, 0x49, 0x56, 0x57, 0x58, 0x59, -0x67, 0x68, 0x69, -0x78, 0x79, -0x7B, -0x89
                            },
                            new[]
                            {
                                -0x35, 0x36, 0x37, 0x38, 0x39, 0x45, 0x46, 0x47, 0x48, 0x49, 0x56, 0x57, 0x58, 0x59, -0x67, 0x68, 0x69, -0x78, 0x79, -0x7A, -0x89
                            },
                            new[]
                            {
                                0x37, 0x38, 0x39, -0x45, 0x46, 0x47, 0x48, 0x49, 0x56, 0x57, 0x58, 0x59, -0x67, 0x68, 0x69, -0x78, 0x79, -0x89
                            },
                            new[]
                            {
                                -0x38, 0x47, 0x48, 0x49, 0x56, 0x57, 0x58, 0x59, -0x67, 0x68, 0x69, -0x78, 0x79, -0x89
                            }
                        };

                        for (int lowRank = 10; lowRank <= 13; lowRank++)
                        {
                            if (positions.ContainsKey(lowRank))
                            {
                                if (positions.Count() > 2)
                                {
                                    return Core.RuleResult.Fail("K");
                                }
                                var otherCards = handInfo.Cards.Where(card => !card.IsJoker && card.Suit != ace.Suit).OrderBy(card => card.Rank).ToArray();
                                int rankSignature = otherCards[0].Rank * 16 + otherCards[1].Rank;
                                foreach (int rs in acceptable[lowRank - 10])
                                {
                                    if (rs == rankSignature || (otherCards[0].Suit != otherCards[1].Suit && -rs == rankSignature))
                                    {
                                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].IsJoker || handInfo.Cards[i].Suit == ace.Suit).Sum(i => 1 << i));
                                    }
                                }
                                return Core.RuleResult.Fail("K");
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 34. [1W] Deuce only
                {
                    if (handInfo.WildCount == 1)
                    {
                        return Core.RuleResult.Pass(1 << handInfo.WildPositions[0]);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 35. [2W] Wild royal flush
                {
                    if (handInfo.WildCount == 2 && handInfo.RanksPerSuit.Values.Any(array => array.Length == 3) && handInfo.RankCounts[1] + handInfo.RankCounts.GetRange(10, 4).Sum() == 3)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 36. [2W] Five of a kind
                {
                    if (handInfo.WildCount == 2 && handInfo.RankCounts.Contains(3))
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 37. [2W] Straight flush
                {
                    if (handInfo.WildCount == 2)
                    {
                        var valid = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                        if (valid.Any())
                        {
                            var ranks = valid.First().Value;
                            if (ranks.Last() - ranks.First() <= 4)
                            {
                                return Core.RuleResult.Pass(31);
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 38. [2W] Four of a kind
                {
                    if (handInfo.WildCount == 2 && handInfo.RankCounts.Contains(2))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].IsJoker || handInfo.RankCounts[handInfo.Cards[i].Rank] == 2).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 39. [2W] 4 to a wild royal flush
                {
                    if (handInfo.WildCount == 2)
                    {
                        var validRanks = handInfo.CardPositionMatrix.Where(kvp => RoyalRanks.Count(rank => kvp.Value.ContainsKey(rank)) == 2);
                        if (validRanks.Any())
                        {
                            var lookup = validRanks.First().Value;
                            return Core.RuleResult.Pass(handInfo.WildPositions.Sum(i => 1 << i) + lookup.Where(kvp => kvp.Key == 1 || kvp.Key >= 10).Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 40. [2W] 4 to a straight flush: WW67, WW78, WW89, WW9T
                {
                    if (handInfo.WildCount == 2)
                    {
                        var valid = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.Count() >= 2);
                        if (valid.Any())
                        {
                            var lookup = valid.First();
                            var ranks = handInfo.RanksPerSuit[lookup.Key];
                            for (int j = 0; j < ranks.Length - 1; j++)
                            {
                                if (ranks[j].InRange(6, 9) && ranks[j + 1] - ranks[j] == 1)
                                {
                                    return Core.RuleResult.Pass(handInfo.WildPositions.Sum(i => 1 << i) + (1 << lookup.Value[ranks[j]]) + (1 << lookup.Value[ranks[j + 1]]));
                                }
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 41. [2W] Two deuces
                {
                    if (handInfo.WildCount == 2)
                    {
                        return Core.RuleResult.Pass(handInfo.WildPositions.Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 42. [3W] Wild royal flush
                {
                    if (handInfo.WildCount == 3 && handInfo.RanksPerSuit.Values.Any(array => array.Length == 2) && handInfo.RankCounts[1] + handInfo.RankCounts.GetRange(10, 4).Sum() == 2)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 43. [3W] Five of a kind: 10s or higher
                {
                    if (handInfo.WildCount == 3 && RoyalRanks.Any(r => handInfo.RankCounts[r] == 2))
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 44. [3W] Three deuces
                {
                    if (handInfo.WildCount == 3)
                    {
                        return Core.RuleResult.Pass(handInfo.WildPositions.Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 45. [4W] Four deuces (rule J: You may keep or discard the non-deuce)
                {
                    if (handInfo.WildCount == 4)
                    {
                        return Core.RuleResult.Pass("J", handInfo.WildPositions.Sum(i => 1 << i), 31);
                    }
                    return Core.RuleResult.Fail();
                }
            );
        }
    }
}