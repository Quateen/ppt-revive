namespace PPTRevive.Application.Common.Models;
public class Patient
{
    public int TotalPatients { get; set; }
    public int DailyPatientCount { get; set; }
    public int MonthlyPatientCount { get; set; }
    public DateTime ReportDate { get; set; }

}
