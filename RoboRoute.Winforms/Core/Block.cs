using System.Drawing;

namespace RoboRoute.Winforms.Core;

// One block in the player's program.
//
// Block is ABSTRACT: you can't write "new Block()". It only says what every block must be able to do,
// and each kind of block (MoveForwardBlock, TurnLeftBlock, TurnRightBlock, RepeatBlock) supplies its own
// version. The rest of the program works with plain "Block" objects and calls these methods without
// knowing which kind it has: that is polymorphism.
//
//   - ProgramRunner calls block.AppendSteps(...) and block.Perform(...): no switch on the block's type.
//   - The editor draws block.DisplayName in block.FillColor: no lookup table.
//   - To add a new block, write one new subclass and add it to Create() below.
public abstract class Block
{
    // ---- What every block must say about itself (each subclass fills these in) ----

    // The kind of block (used by the level's limits and the palette)
    public abstract BlockType Type { get; }

    // The text shown on the block
    public abstract string DisplayName { get; }

    // The colour of the block
    public abstract Color FillColor { get; }

    // ---- What every block must be able to do ----

    // Does this block's action on the robot. Returns false if the action was blocked (the robot ran into a wall).
    public abstract bool Perform(Robot robot, Func<Point, bool> isWalkable);

    // Adds the commands this block turns into onto the end of "steps".
    // A normal block adds itself; a Repeat block adds its contents several times over.
    // Returns false if the program would grow longer than maxSteps.
    public abstract bool AppendSteps(List<Block> steps, int maxSteps);

    // ---- Things most blocks share, which Repeat changes ----

    // Only Repeat blocks are loops
    public virtual bool IsLoop => false;

    // The blocks inside this one. Most blocks have none.
    public virtual IReadOnlyList<Block> Children => Array.Empty<Block>();

    // False for a loop that has nothing runnable inside it
    public virtual bool HasCommand() => true;

    // 0 for a normal block, 1 for a loop with no loops inside, 2 for a loop containing a loop, ...
    public virtual int LoopDepth() => 0;

    // This block plus everything inside it
    public int TotalBlocks()
    {
        int total = 1;
        foreach (Block child in Children)
            total += child.TotalBlocks();
        return total;
    }

    // ---- Making blocks ----

    // The one place that turns a BlockType into the right subclass (the palette uses it to make a new block)
    public static Block Create(BlockType type)
    {
        switch (type)
        {
            case BlockType.MoveForward: return new MoveForwardBlock();
            case BlockType.TurnLeft:    return new TurnLeftBlock();
            case BlockType.TurnRight:   return new TurnRightBlock();
            case BlockType.Repeat:      return new RepeatBlock();
            default:                    throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown block type.");
        }
    }
}
