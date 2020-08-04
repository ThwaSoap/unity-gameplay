using System.Collections.Generic;

namespace GamePlay
{
    public class DBContent
    {
        public List<string> PropertysNames { get; private set; }
        protected List<DBValue> Values = new List<DBValue>();

        public DBContent() { }

        public DBContent(List<string> _propertyName)
        {
            PropertysNames = _propertyName;
            for (var i = 0; i < PropertysNames.Count; i++)
            {
                Values.Add(new DBValue());
            }
        }

        public DBValue this[string _name]
        {
            get
            {
                var index = PropertysNames.FindIndex((v) => { return v == _name; });
                if (index != -1)
                {
                    return Values[index];
                }
                else
                {
                    return null;
                }
            }
        }

        public DBValue this[int _index]
        {
            get { return Values[_index]; }
        }

        #region 内容接口实现
        
        /// <summary>
        /// 查找当前行数据是否包含 名为 _name 的字段
        /// </summary>
        /// <param name="_name">字段名称</param>
        /// <returns>有返回真，否则返回假</returns>
        public bool Container(string _name)
        {
            return PropertysNames.FindIndex((v) => { return v == _name; }) != -1;
        }
        /// <summary>
        /// 根据名称获得字段属性锁在的索引值
        /// </summary>
        /// <param name="_name">字段名称</param>
        /// <returns>失败返回-1,否则返回字段属性的索引值</returns>
        public int Index(string _name)
        {
            return PropertysNames.FindIndex((v) => { return v == _name; });
        }
        /// <summary>
        /// 获得当前数据行的长度
        /// </summary>
        public int Length { get { return Values.Count; } }

        /// <summary>
        /// 尝试获取
        /// </summary>
        /// <param name="_name"></param>
        /// <param name="_value"></param>
        /// <returns></returns>
        /// 
        public bool TryGet(string _name, out DBValue _value)
        {
            for (int i = 0; i < PropertysNames.Count; i++)
            {
                if (PropertysNames[i] == _name)
                {
                    _value = Values[i];
                    return true;
                }
            }
            _value = null;
            return false;
        }

        public DBValue Get(string _name) { return this[_name]; }
        public DBValue Get(int _id) { return this[_id]; }
        #endregion
    }
}
namespace GamePlay.Internal
{
     
    /// <summary>
    /// 动态内容
    /// </summary>
    class DBContentDynamic : DBContent 
    {
        public string ClassName { get; private set; }

        public DBContentDynamic(string _mainClassName) 
        {
            ClassName = _mainClassName;
        }

        public void AddProperty(string _propertyName,DBValue _value) 
        {
            if (_value == null) return;

            var index = this.Index(_propertyName);

            if (index != -1)
            {
                Values[index] = _value;
            }
            else 
            {
                PropertysNames.Add(_propertyName);
                Values.Add(_value);
            }
        }
    }
}
