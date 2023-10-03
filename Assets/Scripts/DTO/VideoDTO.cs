using Assets.Scripts.DTO;
using System;

namespace ChooseMemeServer.DTO
{
    [Serializable]
    public class VideoDTO
    {
        public int id;

        public ClientDTO owner;
    }
}
