using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Reflection;
using System.Web;
using System.Globalization;
using System.Resources;

using Microsoft.Xrm.Sdk;

namespace ALMPOC.CRM.Plugins.Helpers
{
    public static class CrmSdkHelper
    {
        /// <summary>
        /// Returns a culture info object based on the language user settings in CRM
        /// </summary>
        /// <param name="userSettings"></param>
        /// <returns></returns>
        public static CultureInfo GetUserCulture(Entity userSettings)
        {
            int uilanguage = 1033;
            if (userSettings != null && userSettings.Attributes.Contains("uilanguageid"))
            {
                uilanguage = (int)userSettings.Attributes["uilanguageid"];
            }
            return new System.Globalization.CultureInfo(uilanguage);
        }

        /// <summary>
        /// Translates the message in the requested language.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public static string Translate(string message, CultureInfo cultureInfo)
        {
            return Translate(message, cultureInfo, null);
        }

        /// <summary>
        /// Translates the message in the requested language. Use the "data" parameter to provide additional information.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cultureInfo"></param>
        /// <param name="data">The values will be passed in the String.Format method. Make sure the translated message contains placeholders, i.e. "{0}"</param>
        /// <returns></returns>
        public static string Translate(string message, CultureInfo cultureInfo, string[] data)
        {
            ResourceManager resourceManager = new ResourceManager("ALMPOC.CRM.Plugins.Resources.Translations", Assembly.GetExecutingAssembly());
            try
            {
                string errorMessage = resourceManager.GetString(message + "_" + cultureInfo.TwoLetterISOLanguageName);
                if (data != null)
                {
                    errorMessage = string.Format(errorMessage, data);
                }
                return errorMessage;
            }
            catch (Exception)
            {
                return message + " (Translation not found)";

            }
        }

        /// <summary>
        /// Get attribute from primary entity object. If it is missing, get it from backup entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="attribute"></param>
        /// <param name="primaryEntity"></param>
        /// <param name="backupEntity"></param>
        /// <returns></returns>
        public static T GetAttributeCoalesce<T>(string attribute, Entity primaryEntity, Entity backupEntity)
        {
            if (primaryEntity.Contains(attribute))
                return primaryEntity.GetAttributeValue<T>(attribute);
            else if (backupEntity != null)
                return backupEntity.GetAttributeValue<T>(attribute);
            else
                return default(T);
        }

    }
}
