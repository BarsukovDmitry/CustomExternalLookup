<xsl:stylesheet xmlns:x="http://www.w3.org/2001/XMLSchema"
                xmlns:d="http://schemas.microsoft.com/sharepoint/dsp"
                version="1.0"
                exclude-result-prefixes="xsl msxsl ddwrt"
                xmlns:ddwrt="http://schemas.microsoft.com/WebParts/v2/DataView/runtime"
                xmlns:asp="http://schemas.microsoft.com/ASPNET/20"
                xmlns:__designer="http://schemas.microsoft.com/WebParts/v2/DataView/designer"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:SharePoint="Microsoft.SharePoint.WebControls"
                xmlns:ddwrt2="urn:frontpage:internal">

  <xsl:template match="FieldRef[@FieldType='CustomExternalLookupMulti']" mode="header">
    <th class="ms-vh2" nowrap="nowrap" scope="col" style="vertical-align:middle">
      <xsl:value-of select="current()/@DisplayName" />
      <a style="padding-left:2px;padding-right:12px" onclick="GoToLink(this);return false;"
        href="{$HttpVDir}/_layouts/CustomExternalLookup/UpdateData.aspx?ListId={$List}&amp;ColumnName={current()/@Name}">
        <img border="0" src="/_layouts/images/CustomExternalLookup/bdupdate.gif" alt="Обновить внешние данные" title="Обновить внешние данные"></img>
      </a>
    </th>
  </xsl:template>

<xsl:template match="FieldRef[@FieldType='CustomExternalLookup']" mode="header">
  <th nowrap="nowrap" scope="col" onmouseover="OnChildColumn(this)" class="ms-vh2">
    <xsl:attribute name="id">
      <xsl:value-of select="concat('CELField', @Name)"/>
    </xsl:attribute>
    <xsl:call-template name="dvt_headerfield">
      <xsl:with-param name="fieldname">
        <xsl:value-of select="@Name"/>
      </xsl:with-param>
      <xsl:with-param name="fieldtitle">
        <xsl:value-of select="@DisplayName"/>
      </xsl:with-param>
      <xsl:with-param name="displayname">
        <xsl:value-of select="@DisplayName"/>
      </xsl:with-param>
      <xsl:with-param name="fieldtype">
        x:string
      </xsl:with-param>
    </xsl:call-template>
  </th>
  <script type="text/javascript">
    function add<xsl:value-of select="current()/@Name"></xsl:value-of>CELFieldLink(){
      var listId = '<xsl:value-of select="$List"></xsl:value-of>';
      var colName = '<xsl:value-of select="current()/@Name"></xsl:value-of>';
      var vDir = '<xsl:value-of select="$HttpVDir"></xsl:value-of>';
      var updateFieldLink = document.createElement('a');
      updateFieldLink.style.paddingLeft = '2px';
      updateFieldLink.style.paddingRight = '12px';

      function goToUpdateCELFieldLink(){
        GoToLink(updateFieldLink);
        return false;
      }

      if(updateFieldLink.attachEvent)
        updateFieldLink.attachEvent('onclick', goToUpdateCELFieldLink);
      else
        updateFieldLink.addEventListener('click', goToUpdateCELFieldLink, false);

      updateFieldLink.href = vDir + '/_layouts/CustomExternalLookup/UpdateData.aspx?ListId=' + listId + '&amp;ColumnName=' + colName;

      var updateFieldImage = document.createElement('img');
      updateFieldImage.src = '/_layouts/images/CustomExternalLookup/bdupdate.gif';
      updateFieldImage.border = 0;
      updateFieldImage.alt = 'Обновить внешние данные';
      updateFieldImage.title = 'Обновить внешние данные';
    
      updateFieldLink.appendChild(updateFieldImage);
      document.getElementById('<xsl:value-of select="concat('CELField', @Name)"/>').firstChild.appendChild(updateFieldLink);
      
    }
    add<xsl:value-of select="current()/@Name"></xsl:value-of>CELFieldLink();
    
  </script>
</xsl:template>
</xsl:stylesheet>