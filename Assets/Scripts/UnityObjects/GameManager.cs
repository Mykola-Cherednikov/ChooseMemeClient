using ChooseMemeServer.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public enum GameStatus
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
}

public class GameManager : MonoBehaviour
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

        gameStatus = GameStatus.BeforeGame;
        cardsForVoting = new List<Card>();
    }

    private void Update()
    {
        gameStatusText.text = gameStatus.ToString();

        if (Input.GetKeyDown(KeyCode.Mouse0) && (gameStatus == GameStatus.ChoseTheCard || gameStatus == GameStatus.VotingForCard))
        {
            Ray ray = new Ray(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -10f), Camera.main.transform.forward * 50000000);

            if (Physics.Raycast(ray, 100000f))
            {
                List<RaycastHit> hits = Physics.RaycastAll(ray, 100000f).ToList();
                foreach (var hit in hits)
                {
                    if (hit.collider.gameObject.TryGetComponent<Card>(out var card))
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

    private void FixedUpdate()
    {
        if (timer > 0)
        {
            timer -= Time.fixedDeltaTime;
            secondsText.text = String.Format("{0:0.00}", timer);
        }
        else
        {
            secondsText.text = "0";
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
        StartTimer(3);
    }

    private void ReadingTheQuestion(QuestionDTO questionDTO)
    {
        gameStatus = GameStatus.ReadingTheQuestion;
        questionWithIdText.text = questionDTO.id.ToString();
        StartTimer(4);
    }

    private void ChoseTheCard()
    {
        gameStatus = GameStatus.ChoseTheCard;
        StartTimer(60);
    }

    private void AfterChosedCard()
    {
        gameStatus = GameStatus.AfterChosedCard;
        foreach(Card card in you.hand)
        {
            card.UnselectCard();
            card.UnsendCard();
        }

        TurnOffChoseInterface();
    }

    private void StartVoteForCard(List<CardDTO> cards)
    {
        foreach(var card in cardsForVoting)
        {
            Destroy(card.gameObject);
        }

        cardsForVoting = new List<Card>();

        gameStatus = GameStatus.StartVoteForCard;

        foreach (var card in cards)
        {
            Card cardObject = Instantiate(CardPrefab, voteShowcaseContent.transform).GetComponent<Card>();
            cardObject.SetCardData(card.id, null, CardType.CardForVote);
            cardsForVoting.Add(cardObject);
        }
        StartTimer(60);
    }

    private void VotingForCard()
    {
        gameStatus = GameStatus.VotingForCard;
        TurnOnVoteInterface();
        StartTimer(15);
    }

    private void GivingAnotherCard()
    {
        TurnOnChoseInterface();
        TurnOffVoteInterface();
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

        foreach(var player in players)
        {
            PlayerCard playerCard = Instantiate(PlayerCardPrefab, endGamePlayersContent.transform).GetComponent<PlayerCard>();
            playerCard.SetText(player.clientDTO.name, player.points);
        }
    }

    private void StartTimer(int seconds)
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
        foreach(var obj in interfaceForChosing)
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
