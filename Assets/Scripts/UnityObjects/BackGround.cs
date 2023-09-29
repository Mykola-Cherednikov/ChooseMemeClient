using ChooseMemeServer.DTO;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Video;

public class BackGround : MonoBehaviour
{
    [SerializeField]
    private VideoClip clip;

    [SerializeField]
    private VideoClip clip2;

    [SerializeField]
    private GameObject ErrorMessagePrefab;
    [SerializeField]
    private GameObject Canvas;

    private VideoPlayer player;
    private AudioSource audioSource;

    private void Start()
    {
        player = GetComponent<VideoPlayer>();
        audioSource = GetComponent<AudioSource>();
        //Multiplayer.Instance.StartingGameEvent += StartGame;

        //ErrorMessage errorMessage = Instantiate(ErrorMessagePrefab, Canvas.transform).GetComponent<ErrorMessage>();
        //errorMessage.SetErrorMessage(Application.dataPath);

        player.gameObject.SetActive(false);
        player.gameObject.SetActive(true);
    }
    
    private void StartGame(StartGameDTO startGameDTO)
    {
        player.clip = clip;
        player.EnableAudioTrack(0, true);
        player.SetDirectAudioVolume(0, 0.15f);
        player.gameObject.SetActive(false);
        player.gameObject.SetActive(true);
    }
}
