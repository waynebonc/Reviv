namespace Reviv
{
    class SysCfgItem
    {
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public bool IsHex { get; set; }
        public byte[] RawValue { get; set; }

        public SysCfgItem () { }
    }
}
