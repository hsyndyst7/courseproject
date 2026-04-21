using System;
using System.Collections.Generic;
using System.Web.Mvc;
using CourseProject.Models;
using System.Linq;

namespace CourseProject.Controllers
{
    public class CourseController : Controller
    {
        DershaneProjeEntities db = new DershaneProjeEntities();

        public ActionResult CoursePage()
        {
            var courses = db.Course.SqlQuery("SELECT * FROM Course").ToList();
            var teachers = db.Teacher.SqlQuery("SELECT * FROM Teacher").ToList();

            ViewBag.Teacher = teachers;
            return View(courses);
        }

        public ActionResult AddCourse(Course course)
        {
            if (course != null)
            {
                db.Database.ExecuteSqlCommand(@"
                    INSERT INTO Course (Name, Field, TeacherId)
                    VALUES (@p0, @p1, @p2)",
                    course.Name, course.Field, course.TeacherId);

                return RedirectToAction("CoursePage");
            }
            else
            {
                return View();
            }
        }

        public ActionResult DeleteCourse(int id)
        {
            if (id > 0)
            {
                db.Database.ExecuteSqlCommand("DELETE FROM Schedule WHERE CourseId = @p0", id);
                db.Database.ExecuteSqlCommand("DELETE FROM Course WHERE CourseId = @p0", id);
            }

            return RedirectToAction("CoursePage");
        }

        public ActionResult CourseEdit(int id)
        {
            var course = db.Course.SqlQuery("SELECT * FROM Course WHERE CourseId = @p0", id).FirstOrDefault();
            ViewBag.Teachers = db.Teacher.SqlQuery("SELECT * FROM Teacher").ToList();

            return View(course);
        }

        [HttpPost]
        public ActionResult CourseEdit(Course course)
        {
            db.Database.ExecuteSqlCommand(@"
                UPDATE Course SET Name = @p0, Field = @p1, TeacherId = @p2
                WHERE CourseId = @p3",
                course.Name, course.Field, course.TeacherId, course.CourseId);

            return RedirectToAction("CoursePage");
        }
    }
}