using RiptideNetworking;
using RiptideNetworking.Utils;
using UnityEngine;

public enum ClientToServerId
{
    name = 1,
}

public class NetworkManager : MonoBehaviour
{
    /*
    We want to make sure there is only one instance of this
    We are creating a private static instance of our NetworkManager
    and a public static Property to control the instance.
     */
    #region Variables
    private static NetworkManager _networkManagerInstance;
    [SerializeField] private ushort s_port;
    [SerializeField] private ushort s_maxClientCount;
    #endregion
    #region Properties
    public NetworkManager NetworkManagerInstance
    {
        //Property Read is public by default and readys the instance
        get => _networkManagerInstance;
        private set
        {
            //Property private write sets the instance to the value if the instance is null
            if (_networkManagerInstance == null)
            {
                _networkManagerInstance = value;
            }
            //Property checks for already existing NetworkManagers and if the instance doesn't match it destroys it :)
            else if (_networkManagerInstance != value)
            {
                Debug.LogWarning($"{nameof(NetworkManager)} instance already exists, destroy duplicate!");
                Destroy(value);
            }
        }
    }
    public Server GameServer { get; private set; }
    #endregion

    private void Awake()
    {
        //When the object that this script is attached to is activated in the game, set the instance to this and check to see if instance is already set
        NetworkManagerInstance = this;
    }

    private void Start()
    {
        //Logs what the network is doing
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);
        //Create new server
        GameServer = new Server();
        //Starts the Server at port XXXX with X amount of clients
        GameServer.Start(s_port, s_maxClientCount);
        //When a client leaves the server run the PlayerLeft function
        GameServer.ClientDisconnected += PlayerLeft;
    }

    //Checking server activity at set intervals
    private void FixedUpdate()
    {
        GameServer.Tick();
    }

    //When the game Closes it kills the connection to the server
    private void OnApplicationQuit()
    {
        GameServer.Stop();
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        //When a player leaves the server Destroy the player object
        Destroy(Player.list[e.Id].gameObject);
    }
}
