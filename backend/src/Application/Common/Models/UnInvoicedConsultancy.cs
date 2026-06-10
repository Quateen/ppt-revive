using System.ComponentModel.DataAnnotations.Schema;

namespace PPTRevive.Application.Common.Models;
public class UnInvoicedConsultancy
{
    public int Id { get; set; }
    public string? ConsultancyNo { get; set; }
    public DateTime ConsultancyDate { get; set; }
    //public string? LastDoctor { get; set; }
    public int? DoctorId { get; set; }
    public string? DoctorName { get; set; }
    //public int? TreatmentDaysCount { get; set; }
    public int PatientId { get; set; }
    public string? PatientName { get; set; }
    //public string? PatientGender { get; set; }
    public int Status { get; set; }
    public string? StatusName { get; set; }
    //public int? ReferredByDoctorId { get; set; }
    //public string? ReferredByDoctorName { get; set; }
    //public string? Comment { get; set; }
    public string? ConsultancyType { get; set; }
    public int? ConsultancyTypeId { get; set; }
    public string? ConsultancyName { get; set; }

    // For Prescription
    public int? PrescriptionId { get; set; }
    public int? PackageId { get; set; }
    public string? PackageName { get; set; }
    public int? ForDays { get; set; }
    public decimal PackageAmount { get; set; }
    public decimal PackageDiscountPercentage { get; set; }
    public decimal PackageDiscountAmount { get; set; }
    public decimal PackageTotalAmount { get; set; }
    public string? PatientNo { get; set; }
}
