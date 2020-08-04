namespace GamePlayEditor.UDBTool { 
    class Debug
    { 
        #region Print
        static public void Error ( string _format, params object[] _args )
        {
            UnityEngine.Debug.LogErrorFormat (_format,_args);
        }
        static public void Log ( string _format, params object[] _args )
        {
            UnityEngine.Debug.LogFormat ( _format , _args );
        }
        static public void Warning ( string _format, params object[] _args )
        {
            UnityEngine.Debug.LogWarningFormat ( _format, _args );
        }
        static public void Msg ( string _format, params object[] _args )
        {
        }
        #endregion
    }
}


