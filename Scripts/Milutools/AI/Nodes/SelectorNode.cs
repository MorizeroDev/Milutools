using System.Collections.Generic;
using System.Linq;

namespace Milutools.AI.Nodes
{
    public class SelectorNode : IBehaviourNode
    {
        internal List<IBehaviourNode> Nodes;
        IBehaviourNode IBehaviourNode.Previous { get; set; }
        BehaviourState IBehaviourNode.State { get; set; }
        
        private int index = 0;

        internal SelectorNode(IBehaviourNode[] nodes)
        {
            Nodes = nodes.ToList();
            foreach (var node in Nodes)
            {
                node.Previous = this;
            }
        }
        
        BehaviourState IBehaviourNode.Run(BehaviourContext context)
        {
            for (; index < Nodes.Count; index++)
            {
                var node = Nodes[index];
                var state = node.Run(context);
                if (state == BehaviourState.Succeed)
                {
                    return state;
                }
                if (state == BehaviourState.Running)
                {
                    return state;
                }
            }
            return BehaviourState.Failed;
        }
        
        BehaviourState IBehaviourNode.Resume(BehaviourContext context, BehaviourState innerState)
        {
            if (innerState == BehaviourState.Succeed)
            {
                return innerState;
            }
            index++;
            return ((IBehaviourNode)this).Run(context);
        }
        
        void IBehaviourNode.Reset()
        {
            index = 0;
            foreach (var node in Nodes)
            {
                node.Reset();
            }
        }
    }
}
