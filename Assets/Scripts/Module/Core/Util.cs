using System.Collections.Generic;
using System.Linq;

namespace KtaneVideoPoker
{
    namespace Core
    {
        public static class Util
        {
            /// <summary>The number of cards in a standard deck of cards.</summary>
            public static readonly int StandardDeckSize = 52;

            public static long CombinedId(int deckSize, params int[] ids)
            {
                return CombinedId(deckSize, ids.AsEnumerable());
            }

            public static long CombinedId(int deckSize, IEnumerable<int> ids)
            {
                var idArray = ids.ToArray();
                int length = idArray.Length;

                if (length == 0)
                {
                    return 0;
                }

                long ans = Ncr(deckSize, length) - Ncr(deckSize - idArray[0], length);
                for (int i = 1; i < length; i++)
                {
                    ans += Ncr(deckSize - 1 - idArray[i - 1], length - i) - Ncr(deckSize - idArray[i], length - i);
                }
                return ans;
            }

            public static long LcmOfNChoose0To5(int n)
            {
                var ncrArray = Enumerable.Range(0, 6).Select(r => Ncr(n, r));

                // This will be n * (n - 1) * (n - 2) * (n - 3) * (n - 4)
                var answer = ncrArray.Last() * 120;

                // This is a common multiple of every element of ncrArray, but it might not be the LEAST common multiple.
                // If anything, it's the LCM times 2^a * 3^b * 5^c for some nonnegative integers a, b, and c.
                foreach (int p in new[] {2, 3, 5})
                {
                    while (ncrArray.All(ncr => (answer / ncr) % p == 0))
                    {
                        answer /= p;
                    }
                }

                return answer;
            }

            public static long Ncr(int n, int r)
            {
                long result = 1;
                for (int i = 0; i < r; i++)
                {
                    result *= n - i;
                    result /= i + 1;
                }
                return result;
            }

            /// <summary>Returns a list of combinations of unordered terms
            /// Adapted slightly from https://stackoverflow.com/questions/33336540/.</summary>
            public static IEnumerable<IEnumerable<T>> Combinations<T>(this IEnumerable<T> elements, int k, bool repeatsAllowed = false)
            {
                if (k == 0)
                {
                    return new[] { new T[0] };
                }
                else
                {
                    return elements.SelectMany((e, i) => elements.Skip(i + (repeatsAllowed ? 0 : 1)).Combinations(k - 1).Select(c => (new[] {e}).Concat(c)));
                }
            }

            /// <summary>Returns a list of permutations of terms
            /// Adapted slightly from https://stackoverflow.com/questions/33336540/.</summary>
            public static IEnumerable<IEnumerable<T>> Permutations<T>(this IEnumerable<T> elements, int k)
            {
                if (k == 0)
                {
                    return new[] { new T[0] };
                }
                else
                {
                    return elements.SelectMany(e => elements.Except(new[] {e}).Permutations(k - 1).Select(c => (new[] {e}).Concat(c)));
                }
            }

            /// <summary>Returns a list of permutations of terms
            /// Adapted slightly from https://stackoverflow.com/questions/33336540/.</summary>
            public static IEnumerable<IEnumerable<T>> Permutations<T>(this IEnumerable<T> elements)
            {
                return elements.Permutations(elements.Count());
            }
        }
    }
}