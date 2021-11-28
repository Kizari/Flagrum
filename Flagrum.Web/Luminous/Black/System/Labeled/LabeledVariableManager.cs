namespace Black.System.Labeled
{
    public partial class LabeledVariableManager
    {
        public enum TimerType
        {
            TT_INCREMENT = 0x0,
            TT_DECREMENT = 0x1,
            TT_INCREASE_BY_WT = 0x2,
            TT_DECREASE_BY_WT = 0x3,
        }
    }
}
