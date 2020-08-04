namespace GamePlay
{
    using GamePlay.Internal;
    using UnityEngine;
    [System.Serializable]
    public class UUIViewInfoData
    {
        public EUIExitMode ExitMode = EUIExitMode.DelayDestroy;
        public float ExitTime;
        public bool ResetDepthUpward;
        public UUIPlay EntryEffect;
        public UUIPlay LeaveEffect;
        public string Layer = "UIRoot(2D)";
        public int Order;
    }

    [RequireComponent(typeof(RectTransform))]
    public class UUIView : MonoBehaviour, IUI
    {
        #region 内部的东西
        public UUIViewInfoData Info;
        IUIM Manager;
        bool IUI.CanDestroy { get { return CanDestroy; } }

        EUIExitMode IUI.ExitMode { get { return Info.ExitMode; } }

        float IUI.ExitTime { get { return Info.ExitTime; } }

        GameObject IUI.Root { get { return gameObject; } }

        UUIWorker IUI.Worker { get; set; }

        void IUI.Loaded(IUIM _iuim, GameObject _root)
        {
            Manager = _iuim; 
            CanDestroy = true;
            gameObject.SetActive(false);
            OnLoadCompleted();
            OnLoad();
        }

        void IUI.Unload()
        {
            OnUnload();
            Manager = null;
            Info = null;
        }

        void IUI.Downward()
        {
            OnDownward();
        }

        void IUI.Close()
        {
            this.OnPreClose();
            if (gameObject.activeSelf)
            {
                if (Info.EntryEffect != null)
                    Info.EntryEffect.Stop();

                if (Info.LeaveEffect != null)
                {
                    Info.LeaveEffect.Play(() =>
                    {
                        this.OnClose();
                        gameObject.SetActive(false);
                        CanDestroy = true;
                    });
                }
                else
                {
                    this.OnClose();
                    gameObject.SetActive(false);
                    CanDestroy = true;
                }
            }
            else
            {
                this.OnClose();
                CanDestroy = true;
            }
        }

        void IUI.Open()
        {
            CanDestroy = false;
            gameObject.SetActive(true);
            ResetDepth();
            this.OnPreOpen();

            if (Info.LeaveEffect != null)
                Info.LeaveEffect.Stop();
            if (Info.EntryEffect != null)
                Info.EntryEffect.Play(this.OnOpen);
            else
                this.OnOpen();
        }

        void IUI.Upward()
        {
            if (Info.ResetDepthUpward)
                ResetDepth();

            OnUpward();
        }

        bool CanDestroy;

        /// <summary>
        /// 在加载完成时调用，将当前UI节点挂载到Owner节点上
        /// </summary>
        void OnLoadCompleted()
        {
            var owner = Manager.FindLayer(Info.Layer);
            if (owner == null)
            {
                Manager?.Output.Error("没有找到名称为[{0}]的UI节点！", Info.Layer);
            }
            else
            {
                var uiLayout = gameObject.transform as RectTransform;
                var offsetMin = uiLayout.offsetMin;
                var offsetMax = uiLayout.offsetMax;
                gameObject.transform.SetParent(owner.transform);
                uiLayout.offsetMin = offsetMin;
                uiLayout.offsetMax = offsetMax;
                gameObject.transform.localScale = Vector3.one;
            }
        }

        /// <summary>
        /// 重置界面的深度
        /// </summary>
        void ResetDepth()
        {
            var owner = gameObject.transform.parent;

            if (owner == null)
            {
                Manager?.Output.Error("发现一个没有设置层的界面[{0}]", GetType().Name);
                return;
            }

            UUICustomDepth.SetUpward(gameObject.transform, owner);
        }

        protected virtual void OnDestroy()
        {
            if (Manager != null)
            {
                Manager.OnDestroyUI(this);
            }
        }

        #endregion

        /// <summary>
        /// 数据模型发生了变化
        /// </summary>
        protected virtual void OnLoad() {  }
        /// <summary>
        /// 在进入前调用
        /// </summary>
        protected virtual void OnPreOpen() { }
        /// <summary>
        /// 在进入后调用，如果有动画，在动画播放完毕后调用
        /// </summary>
        protected virtual void OnOpen() {  }
        /// <summary>
        /// 在置顶时调用
        /// </summary>
        protected virtual void OnUpward() {  }
        /// <summary>
        /// 在被其他界面遮挡时调用
        /// </summary>
        protected virtual void OnDownward() { }
        /// <summary>
        /// 在退出前调用
        /// </summary>
        protected virtual void OnPreClose() {  }
        /// <summary>
        /// 在退出是调用
        /// </summary>
        protected virtual void OnClose() { }
        /// <summary>
        /// 在销毁时调用
        /// </summary>
        protected virtual void OnUnload() {  }

        protected void CloseSelf(bool _isDestroy=false)
        {
            if (Manager != null)
                Manager.OnCloseUI(this,_isDestroy);
        }
    }
}
