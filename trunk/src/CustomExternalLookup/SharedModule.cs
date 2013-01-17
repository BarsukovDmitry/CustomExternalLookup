using System;
using System.Text;
using Microsoft.SharePoint;

namespace CustomExternalLookup
{
    public static class SharedModule
    {
        public static void CheckConnectionProperties(string fieldName, string connString, string queryString)
        {
            if (string.IsNullOrEmpty(connString))
                throw new Exception(string.Format("Укажите строку подключения в свойствах поля {0} типа CustomExternalLookup", fieldName));
            if (string.IsNullOrEmpty(queryString))
                throw new Exception(string.Format("Укажите строку запроса в свойствах поля {0} типа CustomExternalLookup", fieldName));
        }

        public static bool EnsureSlaveFields(SPList list, string fieldInternalName)
        {
            bool unsafeUpdatesChanged = false;
            bool allowUnsafeUpdates = false;
            bool slaveFieldsHaveNotBeenCreated = true;
            
            foreach (SPField field in list.Fields)
            {
                //проверка, что это наш тип поля
                if (field.TypeAsString != "CustomExternalLookup" && field.TypeAsString != "CustomExternalLookupMulti")
                    continue;
                
                //попытка получения текущего поля
                if (list.Fields.TryGetFieldByStaticName(fieldInternalName + "ID") == null)
                {
                    SPFieldCollection fields = list.Fields;

                    var slaveField = new SPField(fields, SPFieldType.Note.ToString(), fieldInternalName + "ID")
                                         {
                                             Hidden = true, 
                                             RelatedField = fieldInternalName
                                         };

                    allowUnsafeUpdates = list.ParentWeb.AllowUnsafeUpdates;
                    list.ParentWeb.AllowUnsafeUpdates = true;
                    unsafeUpdatesChanged = true;

                    fields.Add(slaveField);

                    list.Update();

                    slaveFieldsHaveNotBeenCreated = false;
                }
            }

            if(unsafeUpdatesChanged)
                list.ParentWeb.AllowUnsafeUpdates = allowUnsafeUpdates;

            return slaveFieldsHaveNotBeenCreated;
        }

        public static string BuildHtmlForDisplayMultiColumn(object value)
        {
            if (value != null && value.ToString() != "")
            {
                string valueWithoutHtmlEntities = value.ToString().Replace("&quot;", "\"");
                var v = new SPFieldMultiColumnValue(valueWithoutHtmlEntities);
                var result = new StringBuilder();
                for (int i = 0; i < v.Count; ++i)
                {
                    if (i > 0)
                        result.Append("<br/>");
                    if (v.Count > 1)
                        result.Append("-");
                    
                    result.Append(v[i]);
                }

                return result.Replace("\"", "&quot;").ToString();
            }

            return "";
        }
    }
}
