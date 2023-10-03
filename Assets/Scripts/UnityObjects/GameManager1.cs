/*using Assets.Scripts.Data;
using ChooseMemeServer.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

*//*public enum GameStatus
{
    BeforeGame,
    StartingGame,
    ReadingTheQuestion,
    ChoseTheCard,
    AfterChosedCard,
    StartVoteForCard,
    VotingForCard,
    GivingAnotherCard,
    EndGame
}*//*

public class GameManager1 : MonoBehaviour
{
    [SerializeField]
    private GameObject MainMenuPrefab;

    [SerializeField]
    private TMP_Text questionText;
    [SerializeField]
    private TMP_Text questionWithIdText;
    [SerializeField]
    private TMP_Text gameStatusText;

    [SerializeField]
    private List<GameObject> interfaceForChosing = new List<GameObject>();
    [SerializeField]
    private GameObject hand;
    [SerializeField]
    private GameObject handContent;

    [SerializeField]
    private GameObject voteTexture;
    [SerializeField]
    private VideoPlayer voteVideoPlayer;

    [SerializeField]
    private List<GameObject> interfaceForVoting = new List<GameObject>();
    [SerializeField]
    private GameObject voteShowcase;
    [SerializeField]
    private GameObject voteShowcaseContent;


    [SerializeField]
    private List<GameObject> interfaceForEndGame = new List<GameObject>();
    [SerializeField]
    private GameObject endGamePlayers;
    [SerializeField]
    private GameObject endGamePlayersContent;

    [SerializeField]
    private GameObject CardPrefab;
    [SerializeField]
    private GameObject PlayerCardPrefab;
    [SerializeField]
    private TMP_Text secondsText;



    [SerializeField]
    private GameStatus gameStatus;

    private List<PlayerData> players;

    private PlayerData you;

    private float timer;

    private Card selectedCard;

    private Card sendedCard;

    private List<Card> cardsForVoting;

    private GameSettings gameSettings;

    List<QuestionData> questions;

    private void Awake()
    {
        gameStatus = GameStatus.BeforeGame;
        cardsForVoting = new List<Card>();


        using (StreamReader r = new StreamReader(Application.dataPath + "/GameSettings.json"))
        {
            string json = r.ReadToEnd();
            gameSettings = JsonUtility.FromJson<GameSettings>(json)!;
        }
        QuestionData[] source;
        using (StreamReader r = new StreamReader(Application.dataPath + "/Loads/questions.json"))
        {
            string json = r.ReadToEnd();
            source = JsonUtility.FromJson<QuestionsArray>(json).Questions;
        }
        questions = new List<QuestionData>(source);
    }

    private void Start()
    {
        Multiplayer.Instance.ReadingTheQuestionEvent += ReadingTheQuestion;
        Multiplayer.Instance.ChoseTheCardEvent += ChoseTheCard;
        Multiplayer.Instance.AfterChosedCardEvent += AfterChosedCard;
        Multiplayer.Instance.StartVoteForCardEvent += StartVoteForCard;
        Multiplayer.Instance.VotingForCardEvent += VotingForCard;
        Multiplayer.Instance.GivingAnotherCardEvent += GivingAnotherCard;
        Multiplayer.Instance.EndGameEvent += EndGame;
        Multiplayer.Instance.AddCardEvent += AddCard;
        Multiplayer.Instance.RemoveCardEvent += RemoveCard;
    }

    private void Update()
    {
        gameStatusText.text = gameStatus.ToString();

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Ray ray = new Ray(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -10f), Camera.main.transform.forward * 50000000);

            if (Physics.Raycast(ray, 100000f))
            {
                List<RaycastHit> hits = Physics.RaycastAll(ray, 100000f).ToList();
                foreach (var hit in hits)
                {
                    if (hit.collider.gameObject.TryGetComponent<Card>(out var card))
                    {
                        if (gameStatus == GameStatus.ReadingTheQuestion ||
                            gameStatus == GameStatus.ChoseTheCard ||
                            (gameStatus == GameStatus.VotingForCard &&
                            card.owner.id != you.id))
                        {
                            if (selectedCard == card)
                            {
                                sendedCard?.UnsendCard();
                                card.SelectCard();
                                sendedCard = card;
                            }
                            else
                            {
                                selectedCard?.UnselectCard();
                                selectedCard = card;
                                selectedCard.SelectCard();
                            }
                        }
                    }
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (timer > 0)
        {
            timer -= Time.fixedDeltaTime;
            if (timer <= 0)
            {
                timer = 0;
            }
            secondsText.text = String.Format("{0:0.00}", timer);
        }
    }

    public void StartingGame(StartGameDTO startGameDTO)
    {
        players = new List<PlayerData>();
        gameStatus = GameStatus.StartingGame;

        TurnOnChoseInterface();

        foreach (var client in startGameDTO.clientsDTO.clientsDTO)
        {
            PlayerData player = new PlayerData();
            player.id = client.id;
            player.nickName = client.name;

            Debug.Log(player.id + " " + player.nickName);
            player.hand = new List<Card>();
            players.Add(player);
        }

        PlayerData youPlayer = players.FirstOrDefault(p => p.id == startGameDTO.you.id);

        you = youPlayer;
        StartTimer((gameSettings.MilliSecondsBeforeStart / 1000f) + ((players.Count * 4 * gameSettings.MilliSecondsPerCard) / 1000f));
    }

    private void ReadingTheQuestion(QuestionDTO questionDTO)
    {
        gameStatus = GameStatus.ReadingTheQuestion;
        questionWithIdText.text = questions[questionDTO.id].text;
        StartTimer(gameSettings.MilliSecondsReadingQuestion / 1000f);
    }

    private void ChoseTheCard()
    {
        gameStatus = GameStatus.ChoseTheCard;
        StartTimer(gameSettings.MilliSecondsForChooseGame / 1000f);
    }

    private void AfterChosedCard()
    {
        gameStatus = GameStatus.AfterChosedCard;
        foreach (Card card in you.hand)
        {
            card.UnselectCard();
            card.UnsendCard();
        }
        selectedCard = null;
        sendedCard = null;

        TurnOffChoseInterface();
    }

    private async void StartVoteForCard(List<CardDTO> cards)
    {
        gameStatus = GameStatus.StartVoteForCard;

        StartTimer((gameSettings.MilliSecondsStartVote / 1000f) * cards.Count);

        voteTexture.SetActive(true);

        foreach (var card in cards)
        {
            voteVideoPlayer.SetDirectAudioVolume(0, 0.15f);
            voteVideoPlayer.url = "file://" + Application.dataPath + $"/Loads/{card.id}.mp4";
            voteVideoPlayer.Prepare();
            voteVideoPlayer.Play();
            await Task.Delay(gameSettings.MilliSecondsStartVote);
        }
        voteTexture.SetActive(false);
    }

    private void VotingForCard(List<CardDTO> cards)
    {
        gameStatus = GameStatus.VotingForCard;
        TurnOnVoteInterface();

        foreach (var card in cardsForVoting)
        {
            Destroy(card.gameObject);
        }

        cardsForVoting = new List<Card>();

        foreach (var card in cards)
        {
            Card cardObject = Instantiate(CardPrefab, voteShowcaseContent.transform).GetComponent<Card>();
            PlayerData player = new PlayerData() { id = card.owner.id, nickName = card.owner.name };
            cardObject.SetCardData(card.id, player, CardType.CardForVote);
            cardsForVoting.Add(cardObject);
        }

        StartTimer(gameSettings.MilliSecondsVoting / 1000f);
    }

    private void GivingAnotherCard()
    {
        selectedCard = null;
        sendedCard = null;
        TurnOffVoteInterface();
        TurnOnChoseInterface();
        foreach (Card card in you.hand)
        {
            card.UnselectCard();
            card.UnsendCard();
        }
        gameStatus = GameStatus.GivingAnotherCard;
    }

    private void AddCard(CardDTO cardDTO)
    {
        PlayerData playerOwner = players.FirstOrDefault(p => p.id == cardDTO.owner.id);
        if (playerOwner == you)
        {
            Card card = Instantiate(CardPrefab, handContent.transform).GetComponent<Card>();
            card.SetCardData(cardDTO.id, playerOwner, CardType.CardForChose);

            Debug.Log($"{playerOwner.nickName} get card, with id = {card.id}");

            playerOwner.hand.Add(card);
        }
    }

    private void RemoveCard(CardDTO cardDTO)
    {
        PlayerData playerOwner = players.FirstOrDefault(p => p.id == cardDTO.owner.id);
        if (playerOwner == you)
        {
            Card card = playerOwner.hand.FirstOrDefault(c => c.id == cardDTO.id);

            Debug.Log($"{playerOwner.nickName} remove card, with id = {card.id}");
            playerOwner.hand.Remove(card);
            Destroy(card.gameObject);
        }
    }

    private void EndGame(List<PlayerDTO> players)
    {
        TurnOffVoteInterface();
        TurnOffChoseInterface();
        TurnOnEndGameInterface();

        players = players.OrderBy(p => p.points).ToList();

        foreach (var player in players)
        {
            PlayerCard playerCard = Instantiate(PlayerCardPrefab, endGamePlayersContent.transform).GetComponent<PlayerCard>();
            playerCard.SetText(player.clientDTO.name, player.points);
        }
    }

    private void StartTimer(float seconds)
    {
        timer = seconds;
    }

    public void ExitFromGame()
    {
        Multiplayer.Instance.DisconnectFromServer();
        MainMenu mainMenu = Instantiate(MainMenuPrefab, this.gameObject.GetComponentInParent<Canvas>().transform).GetComponent<MainMenu>();
        mainMenu.transform.position = this.gameObject.transform.position;
        Destroy(this.gameObject);
    }

    #region Interfaces

    private void TurnOnChoseInterface()
    {
        foreach (var obj in interfaceForChosing)
        {
            obj.SetActive(true);
        }
    }

    private void TurnOffChoseInterface()
    {
        foreach (var obj in interfaceForChosing)
        {
            obj.SetActive(false);
        }
    }

    private void TurnOnVoteInterface()
    {
        foreach (var obj in interfaceForVoting)
        {
            obj.SetActive(true);
        }
    }

    private void TurnOffVoteInterface()
    {
        foreach (var obj in interfaceForVoting)
        {
            obj.SetActive(false);
        }
    }

    private void TurnOnEndGameInterface()
    {
        foreach (var obj in interfaceForEndGame)
        {
            obj.SetActive(true);
        }
    }

    private void TurnOffEndGameInterface()
    {
        foreach (var obj in interfaceForEndGame)
        {
            obj.SetActive(false);
        }
    }

    #endregion

    private void OnDestroy()
    {
        Multiplayer.Instance.ReadingTheQuestionEvent -= ReadingTheQuestion;
        Multiplayer.Instance.ChoseTheCardEvent -= ChoseTheCard;
        Multiplayer.Instance.AfterChosedCardEvent -= AfterChosedCard;
        Multiplayer.Instance.StartVoteForCardEvent -= StartVoteForCard;
        Multiplayer.Instance.VotingForCardEvent -= VotingForCard;
        Multiplayer.Instance.GivingAnotherCardEvent -= GivingAnotherCard;
        Multiplayer.Instance.EndGameEvent -= EndGame;
        Multiplayer.Instance.AddCardEvent -= AddCard;
        Multiplayer.Instance.RemoveCardEvent -= RemoveCard;
    }
}
*/