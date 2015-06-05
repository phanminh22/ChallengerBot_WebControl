﻿using System;

namespace PVPNetConnect.RiotObjects.Platform.Matchmaking
{
    public class BustedLeaver : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.matchmaking.BustedLeaver";

        public BustedLeaver()
        {
        }

        public BustedLeaver(Callback callback)
        {
            this.callback = callback;
        }

        public BustedLeaver(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(BustedLeaver result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("reasonFailed")]
        public String ReasonFailed { get; set; }

        [InternalName("accessToken")]
        public String AccessToken { get; set; }

        [InternalName("leaverPenaltyMilisRemaining")]
        public Double LeaverPenaltyMilisRemaining { get; set; }

        [InternalName("summoner")]
        public Summoner.Summoner Summoner { get; set; }
    }
}