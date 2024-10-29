using System;

namespace Milutools.AI.Nodes
{
    public class RepeaterNode<T> : IBehaviourNode where T : BehaviourContext
    {
        internal int RepeatCount = -1;
        internal Predicate<T> Condition;
        internal IBehaviourNode Node;
        IBehaviourNode IBehaviourNode.Previous { get; set; }
        BehaviourState IBehaviourNode.State { get; set; }

        private int count = 0;
        
        internal RepeaterNode(IBehaviourNode node)
        {
            Node = node;
            if (node != null)
            {
                node.Previous = this;
            }
        }
        
        BehaviourState IBehaviourNode.Run(BehaviourContext context)
        {
            if (RepeatCount == -1)
            {
                while (!Condition((T)context))
                {
                    if (Node.Run(context) == BehaviourState.Running)
                    {
                        return BehaviourState.Running;
                    }
                }
            }
            else
            {
                for (; count < RepeatCount; count++)
                {
                    if (Node.Run(context) == BehaviourState.Running)
                    {
                        return BehaviourState.Running;
                    }
                }
            }
            
            return BehaviourState.Succeed;
        }
        
        BehaviourState IBehaviourNode.Resume(BehaviourContext context, BehaviourState innerState)
        {
            count++;
            return ((IBehaviourNode)this).Run(context);
        }
        
        void IBehaviourNode.Reset()
        {
            count = 0;
            Node.Reset();
        }
    }
}
