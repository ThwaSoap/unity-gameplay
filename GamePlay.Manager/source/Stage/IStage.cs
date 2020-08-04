using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Internal 
{
    interface IStage
    {
        EStageStatus State { get; set; }
        EStageStatus NextState { get; set; }
        UAsyncOperation Progress { get; set; }
        UAsyncOperation Load();
        void Open();

        void Update();

        void Close();
        void Unload();
    }
}
