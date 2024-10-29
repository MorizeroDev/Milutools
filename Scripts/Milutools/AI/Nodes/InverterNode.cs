namespace Milutools.AI.Nodes
{
    public class InverterNode : IBehaviourNode
    {
        internal IBehaviourNode Node;
        IBehaviourNode IBehaviourNode.Previous { get; set; }
        BehaviourState IBehaviourNode.State { get; set; }
        
        internal InverterNode(IBehaviourNode node)
        {
            Node = node;
            if (node != null)
            {
                node.Previous = this;
            }
        }

        BehaviourState InvertState(BehaviourState state)
        {
            return state switch
            {
                BehaviourState.Failed => BehaviourState.Succeed,
                BehaviourState.Succeed => BehaviourState.Failed,
                _ => state
            };
        }
        
        BehaviourState IBehaviourNode.Run(BehaviourContext context)
        {
            return InvertState(Node.Run(context));
        }
        
        BehaviourState IBehaviourNode.Resume(BehaviourContext context, BehaviourState innerState)
        {
            return InvertState(innerState);
        }
        
        void IBehaviourNode.Reset()
        {
            Node.Reset();
        }
    }
}
