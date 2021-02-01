using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace S333
{
    public class GameEventListener : MonoBehaviour, IGameEventListener
    {
        public GameEventSO Event;
        public UnityEvent Response;

        public void GameEventHandler()
        {
            Response.Invoke();
        }

        private void OnEnable()
        {
            if (Event == null) return;
            Event.Register(this);
        }

        private void OnDisable()
        {
            if (Event == null) return;
            Event.Register(this);
        }
    }

    public interface IGameEventListener
    {
        void GameEventHandler();
    }
}

