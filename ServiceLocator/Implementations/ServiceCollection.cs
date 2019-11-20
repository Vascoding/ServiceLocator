using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using ServiceLocator.Contracts;

namespace ServiceLocator.Implementations
{
    public class ServiceCollection : IServiceCollection
    {
        private readonly IDictionary<string, Type> transient = new Dictionary<string, Type>();

        private readonly IDictionary<string, object> singleton = new Dictionary<string, object>();

        public void AddTransient(Type contract, Type implementation)
            => this.transient.Add(contract.Name, implementation);

        public void AddSingleton(Type contract, Type implementation)
            => this.singleton.Add(contract.Name, CreateInstance(implementation));

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
                    acc.Add(this.GetInstance(curr.ParameterType));
                    return acc;
                }).ToArray();

        private object GetInstance(Type type)
            => this.CreateInstance(Assembly.GetAssembly(type)
                .GetTypes()
                .FirstOrDefault(e => type.IsAssignableFrom(e) && e.IsClass));
    }
}