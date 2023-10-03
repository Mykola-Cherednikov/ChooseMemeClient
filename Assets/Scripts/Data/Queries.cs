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

    public static string ChoseVideo(VideoDTO videoDTO)
    {
        return MultiplayerEvents.ChoseVideoGameEvent.ToString() + "\n" + JsonUtility.ToJson(videoDTO) + "\n";
    }

    public static string VoteForVideo(VideoDTO videoDTO)
    {
        return MultiplayerEvents.VoteForVideoGameEvent.ToString() + "\n" + JsonUtility.ToJson(videoDTO) + "\n";
    }
}
