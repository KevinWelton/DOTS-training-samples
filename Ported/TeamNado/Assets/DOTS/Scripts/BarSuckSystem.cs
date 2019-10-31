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
        Entities.WithAll<BarComponent, TornadoComponent, Translation, UnsuckedBarComponent>().ForEach(
            (Entity id, ref BarComponent bar, ref TornadoComponent tor, ref Translation t) =>
            {
                float tdx = (tor.tornadoPos.x - t.Value.x);
                float tdz = (tor.tornadoPos.z - t.Value.z);
                float tornadoDist = Mathf.Sqrt(tdx * tdx + tdz * tdz);
                if (tornadoDist < TornadoConstants.TornadoMaxForceDistance)
                {
                    EntityManager.RemoveComponent<UnsuckedBarComponent>(id);
                    EntityManager.AddComponentData(id, new SuckedBarComponent());
                }
            });
    }
}