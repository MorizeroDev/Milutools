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
        
        private void Awake()
        {
            group = GetComponent<CanvasGroup>();

            canvas = GetComponent<Canvas>();
            
            fadeInAnimator = 
                new Action(() =>
                    {
                        opening = true;
                        group.enabled = true;
                    }).AsMileaseKeyEvent()
                    .ThenOneByOne(
                        group.Milease(nameof(group.alpha), 0f, 1f, 0.25f, 0f, EaseFunction.Quad, EaseType.Out),
                        new Action(() =>
                        {
                            opening = false;
                            group.enabled = false;
                        }).AsMileaseKeyEvent()
                    )
                    .UsingResetMode(RuntimeAnimationPart.AnimationResetMode.ResetToInitialState);
            
            fadeOutAnimator = 
                new Action(() =>
                    {
                        group.enabled = true;
                    }).AsMileaseKeyEvent()
                    .ThenOneByOne(
                        group.Milease(nameof(group.alpha), 1f, 0f, 0.25f),
                        new Action(() =>
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
                opening = false;
                group.enabled = false;
            }
        }

        protected abstract void Begin();
        protected abstract void AboutToClose();

        internal abstract void Open(object parameter);

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
