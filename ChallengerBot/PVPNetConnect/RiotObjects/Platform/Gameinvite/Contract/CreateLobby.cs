namespace PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract
{
    public class CreateLobby : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.gameinvite.contract.CreateLobby";

        public CreateLobby()
        {
        }
    }
}