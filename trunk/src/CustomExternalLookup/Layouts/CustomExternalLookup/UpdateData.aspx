<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UpdateData.aspx.cs" Inherits="CustomExternalLookup.Layouts.CustomExternalLookup.UpdateData" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">

</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">
<SharePoint:EncodedLiteral ID="MessagesLiteral" runat="server"></SharePoint:EncodedLiteral>
<asp:Button ID="UpdateButton" runat="server" Text="Обновить" OnClick="UpdateButton_Click" />
<asp:Button ID="CancelButton" runat="server" Text="Отмена" OnClick="CancelButton_Click" />

<br /><br />

<SharePoint:EncodedLiteral ID="ResultLiteral" runat="server" EncodeMethod="NoEncode"></SharePoint:EncodedLiteral>
</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Обновление внешних данных поля
</asp:Content>

<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
Обновление внешних данных поля
</asp:Content>
