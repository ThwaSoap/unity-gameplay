using System;

namespace GamePlay
{
    using Internal;

    public enum ELoadLevelMode 
    {
        /// <summary>
        /// 成为世界中唯一的主关卡
        /// </summary>
        SingleMainLevel,

        /// <summary>
        /// 成为附加到世界中的子关卡
        /// </summary>
        AdditiveSubLevel,

        /// <summary>
        /// 成为世界中唯一的子关卡
        /// </summary>
        SingleSubLevel,

        /// <summary>
        /// 新的附加关卡成为主关卡
        /// </summary>
        AdditiveMainLevel,

        /// <summary>
        /// 替换掉原来的主关卡，原来的主关卡会被卸载
        /// </summary>
        ReplaceMainLevel,

        /// <summary>
        /// 作为附加关卡添加到世界，并合并到主关卡中
        /// </summary>
        MergeToMainLevel,
        END
    }

    public class ULevelManager
    {
        internal ULevelContainer Container = new ULevelContainer();

        ILevelLoader DefaultLoader;

        public void SetDefaultLoader(ILevelLoader _loader) 
        {
            DefaultLoader = _loader;
        }

        public UAsyncOperation Load(ILevelLoader _loader, string _levelName, ELoadLevelMode _mode = ELoadLevelMode.SingleMainLevel, Action _finish = null) 
        {
            return _loader?.LoadLevel(_levelName, _mode,_finish);
        }

        public UAsyncOperation LoadAsync(ILevelLoader _loader, string _levelName, ELoadLevelMode _mode = ELoadLevelMode.SingleMainLevel, Action _finish = null) 
        {
            return _loader?.LoadLevelAsync(_levelName,_mode,_finish);
        }

        public UAsyncOperation Load(string _levelName, ELoadLevelMode _mode = ELoadLevelMode.SingleMainLevel, Action _finish = null) 
        {
            return Load (DefaultLoader,_levelName, _mode, _finish );
        }

        public UAsyncOperation LoadAsync(string _levelName, ELoadLevelMode _mode = ELoadLevelMode.SingleMainLevel, Action _finish = null) 
        {
            return LoadAsync(DefaultLoader,_levelName,_mode,_finish);
        }

        public void UnloadLevel(string _levelName) 
        {
            Container.UnloadByName(_levelName);
        }

        public void Dispose() 
        {
            Container.Dispose();
        }
    }
}