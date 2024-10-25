using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Milutools.Logger;
using Milutools.Milutools.General;
using UnityEngine;

namespace Milutools.Audio
{
    public abstract class AudioResources : ScriptableObject
    {
        internal abstract void SetupDictionary(Dictionary<EnumIdentifier, AudioClip> dictionary);
        internal abstract AudioClip GetClip(int rawID);
    }
    public class AudioResources<T> : AudioResources where T : Enum, IConvertible
    {
        [Serializable]
        public class AudioItem
        {
            [HideInInspector]
            public string Name;
            public T Identifier;
            public AudioClip Clip;

            [NonSerialized]
            internal T lstIdentifier;
        }

        [SerializeField]
        public List<AudioItem> Items = new();
        
        private void OnEnable()
        {
            foreach (var item in Enum.GetValues(typeof(T)))
            {
                if (Items.Exists(x => x.Identifier.Equals(item)))
                {
                    continue;
                }
                Items.Add(new AudioItem()
                {
                    Identifier = (T)item,
                    Name = item.ToString()
                });
            }
        }

        private void OnValidate()
        {
            foreach (var item in Items)
            {
                if (!item.lstIdentifier.Equals(item.Identifier) || string.IsNullOrEmpty(item.Name))
                {
                    item.Name = item.Identifier.ToString();
                    item.lstIdentifier = item.Identifier;
                }
            }
        }

        internal override void SetupDictionary(Dictionary<EnumIdentifier, AudioClip> dictionary)
        {
            foreach (var item in Items)
            {
                var key = EnumIdentifier.Wrap(item.Identifier);
                if (!dictionary.TryAdd(key, item.Clip))
                {
                    DebugLog.LogError($"Specific audio resources '{key}' is duplicated.");
                }
            }
        }

        internal override AudioClip GetClip(int rawID)
            => Items.FirstOrDefault(x => (int)(object)x.Identifier == rawID)?.Clip;
    }
}
