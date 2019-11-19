using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using ServiceLocator.Contracts;

namespace ServiceLocator.Implementation
{
    public class ServiceCollection : IServiceCollection
    {
        private IDictionary<string, Type> transient = new Dictionary<string, Type>();

        private IDictionary<string, object> singleton = new Dictionary<string, object>();

        public void AddTransient(Type interfaceType, Type implementationType)
            => this.transient.Add(interfaceType.Name, implementationType);

        public void AddSingleton(Type interfaceType, Type implementationType)
            => this.singleton.Add(interfaceType.Name, CreateInstance(implementationType));
        
        public object RetrieveTransient(Type type)
            => this.CreateInstance(transient[type.Name]);

        public object RetrieveSingleton(Type type)
            => this.singleton[type.Name];

        private object CreateInstance(Type type)
            => Assembly
                .GetAssembly(type)
                .CreateInstance(type.FullName, false, BindingFlags.CreateInstance, null,
                this.ResolveDependencies(type), CultureInfo.CurrentCulture, new object[] { });

        private object[] ResolveDependencies(Type type)
            => Assembly
                .GetAssembly(type)
                .GetType(type.FullName)
                .GetConstructors()
                .FirstOrDefault()
                .GetParameters()
                .Aggregate(new List<object>(), (acc, curr) =>
                {
                    acc.Add(GetInstance(curr.ParameterType));
                    return acc;
                }).ToArray();

        private object GetInstance(Type type)
            => CreateInstance(Assembly.GetAssembly(type)
                .GetTypes()
                .FirstOrDefault(e => type.IsAssignableFrom(e) && e.IsClass));
    }
}