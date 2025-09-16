namespace MainMemory.Business
{
    public class ISR
    {
        public string? Name { get; set; }
        public ushort IVTAddress { get; set; }
        public ushort ISRAddress { get; set; }
        public required string TextCode { get; set; }
    }
}