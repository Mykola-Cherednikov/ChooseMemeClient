using Assets.Scripts.DTO;
using System;

namespace ChooseMemeServer.DTO
{
    [Serializable]
    public class CardDTO
    {
        public int id;

        public ClientDTO owner;
    }
}
