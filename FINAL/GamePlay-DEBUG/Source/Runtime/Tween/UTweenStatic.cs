using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Tween 
{
    public static class UTweenStatic 
    {
        internal static void PlayGroupFor(this GameObject go, Func<UTween, bool> _filter, int _groupId, bool _forward, bool _replay, bool _includeInactive) 
        {
            UTween[] result = go.GetComponentsInChildren<UTween>(_includeInactive);
            if (_filter != null)
            {
                foreach (var v in result)
                {
                    if (v.Group != _groupId || _filter.Invoke(v)) continue;
                    if (!v.gameObject.activeSelf == false)
                        v.gameObject.SetActive(true);

                    if (_forward)
                        v.PlayForward(_replay);
                    else
                        v.PlayReverse(_replay);
                }
            }
            else 
            {
                foreach (var v in result)
                {
                    if (v.Group != _groupId) continue;
                    if (!v.gameObject.activeSelf == false)
                        v.gameObject.SetActive(true);

                    if (_forward)
                        v.PlayForward(_replay);
                    else
                        v.PlayReverse(_replay);
                }
            }
        }
        public static void PlayGroupAll(this GameObject go,int _groupId,bool _forward=true,bool _replay=false,bool _includeInactive = false) 
        {
            PlayGroupFor(go,null,_groupId,_forward,_replay,_includeInactive);
        }

        public static void PlayGroupWithoutLoop(this GameObject go, int _groupId, bool _forward = true, bool _replay = false, bool _includeInactive = false) 
        {
            PlayGroupFor(go, (v)=>v.Counter > -1, _groupId, _forward, _replay, _includeInactive);
        }

        public static void PlayGroupOnce(this GameObject go, int _groupId, bool _forward = true, bool _replay = false, bool _includeInactive = false)
        {
            PlayGroupFor(go, (v) => v.Counter != 0, _groupId, _forward, _replay, _includeInactive);
        }

        public static void PlayGroupRepeat(this GameObject go, int _groupId, bool _forward = true, bool _replay = false, bool _includeInactive = false)
        {
            PlayGroupFor(go, (v) => v.Counter < 1, _groupId, _forward, _replay, _includeInactive);
        }
    }
}
