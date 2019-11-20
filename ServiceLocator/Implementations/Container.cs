using System;
using System.Linq;
using System.Reflection;
using ServiceLocator.Contracts;

namespace ServiceLocator.Implementations
{
    public static class Container
    {
        private static IServiceCollection serviceCollection;

        public static void Init(IServiceCollection serviceCollection)
            => Container.serviceCollection = serviceCollection;

        public static void RegisterServices<TMap>()
            => Assembly
                .GetAssembly(typeof(TMap))
                .GetTypes()
                .Where(t => t.IsClass && t.GetInterface($"I{t.Name}") != null)
                .Select(t => new
                {
                    Interface = t.GetInterface($"I{t.Name}"),
                    Implementation = t
                })
                .ToList()
                .ForEach(t => 
                {
                    if (t.Implementation.GetInterface(nameof(ISingleton)) != null)
                        RegisterSingleton(t.Interface, t.Implementation);
                    if (t.Implementation.GetInterface(nameof(ITransient)) != null)
                        RegisterTransient(t.Interface, t.Implementation);
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