using System;

namespace Milutools.AI.Nodes
{
    public class ConditionNode<T> : IBehaviourNode where T : BehaviourContext
    {
        internal Predicate<T> Handler;
        internal IBehaviourNode Node;
        IBehaviourNode IBehaviourNode.Previous { get; set; }
        BehaviourState IBehaviourNode.State { get; set; }

        internal ConditionNode(IBehaviourNode node)
        {
            Node = node;
            if (node != null)
            {
                node.Previous = this;
            }
        }
        
        BehaviourState IBehaviourNode.Run(BehaviourContext context)
        {
            if (Handler((T)context))
            {
                return Node?.Run(context) ?? BehaviourState.Succeed;
            }

            return BehaviourState.Failed;
        }
        
        BehaviourState IBehaviourNode.Resume(BehaviourContext context, BehaviourState innerState)
        {
            return innerState;
        }
        
        void IBehaviourNode.Reset()
        {
            Node?.Reset();
        }
    }
}
