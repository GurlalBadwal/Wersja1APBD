namespace WebApplication1;

public class PrescriptionDTO
{
    public DateTime Date  { get; set; }
    public DateTime DueDate { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
}