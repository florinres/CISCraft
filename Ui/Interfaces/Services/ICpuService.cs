namespace Ui.Interfaces.Services;

public interface ICpuService
{
    Task LoadJsonMpm(string filePath = "", bool debug = false);
    (int, int) StepMicrocode();
}