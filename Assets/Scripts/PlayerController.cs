using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{

    [SerializeField] private KeyCode up = KeyCode.W;
    [SerializeField] private KeyCode left = KeyCode.A;
    [SerializeField] private KeyCode right = KeyCode.D;
    [SerializeField] private KeyCode down = KeyCode.S;
    [SerializeField] private KeyCode shoot = KeyCode.Space;

    private GameManager gameManager;

    private Player[] players;
    private Player player;
    private int playerIndex;

    private NetworkManager networkManager;

    private NetworkObject networkObject;

    [SerializeField] private GameObject turnUI;
    [SerializeField] private GameObject playerSpriteUI;
    [SerializeField] private Sprite turnImage;
    [SerializeField] private Sprite bulletImage;
    [SerializeField] private GameObject deadPlayerUI;

    private string prevMove;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        networkManager = FindFirstObjectByType<NetworkManager>();

        networkObject = GetComponent<NetworkObject>();

        playerIndex = gameManager.IncreasePlayerIndex();

        players = FindObjectsOfType<Player>();

        float distance = Mathf.Infinity;

        foreach(Player p in players)
        {
            if(distance > Vector2.Distance(p.transform.position, this.transform.position))
            {
                player = p;
                player.controller = this;
                distance = Vector2.Distance(p.transform.position, this.transform.position);
            }
        }

        if (networkObject.IsLocalPlayer && !IsServer)
        {
            playerSpriteUI.GetComponent<Image>().sprite = player.GetComponent<SpriteRenderer>().sprite;
            playerSpriteUI.GetComponent<Image>().color = Color.white;
        }
            

        SetPlayer();
    }

    public void SetPlayer()
    {
        gameManager.SignPlayerServerRpc(playerIndex);
    }

    // Update is called once per frame
    void Update()
    {
        if(networkObject.IsLocalPlayer)
        {
            if (Input.GetKeyDown(up))
            {
                gameManager.PlayerMoveServerRPC(playerIndex, "up");
                prevMove = "up";
                turnUI.transform.eulerAngles = GetRotationVector("up");
                turnUI.GetComponent<Image>().sprite = turnImage;
                turnUI.GetComponent<Image>().color = Color.red;
            }
            if (Input.GetKeyDown(left))
            {
                gameManager.PlayerMoveServerRPC(playerIndex, "left");
                prevMove = "left";
                turnUI.transform.eulerAngles = GetRotationVector("left");
                turnUI.GetComponent<Image>().sprite = turnImage;
                turnUI.GetComponent<Image>().color = Color.yellow;
            }
            if (Input.GetKeyDown(right))
            {
                gameManager.PlayerMoveServerRPC(playerIndex, "right");
                prevMove = "right";
                turnUI.transform.eulerAngles = GetRotationVector("right");
                turnUI.GetComponent<Image>().sprite = turnImage;
                turnUI.GetComponent<Image>().color = Color.cyan;
            }
            if (Input.GetKeyDown(down))
            {
                gameManager.PlayerMoveServerRPC(playerIndex, "down");
                prevMove = "down";
                turnUI.transform.eulerAngles = GetRotationVector("down");
                turnUI.GetComponent<Image>().sprite = turnImage;
                turnUI.GetComponent<Image>().color = Color.green;
            }
            if(Input.GetKeyDown(shoot))
            {
                gameManager.PlayerMoveServerRPC(playerIndex, "shoot");
                turnUI.transform.eulerAngles = GetRotationVector(prevMove);
                turnUI.GetComponent<Image>().sprite = bulletImage;
            }
        }
        else
        {
            turnUI.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
        }
        
    }

    private Vector3 GetRotationVector(string dir)
    {
        Vector3 newPos = Vector3.zero;
        switch (dir)
        {
            case "left":
                newPos += new Vector3(0, 0, 180f);
                break;
            case "right":
                newPos += new Vector3(0, 0, 0f);
                break;
            case "up":
                newPos += new Vector3(0, 0, 90f);
                break;
            case "down":
                newPos += new Vector3(0, 0, 270f);
                break;
            default:
                break;
        }

        return newPos;
    }
    

    

    public void DeadPlayer()
    {
        if (networkObject.IsLocalPlayer)
        {
            if(deadPlayerUI != null)
                deadPlayerUI.SetActive(true);
        }
            
    }

}
