using System;
using System.Collections.Generic;
using System.Linq;
using WavesCS;

namespace wavesminingprobability
{
    static class NodeExtentions
    {
        public static decimal GetGenBalance(this Node node, string address)
        {
            return node.GetObject($"addresses/effectiveBalance/{address}/1000").GetLong("balance");
        }
    }

    class MainClass
    {
        

        public static void Main(string[] args)
        {
            var address = "3P4QYFDHZqLw94MY1XNEwxdXpFiDuaLjTDu";

            Node node = new Node(Node.MainNetChainId);

            var currentHeight = node.GetHeight();
            var currentTimestamp = node.GetBlockTimestamp(currentHeight);

            var firstBlockHeight = currentHeight - 1440;
            var firstBlockTimestamp = node.GetBlockTimestamp(firstBlockHeight);

            const long millisecondsInOneDay = 24 * 60 * 60 * 1000;

            while (currentTimestamp - firstBlockTimestamp > millisecondsInOneDay)
            {
                firstBlockHeight++;
                firstBlockTimestamp = node.GetBlockTimestamp(firstBlockHeight);
            }

            while (currentTimestamp - firstBlockTimestamp < millisecondsInOneDay)
            {
                firstBlockHeight--;
                firstBlockTimestamp = node.GetBlockTimestamp(firstBlockHeight);
            }

            var numBlocks = currentHeight - firstBlockHeight + 1;
            Console.WriteLine($"number of blocks = {numBlocks}");

            var minedBlocks = 0;
            var generators = new Dictionary<string, bool>();
            for (int h = firstBlockHeight; h < currentHeight; h += 100)
            {
                var headers = node.GetObjects($"blocks/headers/seq/{h}/{h + 99}");

                foreach (var gen in headers.Select(header => header.GetString("generator")))
                {
                    generators[gen] = true;
                }

                minedBlocks += headers.Count(header => header.GetString("generator") == address);

            }
            
            var totalGenBalance = 0m;
            foreach (var generator in generators.Keys)
            {
                totalGenBalance += node.GetGenBalance(generator);
            }

            var share = node.GetGenBalance(address) / totalGenBalance;

            var estimatedBlocks = share * numBlocks;
            var performanceRatio = minedBlocks / estimatedBlocks;

            Console.WriteLine($"stake share = {share}");
            Console.WriteLine($"estimated blocks = {estimatedBlocks}");
            Console.WriteLine($"mined blocks = {minedBlocks}");
            Console.WriteLine($"performance ratio = {performanceRatio}");
        }
    }
}
