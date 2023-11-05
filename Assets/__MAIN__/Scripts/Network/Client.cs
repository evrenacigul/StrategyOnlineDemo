using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Utilities;

public class Client : SingletonPunBehaviour<Client>
{
    public bool isConnectedAndReady { get; private set; } = false;

    public bool isJoinedRoom { get; private set; } = false;

    public bool isCreatedRoom { get; private set; } = false;

    public bool isOpponentJoinedRoom { get; private set; } = false;

    public bool cancelCreateOrJoin { get; private set; } = false;

    public bool gameStarted { get; private set; } = false;

    public PhotonPlayer opponent { get; private set; }

    private int roomCount = 0;

    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings("1.0");
    }

    public override void OnConnectedToPhoton()
    {
        Debug.Log("Connected to server.");

        StartCoroutine(JoinLobbyWhenReady());
    }

    public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
    {
        RoomCreateOrJoinFailed(codeAndMsg.ToStringFull());
    }

    public override void OnPhotonCreateRoomFailed(object[] codeAndMsg)
    {
        RoomCreateOrJoinFailed(codeAndMsg.ToStringFull());
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined to lobby.");

        isConnectedAndReady = true;
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room.");

        isJoinedRoom = true;
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Created room.");

        isCreatedRoom = true;
    }

    public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
    {
        RoomCreateOrJoinFailed(codeAndMsg.ToStringFull());
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        Debug.Log("Opponent is joined " + newPlayer.ID);

        opponent = newPlayer;

        isOpponentJoinedRoom = true;
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        CancelJoinOrCreate();
    }

    public override void OnDisconnectedFromPhoton()
    {
        Debug.Log("Disconnected");

        Disconnected();
    }

    public override void OnConnectionFail(DisconnectCause cause)
    {
        Debug.Log("Connection failed " + cause.ToString());

        Disconnected();
    }

    public void CancelJoinOrCreate()
    {
        cancelCreateOrJoin = true;
        roomCount = 0;

        if (PhotonNetwork.connected)
        {
            PhotonNetwork.Disconnect();
            Disconnected();
        }
    }

    internal void Disconnected()
    {
        isConnectedAndReady = false;
        isJoinedRoom = false;
        isCreatedRoom = false;
        cancelCreateOrJoin = false;
        isOpponentJoinedRoom = false;

        GameManager.Instance.SetGameState(GameStates.InLobby);
    }

    internal void RoomCreateOrJoinFailed(string failMsg)
    {
        if (cancelCreateOrJoin) return;

        Debug.Log("Room Create or Join failed " + failMsg);

        roomCount++;

        QuickMatch(true);
    }

    internal void QuickMatch(bool createRoom)
    {
        if (!PhotonNetwork.insideLobby) return;

        if (createRoom)
            PhotonNetwork.JoinOrCreateRoom("room" + roomCount.ToString(), new RoomOptions { MaxPlayers = 2, IsOpen = true, IsVisible = true }, null);
        else
            PhotonNetwork.JoinRandomRoom();
    }

    IEnumerator JoinLobbyWhenReady()
    {
        yield return new WaitUntil(() => PhotonNetwork.connectedAndReady);

        PhotonNetwork.JoinLobby();

        yield return new WaitUntil(() => isConnectedAndReady);

        QuickMatch(false);

        yield return new WaitUntil(() => (isJoinedRoom || isCreatedRoom) && (isOpponentJoinedRoom || PhotonNetwork.room.PlayerCount >= 2));

        gameStarted = true;

        GameManager.Instance.InGameScene();

        yield return null;
    }
}
