namespace Ui.Interfaces.Services;

public interface IAssemblerService
{
    event EventHandler<byte[]> SourceCodeAssembled;
    
    byte[] AssembleSourceCodeService(string sourceCode);
}