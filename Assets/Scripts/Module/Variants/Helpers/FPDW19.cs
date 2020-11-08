using System.Collections.Generic;
using System.Linq;

namespace KtaneVideoPoker
{
    namespace Variants
    {
        namespace Helpers
        {
            class FPDW19
            {
                private static int[][] Parse(string str)
                {
                    return str.Split(new[] {' '}).Select(s =>
                    {
                        var rankCharToBitmap = "A23456789TJQK".Select((c, i) => new KeyValuePair<char, int>(c, 2 << i)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                        var bitmaps = s.Split(new[] {'/'}).Select(ranksPerSuit => ranksPerSuit.Sum(c => rankCharToBitmap[c])).ToList();
                        while (bitmaps.Count() < 4)
                        {
                            bitmaps.Add(0);
                        }
                        bitmaps.Sort();
                        return bitmaps.ToArray();
                    }).ToArray();
                }

                public static int[][] NoPenaltyExceptions = Parse("QK/348 JK/34/5 JK/34/6 QK/34/5 QK/34/6 QK/34/7 QK/35/6 JK/35/4 QK/35/4 QK/36/4 QK/36/5 JK/45/3 JK/46/3 QK/45/3 QK/46/3 QK/47/3 QK/56/3 JK/3/4/5 QK/3/4/5 QK/3/4/6");

                public static int[][] PenaltyExceptions = Parse("TK/94/7 TK/94/8 TK/95/6 TK/95/7 TK/95/8 JK/95/7 JK/95/8 JK/96/7 JK/96/8 QK/96/7 QK/96/8 QK/97/8 TK/A7/8 TK/97/4 TK/98/4 TK/96/5 TK/97/5 TK/98/5 JK/97/5 JK/98/5 JK/97/6 JK/98/6 QK/97/6 QK/98/7 TK/A8/7 TK/9/47 TK/9/48 TK/9/56 TK/9/57 TK/9/58 JK/9/57 JK/9/58 JK/9/67 JK/9/68 QK/9/67 QK/9/78 TK/A/78 TK/9/3/7 TK/9/4/6 TK/9/4/7 TK/9/4/8 TK/9/5/6 TK/9/5/7 TK/9/5/8 JK/9/4/7 JK/9/4/8 JK/9/5/6 JK/9/5/7 JK/9/5/8 JK/9/6/7 JK/9/6/8 QK/9/5/7 QK/9/5/8 QK/9/6/7 QK/9/6/8 QK/9/7/8 TK/A/6/7 TK/A/6/8 TK/A/7/8 JK/A/7/8");
            }
        }
    }
}