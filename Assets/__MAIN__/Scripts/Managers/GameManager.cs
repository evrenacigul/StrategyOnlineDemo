using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Utilities;
using Controllers;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    Client client;
    UI_Controller ui_controller;

    public UnityEvent<GameStates> OnGameStateChanged;
    public GameStates gameState { get; private set; }

    void Start()
    {
        if (client == null)
            client = Client.Instance;

        if(ui_controller == null)
            ui_controller = UI_Controller.Instance;

        SetGameState(GameStates.InLobby);
    }

    public void SetGameState(GameStates gameState)
    {
        this.gameState = gameState;

        switch(gameState) 
        {
            case GameStates.InLobby:
                LobbyScene();
                break;
            case GameStates.InGame:
                InGameScene();
                break;
        }

        OnGameStateChanged?.Invoke(gameState);
    }

    internal void LobbyScene()
    {
        SceneManager.LoadScene("Lobby");
        ui_controller.InLobby();
    }

    internal void InGameScene()
    {
        if (!client.gameStarted)
        {
            SetGameState(GameStates.InLobby);
            return;
        }

        SceneManager.LoadScene("InGame");
        ui_controller.InGame();
    }
}

public enum GameStates
{
    InLobby,
    InGame
}
