namespace Ui.Interfaces.Services;

public interface IAssemblerService
{
    event EventHandler<byte[]> SourceCodeAssembled;
    
    Dictionary<short, ushort> AssembleSourceCodeService(string sourceCode, ushort sectionOffset);
}