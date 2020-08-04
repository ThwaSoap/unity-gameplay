using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Internal
{
    class UStartupManager
    {
        List<Type> OptionTypes = new List<Type>();
        List<GameObject> OptionOwners = new List<GameObject>();
        List<IStartupOperation> Opeartions = new List<IStartupOperation>();

        Type InterfaceType;
        Type MonoComponent;
        UGameManager Parent;
        public UStartupManager()
        {
            InterfaceType = typeof(IStartupOperation);
            MonoComponent = typeof(MonoBehaviour);
        }

        public void Initialized(UGameManager _parent) 
        {
            Parent = _parent;
        }

        internal void RegisterOptionByChildren(GameObject _go)
        {
            if (_go == null) return;
            Transform root = _go.transform;
            foreach (Transform t in root)
            {
                RegisterOptionObject(t.gameObject);
            }
        }

        internal void RegisterOptionTypes(Type _type)
        {
            if (!_type.IsClass) return;
            if (_type.IsAbstract) return;
            if (_type.Equals(MonoComponent)) return;
            if (_type.GetInterface(InterfaceType.FullName) == null) return;
            OptionTypes.Add(_type);
        }

        internal void RegisterOptionObject(GameObject _go)
        {
            if (null == _go) return;
            if (OptionOwners.Contains(_go)) return;

            OptionOwners.Add(_go);
        }

        internal void InitStartup()
        {
            Opeartions.Clear();

            for (int i = 0; i < OptionTypes.Count; i++)
            {
                var item = System.Activator.CreateInstance(OptionTypes[i]);
                if (item != null)
                {
                    Opeartions.Add(item as IStartupOperation);
                }
            }

            for (int i = 0; i < OptionOwners.Count; i++)
            {
                var options = OptionOwners[i].GetComponents<IStartupOperation>();
                Opeartions.AddRange(options);
            }

            Opeartions.Sort((a, b) =>
            {
                return a.Priority.CompareTo(b.Priority);
            });

            OptionTypes.Clear();
            OptionOwners.Clear();
        }

        internal IEnumerator LoadOptions()
        {
            for (var i = 0; i < Opeartions.Count; i++)
            {
                yield return Opeartions[i].Load(Parent);
            }
        }

        internal void UnloadOptions()
        {
            for (int i = Opeartions.Count - 1; i >= 0; i--)
            {
                try
                {
                    Opeartions[i].Unload(Parent);
                }
                catch { }
            }

            Opeartions.Clear();
        }
    }
}

