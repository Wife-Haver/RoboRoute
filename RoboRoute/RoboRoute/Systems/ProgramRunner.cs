using Microsoft.Xna.Framework;
using RoboRoute.RoboRoute.Commands;
using RoboRoute.RoboRoute.Core;
using System;
using System.Collections.Generic;

namespace RoboRoute
{
    // Holds the player's program (the block queue) and executes it on the robot.
    // It knows nothing about the mouse or the screen, so it's easy to extend
    // later (loops, goal checks, pauses between blocks...).
    public class ProgramRunner
    {
        private readonly Robot _robot;
        private readonly Func<Point, bool> _isWalkable;
        private readonly Point _startTile;

        private int _nextStep;          // index of the next block to execute

        // The player's program, top to bottom
        public List<BlockType> Queue { get; } = new List<BlockType>();

        public bool IsRunning { get; private set; }
        public int ActiveStep { get; private set; } = -1;   // block currently executing, -1 = none
        public string Status { get; private set; } = "";
        public Color StatusColor { get; private set; } = Color.White;

        public ProgramRunner(Robot robot, Func<Point, bool> isWalkable, Point startTile)
        {
            _robot = robot;
            _isWalkable = isWalkable;
            _startTile = startTile;
        }

        public void Start()
        {
            if (IsRunning || Queue.Count == 0) return;

            _robot.Reset(_startTile);   // always start from the beginning
            IsRunning = true;
            _nextStep = 0;
            ActiveStep = -1;
            Status = "Running...";
            StatusColor = Color.White;
        }

        public void Stop()
        {
            if (!IsRunning) return;

            _robot.Reset(_startTile);
            IsRunning = false;
            ActiveStep = -1;
            Status = "Stopped";
            StatusColor = Color.LightGray;
        }

        // Called when the player starts editing again: the old result no longer applies
        public void ClearStatus()
        {
            Status = "";
        }

        // Runs one block at a time: the next block starts only when the robot has finished the last one
        public void Update()
        {
            if (!IsRunning || _robot.IsBusy) return;

            if (_nextStep >= Queue.Count)
            {
                Finish("Program finished!", new Color(120, 220, 140));
                return;
            }

            ActiveStep = _nextStep;
            BlockType block = Queue[_nextStep];
            _nextStep++;

            switch (block)
            {
                case BlockType.MoveForward:
                    if (!_robot.MoveForward(_isWalkable))
                        Finish("Blocked by a wall at step " + (ActiveStep + 1), new Color(255, 120, 120));
                    break;

                case BlockType.TurnLeft:
                    _robot.TurnLeft();
                    break;

                case BlockType.TurnRight:
                    _robot.TurnRight();
                    break;
            }
        }

        private void Finish(string message, Color color)
        {
            IsRunning = false;
            ActiveStep = -1;
            Status = message;
            StatusColor = color;
        }
    }
}