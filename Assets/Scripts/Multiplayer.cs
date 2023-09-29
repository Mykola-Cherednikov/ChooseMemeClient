using Assets.Scripts.DTO;
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
    AddCardGameEvent, //server
    RemoveCardGameEvent, //server
    ReadingTheQuestionGameEvent, //server
    ChoseTheCardGameEvent, //server
    AfterChosedCardGameEvent, //server
    StartVoteForCardGameEvent, //server
    VotingForCardGameEvent, //server
    GivingAnotherCardGameEvent, //server
    EndGameGameEvent, //server

    ChoseCardGameEvent, //client
    VoteForCardGameEvent, //client
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

    public delegate void AddCard(CardDTO cardDTO);
    public event AddCard AddCardEvent;

    public delegate void ReadingTheQuestion(QuestionDTO questionDTO);
    public event ReadingTheQuestion ReadingTheQuestionEvent;

    public delegate void ChoseTheCard();
    public event ChoseTheCard ChoseTheCardEvent;

    public delegate void AfterChosedCard();
    public event AfterChosedCard AfterChosedCardEvent;

    public delegate void RemoveCard(CardDTO cardDTO);
    public event RemoveCard RemoveCardEvent;

    public delegate void StartVoteForCard(List<CardDTO> cardsDTO);
    public event StartVoteForCard StartVoteForCardEvent;

    public delegate void VotingForCard();
    public event VotingForCard VotingForCardEvent;

    public delegate void GivingAnotherCard();
    public event GivingAnotherCard GivingAnotherCardEvent;

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
                            case MultiplayerEvents.AddCardGameEvent:
                                {
                                    CardDTO cardDTO = JsonUtility.FromJson<CardDTO>(queries.Dequeue());
                                    AddCardEvent?.Invoke(cardDTO);
                                    break;
                                }
                            case MultiplayerEvents.RemoveCardGameEvent:
                                {
                                    CardDTO cardDTO = JsonUtility.FromJson<CardDTO>(queries.Dequeue());
                                    RemoveCardEvent?.Invoke(cardDTO);
                                    break;
                                }
                            case MultiplayerEvents.ChoseTheCardGameEvent:
                                {
                                    ChoseTheCardEvent?.Invoke();
                                    break;
                                }
                            case MultiplayerEvents.AfterChosedCardGameEvent:
                                {
                                    AfterChosedCardEvent?.Invoke();
                                    break;
                                }
                            case MultiplayerEvents.SetNewHost:
                                {
                                    SetNewHostEvent?.Invoke();
                                    break;
                                }
                            case MultiplayerEvents.StartVoteForCardGameEvent:
                                {
                                    ArrayOfCardsDTO array = JsonUtility.FromJson<ArrayOfCardsDTO>(queries.Dequeue());
                                    List<CardDTO> cards = new List<CardDTO>(array.cards);
                                    StartVoteForCardEvent.Invoke(cards);
                                    break;
                                }
                            case MultiplayerEvents.VotingForCardGameEvent:
                                {
                                    VotingForCardEvent?.Invoke();
                                    break;
                                }
                            case MultiplayerEvents.GivingAnotherCardGameEvent:
                                {
                                    GivingAnotherCardEvent?.Invoke(); 
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
                                    Debug.Log("Ping");
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

    public async void ChoseCard(CardDTO cardDTO)
    {
        string query = Queries.ChoseCard(cardDTO);

        await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(query));
    }

    public async void VoteForCard(CardDTO cardDTO)
    {
        string query = Queries.VoteForCard(cardDTO);

        await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(query));
    }
}
