<%@ Control Language="C#" Debug="true" %>
<%@Assembly Name="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@Register TagPrefix="SharePoint" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" namespace="Microsoft.SharePoint.WebControls"%>
<%@Register TagPrefix="CustomExternalLookup" Namespace="CustomExternalLookup.Controls.EntityPicker" Assembly="$SharePoint.Project.AssemblyFullName$" %>
<%@Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>

<SharePoint:RenderingTemplate ID="CustomExternalLookupFieldControl" runat="server">
    <Template>
        <asp:Panel ID="FieldPanel" runat="server">
            <style type="text/css">
                div.MessagesLiteralWrap {
                    color:Red;
                }
            </style>

            <CustomExternalLookup:CustomExternalLookupEditor ID="EntityEditor" runat="server" MultiSelect="false" ValidatorEnabled="true" MaximumEntities="1" Width="600" Visible="false" />
            <asp:DropDownList ID="DropDownList" runat="server" Visible="false"></asp:DropDownList>
        
            <asp:Panel ID="ListBoxesPanel" runat="server" Visible="false" CssClass="CELListBoxesPanel">
                
                <SharePoint:CssLink ID="CELCssLink" runat="server" DefaultUrl="/_layouts/CustomExternalLookup/CELField.css" /> 

                <asp:Panel ID="CELGroup1" runat="server" CssClass="CELGroupHorizontal">
                    <asp:ListBox ID="LeftListBox" runat="server" SelectionMode="Multiple" CssClass="CELLeftListBox"></asp:ListBox>
                </asp:Panel>
                <asp:Panel ID="CELGroupButtons" CssClass="CELGroupHorizontal CELButtonsHorizontal" runat="server">
                    <asp:Button ID="AddButton" CssClass="CELAddButton ms-ButtonHeightWidth" Text="Добавить &gt;" runat="server" Enabled="False" />
                    <asp:Button ID="RemoveButton" CssClass="CELRemoveButton ms-ButtonHeightWidth" Text="&lt; Удалить" runat="server" Enabled="False" />
                </asp:Panel>
                <asp:Panel ID="CELGroup2" CssClass="CELGroupHorizontal" runat="server">
                    <asp:ListBox ID="RightListBox" runat="server" SelectionMode="Multiple" CssClass="CELRightListBox" />
                    <div class="CELHiddenFieldWrap">
                        <asp:HiddenField ID="RightListBoxValues" runat="server" />
                    </div>
                </asp:Panel>
             
            </asp:Panel>
            
            <div class="MessagesLiteralWrap">
                <SharePoint:EncodedLiteral ID="MessagesLiteral" runat="server"></SharePoint:EncodedLiteral>
            </div>
        </asp:Panel>
    </Template>
</SharePoint:RenderingTemplate>

<SharePoint:RenderingTemplate ID="CustomExternalLookupDisplayFieldControl" runat="server">
    <Template>
        <SharePoint:EncodedLiteral ID="FieldValueLiteral" runat="server" EncodeMethod="HtmlEncodeAllowSimpleTextFormatting"></SharePoint:EncodedLiteral>
    </Template>
</SharePoint:RenderingTemplate>

