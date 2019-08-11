﻿using SabberStoneCore.Enums;
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
    DEAD,
    DONE
}

public abstract class BasicGen : MonoBehaviour
{
    public AnimationState AnimState { get; set; }

    public void Start()
    {
    }

    void Update()
    {

    }


    public virtual void UpdateEntity(EntityExt entityExt)
    {
        Debug.Log($"UpdateEntity not implemented! {gameObject.GetType()}");
    }

}