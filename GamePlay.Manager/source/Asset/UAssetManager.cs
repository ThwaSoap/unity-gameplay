namespace GamePlay
{
    using Internal;
    public class UAssetManager
    {
        public UAssetLoader Loader = new UResourceLoader();
        
        public void Initialized(ULevelManager _levelMgr) 
        {
            (Loader as UResourceLoader).Init(_levelMgr);
        }
    }
}
