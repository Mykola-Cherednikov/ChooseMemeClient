using Assets.Scripts.DTO;
using System;

namespace ChooseMemeServer.DTO
{
    [Serializable]
    public class PlayerDTO
    {
        public ClientDTO clientDTO;

        public int points;
    }
}
