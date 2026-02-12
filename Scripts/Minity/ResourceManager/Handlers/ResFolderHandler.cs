using System;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Minity.ResourceManager.Handlers
{
    [ResHandler("resources")]
    public class ResFolderHandler : IResHandler
    {
        private Object? _resource;
        private string _location;
        
        public void Initialize(Uri uri)
        {
            _location = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
        }
        
        public async Task<Object> LoadAsync<T>() where T : Object
        {
            var task = Resources.LoadAsync<T>(_location);
            var tcs = new TaskCompletionSource<Object>();
            task.completed += (_) =>
            {
                tcs.TrySetResult(task.asset);
            };
            _resource = await tcs.Task;
            return _resource;
        }

        public Object Load<T>() where T : Object
        {
            _resource = Resources.Load<T>(_location);
            return _resource;
        }

        public void Release()
        {
            if (_resource == null)
            {
                return;
            }
            
            if (_resource is not GameObject && _resource is not Component)
            {
                Resources.UnloadAsset(_resource);
            }
            
            _resource = null;
        }
    }
}
