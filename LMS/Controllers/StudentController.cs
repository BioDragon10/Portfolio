﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = from c in db.Classes
                        join e in db.Enrolleds
                        on c.ClassId equals e.ClassId
                        where e.StudentId == uid
                        select new
                        {
                            subject = c.Course.Department,
                            number = c.Course.Num,
                            name = c.Course.Name,
                            season = c.Season,
                            year = c.Year,
                            grade = e.Grade
                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {

            var query = from e in db.Enrolleds
                        join c in db.Classes
                        on e.ClassId equals c.ClassId
                        join ac in db.AssignmentCategories
                        on c.ClassId equals ac.ClassId
                        join a in db.Assignments
                        on ac.CategoryId equals a.CategoryId
                        where c.Course.Num == num
                        && c.Course.Department == subject
                        && c.Season == season
                        && c.Year == year
                        && e.StudentId == uid
                        select new
                        {
                            AssignID = a.AssignId,
                            aname = a.Name,
                            cname = ac.Name,
                            due = a.Due,

                        };

            var query2 = from q in query
                        join s in db.Submissions
                        on new { A = q.AssignID, B = uid} equals new {A = s.AssignId, B = s.StudentId}
                        into rightside

                        from r in rightside.DefaultIfEmpty()
                        
                        select new
                        {
                            aname = q.aname,
                            cname = q.cname,
                            due = q.due,
                            score = r == null ? null : (uint?) r.Score
                        };


            return Json(query2.ToArray());
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            var getAssignID = from c in db.Classes
                              join ac in db.AssignmentCategories
                              on c.ClassId equals ac.ClassId
                              join a in db.Assignments
                              on ac.CategoryId equals a.CategoryId
                              where c.Course.Num == num
                              && c.Course.Department == subject
                              && c.Season == season
                              && c.Year == year
                              && ac.Name == category
                              && a.Name == asgname
                              select a.AssignId;

            var alreadyASubmission = from s in db.Submissions
                                     where s.AssignId == getAssignID.First() && s.StudentId == uid
                                     select s;

            if (alreadyASubmission.Count() > 0)
            {
                Submission submission = alreadyASubmission.Single();

                submission.Time = DateTime.Now;
                submission.Contents = contents;

                db.SaveChanges();

                return Json(new { success = true });
            }
            else
            {
                Submission submission = new();

                submission.Score = 0;
                submission.Contents = contents;
                submission.Time = DateTime.Now;
                submission.AssignId = getAssignID.First();
                submission.StudentId = uid;

                try
                {
                    db.Submissions.Add(submission);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false });
                }

            }
        }


        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            var getStudentUID = from s in db.Students
                                where s.UId == uid
                                select s.UId;

            var getClassID = from c in db.Classes
                             where c.Course.Num == num && c.Course.Department == subject 
                             && c.Season == season && c.Year == year
                             select c.ClassId;

            Enrolled enrolled = new Enrolled();
            enrolled.Grade = "--";
            enrolled.StudentId = getStudentUID.First();
            enrolled.ClassId = getClassID.First();

            try
            {
                db.Enrolleds.Add(enrolled);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }


            return Json(new { success = true });
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            var query = (from e in db.Enrolleds
                        where e.StudentId == uid && e.Grade != "--"
                        select e.Grade).ToList();

            if (query.Count() == 0)
            {
                return Json(new { gpa = 0.0 });
            }

            double sum = 0;
            int numClasses = 0;
            foreach (var grd in query)
            {
                sum += GetGradePoint(grd);
                numClasses++;
            }
            double gpa = sum / numClasses;

            return Json(new { gpa = gpa });
        }

        /// <summary>
        /// Converts a letter grade to a grade point score based on credits earned
        /// </summary>
        /// <param name="grade">Standard letter grade</param>
        /// <returns></returns>
        private double GetGradePoint(string grade)
        {
            switch (grade)
            {
                case "A":
                    return 4.0f;
                case "A-":
                    return 3.7;
                case "B+":
                    return 3.3;
                case "B":
                    return 3.0;
                case "B-":
                    return 2.7;
                case "C+":
                    return 2.3;
                case "C":
                    return 2.0;
                case "C-":
                    return 1.7;
                case "D+":
                    return 1.3;
                case "D":
                    return 1.0;
                case "D-":
                    return 0.7;
                default:
                    return 0.0;
            }
        }
                
        /*******End code to modify********/

    }
}

