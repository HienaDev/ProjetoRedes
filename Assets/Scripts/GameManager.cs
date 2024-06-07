using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{

    private Dictionary<Player, List<string>> players = new Dictionary<Player, List<string>>();

    private List<Bullet> bullets = new List<Bullet>();

    private NetworkManager networkManager;
    public int currentPlayers => networkManager.ConnectedClients.Count;

    private InstantiateGrid grid;

    public static int playerIndex = 0;

    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private float timerTurn;
    private float justTurned;

    [SerializeField] private float timerToStart;
    private float timerRunning;
    private bool gameStarted;

    private NetworkSetup networkSetup;

    private TextMeshProUGUI timerText;


    public int IncreasePlayerIndex()
    {
        playerIndex++;
        return playerIndex - 1;
        
    }



    [SerializeField] private GameObject greenBall;

    // Start is called before the first frame update
    void Start()
    {

        networkManager = FindFirstObjectByType<NetworkManager>();

        grid = FindObjectOfType<InstantiateGrid>();

        justTurned = timerTurn;

        gameStarted = false;

        timerText = GetComponentInChildren<TextMeshProUGUI>();

        networkSetup = FindObjectOfType<NetworkSetup>();

  
    }

    // Update is called once per frame
    void Update()
    {

        int playersAlive = 0;

        foreach (KeyValuePair<Player, List<string>> p in players)
        {
            if (p.Key != null)
                playersAlive++;

        }

        


        

        if (Time.time - justTurned > timerTurn && gameStarted)
        {
            UpdatePlayers();
            justTurned = Time.time;
            timerToStart = timerTurn;
        }

        Debug.Log("players alive: " + playersAlive);

        if (currentPlayers >= 2)
        {
            Color ballColor = greenBall.GetComponent<SpriteRenderer>().color;

            timerToStart -= Time.deltaTime;


            if (timerToStart < 1)
                timerText.color = Color.red;
            else if (timerToStart < 2)
                timerText.color = Color.yellow;
            else if (timerToStart < 3)
                timerText.color = Color.green;
            timerText.text = Math.Round(timerToStart, 1).ToString();

            UpdatePlayersClientRpc(ballColor, timerToStart);

            if (!gameStarted && timerToStart < 0f)
            {
                gameStarted = true;
            }

            if (playersAlive <= 1 && gameStarted)
            {
                ResetGame();
            }
        }


        if (currentPlayers == 3 && !gameStarted)
        {
            Debug.Log("Update grid");
            UpdateGrid(8);
        }
        else if (currentPlayers == 4 && !gameStarted)
        {
            Debug.Log("Update grid");
            UpdateGrid(10);
        }

    }


    private void UpdateGrid(int size)
    {
        //
        InstantiateGrid grid = FindObjectOfType<InstantiateGrid>();

        grid.SetGrid(size);

        UpdateGridClientRpc(size);
    }

    [ClientRpc]
    private void UpdateGridClientRpc(int size)
    {
        InstantiateGrid grid = FindObjectOfType<InstantiateGrid>();

        grid.SetGrid(size);
    }

    private void ResetGame()
    {
        Debug.Log("reset game");


        ResetGameClientRpc();

        

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        

        return;

        //if (networkManager.IsClient) return;

        //ResetGameClientRpc();

        networkSetup.startEarlyInterface.SetActive(false);
        networkSetup.gameInterface.SetActive(false);
        networkSetup.menuInterface.SetActive(true);


        foreach(Bullet b in bullets)
        {
            if(b != null)
            {
                b.GetComponent<NetworkObject>().Despawn();
                Destroy(b.GetComponent<NetworkObject>());
                Destroy(b);

            }
            
        }

        foreach (KeyValuePair<Player, List<string>> p in players)
        {
            if (p.Key != null)
            {
                p.Key.controller.GetComponent<NetworkObject>().Despawn();
                Destroy(p.Key.controller.GetComponent<NetworkObject>());
                Destroy(p.Key.controller);

                p.Key.GetComponent<NetworkObject>().Despawn();
                Destroy(p.Key.GetComponent<NetworkObject>());
                Destroy(p.Key);
            }

        }

        foreach(PlayerController pc in networkSetup.playerControllers)
        {
            if (pc != null)
            {
                pc.GetComponent<NetworkObject>().Despawn();
                Destroy(pc.GetComponent<NetworkObject>());
                Destroy(pc);
            }
        }


        gameObject.GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject.GetComponent<NetworkObject>());
        Destroy(gameObject);

    }

    [ClientRpc]
    private void ResetGameClientRpc()
    {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        return;
        foreach (Bullet b in bullets)
        {
            b.GetComponent<NetworkObject>().Despawn();
            Destroy(b.GetComponent<NetworkObject>());
            Destroy(b);
        }

        foreach (KeyValuePair<Player, List<string>> p in players)
        {
            if (p.Key != null)
            {
                p.Key.controller.GetComponent<NetworkObject>().Despawn();
                Destroy(p.Key.controller.GetComponent<NetworkObject>());
                Destroy(p.Key.controller);

                p.Key.GetComponent<NetworkObject>().Despawn();
                Destroy(p.Key.GetComponent<NetworkObject>());
                Destroy(p.Key);
            }

        }

        foreach (PlayerController pc in networkSetup.playerControllers)
        {
            if (pc != null)
            {
                pc.GetComponent<NetworkObject>().Despawn();
                Destroy(pc.GetComponent<NetworkObject>());
                Destroy(pc);
            }
        }

        networkSetup.startEarlyInterface.SetActive(false);
        networkSetup.gameInterface.SetActive(false);
        networkSetup.menuInterface.SetActive(true);

    }


    private void UpdatePlayers()
    {

        if (networkManager.IsClient) return;

        if (currentPlayers >= 2)
        {

            if (gameStarted)
            {
                greenBall.GetComponent<SpriteRenderer>().color = Color.green;

                foreach (KeyValuePair<Player, List<string>> p in players)
                {
                    if (p.Key != null)
                        MovePlayer(p.Key);

                }

                foreach (Bullet b in bullets)
                {
                    if (b != null)
                        MoveBullet(b);
                }
            }


        }
        else if (currentPlayers == 1)
        {
            greenBall.GetComponent<SpriteRenderer>().color = Color.yellow;
        }
        else if (currentPlayers == 0)
        {
            greenBall.GetComponent<SpriteRenderer>().color = Color.red;
        }

        



        
    }

    [ClientRpc]
    private void UpdatePlayersClientRpc(Color ballColor, float timer)
    {


        greenBall.GetComponent<SpriteRenderer>().color = ballColor;
        if (timer < 1)
            timerText.color = Color.red;
        else if (timer < 2)
            timerText.color = Color.yellow;
        else if (timer < 3)
            timerText.color = Color.green;

        timerText.text = Math.Round(timer, 1).ToString();

    }

    private void MovePlayer(Player player)
    {

        if (players[player].Count > 0)
        {
            Vector3 newPos = GetNewVector(player.transform.position, players[player].Last());

            if (players[player].Last() == "shoot")
            {
                Bullet bullet = Instantiate(bulletPrefab, transform);
                bullets.Add(bullet);
                bullet.direction = players[player][players[player].Count - 2];
                bullet.transform.eulerAngles = GetRotation(bullet.direction);
                bullet.transform.position = player.transform.position;
                

                var netObj = bullet.GetComponent<NetworkObject>();
                netObj.Spawn();
            }   
            else if (CheckIfInGrid(newPos))
                player.transform.position = newPos;

        }

    }

    private void MoveBullet(Bullet bullet)
    {

        Vector3 newPos = GetNewVector(bullet.transform.position, bullet.direction);
        if (CheckIfInGrid(bullet.transform.position))
        {
            bullet.transform.position = newPos;
            CheckForPlayerDeath(bullet);
        }   
        else
        {
            Destroy(bullet.GetComponent<NetworkObject>());
            Destroy(bullet, 0.1f);
        }
            

       
    }

    private void CheckForPlayerDeath(Bullet bullet)
    {
        List<Player> playersToKill = new List<Player>();
        List<Bullet> bulletsToDestroy = new List<Bullet>();

        foreach (KeyValuePair<Player, List<string>> p in players)
        {
            if (p.Key != null)
            {
                if (Vector2.Distance(p.Key.transform.position, bullet.transform.position) < 3)
                {
                    playersToKill.Add(p.Key);
                }

            }
            
        }

        foreach (Bullet b in bullets)
        {
            if(b != bullet && b != null)
            {
                if (Vector2.Distance(b.transform.position, bullet.transform.position) < 3)
                {
                    bulletsToDestroy.Add(b);
                }
            }
        }

        //int playersToKillCount = playersToKill.Count;

        for (int i = 0; i < playersToKill.Count; i++)
        {
            playersToKill[i].GetComponent<NetworkObject>().Despawn();

        }

        if(playersToKill.Count > 0 || bulletsToDestroy.Count > 0)
        {
            bullet.GetComponent<NetworkObject>().Despawn();
            Destroy(bullet.GetComponent<NetworkObject>());
            Destroy(bullet);//, 0.1f);
        }

        foreach (Bullet b in bulletsToDestroy)
        {
            b.GetComponent<NetworkObject>().Despawn();
            Destroy(b.GetComponent<NetworkObject>());
            Destroy(b);//, 0.1f);
        }

        int[] indexOfDeadPlayers = new int[playersToKill.Count];
        for (int i = 0;i < indexOfDeadPlayers.Length;i++)
        {
            indexOfDeadPlayers[i] = Array.IndexOf(players.Keys.ToArray(), playersToKill[i]);
        }


        for (int i = 0; i < indexOfDeadPlayers.Length; i++)
        {
            //Debug.Log("player in " + i + " destroyed");
        }

        //CheckForPlayerDeathClientRpc(indexOfDeadPlayers, bullets.IndexOf(bullet));
    }

    [ClientRpc]
    private void CheckForPlayerDeathClientRpc(int[] playersIndex, int bulletIndex)
    {
        for (int i = 0; i < playersIndex.Length; i++)
        {
            Debug.Log("player in " + i + " destroyed");
            players.ElementAt(i).Key.gameObject.SetActive(false);
        }

        bullets[bulletIndex].gameObject.SetActive(false);
    }



    private Vector3 GetNewVector(Vector3 pos, string dir)
    {
        Vector3 newPos = pos;
        switch (dir)
        {
            case "left":
                newPos += new Vector3(-32, 0, 0f);
                break;
            case "right":
                newPos += new Vector3(32, 0, 0f);
                break;
            case "up":
                newPos += new Vector3(0, 32, 0f);
                break;
            case "down":
                newPos += new Vector3(0, -32, 0f);
                break;
            default:
                break;
        }

        return newPos;
    }

    private Vector3 GetRotation(string dir)
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

    private bool CheckIfInGrid(Vector3 pos) => (pos.x >= (grid.GridSize / 2) * -32 && pos.x <= (grid.GridSize / 2) * 32
                                                 && pos.y >= (grid.GridSize / 2) * -32 && pos.y <= (grid.GridSize / 2) * 32);



    [ServerRpc(RequireOwnership = false)]
    public void SignPlayerServerRpc(int playerIndex)
    {
        players.Add(NetworkSetup.PlayerSprites[playerIndex], new List<string>());
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerMoveServerRPC(int player, string move)
    {
        if(players.ElementAt(player).Value.Count > 0)
        {
            if (!(players.ElementAt(player).Value.Last() == move && move == "shoot"))
                players.ElementAt(player).Value.Add(move);
        }
        else
        {
            if(move != "shoot")
                players.ElementAt(player).Value.Add(move);
        }
        
    }

    public void StartEarly()
    {
        if (!gameStarted)
            timerToStart = 3f;
    }
}
