namespace PPTRevive.Application.Common.Models;

public class SalaryDto
{
    public decimal TotalSalary { get; set; }
    public decimal TotalTax { get; set; }
    public decimal TotalBonus { get; set; }
    public decimal TotalComission { get; set; }
    public DateTime MonthYear { get; set; }
    public decimal TotalReferralCommission { get; set; }
    public decimal TotalHomeVisitCommission { get; set; }
    public decimal TotalDeduction { get; set; }
    public int? Status { get; set; }
}
