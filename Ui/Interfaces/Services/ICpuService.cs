using Ui.ViewModels.Generics;

namespace Ui.Interfaces.Services;

public interface ICpuService
{
    HighlightCurrentLineBackgroundRenderer? Highlight { get; set; }
    Task LoadJsonMpm(string filePath = "", bool debug = false);
    (int, int) StepMicrocommand();
    public void SetActiveEditor(FileViewModel fileViewModel);
    public void ResetProgram();
    public void StartDebugging();
    public void StopDebugging();
    public void SetDebugSymbols(Dictionary<short, ushort> debugSymbols);
}