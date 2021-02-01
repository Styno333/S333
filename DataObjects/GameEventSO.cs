using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace S333
{
    [CreateAssetMenu(fileName = "New GameEvent", menuName = "S333/GameEvent")]
    public class GameEventSO : ScriptableObject
    {

        private List<IGameEventListener> listeners = new List<IGameEventListener>();

        public void Raise()
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
                listeners[i].GameEventHandler();
        }

        public void Register(IGameEventListener listener)
        {
            listeners.Add(listener);
        }

        public void UnRegister(IGameEventListener listener)
        {
            listeners.Remove(listener);
        }
    }
}

