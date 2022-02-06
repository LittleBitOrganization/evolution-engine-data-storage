using LittleBit.Modules.CoreModule;

namespace LittleBit.Modules.StorageModule
{
    public class StorageData<T> where T : Data, new()
    {
        private IDataStorageService dataStorageService;
        private readonly string key;

        private T value;
        private object handler;

        public T Value
        {
            get { return value; }
            set
            {
                this.value = value;
                dataStorageService.SetData(key, value);
            }
        }

        public StorageData(object handler, IDataStorageService dataStorageService, string key)
        {
            this.handler = handler;
            this.key = key;
            this.dataStorageService = dataStorageService;
         
            Update();
        }
        
        public void Update()
        {
            value = dataStorageService.GetData<T>(key);
        }

        public void Subscribe(IDataStorageService.GenericCallback<T> onUpdateData)
        {
            dataStorageService.AddUpdateDataListener(handler, key, onUpdateData);
        }

        public void Unsubscribe(IDataStorageService.GenericCallback<T> onUpdateData)
        {
            dataStorageService.RemoveUpdateDataListener(handler, key, onUpdateData);
        }

        public void RemoveAllListeners()
        {
            dataStorageService.RemoveAllUpdateDataListenersOnObject(handler);
        }
        
    }
}