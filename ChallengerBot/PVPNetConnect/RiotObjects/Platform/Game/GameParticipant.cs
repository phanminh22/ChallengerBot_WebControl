using System;

namespace PVPNetConnect.RiotObjects.Platform.Game
{
    public class GameParticipant : Participant
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.game.GameParticipant";

        public GameParticipant()
        {
        }

        public GameParticipant(Callback callback)
        {
            this.callback = callback;
        }

        public GameParticipant(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public new delegate void Callback(GameParticipant result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("pickTurn")]
        public Int32 TimeAddedToQueue { get; set; }

        [InternalName("summonerInternalName")]
        public String SummonerInternalName { get; set; }

        [InternalName("summonerName")]
        public String SummonerName { get; set; }
    }
}
