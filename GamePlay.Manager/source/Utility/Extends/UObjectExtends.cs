using System;
using System.Collections.Generic;
using UnityEngine;
namespace GamePlay
{
    public static class UObjectExtends
    {
        public static GameObject GetNode(this GameObject _obj, string _name)
        {
            if (_obj == null) return null;

            LinkedList<Transform> list = new LinkedList<Transform>();
            list.AddLast(_obj.transform);
            GameObject result = null;
            Transform node = null;
            Transform curr = null;
            while (list.Count > 0)
            {
                node = list.First.Value;
                curr = node.Find(_name);

                if (curr != null)
                {
                    result = curr.gameObject;
                    break;
                }
                else
                {
                    foreach (Transform v in node)
                    {
                        if (v.childCount > 0)
                            list.AddLast(v);
                    }
                }
                list.RemoveFirst();
            }
            if (list.Count > 0)
                list.Clear();
            list = null;
            return result;
        }

        public static bool TryGetNode(this GameObject _obj, string _name, out GameObject _outNode)
        {
            _outNode = GetNode(_obj, _name);
            return _outNode != null;
        }

        public static T GetScript<T>(this GameObject _obj, string _name)
            where T : MonoBehaviour
        {
            if (_obj == null)
                return default(T);

            var array = _obj.GetComponentsInChildren<T>(true);

            foreach (var v in array)
            {
                if (v.gameObject.name == _name)
                    return v;
            }

            return default(T);
        }

        public static bool TryGetScript<T>(this GameObject _obj, string _name, out T _outScript)
           where T : MonoBehaviour
        {
            _outScript = GetScript<T>(_obj, _name);
            return _outScript != null;
        }

        public static Component GetScript(this GameObject _obj, System.Type _type, string _name)
        {
            if (_obj == null)
                return null;

            var array = _obj.GetComponentsInChildren(_type, true);

            foreach (var v in array)
            {
                if (v.gameObject.name == _name)
                    return v;
            }

            return null;
        }
    }
}
