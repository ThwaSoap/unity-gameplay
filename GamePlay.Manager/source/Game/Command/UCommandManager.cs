using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace GamePlay.Internal 
{
    class UCommandManager
    {
        ILog Output;
        Dictionary<string, List<ICommandContext>> Commands = new Dictionary<string, List<ICommandContext>>();
        List<Type> BaseTypes = new List<Type>();
        public List<Type> CustomTypes;
        public Func<Type, string, object> OnCustomConvertCallback; 
        public UCommandManager()
        {
            BaseTypes.Add(typeof(bool));
            BaseTypes.Add(typeof(short));
            BaseTypes.Add(typeof(int));
            BaseTypes.Add(typeof(long));
            BaseTypes.Add(typeof(float));
            BaseTypes.Add(typeof(double));
            BaseTypes.Add(typeof(string));
        }

        #region 解析命令
        /// <summary>
        /// 检查这个函数是否属于命令函数
        /// </summary>
        /// <param name="_method">函数的信息</param>
        /// <returns>返回是或否</returns>
        bool IsCommandMethod(MethodInfo _method)
        {
            if (_method.IsStatic == false) return false;
            
            var paramsInfo = _method.GetParameters();

            foreach (var v in paramsInfo)
            {
                if (this.CheckoutIsBaseType(v.ParameterType) == false)
                    return false;
            }

            return true;
        }


        /// <summary>
        /// 检查这个类型是否属于基础数据类型
        /// </summary>
        /// <param name="_type">数据的类型</param>
        /// <returns>返回是或否</returns>
        bool CheckoutIsBaseType(Type _type)
        {
            foreach (var v in BaseTypes)
            {
                if (v.Equals(_type))
                    return true;
            }
            if (CustomTypes == null) return false;
            for (int i = 0; i < CustomTypes.Count; i++) 
            {
                if (CustomTypes[i].Equals(_type))  
                    return true;
            }
            return false;
        }

        object OnConvert(Type _type, string _value) 
        {
            if (OnCustomConvertCallback != null) return OnCustomConvertCallback(_type,_value);
            return null;
        }
        #endregion


        internal void SetLog(ILog _output) 
        {
            Output = _output;
        }
        public bool TryGetCommandContext(string _className, out List<ICommandContext> _outListContent)
        {
            return Commands.TryGetValue(_className, out _outListContent);
        }

        public bool TryGetCommandContext(string _className, string _contextName, out ICommandContext _outContext) 
        {
            _outContext = null;
            List<ICommandContext> outListContent;
            if (Commands.TryGetValue(_className, out outListContent)) 
            {
                for (var i = 0; i < outListContent.Count; i++)
                {
                    if (outListContent[i].MethodName == _contextName)
                    {
                        _outContext = outListContent[i];
                        break;
                    }
                }
            }
            return _outContext != null;
        }

        public void RegisterCmd(Type _classType)
        {
            if (false == _classType.IsClass)
            {
                Output.Error("{0} 不是一个类，无法进行命令注册!", _classType.Name);
                return;
            }

            var methods = _classType.GetMethods();
            List<ICommandContext> results = new List<ICommandContext>();
            foreach (var v in methods)
            {
                if (this.IsCommandMethod(v))
                {
                    results.Add(new CommandContext(Output,_classType, v, OnConvert));
                    Output.Log("注册命令->{0}.{1}", _classType.Name, v.Name);
                }
            }

            if (results.Count > 0)
            {
                this.Commands[_classType.Name] = results;
            }
            else
            {
                Output.Warning("{0}类中没有任何可以注册的命令!", _classType.Name);
            }
        }

        public void UnregisterCmd(Type _classType)
        {
            Commands.Remove(_classType.Name);
        }


        public ICommandContextExecuted Execute(ICommandContext _commandContext, string _argsContext)
        {
            if (null == _commandContext) return null;
            return (_commandContext as CommandContext).Execute(_argsContext);
        }

        public void Execute(ICommandContextExecuted _commandContextExecuted)
        {
            if (null == _commandContextExecuted && null == _commandContextExecuted.CommandContext) return;

            CommandContextExecuted executedContext = _commandContextExecuted as CommandContextExecuted;

            (_commandContextExecuted.CommandContext as CommandContext).Execute(executedContext.Args);
        }

        char[] SPLIT_TAG_1 = new char[] { ' ' }; 
        public void Exc(string _content) 
        {
            string[] result = _content.Split(SPLIT_TAG_1, 2);
            if (result.Length != 2) 
            {
                Output.Error("执行的命令格式不正确 \"{0}\"",_content);
                return;
            }

            string[] hander = result[0].Split('.'); 
            if (hander.Length != 2) 
            {
                Output.Error("执行的命令缺少类名或者函数名 \"{0}\"", _content);
                return;
            }

            ICommandContext outValue;
            if (TryGetCommandContext(hander[0], hander[1], out outValue)) 
            {
                Execute(outValue, result[1]);
            }
        }
          
        public void Dispose()
        {
            Commands.Clear(); 
            BaseTypes.Clear();
            CustomTypes = null;
        }
    }
}
