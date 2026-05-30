using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using _.Scripts.Gameplay.Scriiptable_Objects;
using Unity.Mathematics;
using UnityEngine;

namespace _.Scripts.Utility
{
    [Serializable]
    public struct IdentityPair<T>
    {
        public string Identity;
        public T      Value;
    }

    [Serializable]
    public class CacheIdentityPairArray<T>
    {
        [SerializeField] private IdentityPair<T>[] _pairs;

#region Runtime

        private int _pairsHashCode; 
        private Dictionary<string, T> _cache;
        
        public void Cache()
        {
            _pairsHashCode = _pairs.GetHashCode();
            
            _cache = _pairs.ToDictionary(
                k => k.Identity,
                v => v.Value
            );
        }

        private void EnsureCacheValid()
        {
            if (_cache == null || _pairsHashCode != _pairs.GetHashCode())
                Cache();
        }

#endregion

        public IReadOnlyDictionary<string, T> Values
        {
            get
            {
                EnsureCacheValid();
                return _cache;
            }
        }
    }
    
    [Serializable]
    public class GridStructure<T>
    where T : class, new()
    {

#region Fields

        private int _width;
        private int _height;

        private Dictionary<int2, Node<T>> _nodes;

#endregion

#region Properties

        public IReadOnlyDictionary<int2, Node<T>> Nodes => _nodes;
        
        private int2[] NeighborsIdentitiesDeltas => new[]
        {
            new int2(-1, 0),
            new int2(-1, 1),
            new int2(0,  1),
            new int2(1,  1),
            new int2(1,  0),
            new int2(1,  -1),
            new int2(0,  -1),
            new int2(-1, -1),
        };

#endregion

        public GridStructure(int width, int height, Func<T> defaultNodeValue = null)
        {
            _width = width;
            _height = height;
            
            _nodes = new Dictionary<int2, Node<T>>();

            BuildGrid(defaultNodeValue);
        }

#region Private Methods

        private void BuildGrid(Func<T> defaultNodeValue)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    int2              identity            = new int2(x, y);
                    IEnumerable<int2> neighborsIdentities = GetNeighborsIdentitiesFor(identity);
                    T                 value               = defaultNodeValue?.Invoke();
                    
                    var node = new Node<T>(
                        identity,
                        neighborsIdentities,
                        value
                    );
                    
                    if (!_nodes.TryAdd(identity, node))
                        throw new Exception($"An error occured while building grid at node with identity {identity}");
                }
            }
        }

        private IEnumerable<int2> GetNeighborsIdentitiesFor(int2 identity)
        {
            foreach (var delta in NeighborsIdentitiesDeltas)
            {
                var neighborIdentity = identity + delta;
                
                if (IsIdentityInGridBounds(neighborIdentity))
                    yield return neighborIdentity;
            }
        }

        private bool IsIdentityInGridBounds(int2 identity)
        {
            return identity.x >= 0 && identity.x < _width &&
                   identity.y >= 0 && identity.y < _height;
        }

#endregion

#region Public API

        public Node<T> GetNode(int2 identity)
        {
            if (!IsIdentityInGridBounds(identity))
                throw new ArgumentOutOfRangeException($"Identity {identity} of node is out of grid bounds!");
            
            if (!_nodes.TryGetValue(identity, out var node))
                throw new Exception($"An error occured while trying to get node with identity {identity}");

            return node;
        }
        
        public IEnumerable<Node<T>> GetNeighborsFor(int2 identity)
        {
            var rootNode = GetNode(identity);

            foreach (var neighborIdentity in rootNode.NeighborsIdentities)
            {
                if (!_nodes.TryGetValue(neighborIdentity, out var neighborNode))
                    throw new Exception($"An error occured while trying to get node with identity {neighborIdentity}");
                
                yield return neighborNode;
            }
        }

#endregion

    }
    
    [Serializable]
    public class Node<T>
    where T : class, new()
    {
#region Fields

        private int2   _identity;
        private T      _value;
        private int2[] _neighborsIdentities;

#endregion

#region Properties

        public int2   Identity            => _identity;
        public T      Value               => _value;
        public int2[] NeighborsIdentities => _neighborsIdentities;

#endregion

        public Node(int2 identity, IEnumerable<int2> neighborsIdentities, T value = null)
        {
            _identity            = identity;
            _value               = value;
            _neighborsIdentities = neighborsIdentities.ToArray();
        }
    }
    
    [Serializable]
    public class Timer
    {
#region Fields

        private float _finalValue;
        private float _currentValue;
        private bool  _isPaused;

#endregion

#region Events

        private event Action _onTimerCompleted;
        private event Action _onTimerChanged;
        private event Action _onTimerStopped;
        
        public event Action OnTimerCompleted
        {
            add => _onTimerCompleted += value;
            remove => _onTimerCompleted -= value;
        }

        public event Action OnTimerChanged
        {
            add => _onTimerChanged += value;
            remove => _onTimerChanged -= value;
        }
        
        public event Action OnTimerStopped
        {
            add => _onTimerStopped += value;
            remove => _onTimerStopped -= value;
        }

#endregion

#region Properties

        public float CurrentValue
        {
            get => _currentValue;
            set
            {
                var nextValue = math.clamp(value, 0f, _finalValue);
                
                _currentValue = nextValue;
            }
        }

#endregion

#region Constructors

        public Timer(float finalValue)
        {
            _finalValue   = finalValue;
            _currentValue = 0f;
        }

        public Timer(float currentValue, float finalValue)
        {
            _finalValue   = finalValue;
            _currentValue = currentValue;
        }

#endregion

#region Public API

        public async Task Run(CancellationToken cancellationToken = default)
        {
            _isPaused = false;
            
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Yield();
                
                if (_isPaused)
                    continue;

                _currentValue += Time.deltaTime;
                
                _onTimerChanged?.Invoke();

                if (_currentValue >= _finalValue)
                    break;
            }

            if (cancellationToken.IsCancellationRequested)
                _onTimerStopped?.Invoke();
            else
                _onTimerCompleted?.Invoke();
        }

        public void Pause(bool value = true)
        {
            _isPaused = value;
        }

#endregion
    }
}