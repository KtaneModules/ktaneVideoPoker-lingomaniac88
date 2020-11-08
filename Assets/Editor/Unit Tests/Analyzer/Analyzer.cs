using System;
using System.Collections.Generic;
using System.Linq;

namespace KtaneVideoPoker
{
    using Core;

    namespace Analyzer
    {
        public static class Analyzer
        {
            /// <summary>Calculates optimal plays for each poker hand for a given variant of video poker.
            /// The algorithm is adapted from https://wizardofodds.com/games/video-poker/methodology/.</summary>
            public static AnalysisResult Analyze(Variants.IVariant variant)
            {
                int deckSize = Util.StandardDeckSize + variant.JokerCount;

                var grandPayoutTable = Enumerable.Range(0, 6).Select(r => Enumerable.Repeat(0L, (int) Util.Ncr(deckSize, r)).ToArray()).ToArray();

                var allCards = Enumerable.Range(0, deckSize).Select(Card.CreateWithId);

                var analysis = new AnalysisResult(deckSize);

                // Determine total payouts for subsets of cards
                foreach (var cards in allCards.Combinations(5))
                {
                    var hand = new Hand(cards);
                    long payout = variant.PayoutForResult(variant.Evaluate(hand));

                    var indices = cards.Select(card => card.Id).ToArray();
                    int i0 = indices[0];
                    int i1 = indices[1];
                    int i2 = indices[2];
                    int i3 = indices[3];
                    int i4 = indices[4];

                    grandPayoutTable[5][Util.CombinedId(deckSize, i0, i1, i2, i3, i4)] += payout;
                    grandPayoutTable[4][Util.CombinedId(deckSize, i0, i1, i2, i3)] += payout;
                    grandPayoutTable[4][Util.CombinedId(deckSize, i0, i1, i2, i4)] += payout;
                    grandPayoutTable[4][Util.CombinedId(deckSize, i0, i1, i3, i4)] += payout;
                    grandPayoutTable[4][Util.CombinedId(deckSize, i0, i2, i3, i4)] += payout;
                    grandPayoutTable[4][Util.CombinedId(deckSize, i1, i2, i3, i4)] += payout;
                    grandPayoutTable[3][Util.CombinedId(deckSize, i0, i1, i2)] += payout;
                    grandPayoutTable[3][Util.CombinedId(deckSize, i0, i1, i3)] += payout;
                    grandPayoutTable[3][Util.CombinedId(deckSize, i0, i1, i4)] += payout;
                    grandPayoutTable[3][Util.CombinedId(deckSize, i0, i2, i3)] += payout;
                    grandPayoutTable[3][Util.CombinedId(deckSize, i0, i2, i4)] += payout;
                    grandPayoutTable[3][Util.CombinedId(deckSize, i0, i3, i4)] += payout;
                    grandPayoutTable[3][Util.CombinedId(deckSize, i1, i2, i3)] += payout;
                    grandPayoutTable[3][Util.CombinedId(deckSize, i1, i2, i4)] += payout;
                    grandPayoutTable[3][Util.CombinedId(deckSize, i1, i3, i4)] += payout;
                    grandPayoutTable[3][Util.CombinedId(deckSize, i2, i3, i4)] += payout;
                    grandPayoutTable[2][Util.CombinedId(deckSize, i0, i1)] += payout;
                    grandPayoutTable[2][Util.CombinedId(deckSize, i0, i2)] += payout;
                    grandPayoutTable[2][Util.CombinedId(deckSize, i0, i3)] += payout;
                    grandPayoutTable[2][Util.CombinedId(deckSize, i0, i4)] += payout;
                    grandPayoutTable[2][Util.CombinedId(deckSize, i1, i2)] += payout;
                    grandPayoutTable[2][Util.CombinedId(deckSize, i1, i3)] += payout;
                    grandPayoutTable[2][Util.CombinedId(deckSize, i1, i4)] += payout;
                    grandPayoutTable[2][Util.CombinedId(deckSize, i2, i3)] += payout;
                    grandPayoutTable[2][Util.CombinedId(deckSize, i2, i4)] += payout;
                    grandPayoutTable[2][Util.CombinedId(deckSize, i3, i4)] += payout;
                    grandPayoutTable[1][i0] += payout;
                    grandPayoutTable[1][i1] += payout;
                    grandPayoutTable[1][i2] += payout;
                    grandPayoutTable[1][i3] += payout;
                    grandPayoutTable[1][i4] += payout;
                    grandPayoutTable[0][0] += payout;
                }

                IEnumerable<bool[]> bitFields = Enumerable.Range(0, 32).Select(n => Enumerable.Range(0, 5).Select(i => (n & (1 << i)) == (1 << i)).ToArray());

                var bitCounts = bitFields.Select(bitField => bitField.Count(b => b)).ToArray();

                var pieBuildingBlocks = Enumerable.Range(0, 32).Select(i => Enumerable.Range(0, 32).Where(j => (i & j) == i).Select(j => j * ((bitCounts[j & ~i] % 2 == 1) ? -1 : 1))).ToArray();

                long rawPayoutMultiplier = Util.LcmOfNChoose0To5(deckSize - 5);

                foreach (var handClass in HandClass.AllHandClassesWithMaximumJokerCount(variant.JokerCount))
                {
                    var sampleHand = handClass.GetSampleHand(deckSize);
                    var cardIds = sampleHand.Cards.Select(card => card.Id).OrderBy(n => n);
                    
                    // Fetch all relevant values from the grand payout table
                    var cardIdSelections = bitFields.Select(bitField => cardIds.Where((n, i) => bitField[i])); 
                    var combinedIds = cardIdSelections.Select(ids => Util.CombinedId(deckSize, ids));
                    var rawPayoutTableLookups = combinedIds.Select((ids, i) => grandPayoutTable[bitCounts[i]][ids]).ToArray();

                    // Determine the payout for each strategy
                    // A strategy is represented by a bit field where bit i being set means to keep sampleHand.Cards[i]
                    var payoutsPerStrategy = bitFields.Select((bitField, i) =>
                    {
                        var rawPayouts = pieBuildingBlocks[i].Select(j => (j >= 0 ? 1 : -1) * rawPayoutTableLookups[Math.Abs(j)]);
                        // Multiply by a constant (to ensure integer division in the next step) and divide by the number of possible replacement draws
                        return rawPayoutMultiplier / Util.Ncr(deckSize - 5, 5 - bitCounts[i]) * rawPayouts.Sum();
                    }).ToArray();

                    long bestPayout = payoutsPerStrategy.Max();
                    
                    var optimalStrategyArray = Enumerable.Range(0, 32).Where(i => payoutsPerStrategy[i] == bestPayout);

                    // The strategies we generated are relative to the list of sorted cards, which may not necessarily be the same as what we see here.
                    var sampleIndices = sampleHand.GetCardIdSortIndices();

                    foreach (var hand in handClass.GetAllHands(deckSize))
                    {
                        long combinedId = Util.CombinedId(deckSize, hand.Cards.Select(card => card.Id).OrderBy(n => n));
                        analysis.ExpectedPayouts[combinedId] = bestPayout;
                        
                        // Translate the each sorted optimal strategy from the sample hand to this hand
                        uint optimalStrategies = (uint) optimalStrategyArray.Select(sampleIndexStrategy =>
                        {
                            var trueIndices = hand.GetCardIdSortIndices();
                            return Enumerable.Range(0, 5).Where(i => (sampleIndexStrategy & (1 << sampleIndices[i])) != 0).Select(i => 1 << trueIndices[i]).Sum();
                        }).Select(trueStrategy => 1 << trueStrategy).Sum();
                        
                        analysis.OptimalStrategies[combinedId] = optimalStrategies;
                    }
                }

                return analysis;
            }
        }
    }
}