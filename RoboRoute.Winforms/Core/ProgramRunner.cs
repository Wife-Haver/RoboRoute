using System;
using System.Collections.Generic;
using System.Drawing;
using static RoboRoute.Winforms.Core.Robot;

namespace RoboRoute.Winforms.Core
{
    // Holds the player's program (a tree of blocks) and executes it on the robot.
    // It knows nothing about the mouse or the screen: the form calls Update() on every timer tick.
    public class ProgramRunner
    {
        // Safety limit: nested loops multiply (99 x 99 x 99...), so refuse absurdly long runs
        private const int MaxSteps = 5000;

        private readonly Robot _robot;
        private readonly Func<Point, bool> _isWalkable;
        private readonly Func<Point, bool> _isGoal;
        private readonly Func<Point, bool> _isHazard;
        private readonly Point _startTile;
        private readonly Direction _startFacing;

        // The program unrolled into a flat list of commands, built when Run is pressed.
        // A Repeat block with Count 3 and two children becomes those two children, three times over.
        private readonly List<Block> _steps = new List<Block>();
        private int _nextStep;          // index of the next step to execute

        // The player's program, top to bottom (Repeat blocks hold their own children)
        public List<Block> Queue { get; } = new List<Block>();

        public bool IsRunning { get; private set; }
        public Block ActiveBlock { get; private set; }   // the command currently executing, null = none
        public string Status { get; private set; } = "";
        public Color StatusColor { get; private set; } = Color.White;

        // True when the last run ended with the robot standing on the goal tile.
        // Use this to unlock the next level, show a win screen, etc.
        public bool ReachedGoal { get; private set; }

        public ProgramRunner(
            Robot robot,
            Func<Point, bool> isWalkable,
            Func<Point, bool> isGoal,
            Func<Point, bool> isHazard,
            Point startTile,
            Direction startFacing)
        {
            _robot = robot;
            _isWalkable = isWalkable;
            _isGoal = isGoal;
            _isHazard = isHazard;
            _startTile = startTile;
            _startFacing = startFacing;
        }

        public void Start()
        {
            if (IsRunning || Queue.Count == 0) return;

            _steps.Clear();
            if (!Unroll(Queue))
            {
                Status = "Too long! (over " + MaxSteps + " steps)";
                StatusColor = Color.FromArgb(255, 120, 120);
                return;
            }

            if (_steps.Count == 0)
            {
                Status = "Nothing to run";
                StatusColor = Color.LightGray;
                return;
            }

            _robot.Reset(_startTile, _startFacing);   // always start from the beginning
            IsRunning = true;
            ReachedGoal = false;
            _nextStep = 0;
            ActiveBlock = null;
            Status = "Running...";
            StatusColor = Color.White;
        }

        public void Stop()
        {
            if (!IsRunning) return;

            _robot.Reset(_startTile, _startFacing);
            IsRunning = false;
            ReachedGoal = false;
            ActiveBlock = null;
            Status = "Stopped";
            StatusColor = Color.LightGray;
        }

        // Called when the player starts editing again: the old result no longer applies
        public void ClearStatus()
        {
            Status = "";
            ReachedGoal = false;
        }

        // Runs one command at a time: the next one starts only when the robot has finished the last one
        public void Update()
        {
            if (!IsRunning || _robot.IsBusy) return;

            // The robot has finished its last move: is it standing on a hazard?
            if (_isHazard(_robot.TilePos))
            {
                Finish("Hit a hazard at step " + _nextStep, Color.FromArgb(255, 120, 120));
                return;
            }

            if (_nextStep >= _steps.Count)
            {
                // The program is over and the robot has stopped moving: did it end on the goal?
                if (_isGoal(_robot.TilePos))
                {
                    ReachedGoal = true;
                    Finish("Level complete!", Color.FromArgb(120, 220, 140));
                }
                else
                {
                    Finish("Didn't reach the goal", Color.FromArgb(255, 200, 100));
                }
                return;
            }

            ActiveBlock = _steps[_nextStep];
            _nextStep++;

            // Each block knows how to do its own action: no switch on the block's type needed
            if (!ActiveBlock.Perform(_robot, _isWalkable))
                Finish("Blocked by a wall at step " + _nextStep, Color.FromArgb(255, 120, 120));
        }

        // Flattens the program into _steps. Each block adds its own commands (a Repeat block adds its contents
        // several times over). Returns false if the program would be longer than MaxSteps.
        private bool Unroll(List<Block> blocks)
        {
            foreach (Block block in blocks)
            {
                if (!block.AppendSteps(_steps, MaxSteps)) return false;
            }
            return true;
        }

        private void Finish(string message, Color color)
        {
            IsRunning = false;
            ActiveBlock = null;
            Status = message;
            StatusColor = color;
        }
    }
}