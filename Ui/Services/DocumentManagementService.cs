
using Ui.Models;

namespace Ui.Services;

public class DocumentManagementService
{
    public List<FileViewModel> Documents { get; set; }
    public int ActiveDocumentIndex { get; set; }
}