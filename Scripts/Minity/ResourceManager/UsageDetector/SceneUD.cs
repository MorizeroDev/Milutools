using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Minity.ResourceManager.UsageDetector
{
    public struct SceneUD : IUsageDetector
    {
        private static uint sceneVersion;
        private static bool initialized = false;

        private uint bindSceneVersion;

        public uint GetSceneVersion() => bindSceneVersion;
        
        public void Initialize(object? bind)
        {
            if (!initialized)
            {
                initialized = true;
                SceneManager.activeSceneChanged += OnSceneChanged;
                Application.quitting += UnRegisterSceneEvent;
            }

            bindSceneVersion = sceneVersion;
        }

        public bool IsUsing()
        {
            return sceneVersion == bindSceneVersion;
        }

        private static void UnRegisterSceneEvent()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
            Application.quitting -= UnRegisterSceneEvent;
        }
        
        private static void OnSceneChanged(Scene scene1, Scene scene2)
        {
            sceneVersion++;
        }
        
        public IUsageDetector CombineDetector(IUsageDetector detector)
        {
            var compose = new ComposeUD();
            compose.CombineDetector(this);
            return compose;
        }
    }
}
