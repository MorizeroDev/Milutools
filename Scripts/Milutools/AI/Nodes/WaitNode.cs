using System;
using UnityEngine;

namespace Milutools.AI.Nodes
{
    public class WaitNode : IBehaviourNode
    {
        internal float Time = 0f;
        IBehaviourNode IBehaviourNode.Previous { get; set; }
        BehaviourState IBehaviourNode.State { get; set; }

        private float tick = 0f;

        BehaviourState IBehaviourNode.Run(BehaviourContext context)
        {
            tick += context.Tree.UpdateMethod == BehaviourTree.UpdateMethods.OnUpdate ? 
                    UnityEngine.Time.deltaTime : UnityEngine.Time.fixedDeltaTime;
            var finished = tick >= Time;
            if (!finished)
            {
                context.Tree.CurrentRunningNode = this;
            }
            return finished ? BehaviourState.Succeed : BehaviourState.Running;
        }

        BehaviourState IBehaviourNode.Resume(BehaviourContext context, BehaviourState innerState)
        {
            throw new NotImplementedException();
        }

        void IBehaviourNode.Reset()
        {
            tick = 0f;
        }
    }
}
