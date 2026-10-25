using System.Drawing;

namespace RoboRoute.Winforms.Core;

// A loop: runs the blocks inside it Count times.
// Because the blocks inside can be Repeat blocks too, a program is a tree, not a flat list.
public class RepeatBlock : Block
{
    public const int MinCount = 1;
    public const int MaxCount = 99;

    // How many times the blocks inside run (the player types this into the block)
    public int Count { get; set; } = 3;

    // The blocks inside the loop, in order. The editor adds and removes blocks here.
    public List<Block> Body { get; } = new List<Block>();

    public override BlockType Type => BlockType.Repeat;
    public override string DisplayName => "Repeat";
    public override Color FillColor => Color.FromArgb(130, 70, 160);

    public override bool IsLoop => true;
    public override IReadOnlyList<Block> Children => Body;

    public override bool HasCommand()
    {
        foreach (Block child in Body)
        {
            if (child.HasCommand()) return true;
        }
        return false;
    }

    public override int LoopDepth()
    {
        int deepest = 0;
        foreach (Block child in Body)
            deepest = Math.Max(deepest, child.LoopDepth());
        return 1 + deepest;
    }

    // A loop is never run as one step: AppendSteps replaces it with its contents, so Perform is never called on it
    public override bool Perform(Robot robot, Func<Point, bool> isWalkable)
    {
        throw new InvalidOperationException("A Repeat block is expanded into its contents and is never performed directly.");
    }

    public override bool AppendSteps(List<Block> steps, int maxSteps)
    {
        // Skipping loops with nothing inside also keeps empty nested loops from burning time
        if (!HasCommand()) return true;

        for (int i = 0; i < Count; i++)
        {
            foreach (Block child in Body)
            {
                if (!child.AppendSteps(steps, maxSteps)) return false;
            }
        }
        return true;
    }
}
