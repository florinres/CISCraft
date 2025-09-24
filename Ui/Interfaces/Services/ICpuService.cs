using Ui.Models;
using Ui.ViewModels.Generics;

namespace Ui.Interfaces.Services;

public interface ICpuService
{
    public List<MemorySection> MemorySections { get; set; }
    Task LoadJsonMpm(string filePath = "", bool debug = false);
    public void ResetProgram();
    public void StartDebugging();
    public void StopDebugging();
    public void UpdateDebugSymbols(string code, Dictionary<ushort, ushort> debugSymbols, ushort sectionAddress);
    public ushort GetIR();
    void StepMicrocommand();
    void StepMicroinstruction();
    void StepInstruction();
    public void TriggerInterrupt(ISR isr);
}