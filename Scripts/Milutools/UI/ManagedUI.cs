using System;
using Milease.Core;
using Milease.Core.Animator;
using Milease.Enums;
using Milease.Utils;
using Milutools.Logger;
using UnityEngine;
using UnityEngine.UI;

namespace Milutools.Milutools.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(GraphicRaycaster))]
    public abstract class ManagedUI : MonoBehaviour
    {
        internal Action CloseInternalCallback;
        internal object Callback;

        internal bool WithTransition;
        
        private MilInstantAnimator fadeInAnimator, fadeOutAnimator;
        private GraphicRaycaster rayCaster;
        private CanvasGroup group;

        private bool closing = false;
        
        private void Awake()
        {
            rayCaster = GetComponent<GraphicRaycaster>();
            group = GetComponent<CanvasGroup>();
            
            rayCaster.enabled = false;
            
            fadeInAnimator = 
                new Action(() => group.enabled = true).AsMileaseKeyEvent()
                    .ThenOneByOne(
                        group.Milease(nameof(group.alpha), 0f, 1f, 0.5f, 0f, EaseFunction.Circ, EaseType.Out),
                        new Action(() =>
                        {
                            rayCaster.enabled = true;
                            group.enabled = false;
                        }).AsMileaseKeyEvent()
                    )
                    .UsingResetMode(RuntimeAnimationPart.AnimationResetMode.ResetToInitialState);
            
            fadeOutAnimator = 
                new Action(() =>
                    {
                        rayCaster.enabled = false;
                        group.enabled = true;
                    }).AsMileaseKeyEvent()
                    .ThenOneByOne(
                        group.Milease(nameof(group.alpha), 1f, 0f, 0.5f, 
                            0f, EaseFunction.Circ, EaseType.Out),
                        new Action(() =>
                        {
                            gameObject.SetActive(false);
                            CloseInternalCallback?.Invoke();
                            closing = false;
                        }).AsMileaseKeyEvent()
                    )
                    .UsingResetMode(RuntimeAnimationPart.AnimationResetMode.ResetToInitialState);
            
            Begin();
        }

        private void OnEnable()
        {
            closing = false;
            if (WithTransition)
            {
                fadeInAnimator.PlayImmediately();
            }
            else
            {
                group.enabled = false;
            }
        }

        protected abstract void Begin();
        protected abstract void AboutToClose();

        internal abstract void Open(object parameter);

        internal void CloseInternal()
        {
            if (closing)
            {
                DebugLog.LogWarning("Duplicated closing operation on UI.");
                return;
            }
            
            if (WithTransition)
            {
                closing = true;
                fadeOutAnimator.PlayImmediately();
            }
            else
            {
                CloseInternalCallback?.Invoke();
            }
        }
    }
    
    public abstract class ManagedUIReturnValueOnly<R> : ManagedUI
    {
        internal override void Open(object parameter)
        {
            AboutToOpen();
        }

        public void Close(R returnValue)
        {
            CloseInternalCallback = () =>
            {
                AboutToClose();
                ((Action<R>)Callback)?.Invoke(returnValue);
            };
            CloseInternal();
        }
        
        public abstract void AboutToOpen();
    }
    
    public abstract class ManagedUI<P> : ManagedUI
    {
        internal override void Open(object parameter)
        {
            AboutToOpen((P)parameter);
        }
        
        public void Close()
        {
            CloseInternalCallback = () =>
            {
                AboutToClose();
                ((Action)Callback)?.Invoke();
            };
            CloseInternal();
        }
        
        public abstract void AboutToOpen(P parameter);
    }
    
    public abstract class ManagedUI<P, R> : ManagedUI
    {
        internal override void Open(object parameter)
        {
            AboutToOpen((P)parameter);
        }
        
        public void Close(R returnValue)
        {
            CloseInternalCallback = () =>
            {
                AboutToClose();
                ((Action<R>)Callback)?.Invoke(returnValue);
            };
            CloseInternal();
        }
        
        public abstract void AboutToOpen(P parameter);
    }
}
