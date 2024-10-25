using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Milutools.SceneRouter
{
    public abstract class LoadingAnimator : MonoBehaviour
    {
        public float Progress => loadingOperation?.progress ?? 0f;
        public string TargetScene { get; internal set; }

        private AsyncOperation loadingOperation;
        
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            AboutToLoad();
        }

        /// <summary>
        /// Call when the loading animator is ready
        /// </summary>
        protected void ReadyToLoad()
        {
            loadingOperation = SceneManager.LoadSceneAsync(TargetScene);
            loadingOperation!.completed += (_) => OnLoaded();
        }

        /// <summary>
        /// Call when the loading animator finishes
        /// </summary>
        protected void FinishLoading()
        {
            Destroy(gameObject);
        }
        
        public abstract void AboutToLoad();
        
        public abstract void OnLoaded();
    }
}
