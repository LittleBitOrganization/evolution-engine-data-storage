using System;
using System.Collections.Generic;
using LittleBit.Modules.CoreModule;
using NaughtyAttributes;
using UnityEngine;

namespace LittleBit.Modules.StorageModule
{
    [CreateAssetMenu(fileName = "DataInfoReadonly", menuName = "Data/DataInfoReadonly", order = 0)]
    public class InfoDataStorageService : ScriptableObject, IService
    {
        [SerializeField] private List<InfoData> _storage = new List<InfoData>();

        public void UpdateStorage(Dictionary<string, Data> storage)
        {
            foreach (var key in storage.Keys)
            {
                _storage.Add(new InfoData(key, storage[key]));
            }
        }

        public void UpdateData(string key, Data data)
        {
            InfoData infoData = _storage.Find(value => value.name == key);
            if (infoData == null)
            {
                _storage.Add(new InfoData(key, data));
            }
            else
            {
                infoData.UpdateData(data);
            }
        }
        
        
        [Serializable]
        public class InfoData
        {
            [HideInInspector] public string name;
            [SerializeField] [ResizableTextArea] private string _data;
            
            public InfoData (string name, Data data)
            {
                this.name = name;
                _data = ConvertToString(data);
            }

            public void UpdateData(Data data)
            {
                _data = ConvertToString(data);
            }

            private string ConvertToString(Data data)
            {
                return JsonUtility.ToJson(data).Replace(",", ",\n");
            }

        }
        
        public void Clear()
        {
            _storage.Clear();
        }
    }
}