using ChooseMemeServer.DTO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

        Testing();
    }

    private void Testing()
    {
        player.Prepare();
        player.prepareCompleted += Test;
        
    }

    private void Test(VideoPlayer player)
    {
        Debug.Log(player.texture.width);
        Debug.Log(player.texture.height);
    }
}
