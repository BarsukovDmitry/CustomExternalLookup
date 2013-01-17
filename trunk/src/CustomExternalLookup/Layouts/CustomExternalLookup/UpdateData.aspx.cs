using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System.Data;
using System.Collections.Generic;
using Microsoft.SharePoint.Utilities;
using CustomExternalLookup.Models;

namespace CustomExternalLookup.Layouts.CustomExternalLookup
{
    public partial class UpdateData : LayoutsPageBase
    {
        SPWeb _web;
        SPList _list;
        SPField _field;
        
        protected void Page_Load(object sender, EventArgs e)
        {
            _web = SPContext.Current.Web;
            _list = _web.Lists[new Guid(Request["ListId"])];
            _field = _list.Fields.TryGetFieldByStaticName(Request["ColumnName"]);
            if (!_list.DoesUserHavePermissions(SPBasePermissions.ManageWeb))
                SPUtility.Redirect(SPUtility.AccessDeniedPage + "?Source=" + SPHttpUtility.UrlKeyValueEncode(_web.Site.MakeFullUrl(Request.RawUrl)), SPRedirectFlags.RelativeToLayoutsPage, Context);

            if (!IsPostBack)
                MessagesLiteral.Text = "Будет выполнено обновление значений поля \"" + _field.Title + "\" из базы данных. Продолжить?";

        }

        protected void UpdateButton_Click(object sender, EventArgs e)
        {
            string connString = _field.GetCustomProperty("ConnectionString").ToString();
            string queryString = _field.GetCustomProperty("QueryString").ToString();

            SharedModule.CheckConnectionProperties(_field.Title, connString, queryString);

            var dm = new DataManager(connString, queryString);
            DataTable data = null;
            SPSecurity.RunWithElevatedPrivileges(() => data = dm.GetRecords());

            int changedCount = 0;
            var deletedFromBDMessages = new List<string>();

            bool textChanged = false;
            foreach (SPListItem item in _list.Items)
            {
                if (item[_field.Id] == null || item[_field.InternalName + "ID"] == null || item[_field.InternalName + "ID"].ToString() == "")
                    continue;

                string resultText = "";
                
                if (_field.TypeAsString == "CustomExternalLookup")
                {
                    string value = item[_field.InternalName + "ID"].ToString();
                    string text = item[_field.Id].ToString();

                    DataRow row = data.Rows.Find(Convert.ToInt32(value));
                    if (row != null)
                    {
                        if (row["Value"].ToString() != text)
                        {
                            text = row["Value"].ToString();
                            textChanged = true;
                            ++changedCount;
                        }
                    }
                    else
                        deletedFromBDMessages.Add(string.Format("<a href=\"{0}\" target=\"_blank\">{1}</a>: {2}", SPHttpUtility.UrlPathEncode(_web.Site.MakeFullUrl(_list.DefaultDisplayFormUrl + "?ID=" + item.ID.ToString()), false), item.Title, text));

                    resultText = text;
                }
                else if (_field.TypeAsString == "CustomExternalLookupMulti")
                {
                    var mcValues = new SPFieldMultiColumnValue(item[_field.InternalName + "ID"].ToString());
                    var mcTexts = new SPFieldMultiColumnValue(item[_field.Id].ToString());

                    for (int i = 0; i < mcValues.Count; ++i)
                    {
                        DataRow row = data.Rows.Find(Convert.ToInt32(mcValues[i]));
                        if (row != null)
                        {
                            if (row["Value"].ToString() != mcTexts[i])
                            {
                                mcTexts[i] = row["Value"].ToString();
                                textChanged = true;
                                ++changedCount;
                            }
                        }
                        else
                            deletedFromBDMessages.Add(string.Format("<a href=\"{0}\" target=\"_blank\">{1}</a>: {2}", SPHttpUtility.UrlPathEncode(_web.Site.MakeFullUrl(_list.DefaultDisplayFormUrl + "?ID=" + item.ID.ToString()), false), item.Title, mcTexts[i]));

                    }
                    resultText = mcTexts.ToString();
                }

                if (textChanged)
                {
                    item[_field.Id] = resultText;
                    item.Update();
                }
            }

            ResultLiteral.Text = string.Format("Обновлено значений: {0}. Значений, ссылающихся на удалённые данные: {1}", changedCount, deletedFromBDMessages.Count);

            if (deletedFromBDMessages.Count > 0)
            {
                ResultLiteral.Text = ResultLiteral.Text + "<br/><br/>Ссылаются на удалённые данные:<br/>";
                foreach (string val in deletedFromBDMessages)
                    ResultLiteral.Text = ResultLiteral.Text + string.Format("{0}<br/>", val);
            }
                        
            CancelButton.Text = "Закрыть";
        }

        protected void CancelButton_Click(object sender, EventArgs e)
        {
            Response.Redirect(Request["Source"]);
        }
    }
}
