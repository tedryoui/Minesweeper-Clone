using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace _.Scripts.Services
{
    public class DataService
    {
#region Structures

        public interface IModel { }

#endregion
        
        private Dictionary<string, IModel> _dataModels;

        public DataService()
        {
            _dataModels = new Dictionary<string, IModel>();
        }

#region Models Managing

        public void AddModel(string identity, IModel model)
        {
            if (string.IsNullOrEmpty(identity))
                throw new  ArgumentException("Identity cannot be null or empty.");
            
            if (model == null)
                throw new  ArgumentException("Model cannot be null.");

            if (HasModel(identity))
                throw new Exception("Model is already exists.");

            if (!_dataModels.TryAdd(identity, model))
                throw new Exception($"An error occured while adding model {identity}");
        }

        public void RemoveModel(string identity, out IModel iModel)
        {
            if (string.IsNullOrEmpty(identity))
                throw new  ArgumentException("Identity cannot be null or empty.");
            
            if (!_dataModels.Remove(identity, out iModel))
                throw new Exception($"An error occured while removing model {identity}");
        }

        public bool HasModel(string identity)
        {
            return _dataModels.ContainsKey(identity);
        }

        public IModel GetModel(string identity)
        {
            if (_dataModels.TryGetValue(identity, out var model))
                return model;

            throw new KeyNotFoundException($"Data model {identity} not found!");
        }

        public T GetModel<T>(string identity)
            where T : IModel
        {
            var iModel = GetModel(identity);

            if (iModel is T tModel)
                return tModel;

            throw new KeyNotFoundException($"Data model {identity} and type {typeof(T)} not found!");
        }

#endregion
    }
}