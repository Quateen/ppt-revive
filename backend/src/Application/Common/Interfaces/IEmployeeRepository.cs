using PPTRevive.Application.Common.Models;

namespace PPTRevive.Application.Common.Interfaces;

public interface IEmployeeRepository
{
    Task<IEnumerable<EmployeeDto>> GetEmployees(int employeeId, int pageNo, int pageSize);
    Task<Tuple<SalaryDto?, IEnumerable<SalaryEmployeeDto>>> GetSalaryById(int salaryId);
    Task<Doctor?> DoctorReport(int DoctorId);
    Task<Patient?> PatientReport();

    Task<IEnumerable<PatientDto>> Patientlist();
    //Task<Invoice> GetInvoiceById(long invoiceId);
    //Task<IEnumerable<InvoiceDetail>> GetInvoiceDetailsByInvoiceId(long invoiceId);
    //Task<NewInvoiceHeaderDetails> GetNewInvoiceHeaderDetailsByClientId(long clientId);
    //Task<IEnumerable<ClientCompletedTasks>> GetClientCompletedTasksByClientId(long clientId);
    ////Task<long> CreateInvoice(InvoiceCreateRequest request);
    //Task<long> UpdateInvoice(InvoiceCreateUpdateRequest request);
    //Task<long> CreateInvoice(InvoiceCreateUpdateRequest request);
}
