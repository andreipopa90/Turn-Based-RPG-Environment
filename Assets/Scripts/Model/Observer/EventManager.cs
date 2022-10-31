using System.Collections.Generic;
using System.Linq;

namespace Model.Observer
{
    public class EventManager
    {
        private List<Unit> Listeners { get; set; }

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
                         Where(l => l.Ailments.Contains("Avenger") && !l.UnitName.Equals("Player")))
            {
                listener.CurrentHealth += listener.MaxHealth * 3 / 2;
                listener.MaxHealth *= 3 / 2;
                listener.Spe *= 3 / 2;
            }
        }
    }
}