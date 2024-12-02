using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using Unity.Collections;
using Unity.Transforms;

public struct initializedClient : IComponentData
{

}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class serverSystem : SystemBase
{
    private ComponentLookup<NetworkId> Clients;

    protected override void OnCreate()
    {
        Clients = GetComponentLookup<NetworkId>(true);
    }

    protected override void OnUpdate()
    {
        Clients.Update(this);
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach(var(request, command, entity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<SpawnPlayerRpcCommand>>().WithEntityAccess())
        {
            PrefabsData prefabs;
            if(SystemAPI.TryGetSingleton<PrefabsData>(out prefabs) && prefabs.prefab != null)
            {
                Entity player = commandBuffer.Instantiate(prefabs.prefab);
                Debug.Log("spawned player with nick: " + command.ValueRO.nickname);
                var networkID = Clients[request.ValueRO.SourceConnection];
                commandBuffer.SetComponent(player, new GhostOwner()
                {
                    NetworkId = networkID.Value
                });

                //destroy player if disconnected
                commandBuffer.AppendToBuffer(request.ValueRO.SourceConnection, new LinkedEntityGroup()
                {
                    Value = player
                });

            }
            commandBuffer.DestroyEntity(entity);
        }

        foreach(var (id, entity) in SystemAPI.Query<RefRO<NetworkId>>().WithNone<initializedClient>().WithEntityAccess())
        {
            commandBuffer.AddComponent<initializedClient>(entity);
            Debug.Log("client connected with id: " + id.ValueRO.Value);
            PrefabsData prefabManager = SystemAPI.GetSingleton<PrefabsData>();
            if(prefabManager.prefab != null)
            {
                Entity player = commandBuffer.Instantiate(prefabManager.prefab);

                commandBuffer.SetComponent(player, new GhostOwner()
                {
                    NetworkId = id.ValueRO.Value
                });
            }

        }
        commandBuffer.Playback(EntityManager);
        commandBuffer.Dispose();
    }

}
