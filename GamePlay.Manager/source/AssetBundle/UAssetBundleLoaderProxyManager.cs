using System.Collections.Generic;

namespace GamePlay
{
    public class UAssetBundleLoaderProxyManager
    {
        IAssetBundleLoader InternalLoader;
        IAssetBundleLoader CustomLoader;
        IAssetBundleLoader FinalLoader { get { return CustomLoader != null ? CustomLoader : InternalLoader; } }
        Dictionary<string, UAssetBundleLoaderProxy> Proxies = new Dictionary<string, UAssetBundleLoaderProxy>();
        UAssetBundleLoaderProxy DefaultProxy;

        public UAssetBundleLoaderProxyManager(IAssetBundleLoader _internal) 
        {
            InternalLoader = _internal;
        }

        public void SetCustomLoader(IAssetBundleLoader _loader) 
        {
            CustomLoader = _loader;
        }

        public UAssetBundleLoaderProxy GetDefaultProxy() { return DefaultProxy; } 

        public UAssetBundleLoaderProxy GetProxy(string _name) { return Proxies[_name]; } 

        public UAssetBundleLoaderProxy this[string _name] 
        {
            get { return Proxies[_name]; }
        }

        public void CreateLoaderProxy(string _name, string _path, bool _isDefaultLoader)
        {
            UAssetBundleLoaderProxy proxy = new UAssetBundleLoaderProxy(_path, FinalLoader);
            if (Proxies.ContainsKey(_name))
            {
                var curr = Proxies[_name];
                Proxies[_name] = proxy;

                if (curr == DefaultProxy)
                {
                    DefaultProxy = proxy;
                }
            }
            else Proxies.Add(_name, proxy);

            if (DefaultProxy == null || _isDefaultLoader)
            {
                DefaultProxy = proxy;
            }
        }

        public void DeleteLoaderProxy(string _name)
        {
            if (Proxies.ContainsKey(_name))
            {
                if (Proxies[_name] == DefaultProxy)
                {
                    DefaultProxy = null;
                }
                Proxies.Remove(_name);
            }
        }

        public void Clear() 
        {
            foreach (var v in Proxies) 
            { v.Value.Clear(); }
            Proxies.Clear();
        }
    }
}
