using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerController controller;

    private void OnDestroy()
    {
        controller.DeadPlayer();
    }

}
