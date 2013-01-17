using System;
using System.Collections.Generic;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Data;
using CustomExternalLookup.Models;

namespace CustomExternalLookup.Controls.EntityPicker
{
    public class CustomExternalLookupEditor : EntityEditorWithPicker
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            PickerDialogType = typeof(CustomExternalLookupPickerDialog);
        }

        /// <summary>
        /// ������� ��������� �������� �������� ��� ������ �� ��������, ������� �� ������� ���������
        /// </summary>
        /// <param name="unresolvedText"></param>
        /// <returns></returns>
        protected override PickerEntity[] ResolveErrorBySearch(string unresolvedText)
        {
            base.ResolveErrorBySearch(unresolvedText);

            var dm = new DataManager(PickerData.ConnectionString, PickerData.QueryString);
            DataTable results = null;
            SPSecurity.RunWithElevatedPrivileges(() => results = dm.GetRecords(unresolvedText));
            
            return ConvertDataTableToPickerEntities(results);
        }

        /// <summary>
        /// �������� ��������. ������ ��� ������ �� ������ ��������� �����
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override PickerEntity ValidateEntity(PickerEntity entity)
        {
            if (entity.IsResolved)
                return entity;

            var dm = new DataManager(PickerData.ConnectionString, PickerData.QueryString);
            DataRow row = null;
            SPSecurity.RunWithElevatedPrivileges(() => row = dm.GetRecord(entity.DisplayText));
            
            if (row == null)
                return entity;

            // Resolve entity
            var pe = new PickerEntity
                         {
                             Key = row["ID"].ToString(), 
                             DisplayText = row["Value"].ToString()
                         };
            entity = pe;
            entity.IsResolved = true;

            ////���� �� ��������
            //if (entity.DisplayText.Contains(CustomExternalLookupValue.Separator))
            //    ErrorMessage = string.Format("��������������: �������� �������� ������������ ��� ������� ���������� ������� {0} � ����� ��������� ���������. ���������� �������� �������� �� ������� ��������� ������", CustomExternalLookupValue.Separator);

            return entity;
        }

        public CustomExternalLookupData PickerData
        {
            get
            {
                byte[] buffer = Convert.FromBase64String(CustomProperty);
                using (var ms = new MemoryStream(buffer))
                {
                    var bf = new BinaryFormatter();
                    return bf.Deserialize(ms) as CustomExternalLookupData;
                }
            }
            set
            {
                var bf = new BinaryFormatter();
                using (var ms = new MemoryStream())
                {
                    bf.Serialize(ms, value);
                    CustomProperty = Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        static public PickerEntity CreatePickerEntity(int id, string value)
        {
            var pe = new PickerEntity
                         {
                             Key = id.ToString(), 
                             DisplayText = value
                         };
            return pe;
        }

        public PickerEntity[] ConvertDataTableToPickerEntities(DataTable table)
        {
            var entities = new List<PickerEntity>();

            foreach (DataRow row in table.Rows)
                entities.Add(CreatePickerEntity((int)row["ID"], row["Value"].ToString()));

            return entities.ToArray();
        }
        
    }
}
    
