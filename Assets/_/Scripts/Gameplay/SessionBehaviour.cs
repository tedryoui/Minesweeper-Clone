using System.Threading;
using System.Threading.Tasks;
using _.Scripts.Gameplay.Scriiptable_Objects;
using _.Scripts.Models;
using _.Scripts.Services;
using _.Scripts.Structures;
using _.Scripts.Utility;
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
            public enum CellType
            {
                Empty, 
                Mine
            }
            
            public       CellType cellType;
            public       bool     isOpened;
            public       int      minesAround;

            public static Cell Default => new Cell
            {
                cellType    = CellType.Empty,
                isOpened    = false,
                minesAround = 0
            };
        }

        private GridStructure<Cell> _grid;
        private Timer               _timer;
        
        private CancellationTokenSource _exitCancellationTokenSource;
        
        public CancellationToken ExitCancellationToken => _exitCancellationTokenSource.Token;

        public SessionBehaviour(GridStructure<Cell> grid, Timer timer)
        {
            _exitCancellationTokenSource = CancellationTokenSource
                .CreateLinkedTokenSource(
                    ExitCancellationToken,
                    Application.exitCancellationToken
                );
            
            _grid  = grid;
            _timer = timer;
            
            _timer.Run(ExitCancellationToken);
        }
    }
}