using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PortalsVR;

public class PlayerReferences : MonoBehaviour
{
    public static PlayerReferences Instance;

    public Eye LeftEye;
    public Eye RightEye;

    public Transform LeftAlias;
    public Transform RightAlias;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Destroy(this);
        }
    }
}
