using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace ALMPOC.CRM.Plugins.Helpers
{
    public static class CrmEntityExtensions
    {
        #region Entity Extensions

        public static Guid? GetEntityReferenceId(this Entity entity, string valueField)
        {
            var attribute = entity.GetAttributeValue<EntityReference>(valueField);

            return attribute != null ? attribute.Id : (Guid?)null;
        }

        public static string GetEntityReferenceName(this Entity entity, string valueField)
        {
            var attribute = entity.GetAttributeValue<EntityReference>(valueField);

            return attribute != null ? attribute.Name : null;
        }

        public static int? GetOptionSetValue(this Entity entity, string valueField)
        {
            var attribute = entity.GetAttributeValue<OptionSetValue>(valueField);

            return attribute != null ? attribute.Value : default(int?);
        }

        public static decimal? GetMoneyValue(this Entity entity, string valueField)
        {
            var attribute = entity.GetAttributeValue<Money>(valueField);

            return attribute != null ? attribute.Value : default(decimal?);
        }

        public static DateTime? GetLocalDateTime(this Entity entity, string valueField)
        {
            var attribute = entity.GetAttributeValue<DateTime?>(valueField);
            return attribute;
            //return attribute != null ? attribute.Value.ToLocalTime() : (DateTime?)null;
        }

        public static T GetAliasedValue<T>(this Entity entity, string valueField)
        {
            var attribute = entity.GetAttributeValue<AliasedValue>(valueField);

            return attribute != null ? (T)attribute.Value : default(T);
        }

        public static int? GetAliasedOptionSetValue(this Entity entity, string valueField)
        {
            var attribute = entity.GetAttributeValue<AliasedValue>(valueField);

            return attribute?.Value != null ? ((OptionSetValue)attribute.Value).Value : default(int?);
        }

        public static string GetAliasedEntityReferenceName(this Entity entity, string valueField)
        {
            var attribute = entity.GetAttributeValue<AliasedValue>(valueField);

            return attribute?.Value != null ? ((EntityReference)attribute.Value).Name : default(string);
        }

        public static Guid? GetAliasedEntityReferenceId(this Entity entity, string valueField)
        {
            var attribute = entity.GetAttributeValue<AliasedValue>(valueField);

            return attribute?.Value != null ? ((EntityReference)attribute.Value).Id : default(Guid?);
        }

        #endregion

        #region Native Type Extensions

        public static OptionSetValue ParseToOptionSetValue(this int? value)
        {
            if (value.HasValue)
            {
                return new OptionSetValue(value.Value);
            }

            return default(OptionSetValue);
        }

        public static EntityReference ParseToEntityReference(this Guid? id, string entityName)
        {
            if (id.HasValue) return id.Value.ParseToEntityReference(entityName);

            return default(EntityReference);
        }

        public static EntityReference ParseToEntityReference(this Guid id, string entityName)
        {
            if (!id.Equals(Guid.Empty)) return new EntityReference(entityName, id);

            return default(EntityReference);
        }

        public static Money ParseToMoney(this decimal? value)
        {
            if (value.HasValue) return new Money(value.Value);

            return default(Money);
        }

        #endregion
    }

    public class EntityComparer : IEqualityComparer<Entity>
    {
        public string AttributeName { get; set; }

        public EntityComparer(string attributeName, Type attributeType)
        {
            this.AttributeName = attributeName;
        }

        public bool Equals(Entity e1, Entity e2)
        {
            var attributeType = e1.Attributes[this.AttributeName]?.GetType() ?? e2.Attributes[this.AttributeName]?.GetType();
            if (attributeType == null)
            {
                return true; // always equal
            }
            if (attributeType == typeof(Guid))
            {
                return e1.GetAttributeValue<Guid>(this.AttributeName) == e2.GetAttributeValue<Guid>(this.AttributeName);
            }
            if (attributeType == typeof(EntityReference))
            {
                return e1.GetAttributeValue<EntityReference>(this.AttributeName)?.Id == e2.GetAttributeValue<EntityReference>(this.AttributeName)?.Id;
            }

            throw new Exception(string.Format("EntityComparer.Equals ERROR: Unhandled attribute type '{0}", attributeType.ToString()));
        }

        public int GetHashCode(Entity e)
        {
            var attributeType = e.Attributes[this.AttributeName]?.GetType();
            if (attributeType == null)
            {
                return 0; 
            }
            if (attributeType == typeof(Guid))
            {
                return e.GetAttributeValue<Guid>(this.AttributeName).GetHashCode();
            }
            if (attributeType == typeof(EntityReference)) {
                return e.GetAttributeValue<EntityReference>(this.AttributeName).GetHashCode();
            }

            throw new Exception(string.Format("EntityComparer.GetHashCode ERROR: Unhandled attribute type '{0}", attributeType.ToString()));
        }

    }
}
