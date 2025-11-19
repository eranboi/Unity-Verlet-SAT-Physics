namespace State_Machine
{
    public class PlayerState : BaseState
    {
        public InputHandler input;

        public override void SetCore(Core _core)
        {
            base.SetCore(_core);
            if (_core != null)
            {
                input = _core.GetComponent<InputHandler>();
            }
            else
            {
                input = null;
            }
        }
    }
}