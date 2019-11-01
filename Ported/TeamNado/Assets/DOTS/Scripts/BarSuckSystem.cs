using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateAfter(typeof(TornadoUpdateSystem))]
public class BarSuckSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var tor = GetSingleton<TornadoComponent>();

        Entities.WithAll<BarComponent, Translation, UnsuckedBarComponent>().ForEach(
            (Entity id, ref BarComponent bar, ref Translation t) =>
            {
                float tdx = (tor.tornadoPos.x - t.Value.x);
                float tdz = (tor.tornadoPos.z - t.Value.z);
                float tornadoDist = Mathf.Sqrt(tdx * tdx + tdz * tdz);
                if (tornadoDist < TornadoConstants.TornadoMaxForceDistance)
                {
                    EntityManager.RemoveComponent<UnsuckedBarComponent>(id);
                    EntityManager.AddComponentData(id, new SuckedBarComponent());

                    //bar.velocity.y = TornadoConstants.TornadoUpForce;
                    bar.oldX = t.Value.x;
                    bar.oldY = t.Value.y;
                    bar.oldZ = t.Value.z;
                }
            });
    }
}