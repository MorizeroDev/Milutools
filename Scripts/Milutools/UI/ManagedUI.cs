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
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class ManagedUI : MonoBehaviour
    {
        internal UI Source;
        internal Action CloseInternalCallback;
        internal object Callback;

        internal bool WithTransition;
        
        private MilInstantAnimator fadeInAnimator, fadeOutAnimator;
        private CanvasGroup group;
        private Canvas canvas;

        private bool closing = false;
        private bool opening = false;

        internal void SetSortingOrder(int order)
        {
            canvas.sortingOrder = order;
        }
        
        protected void OverrideInTransition(MilInstantAnimator animator)
            => fadeInAnimator = animator;
        
        protected void OverrideOutTransition(MilInstantAnimator animator)
            => fadeOutAnimator = animator;
        
        
        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
            canvas = GetComponent<Canvas>();
            
            Begin();
            
            fadeInAnimator ??= 
                group.Milease(nameof(group.alpha), 0f, 1f, 0.25f, 0f, EaseFunction.Quad, EaseType.Out)
                    .UsingResetMode(RuntimeAnimationPart.AnimationResetMode.ResetToInitialState);
            
            fadeOutAnimator ??= 
                group.Milease(nameof(group.alpha), 1f, 0f, 0.25f)
                    .UsingResetMode(RuntimeAnimationPart.AnimationResetMode.ResetToInitialState);
        }

        private void OnEnable()
        {
            closing = false;
            if (WithTransition)
            {
                opening = true;
                group.enabled = true;
                group.alpha = 1f;
                fadeInAnimator.PlayImmediately(() =>
                {
                    opening = false;
                    group.enabled = false;
                });
            }
            else
            {
                opening = false;
                group.enabled = false;
            }
        }

        protected abstract void Begin();
        protected abstract void AboutToClose();

        internal abstract void Open(object parameter);

        private void OnClosed()
        {
            CloseInternalCallback?.Invoke();
            closing = false;
            if (Source.Mode != UIMode.Singleton)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        
        internal void CloseInternal()
        {
            if (opening)
            {
                return;
            }
            
            if (closing)
            {
                DebugLog.LogWarning("Duplicated closing operation on UI.");
                return;
            }

            UIManager.CurrentSortingOrder--;
            if (WithTransition)
            {
                closing = true;
                group.enabled = true;
                group.alpha = 1f;
                fadeOutAnimator.PlayImmediately(OnClosed);
            }
            else
            {
                OnClosed();
            }
        }
    }
    
    public abstract class ManagedUIReturnValueOnly<T, R> : ManagedUI
    {
        public static void Open(Action<R> callback = null)
        {
            UIManager.Get(typeof(ManagedUIReturnValueOnly<T, R>))
                     .Open(callback);
        }
        
        internal override void Open(object parameter)
        {
            AboutToOpen();
        }

        public void Close(R returnValue)
        {
            CloseInternalCallback = () =>
            {
                ((Action<R>)Callback)?.Invoke(returnValue);
            };
            AboutToClose();
            CloseInternal();
        }
        
        public abstract void AboutToOpen();
    }
    
    public abstract class ManagedUI<T, P> : ManagedUI
    {
        public static void Open(P parameter, Action callback = null)
        {
            UIManager.Get(typeof(ManagedUI<T, P>))
                .SetParameter(parameter)
                .Open(callback);
        }
        
        internal override void Open(object parameter)
        {
            AboutToOpen((P)parameter);
        }
        
        public void Close()
        {
            CloseInternalCallback = () =>
            {
                ((Action)Callback)?.Invoke();
            };
            AboutToClose();
            CloseInternal();
        }
        
        public abstract void AboutToOpen(P parameter);
    }
    
    public abstract class ManagedUI<T, P, R> : ManagedUI
    {
        public static void Open(P parameter, Action<R> callback = null)
        {
            UIManager.Get(typeof(ManagedUI<T, P, R>))
                .SetParameter(parameter)
                .Open(callback);
        }
        
        internal override void Open(object parameter)
        {
            AboutToOpen((P)parameter);
        }
        
        public void Close(R returnValue)
        {
            CloseInternalCallback = () =>
            {
                ((Action<R>)Callback)?.Invoke(returnValue);
            };
            AboutToClose();
            CloseInternal();
        }
        
        public abstract void AboutToOpen(P parameter);
    }
}
