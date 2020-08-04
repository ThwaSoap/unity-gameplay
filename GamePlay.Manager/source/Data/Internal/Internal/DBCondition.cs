using System;

namespace GamePlay.Internal
{
    /// <summary>
    /// 条件类
    /// </summary>
    class DBCondition
    {
        Object Value;
        Func<object, object, bool> Compare;
        IProperty Property;
        DBCondition NextCondition;

        public DBCondition(IProperty _property, object _value, Func<object, object, bool> _compare)
        {
            Property = _property;
            Value = _value;
            Compare = _compare;
        }


        public void SetNextCondition(DBCondition _nextCondition)
        {
            NextCondition = _nextCondition;
        }

        /// <summary>
        /// 获得最后一个条件
        /// </summary>
        public DBCondition Last
        {
            get
            {
                DBCondition c = this;
                while (c.NextCondition != null)
                {
                    c = c.NextCondition;
                }
                return c;
            }
        }

        /// <summary>
        /// 判断这个家伙的属性是否满足指定的值
        /// </summary>
        /// <param name="_target"> 目标对象 </param>
        /// <returns> 所有条件满足返回真，否则返回假 </returns>
        public bool IsPass(object _target)
        {
            if (Compare(Property.PropertyValue(_target), Value))
            {
                if (NextCondition != null)
                    return NextCondition.IsPass(_target);
                else
                    return true;
            }
            else
            {
                return false;
            }
        }
    } 
}
