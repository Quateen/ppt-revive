namespace PPTRevive.Application.Common.Models;
public class FollowUpConsultancyDto
{
    public int ConsultancyId { get; set; }
    public string? ConsultancyNo { get; set; }
    public DateTime ConsultancyDate { get; set; }
    public string? LastDoctor { get; set; }
    public int DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public int TreatmentDaysCount { get; set; }
    public int PatientId { get; set; }
    public string? PatientName { get; set; }
    public int ConsultancyStatusId { get; set; }
    public int ConsultancyTypeId { get; set; }
    public string? ConsultancyTypeName { get; set; }
    public bool Panel { get; set; }
    public string? PanelName { get; set; }
    public int? ReferredByDoctorId { get; set; }
    public string? ReferredByDoctorName { get; set; }
    public int TreatmentSessionNo { get; set; }
    public int? ParentConsultancyId { get; set; }
}
