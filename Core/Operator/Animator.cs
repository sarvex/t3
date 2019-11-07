using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using T3.Core.Animation;
using T3.Core.Logging;

namespace T3.Core.Operator
{
    public class SymbolExtension
    {
        // todo: how is a symbol extension defined, what exactly does this mean
    }

    public class Animator : SymbolExtension
    {
        struct CurveId
        {
            public CurveId(Guid instanceId, Guid inputId, int index = 0)
            {
                InstanceId = instanceId;
                InputId = inputId;
                Index = index;
            }

            public CurveId(IInputSlot inputSlot, int index = 0)
            {
                InstanceId = inputSlot.Parent.Id;
                InputId = inputSlot.Id;
                Index = index;
            }

            public readonly Guid InstanceId;
            public readonly Guid InputId;
            public readonly int Index;
        }

        public void CreateInputUpdateAction<T>(IInputSlot inputSlot)
        {
            if (inputSlot is Slot<float> floatInputSlot)
            {
                var newCurve = new Curve();
                newCurve.AddOrUpdateV(EvaluationContext.GlobalTime, new VDefinition()
                                                                    {
                                                                        Value = floatInputSlot.Value,
                                                                        InType = VDefinition.Interpolation.Spline,
                                                                        OutType = VDefinition.Interpolation.Spline,
                                                                    });
                _animatedInputCurves.Add(new CurveId(inputSlot), newCurve);
                newCurve.AddOrUpdateV(EvaluationContext.GlobalTime + 1, new VDefinition()
                                                                        {
                                                                            Value = floatInputSlot.Value + 2,
                                                                            InType = VDefinition.Interpolation.Spline,
                                                                            OutType = VDefinition.Interpolation.Spline,
                                                                        });

                floatInputSlot.UpdateAction = context =>
                                              {
                                                  floatInputSlot.Value = (float)newCurve.GetSampledValue(context.Time);
                                              };
                floatInputSlot.DirtyFlag.Trigger |= DirtyFlagTrigger.Animated;
            }
            else if (inputSlot is Slot<System.Numerics.Vector4> vector4InputSlot)
            {
                var newCurveX = new Curve();
                newCurveX.AddOrUpdateV(EvaluationContext.GlobalTime, new VDefinition()
                                                                     {
                                                                         Value = vector4InputSlot.Value.X,
                                                                         InType = VDefinition.Interpolation.Spline,
                                                                         OutType = VDefinition.Interpolation.Spline,
                                                                     });
                _animatedInputCurves.Add(new CurveId(inputSlot, 0), newCurveX);
                newCurveX.AddOrUpdateV(EvaluationContext.GlobalTime + 1, new VDefinition()
                                                                         {
                                                                             Value = vector4InputSlot.Value.X + 2,
                                                                             InType = VDefinition.Interpolation.Spline,
                                                                             OutType = VDefinition.Interpolation.Spline,
                                                                         });

                var newCurveY = new Curve();
                newCurveY.AddOrUpdateV(EvaluationContext.GlobalTime, new VDefinition()
                                                                     {
                                                                         Value = vector4InputSlot.Value.Y,
                                                                         InType = VDefinition.Interpolation.Spline,
                                                                         OutType = VDefinition.Interpolation.Spline,
                                                                     });
                _animatedInputCurves.Add(new CurveId(inputSlot, 1), newCurveY);

                var newCurveZ = new Curve();
                newCurveZ.AddOrUpdateV(EvaluationContext.GlobalTime, new VDefinition()
                                                                     {
                                                                         Value = vector4InputSlot.Value.Z,
                                                                         InType = VDefinition.Interpolation.Spline,
                                                                         OutType = VDefinition.Interpolation.Spline,
                                                                     });
                _animatedInputCurves.Add(new CurveId(inputSlot, 2), newCurveZ);

                var newCurveW = new Curve();
                newCurveW.AddOrUpdateV(EvaluationContext.GlobalTime, new VDefinition()
                                                                     {
                                                                         Value = vector4InputSlot.Value.W,
                                                                         InType = VDefinition.Interpolation.Spline,
                                                                         OutType = VDefinition.Interpolation.Spline,
                                                                     });
                _animatedInputCurves.Add(new CurveId(inputSlot, 3), newCurveW);

                vector4InputSlot.UpdateAction = context =>
                                                {
                                                    vector4InputSlot.Value.X = (float)newCurveX.GetSampledValue(context.Time);
                                                    vector4InputSlot.Value.Y = (float)newCurveY.GetSampledValue(context.Time);
                                                    vector4InputSlot.Value.Z = (float)newCurveZ.GetSampledValue(context.Time);
                                                    vector4InputSlot.Value.W = (float)newCurveW.GetSampledValue(context.Time);
                                                };
                vector4InputSlot.DirtyFlag.Trigger |= DirtyFlagTrigger.Animated;
            }
            else
            {
                Log.Error("Could not create update action.");
            }
        }

        internal void CreateUpdateActionsForExistingCurves(Instance compositionInstance)
        {
            // gather all inputs that correspond to stored ids
            var relevantInputs = from curveEntry in _animatedInputCurves
                                 from childInstance in compositionInstance.Children
                                 where curveEntry.Key.InstanceId == childInstance.Id
                                 from inputSlot in childInstance.Inputs
                                 where curveEntry.Key.InputId == inputSlot.Id
                                 group (inputSlot, curveEntry.Value) by (childInstance.Id, inputSlot.Id)
                                 into inputGroup
                                 select inputGroup;

            foreach (var groupEntry in relevantInputs)
            {
                if (groupEntry.Count() == 1)
                {
                    var (inputSlot, curve) = groupEntry.First();
                    if (inputSlot is Slot<float> typedInputSlot)
                    {
                        typedInputSlot.UpdateAction = context => { typedInputSlot.Value = (float)curve.GetSampledValue(context.Time); };
                        typedInputSlot.DirtyFlag.Trigger |= DirtyFlagTrigger.Animated;
                    }
                }
                else
                {
                    var entries = groupEntry.ToArray();
                    Debug.Assert(entries.Length == 4);
                    var inputSlot = entries[0].inputSlot;
                    if (inputSlot is Slot<Vector4> vector4InputSlot)
                    {
                        vector4InputSlot.UpdateAction = context =>
                                                        {
                                                            vector4InputSlot.Value.X = (float)entries[0].Value.GetSampledValue(context.Time);
                                                            vector4InputSlot.Value.Y = (float)entries[1].Value.GetSampledValue(context.Time);
                                                            vector4InputSlot.Value.Z = (float)entries[2].Value.GetSampledValue(context.Time);
                                                            vector4InputSlot.Value.W = (float)entries[3].Value.GetSampledValue(context.Time);
                                                        };
                        vector4InputSlot.DirtyFlag.Trigger |= DirtyFlagTrigger.Animated;
                    }
                }
            }
        }

        public void RemoveAnimationFrom(IInputSlot inputSlot)
        {
            inputSlot.SetUpdateActionBackToDefault();
            inputSlot.DirtyFlag.Trigger &= ~DirtyFlagTrigger.Animated;
            var curveKeysToRemove = (from curve in _animatedInputCurves
                                     where curve.Key.InstanceId == inputSlot.Parent.Id
                                     where curve.Key.InputId == inputSlot.Id
                                     select curve.Key).ToArray(); // ToArray is needed to remove from collection in batch
            foreach (var curveKey in curveKeysToRemove)
            {
                _animatedInputCurves.Remove(curveKey);
            }
        }

        public bool IsInputSlotAnimated(IInputSlot inputSlot)
        {
            return _animatedInputCurves.ContainsKey(new CurveId(inputSlot));
        }

        public IEnumerable<Curve> GetCurvesForInput(IInputSlot inputSlot)
        {
            return from curve in _animatedInputCurves
                   where curve.Key.InstanceId == inputSlot.Parent.Id
                   where curve.Key.InputId == inputSlot.Id
                   orderby curve.Key.Index
                   select curve.Value;
        }
        
        public void Write(JsonTextWriter writer)
        {
            if (_animatedInputCurves.Count == 0)
                return;

            writer.WritePropertyName("Animator");
            writer.WriteStartArray();

            foreach (var entry in _animatedInputCurves)
            {
                writer.WriteStartObject();

                writer.WriteValue("InstanceId", entry.Key.InstanceId);
                writer.WriteValue("InputId", entry.Key.InputId);
                if (entry.Key.Index != 0)
                {
                    writer.WriteValue("Index", entry.Key.Index);
                }
                entry.Value.Write(writer); // write curve itself

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }

        public void Read(JToken inputToken)
        {
            foreach (JToken entry in inputToken)
            {
                Guid instanceId = Guid.Parse(entry["InstanceId"].Value<string>());
                Guid inputId = Guid.Parse(entry["InputId"].Value<string>());
                var indexToken = entry.SelectToken("Index");
                int index = indexToken?.Value<int>() ?? 0;
                Curve curve = new Curve();
                curve.Read(entry);

                _animatedInputCurves.Add(new CurveId(instanceId, inputId, index), curve);
            }
        }

        public List<Layer> Layers { get; set; } = new List<Layer>()
                                                  {
                                                      new Layer()
                                                      {
                                                          Clips = new List<Clip>()
                                                                  {
                                                                      new Clip() { Name = "ClipA", Id = Guid.NewGuid(), StartTime = 10, EndTime = 15, },
                                                                      new Clip() { Name = "ClipB", Id = Guid.NewGuid(), StartTime = 0, EndTime = 7, },
                                                                  }
                                                      },
                                                      new Layer()
                                                      {
                                                          Clips = new List<Clip>()
                                                                  {
                                                                      new Clip() { Name = "ClipA", Id = Guid.NewGuid(), StartTime = 12, EndTime = 12.2f, },
                                                                      new Clip() { Name = "ClipB", Id = Guid.NewGuid(), StartTime = 0, EndTime = 2, },
                                                                  }
                                                      },
                                                  };

        public IEnumerable<Clip> GetAllTimeClips()
        {
            foreach (var layer in Layers)
            {
                foreach (var clip in layer.Clips)
                {
                    yield return clip;
                }
            }
        }
        
        
        private readonly Dictionary<CurveId, Curve> _animatedInputCurves = new Dictionary<CurveId, Curve>();
        
        public class Layer
        {
            public Guid Id { get; set; }
            public List<Clip> Clips = new List<Clip>();
        }

        public class Clip
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public double StartTime { get; set; }
            public double EndTime { get; set; } = 10;
            public double SourceStartTime { get; set; }
            public double SourceEndTime { get; set; } = 10;
        }        
    }
}