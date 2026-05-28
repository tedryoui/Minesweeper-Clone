using System.Collections.Generic;
using _.Scripts.Gameplay.Scriiptable_Objects;
using _.Scripts.Services;
using _.Scripts.Structures;
using _.Scripts.Utility;
using Unity.Mathematics;
using VContainer;

namespace _.Scripts.Gameplay
{
    public static class SessionFactory
    {
        private static IObjectResolver _objectResolver;
        private static GameplayPreset  _gameplayPreset;
        
        public static SessionBehaviour Create(
            IObjectResolver objectResolver, 
            GameplayPreset gameplayPreset)
        {
            _objectResolver = objectResolver;
            _gameplayPreset = gameplayPreset;
            
            SessionBehaviour sessionBehaviour;

            var dataService = objectResolver.Resolve<DataService>();

            if (!dataService.HasModel(MODEL_IDENTITIES.SESSION_MODEL))
                sessionBehaviour = CreateSession();
            else
                sessionBehaviour = LoadSession();
            
            return sessionBehaviour;
        }

        private static SessionBehaviour CreateSession()
        {
            var grid               = new GridStructure<SessionBehaviour.Cell>(
                _gameplayPreset.Width, 
                _gameplayPreset.Height, 
                () => SessionBehaviour.Cell.Default
                );
            var mineCellIdentities = CreateMineCellIdentities();

            foreach (var identity in mineCellIdentities)
            {
                var cell = grid.GetNode(identity).Value;
                
                cell.cellType = SessionBehaviour.Cell.CellType.Mine;
            }

            for (int x = 0; x < _gameplayPreset.Width; x++)
            {
                for (int y = 0; y < _gameplayPreset.Height; y++)
                {
                    var identity        = new int2(x, y);
                    var node            = grid.GetNode(identity);
                    
                    if (node.Value.cellType is SessionBehaviour.Cell.CellType.Mine)
                        continue;
                    
                    var neighboursNodes = grid.GetNeighborsFor(identity);
                    var mineCount       = 0;
                    
                    foreach (var neighbourNode in neighboursNodes)
                    {
                        if (neighbourNode.Value.cellType is SessionBehaviour.Cell.CellType.Mine)
                            mineCount++;
                    }
                    
                    node.Value.minesAround = mineCount;
                }
            }

            var timer = new Timer(_gameplayPreset.TimerSeconds);
            
            return new SessionBehaviour(grid, timer);
        }

        private static IEnumerable<int2> CreateMineCellIdentities()
        {
            var random = new Unity.Mathematics.Random();
            var mineCount = _gameplayPreset.MineCount;

            for (int m = 0; m < mineCount; m++)
            {
                var identity = random
                    .NextInt2(
                        new int2(0, 0),  
                        new int2(_gameplayPreset.Width, _gameplayPreset.Height)
                    );
                
                yield return identity;
            }
        }

        private static SessionBehaviour LoadSession()
        {
            return null;
        }
    }
}