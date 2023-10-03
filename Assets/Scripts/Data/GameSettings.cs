using System;

namespace Assets.Scripts.Data
{
    [Serializable]
    public class GameSettings
    {
        public int MilliSecondsBeforeStart;

        public int MilliSecondsPerVideoGive;

        public int MilliSecondsReadingQuestion;

        public int MilliSecondsForChoseVideo;

        public int MilliSecondsVotingPerVideo;
    }
}
