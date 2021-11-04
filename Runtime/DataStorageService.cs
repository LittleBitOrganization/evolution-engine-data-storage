using System;
using System.Collections;
using System.Collections.Generic;
using LittleBit.Modules.CoreModule;

namespace LittleBit.Modules.StorageModule
{
    public class DataStorageService : IDataStorageService
    {
        private readonly Dictionary<string, Data> _storage;
        private readonly ISaveService _saveService;
        private readonly Dictionary<Type, Dictionary<string, ArrayList>> _typedListeners;


        private IDataInfo _infoDataStorageService;
        
        public DataStorageService(ISaveService saveService, IDataInfo infoDataStorageService)
        {
            _storage = new Dictionary<string, Data>();
            _saveService = saveService;
            _infoDataStorageService = infoDataStorageService;
            _infoDataStorageService.Clear();
            _typedListeners = new Dictionary<Type, Dictionary<string, ArrayList>>();
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
            
            var type = typeof(T);

            if (_typedListeners.ContainsKey(type)) //TODO: refactor scopes
            {
                if (_typedListeners[type].ContainsKey(key))
                {
                    foreach (var obj in _typedListeners[type][key])
                    {
                        (obj as IDataStorageService.GenericCallback<T>)(data);
                    }
                }
            }
            
            _saveService.SaveData(key, data);
            _infoDataStorageService.UpdateData(key, data);
        }
        
        public void AddUpdateDataListener<T>(string key, IDataStorageService.GenericCallback<T> onUpdateData)
        {
            var type = typeof(T);

            if (!_typedListeners.ContainsKey(type))
            {
                _typedListeners[type] = new Dictionary<string, ArrayList>();
            }

            if (!_typedListeners[type].ContainsKey(key))
                _typedListeners[type][key] = new ArrayList();

            _typedListeners[type][key].Add(onUpdateData);
        }

        public void RemoveUpdateDataListener<T>(string key, IDataStorageService.GenericCallback<T> onUpdateData)
        {
            var type = typeof(T);
            if (_typedListeners.ContainsKey(type))
            {
                _typedListeners[type][key].Remove(onUpdateData);
            }
        }
    }
}
