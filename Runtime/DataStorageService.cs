using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LittleBit.Modules.CoreModule;

namespace LittleBit.Modules.StorageModule
{
    public class DataStorageService : IDataStorageService
    {
        private readonly Dictionary<string, Data> _storage;
        private readonly ISaveService _saveService;
        private readonly Dictionary<object, TypedDelegates> _listeners;


        private IDataInfo _infoDataStorageService;

        public DataStorageService(ISaveService saveService, IDataInfo infoDataStorageService)
        {
            _storage = new Dictionary<string, Data>();
            _saveService = saveService;
            _infoDataStorageService = infoDataStorageService;
            _infoDataStorageService.Clear();
            _listeners = new Dictionary<object, TypedDelegates>();
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
            if (!_storage.ContainsKey(key)) _storage.Add(key, data);
            else _storage[key] = data;

            var type = typeof(T);

            foreach (var obj in _listeners.Keys.ToList())
            {
                if (!_listeners[obj].ContainsKey(type)) continue;

                if (!_listeners[obj][type].ContainsKey(key)) continue;

                foreach (var listener in _listeners[obj][type][key])
                {
                    (listener as IDataStorageService.GenericCallback<T>)(data);
                }
            }

            _saveService.SaveData(key, data);
            _infoDataStorageService.UpdateData(key, data);
        }

        public void AddUpdateDataListener<T>(object handler, string key,
            IDataStorageService.GenericCallback<T> onUpdateData)
        {
            var type = typeof(T);

            if (!_listeners.ContainsKey(handler))
                _listeners[handler] = new TypedDelegates();


            if (!_listeners[handler].ContainsKey(type))
                _listeners[handler][type] = new Dictionary<string, ArrayList>();


            if (!_listeners[handler][type].ContainsKey(key))
                _listeners[handler][type][key] = new ArrayList();

            _listeners[handler][type][key].Add(onUpdateData);
        }

        public void RemoveUpdateDataListener<T>(object handler, string key, IDataStorageService.GenericCallback<T> onUpdateData)
        {
            var type = typeof(T);

            if (!_listeners.ContainsKey(handler)) return;

            if (!_listeners[handler].ContainsKey(type)) return;

            if (!_listeners[handler][type].ContainsKey(key)) return;

            if(!_listeners[handler][type][key].Contains(onUpdateData)) return;
            
            _listeners[handler][type][key].Remove(onUpdateData);
        }

        public void RemoveAllUpdateDataListenersOnObject(object handler)
        {
            if(!_listeners.ContainsKey(handler)) return;

            foreach (var type in _listeners[handler].Keys)
            {
                foreach (var key in _listeners[handler][type].Keys)
                {
                    _listeners[handler][type][key].Clear();
                }
            }
        }
    }

    public class TypedDelegates : Dictionary<Type, Dictionary<string, ArrayList>>
    {
    }
}