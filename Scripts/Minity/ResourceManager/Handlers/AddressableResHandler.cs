#if UNITY_ENABLE_ADDRESSABLES
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Minity.ResourceManager.Handlers
{
    [ResHandler("addressable")]
    public class AddressableResHandler : IResHandler
    {
        private AsyncOperationHandle? _addressableHandle;
        
        private Object? _resource;
        private string _location;
        
        public void Initialize(Uri uri)
        {
            _location = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
        }
        
        public async Task<Object> LoadAsync<T>() where T : Object
        {
            var task = Addressables.LoadAssetAsync<T>(_location);
            await task.Task;
            
            if (task.Status != AsyncOperationStatus.Succeeded)
            {
                throw new Exception("Resource load failed");
            }

            _resource = task.Result;
            _addressableHandle = task;
            
            return _resource!;
        }

        public Object Load<T>() where T : Object
        {
            var handle = Addressables.LoadAssetAsync<T>(_location);
            handle.WaitForCompletion();
            
            _resource = handle.Result;
            _addressableHandle = handle;
            
            return _resource;
        }

        public void Release()
        {
            if (_addressableHandle == null || !_addressableHandle.Value.IsValid())
            {
                return;
            }
            Addressables.Release(_addressableHandle.Value);
            _addressableHandle = null;
            _resource = null;
        }
    }
}
#endif
