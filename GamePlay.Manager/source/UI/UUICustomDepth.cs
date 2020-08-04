using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    public class UUICustomDepth : MonoBehaviour
    {
        public int Depth = 0;
        public static void SetUpward ( Transform _target, Transform _owner )
        {
            //> 使得当前UI置顶
            _target.SetAsLastSibling ();

            //> adjust custom depth uis.
            List<UUICustomDepth> outList = new List<UUICustomDepth>();
            _owner.GetComponentsInChildren<UUICustomDepth> ( false, outList );

            //> 从大到小排列，调整固定显示在底的UI
            outList.Sort ( ( a, b ) =>
            {
                return b.Depth.CompareTo ( a.Depth );
            } );

            for ( var i = 0 ; i < outList.Count ; i++ )
            {
                if ( outList[i].Depth < 0 )
                {
                    outList[i].transform.SetAsFirstSibling ();
                }
            }

            //> 从小到大排列，调整固定显示在顶部的UI
            outList.Sort ( ( a, b ) =>
            {
                return a.Depth.CompareTo ( b.Depth );
            } );

            for ( var i = 0 ; i < outList.Count ; i++ )
            {
                if ( outList[i].Depth > 0 )
                {
                    outList[i].transform.SetAsLastSibling ();
                }
            }
        }
    }
}

