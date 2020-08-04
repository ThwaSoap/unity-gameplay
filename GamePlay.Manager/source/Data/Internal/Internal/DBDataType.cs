using System;
using System.Collections.Generic;

namespace GamePlay.Internal
{  
    abstract class DBDataType : IDataType
    {
        #region 比较类
        class Compare
        {
            public string Sign { get; private set; }
            public Func<object, object, bool> OnCompare { get; private set; }

            public Compare(string _sign, Func<object, object, bool> _onCompare)
            {
                Sign = _sign;
                OnCompare = _onCompare;
            }
        }
        #endregion

        protected Type DataType;
        List<Compare> CompareQueue = new List<Compare>();
        List<string> Signs = new List<string>();
        public DBDataType(Type _dataType) 
        {
            DataType = _dataType;
            AddCompare("=", IsEquals);
            AddCompare("!=", NotEquals);
            SortSigns();
        } 

        protected void SortSigns() {
            Signs.Sort((a, b) => 
            {
                if (a.Length < b.Length) return 1;
                else if (a.Length > b.Length) return -1;
                else return 0;
            });
        }

        private bool IsEquals(object _a, object _b) 
        {
            return _a.Equals(_b);
        }

        private bool NotEquals(object _a, object _b) 
        {
            return !_a.Equals(_b);
        }

        protected void AddCompare(string _sign,Func<object, object, bool> _compare) 
        {
            Signs.Add(_sign); 
            CompareQueue.Add(new Compare(_sign,_compare));
        }

        abstract protected bool GetData(string _content,out object _outValue );

        protected virtual int OnIndexOf(string _content) 
        {
            return _content.IndexOf(' ');
        }

        Type IDataType.BaseType
        {
            get { return DataType; }
        }

        bool IDataType.GetValue(string _content, out object _result)
        {
            return GetData(_content,out _result);
        }

        bool IDataType.GetCompare(string _sign, out Func<object, object, bool> _outCompare)
        {
            var result = CompareQueue.Find((v) => { return v.Sign == _sign; });

            if (result != null)
            {
                _outCompare = result.OnCompare;
                return true;
            }

            _outCompare= null;
            return false;
        }

        int IDataType.CheckoutContentLength ( string _content ) 
        {
            return OnIndexOf(_content);
        }

        List<string> IDataType.Signs 
        {
            get { return Signs; }
        }
    }
}
