using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    /// <summary>
    /// 这是UPlay框架启动前自定义的操作项
    /// </summary>
    public interface IStartupOperation 
    {
        /// <summary>
        /// 这个项目执行的优先级，值越小执行顺序越靠前。
        /// </summary>
        int Priority { get; }
        /// <summary>
        /// 这个项目执行的具体操作，执行在Main前面
        /// </summary>
        /// <param name="_main"></param>
        /// <returns></returns>
        IEnumerator Load(UGameManager _main);
        /// <summary>
        /// 这个项目的卸载操作，执行在Exit后面
        /// </summary>
        /// <param name="_main"></param>
        void Unload(UGameManager _main);
    }
}