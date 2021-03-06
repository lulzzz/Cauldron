﻿using Cauldron.Interception;
using System;
using System.Reflection;

namespace Fody_Assembly_Validation_External_Resources
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class ExternalLockableMethodAttribute : Attribute, IMethodInterceptor, ISyncRoot
    {
        public object SyncRoot { get; set; }

        public void OnEnter(Type declaringType, object instance, MethodBase methodbase, object[] values)
        {
            if (SyncRoot == null)
                throw new ArgumentNullException(nameof(SyncRoot));

            lock (SyncRoot)
            {
            }
        }

        public void OnException(Exception e)
        {
        }

        public void OnExit()
        {
        }
    }
}