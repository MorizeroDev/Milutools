using System;
using System.Collections;
using System.Collections.Generic;
using Milease.Core;
using Milease.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Milutools.SceneRouter
{
    public class BlackFade : LoadingAnimator
    {
        public Image Panel;
        
        public override void AboutToLoad()
        {
            Panel.Milease(UMN.Color, Color.clear, Color.black, 0.5f)
                .Then(
                    new Action(ReadyToLoad).AsMileaseKeyEvent()
                )
                .UsingResetMode(RuntimeAnimationPart.AnimationResetMode.ResetToInitialState)
                .PlayImmediately();
        }

        public override void OnLoaded()
        {
            Panel.Milease(UMN.Color, Color.black, Color.clear, 0.5f)
                .Then(
                    new Action(FinishLoading).AsMileaseKeyEvent()
                )
                .UsingResetMode(RuntimeAnimationPart.AnimationResetMode.ResetToInitialState)
                .PlayImmediately();
        }
    }
}
