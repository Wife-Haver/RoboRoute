using System;
using System.Collections.Generic;

namespace RoboRoute.RoboRoute.Commands
{
    // One block in the player's program. Normal blocks (Move, Turn...) just have a Type.
    // A Repeat block also has a Count and a list of child blocks that it runs Count times.
    // Because Children can contain more Repeat blocks, programs are a tree, not a flat list.
    public class Block
    {
        public const int MinCount = 1;
        public const int MaxCount = 99;

        public BlockType Type { get; }

        // Repeat blocks only: how many times to run the children
        public int Count { get; set; } = 3;

        // Repeat blocks only: the blocks inside the loop, in order
        public List<Block> Children { get; } = new List<Block>();

        public bool IsLoop => Type == BlockType.Repeat;

        public Block(BlockType type)
        {
            Type = type;
        }

        // This block plus everything inside it
        public int TotalBlocks()
        {
            int total = 1;
            foreach (Block child in Children)
                total += child.TotalBlocks();
            return total;
        }

        // 0 for a normal block, 1 for a loop with no loops inside, 2 for a loop containing a loop, ...
        public int LoopDepth()
        {
            if (!IsLoop) return 0;

            int deepest = 0;
            foreach (Block child in Children)
                deepest = Math.Max(deepest, child.LoopDepth());
            return 1 + deepest;
        }

        // False for a loop that has nothing runnable inside it (empty, or only empty loops)
        public bool HasCommand()
        {
            if (!IsLoop) return true;

            foreach (Block child in Children)
            {
                if (child.HasCommand()) return true;
            }
            return false;
        }
    }
}