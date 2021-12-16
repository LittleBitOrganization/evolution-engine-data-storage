using System;
using System.Collections;
using System.Collections.Generic;
using LittleBit.Modules.CoreModule;

namespace LittleBit.Modules.StorageModule
{
    public class DataStorageService : IDataStorageService, ISavable
    {
        private readonly Dictionary<string, Data> _storage;
        private readonly ISaveService _saveService;
        private readonly ISaverService _saverService;
        private readonly Dictionary<object, TypedDelegates> _listeners;


        private IDataInfo _infoDataStorageService;
        private Queue<PostRemoveCommand> _postRemoveAllUpdateDataListener;
        private Queue<PostRemoveCommand> _postRemoveUpdateDataListener;
        public DataStorageService(ISaveService saveService, ISaverService saverService, IDataInfo infoDataStorageService)
        {
            _storage = new Dictionary<string, Data>();
            _saveService = saveService;
            _saverService = saverService;
            _saverService.AddSavableObject(this);
            _infoDataStorageService = infoDataStorageService;
            _infoDataStorageService.Clear();
            _listeners = new Dictionary<object, TypedDelegates>();
            _postRemoveAllUpdateDataListener = new Queue<PostRemoveCommand>();
            _postRemoveUpdateDataListener = new Queue<PostRemoveCommand>();
        }

        public T GetData<T>(string key) where T : Data, new()
        {
            RemoveUnusedListeners();
            if(key == null)
                throw new Exception("Key is null");
            if(key.Length == 0)
                throw new Exception("Key is empty");
            
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
            RemoveUnusedListeners();
            if (!_storage.ContainsKey(key)) _storage.Add(key, data);
            else _storage[key] = data;

            var type = typeof(T);

            foreach (var obj in _listeners.Keys)
            {
                if (!_listeners[obj].ContainsKey(type)) continue;

                if (!_listeners[obj][type].ContainsKey(key)) continue;

                foreach (var listener in _listeners[obj][type][key])
                {
                    (listener as IDataStorageService.GenericCallback<T>)(data);
                }
            }

            RemoveUnusedListeners();
            
            _infoDataStorageService.UpdateData(key, data);
        }

        private void RemoveUnusedListeners()
        {
            while (_postRemoveUpdateDataListener.Count > 0)
            {
                PostRemoveCommand command = _postRemoveUpdateDataListener.Dequeue();
                command.List.Remove(command.OnUpdateData);
            }

            while (_postRemoveAllUpdateDataListener.Count > 0)
            {
                PostRemoveCommand command = _postRemoveAllUpdateDataListener.Dequeue();
                command.List.Clear();
            }
          
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
            
            _postRemoveUpdateDataListener.Enqueue(new PostRemoveCommand(_listeners[handler][type][key], onUpdateData));
        }
        
        public void RemoveAllUpdateDataListenersOnObject(object handler)
        {
            if(!_listeners.ContainsKey(handler)) return;

            foreach (var type in _listeners[handler].Keys)
            {
                foreach (var key in _listeners[handler][type].Keys)
                {
                    _postRemoveAllUpdateDataListener.Enqueue(new PostRemoveCommand(_listeners[handler][type][key], null));
                }
            }
        }
        
        public void Save()
        {
            foreach (var pairData in _storage)
            {
                _saveService.SaveData(pairData.Key, pairData.Value);
            }
        }

      
        
        public class PostRemoveCommand
        {
            private ArrayList _list;
            private object _onUpdateData;
            public PostRemoveCommand(ArrayList list, object onUpdateData)
            {
                _list = list;
                _onUpdateData = onUpdateData;
            }

            public ArrayList List => _list;

            public object OnUpdateData => _onUpdateData;
        }
    }

    public class TypedDelegates : Dictionary<Type, Dictionary<string, ArrayList>>
    {
        
    }
}