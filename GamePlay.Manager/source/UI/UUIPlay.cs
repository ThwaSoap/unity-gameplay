using System;
using UnityEngine;

namespace GamePlay
{
    public abstract class UUIPlay : MonoBehaviour
    {
        public abstract void Play (Action _finished);
        public abstract void Stop ();
    }
}
