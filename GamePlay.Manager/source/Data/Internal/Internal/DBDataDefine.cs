using System;

namespace GamePlay.Internal
{
    class DBDataBool : DBDataType
    {
        public DBDataBool()
            : base(typeof(bool))
        {
        }
        protected override bool GetData(string _content, out object _outValue)
        {
            bool outValue;
            if (bool.TryParse(_content, out outValue))
            {
                _outValue = outValue;
                return true;
            }
            _outValue = default(bool);
            return false;
        }

        protected override int OnIndexOf(string _content)
        {
            string checkout;
            RECHECKOUT:
            if (_content.Length >= 5)
            {
                checkout = _content.Remove(5).ToLower();

                if (checkout == "false")
                {
                    return 5;
                }
                else 
                {
                    _content = checkout.Remove(4);
                    goto RECHECKOUT;
                }
            }
            else if (_content.Length == 4 && _content.ToLower() == "true")
            {
                return 4;
            }
            else return -1;
        }
    }

    abstract class DBDataNumber : DBDataType
    {
        public DBDataNumber(Type _type)
            : base(_type) 
        {
            AddCompare(">",Greater);
            AddCompare("<",Less);
            AddCompare(">=",GreaterEquals);
            AddCompare("<=",LessEquals);
            SortSigns();
        }

        int CompareBoth(object _a,object _b)
        {
            return ((IComparable)_a).CompareTo(_b);
        }

        bool Greater(object _a, object _b) 
        {
            return CompareBoth(_a,_b) > 0;
        }

        bool Less(object _a, object _b) 
        {
            return CompareBoth(_a,_b) < 0;
        }

        bool GreaterEquals(object _a, object _b) 
        {
            return CompareBoth(_a,_b) >= 0;
        }

        bool LessEquals(object _a, object _b) 
        {
            return CompareBoth(_a,_b) <= 0;
        }

        protected override int OnIndexOf(string _content)
        {
            var index = _content.IndexOf(' ');
            if (index == -1) 
            {
                index = _content.Length;
            }

            return index;
        }
    }

    class DBDataShort : DBDataNumber
    {
        public DBDataShort() : base(typeof(short)) { }
        protected override bool GetData(string _content, out object _outValue)
        {
            short outValue;
            if (short.TryParse(_content, out outValue)) 
            {
                _outValue = outValue;
                return true;
            }
            _outValue = default(short);
            return false;
        }
    }

    class DBDataInt : DBDataNumber
    {
        public DBDataInt() : base(typeof(int)) { }
        protected override bool GetData(string _content, out object _outValue)
        {
            int outValue;
            if (int.TryParse(_content, out outValue))
            {
                _outValue = outValue;
                return true;
            }
            _outValue = default(int);
            return false;
        }
    }

    class DBDataLong : DBDataNumber
    {
        public DBDataLong() : base(typeof(long)) { }
        protected override bool GetData(string _content, out object _outValue)
        {
            long outValue;
            if (long.TryParse(_content, out outValue))
            {
                _outValue = outValue;
                return true;
            }
            _outValue = default(long);
            return false;
        }
    }

    class DBDataFloat : DBDataNumber
    {
        public DBDataFloat() : base(typeof(float)) { }
        protected override bool GetData(string _content, out object _outValue)
        {
            float outValue;
            if (float.TryParse(_content, out outValue))
            {
                _outValue = outValue;
                return true;
            }
            _outValue = default(float);
            return false;
        }
    }

    class DBDataDouble : DBDataNumber
    {
        public DBDataDouble() : base(typeof(double)) { }
        protected override bool GetData(string _content, out object _outValue)
        {
            double outValue;
            if (double.TryParse(_content, out outValue))
            {
                _outValue = outValue;
                return true;
            }
            _outValue = default(double);
            return false;
        }
    }

    class DBDataEnum : DBDataNumber
    {
        public DBDataEnum(Type _type) : base(_type) { }

        protected override bool GetData(string _content, out object _outValue)
        {
            try
            {
                _outValue = Enum.Parse(DataType, _content);
                return true;
            }
            catch
            {
                _outValue = null;
                return false;
            }
        }
    }

    class DBDataString : DBDataNumber 
    {
        public DBDataString():base(typeof(string)) 
        {
            AddCompare("<<", StartWith);
            AddCompare(">>", EndWith);
            AddCompare("<>", Container);
            AddCompare("<<<", StartWithCase);
            AddCompare(">>>", EndWithCase);
            AddCompare("><", ContainerCase);
            SortSigns();
        }

        protected override bool GetData(string _content, out object _outValue)
        {
            _outValue = _content.Trim('\'');
            return true;
        }

        bool StartWith(object _a, object _b)
        {
            return ((string)_a).StartsWith((string)_b);
        }

        bool EndWith(object _a, object _b)
        {
            return ((string)_a).EndsWith((string)_b);
        }

        bool Container(object _a, object _b)
        {
            return ((string)_a).Contains((string)_b);
        }

        bool StartWithCase(object _a, object _b)
        {
            return ((string)_a).StartsWith((string)_b, StringComparison.CurrentCultureIgnoreCase);
        }

        bool EndWithCase(object _a, object _b)
        {
            return ((string)_a).EndsWith((string)_b, StringComparison.CurrentCultureIgnoreCase);
        }

        bool ContainerCase(object _a, object _b)
        {
            string a = ((string)_a).ToLower();
            string b = ((string)_b).ToLower();

            return a.Contains(b);
        }

        protected override int OnIndexOf(string _content)
        {
            if (_content.Length < 1 || _content[0] != '\'') return -1;


            for (var i = 1; i < _content.Length; i++) 
            {
                if (_content[i] == '\'' && _content[i - 1] != '\\') return i + 1;
            }

            return -1;
        }
    }
}
