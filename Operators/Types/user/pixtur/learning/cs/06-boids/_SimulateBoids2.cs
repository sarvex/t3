using SharpDX.Direct3D11;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;

namespace T3.Operators.Types.Id_9c3f142f_76d2_4395_9796_3857413084e2
{
    public class _SimulateBoids2 : Instance<_SimulateBoids2>
    {

        [Output(Guid = "b36d411b-8a92-426e-94ab-0c9bff6b92a7")]
        public readonly Slot<T3.Core.DataTypes.BufferWithViews> Points = new Slot<T3.Core.DataTypes.BufferWithViews>();

        [Input(Guid = "1a0f4276-cd30-4974-b9de-5b7f4f4de6d2")]
        public readonly InputSlot<T3.Core.DataTypes.BufferWithViews> PointBuffer = new InputSlot<T3.Core.DataTypes.BufferWithViews>();

        [Input(Guid = "75ee8f92-f110-40d0-9bdc-ccf9413dc266")]
        public readonly InputSlot<float> EffectLayer = new InputSlot<float>();

        [Input(Guid = "fce39d1d-74cf-4769-8a85-3ba5192ac26c")]
        public readonly InputSlot<float> CellSize = new InputSlot<float>();

        [Input(Guid = "143cf325-a279-4a3b-aac5-32a3dd63a117")]
        public readonly InputSlot<float> JitterCellLookup = new InputSlot<float>();

        [Input(Guid = "5e8c41a5-5389-4ab7-9060-3b0a6ca98267")]
        public readonly InputSlot<SharpDX.Direct3D11.Texture2D> EffectTexture = new InputSlot<SharpDX.Direct3D11.Texture2D>();

        [Input(Guid = "a2ea7dcb-f30d-4f13-8e7d-2faa38518525")]
        public readonly InputSlot<float> EffectTwist = new InputSlot<float>();

        [Input(Guid = "9b5414e6-d6c1-47ab-b5ee-a462a2474c30")]
        public readonly InputSlot<bool> Wrap = new InputSlot<bool>();

        [Input(Guid = "8a70798c-6448-44ef-a0cb-c9bf22bcc3e1")]
        public readonly InputSlot<T3.Core.DataTypes.StructuredList> BoidDefinitions = new InputSlot<T3.Core.DataTypes.StructuredList>();


    }
}

