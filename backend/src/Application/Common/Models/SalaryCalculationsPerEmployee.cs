namespace PPTRevive.Application.Common.Models;

public class SalaryCalculationsPerEmployee
{
    public int UserId { get; set; }
    public int Id { get; set; }
    public string? DisplayName { get; set; }
    public int UserType { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal SalaryAmount { get; set; }
    public decimal BonusAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ReferralCommissionAmount { get; set; }
    public decimal HomeVisitCommissionAmount { get; set; }
}
