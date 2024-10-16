using System;
using Milutools.Logger;

namespace Milutools.Milutools.UI
{
    public class UIContext
    {
        internal UI UI;
        internal object Parameter = null;
        internal bool WithTransition = true;

        public UIContext SetParameter<T>(T parameter)
        {
            if (typeof(T) != UI.ParameterType)
            {
                DebugLog.LogError($"Parameter for {UI.Identifier} is incorrect, " +
                                  $"expected type: {UI.ParameterType.FullName}, actual: {typeof(T).FullName}");
                return this;
            }

            Parameter = parameter;
            return this;
        }

        public UIContext WithoutTransition()
        {
            WithTransition = false;
            return this;
        }

        public void Open<T>(Action<T> callback = null)
        {
            OpenInternal(callback);
        }
        
        public void Open(Action callback = null)
        {
            if (UI.ReturnValueType != null && callback != null)
            {
                DebugLog.LogError($"Specific UI {UI.Identifier} has return value, use Open<T>() instead.");
                return;
            }
            OpenInternal(callback);
        }

        private void OpenInternal(object callback)
        {
            var go = UI.Create();
            var ui = go.GetComponent<ManagedUI>();
            ui.WithTransition = WithTransition;
            ui.Callback = callback;
            ui.Open(Parameter);
            go.SetActive(true);
        }
    }
}
