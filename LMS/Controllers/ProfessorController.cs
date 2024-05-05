using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
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

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
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

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var query = from s in db.Students
                        join e in db.Enrolleds
                        on s.UId equals e.StudentId
                        join c in db.Classes
                        on e.ClassId equals c.ClassId
                        join cou in db.Courses
                        on c.CourseId equals cou.CourseId
                        where cou.Department == subject && cou.Num == num && c.Season == season && c.Year == year
                        select new
                        {
                            fname = s.FirstName,
                            lname = s.LastName,
                            uid = s.UId,
                            dob = s.Dob,
                            grade = e.Grade
                        };


            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            if (category != null)
            {
                var query = from c in db.Classes
                        join ac in db.AssignmentCategories
                        on c.ClassId equals ac.ClassId
                        join a in db.Assignments
                        on ac.CategoryId equals a.CategoryId
                        where c.Course.Num == num
                        && c.Course.Department == subject
                        && c.Season == season
                        && c.Year == year
                        && ac.Name == category
                        select new
                        {
                            aname = a.Name,
                            cname = ac.Name,
                            due = a.Due,
                            submissions = a.Submissions.Count()
                        };
                return Json(query.ToArray());
            }
            else
            {
                var query = from c in db.Classes
                        join ac in db.AssignmentCategories
                        on c.ClassId equals ac.ClassId
                        join a in db.Assignments
                        on ac.CategoryId equals a.CategoryId
                        where c.Course.Num == num
                        && c.Course.Department == subject
                        && c.Season == season
                        && c.Year == year
                        select new
                        {
                            aname = a.Name,
                            cname = ac.Name,
                            due = a.Due,
                            submissions = a.Submissions.Count()
                        };
                return Json(query.ToArray());
            }
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var query = from c in db.Classes
                        join ac in db.AssignmentCategories
                        on c.ClassId equals ac.ClassId
                        where c.Course.Num == num
                        && c.Course.Department == subject
                        && c.Season == season
                        && c.Year == year
                        select new
                        {
                            name = ac.Name,
                            weight = ac.Weight
                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            var query = from c in db.Classes
                        where c.Course.Num == num
                        && c.Course.Department == subject
                        && c.Season == season
                        && c.Year == year
                        select c.ClassId;

            AssignmentCategory assignmentCategory = new AssignmentCategory();
            assignmentCategory.Weight = (uint)catweight;
            assignmentCategory.Name = category;
            assignmentCategory.ClassId = query.First();

            try
            {
                db.AssignmentCategories.Add(assignmentCategory);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }

            return Json(new { success = true });
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            var query = from c in db.Classes
                        join ac in db.AssignmentCategories
                        on c.ClassId equals ac.ClassId
                        where c.Course.Num == num
                        && c.Course.Department == subject
                        && c.Season == season
                        && c.Year == year
                        && ac.Name == category
                        select ac.CategoryId;

            Assignment assignment = new Assignment();
            assignment.Name = asgname;
            assignment.Contents = asgcontents;
            assignment.Due = asgdue;
            assignment.Points = (uint)asgpoints;
            assignment.CategoryId = query.First();

            try
            {
                db.Assignments.Add(assignment);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }

            var classquery = from c in db.Classes
                             where c.Course.Num == num
                             && c.Course.Department == subject
                             && c.Season == season
                             && c.Year == year
                             select c.ClassId;

            UpdateGrades(classquery.Single());

            return Json(new { success = true });
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            var query = from c in db.Classes
                        join ac in db.AssignmentCategories
                        on c.ClassId equals ac.ClassId
                        join a in db.Assignments
                        on ac.CategoryId equals a.CategoryId
                        join s in db.Submissions
                        on a.AssignId equals s.AssignId
                        where c.Course.Num == num
                        && c.Course.Department == subject
                        && c.Season == season
                        && c.Year == year
                        && ac.Name == category
                        && a.Name == asgname
                        select new
                        {
                            fname = s.Student.FirstName,
                            lname = s.Student.LastName,
                            uid = s.Student.UId,
                            time = s.Time,
                            score = s.Score,
                        };



            return Json(query.ToArray());
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            var query = from c in db.Classes
                        join ac in db.AssignmentCategories
                        on c.ClassId equals ac.ClassId
                        join a in db.Assignments
                        on ac.CategoryId equals a.CategoryId
                        join s in db.Submissions
                        on a.AssignId equals s.AssignId
                        where c.Course.Num == num
                        && c.Course.Department == subject
                        && c.Season == season
                        && c.Year == year
                        && ac.Name == category
                        && a.Name == asgname
                        && s.StudentId == uid
                        select s;

            try
            {
                query.First().Score = (uint)score;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }

            var classquery = from c in db.Classes
                             where c.Course.Num == num
                             && c.Course.Department == subject
                             && c.Season == season
                             && c.Year == year
                             select c.ClassId;

            UpdateGrades(classquery.Single(), uid);

            return Json(new { success = true });
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = from c in db.Classes
                        where c.ProfId == uid
                        select new
                        {
                            subject = c.Course.Department,
                            number = c.Course.Num,
                            name = c.Course.Name,
                            season = c.Season,
                            year = c.Year
                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Updates the class grade for a given student
        /// </summary>
        /// <param name="classId">ID of the class to assign a grade to</param>
        /// <param name="uid">Student ID</param>
        private void UpdateGrades(uint classId, string uid)
        {
            var query = from e in db.Enrolleds
                        join c in db.Classes
                        on e.ClassId equals c.ClassId
                        join ac in db.AssignmentCategories
                        on c.ClassId equals ac.ClassId
                        where c.ClassId == classId
                        join a in db.Assignments
                        on ac.CategoryId equals a.CategoryId
                        where c.ClassId == classId && e.StudentId == uid
                        select new { assignID = a.AssignId, courseID = c.CourseId, StudentId = e.StudentId , 
                            Points = a.Points, Weight= ac.Weight, catname = ac.Name, ClassId = c.ClassId};

            var query2 = from q in query
                        join s in db.Submissions
                        on new { A = q.assignID, C = uid} equals new {A = s.AssignId, C= s.StudentId }
                        into rightside
                        from r in rightside.DefaultIfEmpty()
                        select new
                        {
                            subscore = r == null ? 0 : r.Score,
                            assignpoints = q.Points,
                            categoryWeight = q.Weight,
                            catname = q.catname,
                            classId = q.ClassId
                        };

            // Maps category name to category weight
            Dictionary<string, uint> weights = new Dictionary<string, uint>();
            // Maps category name to subscore and totalpoints
            Dictionary<string, List<(uint, uint)>> assignments = new();

            double totalWeight = 0;
            foreach (var sub in query2.ToList())
            {
                if (assignments.ContainsKey(sub.catname))

                {
                    assignments[sub.catname].Add((sub.subscore, sub.assignpoints));
                }
                else
                {
                    assignments[sub.catname] = new List<(uint, uint)>
                    {
                        (sub.subscore, sub.assignpoints)
                    };

                    weights[sub.catname] = sub.categoryWeight;
                    totalWeight += sub.categoryWeight;
                }
            }

            double finalGrade = 0;
            foreach (var cat in assignments.Keys)
            {
                double totalEarned = 0;
                double totalPossible = 0;

                foreach(var list in assignments[cat])
                {
                    totalEarned += list.Item1;
                    totalPossible += list.Item2;
                }

                double grade = totalEarned / totalPossible * weights[cat];
                finalGrade += grade;
            }
            finalGrade *= 100.0 / totalWeight;

            string letterGrade = NumToLetterGrade(finalGrade);
            var classQuery = from e in db.Enrolleds
                             where e.StudentId == uid && e.ClassId == classId
                             select e;
            classQuery.Single().Grade = letterGrade;
            db.SaveChanges();
        }
        
        /// <summary>
        /// Updates class grades for all students in a class
        /// </summary>
        /// <param name="classId">ID of class to update</param>
        private void UpdateGrades(uint classId)
        {
            var query = (from e in db.Enrolleds
                        where e.ClassId == classId
                        select e.StudentId).ToList();

            foreach(var studId in query) 
            {
                UpdateGrades(classId, studId);
            }

        }

        /// <summary>
        /// Converts a number grade to a letter grade standard
        /// </summary>
        /// <param name="grade">Grade percentage out of 100</param>
        /// <returns></returns>
        private string NumToLetterGrade(double grade)
        {
            if (grade < 60)
                return "E";
            else if (grade < 63)
                return "D-";
            else if (grade < 67)
                return "D";
            else if (grade < 70)
                return "D+";
            else if (grade < 73)
                return "C-";
            else if (grade < 77)
                return "C";
            else if (grade < 80)
                return "C+";
            else if (grade < 83)
                return "B-";
            else if (grade < 87)
                return "B";
            else if (grade < 90)
                return "B+";
            else if (grade < 93)
                return "A-";
            else
                return "A";
        }



        /*******End code to modify********/
    }
}

