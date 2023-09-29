using ChooseMemeServer.DTO;
using UnityEngine;

public class Queries
{
    public static string SetNickNameQuery(NicknameDTO nickNameDTO)
    {
        return MultiplayerEvents.SetNickName.ToString() + "\n" + JsonUtility.ToJson(nickNameDTO) + "\n";
    }

    public static string CreateLobbyQuery(ShortLobbyDTO shortLobbyDTO)
    {
        return MultiplayerEvents.CreateLobby.ToString() + "\n" + JsonUtility.ToJson(shortLobbyDTO) + "\n";
    }

    public static string ConnectToLobbyQuery(ShortLobbyDTO shortLobbyDTO)
    {
        return MultiplayerEvents.ConnectToLobby.ToString() + "\n" + JsonUtility.ToJson(shortLobbyDTO) + "\n";
    }

    public static string ChoseCard(CardDTO cardDTO)
    {
        return MultiplayerEvents.ChoseCardGameEvent.ToString() + "\n" + JsonUtility.ToJson(cardDTO) + "\n";
    }

    public static string VoteForCard(CardDTO cardDTO)
    {
        return MultiplayerEvents.VoteForCardGameEvent.ToString() + "\n" + JsonUtility.ToJson(cardDTO) + "\n";
    }
}
