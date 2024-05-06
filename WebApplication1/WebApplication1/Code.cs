using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;

namespace WebApplication1;
[ApiController]
[Route("api/prescriptions")]
public class Code : ControllerBase
{
    private IConfiguration _configuration;

    public Code(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    [HttpGet]
    public IActionResult GetData(string DoctorName=null)
    {
        var noDocName = new List<PresDTO>();
        DoctorPresDTO DocName = null;
        using SqlConnection connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
        connection.Open();
        using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        if (DoctorName.IsNullOrEmpty())
        {
            command.CommandText = @"SELECT Prescription.Id, Date, DueDate, Patient.LastName as PName, Doctor.LastName as DName 
                                    FROM Prescription
                                    JOIN Patient on Patient.Id = Prescription.IdPatient
                                    JOIN Doctor on Doctor.Id = Prescription.IdDoctor";
            
            
            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                noDocName.Add(new PresDTO()
                {
                    PresID = (int)reader["ID"], Datee = (DateTime)reader["Date"], DueDate = (DateTime)reader["DueDate"], PLastName = (string)reader["PName"], DLastName = (string)reader["DName"]
                });
            }

            return Ok(noDocName);
        }
        
        command.CommandText = @"SELECT Doctor.Id as DId, Doctor.Name as DName, Doctor.LastName as DLName, Prescription.ID as PId, Prescription.Date, Prescription.DueDate, Prescription.IdPatient as PPId
                                FROM Doctor
                                JOIN Prescription on Prescription.IdDoctor = Doctor.Id
                                WHERE Doctor.Name = @Doname";
        command.Parameters.AddWithValue("@DoName", DoctorName);

        using SqlDataReader reader1 = command.ExecuteReader();
        if (!reader1.HasRows)
        {
            return BadRequest("Doctor not found");
        }
        while (reader1.Read())
        {
            if (DocName == null)
            {
                DocName = new DoctorPresDTO()
                {
                    Id = (int)reader1["DId"], Name = (string)reader1["DName"], Lastname = (string)reader1["DLName"],
                    DocPres = new List<Prescription>()
                    {
                        new Prescription()
                        {
                            Id = (int)reader1["PId"], Datee = (DateTime)reader1["Date"],
                            DueDate = (DateTime)reader1["DueDate"], IdPatient = (int)reader1["PPId"]
                        }
                    }
                };
            }
            else
            {
                DocName.DocPres.Add(new Prescription()
                {
                    Id = (int)reader1["PId"], Datee = (DateTime)reader1["Date"],
                    DueDate = (DateTime)reader1["DueDate"], IdPatient = (int)reader1["PPId"]
                });
            }
        }
        
        return Ok(DocName);
    }
    [HttpPost]
    public IActionResult InsertInPrescriptions(PrescriptionDTO prescriptionDto)
    {
        decimal? PK = null;
        if (prescriptionDto.Date < prescriptionDto.DueDate)
        {
            using SqlConnection connection = new SqlConnection(_configuration["ConnectionStrings:DefaultConnection"]);
            connection.Open();
            using SqlCommand command = new SqlCommand();
            command.Connection = connection;

            command.CommandText = "SELECT 1 FROM Patient WHERE Id = @PatientId";
            command.Parameters.AddWithValue("@PatientId", prescriptionDto.PatientId);

            var check = command.ExecuteScalar();

            if ((int)check == 1)
            {
                command.CommandText = "SELECT 1 FROM Doctor WHERE Id = @DocId";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@DocId", prescriptionDto.DoctorId);
                
                var check1 = command.ExecuteScalar();

                if ((int)check1 == 1)
                {
                    command.CommandText = "INSERT INTO Prescription (Date, DueDate, IdPatient, IdDoctor) VALUES (@Date, @Duedate, @IdP, @IdD); SELECT Scope_Identity()";
                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@Date", prescriptionDto.Date);
                    command.Parameters.AddWithValue("@Duedate", prescriptionDto.DueDate);
                    command.Parameters.AddWithValue("@IdP", prescriptionDto.PatientId);
                    command.Parameters.AddWithValue("@IdD", prescriptionDto.DoctorId);

                    PK = (decimal?)command.ExecuteScalar();

                    if (PK == null)
                    {
                        return BadRequest("An error Occured");
                    }
                        
                }
                else
                {
                    return NotFound("Doctor");
                }
            }
            else
            {
                return NotFound("Patient");
            }
        }
        else
        {
            return BadRequest("DueDate same or less than date");
        }

        var response = new { P = prescriptionDto, Id = PK };
        return Ok(response);
    }
}