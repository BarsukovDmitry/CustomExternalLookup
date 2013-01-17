using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SharePoint.WebControls;
using System.Web.UI.WebControls;
using System.Collections;
using System.Data;
using CustomExternalLookup.Models;
using CustomExternalLookup.Controls.EntityPicker;
using Microsoft.SharePoint;
using System.ComponentModel;

namespace CustomExternalLookup.Controls
{
    public class CustomExternalLookupFieldControl : BaseFieldControl
    {
        //контролы режимов создания и редактирования
        EncodedLiteral _messagesLiteral;
        Panel _fieldPanel;
        
        CustomExternalLookupEditor _entityEditor;
        DropDownList _dropDownList;

        Panel _listBoxesPanel;
        ListBox _leftListBox;
        ListBox _rightListBox;
        HiddenField _rightListBoxValues;
        
        //контролы режима просмотра
        EncodedLiteral _fieldValueLiteral;

        //просто переменные
        bool _pickerEntityMode;
        string _connString = "";
        string _queryString = "";
        bool _queryStringChanged;

        public bool MultiSelectMode;
        
        /// <summary>
        /// Указывает, нужно ли выполнить запрос элементов для выбора значений. По умолчанию true
        /// </summary>
        [Description("Указывает, нужно ли выполнить запрос элементов для выбора значений. По умолчанию true")]
        public bool ExecuteQuery
        {
            get
            {
                return ViewState["ExecuteQuery"] == null || (bool)ViewState["ExecuteQuery"];
            }
            set
            {
                ViewState["ExecuteQuery"] = value;
            }
        }

        /// <summary>
        /// Заменяет настроенную в свойствах данного поля строку запроса данных из БД
        /// </summary>
        [Description("Заменяет настроенную в свойствах данного поля строку запроса данных из БД")]
        public string QueryString
        {
            get
            {
                return ViewState["QueryString"] != null ? (string)ViewState["QueryString"] : "";
            }

            set
            {
                ViewState["QueryString"] = value;
                _queryStringChanged = true;
            }
        }

        /// <summary>
        /// Возвращает и устанавливает значения в элементах управления по переданных идентификаторам в формате MultiColumn
        /// </summary>
        public int[] SelectedIds
        {
            get
            {
                EnsureChildControls();

                if (!ExecuteQuery)
                    return new int[]{};

                var values = new SPFieldMultiColumnValue(GetValues());
                var ids = new List<int>();
                for (int i = 0; i < values.Count; ++i)
                    ids.Add(Convert.ToInt32(values[i]));

                return ids.ToArray();
            }

            set
            {
                EnsureChildControls();

                //запрос значений для режима PickerDialog
                if (_pickerEntityMode)
                {
                    if (value.Length == 0)
                    {
                        _entityEditor.UpdateEntities(new ArrayList());
                        return;
                    }
                    
                    //выбор идентификторов для поиска: все, если режим множественного выбора или первое, если не множественного
                    int[] ids = MultiSelectMode ? value : new[] { value[0] };
                    
                    //запрос данных для установки выбранных значений в EntityEditor'е
                    var dm = new DataManager(_connString, _queryString);
                    DataTable table = null;
                    
                    SPSecurity.RunWithElevatedPrivileges(() => table = dm.GetRecordsByIds(ids));
                    
                    //подготовка значений
                    var pe = new ArrayList();
                    foreach (int t in ids)
                    {
                        DataRow row = table.Rows.Find(t);
                        if(row != null)
                            pe.Add(CustomExternalLookupEditor.CreatePickerEntity(t, row["Value"].ToString()));
                        else
                            throw new SPException("Не удалось найти указанное значение ID в базе данных");
                    }

                    _entityEditor.UpdateEntities(pe);
                }
                else
                {
                    if (MultiSelectMode)
                    {
                        string commaValues = "";
                        for (int i = 0; i < value.Length; ++i)
                        {
                            if (i > 0)
                                commaValues += ",";
                            commaValues += value[i].ToString();
                        }

                        _rightListBoxValues.Value = commaValues;
                    }
                    else
                    {
                        if (value.Length == 0)
                        {
                            _dropDownList.SelectedValue = "";
                            return;
                        }
                        _dropDownList.SelectedValue = value[0].ToString();
                    }
                }
            }
        }

        /// <summary>
        /// Название ClientControl, где описана разметка элементов управления
        /// </summary>
        protected override string DefaultTemplateName
        {
            get { return "CustomExternalLookupFieldControl"; }
        }

        public override string DisplayTemplateName
        {
            get
            {
                return "CustomExternalLookupDisplayFieldControl";
            }
           
        }

        protected override void OnInit(EventArgs e)
        {
            if (Field == null)
                throw new SPException("Указанное в FieldName CustomExternalLookupField поле не найдено");
            
            //добавление связанных полей, если их нет
            if (!SharedModule.EnsureSlaveFields(List, Field.InternalName))
                Page.Response.Redirect(Page.Request.Url.AbsoluteUri);

            base.OnInit(e);
        }
        
        #region CreateChildControls
        /// <summary>
        /// Настройка элементов управления для форм
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            
            if (Field == null)
                return;
            
            //чтение свойств поля
            _connString = Field.GetCustomProperty("ConnectionString").ToString();
            _queryString = Field.GetCustomProperty("QueryString").ToString();
            _pickerEntityMode = (bool)Field.GetCustomProperty("PickerEntity");
            
            SharedModule.CheckConnectionProperties(FieldName, _connString, _queryString);

            //В НАДЕЖДЕ НА СВЕТЛОЕ БУДУЩЕЕ  
            //this.multiSelectMode = Field.GetCustomProperty("MultiSelect");
            
            MultiSelectMode = Field.TypeAsString == "CustomExternalLookupMulti";

            //создание и настройка контролов для режима отображения
            if (ControlMode == SPControlMode.Display)
            {
                _fieldValueLiteral = TemplateContainer.FindControl("FieldValueLiteral") as EncodedLiteral;

                if (_fieldValueLiteral == null)
                    MissingControlException("FieldValueLiteral");

                if (ItemFieldValue != null && ItemFieldValue.ToString() != "")
                    _fieldValueLiteral.Text = MultiSelectMode ? SharedModule.BuildHtmlForDisplayMultiColumn(ItemFieldValue) : ItemFieldValue.ToString();

                return;
            }

            //создание и настройка контролов для режимов создания и редактирования элемента
            _fieldPanel = TemplateContainer.FindControl("FieldPanel") as Panel;
            if (_fieldPanel == null)
                MissingControlException("FieldPanel");

            _messagesLiteral = TemplateContainer.FindControl("MessagesLiteral") as EncodedLiteral;
            if (_messagesLiteral == null)
                MissingControlException("MessagesLiteral");

            //в настроках включён выбор с помощью EntityPicker'a
            if (_pickerEntityMode)
            {
                _entityEditor = (CustomExternalLookupEditor)TemplateContainer.FindControl("EntityEditor");
                if (_entityEditor == null)
                    MissingControlException("EntityEditor");

                _entityEditor.Visible = true;

                if (MultiSelectMode)
                {
                    //множественный выбор
                    _entityEditor.MultiSelect = true;
                    _entityEditor.Rows = 8;
                    _entityEditor.MaximumEntities = 0;
                    _entityEditor.PlaceButtonsUnderEntityEditor = true;
                }
                else
                {
                    _entityEditor.MultiSelect = false;
                    _entityEditor.Rows = 2;
                    _entityEditor.MaximumEntities = 1;
                    _entityEditor.PlaceButtonsUnderEntityEditor = false;
                }
                _entityEditor.DialogWidth = 800;
                _entityEditor.DialogHeight = 600;
                _entityEditor.ToolTip = Field.Title;

            }
            //иначе - выбор из списков
            else
            {
                if (MultiSelectMode)
                {
                    _listBoxesPanel = TemplateContainer.FindControl("ListBoxesPanel") as Panel;
                    if (_listBoxesPanel == null)
                        MissingControlException("ListBoxesPanel");

                    _listBoxesPanel.Visible = true;
                    
                    _leftListBox = TemplateContainer.FindControl("LeftListBox") as ListBox;
                    if (_leftListBox == null)
                        MissingControlException("LeftListBox");

                    _rightListBox = TemplateContainer.FindControl("RightListBox") as ListBox;
                    if (_rightListBox == null)
                        MissingControlException("RightListBox");

                    _rightListBoxValues = TemplateContainer.FindControl("RightListBoxValues") as HiddenField;
                    if (_rightListBoxValues == null)
                        MissingControlException("RightListBoxValues");

                    //настройка размеров листбоксов для множественного выбора
                    int height = 125,
                        width  = 143;
                    object heightTemp = Field.GetCustomProperty("Height");
                    if (heightTemp != null)
                        height = Convert.ToInt32(heightTemp);
                    object widthTemp = Field.GetCustomProperty("Width");
                    if (widthTemp != null)
                        width = Convert.ToInt32(widthTemp);
                    
                    _leftListBox.Width = _rightListBox.Width = width;
                    _leftListBox.Height = _rightListBox.Height = height;

                    //настройка вертикального расположения
                    var vertical = (bool)Field.GetCustomProperty("Vertical");
                    if (vertical)
                    {
                        var CELGroup1 = TemplateContainer.FindControl("CELGroup1") as Panel;
                        if (CELGroup1 == null)
                            MissingControlException("CELGroup1");
                        
                        var CELGroupButtons = TemplateContainer.FindControl("CELGroupButtons") as Panel;
                        if (CELGroupButtons == null)
                            MissingControlException("CELGroupButtons");
                        
                        var CELGroup2 = TemplateContainer.FindControl("CELGroup2") as Panel;
                        if (CELGroup2 == null)
                            MissingControlException("CELGroup2");
                        
                        CELGroup1.CssClass = CELGroup2.Attributes["class"] ="CELGroupVertical";
                        CELGroupButtons.CssClass = "CELButtonsVertical";

                        var AddButton = TemplateContainer.FindControl("AddButton") as Button;
                        if (AddButton == null)
                            MissingControlException("AddButton");

                        var RemoveButton = TemplateContainer.FindControl("RemoveButton") as Button;
                        if (RemoveButton == null)
                            MissingControlException("RemoveButton");
                        
                        AddButton.Text = "∨ Добавить";
                        RemoveButton.Text = "∧ Удалить";
                    }
                }
                else
                {
                    _dropDownList = TemplateContainer.FindControl("DropDownList") as DropDownList;
                    if (_dropDownList == null)
                        MissingControlException("DropDownList");
                    
                    _dropDownList.Visible = true;
                    _dropDownList.ToolTip = Field.Title;
                }
            }
        }
        #endregion

        #region Value
        /// <summary>
        /// Получение и установка значения из/в контрол
        /// </summary>
        public override object Value
        {
            get
            {
                EnsureChildControls();

                if (!ExecuteQuery)
                    return "";

                if (MultiSelectMode)
                {
                    var texts = new SPFieldMultiColumnValue();
                    if (_pickerEntityMode)
                        foreach (PickerEntity pe in _entityEditor.ResolvedEntities)
                            texts.Add(pe.DisplayText);
                    else
                    {
                        string[] values = _rightListBoxValues.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string val in values)
                            texts.Add(_leftListBox.Items.FindByValue(val).Text);
                    }
                    return texts.ToString();
                }

                string text = "";
                if (_pickerEntityMode)
                    foreach (PickerEntity pe in _entityEditor.ResolvedEntities)
                        text = pe.DisplayText;
                else
                    if ((_dropDownList.SelectedIndex > 0 && !Field.Required)
                        || (_dropDownList.SelectedIndex > -1 && Field.Required))
                        text = _dropDownList.SelectedItem.Text;

                return text;
            }
        
            set
            {
                EnsureChildControls();

                if (value == null || value.ToString() == "")
                    return;

                if (!ExecuteQuery)
                    return;

                object slaveFieldValueObject = SPContext.Current.ListItem[SPContext.Current.List.Fields.TryGetFieldByStaticName(Field.InternalName + "ID").Id];

                //если значение передаётся кодом из формы, то оно игнорируется 
                if (slaveFieldValueObject == null)
                    return;

                string slaveFieldValue = slaveFieldValueObject.ToString();

                if (MultiSelectMode)
                {
                    var values = new SPFieldMultiColumnValue(slaveFieldValue);
                    var texts = value as SPFieldMultiColumnValue;
                    if (_pickerEntityMode)
                    {
                        var pe = new ArrayList();
                        for (int i = 0; i < values.Count; ++i)
                            pe.Add(CustomExternalLookupEditor.CreatePickerEntity(Convert.ToInt32(values[i]), texts[i]));

                        _entityEditor.UpdateEntities(pe);
                    }
                    else
                    {
                        _rightListBoxValues.Value = "";
                        int itemCount = 0;
                        for (int i = 0; i < values.Count; ++i)
                        {
                            if (_leftListBox.Items.FindByValue(values[i]) != null)
                            {
                                if (itemCount > 0)
                                    _rightListBoxValues.Value += ",";
                                _rightListBoxValues.Value += values[i];
                                
                                ++itemCount;

                                if (_leftListBox.Items.FindByValue(values[i]).Text != texts[i])
                                    ChangedValueMessage(texts[i], _leftListBox.Items.FindByValue(values[i]).Text);
                            }
                            else
                                DeletedValueMessage(Convert.ToInt32(values[i]), texts[i]);
                        }
                    }
                }
                else
                {
                    if (_pickerEntityMode)
                    {
                        var pe = new ArrayList
                                     {
                                         CustomExternalLookupEditor.CreatePickerEntity(
                                             Convert.ToInt32(slaveFieldValue), value.ToString())
                                     };
                        _entityEditor.UpdateEntities(pe);
                    }
                    else
                    {
                        if (_dropDownList.Items.FindByValue(slaveFieldValue) != null)
                        {
                            _dropDownList.SelectedValue = slaveFieldValue;
                            if (_dropDownList.SelectedItem.Text != value.ToString())
                                ChangedValueMessage(value.ToString(), _dropDownList.SelectedItem.Text);
                        }
                        else
                            DeletedValueMessage(Convert.ToInt32(slaveFieldValue), value.ToString());
                    }
                }
            }
        }

        #endregion

        #region Focus
        public override void Focus()
        {
            if (_pickerEntityMode)
                _entityEditor.Focus();
            else
            {
                if (MultiSelectMode)
                    _leftListBox.Focus();
                else
                    _dropDownList.Focus();
            }
        } 
        #endregion

        public override void Validate()
        {
            const string requiredValueMessage = "Требуется значение для этого обязательного поля";

            if (ControlMode != SPControlMode.Display && IsValid)
            {
                if (_pickerEntityMode)
                {
                    _entityEditor.Validate();
                    if (Field.Required && _entityEditor.Entities.Count == 0)
                    {
                        IsValid = false;
                        _entityEditor.ErrorMessage = "";
                        ErrorMessage = requiredValueMessage;
                    }
                    if (_entityEditor.IsValid == false)
                    {
                        IsValid = false;
                        _entityEditor.ErrorMessage = "";
                        ErrorMessage = "Не удалось найти указанное значение в базе данных";
                    }
                }
                else
                {
                    if (MultiSelectMode)
                        if (Field.Required && _rightListBoxValues.Value.Length == 0)
                        {
                            IsValid = false;
                            ErrorMessage = requiredValueMessage;
                        }
                    else
                        if (Field.Required && string.IsNullOrEmpty(_dropDownList.SelectedValue))
                        {
                            IsValid = false;
                            ErrorMessage = requiredValueMessage;
                        }
                }
            }

            base.Validate();
        }

        //заполнение компонентов вариантами для выбора или настройка для связи с источником данных
        protected override void  OnLoad(EventArgs e)
        {
            EnsureChildControls();

            if (ControlMode == SPControlMode.Display)
                return;

            if (_pickerEntityMode)
            {
                //настройка свойств EntityPicker'а для подключения к внешнему источнику данных
                var editorData = new CustomExternalLookupData
                                     {
                                         ConnectionString = _connString,
                                         QueryString = string.IsNullOrEmpty(QueryString) ? _queryString : QueryString
                                     };

                _entityEditor.PickerData = editorData;
                _entityEditor.Enabled = ExecuteQuery;
            }
            else
            {
                if (ExecuteQuery)
                {
                    if (!Page.IsPostBack || _queryStringChanged)
                    {
                        var dm = new DataManager(_connString, string.IsNullOrEmpty(QueryString) ? _queryString : QueryString);
                        DataTable records = null;
                        SPSecurity.RunWithElevatedPrivileges(() => records = dm.GetRecords());

                        if (MultiSelectMode)
                        {
                            _leftListBox.Items.Clear();
                                
                            foreach (DataRow rec in records.Rows)
                            {
                                var newListItem = new ListItem
                                                      {Value = rec["ID"].ToString(), Text = rec["Value"].ToString()};
                                _leftListBox.Items.Add(newListItem);
                            }

                            _leftListBox.Enabled = _rightListBox.Enabled = true;
                        }
                        else
                        {
                            _dropDownList.Items.Clear();
                            foreach (DataRow row in records.Rows)
                                _dropDownList.Items.Add(new ListItem(row["Value"].ToString(), row["ID"].ToString()));
                            if (!Field.Required)
                                _dropDownList.Items.Insert(0, new ListItem("(нет)", string.Empty));
                            _dropDownList.Enabled = true;
                        }
                    }
                }
                else
                {
                    if (MultiSelectMode)
                    {
                        _leftListBox.Items.Clear();
                        _rightListBox.Items.Clear();
                        _rightListBoxValues.Value = "";
                        _leftListBox.Enabled = _rightListBox.Enabled = false;
                    }
                    else
                    {
                        _dropDownList.Items.Clear();
                        _dropDownList.Enabled = false;
                    }
                }
            }

            if (MultiSelectMode && !_pickerEntityMode)
            {
                //проверка того, что в поле множественного выбора выбраны только допустимые значения   
                string[] valuesSplit = _rightListBoxValues.Value.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                if (valuesSplit.Any(val => _leftListBox.Items.FindByValue(val) == null))
                    throw new SPException("Выбрано недопустимое значение");

                //обновление атрибутов title для options select'а
                AddTitlesToListBoxItems(_leftListBox.Items);
            }
            
            base.OnLoad(e);
            
        }

        protected override void OnPreRender(EventArgs e)
        {
            //для множественного выбора в листбоксах
            if (ControlMode != SPControlMode.Display && MultiSelectMode && !_pickerEntityMode)
            {
                //jquery
                Page.ClientScript.RegisterClientScriptBlock(Page.GetType(), "jquery16", "if (typeof jQuery == 'undefined') {" +
                    "document.write(unescape(\"%3Cscript src='/_layouts/CustomExternalLookup/jquery.min.js'%3E%3C/script%3E\"));" +
                "}", true);

                //подключение скриптов для связи листбоксов
                Page.ClientScript.RegisterClientScriptInclude("LinkedSelects", "/_layouts/CustomExternalLookup/LinkedSelects.js");
                Page.ClientScript.RegisterClientScriptInclude("CELField", "/_layouts/CustomExternalLookup/CELField.js?v3");
            }
            
            base.OnPreRender(e);
        }

        protected override void Render(System.Web.UI.HtmlTextWriter output)
        {
            if (ControlMode != SPControlMode.Display && MultiSelectMode && !_pickerEntityMode)
            {
                //установить доступные для выбора в листбоксах значения (это не защита. Защита в конце OnLoad)
                foreach (ListItem item in _leftListBox.Items)
                    Page.ClientScript.RegisterForEventValidation(_rightListBox.UniqueID, item.Value);
            }

            base.Render(output);
        }

        //добавление значения в скрытое связанное поле
        public override void UpdateFieldValueInItem()
        {
            SPContext.Current.ListItem[SPContext.Current.List.Fields.TryGetFieldByStaticName(Field.InternalName + "ID").Id] = GetValues();

            base.UpdateFieldValueInItem();
        }

        private void MissingControlException(string controlName)
        {
            throw new ArgumentException(string.Format("Corrupted CustomExternalLookupFieldControl template: {0} missing", controlName));
        }

        private void AddTitlesToListBoxItems(ListItemCollection items)
        {
            foreach (ListItem item in items)
            {
                item.Attributes.Add("title", item.Text);
            }
        }

        private void DeletedValueMessage(int ID, string value)
        {
            _messagesLiteral.Text += string.Format("Не удалось найти значение \"{0}\" с ID {1} в источнике данных. ", value, ID);
        }

        private void ChangedValueMessage(string oldValue, string newValue)
        {
            _messagesLiteral.Text += string.Format("Значение \"{0}\" в источнике данных было изменено на \"{1}\". ", oldValue, newValue);
        }

        //получение текущих значений (идентификаторов) из элементов 
        private string GetValues()
        {
            string retValue = "";
            
            if (MultiSelectMode)
            {
                var values = new SPFieldMultiColumnValue();
                if (_pickerEntityMode)
                    foreach (PickerEntity pe in _entityEditor.ResolvedEntities)
                        values.Add(pe.Key);
                else
                {
                    string[] splitValues = _rightListBoxValues.Value.Split(',');
                    foreach (string val in splitValues)
                        values.Add(val);
                }
                retValue = values.ToString();
            }
            else
            {
                if (_pickerEntityMode)
                    foreach (PickerEntity pe in _entityEditor.ResolvedEntities)
                        retValue = pe.Key;
                else
                    if ((_dropDownList.SelectedIndex > 0 && !Field.Required)
                        || (_dropDownList.SelectedIndex > -1 && Field.Required))
                        retValue = _dropDownList.SelectedValue;
            }

            return retValue;
        }
    }
}
