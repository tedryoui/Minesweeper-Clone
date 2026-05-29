using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace _.Scripts.Structures
{
    public class GridStructure<T>
    where T : class, new()
    {
        public class Node
        {
#region Fields

            private int2 _identity;
            private T    _value;
            
            private List<int2> _neighborsIdentities;

#endregion

#region Properties

            public int2 Identity => _identity;
            public T    Value    => _value;

            public List<int2> NeighborsIdentities => _neighborsIdentities;

#endregion

            public Node(int2 identity, IEnumerable<int2> neighborsIdentities, T value = null)
            {
                _identity            = identity;
                _value               = value;
                _neighborsIdentities = neighborsIdentities.ToList();
            }
        }

#region Fields

        private int _width;
        private int _height;

        private Dictionary<int2, Node> _nodes;

#endregion

#region Properties

        public IReadOnlyDictionary<int2, Node> Nodes => _nodes;
        
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
            
            _nodes = new Dictionary<int2, Node>();

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
                    
                    var node = new Node(
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
                
                if (neighborIdentity.x >= 0 && neighborIdentity.x < _width &&
                    neighborIdentity.y >= 0 && neighborIdentity.y < _height)
                    yield return neighborIdentity;
            }
        }

#endregion

#region Public Methods

        public Node GetNode(int2 identity)
        {
            if (identity.x < 0 || identity.x >= _width ||
                identity.y < 0 || identity.y >= _height)
                throw new ArgumentOutOfRangeException($"Identity {identity} of node is out of grid bounds!");
            
            if (!_nodes.TryGetValue(identity, out var node))
                throw new Exception($"An error occured while trying to get node with identity {identity}");

            return node;
        }
        
        public IEnumerable<Node> GetNeighborsFor(int2 identity)
        {
            var rootNode = GetNode(identity);

            foreach (var neighborIdentity in rootNode.NeighborsIdentities)
            {
                if (_nodes.TryGetValue(neighborIdentity, out var neighborNode))
                    yield return neighborNode;
#if UNITY_EDITOR ||  DEVELOPMENT_BUILD
                else 
                    Debug.LogError($"An error occured while trying to get node with identity {neighborIdentity}");
#endif
            }
        }

#endregion

    }
}