using System;
using System.Runtime.CompilerServices;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

/// <summary>
/// ”√”⁄async await  ≈‰
/// </summary>
public class IAsyncStateMachineAdapter : CrossBindingAdaptor
{
    public override Type BaseCLRType => typeof(IAsyncStateMachine);

    public override Type AdaptorType => typeof(Adapter);

    public override object CreateCLRInstance(AppDomain appdomain, ILTypeInstance instance)
    {
        return new Adapter(appdomain, instance);
    }

    public class Adapter : IAsyncStateMachine, CrossBindingAdaptorType
    {
        private ILTypeInstance instance;
        private AppDomain appdomain;

        public Adapter() { }
        public Adapter(AppDomain appDomain, ILTypeInstance instance)
        {
            this.appdomain = appDomain;
            this.instance = instance;
            mMoveNext = instance.Type.GetMethod("MoveNext", 0);
            mSetStateMachine = instance.Type.GetMethod("SetStateMachine");
        }
        public ILTypeInstance ILInstance { get { return instance; } set { instance = value; } }

        IMethod mMoveNext;
        bool mMoveNextGot;
        public void MoveNext()
        {
            if (instance != null)
            {
                if (!mMoveNextGot)
                {
                    mMoveNext = instance.Type.GetMethod("MoveNext", 0);
                    mMoveNextGot = true;
                }
                if (mMoveNext != null)
                {
                    appdomain.Invoke(mMoveNext, instance, null);
                }
            }
        }

        IMethod mSetStateMachine;
        bool mSetStateMachineGot;
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            if (instance != null)
            {
                if (!mSetStateMachineGot)
                {
                    mSetStateMachine = instance.Type.GetMethod("SetStateMachine", 1);
                    mSetStateMachineGot = true;
                }
                if (mSetStateMachine != null)
                {
                    appdomain.Invoke(mSetStateMachine, instance, stateMachine);
                }
            }
        }

        public override string ToString()
        {
            IMethod m = appdomain.ObjectType.GetMethod("ToString", 0);
            m = instance.Type.GetVirtualMethod(m);
            if (m == null || m is ILMethod)
            {
                return instance.ToString();
            }

            return instance.Type.FullName;
        }
    }
}