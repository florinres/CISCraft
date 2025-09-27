using Assembler.Business;
using MainMemory.Business;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;
using Ui.Interfaces.Services;
using ASMBLR = Assembler.Business.Assembler;

namespace Ui.Services;

public class AssemblerService : IAssemblerService
{
    private readonly ASMBLR _assembler;
    private readonly IMainMemory _mainMemory;
    public event EventHandler<byte[]>? SourceCodeAssembled;
    public event EventHandler<IReadOnlyList<AssemblyError>>? AssemblyErrorsReported;
    public List<ISR>? Isrs;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true
    };
    public AssemblerService(ASMBLR assembler, IMainMemory memory)
    {
        _assembler = assembler;
        _mainMemory = memory;
    }
    private List<ISR> ReadIVTJson(){
        string currentFolder = Path.GetFullPath(AppContext.BaseDirectory + "../../../../");
        string jsonPath = Path.Combine(currentFolder + "Configs", "IVT.json");
        if (!File.Exists(jsonPath))
            return new List<ISR>();
        string json = File.ReadAllText(jsonPath);

        return JsonSerializer.Deserialize<List<ISR>>(json, JsonOpts) ?? new();
    }

    public Dictionary<ushort, ushort> AssembleSourceCodeService(string sourceCode, ushort sectionAddress)
    {
        List<Assembler.Business.AssemblyError> err;
        int machineCodeLen = -1;
        var objectCode = _assembler.Assemble(sourceCode, out machineCodeLen, out err);
        
        if (machineCodeLen <= 0)
        {            
            // Use dispatcher to show message box on UI thread
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                MessageBox.Show("Assembly failed: Please check Logs directory for more details.", "Assembly Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }));
            
            // Notify subscribers about the errors - they will handle thread synchronization
            AssemblyErrorsReported?.Invoke(this, err);
            return null;
        }

        // Let the memory wrapper know it's about to receive multiple updates
        // This prevents flickering by batching the updates
        _mainMemory.LoadAtOffset(objectCode, sectionAddress);

        // Only notify about the assembled code after all updates are complete
        SourceCodeAssembled?.Invoke(this, objectCode);

        // Clear any previous errors since assembly succeeded
        AssemblyErrorsReported?.Invoke(this, new List<AssemblyError>());

        return _assembler.GetDebugSymbols(sectionAddress);
    }
}