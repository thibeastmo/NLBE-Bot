namespace NLBE_Bot
{
    public class TankHof
    {
        public TankHof(string link, string speler, string tank, int damage, int tier)
        {
            this.link = link;
            this.speler = speler;
            this.tank = tank;
            this.damage = damage;
            this.tier = tier;
        }

        public string link { get; }
        public string speler { get; set;  }
        public string tank { get; }
        public int damage { get; }
        public int tier { get; }
        public short place { get; set; } = 1;
    }
}
