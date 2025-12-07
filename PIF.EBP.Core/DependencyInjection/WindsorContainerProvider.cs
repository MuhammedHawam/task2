using Castle.Windsor;
using System;

namespace PIF.EBP.Core.DependencyInjection
{
    public class WindsorContainerProvider
    {
        private static IWindsorContainer _container;
        public static IWindsorContainer Container
        {
            get
            {
                if (_container == null)
                {
                    throw new InvalidOperationException("Windsor container has not been initialized.");
                }

                return _container;
            }
            set { _container = value; }
        }
    }
}
