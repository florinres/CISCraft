
using System.IO;
using CPU.Business.Models;
using Ui.Interfaces.Services;
using Ui.Interfaces.ViewModel;
using Ui.ViewModels.Components.Microprogram;

namespace Ui.Services;

public class CpuService : ICpuService
{
    private readonly CPU.Business.CPU _cpu;
    private readonly IMicroprogramViewModel _microprogramService;

    public CpuService(IMicroprogramViewModel microprogramService, CPU.Business.CPU cpu)
    {
        _cpu = cpu;
        _microprogramService = microprogramService;
        LoadJsonMpm();
    }

    public async Task LoadJsonMpm(string filePath = "", bool debug = false)
    {
        string fullPath;
        
        if (filePath == "")
        {
            var projectRoot = AppContext.BaseDirectory!;
            fullPath = Path.Combine(projectRoot, "Assets", "Mpm.json");
        }
        else
        {
            fullPath = filePath;
        }

        var jsonString = await File.ReadAllTextAsync(fullPath);
        //both can be paralized if necesarry
        _microprogramService.LoadMicroprogramFromJson(jsonString);
        _cpu.LoadJsonMpm(jsonString);
    }

    public (int, int) StepMicrocommand()
    {
         var (row, column) = _cpu.StepMicrocommand();
         _microprogramService.CurrentRow = row;
         _microprogramService.CurrentColumn = column;
         return (row, column);
    }
}