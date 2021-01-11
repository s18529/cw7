using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Zad3.Helpers;
using Zad3.Models;

namespace Zad3.Services
{
    public class StudnetDbService : IStudentDbService
    {
        private string connectionString = "Data Source=db-mssql;Initial Catalog=s18529;Integrated Security=True";
        public MyHelper AddStudent(Student student)
        {
            if (student.IndexNumber.Equals(null) || student.FirstName.Equals(null) || student.LastName.Equals(null) || student.Bdate == null || student.Studies.Equals(null))
            {
                return new MyHelper("nie podano wszystkich informacji", -1);
            }
            using (var client = new SqlConnection(connectionString))
            using (var command = new SqlCommand())
            {
                command.Connection = client;
                client.Open();
                var transaction = client.BeginTransaction();
                command.Transaction = transaction;
                command.CommandText = "select IndexNumber from Student";
                var dr = command.ExecuteReader();
                while (dr.Read())
                {
                    if (dr[0].Equals(student.IndexNumber))
                    {
                        dr.Close();
                        transaction.Rollback();
                        return new MyHelper("niepoprawny index studenta", -1);
                    }
                }
                dr.Close();
                int idStudy;
                command.CommandText = "select idStudy from Studies where name = @name";
                command.Parameters.AddWithValue("name", student.Studies);
                dr = command.ExecuteReader();
                if (!dr.Read())
                {
                    dr.Close();
                    transaction.Rollback();
                    return new MyHelper("Niepoprawne studia", -1);
                }
                else
                {
                    idStudy = int.Parse(dr[0].ToString());
                }
                dr.Close();
                command.CommandText = "select * from enrollment where semester=2 and idStudy = @idStudy";
                command.Parameters.AddWithValue("idStudy", idStudy);

                dr = command.ExecuteReader();
                int id = 0;
                Enrollment enrollment = new Enrollment();
                if (!dr.Read())
                {
                    command.CommandText = "select max(idEnrollment) from Enrollment";
                    dr.Close();
                    var dr2 = command.ExecuteReader();

                    id = int.Parse(dr2[0].ToString());
                    id++;
                    dr2.Close();
                    command.CommandText = "insert into Enrollment values (@id, 1, @idStudy, @date)";
                    command.Parameters.AddWithValue("date", DateTime.Now);
                    command.Parameters.AddWithValue("id", id);
                    command.ExecuteNonQuery();
                    enrollment.IdEnrollment = id;
                    enrollment.IdStudy = idStudy;
                    enrollment.StartDate = DateTime.Now;
                    enrollment.Semester = 1;
                }
                else
                {
                    id = int.Parse(dr[0].ToString());
                    enrollment.IdEnrollment = id;
                    enrollment.IdStudy = int.Parse(dr[2].ToString());
                    enrollment.StartDate = DateTime.Parse(dr[3].ToString());
                    enrollment.Semester = int.Parse(dr[1].ToString());
                    command.Parameters.AddWithValue("id", id);
                }
                dr.Close();
                command.CommandText = "insert into student values (@Inumber, @Fname, @Lname, @Bdate, @id)";
                command.Parameters.AddWithValue("Inumber", student.IndexNumber);
                command.Parameters.AddWithValue("Fname", student.FirstName);
                command.Parameters.AddWithValue("Lname", student.LastName);
                command.Parameters.AddWithValue("Bdate", student.Bdate);
                command.ExecuteNonQuery();
                transaction.Commit();
                MyHelper myHelper = new MyHelper("Dodano", 0);
                myHelper.enrollment = enrollment;

                return myHelper;

            }
        }

        public MyHelper Promote(Study study)
        {
            using (var client = new SqlConnection(connectionString))
            using (var command = new SqlCommand())
            {
                command.Connection = client;
                client.Open();
                if (study.Semestr == null || study.Studies.Equals(null))
                {
                    return new MyHelper("błedne dane",-1);
                }
                command.CommandText = "select * from enrollment e join Studies s on s.IdStudy=e.IdStudy where semester=@sem and s.name =@name";
                command.Parameters.AddWithValue("sem", study.Semestr);
                command.Parameters.AddWithValue("name", study.Studies);
                var dr = command.ExecuteReader();
                if (!dr.Read())
                {
                    return new MyHelper("Nie ma takiego enrollmentu", -1);
                }
                dr.Close();
                using (var com = new SqlCommand())
                {
                    com.Connection = client;
                    com.CommandText = "pormote";
                    com.CommandType = CommandType.StoredProcedure;
                    com.Parameters.AddWithValue("Studies", study.Studies);
                    com.Parameters.AddWithValue("Semester", study.Semestr);
                    command.ExecuteNonQuery();
                }
                command.CommandText = "select * from enrollment e join Studies st on e.IdStudy = st.IdStudy where e.Semester = (@sem+1) and st.Name = @name";
                dr = command.ExecuteReader();
                if (!dr.Read())
                {
                    return new MyHelper("problem przy dodawaniu enrolmment", -1);
                }
                Enrollment enrollment = new Enrollment();
                enrollment.IdEnrollment = int.Parse(dr[0].ToString());
                enrollment.IdStudy = int.Parse(dr[2].ToString());
                enrollment.StartDate = DateTime.Parse(dr[3].ToString());
                enrollment.Semester = int.Parse(dr[1].ToString());
                MyHelper helper = new MyHelper("promowano", 0);
                helper.enrollment = enrollment;
                return helper;
            }
        }
    }
}
