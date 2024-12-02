using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct GoInGameRPC : IRpcCommand
{

}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial struct goInGameClientSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp);
        builder.WithAny<NetworkId>();
        builder.WithNone<NetworkStreamInGame>();
        state.RequireForUpdate(state.GetEntityQuery(builder));
    }

    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (id, entity) in SystemAPI.Query<RefRO<NetworkId>>().WithNone<NetworkStreamInGame>().WithEntityAccess())
        {
            commandBuffer.AddComponent<NetworkStreamInGame>(entity);
            var request = commandBuffer.CreateEntity();
            commandBuffer.AddComponent<GoInGameRPC>(request);
            commandBuffer.AddComponent<SendRpcCommandRequest>(request);
        }
        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();
    }
}
