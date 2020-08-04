using UnityEngine.SceneManagement;

namespace GamePlay
{
    using Internal;
    public class ULevel
    {
        public string Name { get; private set; }
        public int Handle { get; private set; }
        public bool Loaded { get { return IsBind; } }

        #region Internal Interface
        /// <summary>
        /// IsBind=true, BindInstance有效，否则无效
        /// </summary>
        internal bool IsBind { get; private set; }
        internal Scene BindInstance { get; private set; }
        internal ELoadLevelMode LoadMode { get; private set; }
        internal UUnityLevelRequest Request { get; set; }
        internal ULevel(string _name, int _handle, ELoadLevelMode _mode)
        {
            Name = _name;
            Handle = _handle;
            LoadMode = _mode;
        }
        internal void SetBind(Scene _instance)
        {
            IsBind = true;
            BindInstance = _instance;
        }
        #endregion
    }
}