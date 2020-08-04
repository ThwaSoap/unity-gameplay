using System;
using System.Collections.Generic;

namespace GamePlay.Internal
{
    class DBCommand
    {
        /// <summary>
        /// 所有并行的条件
        /// </summary>
        List<DBCondition> Conditions = new List<DBCondition>();

        /// <summary>
        /// 所有允许查找的属性
        /// </summary>
        List<IProperty> Propertys = new List<IProperty>();

        /// <summary>
        /// 是否应用首选项
        /// </summary>
        bool ApplyFirstSearch;

        /// <summary>
        /// 是否应用尾选项
        /// </summary>
        bool ApplyLastSearch;

        /// <summary>
        /// 首选项的个数
        /// </summary>
        int FirstCount;

        /// <summary>
        /// 尾选项的个数
        /// </summary>
        int LastCount;

        public int FirstSearch
        {
            get { return FirstCount; }
            set
            {
                FirstCount = value;
                ApplyFirstSearch = value > 0;
            }
        }

        public int LastSearch
        {
            get { return LastCount; }
            set
            {
                LastCount = value;
                ApplyLastSearch = value > 0;
            }
        }

        public DBCommand(List<IProperty> _propertys) 
        {
            Propertys = _propertys;
        }

        public bool SetCommand(string _command) 
        {
            _command = _command.Trim();
            int outFirstCount,outLastCount;
            ParseCommandHeader(_command, "first", out _command, out outFirstCount);
            ParseCommandHeader(_command, "last", out _command, out outLastCount);
            FirstSearch = outFirstCount;
            LastSearch = outLastCount;

            /* 这是我们解析出来的条件运算符 */
            string conditionOperator = "";

            /* 这是第一组条件，其他并行条件以单向链表形式存储在子节点下 */
            DBCondition current = null;

            /* 在我们成功解析完第一个字段数据后，就可以解析条件运算符了*/
            bool canParseOperator = false;
            while (_command != "") 
            {
                /* 条件运算符的解析 */
                if (canParseOperator)
                {
                    if (ParseConditionOperator(_command, out _command, out conditionOperator) == false)
                        return false;
                }
                else 
                {
                    canParseOperator = true;
                }

                #region 字段属性的解析
                DBCondition condition;
                IProperty outProperty;
                if (ParseProperty(_command, out _command, out outProperty) == false) 
                    return false; 

                Func<object,object,bool> outFunc;
                if (ParseSign(_command, outProperty,out _command, out outFunc) == false)  
                    return false; 

                object outValue;
                if (ParseValue(_command, outProperty, out _command, out outValue) == false) 
                    return false;
                #endregion

                #region 创建并对查询条件进行分组
                condition = new DBCondition(outProperty,outValue,outFunc);

                if (current == null)
                {
                    current = condition;
                }
                else if (conditionOperator == "," || conditionOperator == "&&")
                {
                    current.Last.SetNextCondition(condition);
                }
                else if (conditionOperator == ";" || conditionOperator == "||") 
                {
                    Conditions.Add(current);
                    current = condition;
                }
                #endregion
            }
            if (current != null) 
            {
                Conditions.Add(current);
            }
            return Conditions.Count > 0 || ApplyFirstSearch || ApplyLastSearch;
        }

        /// <summary>
        /// 解析条件运算符
        /// </summary>
        /// <param name="_command">初始命令行内容</param>
        /// <param name="_outCommand">剩余命令行内容</param>
        /// <param name="_outConditionOperator">输出的条件运算符</param>
        /// <returns>成功返回真，否则返回假</returns>
        bool ParseConditionOperator(string _command, out string _outCommand, out string _outConditionOperator)
        {
            _outCommand = _command;
            _outConditionOperator = "";
            if (_command[0] == ';' || _command[0] == ';') 
            {
                _outConditionOperator = new string(_command[0],1);
                _outCommand = _command.Remove(0,1).TrimStart();
                return true;
            }
            else if (_command.Length > 1) 
            {
                var val = _command.Substring(0,2);

                if (val == "&&" || val == "||") 
                {
                    _outCommand = _command.Remove(0,2).TrimStart();
                    _outConditionOperator = val;
                    return true;
                } 
            }
            return false;
        }

        /// <summary>
        /// 解析值
        /// </summary>
        /// <param name="_command">初始命令行内容</param>
        /// <param name="_property">作用的字段实例</param>
        /// <param name="_outCommand">剩余的命令行</param>
        /// <param name="_outValue">输出的数据实例</param>
        /// <returns>成功返回真，否则返回假</returns>
        bool ParseValue(string _command, IProperty _property, out string _outCommand, out object _outValue) 
        {
            _outCommand = _command;
            _outValue = null;

            var length = _property.CheckoutContentLength(_command);

            if (length != -1) 
            {
                var strValue = _command.Substring(0, length);

                 if (_property.TryGetValue(strValue, out _outValue)) 
                 {
                     _outCommand = _command.Remove(0, length).TrimStart();
                     return true;
                 }
            }

            return false;
        }

        /// <summary>
        /// 解析符号
        /// </summary>
        /// <param name="_command">初始命令行</param>
        /// <param name="_property">作用的字段实例</param>
        /// <param name="_outCommand">剩余的命令行</param>
        /// <param name="_outFunc">输出的比较过程</param>
        /// <returns>成功返回真，否则返回假</returns>
        bool ParseSign(string _command,IProperty _property ,out string _outCommand, out Func<object, object, bool> _outFunc) 
        {
            _outCommand = _command;
            _outFunc = null;

            foreach (var v in _property.Signs) 
            {
                if (_command.Substring(0, v.Length) == v) 
                {
                    _outCommand = _command.Remove(0,v.Length).TrimStart();
                    _property.TryGetCompare(v,out _outFunc);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 解析字段
        /// </summary>
        /// <param name="_command">初始命令行</param>
        /// <param name="_outCommand">剩余命令行</param>
        /// <param name="_outProperty">输出的字段实例</param>
        /// <returns>成功返回真，否则返回假</returns>
        bool ParseProperty(string _command, out string _outCommand,out IProperty _outProperty) 
        {
            _outCommand = _command;
            _outProperty = null;

            foreach (var v in Propertys) 
            {
                if (_command.StartsWith(v.PropertyName)) 
                {
                    _outCommand = _command.Remove(0,v.PropertyName.Length).TrimStart();
                    _outProperty = v;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 函数用来解析特殊命令头
        /// </summary>
        /// <param name="_command">原命令行内容</param>
        /// <param name="_token">命令标识</param>
        /// <param name="_outCommand">剩余的命令行</param>
        /// <param name="_outValue">输出的命令数值</param>
        /// <returns>成功返回真，否则返回假</returns>
        bool ParseCommandHeader(string _command,string _token,out string _outCommand,out int _outValue) 
        {
            _outCommand = _command;
            _outValue   = 0;
            if (_command.Length < _token.Length) return false;
             
            if (_command.Substring(0, _token.Length).ToLower() == _token.ToLower()) 
            {
                _command = _command.Remove(0, _token.Length).TrimStart();

                if (_command[0] == ':') 
                {
                    _command = _command.Remove(0,1).Trim();

                    var endIndex = _command.IndexOf(';');
                    if (endIndex != -1) 
                    {
                        var value = _command.Substring(0,endIndex);

                        /* 如果值等于 '*' 默认设置查找所有内容 */
                        if (value == "*")
                        {
                            _outValue = int.MaxValue;
                            _outCommand = _command.Remove(0, endIndex + 1).TrimStart();
                        }
                        else 
                        {
                            if (int.TryParse(value, out _outValue))
                            {
                                _outCommand = _command.Remove(0, endIndex + 1).TrimStart();
                                return true;
                            }
                        }
                    }
                }
            } 
            return false;
        }

        /// <summary>
        /// 查找
        /// </summary>
        /// <param name="_queue">被查找的队列</param>
        /// <returns>失败时返回空链表</returns>
        public List<T> Search<T>(T[] _queue) where T : class
        {
            T current;
            List<T> results = new List<T>();

            if (ApplyFirstSearch && FirstCount > 0 && FirstCount != int.MaxValue)
            {
                //> 带有首选项条件的筛选
                for (var i = 0; i < _queue.Length; i++)
                {
                    if (FirstSearching(ref results, _queue[i])) break;
                }
            }
            else
            {
                //> 全局筛选
                for (var i = 0; i < _queue.Length; i++)
                {
                    current = _queue[i];
                    for (var n = 0; n < Conditions.Count; n++)
                    {
                        if (Conditions[n].IsPass(current))
                        {
                            results.Add(current);
                            break;
                        }
                    }
                }
            }

            //> 检查并应用尾选项筛选(移除前面多余的筛选结果)
            if (ApplyLastSearch && LastCount > 0 && results.Count > LastCount)
            {
                results = results.GetRange(results.Count - LastCount, LastCount);
            }

            return results;
        }

        /// <summary>
        /// 首选项搜索
        /// </summary>
        /// <param name="_results">搜索结果</param>
        /// <param name="_target">被检查的单位</param>
        /// <returns>满足首选项条件返回真，否则返回假</returns>
        bool FirstSearching<T>(ref List<T> _results, T _target) where T : class
        {
            if (Conditions.Count == 0)
            {
                _results.Add(_target);
                return _results.Count >= FirstCount;
            }
            for (var n = 0; n < Conditions.Count; n++)
            {
                if (Conditions[n].IsPass(_target))
                {
                    _results.Add(_target);
                    return _results.Count >= FirstCount;
                }
            }
            return false;
        }
    }
}
