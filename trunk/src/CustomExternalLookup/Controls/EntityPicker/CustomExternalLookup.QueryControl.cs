using System;
using System.Data;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using CustomExternalLookup.Models;

namespace CustomExternalLookup.Controls.EntityPicker
{
    public class CustomExternalLookupQueryControl : PickerQueryControlBase
    {
        public TableResultControl ResultControl
        {
            get { return (TableResultControl)base.PickerDialog.ResultControl; }
        }

        public CustomExternalLookupEditor EditorControl
        {
            get { return (CustomExternalLookupEditor)base.PickerDialog.EditorControl; }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            base.ColumnList.Visible = false;
        }

        public override PickerEntity GetEntity(DataRow dataRow)
        {
            if (dataRow == null)
                return null;

            var entity = new PickerEntity
                             {
                                 Key = Convert.ToString(dataRow["ID"]), 
                                 DisplayText = Convert.ToString(dataRow["Value"]),
                                 IsResolved = true
                             };

            return entity;
        }

        protected override int IssueQuery(string search, string group, 
            int pageIndex, int pageSize)
        {
            search = (search != null) ? search.Trim() : null;

            //��������, ��� ������ ������� �� �����
            if (string.IsNullOrEmpty(search))
            {
                PickerDialog.ErrorMessage = "������� ��������� ��� ������";
                return 0;
            }
            
            //�������� ������, ��������������� �������
            var dm = new DataManager(EditorControl.PickerData.ConnectionString, EditorControl.PickerData.QueryString);
            DataTable table = null;
            SPSecurity.RunWithElevatedPrivileges(() => table = dm.GetRecords(search));
            
            //����������� ������ �� �������
            if (table.Rows.Count == 0)
            {
                PickerDialog.ErrorMessage = "�� ������ ������� ������ �� �������";
                return 0;
            }
            
            // Return results to dialog
            PickerDialog.Results = table;
            PickerDialog.ResultControl.PageSize = table.Rows.Count; 

            // Return number of records
            return table.Rows.Count;
        }

    }
}
