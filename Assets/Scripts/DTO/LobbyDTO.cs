using ChooseMemeServer.DTO;
using System;

namespace Assets.Scripts.DTO
{
    [Serializable]
    public class LobbyDTO
    {
        public int id;

        public string name;

        public int numOfClients;

        public ArrayOfClientsDTO clientsDTO;

        public int maxNumOfClients;

        public bool isHost;
    }
}
