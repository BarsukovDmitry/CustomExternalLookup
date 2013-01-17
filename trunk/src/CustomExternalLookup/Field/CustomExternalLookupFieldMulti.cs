using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint.Security;
using CustomExternalLookup.Controls;

namespace CustomExternalLookup.Field
{
    public class CustomExternalLookupMultiField : SPFieldMultiColumn
    {
        //сделать проверку выбранных значений в мультиселекте на присутствие в вариантах выбора
        
        public CustomExternalLookupMultiField(SPFieldCollection fields, string fieldName)
            : base(fields, fieldName)
        {
        }

        public CustomExternalLookupMultiField(SPFieldCollection fields, string typeName, string displayName)
            : base(fields, typeName, displayName)
        {
        }

        /// <summary>
        /// Переназначение элемента управления для поля
        /// </summary>
        public override BaseFieldControl FieldRenderingControl
        {
            [SharePointPermission(SecurityAction.LinkDemand, ObjectModel = true)]
            get
            {
                BaseFieldControl fieldControl = new CustomExternalLookupFieldControl();
                fieldControl.FieldName = this.InternalName;
                
                return fieldControl;
            }
        }

        /// <summary>
        /// Валидация строки, которая будет происходит при любом изменении значения поля через объектную модель шарепоинта (кроме сервисов, редактирования базы и др)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        //public override string GetValidatedString(object value)
        //{
        //    if (this.Required && (value == null || value.ToString() == ""))
        //        throw new SPFieldValidationException(this.Title + " должно иметь значение");

        //    return base.GetValidatedString(value);
        //}

        public override string GetFieldValueAsHtml(object value)
        {
            //В НАДЕЖДЕ НА СВЕТЛОЕ БУДУЩЕЕ
            //object multiSelectMode = GetCustomProperty("MultiSelect");

            bool multiSelectMode = true;
            if (multiSelectMode != null && multiSelectMode && value != null)
                return SharedModule.BuildHtmlForDisplayMultiColumn(value.ToString());

            return base.GetFieldValueAsHtml(value);
        }

        /// <summary>
        /// Удаление связанного поля
        /// </summary>
        public override void OnDeleting()
        {
            SPList list = SPContext.Current.List;
            SPFieldCollection fields = list.Fields;

            for (int i = fields.Count - 1; i > -1; --i)
            {
                SPField field = fields[i];
                if (field.RelatedField == InternalName)
                {
                    field.Hidden = false;
                    field.Update();
                    list.Fields.Delete(field.InternalName);
                }
            }

            base.OnDeleting();
        }

    }
}
