using System;
using System.Collections;
using Microsoft.SharePoint.WebControls;

namespace CustomExternalLookup.Controls.EntityPicker
{
    public class CustomExternalLookupPickerDialog : PickerDialog
    {
        public new CustomExternalLookupEditor EditorControl
        {
            get { return (CustomExternalLookupEditor)base.EditorControl; }
        }

        public new TableResultControl ResultControl
        {
            get { return (TableResultControl)base.ResultControl; }
        }

        public CustomExternalLookupPickerDialog()
            : base(new CustomExternalLookupQueryControl(), new TableResultControl(), new CustomExternalLookupEditor())
        {
            // Create a list of column display names
            ArrayList columnDisplayNames = ResultControl.ColumnDisplayNames;
            columnDisplayNames.Clear();
            columnDisplayNames.Add("ID");
            columnDisplayNames.Add("Значение");

            // Create a list of column names
            ArrayList columnNames = ResultControl.ColumnNames;
            columnNames.Clear();
            columnNames.Add("ID");
            columnNames.Add("Value");

            // Create a list of column widths
            ArrayList columnWidths = ResultControl.ColumnWidths;
            columnWidths.Clear();
            columnWidths.Add("10%");
            columnWidths.Add("90%");
        }
        
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            DialogTitle = "Выбор значения";
            Description = "Выберите значение из таблицы";
        }

        
    }
}
