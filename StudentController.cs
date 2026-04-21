using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CourseProject.Models;
using ProjeAdin.Models;

namespace CourseProject.Controllers
{
    public class StudentController : Controller
    {
        DershaneProjeEntities db = new DershaneProjeEntities();

        public ActionResult StudentPage()
        {
            var Slist = db.Student.SqlQuery("SELECT * FROM Student").ToList();
            return View(Slist);
        }

        public ActionResult PayInstallements()
        {
            int studentId = Convert.ToInt32(Session["StudentId"]);
            var student = db.Student.SqlQuery("SELECT * FROM Student WHERE StudentId = @p0", studentId).FirstOrDefault();

            var payments = db.Payment.SqlQuery("SELECT * FROM Payment WHERE StudentID = @p0 ORDER BY PaymentDate DESC", studentId).ToList();
            ViewBag.Payments = payments;

            return View(student);
        }


        [HttpPost]
        public ActionResult AddStudent(Student student)
        {
            if (student != null)
            {
                var suitableClassrooms = db.Classroom.SqlQuery("SELECT * FROM Classroom WHERE Field = @p0 ORDER BY Number", student.Field).ToList();

                Classroom assignedClassroom = null;

                foreach (var classroom in suitableClassrooms)
                {
                    var count = db.Database.SqlQuery<int>(
                        "SELECT COUNT(*) FROM Student WHERE ClassroomId = @p0", classroom.ClassroomId).FirstOrDefault();

                    if (count < 5)
                    {
                        assignedClassroom = classroom;
                        student.ClassroomId = classroom.ClassroomId;
                        break;
                    }
                }

                if (assignedClassroom == null)
                {
                    ModelState.AddModelError("", "Uygun sınıf bulunamadı.");
                    return View(student);
                }

                decimal normalUcret = 100000;
                int taksitSayisi = student.Installments ?? 1;
                decimal artisYuzdesi = taksitSayisi * 2;
                decimal toplamUcret = normalUcret * (1 + (artisYuzdesi / 100));
                decimal odenenUcret = student.PaidFee ?? 0;

                db.Database.ExecuteSqlCommand(@"
                    INSERT INTO Student 
                    (FirstName, LastName, Gender, Phone, ParentName, ParentPhone, Field, Installments, TotalFee, PaidFee, ClassroomId, Username, Password)
                    VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12)",
                    student.FirstName,
                    student.LastName,
                    student.Gender,
                    student.Phone,
                    student.ParentName,
                    student.ParentPhone,
                    student.Field,
                    taksitSayisi,
                    toplamUcret,
                    odenenUcret,
                    student.ClassroomId,
                    student.Username,
                    student.Password);

                return RedirectToAction("StudentPage");
            }

            return View();
        }

        public ActionResult DeleteStudent(int a)
        {
            var student = db.Student.SqlQuery("SELECT * FROM Student WHERE StudentId = @p0", a).FirstOrDefault();

            if (student != null)
            {
                db.Database.ExecuteSqlCommand("DELETE FROM Payment WHERE StudentID = @p0", a);
                db.Database.ExecuteSqlCommand("DELETE FROM Student WHERE StudentId = @p0", a);
            }

            return RedirectToAction("StudentPage");
        }

        public ActionResult StudentEdit(int a)
        {
            var student = db.Student.SqlQuery("SELECT * FROM Student WHERE StudentId = @p0", a).FirstOrDefault();
            return View(student);
        }

        [HttpPost]
        public ActionResult StudentEdit(Student student)
        {
            Student newStudent = db.Student.SqlQuery("SELECT * FROM Student WHERE StudentId = @p0", student.StudentId).FirstOrDefault();

            newStudent.FirstName = student.FirstName;
            newStudent.LastName = student.LastName;
            newStudent.Gender = student.Gender;
            newStudent.Phone = student.Phone;
            newStudent.ParentName = student.ParentName;
            newStudent.ParentPhone = student.ParentPhone;
            newStudent.Field = student.Field;

            

            db.SaveChanges();
            return RedirectToAction("StudentPage");
        }

        public ActionResult PayInstallementsPage()
        {
            if (Session["StudentId"] == null)
            {
                return RedirectToAction("Login", "Home");
            }

            int studentId = Convert.ToInt32(Session["StudentId"]);

            var student = db.Student.SqlQuery("SELECT * FROM Student WHERE StudentId = @p0", studentId).FirstOrDefault();
            if (student == null)
                return HttpNotFound("\u00d6\u011frenci bulunamad\u0131.");

            var payments = db.Payment.SqlQuery("SELECT * FROM Payment WHERE StudentID = @p0 ORDER BY PaymentDate DESC", studentId).ToList();

            ViewBag.Payments = payments;

            return View("PayInstallementsPage", student);
        }

        [HttpPost]

       
        public ActionResult PayInstallementsPagePost()
        {
            int studentId = Convert.ToInt32(Session["StudentId"]);

            var student = db.Student.SqlQuery("SELECT * FROM Student WHERE StudentId = @p0", studentId).FirstOrDefault();
            if (student == null)
                return HttpNotFound("Öğrenci bulunamadı.");

            if (student.PaidFee >= student.TotalFee)
            {
                TempData["Message"] = "Tüm taksitler ödenmiştir.";
                return RedirectToAction("PayInstallements");
            }

            var taksitTutari = Math.Round((decimal)(student.TotalFee / student.Installments), 2);
            var kalanOdeme = student.TotalFee - student.PaidFee;

            if (taksitTutari > kalanOdeme)
                taksitTutari = (decimal)kalanOdeme;

            db.Database.ExecuteSqlCommand("UPDATE Student SET PaidFee = PaidFee + @p0 WHERE StudentId = @p1", taksitTutari, studentId);

            db.Database.ExecuteSqlCommand("INSERT INTO Payment (StudentID, PaymentDate, Amount) VALUES (@p0, GETDATE(), @p1)",
                studentId, taksitTutari);

            TempData["Message"] = "Ödeme başarıyla yapıldı.";

            // Ödeme sonrası sayfayı yeniden GET olarak çağır
            return RedirectToAction("PayInstallements");
        }


    }
}
