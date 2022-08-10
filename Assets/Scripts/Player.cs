using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

public class Player : MonoBehaviour
{
    #region Variables
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();
    #endregion
    #region Properties
    public ushort Id { get; private set; }
    public string Username { get; private set; }
    #endregion

    public static void Spawn(ushort id, string username)
    {
        //Instantiates PlayerPrefab
        Player player = Instantiate(GameLogic.GameLogicInstance.PlayerPrefab, new Vector3(0, 1, 0), Quaternion.identity).GetComponent<Player>();
        //Sets Player name (creates a default name if username is not filled in)
        player.name = $"Player{id}({(string.IsNullOrEmpty(username)? "Guest" : username)})";
        //Sets Player ID
        player.Id = id;
        //Sets Player Username (creates a default username if username is not filled in)
        player.Username = string.IsNullOrEmpty(username) ? "Guest" : username;
        list.Add(id, player);
    }

    private void OnDestroy()
    {
        list.Remove(Id);
    }

    [MessageHandler((ushort) ClientToServerId.name)]
    private static void Name(ushort fromClientId, Message message)
    {
        Spawn(fromClientId, message.GetString());
    }
}
