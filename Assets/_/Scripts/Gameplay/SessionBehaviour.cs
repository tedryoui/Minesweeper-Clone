using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using _.Scripts.Gameplay.Scriiptable_Objects;
using _.Scripts.Models;
using _.Scripts.Services;
using _.Scripts.Utility;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using VContainer;
using Timer = _.Scripts.Utility.Timer;

namespace _.Scripts.Gameplay
{
    public class SessionBehaviour
    {
        public class Cell
        {
            [Flags]
            public enum CellType
            {
                Opened = 1 << 0,
                Closed = 1 << 1,
                Empty = Opened | 1 << 2,
                Mine = Opened | 1 << 3,
                Flag = Closed | 1 << 4,
                Number = Opened | 1 << 5
            }
            
            public CellType initialFlags;
            public CellType overwrittenFlags;
            public int      minesAround;

            public static Cell Default => new Cell
            {
                initialFlags     = CellType.Empty,
                overwrittenFlags = CellType.Closed,
                minesAround      = 0
            };
        }

        private GridStructure<Cell>     _grid;
        private Timer                   _timer;
        private CancellationTokenSource _exitCancellationTokenSource;
        private HashSet<int2>           _closedCellIdentities;
        private HashSet<int2>           _flaggedCellIdentities;

        public GridStructure<Cell> Grid                  => _grid;
        public Timer               Timer                 => _timer;
        public CancellationToken   ExitCancellationToken => _exitCancellationTokenSource.Token;
        public HashSet<int2>       ClosedCellIdentities  => _closedCellIdentities;
        public HashSet<int2>       FlaggedCellIdentities => _flaggedCellIdentities;

        public event Action<int2> onCellChanged;
        public event Action       onMine;

        public SessionBehaviour(GridStructure<Cell> grid, Timer timer)
        {
            _exitCancellationTokenSource = CancellationTokenSource
                .CreateLinkedTokenSource(
                    Application.exitCancellationToken
                );
            
            _grid  = grid;
            _timer = timer;

            InitializeCellCollections();
            
            _timer.Run(ExitCancellationToken);
        }

        private void InitializeCellCollections()
        {
            _closedCellIdentities = new HashSet<int2>();
            _flaggedCellIdentities = new HashSet<int2>();

            foreach (var node in _grid.Nodes.Values)
            {
                var identity = node.Identity;
                var cell     = node.Value;
                
                if (cell.overwrittenFlags is Cell.CellType.Closed)
                    _closedCellIdentities.Add(identity);
                else if (cell.overwrittenFlags is Cell.CellType.Flag)
                    _flaggedCellIdentities.Add(identity);
            }
        }

        public void OpenCell(int2 identity)
        {
            var data             = _grid.GetNode(identity);
            var initialFlags     = data.Value.initialFlags;
            var overwrittenFlags = data.Value.overwrittenFlags;

            if (overwrittenFlags is not Cell.CellType.Closed or Cell.CellType.Flag)
                return;

            data.Value.overwrittenFlags = initialFlags;
            
            if (initialFlags is Cell.CellType.Mine)
                onMine?.Invoke();
            else if (initialFlags is Cell.CellType.Empty)
                TouchNeighbors(identity);
            
            IdentifyCell(identity);
            onCellChanged?.Invoke(identity);
        }

        public void FlagCell(int2 identity)
        {
            var data             = _grid.GetNode(identity);
            var overwrittenFlags = data.Value.overwrittenFlags;

            switch (overwrittenFlags)
            {
                case Cell.CellType.Closed:
                    data.Value.overwrittenFlags = Cell.CellType.Flag;
                    break;
                case Cell.CellType.Flag:
                    data.Value.overwrittenFlags = Cell.CellType.Closed;
                    break;
            }
            
            IdentifyCell(identity);
            onCellChanged?.Invoke(identity);
        }

        private void TouchNeighbors(int2 identity)
        {
            var node      =  _grid.GetNode(identity);
            var neighbors = node.NeighborsIdentities;

            foreach (var neighbor in neighbors)
            {
                var neighborNode = _grid.GetNode(neighbor);
                
                if (neighborNode.Value.overwrittenFlags is Cell.CellType.Closed &&
                    neighborNode.Value.initialFlags is Cell.CellType.Empty or Cell.CellType.Number)
                    OpenCell(neighbor);
            }
        }

        private void IdentifyCell(int2 identity)
        {
            var data            = _grid.GetNode(identity);
            var overwrittenType = data.Value.overwrittenFlags;

            switch (overwrittenType)
            {
                case 
                    Cell.CellType.Opened or 
                    Cell.CellType.Empty or 
                    Cell.CellType.Mine or 
                    Cell.CellType.Number:
                    _closedCellIdentities.Remove(identity);
                    _flaggedCellIdentities.Remove(identity);
                    break;
                case Cell.CellType.Closed:
                    _closedCellIdentities.Add(identity);
                    _flaggedCellIdentities.Remove(identity);
                    break;
                case Cell.CellType.Flag:
                    _flaggedCellIdentities.Add(identity);
                    _closedCellIdentities.Remove(identity);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Exit()
        {
            _exitCancellationTokenSource.Cancel();
        }
    }
}