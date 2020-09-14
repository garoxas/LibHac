using System;

namespace LibHac.Common
{
    internal interface IEnableSharedFromThis<T> where T : class, IDisposable, IEnableSharedFromThis<T>
    {
        void SetSelfReference(ReferenceCountedDisposable<T> reference);
    }

    internal static class EnableSharedFromThisExtensions
    {
        public static ReferenceCountedDisposable<T> MakeShared<T>(this T obj) where T : class, IDisposable, IEnableSharedFromThis<T>
        {
            var shared = new ReferenceCountedDisposable<T>(obj);
            shared.Target.SetSelfReference(shared);
            return shared;
        }
    }
}
