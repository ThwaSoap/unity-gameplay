using System;
using UnityEngine;

namespace GamePlay
{
    public abstract class UUIRequest : UAsyncOperation
    {
        public GameObject UI { get; protected set; }
        public UUIView Script { get; protected set; }

        internal void InitScript(UUIView _script) 
        {
            Script = _script;
            LoadEvent?.Invoke(Script);
            LoadEvent = null;
        }

        internal event Action<UUIView> LoadEvent;
        public event Action<UUIView> Complete 
        {
            add 
            {
                if (Script != null)
                {
                    value?.Invoke(Script);
                }
                else 
                {
                    LoadEvent += value;
                }
            }

            remove 
            {
                LoadEvent -= value;
            }
        }
    }
}
