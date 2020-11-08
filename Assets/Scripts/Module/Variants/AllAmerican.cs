using System;
using System.Collections.Generic;
using System.Linq;

namespace KtaneVideoPoker
{
    namespace Variants
    {
        public class AllAmerican: IVariant
        {
            public int JokerCount { get { return 0; }}

            private int FourOfAKindPayout;
            private bool BettingFiveCredits;

            public AllAmerican(int fourOfAKindPayout, bool bettingFiveCredits = true)
            {
                FourOfAKindPayout = fourOfAKindPayout;
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
                        return 200;
                    case Core.HandResult.FourOfAKind:
                        return FourOfAKindPayout;
                    case Core.HandResult.FullHouse:
                    case Core.HandResult.Flush:
                    case Core.HandResult.Straight:
                        return 8;
                    case Core.HandResult.ThreeOfAKind:
                        return 3;
                    case Core.HandResult.TwoPair:
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

            public static Core.Strategy Strategy40 = new Core.Strategy(
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
                (handInfo) => // 3. Full house
                {
                    var trips = handInfo.RankCounts.IndexOf(3);
                    var pair = handInfo.RankCounts.IndexOf(2);
                    if (trips > 0 && pair > 0)
                    {
                        return Core.RuleResult.Pass(31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 4. Four of a kind (rule A: You may either keep or discard the kicker.)
                {
                    var quads = handInfo.RankCounts.IndexOf(4);
                    if (quads > 0)
                    {
                        var cardIndicesToKeep = handInfo.CardPositionMatrix.Values.Select(array => array[quads]);
                        return Core.RuleResult.Pass("A", cardIndicesToKeep.Select(i => 1 << i).Sum(), 31);
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 5. 4 to a royal flush
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
                (handInfo) => // 6. 4 to an open-ended straight flush
                {
                    // Check for >= 4 because there could still be a flush
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length >= 4);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }

                    var suitAndRanks = validSuits.First();
                    for (int i = 0; i < suitAndRanks.Value.Count() - 3; i++)
                    {
                        int lowRank = suitAndRanks.Value[i];
                        if (lowRank > 1 && suitAndRanks.Value[i + 3] == lowRank + 3)
                        {
                            return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Where(kvp => kvp.Key.InRange(lowRank, lowRank + 3)).Sum(kvp => 1 << kvp.Value));
                        }
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
                (handInfo) => // 10. Three of a kind
                {
                    var trips = handInfo.RankCounts.IndexOf(3);
                    if (trips > 0)
                    {
                        var cardIndicesToKeep = CardIndices.Where(i => handInfo.Cards[i].Rank == trips);
                        return Core.RuleResult.Pass(cardIndicesToKeep.Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 11. Suited TJQ or JQK (rule B: )
                {
                    var valid = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(11) && kvp.Value.ContainsKey(12) && (kvp.Value.ContainsKey(10) != kvp.Value.ContainsKey(13)));
                    if (!valid.Any())
                    {
                        return Core.RuleResult.Fail();
                    }

                    var suitAndRankLookup = valid.First();

                    if (suitAndRankLookup.Value.ContainsKey(13) && handInfo.RankCounts[10] == 1)
                    {
                        return Core.RuleResult.Fail("B");
                    }

                    return Core.RuleResult.Pass(suitAndRankLookup.Value.Where(kvp => kvp.Key >= 10).Sum(kvp => 1 << kvp.Value));
                },
                (handInfo) => // 12. 4 to a flush (rule C: Suited TJK or TQK beats 4 to a flush if the unsuited card is a 2 through 8 or a 10.)
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 4);
                    if (validSuits.Any())
                    {
                        var suitAndRanks = validSuits.First();

                        var cardPositions = handInfo.CardPositionMatrix[suitAndRanks.Key];
                        if (cardPositions.ContainsKey(10) && cardPositions.ContainsKey(13) && (cardPositions.ContainsKey(11) || cardPositions.ContainsKey(12)))
                        {
                            var unsuitedCard = handInfo.Cards.Where(card => card.Suit != suitAndRanks.Key).First();
                            if (unsuitedCard.Rank.InRange(2, 8) || unsuitedCard.Rank == 10)
                            {
                                return Core.RuleResult.Fail("C");
                            }
                        }

                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[validSuits.First().Key].Values.Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 13. Suited TJK or TQK
                {
                    // Look for at least 3 cards instead of exactly 3.
                    // This catches fallthroughs from exception C.
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length >= 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }

                    var suitAndRanks = validSuits.First();

                    // Since rule 11 failed, this check is sufficient
                    if (suitAndRanks.Value.Count(rank => rank >= 10) >= 3)
                    {
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Where(kvp => kvp.Key >= 10).Sum(kvp => 1 << kvp.Value));
                    }

                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 14. Two pair
                {
                    if (handInfo.RankCounts.Count(n => n == 2) == 2)
                    {
                        var indexToDiscard = CardIndices.Where(i => handInfo.RankCounts[handInfo.Cards[i].Rank] == 1).First();
                        return Core.RuleResult.Pass(31 - (1 << indexToDiscard));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 15. Suited JQA, JKA, or QKA
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }

                    var suitAndRanks = validSuits.First();

                    if (suitAndRanks.Value.Count(rank => rank >= 11) >= 2 && handInfo.CardPositionMatrix[suitAndRanks.Key].ContainsKey(1))
                    {
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Where(kvp => kvp.Key >= 11 || kvp.Key == 1).Sum(kvp => 1 << kvp.Value));
                    }

                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 16. 4 to an open-ended straight, except 89TJ and JQKA (rule D: if there is also a pair, it doesn't matter which paired card you discard.)
                {
                    int runningTotal = handInfo.RankCounts.GetRange(1, 4).Sum(n => Math.Min(n, 1));
                    for (int lowRank = 2; lowRank <= 10; lowRank++)
                    {
                        runningTotal += Math.Min(handInfo.RankCounts[lowRank + 3], 1);
                        runningTotal -= Math.Min(handInfo.RankCounts[lowRank - 1], 1);
                        if (runningTotal == 4 && lowRank != 8)
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
                                return Core.RuleResult.Pass("D", strategies);
                            }
                            else
                            {
                                return Core.RuleResult.Pass(strategies);
                            }
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 17. High pair
                {
                    foreach (int rank in new[] {11, 12, 13, 1})
                    {
                        if (handInfo.RankCounts[rank] == 2)
                        {
                            return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == rank).Sum(i => 1 << i));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 18. Suited TJA, TQA, or TKA
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3 && kvp.Value[0] == 1 && kvp.Value[1] == 10);

                    if (validSuits.Any())
                    {
                        var suit = validSuits.First().Key;
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suit].Values.Sum(i => 1 << i));
                    }

                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 19. Unsuited 89TJ (rule D)
                {
                    if (handInfo.RankCounts.GetRange(8, 4).Min() == 1)
                    {
                        List<int>[] indexChoices = Enumerable.Range(0, 4).Select(i => new int[0].ToList()).ToArray();
                        for (int i = 0; i < 5; i++)
                        {
                            int arrayIndex = handInfo.Cards[i].Rank - 8;
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
                            return Core.RuleResult.Pass("D", strategies);
                        }
                        else
                        {
                            return Core.RuleResult.Pass(strategies);
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 20. 3 to a straight flush: Ace low (rule E) or 0 gaps or 1 gap
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);

                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }

                    var suitAndRanks = validSuits.First();
                    var suit = suitAndRanks.Key;
                    var ranks = suitAndRanks.Value;

                    if ((ranks[0] == 1 && ranks[2] <= 5) || ranks[2] - ranks[0] <= 3)
                    {
                        if (ranks[0] == 1)
                        {
                            // Rule E: 4 to a straight beats this 3 to a straight flush if the unsuited cards form a pair.
                            var unsuitedCards = handInfo.Cards.Where(card => card.Suit != suit);
                            if (unsuitedCards.Select(card => card.Rank).Distinct().Count() == 1 && unsuitedCards.First().Rank <= 5)
                            {
                                return Core.RuleResult.Fail("E");
                            }
                        }
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suit].Sum(kvp => 1 << kvp.Value));
                    }

                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 21. Unsuited JQKA
                {
                    var ranks = new[] {1, 11, 12, 13};
                    if (ranks.All(rank => handInfo.RankCounts[rank] == 1))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank >= 11 || handInfo.Cards[i].Rank == 1).Select(i => 1 << i).Sum());
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 22. 4 to a straight: Ace low (rule D)
                {
                    if (Enumerable.Range(1, 5).Count(rank => handInfo.RankCounts[rank] >= 1) >= 4)
                    {
                        List<int>[] indexChoices = Enumerable.Range(0, 5).Select(i => new int[0].ToList()).ToArray();
                        for (int i = 0; i < 5; i++)
                        {
                            int arrayIndex = handInfo.Cards[i].Rank - 1;
                            if (arrayIndex.InRange(0, 4))
                            {
                                indexChoices[arrayIndex].Add(i);
                            }
                        }

                        var strategies = new[] {0};

                        foreach (var choices in indexChoices.Where(list => list.Any()))
                        {
                            strategies = choices.SelectMany(choice => strategies.Select(bitmask => bitmask + (1 << choice))).ToArray();
                        }

                        if (strategies.Length > 1)
                        {
                            return Core.RuleResult.Pass("D", strategies);
                        }
                        else
                        {
                            return Core.RuleResult.Pass(strategies);
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 23. 4 to a straight: 3 high cards (rule D)
                {
                    var partialStraights = new[]
                    {
                        new[] {9, 11, 12, 13},
                        new[] {10, 11, 12, 1},
                        new[] {10, 11, 13, 1},
                        new[] {10, 12, 13, 1}
                    };
                    var matches = partialStraights.Where(partialStraight => partialStraight.Count(rank => handInfo.RankCounts[rank] >= 1) == 4);
                    if (!matches.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var match = matches.First().ToList();
                    
                    List<int>[] indexChoices = Enumerable.Range(0, 4).Select(i => new int[0].ToList()).ToArray();
                    for (int i = 0; i < 5; i++)
                    {
                        int arrayIndex = match.IndexOf(handInfo.Cards[i].Rank);
                        if (arrayIndex >= 0)
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
                        return Core.RuleResult.Pass("D", strategies);
                    }
                    else
                    {
                        return Core.RuleResult.Pass(strategies);
                    }
                },
                (handInfo) => // 24. 3 to a straight flush: 2 gaps, 2 high cards
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);

                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }

                    var suitAndRanks = validSuits.First();
                    var suit = suitAndRanks.Key;
                    var ranks = suitAndRanks.Value;

                    if (ranks[2] - ranks[0] == 4 && ranks[1] >= 11)
                    {
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suit].Sum(kvp => 1 << kvp.Value));
                    }

                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 25. 4 to a straight: 2 high cards (rule D)
                {
                    var partialStraights = new[]
                    {
                        new[] {8, 9, 11, 12},
                        new[] {8, 10, 11, 12},
                        new[] {9, 10, 11, 13},
                        new[] {9, 10, 12, 13}
                    };
                    var matches = partialStraights.Where(partialStraight => partialStraight.Count(rank => handInfo.RankCounts[rank] >= 1) == 4);
                    if (!matches.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var match = matches.First().ToList();
                    
                    List<int>[] indexChoices = Enumerable.Range(0, 4).Select(i => new int[0].ToList()).ToArray();
                    for (int i = 0; i < 5; i++)
                    {
                        int arrayIndex = match.IndexOf(handInfo.Cards[i].Rank);
                        if (arrayIndex >= 0)
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
                        return Core.RuleResult.Pass("D", strategies);
                    }
                    else
                    {
                        return Core.RuleResult.Pass(strategies);
                    }
                },
                (handInfo) => // 26. 3 to a straight flush: 2 gaps, 1 high card (rule E)
                {
                    var validSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);

                    if (!validSuits.Any())
                    {
                        return Core.RuleResult.Fail();
                    }

                    var suitAndRanks = validSuits.First();
                    var suit = suitAndRanks.Key;
                    var ranks = suitAndRanks.Value;

                    if (ranks[2] - ranks[0] == 4 && ranks[2] >= 11)
                    {
                        // Rule E: 4 to a straight beats this 3 to a straight flush if the unsuited cards form a pair.
                        var unsuitedCards = handInfo.Cards.Where(card => card.Suit != suit);
                        if (unsuitedCards.Select(card => card.Rank).Distinct().Count() == 1 && (unsuitedCards.First().Rank - ranks[0]).InRange(0, 4))
                        {
                            return Core.RuleResult.Fail("E");
                        }

                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suit].Sum(kvp => 1 << kvp.Value));
                    }

                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 27. 4 to a straight: 1 high card (rule D)
                {
                    var partialStraights = new[]
                    {
                        new[] {7, 8, 9, 11},
                        new[] {7, 8, 10, 11},
                        new[] {7, 9, 10, 11},
                        new[] {8, 9, 10, 12}
                    };
                    var matches = partialStraights.Where(partialStraight => partialStraight.Count(rank => handInfo.RankCounts[rank] >= 1) == 4);
                    if (!matches.Any())
                    {
                        return Core.RuleResult.Fail();
                    }
                    var match = matches.First().ToList();
                    
                    List<int>[] indexChoices = Enumerable.Range(0, 4).Select(i => new int[0].ToList()).ToArray();
                    for (int i = 0; i < 5; i++)
                    {
                        int arrayIndex = match.IndexOf(handInfo.Cards[i].Rank);
                        if (arrayIndex >= 0)
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
                        return Core.RuleResult.Pass("D", strategies);
                    }
                    else
                    {
                        return Core.RuleResult.Pass(strategies);
                    }
                },
                (handInfo) => // 28. Low pair
                {
                    for (int rank = 2; rank <= 10; rank++)
                    {
                        if (handInfo.RankCounts[rank] == 2)
                        {
                            return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank == rank).Sum(i => 1 << i));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                // From here, we can assume that each rank appears at most once.
                (handInfo) => // 29. 4 to a straight: 0 high cards (rules F and G)
                {
                    // A dictionary mapping each straight flush draw to the straight draws that it beats.
                    var ruleFExceptions = new Dictionary<int, HashSet<int>>
                    {
                        { 0x236, new HashSet<int>() },
                        { 0x246, new HashSet<int>(new[] { 0x4678 }) },
                        { 0x256, new HashSet<int>(new[] { 0x5679, 0x5689 }) },
                        { 0x347, new HashSet<int>() },
                        { 0x357, new HashSet<int>(new[] { 0x5789 }) },
                        { 0x367, new HashSet<int>(new[] { 0x678A, 0x679A }) },
                        { 0x458, new HashSet<int>() },
                        { 0x468, new HashSet<int>(new[] { 0x2346, 0x689A }) },
                        { 0x478, new HashSet<int>() },
                        { 0x569, new HashSet<int>(new[] { 0x2356, 0x2456 }) },
                        { 0x579, new HashSet<int>(new[] { 0x3457 }) },
                        { 0x589, new HashSet<int>() },
                        { 0x67A, new HashSet<int>(new[] { 0x3467, 0x3567 }) },
                        { 0x68A, new HashSet<int>(new[] { 0x4568, 0x4578 }) },
                        { 0x69A, new HashSet<int>() }
                    };

                    int rankMask = handInfo.Cards.Sum(card => 1 << card.Rank);

                    var goodStraightDraws = new List<int>();
                    var goodStrategies = new List<int>();

                    for (int lowRank = 2; lowRank <= 6; lowRank++)
                    {
                        for (int skippedRank = lowRank + 1; skippedRank <= lowRank + 3; skippedRank++)
                        {
                            var ranks = Enumerable.Range(lowRank, 5).Where(rank => rank != skippedRank).ToArray();
                            int mask = ranks.Sum(i => 1 << i);
                            if ((rankMask & mask) == mask)
                            {
                                // We have this straight draw
                                // Convert to a hex number so we can look up this value in RuleFExceptions later
                                int straightKey = ranks[0] * 4096 + ranks[1] * 256 + ranks[2] * 16 + ranks[3];
                                goodStraightDraws.Add(straightKey);
                                
                                int strategy = 31 - (1 << CardIndices.Where(i => !handInfo.Cards[i].Rank.InRange(lowRank, lowRank + 4)).First());
                                goodStrategies.Add(strategy);
                            }
                        }
                    }

                    if (goodStrategies.Count() == 0)
                    {
                        return Core.RuleResult.Fail();
                    }

                    var straightFlushDraws = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3 && kvp.Value[2] - kvp.Value[0] == 4);
                    if (straightFlushDraws.Any())
                    {
                        var suitAndRanks = straightFlushDraws.First();
                        var ranks = suitAndRanks.Value;

                        int straightFlushKey = ranks[0] * 256 + ranks[1] * 16 + ranks[2];
                        if (ruleFExceptions[straightFlushKey].Intersect(goodStraightDraws).Any())
                        {
                            return Core.RuleResult.Fail("F");
                        }
                    }

                    if (goodStrategies.Count() > 1)
                    {
                        return Core.RuleResult.Pass("G", goodStrategies.ToArray());
                    }
                    else
                    {
                        return Core.RuleResult.Pass(goodStrategies[0]);
                    }
                },
                (handInfo) => // 30. 3 to a straight flush: 2 gaps, 0 high cards
                {
                    var straightFlushDraws = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3 && kvp.Value[2] - kvp.Value[0] == 4);
                    if (straightFlushDraws.Any())
                    {
                        var suit = straightFlushDraws.First().Key;
                        return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suit].Sum(kvp => 1 << kvp.Value));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 31. Suited JQ (rule H: Unsuited TJQ beats suited JQ if the remaining two cards are both 7s or lower and one of them is the same suit as the Jack and Queen.)
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(11) && kvp.Value.ContainsKey(12));
                    if (validSuits.Any())
                    {
                        var suitAndRankLookup = validSuits.First();
                        if (Enumerable.SequenceEqual(handInfo.RankCounts.GetRange(8, 3), new[] {0, 0, 1}) && handInfo.RanksPerSuit[suitAndRankLookup.Key].Length >= 3)
                        {
                            return Core.RuleResult.Fail("H");
                        }
                        var rankLookup = suitAndRankLookup.Value;
                        return Core.RuleResult.Pass((1 << rankLookup[11]) | (1 << rankLookup[12]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 32. Unsuited TJQ or JQK
                {
                    if (handInfo.RankCounts[11] == 1 && handInfo.RankCounts[12] == 1 && (handInfo.RankCounts[10] == 1 || handInfo.RankCounts[13] == 1))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(10, 13)).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 33. 3 to a flush: 2 high cards
                {
                    var flushDraws = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (flushDraws.Any())
                    {
                        var suitAndRanks = flushDraws.First();
                        var ranks = suitAndRanks.Value;
                        if ((ranks[0] == 1 && ranks[2] >= 11) || ranks[1] >= 11)
                        {
                            return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 34. Suited 7TA or 8TA
                {
                    var flushDraws = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (flushDraws.Any())
                    {
                        var suitAndRanks = flushDraws.First();
                        var ranks = suitAndRanks.Value;
                        if (ranks[0] == 1 && ranks[1].InRange(7, 8) && ranks[2] == 10)
                        {
                            return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 35. Suited TJ, JK, JA, QK, QA, or KA  (rule I: Unsuited 9TJ beats suited TJ if the hand contains an Ace and a penalty card of 6 or lower.)
                {
                    var draws = new[]
                    {
                        new[] {10, 11},
                        new[] {11, 13},
                        new[] {1, 11},
                        new[] {12, 13},
                        new[] {1, 12},
                        new[] {1, 13}
                    };
                    for (int index = 0; index < draws.Length; index++)
                    {
                        var validSuits = handInfo.CardPositionMatrix.Where(kvp => draws[index].All(rank => kvp.Value.ContainsKey(rank)));
                        if (validSuits.Any())
                        {
                            var suitAndRankLookup = validSuits.First();
                            if (index == 0 && handInfo.RankCounts[1] == 1 && handInfo.RankCounts[9] == 1 && Enumerable.Range(2, 5).Any(rank => suitAndRankLookup.Value.ContainsKey(rank)))
                            {
                                return Core.RuleResult.Fail("I");
                            }
                            return Core.RuleResult.Pass(draws[index].Sum(rank => 1 << suitAndRankLookup.Value[rank]));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 36. Unsuited 9TJ, 9JQ, TJK, TQK (rule J: Suited TQ beats unsuited TQK if the remaining two cards are 8s or lower and neither is a flush penalty card.)
                {
                   foreach (var ranksToFind in new[] {new[] {9, 10, 11}, new[] {9, 11, 12}, new[] {10, 11, 13}, new[] {10, 12, 13}})
                    {
                        if (ranksToFind.All(rank => handInfo.RankCounts[rank] > 0))
                        {
                            if (ranksToFind[1] == 12)
                            {
                                var queenSuit = handInfo.Cards.Where(card => card.Rank == 12).First().Suit;
                                if (Enumerable.SequenceEqual(handInfo.RanksPerSuit[queenSuit], new[] {10, 12}) && handInfo.RankCounts.GetRange(2, 6).Sum() == 2)
                                {
                                    return Core.RuleResult.Fail("J");
                                }
                            }
                            return Core.RuleResult.Pass(CardIndices.Where(i => ranksToFind.Contains(handInfo.Cards[i].Rank)).Sum(i => 1 << i));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 37. Suited TQ (rule K: Suited 2TQ through 7TQ all beat suited TQ if the two unsuited cards are either (a) a 9 and a card with rank 7 or lower, (b) an Ace and an 8, or (c) an Ace and a 9.)
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(10) && kvp.Value.ContainsKey(12));
                    if (validSuits.Any())
                    {
                        var suitAndRankLookup = validSuits.First();
                        var suit = suitAndRankLookup.Key;
                        var rankLookup = suitAndRankLookup.Value;

                        if (handInfo.RanksPerSuit[suit][0] <= 7)
                        {
                            var unsuitedCards = handInfo.Cards.Where(card => card.Suit != suit).OrderBy(card => card.Rank).ToArray();
                            if ((unsuitedCards[1].Rank == 9 && unsuitedCards[0].Rank <= 7) || (unsuitedCards[1].Rank == 8 && unsuitedCards[0].Rank == 1))
                            {
                                return Core.RuleResult.Fail("K");
                            }

                            if (unsuitedCards[0].Rank == 1 && unsuitedCards[1].Rank.InRange(2, 7))
                            {
                                // It doesn't matter if we keep the lowest card in the flush or not
                                int threeCardStrategy = rankLookup.Values.Sum(i => 1 << i);
                                int twoCardStrategy = (1 << rankLookup[10]) | (1 << rankLookup[12]);
                                return Core.RuleResult.Pass("K", twoCardStrategy, threeCardStrategy);
                            }
                        }

                        return Core.RuleResult.Pass((1 << rankLookup[10]) | (1 << rankLookup[12]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 38. 3 to a flush containing 9J, 9Q, TQ, King, or Ace (rule L: If this flush draw contains an Ace, but not an 8, and the unsuited cards are a Jack and Queen, then you may also play this hand as unsuited JQ.)
                {
                    var flushDraws = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.Count() == 3);
                    if (flushDraws.Any())
                    {
                        var rankLookup = flushDraws.First().Value;
                        if ((rankLookup.ContainsKey(9) && (rankLookup.ContainsKey(11) || rankLookup.ContainsKey(12))) || (rankLookup.ContainsKey(10) && rankLookup.ContainsKey(12)) || rankLookup.ContainsKey(13) || rankLookup.ContainsKey(1))
                        {
                            if (rankLookup.ContainsKey(1) && !rankLookup.ContainsKey(8) && handInfo.RankCounts[11] == 1 && handInfo.RankCounts[12] == 1)
                            {
                                int threeCardStrategy = rankLookup.Sum(kvp => 1 << kvp.Value);
                                return Core.RuleResult.Pass("L", threeCardStrategy, 31 - threeCardStrategy);
                            }
                            return Core.RuleResult.Pass(rankLookup.Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 39. Unsuited JQ (rule M: 3 to a flush beats unsuited JQ if the flush draw contains an 8 and the fifth card is an Ace. If the fifth card is an Ace but the flush draw doesn't contain an 8, then both plays are of equal value.)
                {
                    if (handInfo.RankCounts[11] == 0 || handInfo.RankCounts[12] == 0)
                    {
                        return Core.RuleResult.Fail();
                    }

                    int jackQueenStrategy = CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(11, 12)).Sum(i => 1 << i);

                    var flushDraws = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (flushDraws.Any() && handInfo.RankCounts[1] == 1)
                    {
                        var flushDraw = flushDraws.First();
                        if (flushDraw.Value[0].InRange(2, 7) && flushDraw.Value[2].InRange(11, 12))
                        {
                            if (flushDraw.Value[1] == 8)
                            {
                                return Core.RuleResult.Fail("M");
                            }
                            else
                            {
                                int flushStrategy = handInfo.CardPositionMatrix[flushDraw.Key].Sum(kvp => 1 << kvp.Value);
                                return Core.RuleResult.Pass("M", flushStrategy, jackQueenStrategy);
                            }
                        }
                    }
                    return Core.RuleResult.Pass(jackQueenStrategy);
                },
                (handInfo) => // 40. 3 to a flush containing a Jack or Queen
                {
                    var flushDraws = handInfo.RanksPerSuit.Where(kvp => kvp.Value.Length == 3);
                    if (flushDraws.Any())
                    {
                        var suitAndRanks = flushDraws.First();
                        var ranks = suitAndRanks.Value;
                        if (ranks.Intersect(new[] {11, 12}).Any())
                        {
                            return Core.RuleResult.Pass(handInfo.CardPositionMatrix[suitAndRanks.Key].Sum(kvp => 1 << kvp.Value));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 41. Unsuited JKA or QKA
                {
                    if (handInfo.RankCounts[13] == 1 && handInfo.RankCounts[1] == 1 && (handInfo.RankCounts[11] == 1 || handInfo.RankCounts[12] == 1))
                    {
                        return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank >= 11 || handInfo.Cards[i].Rank == 1).Sum(i => 1 << i));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 42. Suited 9J
                {
                    var candidates = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(9) && kvp.Value.ContainsKey(11));
                    if (candidates.Any())
                    {
                        if (handInfo.RankCounts[8] == 1 && handInfo.RankCounts[13] == 1)
                        {
                            return Core.RuleResult.Fail("N");
                        }
                        var lookup = candidates.First().Value;
                        return Core.RuleResult.Pass((1 << lookup[9]) | (1 << lookup[11]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 43. Suited TK
                {
                    var candidates = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.ContainsKey(10) && kvp.Value.ContainsKey(13));
                    if (candidates.Any())
                    {
                        var lookup = candidates.First().Value;
                        return Core.RuleResult.Pass((1 << lookup[10]) | (1 << lookup[13]));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 44. Unsuited JK, JA, QK, QA, or KA (rules O, P, Q)
                {
                    var ranksToFind = new[]
                    {
                        new[] {11, 13},
                        new[] {11, 1},
                        new[] {12, 13},
                        new[] {12, 1},
                        new[] {13, 1}
                    };

                    for (int index = 0; index < ranksToFind.Length; index++)
                    {
                        if (ranksToFind[index].All(rank => handInfo.RankCounts[rank] == 1))
                        {
                            if (index == 1)
                            {
                                var jackSuit = handInfo.Cards.Where(card => card.Rank == 11).First().Suit;
                                var twoThroughSixCounts = handInfo.RankCounts.GetRange(2, 5).Sum();
                                if (((handInfo.RankCounts[6] == 1 && twoThroughSixCounts == 3) || handInfo.RanksPerSuit[jackSuit].Length == 1) && handInfo.RankCounts[9] == 0)
                                {
                                    // Rule O: Jack only beats unsuited JA if either (a) the highest non-royal card in the hand is a 6, or (b) no other card in the hand has the same suit as the Jack. However, if the hand contains a 9, always play unsuited JA.
                                    return Core.RuleResult.Fail("O");
                                }
                                if (handInfo.CardPositionMatrix[jackSuit].ContainsKey(8) && handInfo.RankCounts[7] == 0 && handInfo.RankCounts[9] == 0 && handInfo.RankCounts[10] == 0)
                                {
                                    // Rule P: Suited 8J beats unsuited JA and Jack only if the former doesn't have a straight penalty card.
                                    return Core.RuleResult.Fail("P");
                                }
                            }

                            if (index == 3)
                            {
                                var queenSuit = handInfo.Cards.Where(card => card.Rank == 12).First().Suit;
                                if (handInfo.CardPositionMatrix[queenSuit].ContainsKey(9) && handInfo.RankCounts[8] == 0 && handInfo.RankCounts[10] == 0)
                                {
                                    // Rule Q: Suited 9Q beats unsuited QA if the hand does not contain an 8 or a 10.
                                    return Core.RuleResult.Fail("Q");
                                }
                            }

                            return Core.RuleResult.Pass(CardIndices.Where(i => ranksToFind[index].Contains(handInfo.Cards[i].Rank)).Sum(i => 1 << i));
                        }
                    }

                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 45. Single Jack or King (rule P: Suited 8J beats unsuited JA and Jack only if the former doesn't have a straight penalty card.)
                {
                    if (handInfo.RankCounts[13] == 1)
                    {
                        return Core.RuleResult.Pass(1 << CardIndices.Where(i => handInfo.Cards[i].Rank == 13).First());
                    }

                    if (handInfo.RankCounts[11] == 1)
                    {
                        int jackPosition = CardIndices.Where(i => handInfo.Cards[i].Rank == 11).First();
                        if (handInfo.CardPositionMatrix[handInfo.Cards[jackPosition].Suit].ContainsKey(8) && handInfo.RankCounts[7] == 0 && handInfo.RankCounts[9] == 0 && handInfo.RankCounts[10] == 0)
                        {
                            // We didn't need to check for queens because we would've kept unsuited JQ before.
                            return Core.RuleResult.Fail("P");
                        }
                        return Core.RuleResult.Pass(1 << jackPosition);
                    }

                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 46. Suited 8J, 9Q, or TA
                {
                    var ranksToFind = new[]
                    {
                        new[] {8, 11},
                        new[] {9, 12},
                        new[] {10, 1}
                    };

                    foreach (var ranks in ranksToFind)
                    {
                        var validSuits = handInfo.CardPositionMatrix.Where(kvp => ranks.All(rank => kvp.Value.ContainsKey(rank)));
                        if (validSuits.Any())
                        {
                            var rankLookup = validSuits.First().Value;
                            return Core.RuleResult.Pass(ranks.Sum(rank => 1 << rankLookup[rank]));
                        }
                    }

                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 47. Single Queen or Ace
                {
                    if (handInfo.RankCounts[12] == 1)
                    {
                        return Core.RuleResult.Pass(1 << CardIndices.Where(i => handInfo.Cards[i].Rank == 12).First());
                    }

                    if (handInfo.RankCounts[1] == 1)
                    {
                        return Core.RuleResult.Pass(1 << CardIndices.Where(i => handInfo.Cards[i].Rank == 1).First());
                    }

                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 48. 3 to a straight: 345, 456, 567, 678, 789, or 89T
                {
                    int total = handInfo.RankCounts.GetRange(2, 3).Sum();
                    for (int lowRank = 3; lowRank <= 8; lowRank++)
                    {
                        total += handInfo.RankCounts[lowRank + 2] - handInfo.RankCounts[lowRank - 1];
                        if (total == 3)
                        {
                            return Core.RuleResult.Pass(CardIndices.Where(i => handInfo.Cards[i].Rank.InRange(lowRank, lowRank + 2)).Sum(i => 1 << i));
                        }
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 49. 3 to a flush: anything not previously listed
                {
                    var validSuits = handInfo.CardPositionMatrix.Where(kvp => kvp.Value.Count() == 3);
                    if (validSuits.Any())
                    {
                        return Core.RuleResult.Pass(validSuits.First().Value.Sum(kvp => 1 << kvp.Value));
                    }
                    return Core.RuleResult.Fail();
                },
                (handInfo) => // 50. 2 to a straight flush (take the first one that applies, except as indicated by rule R): 67, 9T, 45, 56, 78, 89, 79, 8T, 34, 35, 46, 57, 68
                {
                    var ranksToFind = new[]
                    {
                        new[] {6, 7}, // 0
                        new[] {9, 10}, // 1
                        new[] {4, 5}, // 2
                        new[] {5, 6}, // 3
                        new[] {7, 8}, // 4
                        new[] {8, 9}, // 5
                        new[] {7, 9}, // 6
                        new[] {8, 10}, // 7
                        new[] {3, 4}, // 8
                        new[] {3, 5}, // 9
                        new[] {4, 6}, // 10
                        new[] {5, 7}, // 11
                        new[] {6, 8} // 12
                    };

                    // Ties and exceptions are specified as follows:
                    // [upperStraightFlushIndex: [lowerStraightFlushIndex: unsuitedRank]]
                    var ruleSTies = new Dictionary<int, Dictionary<int, int>>
                    {
                        { 1, new Dictionary<int, int> { {2, 7} } },
                        { 7, new Dictionary<int, int> { {11, 2} } },
                        { 8, new Dictionary<int, int> { {12, 10} } }
                    };

                    var ruleSExceptions = new Dictionary<int, Dictionary<int, int>>
                    {
                        { 2, new Dictionary<int, int> { {5, 2} } },
                        { 6, new Dictionary<int, int> { {8, 10}, {9, 10} } },
                        { 7, new Dictionary<int, int> { {8, 7}, {9, 7} } }
                    };

                    for (int i = 0; i < ranksToFind.Length; i++)
                    {
                        var validSuits = handInfo.RanksPerSuit.Where(kvp => Enumerable.SequenceEqual(ranksToFind[i], kvp.Value));
                        if (validSuits.Any())
                        {
                            var suit = validSuits.First().Key;
                            int suitStrategy = handInfo.CardPositionMatrix[suit].Sum(kvp => 1 << kvp.Value);

                            if (ruleSTies.ContainsKey(i))
                            {
                                var otherTwoCardSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Key != suit && kvp.Value.Length == 2);
                                if (otherTwoCardSuits.Any())
                                {
                                    var otherSuitAndRanks = otherTwoCardSuits.First();
                                    var otherIndices = ruleSTies[i].Where(kvp => Enumerable.SequenceEqual(ranksToFind[kvp.Key], otherSuitAndRanks.Value));
                                    if (otherIndices.Any() && handInfo.RankCounts[otherIndices.First().Value] == 1)
                                    {
                                        // Tie, accept either option
                                        var otherRanks = ranksToFind[otherIndices.First().Key];
                                        int otherStrategy = CardIndices.Where(k => otherRanks.Contains(handInfo.Cards[k].Rank)).Sum(k => 1 << k);
                                        return Core.RuleResult.Pass("R", suitStrategy, otherStrategy);
                                    }
                                }
                            }

                            if (ruleSExceptions.ContainsKey(i))
                            {
                                var otherTwoCardSuits = handInfo.RanksPerSuit.Where(kvp => kvp.Key != suit && kvp.Value.Length == 2);
                                if (otherTwoCardSuits.Any())
                                {
                                    var otherSuitAndRanks = otherTwoCardSuits.First();
                                    var otherIndices = ruleSExceptions[i].Where(kvp => Enumerable.SequenceEqual(ranksToFind[kvp.Key], otherSuitAndRanks.Value));
                                    if (otherIndices.Any() && handInfo.RankCounts[otherIndices.First().Value] == 1)
                                    {
                                        // Tie, accept either option
                                        var otherRanks = ranksToFind[otherIndices.First().Key];
                                        int otherStrategy = CardIndices.Where(k => otherRanks.Contains(handInfo.Cards[k].Rank)).Sum(k => 1 << k);
                                        return Core.RuleResult.Pass("R", otherStrategy);
                                    }
                                }
                            }

                            return Core.RuleResult.Pass(suitStrategy);
                        }
                    }

                    return Core.RuleResult.Fail();
                }
            );
        }
    }
}