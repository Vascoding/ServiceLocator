using System;

namespace ServiceLocator.Contracts
{
    public interface IServiceCollection
    {
        void AddTransient(Type contract, Type implementation);

        void AddSingleton(Type contract, Type implementation);

        object RetrieveTransient(Type contract);

        object RetrieveSingleton(Type contract);
    }
}