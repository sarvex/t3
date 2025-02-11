#include "lib/shared/point.hlsl"

cbuffer Params : register(b0)
{
    float ConnectPointsMode;
    float ApplyTargetOrientation;
    float ApplyTargetScaleW;
    float MultiplyTargetW;
}

StructuredBuffer<Point> SourcePoints : t0;         // input
StructuredBuffer<Point> TargetPoints : t1;         // input
RWStructuredBuffer<Point> ResultPoints : u0;    // output

[numthreads(64,1,1)]
void main(uint3 i : SV_DispatchThreadID)
{

// ResultPoints[i.x] = SourcePoints[i.x];
// return;

    uint resultPointCount, sourcePointCount, targetPointCount, stride;
    
    SourcePoints.GetDimensions(sourcePointCount,stride);
    TargetPoints.GetDimensions(targetPointCount,stride);
    ResultPoints.GetDimensions(resultPointCount,stride);

    if(i.x >= resultPointCount) {
        return;
    }

    if(ConnectPointsMode < 0.5) {
        uint sourceLength = sourcePointCount + 1;

        uint sourceIndex = i.x % (sourceLength);
        uint targetIndex = (i.x / sourceLength )  % targetPointCount;
        
        if(sourceIndex == sourcePointCount) {
            ResultPoints[i.x].position =  0;
            ResultPoints[i.x].w = sqrt(-1);            
        }
        else {
            Point A = SourcePoints[sourceIndex];
            Point B = TargetPoints[targetIndex];
            float s = ApplyTargetScaleW > 0.5 ? B.w : 1;
            float3  pLocal = ApplyTargetOrientation  > 0.5
                            ? rotate_vector(A.position, B.rotation)
                            : A.position;

            ResultPoints[i.x].position = pLocal  * s + B.position;
            ResultPoints[i.x].w = MultiplyTargetW > 0.5 ? A.w * B.w : A.w;
            ResultPoints[i.x].rotation = ApplyTargetOrientation  > 0.5 ? qmul(B.rotation, A.rotation)
                                                                    : A.rotation;
        }
    }
    else {
        uint loopLength = targetPointCount + 1;
        uint sourceIndex = i.x / loopLength;
        uint targetIndex = i.x % loopLength;
        
        if(targetIndex == loopLength - 1) {
            ResultPoints[i.x].position =  0;
            ResultPoints[i.x].w = sqrt(-1);
        }
        else {
            //uint targetIndex = (i.x / sourcePointCount )  % targetPointCount;
            //uint sourceIndex = i.x % loopLength;
        
            Point sourceP = SourcePoints[sourceIndex];
            Point targetP = TargetPoints[targetIndex];

            float s = ApplyTargetScaleW > 0.5 ? targetP.w : 1;
            
            float3  pLocal = ApplyTargetOrientation  > 0.5
                            ? rotate_vector(sourceP.position, targetP.rotation)
                            : sourceP.position;

            ResultPoints[i.x].position = pLocal  * s  + targetP.position;
            ResultPoints[i.x].w = MultiplyTargetW > 0.5 ? sourceP.w * targetP.w : sourceP.w;
            ResultPoints[i.x].rotation = ApplyTargetOrientation  > 0.5 ? qmul(targetP.rotation, sourceP.rotation)
                                                                    : sourceP.rotation;
        }

    }

}
