using ChooseMemeServer.DTO;
using System;
using TMPro;
using UnityEngine;

public class LobbyInfoRow : MonoBehaviour
{
    [SerializeField]
    private TMP_Text idText;

    [SerializeField]
    private TMP_Text nameText;

    [SerializeField]
    private TMP_Text numOfClientsText;

    public void SetLobbyInfoRowText(int id, string name, string numOfClients)
    {
        idText.text = id.ToString();
        nameText.text = name;
        numOfClientsText.text = numOfClients;
    }

    public void ConnectToLobby()
    {
        ShortLobbyDTO shortLobbyDTO = new ShortLobbyDTO();
        shortLobbyDTO.id = Convert.ToInt32(idText.text);
        Multiplayer.Instance.ConnectToLobby(shortLobbyDTO);
    }
}
