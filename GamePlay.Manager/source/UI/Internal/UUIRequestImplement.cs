using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace GamePlay.Internal
{
    using Internal; 
    class UUIRequestBy : UUIRequest
    {
        UResourceRequest Request;
        float LastProgress;

        public UUIRequestBy(UResourceRequest _request,UUIWorker _worker,Action<UUIWorker, GameObject> _callback) 
        {
            Request = _request;
            this.StartCoroutine(Start(_worker, _callback));
        }


        IEnumerator Start(UUIWorker _worker, Action<UUIWorker, GameObject> _callback) 
        {
            yield return Request; 
            var template = Request.GetAsset<GameObject>(); 
            if (template != null) 
            {
                UI = GameObject.Instantiate(template);
            }

            _callback.Invoke(_worker, UI);

            LastProgress = 0.1f;
            Request = null;
        }

        public override float Progress 
        { 
            get
            {
                if (Request == null) return 1f;

                return (Request.Progress  + LastProgress) / 1.1f; 
            } 
        }
    }

    class UUIRequestByDone : UUIRequest 
    {
        public UUIRequestByDone(GameObject _ui) 
        {
            UI = _ui;
            Progress = 1f;
        }
    }
}

