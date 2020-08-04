using System;
using System.Collections.Generic;

namespace GamePlay
{
    public class UDBPropertyTitle
    {
        public readonly string Name;
        public readonly string TypeName; 

        public UDBPropertyTitle(string _name, string _typeName) 
        {
            this.Name = _name;
            this.TypeName = _typeName; 
        }
    }

    public class UDBPropertyTitleDB : UDBPropertyTitle
    {
        Func<object, object> OnGetValue;
        public UDBPropertyTitleDB(string _name, string _typeName,Func<object,object> _Value) :base(_name,_typeName)
        {
            this.OnGetValue = _Value;
        }

        public string GetStringValue(object _target) 
        {
            return OnGetValue(_target).ToString();
        }
    }
}

namespace GamePlay.Internal
{
    interface IProperty
    {
        /// <summary>
        /// 获取目标对象的属性值
        /// </summary>
        /// <param name="_target">目标对象</param>
        /// <returns></returns>
        object PropertyValue(object _target);

        /// <summary>
        /// 获取字段名称
        /// </summary>
        string PropertyName { get; }

        /// <summary>
        /// 获取字段类型
        /// </summary>
        Type PropertyType { get; }

        /// <summary>
        /// 字段的名称
        /// </summary>
        string PropertyTypeName { get; }

        /// <summary>
        /// 尝试获得该数据类型对应的实例值
        /// </summary>
        /// <param name="_content">文本内容</param>
        /// <param name="_result">获取结果</param>
        /// <returns> 成功返回真，否则返回假 </returns>
        bool TryGetValue(string _content, out object _result);

        /// <summary>
        /// 尝试获取该对象可以参与的比较函数
        /// </summary>
        /// <param name="_sign">比较符号</param>
        /// <param name="_outCompare">输出结果</param>
        /// <returns>成功返回真，否则返回假</returns>
        bool TryGetCompare(string _sign, out Func<object, object, bool> _outCompare);

        int CheckoutContentLength(string _content);

        List<string> Signs { get; }
    }
}
