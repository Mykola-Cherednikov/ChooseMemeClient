using ChooseMemeServer.DTO;
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

public class Card : MonoBehaviour
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

        RenderTexture texture = new RenderTexture(256, 256, 1, RenderTextureFormat.ARGB32);

        videoPlayer.targetTexture = texture;

        image.texture = texture;

        videoPlayer.url = "file://" + Application.dataPath + $"/Loads/{this.id}.mp4";

        videoPlayer.Prepare();
        await Task.Delay(100);
        videoPlayer.Stop();
    }

    public void SelectCard()
    {
        if (!isSelected)
        {
            isSelected = true;
            videoPlayer.Prepare();
            videoPlayer.Play();
        }
        else
        {
            isSended = true;
            if (type == CardType.CardForChose)
            {
                Multiplayer.Instance.ChoseCard(new CardDTO() { id = this.id });
            }
            else
            {
                Multiplayer.Instance.VoteForCard(new CardDTO() { id = this.id});
            }
        }
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
