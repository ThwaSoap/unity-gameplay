using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay 
{
    public interface IGameInstance 
    {
        IEnumerator StartInstance(UGameManager _manager);
        IEnumerator Open(UGameManager _manager,object _object);
        void Close(UGameManager _manager);
    }
}
