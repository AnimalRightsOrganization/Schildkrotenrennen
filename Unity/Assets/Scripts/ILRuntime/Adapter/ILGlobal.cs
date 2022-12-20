using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace Client
{
    public class ILGlobal : MonoBehaviour
    {
        public static ILGlobal Instance;
        public AppDomain appdomain;

        void Awake()
        {
            Instance = this;
            appdomain = new AppDomain();
        }

        void OnApplicationQuit()
        {
            appdomain.Invoke("HotFix.Main", "Dispose", null, null);
        }

        void OnDestroy() { }

        public void GlobalInit()
        {
            StartCoroutine(LoadHotFixAssembly());
        }

        // 从ab包加载dll
        IEnumerator LoadHotFixAssembly()
        {
            yield return null;

            byte[] dll = ResManager.LoadDLL();
            var fs = new MemoryStream(dll);
            appdomain.LoadAssembly(fs, null, null);

            InitializeILRuntime();
            OnHotFixLoaded();
        }

        // 注册类、委托、跨域继承(Adaptor)
        unsafe void InitializeILRuntime()
        {
#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
            //由于Unity的Profiler接口只允许在主线程使用，为了避免出异常，需要告诉ILRuntime主线程的线程ID才能正确将函数运行耗时报告给Profiler
            appdomain.UnityMainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
#endif

#if !UNITY_EDITOR && !USE_ASSETBUNDLE
            // CLR绑定（当需要绑定代码时）
            //ILRuntime.Runtime.Generated.CLRBindings.Initialize(appdomain);
#endif

            // 这里做一些ILRuntime的注册
            appdomain.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter()); //注册跨域继承（HotFix的Class需要继承Unity工程代码的类时）
            appdomain.RegisterCrossBindingAdaptor(new CoroutineAdapter()); //注册IDisposable, IEnumerator
            appdomain.RegisterCrossBindingAdaptor(new IAsyncStateMachineAdapter()); //注册Task

            //注册litjson
            //LitJson.JsonMapper.RegisterILRuntimeCLRRedirection(appdomain);

            // 注册"空参空返回"型的委托
            appdomain.DelegateManager.RegisterMethodDelegate<ILTypeInstance>();
            appdomain.DelegateManager.RegisterDelegateConvertor<System.Predicate<ILRuntime.Runtime.Intepreter.ILTypeInstance>>((act) =>
            {
                return new System.Predicate<ILTypeInstance>((obj) =>
                {
                    return ((System.Func<ILTypeInstance, System.Boolean>)act)(obj);
                });
            });
            appdomain.DelegateManager.RegisterDelegateConvertor<UnityAction>((act) => { return new UnityAction(() => { ((System.Action)act)(); }); });
            appdomain.DelegateManager.RegisterMethodDelegate<System.Object, System.Net.Sockets.SocketAsyncEventArgs>();
            appdomain.DelegateManager.RegisterDelegateConvertor<System.EventHandler<System.Net.Sockets.SocketAsyncEventArgs>>((act) =>
            {
                return new System.EventHandler<System.Net.Sockets.SocketAsyncEventArgs>((sender, e) =>
                {
                    ((System.Action<System.Object, System.Net.Sockets.SocketAsyncEventArgs>)act)(sender, e);
                });
            });
            // 注册Dotween委托
            appdomain.DelegateManager.RegisterDelegateConvertor<DG.Tweening.TweenCallback>((act) =>
            {
                return new DG.Tweening.TweenCallback(() =>
                {
                    ((System.Action)act)();
                });
            });
            // 注册Bool委托
            appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<System.Boolean>>((act) =>
            {
                return new UnityEngine.Events.UnityAction<System.Boolean>((arg0) =>
                {
                    ((System.Action<System.Boolean>)act)(arg0);
                });
            });
            // 注册Kcp委托
            appdomain.DelegateManager.RegisterMethodDelegate<System.ArraySegment<System.Byte>, kcp2k.KcpChannel>();
            appdomain.DelegateManager.RegisterMethodDelegate<kcp2k.ErrorCode, System.String>();


            LitJson.JsonMapper.RegisterILRuntimeCLRRedirection(appdomain);
            HotFix.ILHelper.InitILRuntime(appdomain); //好像没啥用

            // HelloWorld，第一次方法调用
            //appdomain.Invoke("HotFix.Main", "Proto", gameObject, null); //实例方法
            //appdomain.Invoke("HotFix.Main", "OnProto", null, null); //静态方法
        }

        // 加载完成，调用ILR代码
        void OnHotFixLoaded()
        {
            IL_InitAdapter("UIManager");
            IL_InitAdapter("EventManager");
            IL_InitAdapter("PoolManager");

            // IL热更加载UI
            appdomain.Invoke("HotFix.Main", "Init", gameObject, GameManager.present); //static方法
        }
        void IL_InitAdapter(string adapterName)
        {
            GameObject obj = new GameObject($"IL_{adapterName}");
            obj.transform.SetParent(this.transform);
            var script = obj.AddComponent<ILMonoBehaviour>();
            script.className = adapterName;
            //Debug.Log($"{adapterName}.Run");
            script.Run();
        }

#region C#调用HotFix的方法
        public void callHotFix(string type, string method, object instance, params object[] p)
        {
            //Debug.Log("C# → HotFix");
            appdomain.Invoke(type, method, instance, p);

            //appdomain.Invoke("HotFix.PoolManager", "StaticPrint", null, null); //static
            //var manager = transform.Find("IL_PoolManager").gameObject;
            //Debug.Assert(manager);
            //appdomain.Invoke("HotFix.PoolManager", "Print", null, gameObject);
        }
#endregion

#region HotFix调用C#的方法
        public void callCsharp()
        {
            //Debug.Log("HotFix → C#");
        }
#endregion
    }
}