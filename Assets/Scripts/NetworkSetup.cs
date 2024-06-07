using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System;
using Debug = UnityEngine.Debug;
using System.IO;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor.Build.Reporting;
using UnityEditor;
using System.Runtime.InteropServices;
#endif

#if UNITY_STANDALONE_WIN
using System.Diagnostics;
using System.Runtime.InteropServices;
using TMPro;
using Unity.Services.Relay.Models;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine.UI;
using UnityEngine.XR;
#endif



public class NetworkSetup : MonoBehaviour
{

    public class RelayHostData
    {
        public string JoinCode;
        public string IPv4Address;
        public ushort Port;
        public Guid AllocationID;
        public byte[] AllocationIDBytes;
        public byte[] ConnectionData;
        public byte[] HostConnectionData;
        public byte[] Key;
    }

    [SerializeField] private bool forceServer = false;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerController[] playerController;
    [SerializeField] private Player[] playerPrefabs;
    [SerializeField] private Transform[] playerSpawnLocations;
    [SerializeField] private string joinCode;
    [SerializeField] private TextMeshProUGUI textJoinCode;
    [SerializeField] private int maxPlayers = 4;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] public GameObject gameInterface;
    [SerializeField] public GameObject loadingInterface;
    [SerializeField] public GameObject menuInterface;
    [SerializeField] public GameObject startEarlyInterface;

    public static List<Player> PlayerSprites { get; private set; }
    int playerPrefabIndex = 0;
    int playerControllerIndex = 0;

    public List<PlayerController> playerControllers = new List<PlayerController>();

    private bool isServer = false;

    private bool isRelay;
    private UnityTransport transport;
    private RelayHostData relayData;

    const int SM_CXSCREEN = 0;
    const int SM_CYSCREEN = 1;

    const uint SWP_NOSIZE = 0x0001;
    const uint SWP_NOZORDER = 0x0004;
    const uint SWP_SHOWWINDOW = 0x0040;



    private void Start()
    {


        gameObject.GetComponent<NetworkManager>().enabled = false;
        gameObject.GetComponent<UnityTransport>().enabled = false;

        SetWindowSize();
    }


    public void StartServer()
    {
        PlayerSprites = new List<Player>();
а а а а // Parse command line arguments

        isServer = true;


        transport = GetComponent<UnityTransport>();
        if (transport.Protocol == UnityTransport.ProtocolType.RelayUnityTransport)
        {
            isRelay = true;
            Debug.Log("Relay true");
        }
        else
        {
            textJoinCode.gameObject.SetActive(false);
        }

        menuInterface.SetActive(false);
        loadingInterface.SetActive(true);

        StartCoroutine(StartAsServerCR());
    }

    public void StartPlayer()
    {
        PlayerSprites = new List<Player>();
        // Parse command line arguments

        joinCode = inputField.text;

        transport = GetComponent<UnityTransport>();
        if (transport.Protocol == UnityTransport.ProtocolType.RelayUnityTransport)
        {
            isRelay = true;
            Debug.Log("Relay true");
        }
        else
        {
            textJoinCode.gameObject.SetActive(false);
        }


        menuInterface.SetActive(false);
        loadingInterface.SetActive(true);

        StartCoroutine(StartAsClientCR());

    }

    IEnumerator StartAsServerCR()
    {
        Debug.Log("start server");
        SetWindowTitle("TurnShooter - Server");
        var networkManager = GetComponent<NetworkManager>();
        networkManager.enabled = true;
        transport.enabled = true;


        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;

        // Wait a frame for setups to be done
        yield return null;

        if (isRelay)
        {
            // Vou chamar uma funчуo que щ async (ver abaixo)
            // Isso devolve um Task 
            var loginTask = Login();

            // Fico р espera que a Task acabe, verificando o IsComplete
            yield return new WaitUntil(() => loginTask.IsCompleted);

            // Verifico se houve um exception na tarefa. Esta foi 
            // executada numa tarefa distinta, por isso nуo щ propagada, e щ normalmente
            // como se propaga erros
            if (loginTask.Exception != null)
            {
                Debug.LogError("Login failed: " + loginTask.Exception);
                yield break;
            }

            // Tarefa foi concluэda, podiamos agora ir buscar o resultado (jс vamos ver)
            Debug.Log("Login successfull!");

            var allocationTask = CreateAllocationAsync(maxPlayers);

            yield return new WaitUntil(() => allocationTask.IsCompleted);

            if (allocationTask.Exception != null)
            {
                Debug.LogError("Allocation failed: " + allocationTask.Exception);
                yield break;
            }
            else
            {
                Debug.Log("Allocation successfull!");

                Allocation allocation = allocationTask.Result;

                relayData = new RelayHostData();

                // Find the appropriate endpoint, just select the first one and use it
                foreach (var endpoint in allocation.ServerEndpoints)
                {
                    relayData.IPv4Address = endpoint.Host;
                    relayData.Port = (ushort)endpoint.Port;
                    break;
                }

                relayData.AllocationID = allocation.AllocationId;
                relayData.AllocationIDBytes = allocation.AllocationIdBytes;
                relayData.ConnectionData = allocation.ConnectionData;
                relayData.Key = allocation.Key;

                var joinCodeTask = GetJoinCodeAsync(relayData.AllocationID);

                yield return new WaitUntil(() => joinCodeTask.IsCompleted);

                if (joinCodeTask.Exception != null)
                {
                    Debug.LogError("Join code failed: " + joinCodeTask.Exception);
                    yield break;
                }
                else
                {
                    Debug.Log("Code retrieved!");

                    relayData.JoinCode = joinCodeTask.Result;

                    if (textJoinCode != null)
                    {
                        textJoinCode.text = $"JoinCode:\n{relayData.JoinCode}";
                        textJoinCode.gameObject.SetActive(true);
                    }

                    transport.SetRelayServerData(relayData.IPv4Address, relayData.Port, relayData.AllocationIDBytes,
                                                 relayData.Key, relayData.ConnectionData);
                }
            }
        }

        if (networkManager.StartServer())
        {
            Debug.Log($"Serving on port {transport.ConnectionData.Port}...");
            // Spawn game manager object
            var spawnedObject = Instantiate(gameManager, Vector3.zero, Quaternion.identity);
            var prefabNetworkObject = spawnedObject.GetComponent<NetworkObject>();
            prefabNetworkObject.Spawn();

            startEarlyInterface.SetActive(true);
            gameInterface.SetActive(true);
            loadingInterface.SetActive(false);

        }
        else
        {
            Debug.LogError($"Failed to serve on port {transport.ConnectionData.Port}...");
        }
    }

    private async Task<Allocation> CreateAllocationAsync(int maxPlayers)
    {
        try
        {
            // This requests space for maxPlayers + 1 connections (the +1 is for the server itself)
            Allocation allocation = await Unity.Services.Relay.RelayService.Instance.CreateAllocationAsync(maxPlayers);
            return allocation;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error creating allocation: " + e);
            throw;
        }
    }

    private async Task Login()
    {
        try
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error login: " + e);
            throw;
        }
    }

    private async Task<string> GetJoinCodeAsync(Guid allocationID)
    {
        try
        {
            string code = await Unity.Services.Relay.RelayService.Instance.GetJoinCodeAsync(allocationID);

            return code;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error retrieving join code: " + e);
            throw;
        }
    }


    private void OnClientConnected(ulong clientId)
    {
        //Debug.Log($"Player {clientId} connected, prefab index = {playerPrefabIndex}!");
        // Check a free spot for this player
        var spawnPos = Vector3.zero;
        var currentPlayers = FindObjectsOfType<Player>();
        foreach (var playerSpawnLocation in playerSpawnLocations)
        {
            var closestDist = float.MaxValue;
            foreach (var player in currentPlayers)
            {
                float d = Vector3.Distance(player.transform.position, playerSpawnLocation.position);
                closestDist = Mathf.Min(closestDist, d);
            }
            if (closestDist > 20)
            {
                spawnPos = playerSpawnLocation.position;
                break;
            }
        }
        // Spawn player object
        var spawnedObject = Instantiate(playerPrefabs[playerPrefabIndex], spawnPos, Quaternion.identity);
        var prefabNetworkObject = spawnedObject.GetComponent<NetworkObject>();
        prefabNetworkObject.Spawn();//AsPlayerObject(clientId, true);
        //prefabNetworkObject.ChangeOwnership(clientId);
        playerPrefabIndex = (playerPrefabIndex + 1) % playerPrefabs.Length;

        PlayerSprites.Add(spawnedObject);



        // Spawn player controller
        var spawnedController = Instantiate(playerController[playerControllerIndex], spawnPos, Quaternion.identity);
        playerControllers.Add(spawnedController);
        var prefabNetworkController = spawnedController.GetComponent<NetworkObject>();
        prefabNetworkController.SpawnAsPlayerObject(clientId, true);
        prefabNetworkController.ChangeOwnership(clientId);
        playerControllerIndex = (playerControllerIndex + 1) % playerController.Length;


    }
    private void OnClientDisconnect(ulong clientId)
    {
        Debug.Log($"Player {clientId} disconnected!");
    }





    IEnumerator StartAsClientCR()
    {
        SetWindowTitle("TurnShooter - Client");
        var networkManager = GetComponent<NetworkManager>();
        networkManager.enabled = true;
        transport.enabled = true;

        // Wait a frame for setups to be done
        yield return null;

        if (isRelay)
        {
            var loginTask = Login();

            yield return new WaitUntil(() => loginTask.IsCompleted);

            if (loginTask.Exception != null)
            {
                Debug.LogError("Login failed: " + loginTask.Exception);
                yield break;
            }

            Debug.Log("Login successfull!");

            //Ask Unity Services for allocation data based on a join code
            var joinAllocationTask = JoinAllocationAsync(joinCode);

            yield return new WaitUntil(() => joinAllocationTask.IsCompleted);

            if (joinAllocationTask.Exception != null)
            {
                Debug.LogError("Join allocation failed: " + joinAllocationTask.Exception);
                menuInterface.SetActive(true);
                loadingInterface.SetActive(false);
                yield break;
            }
            else
            {
                Debug.Log("Allocation joined!");

                relayData = new RelayHostData();

                var allocation = joinAllocationTask.Result;

                // Find the appropriate endpoint, just select the first one and use it
                foreach (var endpoint in allocation.ServerEndpoints)
                {
                    relayData.IPv4Address = endpoint.Host;
                    relayData.Port = (ushort)endpoint.Port;
                    break;
                }

                relayData.AllocationID = allocation.AllocationId;
                relayData.AllocationIDBytes = allocation.AllocationIdBytes;
                relayData.ConnectionData = allocation.ConnectionData;
                relayData.HostConnectionData = allocation.HostConnectionData;
                relayData.Key = allocation.Key;

                transport.SetRelayServerData(relayData.IPv4Address, relayData.Port,
                                             relayData.AllocationIDBytes, relayData.Key, relayData.ConnectionData,
                                             relayData.HostConnectionData);


                if (textJoinCode != null)
                {
                    textJoinCode.text = $"JoinCode:\n{joinCode}";
                    textJoinCode.gameObject.SetActive(true);
                }
            }
        }

        if (networkManager.StartClient())
        {
            Debug.Log($"Connecting on port {transport.ConnectionData.Port}...");
            gameInterface.SetActive(true);
            loadingInterface.SetActive(false);


        }
        else
        {
            Debug.LogError($"Failed to connect on port {transport.ConnectionData.Port}...");
        }

        yield return null;

        
    }


    private async Task<JoinAllocation> JoinAllocationAsync(string joinCode)
    {
        try
        {
            // This requests space for maxPlayers + 1 connections (the +1 is for the server itself)
            var allocation = await Unity.Services.Relay.RelayService.Instance.JoinAllocationAsync(joinCode);

            return allocation;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error joining allocation: " + e);
            throw;
        }
    }


#if UNITY_STANDALONE_WIN
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowText(IntPtr hWnd, string lpString);
    [DllImport("user32.dll", SetLastError = true)]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    [DllImport("user32.dll")]
    static extern IntPtr EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    // Delegate to filter windows
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    private static IntPtr FindWindowByProcessId(uint processId)
    {
        IntPtr windowHandle = IntPtr.Zero;
        EnumWindows((hWnd, lParam) =>
        {
            uint windowProcessId;
            GetWindowThreadProcessId(hWnd, out windowProcessId);
            if (windowProcessId == processId)
            {
                windowHandle = hWnd;
                return false; // Found the window, stop enumerating
а а а а а а }
            return true; // Continue enumerating
а а а а }, IntPtr.Zero);
        return windowHandle;
    }

    static void SetWindowSize()
    {
#if !UNITY_EDITOR
        uint processId = (uint)Process.GetCurrentProcess().Id;
        IntPtr hWnd = FindWindowByProcessId(processId);

        // Generate random position
        System.Random random = new System.Random();
        int randomX = random.Next(0, 1000);
        int randomY = random.Next(0, 400);

        // Move the window to the random position
        if (SetWindowPos(hWnd, IntPtr.Zero, randomX, randomY, 800, 450, SWP_NOZORDER | SWP_SHOWWINDOW))
        {
            Console.WriteLine($"Window moved to position ({randomX}, {randomY})");
        }
        else
        {
            Console.WriteLine("Failed to move the window.");
        }
#endif
    }

    static void SetWindowTitle(string title)
    {
#if !UNITY_EDITOR
а а а а uint processId = (uint)Process.GetCurrentProcess().Id;
а а а а IntPtr hWnd = FindWindowByProcessId(processId);
а а а а if (hWnd != IntPtr.Zero)
а а а а {
а а а а а а SetWindowText(hWnd, title);
а а а а }


        
#endif
    }
#else
        static void SetWindowTitle(string title)
а а     {
а а     }
#endif


#if UNITY_EDITOR
    [MenuItem("Tools/Build Windows (x64)", priority = 0)]
    public static bool BuildGame()
    {
а а а а // Specify build options
а а а а BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = EditorBuildSettings.scenes
          .Where(s => s.enabled)
          .Select(s => s.path)
          .ToArray();
        buildPlayerOptions.locationPathName = Path.Combine("Build", "TurnShooter.exe");
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.None;
        // Perform the build
        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        // Output the result of the build
        Debug.Log($"Build ended with status: {report.summary.result}");
        // Additional log on the build, looking at report.summary
        return report.summary.result == BuildResult.Succeeded;
    }

    [MenuItem("Tools/Build and Launch (Server)", priority = 10)]
    public static void BuildAndLaunch1()
    {
        CloseAll();
        if (BuildGame())
        {
            Launch1();
        }
    }
    [MenuItem("Tools/Build and Launch (Server + Client) _F11", priority = 20)]
    public static void BuildAndLaunch2()
    {
        CloseAll();
        if (BuildGame())
        {
            LaunchALL();
        }
    }
    [MenuItem("Tools/Launch (Server)", priority = 30)]
    public static void Launch1()
    {
        Run("Build\\TurnShooter.exe", "--server");
    }
    [MenuItem("Tools/Launch (Server + 2 Clients) _F12", priority = 40)]
    public static void Launch2()
    {
        CloseAll();
        if (BuildGame())
        {
            Run("Build\\TurnShooter.exe", "--server");
            Run("Build\\TurnShooter.exe", "");
            Run("Build\\TurnShooter.exe", "");
        }

    }

    [MenuItem("Tools/Launch (Server + 4 Clients) _F8", priority = 40)]
    public static void LaunchALL()
    {
        Run("Build\\TurnShooter.exe", "--server");
        Run("Build\\TurnShooter.exe", "");
        Run("Build\\TurnShooter.exe", "");
        Run("Build\\TurnShooter.exe", "");
        Run("Build\\TurnShooter.exe", "");
    }

    [MenuItem("Tools/Launch 2 Client _F9", priority = 50)]
    public static void Launch1Client()
    {
        Run("Build\\TurnShooter.exe", "");
        Run("Build\\TurnShooter.exe", "");
    }

    [MenuItem("Tools/Close All _F10", priority = 100)]
    public static void CloseAll()
    {
        // Get all processes with the specified name
        Process[] processes = Process.GetProcessesByName("TurnShooter");
        foreach (var process in processes)
        {
            try
            {
                // Close the process
                process.Kill();
                // Wait for the process to exit
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                // Handle exceptions, if any
                // This could occur if the process has already exited or you don't have permission to kill it
                Debug.LogWarning($"Error trying to kill process {process.ProcessName}: {ex.Message}");
            }
        }
    }


    private static void Run(string path, string args)
    {
        // Start a new process
        Process process = new Process();
        // Configure the process using the StartInfo properties
        process.StartInfo.FileName = path;
        process.StartInfo.Arguments = args;
        process.StartInfo.WindowStyle = ProcessWindowStyle.Normal; // Choose the window style: Hidden, Minimized, Maximized, Normal
        process.StartInfo.RedirectStandardOutput = false; // Set to true to redirect the output (so you can read it in Unity)
        process.StartInfo.UseShellExecute = true; // Set to false if you want to redirect the output
                                                  // Run the process
        process.Start();
    }

#endif

}


