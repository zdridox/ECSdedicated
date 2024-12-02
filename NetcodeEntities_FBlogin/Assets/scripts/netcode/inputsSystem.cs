using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;
using UnityEngine;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class inputsSystem : SystemBase
{
    private Controls Controls;


    //setup controls
    protected override void OnCreate()
    {
        Controls = new Controls();
        Controls.Enable();
        var builder = new EntityQueryBuilder(Allocator.Temp);
        builder.WithAny<PlayerInputData>();
        RequireForUpdate(GetEntityQuery(builder));
    }

    protected override void OnDestroy()
    {
        Controls.Disable();
    }


    //get move input
    protected override void OnUpdate()
    {
        Vector2 playerMove = Controls.player.move.ReadValue<Vector2>();
        foreach(RefRW<PlayerInputData> input in SystemAPI.Query<RefRW<PlayerInputData>>().WithAll<GhostOwnerIsLocal>())
        {
            input.ValueRW.move = playerMove;
        }
    }
}

