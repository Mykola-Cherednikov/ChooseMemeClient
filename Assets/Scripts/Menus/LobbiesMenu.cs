using Assets.Scripts.DTO;
using ChooseMemeServer.DTO;
using System.Collections.Generic;
using UnityEngine;

public class LobbiesMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject MainMenuPrefab;

    [SerializeField]
    private GameObject LobbyMenuPrefab;

    [SerializeField]
    private GameObject LobbyInfoRowPrefab;

    [SerializeField]
    private GameObject ErrorMessagePrefab;

    [SerializeField]
    private GameObject CreateLobbyWindowPrefab;

    [SerializeField]
    private GameObject context;

    private List<LobbyInfoRow> lobbiesRow;

    void Start()
    {
        lobbiesRow = new();

        Multiplayer.Instance.ReturnLobbiesEvent += SetLobbies;
        Multiplayer.Instance.ReturnLobbyEvent += ConnectToLobby;
        Multiplayer.Instance.ServerIsNotAvaliableEvent += BackWithError;

        if (Multiplayer.Instance.status == ClientStatus.Disconnected)
        {
            StartClient();
        }
        else
        {
            Multiplayer.Instance.AskForLobbies();
        }
    }

    private async void StartClient()
    {
        if (await Multiplayer.Instance.ConnectToServer())
        {
            NicknameDTO nicknameDTO = new NicknameDTO();

            nicknameDTO.NickName = PlayerPrefs.GetString("NickName");

            Multiplayer.Instance.SetNickName(nicknameDTO);

            Multiplayer.Instance.AskForLobbies();
        }
        else
        {
            BackWithError("Server now offline");
        }
    }

    private void BackWithError(string messageError)
    {
        ErrorMessage errorMessage = Instantiate(ErrorMessagePrefab, this.gameObject.GetComponentInParent<Canvas>().transform).GetComponent<ErrorMessage>();
        errorMessage.transform.position = this.gameObject.transform.position;
        errorMessage.SetErrorMessage(messageError);
        BackToMainMenu();
    }

    public void BackToMainMenu()
    {
        Multiplayer.Instance.DisconnectFromServer();
        MainMenu settingsMenu = Instantiate(MainMenuPrefab, this.gameObject.GetComponentInParent<Canvas>().transform).GetComponent<MainMenu>();
        settingsMenu.transform.position = this.gameObject.transform.position;
        Destroy(this.gameObject);
    }

    private void SetLobbies(List<ShortLobbyDTO> lobbies)
    {
        foreach (var shortLobbyDTO in lobbies)
        {
            LobbyInfoRow row = Instantiate(LobbyInfoRowPrefab, context.transform).GetComponent<LobbyInfoRow>();
            row.SetLobbyInfoRowText(shortLobbyDTO.id, shortLobbyDTO.name, $"{shortLobbyDTO.numOfClients}/{shortLobbyDTO.maxNumOfClients}");
            lobbiesRow.Add(row);
        }
    }

    public void CreateLobbyButtonClick()
    {
        if (Multiplayer.Instance.status == ClientStatus.Connected)
        {
            CreateLobbyWindow createLobbyWindow = Instantiate(CreateLobbyWindowPrefab, this.transform).GetComponent<CreateLobbyWindow>();
            createLobbyWindow.lobbiesMenu = this;
        }
    }

    public void CreateLobby(ShortLobbyDTO shortLobbyDTO)
    {
        Multiplayer.Instance.CreateLobby(shortLobbyDTO);
    }

    private void ConnectToLobby(LobbyDTO lobbyDTO)
    {
        if (Multiplayer.Instance.status == ClientStatus.Connected)
        {
            LobbyMenu lobbyMenu = Instantiate(LobbyMenuPrefab, this.gameObject.GetComponentInParent<Canvas>().transform).GetComponent<LobbyMenu>();
            lobbyMenu.transform.position = this.gameObject.transform.position;
            lobbyMenu.SetLobbyInfo(lobbyDTO);
            Destroy(this.gameObject);
        }
    }

    public void RefreshLobbies()
    {
        if (Multiplayer.Instance.status == ClientStatus.Connected)
        {
            foreach (var lobbyRow in lobbiesRow)
            {
                Destroy(lobbyRow.gameObject);
            }
            lobbiesRow = new List<LobbyInfoRow>();
            Multiplayer.Instance.AskForLobbies();
        }
    }

    private void OnDestroy()
    {
        Multiplayer.Instance.ReturnLobbiesEvent -= SetLobbies;
        Multiplayer.Instance.ReturnLobbyEvent -= ConnectToLobby;
        Multiplayer.Instance.ServerIsNotAvaliableEvent -= BackWithError;
    }
}
