namespace PVPNetConnect.RiotObjects.Platform.Systemstate
{
    public class ClientBeforeStart : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.systemstate.ClientBeforeStart";

        public ClientBeforeStart()
        {
        }
    }
}