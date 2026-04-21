using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CourseProject.Models;

namespace CourseProject.Controllers
{
    public class HomeController : Controller
    {
        DershaneProjeEntities db = new DershaneProjeEntities();

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string Username, string Password)
        {
            var user = db.Student.SqlQuery("SELECT * FROM Student WHERE Username = @p0 AND Password = @p1", Username, Password).FirstOrDefault();

            if (user != null)
            {
                Session["giris"] = true;
                Session["fullname"] = user.FirstName + " " + user.LastName;
                Session["StudentId"] = user.StudentId;
                return RedirectToAction("StudentHomePage");
            }
            else
            {
                ViewBag.ErrorMessage = "Kullanıcı adı veya şifre hatalı!";
                return View();
            }
        }

        public ActionResult LoginYönetici()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LoginYönetici(string Username, string Password)
        {
            var user = db.Teacher.SqlQuery("SELECT * FROM Teacher WHERE Username = @p0 AND Password = @p1", Username, Password).FirstOrDefault();

            if (user != null)
            {
                Session["giris"] = true;
                Session["fullname"] = user.FirstName + " " + user.LastName;
                Session["TeacherId"] = user.TeacherId;
                return RedirectToAction("HomePage");
            }
            else
            {
                ViewBag.ErrorMessage = "Kullanıcı adı veya şifre hatalı!";
                return View();
            }
        }

        public ActionResult HomePage()
        {
            return View();
        }

        public ActionResult StudentHomePage()
        {
            return View();
        }

        public ActionResult SchedulePage(string dayFilter = null)
        {
            ViewBag.Classrooms = db.Classroom.SqlQuery("SELECT * FROM Classroom").ToList();
            ViewBag.Courses = db.Course.SqlQuery("SELECT * FROM Course").ToList();

            string query = @"SELECT s.* FROM Schedule s 
                     JOIN Course c ON s.CourseId = c.CourseId
                     JOIN Classroom r ON s.ClassroomId = r.ClassroomId";

            List<Schedule> result;

            if (!string.IsNullOrEmpty(dayFilter))
            {
                query += " WHERE s.DayOfWeek = @p0 ORDER BY s.ClassroomId ASC";
                result = db.Schedule.SqlQuery(query, dayFilter).ToList();
            }
            else
            {
                query += " ORDER BY s.ClassroomId ASC";
                result = db.Schedule.SqlQuery(query).ToList();
            }

            ViewBag.ScheduleList = result;
            ViewBag.SelectedDay = dayFilter;

            return View(new Schedule());
        }

        [HttpPost]
        public ActionResult SchedulePage(Schedule schedule, string dayFilter = null)
        {
            ViewBag.Classrooms = db.Classroom.SqlQuery("SELECT * FROM Classroom").ToList();
            ViewBag.Courses = db.Course.SqlQuery("SELECT * FROM Course").ToList();

            string baseQuery = @"SELECT s.* FROM Schedule s 
                         JOIN Course c ON s.CourseId = c.CourseId
                         JOIN Classroom r ON s.ClassroomId = r.ClassroomId";

            List<Schedule> result;

            if (!string.IsNullOrEmpty(dayFilter))
            {
                baseQuery += " WHERE s.DayOfWeek = @p0 ORDER BY s.ClassroomId ASC";
                result = db.Schedule.SqlQuery(baseQuery, dayFilter).ToList();
            }
            else
            {
                baseQuery += " ORDER BY s.ClassroomId ASC";
                result = db.Schedule.SqlQuery(baseQuery).ToList();
            }

            ViewBag.ScheduleList = result;
            ViewBag.SelectedDay = dayFilter;

            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Lütfen formu doğru doldurun.";
                return View(schedule);
            }

            string conflictQuery = @"SELECT * FROM Schedule 
                             WHERE ClassroomId = @p0 AND DayOfWeek = @p1 AND 
                             ((@p2 >= StartTime AND @p2 < EndTime) OR 
                              (@p3 > StartTime AND @p3 <= EndTime) OR 
                              (@p2 <= StartTime AND @p3 >= EndTime))";

            var conflict = db.Schedule.SqlQuery(conflictQuery, schedule.ClassroomId, schedule.DayOfWeek, schedule.StartTime, schedule.EndTime).Any();

            if (conflict)
            {
                ViewBag.Message = "Seçilen sınıfta bu saatlerde başka bir ders var!";
                return View(schedule);
            }

            db.Schedule.Add(schedule);
            db.SaveChanges();

            // yeniden sıralı şekilde yükle
            string reloadQuery = @"SELECT s.* FROM Schedule s 
                           JOIN Course c ON s.CourseId = c.CourseId
                           JOIN Classroom r ON s.ClassroomId = r.ClassroomId
                           ORDER BY s.ClassroomId ASC";

            ViewBag.ScheduleList = db.Schedule.SqlQuery(reloadQuery).ToList();
            ViewBag.Message = "Ders başarıyla eklendi.";
            return View(new Schedule());
        }

        [HttpPost]
        public ActionResult DeleteSchedule(int id, string dayFilter = null)
        {
            var schedule = db.Schedule.Find(id);
            if (schedule != null)
            {
                db.Schedule.Remove(schedule);
                db.SaveChanges();
            }

            return RedirectToAction("SchedulePage", new { dayFilter = dayFilter });
        }





        public ActionResult Teacherpage()
        {
            var Tlist = db.Teacher.SqlQuery("SELECT * FROM Teacher").ToList();
            return View(Tlist);
        }

        public ActionResult AddTeacher(Teacher teacher)
        {
            if (teacher != null)
            {
                db.Teacher.Add(teacher);
                db.SaveChanges();
                return RedirectToAction("Teacherpage");
            }
            else
            {
                return View();
            }
        }

        public ActionResult DeleteTeacher(int a)
        {
            var teacher = db.Teacher.SqlQuery("SELECT * FROM Teacher WHERE TeacherId = @p0", a).FirstOrDefault();
            db.Teacher.Remove(teacher);
            db.SaveChanges();
            return RedirectToAction("Teacherpage");
        }

        public ActionResult TeacherEdit(int a)
        {
            var teacher = db.Teacher.SqlQuery("SELECT * FROM Teacher WHERE TeacherId = @p0", a).FirstOrDefault();
            return View(teacher);
        }

        [HttpPost]
        public ActionResult TeacherEdit(Teacher teacher)
        {
            var newTeacher = db.Teacher.SqlQuery("SELECT * FROM Teacher WHERE TeacherId = @p0", teacher.TeacherId).FirstOrDefault();

            newTeacher.FirstName = teacher.FirstName;
            newTeacher.LastName = teacher.LastName;
            newTeacher.Phone = teacher.Phone;

            db.SaveChanges();
            return RedirectToAction("Teacherpage");
        }
    }
}