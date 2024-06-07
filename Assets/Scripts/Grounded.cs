using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grounded : MonoBehaviour
{

    private Player player;
    private BallScript ball;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponentInParent<Player>();
        ball = GetComponentInParent<BallScript>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {




    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //player.SetGrounded(false);
    }
}
