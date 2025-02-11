using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace T3.Operators.Types.Id_97032147_ba0c_4454_b878_1048d8faea05
{
    public class InvertFloat : Instance<InvertFloat>
    {
        [Output(Guid = "b383231e-c952-4b0d-adf3-b97c61c02053")]
        public readonly Slot<float> Result = new();

        public InvertFloat()
        {
            Result.UpdateAction = Update;
        }

        private void Update(EvaluationContext context)
        {
            Result.Value = -A.GetValue(context);
        }
        
        [Input(Guid = "020acbf3-de2d-48f6-8515-960014bb1aa9")]
        public readonly InputSlot<float> A = new();

    }
}
