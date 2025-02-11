using T3.Core.DataTypes;
using T3.Core;
using T3.Core.Operator;
using T3.Core.Operator.Attributes;
using T3.Core.Operator.Slots;
using T3.Core.Resource;

namespace T3.Operators.Types.Id_73a55607_c892_4a85_946b_e37354c4c0e4
{
    public class DrawParticles : Instance<DrawParticles>
    {
        [Output(Guid = "29ca47fe-0dbe-4727-a958-55b9c78ca50c", DirtyFlagTrigger = DirtyFlagTrigger.Always)]
        public readonly Slot<Command> Output = new Slot<Command>();


        [Input(Guid = "7fb7da7e-50e1-4221-b001-df6a5a2c58a1")]
        public readonly InputSlot<ParticleSystem> ParticleSystem = new InputSlot<ParticleSystem>();

        [Input(Guid = "0a4f49d0-8b4b-47f1-a2cf-134ebb62cb74")]
        public readonly InputSlot<float> Size = new InputSlot<float>();

        [Input(Guid = "610ac5df-aa5b-4967-8f06-a3e071ce1225")]
        public readonly InputSlot<System.Numerics.Vector4> Color = new InputSlot<System.Numerics.Vector4>();

        [Input(Guid = "c34e46fd-b1be-4224-bd8a-32e418eed96c")]
        public readonly InputSlot<System.Numerics.Vector3> LightPosition = new InputSlot<System.Numerics.Vector3>();

        [Input(Guid = "b7cff360-cbb6-4d28-98a1-dd0fd31145f7")]
        public readonly InputSlot<float> LightIntensity = new InputSlot<float>();

        [Input(Guid = "1b9b4455-d510-4151-93c6-e03a5eb325b8")]
        public readonly InputSlot<float> LightDecay = new InputSlot<float>();

        [Input(Guid = "981e17e2-6f92-4651-90a5-7c6ee4f04438")]
        public readonly InputSlot<float> RoundShading = new InputSlot<float>();

        [Input(Guid = "152e24c1-6743-4636-ac30-533a33b22bbc")]
        public readonly InputSlot<float> NearPlane = new InputSlot<float>();

        [Input(Guid = "3cb3866f-48d0-4824-9302-73a92ba6b132")]
        public readonly InputSlot<T3.Core.DataTypes.Gradient> ColorOverLife = new InputSlot<T3.Core.DataTypes.Gradient>();

        [Input(Guid = "98d353fe-e65e-4ccb-aa79-502dfde30fba")]
        public readonly InputSlot<SharpDX.Direct3D11.Texture2D> ColorForDirection = new InputSlot<SharpDX.Direct3D11.Texture2D>();

    }
}

