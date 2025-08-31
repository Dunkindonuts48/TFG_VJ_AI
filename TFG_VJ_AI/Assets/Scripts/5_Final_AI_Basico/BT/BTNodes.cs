namespace TFG.AI.BT
{
    public enum BTState { Success, Failure, Running }

    public abstract class BTNode
    {
        public abstract BTState Tick();
    }

    public class Sequence : BTNode
    {
        private readonly BTNode[] ch;
        public Sequence(params BTNode[] c) { ch = c; }
        public override BTState Tick()
        {
            foreach (var n in ch)
            {
                var s = n.Tick();
                if (s != BTState.Success) return s;
            }
            return BTState.Success;
        }
    }

    public class Selector : BTNode
    {
        private readonly BTNode[] ch;
        public Selector(params BTNode[] c) { ch = c; }
        public override BTState Tick()
        {
            foreach (var n in ch)
            {
                var s = n.Tick();
                if (s != BTState.Failure) return s;
            }
            return BTState.Failure;
        }
    }

    public class ActionNode : BTNode
    {
        private readonly System.Func<BTState> f;
        public ActionNode(System.Func<BTState> f) { this.f = f; }
        public override BTState Tick() => f();
    }
}
