using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct ClutterComponent : IComponentData
{
    public Vector3 position;
    public float scale;
    public float colorSaturation;

    internal Vector3 velocity;

    public Matrix4x4 transform;
}