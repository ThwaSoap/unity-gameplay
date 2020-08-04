using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace GamePlay.Internal 
{
    /// <summary>
    /// 这个是成功执行过的命令，GameCommand的会存储成功执行过的命令
    /// 并作为历史命令，共用户快速选择
    /// </summary>
    class CommandContextExecuted : ICommandContextExecuted
    {
        public ICommandContext CommandContext { get; private set; }
        public object[] Args { get; private set; }
        public string ArgsContext { get; private set; }

        public CommandContextExecuted(ICommandContext _commandContext, object[] _args, string _argsContext)
        {
            CommandContext = _commandContext;
            Args = _args;
            ArgsContext = _argsContext;
        }
    }

    class CommandContext : ICommandContext
    {
        ILog Output;
        /// <summary>
        /// 调用的函数名称
        /// </summary>
        public MethodInfo Method { get; private set; }

        /// <summary>
        /// 归属的类
        /// </summary>
        public Type ClassType { get; private set; }

        /// <summary>
        /// 函数的参数集合
        /// </summary>
        ParameterInfo[] Params;
        bool[] IsSimple;

        

        Dictionary<char, char> Tags = new Dictionary<char, char>
        {
            {'<','>' },
            {'(',')' },
            {'[',']' },
            {'{','}' }
        };

        Func<Type, string, object> Convert;
        static List<Type> SimpleTypes = new List<Type>()
        {
            typeof(char),typeof(bool),typeof(short),typeof(ushort),typeof(int),typeof(uint),typeof(long),typeof(ulong),typeof(float),typeof(double)
        }; 
        public CommandContext(ILog _output,Type _ClassType, MethodInfo _method, Func<Type, string, object> _convert)
        {
            this.Output = _output;
            this.ClassType = _ClassType;
            this.Method = _method;
            this.Convert = _convert;
            this.Params = Method.GetParameters();
            this.IsSimple = new bool[this.Params.Length];
            for (int i = 0; i < this.IsSimple.Length; i++) 
            {
                this.IsSimple[i] = SimpleTypes.Contains(this.Params[i].ParameterType);
                
                /*for (int n = 0; n < SimpleTypes.Count; n++) 
                {
                    
                    if (false==this.Params[i].ParameterType.Equals(SimpleTypes[n]))
                    {
                        this.IsSimple[i]=false;
                        break;
                    }
                }*/
            }

            this.ParamsContext = "";

            foreach (var v in this.Params)
            {
                this.ParamsContext += "[" + v.ParameterType.Name + "]\t";
            }

            if (this.ParamsContext.Length == 0)
            {
                this.ParamsContext = "(void)";
            }
        }

        public string ClassName
        {
            get { return this.ClassType.Name; }
        }

        public string MethodName
        {
            get { return this.Method.Name; }
        }

        public string ParamsContext
        {
            get;
            private set;
        }

        /// <summary>
        /// 执行一段参数并返回一个上下文对象，
        /// 该对象存储着执行过的参数内容
        /// </summary>
        /// <param name="_complexArgs"></param>
        /// <returns></returns>
        public CommandContextExecuted Execute(string _complexArgs)
        {
            string[] args = new string[this.Params.Length];

            for (var i = 0; i < this.Params.Length; i++)
            {
                string outArg;
                if (TryGetArgs(this.Params[i], ref _complexArgs, out outArg,this.IsSimple[i]))
                    args[i] = outArg;
                else
                    return null;
            }


            return this.Execute(args);
        }

        public void Execute(object[] _args)
        {
            this.Method.Invoke(null, _args);
            Output.Log("命令行:{0} ( {1} )", this.Method.Name, ObjectQueueToString(_args));
        }

        static Type strType = typeof(string);
        /// <summary>
        /// 尝试获得一个参数
        /// </summary>
        /// <param name="_parameterInfo">参数的信息</param>
        /// <param name="_context">文本内容</param>
        /// <param name="_outArg">输出的字符串参数</param>
        /// <returns>成功返回真，否则返回假</returns>
        bool TryGetArgs(ParameterInfo _parameterInfo, ref string _context, out string _outArg,bool _isSimple)
        {
            _outArg = null;
            if (_context.Length == 0) return false;
            if (_isSimple)
            {
                int index = _context.IndexOf(' ');
                if (index != -1)
                {
                    _outArg = _context.Remove(index + 1).Trim();
                    _context = _context.Substring(index).Trim();
                }
                else
                {

                    _outArg = _context.Trim();
                    _context = "";
                }
            }
            else 
            {
                if(strType== _parameterInfo.ParameterType)
                    return TryGetObjectContent(ref _context, out _outArg,'\"');
                else
                    return TryGetObjectContent(ref _context, out _outArg, '(');
            }
            return true;
        }

        bool TryGetObjectContent(ref string _context, out string _outArg,char _startTag) 
        {
            _outArg = null;
            char strTag = _startTag;
            char endTag = strTag; 
            int bit = 1;
            if (_context[0] == '@')
            {
                if (_context.Length > 2)
                {
                    strTag = _context[1];
                    bit = 2;
                }
                else return false;
            }

            if (!Tags.TryGetValue(strTag, out endTag))
            {
                endTag = strTag;
            }

            int begin = _context.IndexOf(strTag);
            if (begin != -1 && begin + 1 < _context.Length)
            {
                for (int i = begin + 1; i < _context.Length;)
                {
                    i = _context.IndexOf(endTag, i);
                    if (i == -1) break;
                    _outArg = _context.Substring(begin + 1, i - bit).Trim();
                    _context = _context.Substring(i + 1).Trim();
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// 它会将传入的字符串参数转换为实例对象并执行
        /// </summary>
        /// <param name="_args">传入的参数集合</param>
        CommandContextExecuted Execute(string[] _args)
        {
            object[] outValues;
            string outValuesStr;
            if (this.GetParamsValue(_args, out outValues, out outValuesStr))
            {
                this.Method.Invoke(null, outValues);
                Output.Log("命令行:{0} ( {1} )", this.Method.Name, outValuesStr);

                outValuesStr = "";
                foreach (var v in _args)
                    outValuesStr += v + " ";

                return new CommandContextExecuted(this, outValues, outValuesStr);
            }
            else
            {

                Output.Error("命令行:{0} 执行失败!", this.Method.Name);
                return null;
            }
        }

        string ObjectQueueToString(object[] outValues)
        {
            string result = "";

            if (null == outValues || outValues.Length == 0) return result;

            result = outValues[0].ToString();

            for (var i = 1; i < result.Length; i++)
            {
                result += "," + result[i].ToString();
            }

            return result;
        }

        bool GetParamsValue(string[] _args, out object[] _outValue, out string _outValueStr)
        {
            _outValue = null;
            _outValueStr = "";
            if (_args.Length < Params.Length) return false;
            _outValue = new object[Params.Length];

            for (var i = 0; i < Params.Length; i++)
            {
                var result = ConvertStringToObject(Params[i].ParameterType, _args[i]);
                if (i == 0)
                {
                    _outValueStr = _args[i];
                }
                else
                {
                    _outValueStr += ", " + _args[i];
                }

                if (result != null)
                {
                    _outValue[i] = result;
                }
                else
                {
                    _outValueStr = "";
                    _outValue = null;
                    return false;
                }
            }

            return true;
        }

        object ConvertStringToObject(Type _type, string _value)
        {
            if (_type == typeof(char)) 
            {
                char outValue;
                if (char.TryParse(_value, out outValue)) return outValue; 
            }
            else if (_type == typeof(bool))
            {
                bool outValue;
                if (bool.TryParse(_value, out outValue)) return outValue;
            }
            else if (_type == typeof(short))
            {
                short outValue;
                if (short.TryParse(_value, out outValue)) return outValue;
            }
            else if (_type == typeof(ushort))
            {
                ushort outValue;
                if (ushort.TryParse(_value, out outValue)) return outValue;
            }
            else if (_type == typeof(int))
            {
                int outValue;
                if (int.TryParse(_value, out outValue)) return outValue;
            }
            else if (_type == typeof(uint))
            {
                uint outValue;
                if (uint.TryParse(_value, out outValue)) return outValue;
            }
            else if (_type == typeof(long))
            {
                long outValue;
                if (long.TryParse(_value, out outValue)) return outValue;
            }
            else if (_type == typeof(float))
            {
                float outValue;
                if (float.TryParse(_value, out outValue)) return outValue;
            }
            else if (_type == typeof(double))
            {
                double outValue;
                if (double.TryParse(_value, out outValue)) return outValue;
            }
            else if (_type == typeof(string))
            {
                return _value;
            }
            else if (Convert != null)
            {
                return Convert(_type, _value);
            }
            return null;
        }
    }
}
