using System;
using System.Collections.Generic;
using Milutools.Logger;
using Milutools.Milutools.General;
using UnityEngine;

namespace Milutools.Audio
{
    public class AudioManager
    {
        internal static readonly Dictionary<EnumIdentifier, AudioClip> ResourcesList = new();
        internal static bool Enabled = false;
        internal static AudioPlayer Player;
        
        public static void Setup(string resPath)
        {
            if (Enabled)
            {
                DebugLog.LogError("Duplicated audio manager initialization.");
                return;
            }
            
            var collection = Resources.LoadAll<AudioResources>(resPath);
            foreach (var list in collection)
            {
                list.SetupDictionary(ResourcesList);
            }

            var go = new GameObject("[Audio Manager]", typeof(AudioPlayer));
            go.SetActive(true);
            Player = go.GetComponent<AudioPlayer>();

            Enabled = true;
        }

        public static void SetBGM<T>(T audio, bool transition = true, float startPosition = 0f) where T : Enum
            => SetAudio(AudioPlayerType.BGMPlayer, audio, transition, startPosition);
        
        public static void SetBGS<T>(T audio, bool transition = true, float startPosition = 0f) where T : Enum
            => SetAudio(AudioPlayerType.BGSPlayer, audio, transition, startPosition);

        public static float PlaySnd<T>(T audio) where T : Enum
        {
            if (!Enabled)
            {
                DebugLog.LogError("The audio manager is not setup yet.");
                return 0f;
            }

            if (audio == null)
            {
                return 0f;
            }
            
            var key = EnumIdentifier.Wrap(audio);
            if (!ResourcesList.ContainsKey(key))
            {
                DebugLog.LogError($"Specific audio resources '{key}' is not included in the setup.");
                return 0f;
            }

            var clip = ResourcesList[key];
            Player.PlaySnd(clip);

            return clip.length;
        }

        public static float GetVolume(AudioPlayerType type)
        {
            if (!Enabled)
            {
                DebugLog.LogError("The audio manager is not setup yet.");
                return 0f;
            }

            return Player.GetVolume(type);
        }
        
        public static void SetVolume(AudioPlayerType type, float volume)
        {
            if (!Enabled)
            {
                DebugLog.LogError("The audio manager is not setup yet.");
                return;
            }

            Player.SetVolume(type, volume);
        }
        
        internal static void SetAudio<T>(AudioPlayerType type, T audio, bool transition = true, float startPosition = 0f) where T : Enum
        {
            if (!Enabled)
            {
                DebugLog.LogError("The audio manager is not setup yet.");
                return;
            }

            if (audio == null)
            {
                Player.SwitchClip(type, null, transition, startPosition);
            }
            
            var key = EnumIdentifier.Wrap(audio);
            if (!ResourcesList.ContainsKey(key))
            {
                DebugLog.LogError($"Specific audio resources '{key}' is not included in the setup.");
                return;
            }
            
            Player.SwitchClip(type, ResourcesList[key], transition, startPosition);
        }
    }
}
