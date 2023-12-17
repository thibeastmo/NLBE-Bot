using System.Collections.Generic;
namespace NLBE_Bot
{
    public class GenerateTeam
    {
        public string leader { get; set; } = string.Empty;
        public List<string> deelnemers { get; set; } = new List<string>();
        public List<string> reserves { get; set; } = new List<string>();
        public int tier;

        public GenerateTeam(int tier)
        {
            this.tier = tier;
        }
    }
}
