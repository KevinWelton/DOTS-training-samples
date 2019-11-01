using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal static class TornadoConstants
{
    public const float TornadoMaxForceDistance = 30.0f;
    public const float TornadoUpForce = 1.4f;
    public const float TornadoHeight = 100.0f;
    public const float ClutterHeight = TornadoHeight * .8f;
    public const float Friction = 0.4f;
    public const float Damping = 0.012f;
    public const float InverseDamping = 1 - Damping;
    public const float TornadoForce = 0.022f;
    public const float TornadoInwardForce = 14.0f;

    //public const float BreakResistance = 0.55f;
}
