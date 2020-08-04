using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Internal
{
    /// <summary>
    /// 命令行接口
    /// </summary>
    interface ICommandContext
    {
        /// <summary>
        /// 所属的类名
        /// </summary>
        string ClassName { get; }
        /// <summary>
        /// 函数名称
        /// </summary>
        string MethodName { get; }

        /// <summary>
        /// 命令参数
        /// 输出示例:[Boolean] [Int32] [String]
        /// </summary>
        string ParamsContext { get; }
    }

    /// <summary>
    /// 历史命令接口
    /// </summary>
    interface ICommandContextExecuted
    {
        /// <summary>
        /// 命令对象
        /// </summary>
        ICommandContext CommandContext { get; }

        /// <summary>
        /// 参数内容：用户手动输入的内容
        /// </summary>
        string ArgsContext { get; }
    }
}