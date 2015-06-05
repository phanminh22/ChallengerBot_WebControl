using System.IO;

namespace ChallengerBot.SWF.SWFTypes
{
    public class End : Tag
    {
        public End()
        {
            TagCode = (int) TagCodes.End;
        }

        public override void ReadData(byte version, BinaryReader binaryReader)
        {
            var rh = new RecordHeader();
            rh.ReadData(binaryReader);
        }
    }
}