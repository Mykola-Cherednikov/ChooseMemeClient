using Assets.Scripts.DTO;
using Assets.Scripts.UnityObjects;
using ChooseMemeServer.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum ClientStatus
{
    Connected,
    Disconnected
}

public enum MultiplayerEvents
{
    Ping, //server
    AskForLobbies, //client
    ReturnLobbies, //server
    SetNickName, //client
    CreateLobby, //client
    ConnectToLobby, //client
    DisconnectFromLobby, //client
    ReturnLobby, //server
    ClientConnectedToLobby, //server 
    ClientDisconnectedFromLobby, //server
    SetNewHost, //server
    StartGame, //client

    StartingGameGameEvent, //server
    ReadingTheQuestionGameEvent, //server
    ChoseTheVideoGameEvent, //server
    AfterChosedVideoGameEvent, //server
    StartVoteForVideoGameEvent, //server
    VotingForVideoGameEvent, //server
    VoteForVideoGameEvent, //client
    GivingAnotherVideoGameEvent, //server
    EndGameGameEvent, //server

    AddVideoGameEvent, //server
    ChoseVideoGameEvent, //client
    RemoveVideoGameEvent, //server
}

public class Multiplayer : MonoBehaviour
{
    public static Multiplayer Instance;

    private void Awake()
    {
        Instance = this;
        status = ClientStatus.Disconnected;
    }

    public delegate void ServerIsNotAvaliable(string errorMessage);
    public event ServerIsNotAvaliable ServerIsNotAvaliableEvent;

    public delegate void ReturnLobbies(List<ShortLobbyDTO> lobbies);
    public event ReturnLobbies ReturnLobbiesEvent;

    public delegate void ReturnLobby(LobbyDTO lobbyDTO);
    public event ReturnLobby ReturnLobbyEvent;

    public delegate void ClientConnectedToLobby(ClientDTO client);
    public event ClientConnectedToLobby ClientConnectedToLobbyEvent;

    public delegate void ClientDisconnectedFromLobby(ClientDTO client);
    public event ClientDisconnectedFromLobby ClientDisconnectedFromLobbyEvent;

    public delegate void SetNewHost();
    public event SetNewHost SetNewHostEvent;

    public delegate void StartingGame(StartGameDTO startGameDTO);
    public event StartingGame StartingGameEvent;

    public delegate void AddVideo(VideoDTO videoDTO);
    public event AddVideo AddVideoEvent;

    public delegate void ReadingTheQuestion(QuestionDTO questionDTO);
    public event ReadingTheQuestion ReadingTheQuestionEvent;

    public delegate void ChoseTheVideo();
    public event ChoseTheVideo ChoseTheVideoEvent;

    public delegate void AfterChosedVideo();
    public event AfterChosedVideo AfterChosedVideoEvent;

    public delegate void RemoveVideo(VideoDTO videoDTO);
    public event RemoveVideo RemoveVideoEvent;

    public delegate void VotingForVideo(VideoDTO videoDTO);
    public event VotingForVideo VotingForVideoEvent;

    public delegate void GivingAnotherVideo();
    public event GivingAnotherVideo GivingAnotherVideoEvent;

    public delegate void EndGame(List<PlayerDTO> playerDTO);
    public event EndGame EndGameEvent;

    private Queue<string> queries;

    private TcpClient client;

    public ClientStatus status;

    public async Task<bool> ConnectToServer() // Connect to server
    {
        client = new TcpClient(); // Create new tcp client object

        queries = new Queue<string>(); // Create new queue with queries

        try
        {
            await client.ConnectAsync(IPAddress.Parse(PlayerPrefs.GetString("IP")), 27015); // Connecting to server

            Debug.Log("Connected");

            ListenServer(); // After connect start listening server

            NicknameDTO nicknameDTO = new(); // Create new NicknameDTO

            nicknameDTO.NickName = PlayerPrefs.GetString("NickName"); // Set nickname to NicknameDTO

            SetNickName(nicknameDTO);

            status = ClientStatus.Connected;

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async void ListenServer() // Listening server
    {
        try
        {
            while (client != null)
            {
                byte[] buffer = new byte[1024]; // byte buffer

                int numOfBytes = await client.GetStream().ReadAsync(buffer); // Read from server to buffer

                string query = Encoding.UTF8.GetString(buffer[..numOfBytes]); // Convert byte buffer to string

                string[] queryRows = query.Split("\n"); // Split into rows

                foreach (var row in queryRows)
                {
                    if (row != string.Empty)
                    {
                        queries.Enqueue(row); // Add querry
                    }
                }

                while (queries.Count > 0) // Handle a query
                {
                    if (queries.TryPeek(out string result) && Enum.TryParse(result, out MultiplayerEvents quaryHead)) // Try to get query head
                    {
                        queries.Dequeue();
                        switch (quaryHead)
                        {
                            case MultiplayerEvents.ReturnLobbies: // Returned Lobbies info
                                {

                                    ArrayOfShortLobbiesDTO lobbiesDTO = JsonUtility.FromJson<ArrayOfShortLobbiesDTO>(queries.Dequeue());
                                    List<ShortLobbyDTO> list = new List<ShortLobbyDTO>(lobbiesDTO.shortLobbiesDTO);
                                    ReturnLobbiesEvent?.Invoke(list);
                                    break;
                                }
                            case MultiplayerEvents.ReturnLobby: // Returned Lobby info
                                {
                                    LobbyDTO lobbyDTO = JsonUtility.FromJson<LobbyDTO>(queries.Dequeue());
                                    ReturnLobbyEvent?.Invoke(lobbyDTO);
                                    break;
                                }
                            case MultiplayerEvents.ClientConnectedToLobby: // Returned Client which connect
                                {
                                    ClientDTO clientDTO = JsonUtility.FromJson<ClientDTO>(queries.Dequeue());
                                    ClientConnectedToLobbyEvent?.Invoke(clientDTO);
                                    break;
                                }
                            case MultiplayerEvents.ClientDisconnectedFromLobby: // Returned Client that disconnect
                                {
                                    ClientDTO clientDTO = JsonUtility.FromJson<ClientDTO>(queries.Dequeue());
                                    ClientDisconnectedFromLobbyEvent?.Invoke(clientDTO);
                                    break;
                                }
                            case MultiplayerEvents.StartingGameGameEvent:
                                {
                                    string s = queries.Dequeue();
                                    StartGameDTO startGameDTO = JsonUtility.FromJson<StartGameDTO>(s);
                                    StartingGameEvent?.Invoke(startGameDTO);
                                    break;
                                }
                            case MultiplayerEvents.ReadingTheQuestionGameEvent:
                                {
                                    QuestionDTO questionDTO = JsonUtility.FromJson<QuestionDTO>(queries.Dequeue());
                                    ReadingTheQuestionEvent?.Invoke(questionDTO);
                                    break;
                                }
                            case MultiplayerEvents.AddVideoGameEvent:
                                {
                                    VideoDTO videoDTO = JsonUtility.FromJson<VideoDTO>(queries.Dequeue());
                                    AddVideoEvent?.Invoke(videoDTO);
                                    break;
                                }
                            case MultiplayerEvents.RemoveVideoGameEvent:
                                {
                                    VideoDTO videoDTO = JsonUtility.FromJson<VideoDTO>(queries.Dequeue());
                                    RemoveVideoEvent?.Invoke(videoDTO);
                                    break;
                                }
                            case MultiplayerEvents.ChoseTheVideoGameEvent:
                                {
                                    ChoseTheVideoEvent?.Invoke();
                                    break;
                                }
                            case MultiplayerEvents.AfterChosedVideoGameEvent:
                                {
                                    AfterChosedVideoEvent?.Invoke();
                                    break;
                                }
                            case MultiplayerEvents.SetNewHost:
                                {
                                    SetNewHostEvent?.Invoke();
                                    break;
                                }
                            case MultiplayerEvents.VotingForVideoGameEvent:
                                {
                                    VideoDTO videoDTO = JsonUtility.FromJson<VideoDTO>(queries.Dequeue());
                                    VotingForVideoEvent?.Invoke(videoDTO);
                                    break;
                                }
                            case MultiplayerEvents.GivingAnotherVideoGameEvent:
                                {
                                    GivingAnotherVideoEvent?.Invoke(); 
                                    break;
                                }
                            case MultiplayerEvents.EndGameGameEvent:
                                {
                                    ArrayOfPlayersDTO array = JsonUtility.FromJson<ArrayOfPlayersDTO>(queries.Dequeue());
                                    List<PlayerDTO> players = new List<PlayerDTO>(array.playersDTO);
                                    EndGameEvent?.Invoke(players);
                                    break;
                                }
                            case MultiplayerEvents.Ping:
                                {
                                    //Debug.Log("Ping");
                                    break;
                                }
                        }
                    }
                    else
                    {
                        queries.Dequeue();
                    }
                }
            }
        }
        catch (Exception)
        {
            ServerIsNotAvaliableEvent?.Invoke("Server is not avaliable");
        }
    }

    public async void SetNickName(NicknameDTO nickNameDTO)
    {
        string query = Queries.SetNickNameQuery(nickNameDTO); // Create string query(Take event, Enter, Serialize NicknameDTO)

        await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(query)); // Send to server string query encoded to bytes
    }

    public async void AskForLobbies()
    {
        string query = MultiplayerEvents.AskForLobbies.ToString() + "\n";

        await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(query));
    }

    public async void CreateLobby(ShortLobbyDTO shortLobbyDTO)
    {
        string query = Queries.CreateLobbyQuery(shortLobbyDTO);

        await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(query));
    }

    public async void ConnectToLobby(ShortLobbyDTO shortLobbyDTO)
    {
        string query = Queries.ConnectToLobbyQuery(shortLobbyDTO);

        await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(query));
    }

    public async void DisconnectFromLobby()
    {
        string query = MultiplayerEvents.DisconnectFromLobby.ToString() + "\n";

        await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(query));
    }

    public void DisconnectFromServer()
    {
        if (client != null)
        {
            client.Close();
            Debug.Log("Client closed");
            client.Dispose();
        }
        client = null;
        status = ClientStatus.Disconnected;
    }

    public async void StartGame()
    {
        string query = MultiplayerEvents.StartGame.ToString() + "\n";

        await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(query));
    }

    public async void ChoseVideo(VideoDTO videoDTO)
    {
        string query = Queries.ChoseVideo(videoDTO);

        await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(query));
    }

    public async void VoteForVideo(VideoDTO videoDTO)
    {
        string query = Queries.VoteForVideo(videoDTO);

        await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(query));
    }
}
