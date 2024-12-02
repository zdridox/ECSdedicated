using UnityEngine;
using Unity.NetCode;
using UnityEngine.Scripting;

[Preserve]
public class autoConnectBootstrap : ClientServerBootstrap
{

    // prevents unity from making default worlds
    public override bool Initialize(string defaultWorldName)
    {
        AutoConnectPort = 0;
        return false;
    }

}
