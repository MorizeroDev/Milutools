using System;

namespace Milutools.AI.Nodes
{
    public class ActionNode<T> : IBehaviourNode where T : BehaviourContext
    {
        internal BehaviourFunction<T> Handler;
        IBehaviourNode IBehaviourNode.Previous { get; set; }
        BehaviourState IBehaviourNode.State { get; set; }
        
        BehaviourState IBehaviourNode.Run(BehaviourContext context)
        {
            var state = Handler((T)context);
            if (state == BehaviourState.Running)
            {
                context.Tree.CurrentRunningNode = this;
            }

            return state;
        }

        BehaviourState IBehaviourNode.Resume(BehaviourContext context, BehaviourState innerState)
        {
            throw new NotImplementedException();
        }

        void IBehaviourNode.Reset()
        {
            
        }
    }
}
