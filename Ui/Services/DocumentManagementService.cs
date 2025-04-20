using Ui2.Models;

namespace Ui2.Services;

public class DocumentManagementService
{
    public List<FileViewModel> Documents { get; set; }
    public int ActiveDocumentIndex { get; set; }
}