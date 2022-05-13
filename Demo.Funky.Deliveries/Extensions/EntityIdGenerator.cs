using System;
using Demo.Funky.Deliveries.Shared;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace Demo.Funky.Deliveries.Extensions
{
    public static class EntityIdGenerator
    {
        public static EntityId GetEntityId<TModel>(string identifier) where TModel : IActor
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            return new EntityId(typeof(TModel).Name, identifier.ToUpper());
        }
    }
}