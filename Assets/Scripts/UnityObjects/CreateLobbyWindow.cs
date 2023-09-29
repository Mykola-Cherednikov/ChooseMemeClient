using ChooseMemeServer.DTO;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobbyWindow : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputField;

    [SerializeField]
    private Slider slider;

    [SerializeField]
    private TMP_Text NumberOfPlayersChangableText;

    [NonSerialized]
    public LobbiesMenu lobbiesMenu;

    public void CreateLobby()
    {
        ShortLobbyDTO shortLobbyDTO = new ShortLobbyDTO();
        shortLobbyDTO.name = inputField.text;
        shortLobbyDTO.maxNumOfClients = Convert.ToInt32(slider.value);

        lobbiesMenu.CreateLobby(shortLobbyDTO);
        Destroy(gameObject);
    }

    public void Close()
    {
        Destroy(gameObject);
    }

    public void SetNumOfPlayers()
    {
        NumberOfPlayersChangableText.text = Convert.ToInt32(slider.value).ToString();
    }
}
