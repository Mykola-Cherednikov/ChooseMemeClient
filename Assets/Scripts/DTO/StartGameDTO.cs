using Assets.Scripts.DTO;
using System;

namespace ChooseMemeServer.DTO
{
    [Serializable]
    public class StartGameDTO
    {
        public ClientDTO you;

        public ArrayOfClientsDTO clientsDTO;
    }
}
