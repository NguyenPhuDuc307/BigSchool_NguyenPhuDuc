using BigSchool_NguyenPhuDuc.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BigSchool_NguyenPhuDuc.Controllers
{
    public class CoursesController : Controller
    {
        // GET: Courses
        BigSchoolContext context = new BigSchoolContext();
        public ActionResult Create()
        {
            Course objCourse = new Course();
            objCourse.lstCategory = context.Categories.ToList();

            return View(objCourse);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Course objcourse)
        {
            // Không xét valid LectureId vì bằng user đăng nhập
            ModelState.Remove("LecturerId");
            if (!ModelState.IsValid)
            {
                objcourse.lstCategory = context.Categories.ToList();
                return View("Create", objcourse);
            }
            //lấy Login user ID
            ApplicationUser user = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
            objcourse.LecturerId = user.Id;
            //add vào CSDL
            context.Courses.Add(objcourse);
            context.SaveChanges();
            //Trở về Home, Action Index
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Attending()
        {
            ApplicationUser currentUser = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
            var listAttendances = context.Attendances.Where(p => p.Attendee == currentUser.Id).ToList();
            var courses = new List<Course>();
            foreach (Attendance temp in listAttendances)
            {
                Course objCourse = temp.Course;
                objCourse.LecturerName = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(objCourse.LecturerId).Name;
                courses.Add(objCourse);
            }
            return View(courses);
        }

        public ActionResult Mine()
        {
            ApplicationUser currentUser = System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
            var courses = context.Courses.Where(c => c.LecturerId == currentUser.Id && c.DateTime > DateTime.Now).ToList();
            foreach (Course i in courses)
            {
                i.LecturerName = currentUser.Name;
            }
            return View(courses);

        }

        public ActionResult Edit(int id)
        {
            var course = context.Courses.First(m => m.Id == id);
            course.lstCategory = context.Categories.ToList();
            return View(course);
        }

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            var course = context.Courses.First(m => m.Id == id);
            var Place = collection["Place"];
            var Datime = collection["Datime"];
            var CategoryId = collection["CategoryId"];

            if (string.IsNullOrEmpty(CategoryId))
            {
                ViewData["Error"] = "Don't empty!";
            }
            else
            {
                course.Place = Place.ToString();
                course.DateTime = Convert.ToDateTime(Datime);
                course.CategoryId = Convert.ToInt32(CategoryId);
                UpdateModel(course);
                context.SaveChanges();
                return RedirectToAction("Mine");
            }
            return this.Edit(id);
        }

        public ActionResult Delete(int id)
        {
            var course = context.Courses.First(m => m.Id == id);
            return View(course);
        }
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            var course = context.Courses.First(m => m.Id == id);
            try
            {
                var attendance = context.Attendances.First(m => m.CoursesId == course.Id);
                context.Attendances.Remove(attendance);
                context.SaveChanges();
            }
            catch
            {

            }
            context.Courses.Remove(course);
            context.SaveChanges();
            return RedirectToAction("Mine");
        }
        public ActionResult LectureIamGoing()
        {
            ApplicationUser currentUser =
            System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>()
            .FindById(System.Web.HttpContext.Current.User.Identity.GetUserId());
            BigSchoolContext context = new BigSchoolContext();
            //danh sách giảng viên được theo dõi bởi người dùng (đăng nhập) hiện tại
            var listFollwee = context.Followings.Where(p => p.FollowerId ==
            currentUser.Id).ToList();
            //danh sách các khóa học mà người dùng đã đăng ký
            var listAttendances = context.Attendances.Where(p => p.Attendee ==
            currentUser.Id).ToList();
            var courses = new List<Course>();
            foreach (var course in listAttendances)
            {
                foreach (var item in listFollwee)
                {
                    if (item.FolloweeId == course.Course.LecturerId)
                    {
                        Course objCourse = course.Course;
                        objCourse.LecturerName =
                        System.Web.HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>()
                        .FindById(objCourse.LecturerId).Name;
                        courses.Add(objCourse);
                    }
                }
            }
            return View(courses);
        }
    }
}