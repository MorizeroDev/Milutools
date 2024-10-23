using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Milutools.Audio
{
    public class AudioPreset : MonoBehaviour
    {
        public enum AudioBehaviour
        {
            Replace, Keep
        }

        [Serializable]
        public class PresetData
        {
            public AudioResources Resources;
            public AudioBehaviour Behaviour;
            public int ID;
            public float StartTime;
        }

        public PresetData BGM, BGS;

        private void Start()
        {
            if (BGM.Behaviour == AudioBehaviour.Replace)
            {
                AudioManager.Player.SwitchClip(AudioPlayerType.BGMPlayer, BGM.Resources.GetClip(BGM.ID), true, BGM.StartTime);
            }
            if (BGS.Behaviour == AudioBehaviour.Replace)
            {
                AudioManager.Player.SwitchClip(AudioPlayerType.BGSPlayer, BGS.Resources.GetClip(BGS.ID), true, BGS.StartTime);
            }
        }
    }
}
