using System;
using System.Collections.Generic;

namespace GamePlay.Internal
{
    interface IDataType
    {
        /// <summary>
        /// 获取数据的类型
        /// </summary>
        Type BaseType { get; }

        /// <summary>
        /// 检查内容的长度
        /// </summary>
        /// <param name="_content">被检查的内容</param>
        /// <returns> 内容有效时返回其索引值，否则返回-1 </returns>
        int CheckoutContentLength(string _content);

        /// <summary>
        /// 获取数据的值
        /// </summary>
        /// <param name="_content">文本内容</param>
        /// <param name="_result">值</param>
        /// <returns>成功返回真，否则返回假</returns>
        bool GetValue(string _content, out object _result);

        /// <summary>
        /// 根据比较符号获取比较过程
        /// </summary>
        /// <param name="_sign">比较符号</param>
        /// <param name="_outCompare">输出的比较过程</param>
        /// <returns>成功返回真，否则返回假</returns>
        bool GetCompare(string _sign, out Func<object, object, bool> _outCompare);

        List<string> Signs { get; }
    }
}
