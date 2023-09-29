using System;

namespace ChooseMemeServer.DTO
{
    [Serializable]
    public class ShortLobbyDTO
    {
        public int id;

        public string name;

        public int numOfClients;

        public int maxNumOfClients;
    }
}
