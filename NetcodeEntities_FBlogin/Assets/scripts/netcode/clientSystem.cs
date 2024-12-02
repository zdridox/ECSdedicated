using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct SpawnPlayerRpcCommand : IRpcCommand
{
    public FixedString64Bytes nickname;
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class clientSystem : SystemBase
{
    protected override void OnCreate()
    {
        //game works only if connected to a server
        RequireForUpdate<NetworkId>();
    }
    protected override void OnUpdate()
    {
        //if(Input.GetKeyDown(KeyCode.Space))
       // {
       //     SpawnPlayerRPC(connectionManager.clientWorld);
       // }
    }

    public void SpawnPlayerRPC(World world) { 
        if(world == null || !world.IsCreated)
        {
            return;
        }
        var entity = world.EntityManager.CreateEntity(typeof(SendRpcCommandRequest), typeof(SpawnPlayerRpcCommand));
        world.EntityManager.SetComponentData(entity, new SpawnPlayerRpcCommand()
        {
            nickname = "nick"
        });
    }
}


