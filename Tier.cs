using System;
using System.Collections.Generic;

namespace NLBE_Bot
{
    public class Tier
    {
        public string Organisator { get; set; } = string.Empty;
        public List<Tuple<ulong, string>> Deelnemers = new List<Tuple<ulong, string>>();
        public string TierNummer { get; set; } = string.Empty;
        public string Datum { get; set; } = string.Empty;
        public int tier { get; set; } = 0;
        public List<string> uniekelingen { get; set; } = new List<string>();
        private bool editedWithRedundance = false;

        public void addDeelnemer(string naam, ulong id)
        {
            this.Deelnemers.Add(new Tuple<ulong, string>(id, naam));
        }

        public bool removeDeelnemer(ulong id)
        {
            for (int i = 0; i < this.Deelnemers.Count; i++)
            {
                if (this.Deelnemers[i].Item1.Equals(id))
                {
                    this.Deelnemers.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }
        public bool isEditedWithRedundance() { return this.editedWithRedundance; }
    }
}
