using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using _.Scripts.Gameplay.Scriiptable_Objects;
using _.Scripts.Models;
using _.Scripts.Services;
using _.Scripts.Structures;
using _.Scripts.Utility;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using VContainer;
using Timer = _.Scripts.Structures.Timer;

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
            _closedCellIdentities.Remove(identity);
            _flaggedCellIdentities.Remove(identity);
        }

        public void FlagCell(int2 identity, bool value)
        {
            if (value)
            {
                _closedCellIdentities.Remove(identity);
                _flaggedCellIdentities.Add(identity);
            }
            else
            {
                _closedCellIdentities.Add(identity);
                _flaggedCellIdentities.Remove(identity);
            }
            
        } 

        public void Exit()
        {
            _exitCancellationTokenSource.Cancel();
        }
    }
}