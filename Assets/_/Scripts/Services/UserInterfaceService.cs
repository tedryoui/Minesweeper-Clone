using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using VContainer;

namespace _.Scripts.Services
{
    public class UserInterfaceService
    {
#region VContainer

        private IObjectResolver _objectResolver;
        
        [Inject]
        public UserInterfaceService(IObjectResolver objectResolver)
        {
            _objectResolver = objectResolver;
            _root           = new GameObject("UI ROOT").transform;
            _values         = new Dictionary<string, AbstractUserInterface>();
        }

#endregion

#region Fields

        private Dictionary<string, AbstractUserInterface> _values;

#endregion

#region Events

        private event Action<string> _onUserInterfaceRemoved;
        
        public event Action<string> OnUserInterfaceRemoved
        {
            add => _onUserInterfaceRemoved += value;
            remove => _onUserInterfaceRemoved -= value;
        }

#endregion
        
#region Scene References

        private Transform _root;

#endregion

#region Properties

        public Transform Root => _root;

#endregion

#region Public API

        public void RegisterUserInterface(string identity, AbstractUserInterface abstractUserInterface)
        {
            if (string.IsNullOrEmpty(identity))
                throw new ArgumentException($"Identity of user interface {identity} cannot be null or empty.");
            
            if (abstractUserInterface == null)
                throw new ArgumentException("UserInterface cannot be null.");
            
            if (_values.ContainsKey(identity))
                throw new Exception($"User interface {identity} is already registered.");
            
            if (!_values.TryAdd(identity, abstractUserInterface))
                throw new Exception($"An error occured while trying to add user interface with identity {identity}.");
            
            _objectResolver.Inject(abstractUserInterface);
            Hide(identity, false);
            DOTween.Complete(abstractUserInterface.gameObject);
        }

        public void RemoveUserInterface(string identity, out AbstractUserInterface userInterface)
        {
            if (string.IsNullOrEmpty(identity))
                throw new  ArgumentException("Identity cannot be null or empty.");
            
            if (!_values.Remove(identity, out userInterface))
                throw new Exception($"An error occured while removing user interface {identity}");
            
            _onUserInterfaceRemoved?.Invoke(identity);
        }

        public bool HasUserInterface(string identity)
        {
            return _values.ContainsKey(identity);
        }

        public AbstractUserInterface GetUserInterface(string identity)
        {
            if (string.IsNullOrEmpty(identity))
                throw new ArgumentException("Identity cannot be null or empty.");
            
            if (!_values.TryGetValue(identity, out AbstractUserInterface userInterface))
                throw new Exception($"User interface with identity {identity} is not registered.");
            
            return userInterface;
        }

        public T GetUserInterface<T>(string identity)
        where T : AbstractUserInterface
        {
            var userInterface = GetUserInterface(identity);
            
            if (userInterface is T tUserInterface)
                return tUserInterface;
            
            throw new Exception($"User interface with identity {identity} is not registered.");
        }

#region User Interface Managing

        public void Show(string identity, bool animate = true)
        {
            if (HasUserInterface(identity))
            {
                var userInterface = _values[identity];
                
                userInterface.Show(animate);
            } else
                throw new Exception($"User interface with identity {identity} is not registered.");
        }

        public void Hide(string identity, bool animate = true)
        {
            if (HasUserInterface(identity))
            {
                var userInterface = _values[identity];
                
                userInterface.Hide(animate);
            } else
                throw new Exception($"User interface with identity {identity} is not registered.");
        }

#endregion

#endregion
    }
}