using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavesCS
{
    public class Block
    {
        char chainId;
        long count;

        public Block(char chainId, long count=100)
        {
            this.chainId = chainId;
            this.count = count;
        }

        public IEnumerator GetEnumerator()
        {
            return new BlocksEnumerator(chainId, count);
        }
    }


    public class BlocksEnumerator : IEnumerator
    {
        Node node;
        long height = -1;
        long position = -1;
        long count = -1;


        public BlocksEnumerator(char chainId, long count=100)
        {
            node = new Node(chainId);
            height = node.GetHeight();
            position = height + 1;
            this.count = count;
        }

        public object Current
        {
            get
            {
                if (height == -1 || count == -1)
                    throw new InvalidOperationException();
                return node.GetBlockTransactionsAtHeight(position);
            }
        }

        public bool MoveNext()
        {
            if (position <= height + 1 && position > height - count)
            {
                position--;
                return true;
            }
            else
                return false;
        }

        public void Reset()
        {
            position = -1;
        }
    }
}
