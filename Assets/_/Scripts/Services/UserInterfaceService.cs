using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace _.Scripts.Services
{
    public class UserInterfaceService
    {
        private IObjectResolver _objectResolver;
        private Transform _root;
        
        private Dictionary<string, AbstractUserInterface> _userInterfaces;

        public Transform Root => _root;

        [Inject]
        public UserInterfaceService(IObjectResolver objectResolver)
        {
            _objectResolver = objectResolver;
            _root           = new GameObject("UI ROOT").transform;
            _userInterfaces = new Dictionary<string, AbstractUserInterface>();
        }

#region UI Managing

        public void RegisterUserInterface(string identity, AbstractUserInterface abstractUserInterface)
        {
            if (string.IsNullOrEmpty(identity))
                throw new ArgumentException($"Identity of user interface {identity} cannot be null or empty.");
            
            if (abstractUserInterface == null)
                throw new ArgumentException("UserInterface cannot be null.");
            
            if (_userInterfaces.ContainsKey(identity))
                throw new Exception($"User interface {identity} is already registered.");
            
            if (!_userInterfaces.TryAdd(identity, abstractUserInterface))
                throw new Exception($"An error occured while trying to add user interface with identity {identity}.");
            
            _objectResolver.Inject(abstractUserInterface);
            Hide(identity);
        }

        public void RemoveUserInterface(string identity, out AbstractUserInterface userInterface)
        {
            if (string.IsNullOrEmpty(identity))
                throw new  ArgumentException("Identity cannot be null or empty.");
            
            if (!_userInterfaces.Remove(identity, out userInterface))
                throw new Exception($"An error occured while removing user interface {identity}");
        }

        public void Show(string identity)
        {
            if (HasUserInterface(identity))
            {
                var userInterface = _userInterfaces[identity];
                
                userInterface.Show();
            } else
                throw new Exception($"User interface with identity {identity} is not registered.");
        }

        public void Hide(string identity)
        {
            if (HasUserInterface(identity))
            {
                var userInterface = _userInterfaces[identity];
                
                userInterface.Hide();
            } else
                throw new Exception($"User interface with identity {identity} is not registered.");
        }

        private bool HasUserInterface(string identity)
        {
            return _userInterfaces.ContainsKey(identity);
        }

        public AbstractUserInterface GetUserInterface(string identity)
        {
            if (string.IsNullOrEmpty(identity))
                throw new ArgumentException("Identity cannot be null or empty.");
            
            if (!_userInterfaces.TryGetValue(identity, out AbstractUserInterface userInterface))
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

#endregion
    }

    public abstract class AbstractUserInterface : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;

        public Canvas Canvas => _canvas;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_canvas == null)
            {
                if (TryGetComponent(out _canvas) && _canvas == null)
                    throw new Exception($"Can not find canvas component in {gameObject.name}");
            }
        }
#endif

        public abstract void Show();
        public abstract void Hide();
    }
}