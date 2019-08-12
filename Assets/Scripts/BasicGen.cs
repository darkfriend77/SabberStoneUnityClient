using SabberStoneCore.Enums;
using SabberStoneCore.Model;
using SabberStoneCore.Model.Entities;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public enum AnimationState
{
    NONE,
    ATTACK,
    DESTROY,
    HEALTHCHANGE,
    DEAD,
    DONE
}

public abstract class BasicGen : MonoBehaviour
{
    public AnimationState AnimState { get; set; }

    internal EntityExt _entityExt;

    public virtual void UpdateEntity(EntityExt entityExt)
    {
        _entityExt = entityExt;
    }

    public int Tag(GameTag gameTag) => _entityExt.Tags[gameTag];

}
