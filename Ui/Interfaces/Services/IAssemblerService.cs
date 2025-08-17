namespace Ui.Interfaces.Services;

public interface IAssemblerService
{
    public Dictionary<short, ushort> DebugSymbols { get; set; }

    event EventHandler<byte[]> SourceCodeAssembled;
    
    byte[] AssembleSourceCodeService(string sourceCode);
}