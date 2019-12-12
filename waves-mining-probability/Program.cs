using System;
using System.Collections.Generic;
using System.Linq;
using WavesCS;

namespace wavesminingprobability
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            string address;

            if (args.Length == 0)
            {
                Console.Write("address = ");
                address = Console.ReadLine();
            }
            else
            {
                address = args[0];
                Console.Write($"address = {address}");
            }

            const long millisecondsInOneDay = 24 * 60 * 60 * 1000;
            const long expectedBlocksInOneDay = 24 * 60;

            Node node = new Node(Node.MainNetChainId);

            var currentHeight = node.GetHeight();
            var currentTimestamp = node.GetBlockTimestamp(currentHeight);

            var firstBlockHeight = currentHeight - expectedBlocksInOneDay;
            var firstBlockTimestamp = node.GetBlockTimestamp(firstBlockHeight);

            while (currentTimestamp - firstBlockTimestamp < millisecondsInOneDay)
            {
                firstBlockHeight--;
                firstBlockTimestamp = node.GetBlockTimestamp(firstBlockHeight);
            }

            while (currentTimestamp - firstBlockTimestamp > millisecondsInOneDay)
            {
                firstBlockHeight++;
                firstBlockTimestamp = node.GetBlockTimestamp(firstBlockHeight);
            }

            var minedBlocks = 0;
            var generators = new Dictionary<string, bool>();

            const long maxSequenceLength = 100;

            for (var h = firstBlockHeight; h <= currentHeight; h += maxSequenceLength)
            {
                var headers = node.GetHeadersSequence(h,Math.Min(h + maxSequenceLength - 1, currentHeight));

                foreach (var generator in headers.Select(header => header.GetString("generator")))
                {
                    generators[generator] = true;
                }

                minedBlocks += headers.Count(header => header.GetString("generator") == address);
            }

            var totalGenBalance = generators.Keys.Sum(node.GetGenBalance);
            var share = node.GetGenBalance(address) / totalGenBalance;
            var numBlocks = currentHeight - firstBlockHeight + 1;
            var estimatedBlocks = share * numBlocks;
            var performanceRatio = minedBlocks / estimatedBlocks;

            const int decimals = 2;
            Console.WriteLine($"number of blocks = {numBlocks}");
            Console.WriteLine($"stake share = {Math.Round(share * 100, decimals)}%");
            Console.WriteLine($"estimated blocks = {Math.Round(estimatedBlocks, decimals)}");
            Console.WriteLine($"mined blocks = {minedBlocks}");
            Console.WriteLine($"performance ratio = {Math.Round(performanceRatio, decimals)}");
        }
    }

    static class NodeExtentions
    {
        public static decimal GetGenBalance(this Node node, string address)
        {
            return node.GetObject($"addresses/effectiveBalance/{address}/1000").GetLong("balance");
        }

        public static IEnumerable<Dictionary<string, object>> GetHeadersSequence(this Node node, long from, long to)
        {
            return node.GetObjects($"blocks/headers/seq/{from}/{to}");
        }
    }
}
