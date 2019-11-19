using System;
using System.Linq;
using System.Reflection;
using ServiceLocator.Contracts;

namespace ServiceLocator.Implementation
{
    public static class Container
    {
        private static IServiceCollection serviceCollection;

        public static void Init(IServiceCollection serviceCollection)
            => Container.serviceCollection = serviceCollection;

        public static void RegisterServices()
            => Assembly
                .GetEntryAssembly()
                .GetTypes()
                .Where(t => t.IsClass && t.GetInterface($"I{t.Name}") != null)
                .Select(c => new
                {
                    Interface = c.GetInterface($"I{c.Name}"),
                    Implementation = c
                })
                .ToList()
                .ForEach(e => 
                {
                    if (e.Implementation.GetInterface(nameof(ISingleton)) != null)
                    {
                        RegisterSingleton(e.Interface, e.Implementation);
                    }
                    else
                    {
                        RegisterTransient(e.Interface, e.Implementation);
                    }
                });

        public static T RetrieveTransient<T>()
            => (T)serviceCollection.RetrieveTransient(typeof(T));

        public static T RetrieveSingleton<T>()
            => (T)serviceCollection.RetrieveSingleton(typeof(T));

        public static void RegisterTransient<TInterface, TImplementation>()
            => RegisterTransient(typeof(TInterface), typeof(TImplementation));

        public static void RegisterTransient(Type contract, Type implementation)
            => serviceCollection.AddTransient(contract, implementation);

        public static void RegisterSingleton<TInterface, TImplementation>()
            => RegisterSingleton(typeof(TInterface), typeof(TImplementation));

        public static void RegisterSingleton(Type contract, Type implementation)
            => serviceCollection.AddSingleton(contract, implementation);
    }
}