namespace PIF.EBP.Core.ESM.DTOs
{
    public class EsmUploadedDocument
    {
        public string Name { get; set; }
        public string KeyName { get; set; }

        public string Extension { get; set; }

        public long Size { get; set; }

        public byte[] Bytes { get; set; }
    }
}
