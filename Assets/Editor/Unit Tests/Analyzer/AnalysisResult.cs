using System;
using System.Linq;
using System.IO;
using System.IO.Compression;

namespace KtaneVideoPoker
{
    using Core;

    namespace Analyzer
    {
        public class AnalysisResult
        {
            public readonly long[] ExpectedPayouts;

            public readonly uint[] OptimalStrategies;

            private double RawToRatio;

            public double ExpectedReturn
            {
                get
                {
                    return ExpectedPayouts.Sum() / RawToRatio;
                }
            }

            public AnalysisResult(int deckSize)
            {
                int length = (int) Util.Ncr(deckSize, 5);
                ExpectedPayouts = Enumerable.Repeat(0L, length).ToArray();
                OptimalStrategies = Enumerable.Repeat(0u, length).ToArray();

                RawToRatio = Util.LcmOfNChoose0To5(deckSize - 5) * length;
            }

            public static AnalysisResult LoadFromFile(string localPath)
            {
                /*var file = File.Open(localPath, FileMode.Open);
                var stream = new GZipStream(file, CompressionMode.Decompress);

                var output = new MemoryStream();

                var buffer = new byte[65536];
                while (true)
                {
                    int count = stream.Read(buffer, 0, buffer.Length);
                    if (count != 0)
                    {
                        output.Write(buffer, 0, count);
                        if (count != buffer.Length)
                        {
                            break;
                        }
                    }
                }

                file.Close();*/
                var buffer = File.ReadAllBytes(localPath);

                if (buffer.Length % 12 != 0)
                {
                    throw new FormatException(string.Format("Length of file, {0}, is not a multiple of 12", buffer.Length));
                }

                int numberOfHands = (int) (buffer.Length / 12);

                // Try to infer the deck size. It'll be at least 52.
                int inferredDeckSize = 52;
                while (Util.Ncr(inferredDeckSize, 5) < numberOfHands)
                {
                    inferredDeckSize++;
                    // Assume we don't have more than 8 jokers
                    if (inferredDeckSize > 60)
                    {
                        break;
                    }
                }

                if (Util.Ncr(inferredDeckSize, 5) != numberOfHands)
                {
                    throw new FormatException(string.Format("Unable to infer deck size from {0} hands.", numberOfHands));
                }

                var analysis = new AnalysisResult(inferredDeckSize);

                Buffer.BlockCopy(buffer, 0, analysis.ExpectedPayouts, 0, numberOfHands * sizeof(long));
                Buffer.BlockCopy(buffer, numberOfHands * sizeof(long), analysis.OptimalStrategies, 0, numberOfHands * sizeof(uint));

                return analysis;
            }

            public void WriteToFile(string localPath)
            {
                var buffer = new byte[ExpectedPayouts.Length * sizeof(long) + OptimalStrategies.Length * sizeof(uint)];
                Buffer.BlockCopy(ExpectedPayouts, 0, buffer, 0, ExpectedPayouts.Length * sizeof(long));
                Buffer.BlockCopy(OptimalStrategies, 0, buffer, ExpectedPayouts.Length * sizeof(long), OptimalStrategies.Length * sizeof(uint));

                /*var output = new MemoryStream();
                var stream = new GZipStream(output, CompressionMode.Compress);
                stream.Write(buffer, 0, buffer.Length);

                File.WriteAllBytes(localPath, output.ToArray());*/
                File.WriteAllBytes(localPath, buffer);
            }
        }
    }
}