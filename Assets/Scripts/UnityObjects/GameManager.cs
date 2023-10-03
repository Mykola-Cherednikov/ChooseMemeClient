using Assets.Scripts.Data;
using Assets.Scripts.UnityObjects;
using ChooseMemeServer.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public enum GameStatus
{
    BeforeGame,
    StartingGame,
    ReadingTheQuestion,
    ChoseTheVideo,
    AfterChosedVideo,
    VotingForVideo,
    GivingAnotherVideo,
    EndGame
}

public class GameManager : MonoBehaviour
{
    [Header("Chose video")]
    #region Chose video

    [SerializeField]
    private GameObject ChoseSet;
    [SerializeField]
    private VideoPlayer videoPlayerForChose;
    [SerializeField]
    private GameObject checkGameObject;
    [SerializeField]
    private GameObject playImageGameObjectForChose;
    [SerializeField]
    private Slider sliderForChose;
    [SerializeField]
    private GameObject placeForVideoHorizontalInChose;
    [SerializeField]
    private GameObject placeForVideoVerticalInChose;
    [SerializeField]
    private RenderTexture choseVideoPlayerHorizontalRT;
    [SerializeField]
    private RenderTexture choseVideoPlayerVerticalRT;

    #endregion

    [Header("Vote video")]
    #region Vote video

    [SerializeField]
    private GameObject VotingSet;
    [SerializeField]
    private VideoPlayer videoPlayerForVote;
    [SerializeField]
    private GameObject heartGameObject;
    [SerializeField]
    private GameObject crossGameObject;
    [SerializeField]
    private GameObject playImageGameObjectForVote;
    [SerializeField]
    private Slider sliderForVote;
    [SerializeField]
    private GameObject placeForVideoHorizontalInVote;
    [SerializeField]
    private GameObject placeForVideoVerticalInVote;
    [SerializeField]
    private RenderTexture voteVideoPlayerHorizontalRT;
    [SerializeField]
    private RenderTexture voteVideoPlayerVerticalRT;

    #endregion

    [Header("Else")]
    [SerializeField]
    private GameObject EndSet;
    [SerializeField]
    private GameObject EndPlayersContent;
    [SerializeField]
    private GameObject PlayerCardPrefab;
    [SerializeField]
    private TMP_Text questionText;
    [SerializeField]
    private TMP_Text secondsText;


    private int numberChoseVideoInPlayer;

    private GameStatus gameStatus;

    private GameSettings gameSettings;

    private List<QuestionData> questions;

    private List<PlayerData> players;

    private PlayerData thisPlayer;

    private int nowPlayingVoteVideo;

    private float timer;



    void Awake()
    {
        numberChoseVideoInPlayer = 0;
        gameStatus = GameStatus.BeforeGame;
        videoPlayerForChose.prepareCompleted += SetChosePlayerPosition;
        videoPlayerForVote.prepareCompleted += SetVotePlayerPosition;
        sliderForChose.SetValueWithoutNotify(0.15f);
        sliderForVote.SetValueWithoutNotify(0.15f);

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

        SetTimer(gameSettings.MilliSecondsBeforeStart);

        Multiplayer.Instance.ReadingTheQuestionEvent += ReadingTheQuestion;
        Multiplayer.Instance.ChoseTheVideoEvent += ChoseTheVideo;
        Multiplayer.Instance.AfterChosedVideoEvent += AfterChosedVideo;
        Multiplayer.Instance.VotingForVideoEvent += VotingForVideo;
        Multiplayer.Instance.GivingAnotherVideoEvent += GivingAnotherVideo;
        Multiplayer.Instance.EndGameEvent += EndGame;
        Multiplayer.Instance.AddVideoEvent += AddVideo;
        Multiplayer.Instance.RemoveVideoEvent += RemoveVideo;
    }

    private void Update()
    {
        if(videoPlayerForChose.time == 15)
        {
            videoPlayerForChose.Stop();
        }
        if (videoPlayerForVote.time == 15)
        {
            videoPlayerForVote.Stop();
        }

        playImageGameObjectForChose.SetActive(videoPlayerForChose.gameObject.activeSelf == true && !videoPlayerForChose.isPlaying);

        playImageGameObjectForVote.SetActive(videoPlayerForVote.gameObject.activeSelf == true && !videoPlayerForVote.isPlaying);

        checkGameObject.SetActive(gameStatus == GameStatus.ChoseTheVideo && thisPlayer.hand != null && thisPlayer.hand[numberChoseVideoInPlayer] != null && thisPlayer.hand[numberChoseVideoInPlayer].chose);

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (gameStatus == GameStatus.ReadingTheQuestion || gameStatus == GameStatus.ChoseTheVideo || gameStatus == GameStatus.VotingForVideo)
            {
                Ray ray = new Ray(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -10f), Camera.main.transform.forward * 50000000);
                if (Physics.Raycast(ray, 100000f))
                {
                    List<RaycastHit> hits = Physics.RaycastAll(ray, 100000f).ToList();
                    foreach (var hit in hits)
                    {
                        if (hit.collider.gameObject.TryGetComponent<VideoPlayer>(out var videoPlayer))
                        {
                            ClickOnVideoPlayer(videoPlayer);
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
            secondsText.text = String.Format("{0:0}", timer);
        }
    }

    public void StartingGame(StartGameDTO startGameDTO)
    {
        players = new List<PlayerData>();
        gameStatus = GameStatus.StartingGame;

        foreach (var client in startGameDTO.clientsDTO.clientsDTO)
        {
            PlayerData player = new PlayerData();
            player.id = client.id;
            player.nickName = client.name;

            Debug.Log(player.id + " " + player.nickName);
            player.hand = new List<Video>();
            players.Add(player);
        }

        PlayerData youPlayer = players.FirstOrDefault(p => p.id == startGameDTO.you.id);

        thisPlayer = youPlayer;

        ChoseSet.SetActive(true);
    }

    private void ReadingTheQuestion(QuestionDTO questionDTO)
    {
        sliderForChose.SetValueWithoutNotify(0.15f);
        gameStatus = GameStatus.ReadingTheQuestion;
        SetTimer(gameSettings.MilliSecondsReadingQuestion);
        ClickOnVideoPlayer(videoPlayerForChose);
        questionText.text = questions[questionDTO.id].text;
    }

    private void ChoseTheVideo()
    {
        gameStatus = GameStatus.ChoseTheVideo;
        SetTimer(gameSettings.MilliSecondsForChoseVideo);
    }

    private void AfterChosedVideo()
    {
        gameStatus = GameStatus.AfterChosedVideo;

        numberChoseVideoInPlayer = 0;

        ChoseSet.SetActive(false);
        VotingSet.SetActive(true);
    }

    private void VotingForVideo(VideoDTO video)
    {
        gameStatus = GameStatus.VotingForVideo;
        ClickOnVideoPlayer(videoPlayerForVote);
        SetTimer(gameSettings.MilliSecondsVotingPerVideo);
        sliderForVote.SetValueWithoutNotify(0.15f);

        videoPlayerForVote.SetDirectAudioVolume(0, 0.15f);
        videoPlayerForVote.url = "file://" + Application.dataPath + $"/Loads/{video.id}.mp4";
        videoPlayerForVote.Prepare();
        videoPlayerForVote.Play();
        nowPlayingVoteVideo = video.id;

        heartGameObject.SetActive(true);
        if (video.owner.id == thisPlayer.id)
        {
            heartGameObject.SetActive(false);
        }
        crossGameObject.SetActive(false);
    }

    private void GivingAnotherVideo()
    {
        gameStatus = GameStatus.GivingAnotherVideo;
        ChoseSet.SetActive(true);
        VotingSet.SetActive(false);
        ClickOnVideoPlayer(videoPlayerForChose);
        foreach (Video video in thisPlayer.hand)
        {
            video.chose = false;
        }
    }

    private void AddVideo(VideoDTO videoDTO)
    {
        PlayerData playerOwner = players.FirstOrDefault(p => p.id == videoDTO.owner.id);
        if (playerOwner == thisPlayer)
        {
            Video video = new Video() { id = videoDTO.id, owner = playerOwner };
            playerOwner.hand.Add(video);
        }
    }

    private void RemoveVideo(VideoDTO videoDTO)
    {
        PlayerData playerOwner = players.FirstOrDefault(p => p.id == videoDTO.owner.id);

        if (playerOwner == thisPlayer)
        {
            Video video = playerOwner.hand.FirstOrDefault(v => v.id == videoDTO.id);
            playerOwner.hand.Remove(video);
        }
    }

    public void PreviousVideo()
    {
        if (gameStatus == GameStatus.ReadingTheQuestion || gameStatus == GameStatus.ChoseTheVideo)
        {
            numberChoseVideoInPlayer = (numberChoseVideoInPlayer - 1) % thisPlayer.hand.Count;
            if (numberChoseVideoInPlayer < 0)
            {
                numberChoseVideoInPlayer = thisPlayer.hand.Count - 1;
            }
            TurnOnVideo();
        }
    }

    public void NextVideo()
    {
        if (gameStatus == GameStatus.ReadingTheQuestion || gameStatus == GameStatus.ChoseTheVideo)
        {
            numberChoseVideoInPlayer = (numberChoseVideoInPlayer + 1) % thisPlayer.hand.Count;
            TurnOnVideo();
        }
    }

    private void TurnOnVideo()
    {
        videoPlayerForChose.url = "file://" + Application.dataPath + $"/Loads/{thisPlayer.hand[numberChoseVideoInPlayer].id}.mp4";
        sliderForChose.SetValueWithoutNotify(0.15f);
        videoPlayerForChose.SetDirectAudioVolume(0, 0.15f);
        videoPlayerForChose.Prepare();
        videoPlayerForChose.Play();
    }

    public void ClickOnVideoPlayer(VideoPlayer videoPlayer)
    {
        if (gameStatus == GameStatus.ReadingTheQuestion || gameStatus == GameStatus.ChoseTheVideo || gameStatus == GameStatus.VotingForVideo || gameStatus == GameStatus.GivingAnotherVideo)
        {
            if (videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }
            else
            {
                if (gameStatus == GameStatus.ReadingTheQuestion || gameStatus == GameStatus.ChoseTheVideo || gameStatus == GameStatus.GivingAnotherVideo)
                {
                    videoPlayer.url = "file://" + Application.dataPath + $"/Loads/{thisPlayer.hand[numberChoseVideoInPlayer].id}.mp4";
                }
                else
                {
                    videoPlayer.url = "file://" + Application.dataPath + $"/Loads/{nowPlayingVoteVideo}.mp4";
                }
                videoPlayerForChose.Prepare();
                videoPlayer.Play();
            }
        }
    }

    public void ChoseVideo()
    {
        if (gameStatus == GameStatus.ChoseTheVideo)
        {
            foreach (var video in thisPlayer.hand)
            {
                video.chose = false;
            }
            thisPlayer.hand[numberChoseVideoInPlayer].chose = true;
            Multiplayer.Instance.ChoseVideo(new VideoDTO() { id = thisPlayer.hand[numberChoseVideoInPlayer].id });
        }
    }

    public void VoteForVideo()
    {
        if (gameStatus == GameStatus.VotingForVideo)
        {
            crossGameObject.SetActive(!crossGameObject.activeSelf);
            Multiplayer.Instance.VoteForVideo(new VideoDTO() { id = nowPlayingVoteVideo });
        }
    }

    private void EndGame(List<PlayerDTO> players)
    {
        ChoseSet.SetActive(false);
        VotingSet.SetActive(false);
        EndSet.SetActive(true);

        players = players.OrderByDescending(p => p.points).ToList();
        foreach (var player in players)
        {
            PlayerCard playerCard = Instantiate(PlayerCardPrefab, EndPlayersContent.transform).GetComponent<PlayerCard>();
            playerCard.SetText(player.clientDTO.name, player.points);
        }
    }

    public void SetVolumeForVideo(float f)
    {
        videoPlayerForChose.SetDirectAudioVolume(0, f);
        videoPlayerForVote.SetDirectAudioVolume(0, f);
    }

    private void SetChosePlayerPosition(VideoPlayer player)
    {
        placeForVideoVerticalInChose.SetActive(false);
        placeForVideoHorizontalInChose.SetActive(false);
        if (player.texture.width >  player.texture.height)
        {
            player.targetTexture = choseVideoPlayerHorizontalRT;
            placeForVideoHorizontalInChose.SetActive(true);
        }
        else
        {
            player.targetTexture = choseVideoPlayerVerticalRT;
            placeForVideoVerticalInChose.SetActive(true);
        }
    }

    private void SetVotePlayerPosition(VideoPlayer player)
    {
        placeForVideoVerticalInVote.SetActive(false);
        placeForVideoHorizontalInVote.SetActive(false);
        if (player.texture.width > player.texture.height)
        {
            player.targetTexture = voteVideoPlayerHorizontalRT;
            placeForVideoHorizontalInVote.SetActive(true);
        }
        else
        {
            player.targetTexture = voteVideoPlayerVerticalRT;
            placeForVideoVerticalInVote.SetActive(true);
        }
    }

    private void SetTimer(int milliseconds)
    {
        timer = milliseconds / 1000f;
    }

    private void OnDestroy()
    {
        Multiplayer.Instance.ReadingTheQuestionEvent -= ReadingTheQuestion;
        Multiplayer.Instance.ChoseTheVideoEvent -= ChoseTheVideo;
        Multiplayer.Instance.AfterChosedVideoEvent -= AfterChosedVideo;
        Multiplayer.Instance.VotingForVideoEvent -= VotingForVideo;
        Multiplayer.Instance.GivingAnotherVideoEvent -= GivingAnotherVideo;
        Multiplayer.Instance.EndGameEvent -= EndGame;
        Multiplayer.Instance.AddVideoEvent -= AddVideo;
        Multiplayer.Instance.RemoveVideoEvent -= RemoveVideo;
    }
}
