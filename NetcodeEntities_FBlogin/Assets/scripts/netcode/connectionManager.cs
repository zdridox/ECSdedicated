using System.Collections;
using System.Net;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using Unity.Scenes;
using UnityEngine;

public class connectionManager : MonoBehaviour
{

    private string listenIP = "127.0.0.1"; // localhost
    private string connectIP = "127.0.0.1"; // localhost
    private ushort port = 1234;
    public static World serverWorld = null;
    public static World clientWorld = null;

    public enum Role
    {
        serverAndClient = 0,
        Server = 1,
        Client = 2
    }

    private static Role role = Role.Client;

    private void Start()
    {
        // set role
        if(Application.isEditor)
        {
            role = Role.serverAndClient;
        }
        else
        {
            if(Application.platform == RuntimePlatform.LinuxServer ||  Application.platform == RuntimePlatform.WindowsServer || Application.platform == RuntimePlatform.OSXServer)
            {
                role = Role.Server;
            }
        }


        StartCoroutine(Connect());
    }

    private IEnumerator Connect()
    {

        // create client/server worlds
        if(role == Role.Server || role == Role.serverAndClient)
        {
            serverWorld = ClientServerBootstrap.CreateServerWorld("serverWorld");
        }

        if (role == Role.Client || role == Role.serverAndClient)
        {
            clientWorld = ClientServerBootstrap.CreateClientWorld("clientWorld");
        }

        // remove default worlds
        foreach(var world in World.All)
        {
            if(world.Flags == WorldFlags.Game)
            {
                world.Dispose();
                break;
            }
        }

        if(serverWorld != null)
        {
            World.DefaultGameObjectInjectionWorld = serverWorld;
        } else
        {
            if(clientWorld != null)
            {
                World.DefaultGameObjectInjectionWorld = clientWorld;
            }
        }

        SubScene[] subScenes = FindObjectsByType<SubScene>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        if(serverWorld != null)
        {
            while(!serverWorld.IsCreated)
            {
                yield return null;
            }
            // load subscenes
            if(subScenes != null)
            {
                for(int i = 0; i < subScenes.Length; i++)
                {
                    SceneSystem.LoadParameters loadParameters = new SceneSystem.LoadParameters() { Flags = SceneLoadFlags.BlockOnStreamIn };
                    var sceneEntity = SceneSystem.LoadSceneAsync(serverWorld.Unmanaged, new Unity.Entities.Hash128(subScenes[i].SceneGUID.Value), loadParameters);
                    while(!SceneSystem.IsSceneLoaded(serverWorld.Unmanaged, sceneEntity))
                    {
                        serverWorld.Update();
                    }
                }
            }

            // listen for clients
            using var query = serverWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            query.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(NetworkEndpoint.Parse(listenIP, port));
        }

        if(clientWorld != null)
        {
            while (!clientWorld.IsCreated)
            {
                yield return null;
            }

            if (subScenes != null)
            {
                for (int i = 0; i < subScenes.Length; i++)
                {
                    SceneSystem.LoadParameters loadParameters = new SceneSystem.LoadParameters() { Flags = SceneLoadFlags.BlockOnStreamIn };
                    var sceneEntity = SceneSystem.LoadSceneAsync(clientWorld.Unmanaged, new Unity.Entities.Hash128(subScenes[i].SceneGUID.Value), loadParameters);
                    while (!SceneSystem.IsSceneLoaded(clientWorld.Unmanaged, sceneEntity))
                    {
                        clientWorld.Update();
                    }
                }
            }
            //connection with server
            using var query = clientWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            query.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(clientWorld.EntityManager, NetworkEndpoint.Parse(connectIP, port));
        }

    }

}
