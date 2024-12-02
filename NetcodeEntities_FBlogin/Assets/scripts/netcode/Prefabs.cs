using Unity.Entities;
using UnityEngine;

public class Prefabs : MonoBehaviour
{
    public GameObject playerPrefab = null;
}

public struct PrefabsData : IComponentData
{
    public Entity prefab;
}


// turns prefabs to entities
public class PrefabsBaker : Baker<Prefabs>
{
    public override void Bake(Prefabs authoring)
    {
        if(authoring.playerPrefab !=null)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new PrefabsData()
            {
                prefab = GetEntity(authoring.playerPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}
