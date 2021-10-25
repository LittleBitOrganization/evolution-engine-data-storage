using System;
using System.Collections.Generic;
using LittleBit.Modules.CoreModule;

namespace LittleBit.Modules.StorageModule
{
    public class DataStorageService : IDataStorageService
    {
        private readonly Dictionary<string, Data> _storage;
        private readonly Dictionary<string, List<Action<string>>> _listeners;
        private readonly ISaveService _saveService;

        private IDataInfo _infoDataStorageService;
        
        public DataStorageService(ISaveService saveService, IDataInfo infoDataStorageService)
        {
            _storage = new Dictionary<string, Data>();
            _listeners = new Dictionary<string, List<Action<string>>>();
            _saveService = saveService;
            _infoDataStorageService = infoDataStorageService;
            _infoDataStorageService.Clear();
        }

        public T GetData<T>(string key) where T : Data, new()
        {
            if (!_storage.ContainsKey(key))
            {
                T data = _saveService.LoadData<T>(key);
                if (data == null)
                {
                    data = new T();
                }

                _storage.Add(key, data);
            }

            _infoDataStorageService.UpdateData(key, _storage[key]);
            return (T) _storage[key];
        }

        public void SetData<T>(string key, T data) where T : Data
        {
            if (!_storage.ContainsKey(key))
            {
                _storage.Add(key, data);
            }
            else
            {
                _storage[key] = data;
            }

            if (_listeners.ContainsKey(key) && _storage.ContainsKey(key))
            {
                var listeners = _listeners[key];
                foreach (var listener in listeners)
                {
                    listener.Invoke(key);
                }
            }

            _saveService.SaveData(key, data);
            _infoDataStorageService.UpdateData(key, data);
        }
        
        public void AddUpdateDataListener(string key, Action<string> onUpdateData)
        {
            if (!_listeners.ContainsKey(key))
            {
                _listeners.Add(key, new List<Action<string>>());
            }

            _listeners[key].Add(onUpdateData);
        }

        public void RemoveUpdateDataListener(string key, Action<string> onUpdateData)
        {
            if (_listeners.ContainsKey(key))
            {
                _listeners[key].Remove(onUpdateData);
            }
        }
    }
}
