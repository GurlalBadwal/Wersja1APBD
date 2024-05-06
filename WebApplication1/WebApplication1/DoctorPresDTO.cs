using System.Runtime.InteropServices.JavaScript;

namespace WebApplication1;

public class DoctorPresDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Lastname { get; set; }
    public List<Prescription> DocPres { get; set; }
}

public class Prescription
{
    public int Id { get; set; }
    public DateTime Datee { get; set; }
    public DateTime DueDate { get; set; }
    public int IdPatient { get; set; }
}