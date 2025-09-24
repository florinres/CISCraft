namespace Ui.Interfaces.Services;

public interface IAssemblerService
{
    event EventHandler<byte[]> SourceCodeAssembled;
    
    Dictionary<ushort, ushort> AssembleSourceCodeService(string sourceCode, ushort sectionOffset);
}