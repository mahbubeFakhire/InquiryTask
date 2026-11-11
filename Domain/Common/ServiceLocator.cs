using System;
using System.Threading;

namespace Domain.Common
{
    public static class ServiceLocator
    {
        private static AsyncLocal<IServiceProvider> diProxy;
        public static IServiceProvider ServiceProvider => diProxy.Value ?? throw new Exception("You should Initialize the ServiceProvider before using it.");

        static ServiceLocator()
        {
            diProxy = new AsyncLocal<IServiceProvider>();
        }
        public static void Initialize(IServiceProvider proxy)
        {
            diProxy.Value = proxy;
        }
    }
}
