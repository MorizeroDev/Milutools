using System;
using System.Linq;
using Milutools.AI.Nodes;
using Milutools.Logger;
using UnityEngine;

namespace Milutools.AI
{
    public abstract class BehaviourTree : MonoBehaviour
    {
        public IBehaviourNode CurrentRunningNode { get; internal set; }
        
        public enum UpdateMethods
        {
            OnUpdate, OnFixedUpdate
        }
        
        public UpdateMethods UpdateMethod = UpdateMethods.OnFixedUpdate;
    }
    
    public abstract class BehaviourTree<T> : BehaviourTree where T : BehaviourContext
    {
        #region Exposed API
        
        public abstract IBehaviourNode Build(T context);

        protected static IBehaviourNode Sequence(params IBehaviourNode[] children)
            => new SequenceNode(children);
        
        protected static IBehaviourNode Selector(params IBehaviourNode[] children)
            => new SelectorNode(children);
        
        protected static IBehaviourNode Condition(Predicate<T> condition, IBehaviourNode child = null)
            => new ConditionNode<T>(child) { Handler = condition };
        
        protected static IBehaviourNode Repeater(Predicate<T> condition, IBehaviourNode child)
            => new RepeaterNode<T>(child) { Condition = condition };
        
        protected static IBehaviourNode Repeater(int repeatCount, IBehaviourNode child)
            => new RepeaterNode<T>(child) { RepeatCount = repeatCount };
        
        protected static IBehaviourNode Inverter(IBehaviourNode child)
            => new InverterNode(child);
        
        protected static IBehaviourNode Action(BehaviourFunction<T> action)
            => new ActionNode<T>() { Handler = action };
        
        protected static IBehaviourNode Wait(float time)
            => new WaitNode() { Time = time };
        
        #endregion

        #region Logic
        
        private IBehaviourNode RootNode;

        public BehaviourContext Context;
        
        public bool RunOnAwake = true;
        public bool Loop = true;
        
        public bool Running { get; private set; }

        public event BehaviourFunction<T> OnFinished;
        
        private void Awake()
        {
            if (!Context)
            {
                DebugLog.LogError("Context is not set.");
            }
            else
            {
                Context.Tree = this;
            }
            
            RootNode = Build((T)Context);
            if (RootNode == null)
            {
                DebugLog.LogError("Null behaviour tree, this is not allowed.");
            }
            
            if (RunOnAwake)
            {
                Running = true;
            }
        }

        public void Start()
        {
            Running = true;
        }

        public void Stop()
        {
            Running = false;
        }

        public void ResetTree()
        {
            RootNode.Reset();
        }
        
        private void Finish()
        {
            Running = false;
            ResetTree();
            OnFinished?.Invoke((T)Context);
        }
        
        private void UpdateTree()
        {
            if (!Running && !Loop)
            {
                return;
            }

            Context.UpdateContext();
            if (CurrentRunningNode == null)
            {
                if (RootNode.Run(Context) != BehaviourState.Running)
                {
                    Finish();
                }
            }
            else
            {
                var state = CurrentRunningNode.Run(Context);
                if (state != BehaviourState.Running)
                {
                    // Restore behaviour tree state
                    var node = CurrentRunningNode;
                    CurrentRunningNode = null;
                    while (node.Previous != null)
                    {
                        node = node.Previous;
                        state = node.Resume(Context, state);
                        if (state == BehaviourState.Running)
                        {
                            return;
                        }
                    }

                    Finish();
                }
            }
        }

        private void Update()
        {
            if (UpdateMethod != UpdateMethods.OnUpdate)
            {
                return;
            }
            
            UpdateTree();
        }

        private void FixedUpdate()
        {
            if (UpdateMethod != UpdateMethods.OnFixedUpdate)
            {
                return;
            }
            
            UpdateTree();
        }

        #endregion
    }
}
