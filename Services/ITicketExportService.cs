using SIGRA.Data.Models;
using System.Threading.Tasks;

namespace SIGRA.Services;

public interface ITicketExportService
{
    Task<byte[]> ExportTicketsAsync(DateTime? from, DateTime? to, string format);
}
