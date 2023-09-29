using Assets.Scripts.DTO;
using ChooseMemeServer.DTO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LobbyMenu : MonoBehaviour
{

    [SerializeField]
    private GameObject GamePrefab;

    [SerializeField]
    private GameObject LobbiesMenuPrefab;

    [SerializeField]
    private GameObject ClientInfoRowPrefab;

    [SerializeField]
    private GameObject ErrorMessagePrefab;

    [SerializeField]
    private GameObject MainMenuPrefab;

    [SerializeField]
    private TMP_Text lobbyNameText;

    [SerializeField]
    private TMP_Text numOfPlayersText;

    [SerializeField]
    private GameObject context;

    [SerializeField]
    private GameObject startButton;

    private List<ClientInfoRow> clientInfoRows;

    private int maxNumOfClients;

    public void Start()
    {
        //maxNumOfClients = 0;
        Multiplayer.Instance.ClientConnectedToLobbyEvent += AddClient;
        Multiplayer.Instance.ClientDisconnectedFromLobbyEvent += RemoveClient;
        Multiplayer.Instance.ServerIsNotAvaliableEvent += BackToMainMenuWithError;
        Multiplayer.Instance.StartingGameEvent += StartingGame;
        Multiplayer.Instance.SetNewHostEvent += SetNewHost;
    }

    public void SetLobbyInfo(LobbyDTO lobbyDTO)
    {
        clientInfoRows = new List<ClientInfoRow>();

        List<ClientDTO> clientList = new List<ClientDTO>(lobbyDTO.clientsDTO.clientsDTO);
        lobbyNameText.text = lobbyDTO.name;
        maxNumOfClients = lobbyDTO.maxNumOfClients;
        numOfPlayersText.text = $"{clientList.Count}/{maxNumOfClients}";

        if (!lobbyDTO.isHost)
        {
            startButton.SetActive(false);
        }

        foreach(ClientDTO c in lobbyDTO.clientsDTO.clientsDTO)
        {
            AddClient(c);
        }
    }

    public void StartGame()
    {
        Multiplayer.Instance.StartGame();
    }

    public void StartingGame(StartGameDTO startGameDTO)
    {
        GameManager game = Instantiate(GamePrefab, this.gameObject.GetComponentInParent<Canvas>().transform).GetComponent<GameManager>();
        game.transform.position = this.gameObject.transform.position;
        game.StartingGame(startGameDTO);
        Destroy(gameObject);
    }

    public void BackToMainMenuWithError(string messageError)
    {
        ErrorMessage errorMessage = Instantiate(ErrorMessagePrefab, this.gameObject.GetComponentInParent<Canvas>().transform).GetComponent<ErrorMessage>();
        errorMessage.transform.position = this.gameObject.transform.position;
        errorMessage.SetErrorMessage(messageError);
        Multiplayer.Instance.DisconnectFromServer();
        MainMenu settingsMenu = Instantiate(MainMenuPrefab, this.gameObject.GetComponentInParent<Canvas>().transform).GetComponent<MainMenu>();
        settingsMenu.transform.position = this.gameObject.transform.position;
        Destroy(this.gameObject);
    }

    public void BackToLobbiesMenu()
    {
        Multiplayer.Instance.DisconnectFromLobby();
        LobbiesMenu lobbiesMenu = Instantiate(LobbiesMenuPrefab, this.gameObject.GetComponentInParent<Canvas>().transform).GetComponent<LobbiesMenu>();
        lobbiesMenu.transform.position = this.gameObject.transform.position;
        Destroy(this.gameObject);
    }

    public void AddClient(ClientDTO client)
    {
        ClientInfoRow clientInfoRow = Instantiate(ClientInfoRowPrefab, context.transform).GetComponent<ClientInfoRow>();

        clientInfoRow.SetText(client.id, client.name);
        clientInfoRow.id = client.id;

        clientInfoRows.Add(clientInfoRow);
        numOfPlayersText.text = $"{clientInfoRows.Count}/{maxNumOfClients}";
    }

    public void RemoveClient(ClientDTO client)
    {
        ClientInfoRow c = clientInfoRows.First(c => c.id == client.id);
        clientInfoRows.Remove(c);
        Destroy(c.gameObject);
        numOfPlayersText.text = $"{clientInfoRows.Count}/{maxNumOfClients}";
    }

    public void SetNewHost()
    {
        startButton.SetActive(true);
    }

    private void OnDestroy()
    {
        Multiplayer.Instance.ClientConnectedToLobbyEvent -= AddClient;
        Multiplayer.Instance.ClientDisconnectedFromLobbyEvent -= RemoveClient;
        Multiplayer.Instance.ServerIsNotAvaliableEvent -= BackToMainMenuWithError;
        Multiplayer.Instance.StartingGameEvent -= StartingGame;
        Multiplayer.Instance.SetNewHostEvent -= SetNewHost;
    }
}
