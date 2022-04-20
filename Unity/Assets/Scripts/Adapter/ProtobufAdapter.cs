//适配文件放到主程序中
using System;
using System.Collections;
using System.Collections.Generic;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.CLR.Method;
using Google.Protobuf;
using Google.Protobuf.Reflection;

public class ProtobufAdapter : CrossBindingAdaptor
{
    static CrossBindingFunctionInfo<ILTypeInstance> mget_Value_Item = new CrossBindingFunctionInfo<ILTypeInstance>("get_Item"); //function有返回值。T IList<T>.this[int index].get
    static CrossBindingMethodInfo mset_Value_Item = new CrossBindingMethodInfo("set_Item"); //method没有返回值 void IList<T>.this[int index].set
    static CrossBindingFunctionInfo<int> mget_Value_Count = new CrossBindingFunctionInfo<int>("get_Count");
    static CrossBindingFunctionInfo<bool> mget_Value_IsReadOnly = new CrossBindingFunctionInfo<bool>("get_IsReadOnly");

    public override Type BaseCLRType
    {
        get
        {
            return null;
        }
    }

    public override Type[] BaseCLRTypes
    {
        get
        {
            return new Type[] {
                typeof(IEquatable<ILTypeInstance>),
                typeof(IComparable<ILTypeInstance>),
                typeof(IEnumerable<System.Byte>),
                typeof(IEnumerable),
                typeof(IEnumerable<ILTypeInstance>),
                typeof(ICollection<ILTypeInstance>),
                typeof(System.Collections.Generic.IList<ILTypeInstance>),
            };
        }
    }

    public override Type AdaptorType
    {
        get
        {
            return typeof(Adaptor);
        }
    }

    public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
    {
        return new Adaptor(appdomain, instance);
    }

    internal class Adaptor :
        IEquatable<ILTypeInstance>,
        IComparable<ILTypeInstance>,
        IEnumerable<System.Byte>,
        IEnumerable,
        IEnumerable<ILTypeInstance>,
        ICollection<ILTypeInstance>,
        IList<ILTypeInstance>,
        CrossBindingAdaptorType
    {
        ILTypeInstance instance;
        ILRuntime.Runtime.Enviorment.AppDomain appdomain;

        public Adaptor() { }
        public Adaptor(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            this.appdomain = appdomain;
            this.instance = instance;
        }

        public object[] data1 = new object[1];

        public ILTypeInstance ILInstance { get { return instance; } }

        #region IEquatable
        IMethod mEquals = null;
        bool mEqualsGot = false;
        public bool Equals(ILTypeInstance other)
        {
            if (!mEqualsGot)
            {
                mEquals = instance.Type.GetMethod("Equals", 1);
                if (mEquals == null)
                {
                    mEquals = instance.Type.GetMethod("System.IEquatable.Equals", 1);
                }
                mEqualsGot = true;
            }
            if (mEquals != null)
            {
                data1[0] = other;
                return (bool)appdomain.Invoke(mEquals, instance, data1);
            }
            return false;
        }
        #endregion

        #region IComparable
        IMethod mCompareTo = null;
        bool mCompareToGot = false;
        public int CompareTo(ILTypeInstance other)
        {
            if (!mCompareToGot)
            {
                mCompareTo = instance.Type.GetMethod("CompareTo", 1);
                if (mCompareTo == null)
                {
                    mCompareTo = instance.Type.GetMethod("System.IComparable.CompareTo", 1);
                }
                mCompareToGot = true;
            }
            if (mCompareTo != null)
            {
                data1[0] = other;
                return (int)appdomain.Invoke(mCompareTo, instance, data1);
            }
            return 0;
        }
        #endregion

        #region IEnumerable<System.Byte>
        public IEnumerator<byte> GetEnumerator()
        {
            IMethod method = null;
            method = instance.Type.GetMethod("GetEnumerator", 0);
            if (method == null)
            {
                method = instance.Type.GetMethod("System.Collections.IEnumerable.GetEnumerator", 0);
            }
            if (method != null)
            {
                var res = appdomain.Invoke(method, instance, null);
                return (IEnumerator<byte>)res;
            }
            return null;
        }
        #endregion

        #region IEnumerable
        IEnumerator IEnumerable.GetEnumerator()
        {
            IMethod method = null;
            method = instance.Type.GetMethod("GetEnumerator", 0);
            if (method == null)
            {
                method = instance.Type.GetMethod("System.Collections.IEnumerable.GetEnumerator", 0);
            }
            if (method != null)
            {
                var res = appdomain.Invoke(method, instance, null);
                return (IEnumerator)res;
            }
            return null;
        }
        #endregion

        #region IList
        public ILTypeInstance this[int index]
        {
            get
            {
                if (mget_Value_Item.CheckShouldInvokeBase(this.instance))
                    return this.instance;
                else
                    return mget_Value_Item.Invoke(this.instance);
            }
            set
            {
                if (mset_Value_Item.CheckShouldInvokeBase(this.instance))
                    this.instance = value;
                else
                    mset_Value_Item.Invoke(this.instance);
            }
        }

        IMethod mIndexOf = null;
        bool mIndexOfGot = false;
        public int IndexOf(ILTypeInstance other)
        {
            if (!mIndexOfGot)
            {
                mIndexOf = instance.Type.GetMethod("IndexOf", 1);
                if (mIndexOf == null)
                {
                    mIndexOf = instance.Type.GetMethod("System.Collections.Generic.IList.IndexOf", 1);
                }
                mIndexOfGot = true;
            }
            if (mIndexOf != null)
            {
                return (int)appdomain.Invoke(mIndexOf, instance, other);
            }
            return 0;
        }

        IMethod mInsert = null;
        bool mInsertGot = false;
        public void Insert(int index, ILTypeInstance other)
        {
            if (!mInsertGot)
            {
                mInsert = instance.Type.GetMethod("Insert", 2);
                if (mInsert == null)
                {
                    mInsert = instance.Type.GetMethod("System.Collections.Generic.IList.Insert", 2);
                }
                mInsertGot = true;
            }
            if (mInsert != null)
            {
                appdomain.Invoke(mInsert, instance, index, other);
            }
        }

        IMethod mRemoveAt = null;
        bool mRemoveAtGot = false;
        public void RemoveAt(int index)
        {
            if (!mRemoveAtGot)
            {
                mRemoveAt = instance.Type.GetMethod("RemoveAt", 1);
                if (mRemoveAt == null)
                {
                    mRemoveAt = instance.Type.GetMethod("System.Collections.Generic.IList.RemoveAt", 1);
                }
                mRemoveAtGot = true;
            }
            if (mRemoveAt != null)
            {
                appdomain.Invoke(mRemoveAt, instance, index);
            }
        }

        public int Count
        {
            get
            {
                if (mget_Value_Count.CheckShouldInvokeBase(this.instance))
                    return this.Count;
                else
                    return mget_Value_Count.Invoke(this.instance);
                /*
                IMethod method = null;
                method = instance.Type.GetMethod("get_Count", 0);
                if (method == null)
                {
                    method = instance.Type.GetMethod("System.Collections.Generic.ICollection.get_Count", 0);
                }
                if (method != null)
                {
                    var res = appdomain.Invoke(method, instance, null);
                    return (int)res;
                }
                return 0;
                */
            }
        }

        public bool IsReadOnly
        {
            get
            {
                if (mget_Value_IsReadOnly.CheckShouldInvokeBase(this.instance))
                    return this.IsReadOnly;
                else
                    return mget_Value_IsReadOnly.Invoke(this.instance);
                /*
                IMethod method = null;
                method = instance.Type.GetMethod("get_IsReadOnly", 0);
                if (method == null)
                {
                    method = instance.Type.GetMethod("System.Collections.Generic.ICollection.get_IsReadOnly", 0);
                }
                if (method != null)
                {
                    var res = appdomain.Invoke(method, instance, null);
                    return (bool)res;
                }
                return false;
                */
            }
        }

        IMethod mAdd = null;
        bool mAddGot = false;
        public void Add(ILTypeInstance other)
        {
            if (!mAddGot)
            {
                mAdd = instance.Type.GetMethod("Add", 1);
                if (mAdd == null)
                {
                    mAdd = instance.Type.GetMethod("System.Collections.Generic.ICollection.Add", 1);
                }
                mAddGot = true;
            }
            if (mAdd != null)
            {
                appdomain.Invoke(mAdd, instance, other);
            }
        }

        IMethod mClear = null;
        bool mClearGot = false;
        public void Clear()
        {
            if (!mClearGot)
            {
                mClear = instance.Type.GetMethod("Clear", 0);
                if (mClear == null)
                {
                    mClear = instance.Type.GetMethod("System.Collections.Generic.ICollection.Clear", 0);
                }
                mClearGot = true;
            }
            if (mClear != null)
            {
                appdomain.Invoke(mClear, instance, null);
            }
        }

        IMethod mContains = null;
        bool mContainsGot = false;
        public bool Contains(ILTypeInstance other)
        {
            if (!mContainsGot)
            {
                mContains = instance.Type.GetMethod("Contains", 1);
                if (mContains == null)
                {
                    mContains = instance.Type.GetMethod("System.Collections.Generic.ICollection.Contains", 1);
                }
                mContainsGot = true;
            }
            if (mContains != null)
            {
                return (bool)appdomain.Invoke(mContains, instance, other);
            }
            return false;
        }

        IMethod mCopyTo = null;
        bool mCopyToGot = false;
        public void CopyTo(ILTypeInstance[] other, int arrayIndex)
        {
            if (!mCopyToGot)
            {
                mCopyTo = instance.Type.GetMethod("CopyTo", 2);
                if (mCopyTo == null)
                {
                    mCopyTo = instance.Type.GetMethod("System.Collections.Generic.ICollection.CopyTo", 2);
                }
                mCopyToGot = true;
            }
            if (mCopyTo != null)
            {
                appdomain.Invoke(mCopyTo, instance, other, arrayIndex);
            }
        }

        IMethod mRemove = null;
        bool mRemoveGot = false;
        public bool Remove(ILTypeInstance other)
        {
            if (!mRemoveGot)
            {
                mRemove = instance.Type.GetMethod("Remove", 1);
                if (mRemove == null)
                {
                    mRemove = instance.Type.GetMethod("System.Collections.Generic.ICollection.Remove", 1);
                }
                mRemoveGot = true;
            }
            if (mRemove != null)
            {
                return (bool)appdomain.Invoke(mRemove, instance, other);
            }
            return false;
        }

        IEnumerator<ILTypeInstance> IEnumerable<ILTypeInstance>.GetEnumerator()
        {
            IMethod method = null;
            method = instance.Type.GetMethod("GetEnumerator", 0);
            if (method == null)
            {
                method = instance.Type.GetMethod("System.Collections.Generic.IEnumerable.GetEnumerator", 0);
            }
            if (method != null)
            {
                var res = appdomain.Invoke(method, instance, null);
                return (IEnumerator<ILTypeInstance>)res;
            }
            return null;
        }
        #endregion
    }
}