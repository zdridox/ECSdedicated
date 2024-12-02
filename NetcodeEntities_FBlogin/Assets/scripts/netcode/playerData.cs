using Unity.Entities;
using UnityEngine;

public class playerData : MonoBehaviour
{
    public float speed = 5f;
}

public struct PlayerDataStruct : IComponentData
{
    public float speed;
}

public class PlayerBaker : Baker<playerData>
{
    public override void Bake(playerData authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new PlayerDataStruct
        {
            speed = authoring.speed
        });
        AddComponent<PlayerInputData>(entity);
    }
}
