using Logic.Entity;
using UnityEngine;
public interface IPlayableContext
{
    GameObject GetEntityGameObject(uint entityInstId);
    GameObject InstantiateEntityGameObject(Entity e);
    bool RecycleEntityGameObject(uint entityInstId, int entityId);
    GameObject InstantiatePfxGameObject(string name);
    GameObject InstantiatePfxGameObject(string name, string parent);
    bool RecyclePfxGameObject(GameObject obj);
    bool RecyclePfxGameObject(GameObject obj, uint afterMs);
    GameObject InstantiateUiGameObject(string name);
    GameObject InstantiateUiGameObject(string name, string parent);
    bool RecycleUiGameObject(GameObject obj);
}