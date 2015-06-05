﻿using System;

namespace PVPNetConnect.RiotObjects.Platform.Matchmaking
{

    public class QueueDodger : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.matchmaking.QueueDodger";

        public QueueDodger()
        {
        }

        public QueueDodger(Callback callback)
        {
            this.callback = callback;
        }

        public QueueDodger(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(QueueDodger result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("reasonFailed")]
        public String ReasonFailed { get; set; }

        [InternalName("summoner")]
        public Summoner.Summoner Summoner { get; set; }

        [InternalName("dodgePenaltyRemainingTime")]
        public Int32 DodgePenaltyRemainingTime { get; set; }

        [InternalName("leaverPenaltyMillisRemaining")]
        public int LeaverPenaltyMillisRemaining { get; set; }

        [InternalName("accessToken")]
        public string AccessToken { get; set; }
    }
}