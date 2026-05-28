using System;
using System.Collections.Generic;
using UnityEngine;

namespace _.Scripts.Services
{
    public class UserInterfaceService
    {
        private Dictionary<string, AbstractUserInterface> _userInterfaces;

        public UserInterfaceService()
        {
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
        }

        public void RemoveUserInterface(string identity)
        {
            if (string.IsNullOrEmpty(identity))
                throw new  ArgumentException("Identity cannot be null or empty.");
            
            if (!_userInterfaces.Remove(identity))
                throw new Exception($"An error occured while removing user interface {identity}");
        }

        public void Show(string identity)
        {
            if (HasUserInterface(identity))
            {
                var userInterface = _userInterfaces[identity];
                
                MoveOnTop(identity);
                
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

        private void MoveOnTop(string identity)
        {
            if (HasUserInterface(identity))
            {
                var userInterface = _userInterfaces[identity];
                
                userInterface.transform.SetAsFirstSibling();
            } else
                throw new Exception($"User interface with identity {identity} is not registered.");
        }

#endregion
    }

    public abstract class AbstractUserInterface : MonoBehaviour
    {
        public abstract void Show();
        public abstract void Hide();
    }
}