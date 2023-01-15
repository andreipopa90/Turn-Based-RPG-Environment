using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using UnityEngine;

namespace Model.Observer
{
    public class EventManager
    {
        public List<Unit> Listeners { get; set; }

        public EventManager()
        {
            Listeners = new List<Unit>();
        }

        public void AddListener(Unit listener)
        {
            Listeners.Add(listener);
        }

        public void RemoveListener(Unit listener)
        {
            Listeners.Remove(listener);
        }

        public void Reset()
        {
            Listeners.Clear();
        }

        public void NotifyOnDeath()
        {
            foreach (var listener in Listeners.
                         Where(l => !l.UnitName.Equals("Player") && l.Affixes.Contains("Avenger")))
            {
                listener.CurrentHealth = (int)(listener.CurrentHealth * 1.5);
                listener.MaxHealth = (int)(listener.MaxHealth * 1.5);
                listener.Spe = (int) (listener.Spe * 1.5);
                listener.Affixes.Remove("Avenger");
            }
        }
    }
}