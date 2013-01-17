using System;

namespace CustomExternalLookup.Controls.EntityPicker
{
    [Serializable]
    public class CustomExternalLookupData 
    {
        public string ConnectionString { get; set; }
        public string QueryString { get; set; }
    }

}
