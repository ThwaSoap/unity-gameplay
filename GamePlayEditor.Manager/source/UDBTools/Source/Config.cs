using System;
using System.Collections.Generic;
using System.IO;

namespace GamePlayEditor.UDBTool
{
    class Config : Debug
    {
        Dictionary<string,string> Configs = new Dictionary<string, string>();  
 
        internal bool Initialized (string _configUrl)
        {
            Configs.Clear ();
            return TryGetCommandLine ( _configUrl, out Configs);
        }

        internal bool GetBoolConfig (string _name,bool _defaultValue = false)
        {
            string outStrValue; 
            if ( Configs.TryGetValue ( _name, out outStrValue ) )
            {
                bool outBoolValue;
                if ( bool.TryParse ( outStrValue.Trim (), out outBoolValue ) )
                {
                    return outBoolValue;
                }
            }
            return _defaultValue;
        }

        internal void SaveConfig ( string configUrl, IConfigPropertys configPropertys )
        { 
            var propertys = configPropertys.GetPropertys ();
            string content  = "";
            foreach ( var v in propertys )
            {
                content += string.Format ( "{0} = {1}\r\n", v.Key, v.Value );
            }
            File.WriteAllText(configUrl,content);
        }

        internal int GetIntConfig ( string _name, int _defaultValue = 0 )
        {
            string outStrValue;
            if ( Configs.TryGetValue ( _name, out outStrValue ) )
            {
                int outIntValue;
                if ( int.TryParse ( outStrValue.Trim (), out outIntValue ) )
                {
                    return outIntValue;
                }
            }
            return _defaultValue;
        }

        internal string GetStrConfig ( string _name, string _defaultValue = "\t" )
        {
            string outStrValue;
            if ( Configs.TryGetValue ( _name, out outStrValue ) )
            {
                return outStrValue;
            }
            return _defaultValue;
        }
        internal string GetUrl (string _name,string _defaultValue)
        {
            string outStrValue;
            Configs.TryGetValue ( _name, out outStrValue );

            if ( string.IsNullOrEmpty ( outStrValue ) )
            {
                outStrValue = _defaultValue;
            }

            if ( outStrValue.Contains ( ":\\" ) )
            {
                return outStrValue;
            }
            else
            {
                return UnityEngine.Application.dataPath + "/" + outStrValue;
            }
        }

        internal List<string> GetFilterOption (string _name)
        {
            string outStrValue;
            List<string> containerNames = new List<string>();
            if ( Configs.TryGetValue ( _name, out outStrValue ) )
            {
                if ( outStrValue != "*" )
                {
                    var result = outStrValue.Split (new char[] {','} );
                    containerNames.AddRange ( result );
                    containerNames.RemoveAll ( ( v ) => { return string.IsNullOrEmpty ( v ); } );
                }
            }
            return containerNames;
        }

        bool TryGetCommandLine ( string _fileName, out Dictionary<string, string> _outCommands )
        {
            _outCommands = new Dictionary<string, string> ();
            try
            {
                var content = File.ReadAllText(_fileName); 
                var commandLine = content.Split(new string[] {"\r\n" },StringSplitOptions.None);
                foreach ( var v in commandLine )
                {
                    if ( v.Contains ( "/*" ) && v.Contains ( "*/" ) || v.Contains ( "//" ) ) continue;
                    var kv = v.Trim().Split ( new char[] { '=' } );

                    if ( kv.Length < 2 ) continue;

                    if ( !_outCommands.ContainsKey ( kv[0].Trim () ) )
                    {
                        Msg ( "{0}\t>>\t{1}", kv[0].Trim(), kv[1].Trim () );
                        _outCommands.Add ( kv[0].Trim (), kv[1].Trim () );
                    }
                }
            }
            catch
            {
                Error ( "Load‘Config.ini’Failed！" );
                return false;
            }
            return true;
        }
    }
}


