/*using ChooseMemeServer.DTO;
using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public enum CardType { 
    CardForChose,
    CardForVote
}

public class CardOld : MonoBehaviour
{
    [SerializeField]
    private VideoPlayer videoPlayer;

    [SerializeField]
    private RawImage image;

    [SerializeField]
    private Sprite UsualTexture;

    [SerializeField]
    private Sprite ChoseTexture;

    [SerializeField]
    private Sprite SendTexture;

    [SerializeField]
    private TMP_Text idText;

    [NonSerialized]
    public int id;

    public PlayerData owner;

    private bool isSelected;

    private bool isSended;

    private CardType type;

    private void Update()
    {
        if (isSended)
        {
            this.gameObject.GetComponent<Image>().sprite = SendTexture;
        }
        else if(isSelected)
        {
            this.gameObject.GetComponent<Image>().sprite = ChoseTexture;
        }
        else
        {
            this.gameObject.GetComponent<Image>().sprite = UsualTexture;
        }
    }

    public async void SetCardData(int id, PlayerData owner, CardType type)
    {
        this.id = id;
        this.owner = owner;
        this.type = type;

        idText.text = this.id.ToString();

        RenderTexture texture = new RenderTexture(576, 324, 1, RenderTextureFormat.ARGB32);

        videoPlayer.targetTexture = texture;

        image.texture = texture;

        videoPlayer.url = "file://" + Application.dataPath + $"/Loads/{this.id}.mp4";



        await Task.Delay(300);
        videoPlayer.SetDirectAudioVolume(0, 0);
        videoPlayer.Prepare();
        await Task.Delay(100);
        videoPlayer.Stop();
        videoPlayer.SetDirectAudioVolume(0, 0.15f);
    }

    public void SelectCard()
    {
        if (!isSelected)
        {
            isSelected = true;
            videoPlayer.Prepare();
            videoPlayer.Play();
            RestartPlay();
        }
        else
        {
            isSended = true;
            if (type == CardType.CardForChose)
            {
                Multiplayer.Instance.ChoseVideo(new VideoDTO() { id = this.id });
            }
            else
            {
                Multiplayer.Instance.VoteForVideo(new VideoDTO() { id = this.id});
            }
        }
    }

    public async void RestartPlay()
    {
        int timer = 15000;
        do
        {
            if (!isSelected)
            {
                return;
            }
            timer -= 100;
            await Task.Delay(100);
        }while(timer > 0);
        videoPlayer.Stop();
        await Task.Delay(100);
        videoPlayer.Prepare();
        await Task.Delay(100);
        videoPlayer.Play();
        await Task.Delay(100);
        RestartPlay();
    }

    public void UnselectCard()
    {
        isSelected = false;
        videoPlayer.Stop();
    }

    public void UnsendCard()
    {
        isSended = false;
    }
}
*/