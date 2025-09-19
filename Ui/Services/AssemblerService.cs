using MainMemory.Business;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Ui.Interfaces.Services;
using ASMBLR = Assembler.Business.Assembler;

namespace Ui.Services;

public class AssemblerService : IAssemblerService
{
    private readonly ASMBLR _assembler;
    private readonly IMainMemory _mainMemory;
    public event EventHandler<byte[]>? SourceCodeAssembled;
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

    public Dictionary<short, ushort> AssembleSourceCodeService(string sourceCode, ushort sectionAddress)
    {
        var objectCode = _assembler.Assemble(sourceCode, out _);

        // Let the memory wrapper know it's about to receive multiple updates
        // This prevents flickering by batching the updates
        _mainMemory.LoadAtOffset(objectCode, sectionAddress);

        // Only notify about the assembled code after all updates are complete
        SourceCodeAssembled?.Invoke(this, objectCode);

        return _assembler.GetDebugSymbols(sectionAddress);
    }
}