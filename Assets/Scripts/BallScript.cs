using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallScript : MonoBehaviour
{
    [SerializeField] private float gravity = 9.8f;
    private float verticalVelocity = 0f;
    private Vector3 move;
    private bool grounded;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        move = Vector3.zero;

        verticalVelocity -= gravity * Time.deltaTime;

        // Move the player
        move = new Vector3(0, verticalVelocity * Time.deltaTime, 0f);
        transform.position += move;
    }

    public void GiveSpeed(Vector3 speed)
    {
        move += speed;
    }
}
