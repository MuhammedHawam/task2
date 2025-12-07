using Microsoft.Xrm.Sdk;
using PIF.EBP.Application.MetaData.DTOs;
using System;

namespace PIF.EBP.Application.Shared
{
    public class CRMUtility
    {
        public static T GetAttributeValue<T>(Entity entity, string attributeName, T defaultValue = default)
        {
            if (entity.Attributes.TryGetValue(attributeName, out var attributeValue))
            {
                try
                {
                    // If the type of T is string, return the attribute value's string representation directly
                    if (typeof(T) == typeof(string))
                    {
                        return (T)(object)(attributeValue?.ToString() ?? string.Empty);
                    }
                    // Special handling for bool since Convert.ToBoolean can handle various types
                    else if (typeof(T) == typeof(bool))
                    {
                        bool result = bool.TryParse(attributeValue?.ToString(), out bool boolValue) && boolValue;
                        return (T)(object)result;
                    }
                    // Special handling for byte arrays
                    else if (typeof(T) == typeof(byte[]))
                    {
                        if (attributeValue is byte[] bytes)
                        {
                            return (T)(object)bytes;
                        }
                        // Optionally, you could handle converting from other formats to byte[] here
                        // For instance, converting a Base64 string to byte[]
                        else if (attributeValue is string base64)
                        {
                            try
                            {
                                return (T)(object)Convert.FromBase64String(base64);
                            }
                            catch
                            {
                                // Handle or log base64 conversion errors
                                return defaultValue;
                            }
                        }
                    }
                    // Use Convert.ChangeType for other types that are supported by Convert
                    else
                    {
                        return (T)Convert.ChangeType(attributeValue, typeof(T));
                    }
                }
                catch (FormatException ex)
                {
                    // Log or handle the exception as necessary
                    Console.WriteLine($"Format exception: {ex.Message}");
                    return defaultValue;
                }
            }

            return defaultValue;
        }
        public static int GetOptionSetValue(Entity entity, string attributeName)
        {
            if (entity.Attributes.TryGetValue(attributeName, out var attributeValue))
            {
                if (attributeValue is OptionSetValue optionSetValue)
                {
                    return optionSetValue.Value;
                }
            }
            return 0;
        }
        public static EntityReferenceDto GetEntityReferenceDto(Entity entityValue, string attrName)
        {
            var entityReference = entityValue.GetAttributeValue<EntityReference>(attrName);
            if (entityReference == null) return null;

            return new EntityReferenceDto
            {
                Id = entityReference.Id.ToString(),
                Name = entityReference.Name
            };
        }
        public static string GetValueByAttrNameAliased(Entity entity, string attributeAlias)
        {
            var aliasedValue = entity.GetAttributeValue<AliasedValue>(attributeAlias);
            return aliasedValue?.Value?.ToString() ?? string.Empty;
        }

        public static int GetCultureForCRM(string UI)
        {
            if (string.IsNullOrEmpty(UI))
                UI = GetCulture();
            switch (UI)
            {
                case "en":
                    return 1033;
                case "ar":
                case "ar-sa":
                    return 1025;
                default:
                    return 1033;
            }
        }
        public static string GetCulture()
        {
            if (System.Threading.Thread.CurrentThread != null)
                return System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName;
            else
                return "en";
        }

    }
}
