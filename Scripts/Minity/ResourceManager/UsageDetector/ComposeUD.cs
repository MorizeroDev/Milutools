using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Minity.ResourceManager.UsageDetector
{
    public class ComposeUD : IUsageDetector
    {
        private readonly List<IUsageDetector> _detectors = new();
        private int startIdx = 0;

        private readonly HashSet<uint> _detectedScenes = new();
        private readonly HashSet<Object> _detectedObjects = new();
        
        public List<IUsageDetector> GetDetectors() => _detectors;
        
        public void Initialize(object? bind)
        {
            
        }
        
        public bool IsUsing()
        {
            var cnt = _detectors.Count;
            for (var i = startIdx; i < cnt; i++)
            {
                if (_detectors[i].IsUsing())
                {
                    return true;
                }

                startIdx++;
            }

            return false;
        }

        public IUsageDetector CombineDetector(IUsageDetector detector)
        {
            if (detector is SceneUD su)
            {
                if (!_detectedScenes.Add(su.GetSceneVersion()))
                {
                    return this;
                }
            }
            
            if (detector is ObjectLinkUD olu)
            {
                if (!_detectedObjects.Add(olu.GetLinkObject()))
                {
                    return this;
                }
            }
            
            if (startIdx == 0)
            {
                _detectors.Add(detector);
            }
            else
            {
                startIdx--;
                _detectors[startIdx] = detector;
            }

            return this;
        }
    }
}
