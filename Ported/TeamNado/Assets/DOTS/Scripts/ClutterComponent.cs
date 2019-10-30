using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct ClutterComponent : IComponentData
{
    public float3 position;
    public float scale;
    public float colorSaturation;

    internal float3 velocity;

    public float4x4 transform;
}