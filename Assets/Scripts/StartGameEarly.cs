using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StartGameEarly : MonoBehaviour
{


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        GameManager gm = FindObjectOfType<GameManager>();

        if (gm != null)
        {
            gm.StartEarly();
        }

        StartGameClientRpc();
    }

    [ClientRpc]
    private void StartGameClientRpc()
    {
        GameManager gm = FindObjectOfType<GameManager>();

        if (gm != null)
        {
            gm.StartEarly();
        }
    }
}
