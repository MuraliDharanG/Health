using HospitalCloud;
using HospitalCloud.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using MongoDB.Driver.Builders;
namespace HospitalCould.Controllers
{
    public class DoctorsController : Controller
    {

        public ActionResult ListPatients()
        {
            var docId = ((CustomPrincipal)HttpContext.User).UserId;
            var data = ReadPatientRegisterationTable(database()).FindAll().Where(s => s.DoctorId == docId).Select(s => s);
            return View(data);
        }

        [HttpGet]
        public ActionResult AddPatient()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddPatient(PatientRegistration patient)
        {
            var db = database();
            patient.DoctorId = ((CustomPrincipal)HttpContext.User).UserId;

            var patientid = ReadPatientRegisterationTable(db).FindAll().Count();

            patient.PatientId = "PA" + patientid.ToString("D4");
            Createpatient(patient, db);
             this.AddToastMessage("Patient Created", "Add New Patient", ToastType.Info);
            return RedirectToAction("ListPatients", "Doctors");
        }

        private MongoDatabase database()
        {
            string connectionString = "Server=localhost:27017";
            Console.WriteLine("Connecting MongoDB");
            MongoClient client = new MongoClient(connectionString);
            MongoServer server = client.GetServer();
            MongoDatabase mongoDatabase = server.GetDatabase("health");
            return mongoDatabase;
        }

        private MongoCollection<PatientRegistration> ReadPatientRegisterationTable(MongoDatabase mongoDatabase)
        {
            MongoCollection<PatientRegistration> patientRegistration = mongoDatabase.GetCollection<PatientRegistration>("PatientRegistration");
            return patientRegistration;
        }


        private MongoCollection<PatientRegistration> Createpatient(PatientRegistration patient, MongoDatabase mongoDatabase)
        {
            MongoCollection<PatientRegistration> patientRegistration = mongoDatabase.GetCollection<PatientRegistration>("PatientRegistration");
            patientRegistration.Insert(patient);
            return patientRegistration;
        }

        [HttpGet]
        public ActionResult EditPatientInfo(string Id)
        {
            MongoCollection<PatientRegistration> patientRegistration = database().GetCollection<PatientRegistration>("PatientRegistration");
            var model = patientRegistration.FindAll().FirstOrDefault(s => s.Id == ObjectId.Parse(Id));
            return View(model);
        }
        //Email and Password

        [HttpPost]
        public ActionResult EditPatientInfo(PatientRegistration model, string patientId)
        {
            MongoCollection<PatientRegistration> patientRegistration = database().GetCollection<PatientRegistration>("PatientRegistration");
            var query = Query.EQ("_id", ObjectId.Parse(patientId));
            var update = Update.Set("Email", model.Email).Set("Password", model.Password);
            patientRegistration.Update(query, update);
            return RedirectToAction("ListPatients", "Doctors");
        }


        public ActionResult ListPatientReport()
        {
            var DoctorId = ((CustomPrincipal)HttpContext.User).UserId;
            var patientreports = ReadCloudFilesTable(database()).FindAll().Where(s => s.DoctorId == DoctorId);

            return View(patientreports);
        }

        [HttpGet]
        public ActionResult UploadPatientReport()
        {
            var DoctorId = ((CustomPrincipal)HttpContext.User).UserId;

            ViewBag.PatientList = new SelectList(ReadPatientRegisterationTable(database()).FindAll().Where(s => s.DoctorId == DoctorId), "PatientId", "PatientName");
            return View();
        }

        [HttpPost]
        public ActionResult UploadPatientReport(CloudFiles cloudfiles, HttpPostedFileBase report)
        {
            string skey = null;
            cloudfiles.DoctorId = ((CustomPrincipal)HttpContext.User).UserId;
            cloudfiles.Date = DateTime.Now;
            cloudfiles.DoctorName = ((CustomPrincipal)HttpContext.User).Username;

            var chars = "ABCDEFGHI1234567890JKLMNOPQRSTUVWXYZabcdefghijklmn#@!*&%opqrstuvwxyz";
            var random = new Random();
            var result = new string(
                Enumerable.Repeat(chars, 5)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());

            skey = result.ToString();
            cloudfiles.SecretKey = skey;

            cloudfiles.Filename = Path.GetFileName(report.FileName);
            cloudfiles.Contentype = report.ContentType;
            cloudfiles.PatientName = ReadPatientRegisterationTable(database()).FindAll().FirstOrDefault(s => s.PatientId == cloudfiles.PatientId).PatientName;
            using (Stream fs = report.InputStream)
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    byte[] bytes = br.ReadBytes((Int32)fs.Length);
                    var db = database();
                    cloudfiles.Data = bytes;
                    CreateCloudFiles(cloudfiles, db);
                }
            }
            this.AddToastMessage("", "Updated Patient Info Sucessfull", ToastType.Info);
            return RedirectToAction("ListPatientReport", "Doctors");
        }

        public void Download(string id)
        {
            //int id = int.Parse((sender as LinkButton).CommandArgument);
            //string User_ID = "d";
            this.AddToastMessage("Download", "Download Patient information", ToastType.Info);
            byte[] bytes;
            string fileName, contentType;
            var db = database();
            var data = ReadCloudFilesTable(db).FindAll().FirstOrDefault(s => s.Id.ToString() == id);
            bytes = data.Data;
            contentType = data.Contentype;
            fileName = data.Filename;

            Response.Clear();
            Response.Buffer = true;
            Response.Charset = "";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.ContentType = contentType;
            Response.AppendHeader("Content-Disposition", "attachment; filename=" + fileName);
            Response.BinaryWrite(bytes);
            Response.Flush();
            Response.End();
        }
        [HttpGet]
        public ActionResult PatientReportRequest()
        {
            return View();
        }

        [HttpPost]
        public ActionResult PatientReportRequest(string DoctorId, string PatientId)
        {
            var db = database();
            var docId = ReadUserRegisterationTable(db).FindAll().FirstOrDefault(s => s.DoctorId == DoctorId).Id;
            var Result = ReadCloudFilesTable(db).FindAll()
                            .Where(s => s.PatientId == PatientId && s.DoctorId == docId.ToString());
            var patienskdetails = ReadSkDetails(db).FindAll().Where(s => s.ResDocId == docId.ToString());
            //ReadSkDetails(database()).FindAll().Where(s => s.ResDocId == docId && s.FileId == fileId).Count()
            var resObject = Result.Select(a
                => new SkViewModel()
                {
                    PatientName = a.PatientName,
                    DoctorName = a.DoctorName,
                    FileName = a.Filename,
                    Topic = a.Filename,
                    Status = (patienskdetails.FirstOrDefault(s=>s.FileId==a.Id.ToString())!=null?
                    patienskdetails.FirstOrDefault(s=>s.FileId==a.Id.ToString()).ResponseDetails:"Not Approved"),
                    DoctorId = a.DoctorId,
                    Id = a.Id.ToString()
                });
            //this.AddToastMessage("Report Request", "Send Request Sucessfully", ToastType.Info);
            return View("SendRequestResult", resObject);
        }

        [HttpGet]
        public ActionResult SendRequest()
        {
            return View();
        }
        [HttpGet]
        public ActionResult SendRequestForFile(string docId, string fileId)
        {
            var rpt = ReadCloudFilesTable(database()).FindAll()
                .FirstOrDefault(s => s.DoctorId == docId && s.Id.ToString() == fileId);

            var skrecord = new SkDetails()
            {
                ReqDocId = ((CustomPrincipal)HttpContext.User).UserId,
                ResDocId = docId,
                FileId = fileId,
                DateOfRequest = DateTime.Now.ToShortDateString(),

                DownloadDetails = "Waiting For Download",
                ResponseDetails = "not responded"
            };
            CreateSkDetails(skrecord, database());
            this.AddToastMessage("Report Request", "Send Request Sucessfully", ToastType.Info);
            return View("PatientReportRequest");
        }
        [HttpGet]
        public ActionResult OtherRequest()
        {
            var userId = ((CustomPrincipal)HttpContext.User).UserId;
            var requests = ReadSkDetails(database()).FindAll().Where(s => s.ResDocId == userId);

            return View(requests);
        }

        [HttpGet]
        public ActionResult OtherRequestForApproved(string status, string RID)
        {
            if (status == "Approved")
            {
                var req = ReadSkDetails(database()).FindAll().FirstOrDefault(s => s.Id.ToString() == RID);

                var toEmail = ReadUserRegisterationTable(database()).FindAll().FirstOrDefault(s => s.Id.ToString() == req.ReqDocId).Email;
                var reqCloudFiles = ReadCloudFilesTable(database()).FindAll().FirstOrDefault(s => s.Id.ToString() == req.FileId);
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                mail.From = new MailAddress("myprojectmail123@gmail.com");
                mail.To.Add(toEmail);
                mail.Subject = "Account Details";

                mail.Body = "Hi, \nPlease find the secret key for downloading the file Below \n\nSecret Key : " + reqCloudFiles.SecretKey + "\n\nThanks. ";

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("myprojectmail123@gmail.com", "project123");
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);
                var isUpdated = UpdateSkDetails(database(), RID);
                this.AddToastMessage(" Approved", "Approved For Download", ToastType.Info);
                return RedirectToAction("OtherRequest");
            }
            else
            {
                this.AddToastMessage("Not Approved", "Not Approved For Download", ToastType.Info);
                return RedirectToAction("OtherRequest");
            }
        }
        public ActionResult DownloadRequestFile(string fileId)
        {
            ViewBag.FileId = fileId;

            return View();
        }
        [HttpPost]
        public ActionResult DownloadRequestForFile(string fileId, string SecurityKey)
        {
            byte[] bytes;
            string fileName, contentType;
            var db = database();
            var data = ReadCloudFilesTable(db).FindAll().FirstOrDefault(s => s.Id.ToString() == fileId);
            if (data.SecretKey == SecurityKey)
            {
                bytes = data.Data;
                contentType = data.Contentype;
                fileName = data.Filename;

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.ContentType = contentType;
                Response.AppendHeader("Content-Disposition", "attachment; filename=" + fileName);
                Response.BinaryWrite(bytes);
                Response.Flush();
                Response.End();
                return RedirectToAction("DownloadRequestFile");
            }
            else
            {
                this.AddToastMessage("", "Security Key Not Valid", ToastType.Error);
                Response.Write("Security Key Not Valid");
                return RedirectToAction("DownloadRequestFile");
            }
        }

        private static MongoCollection<SkDetails> UpdateSkDetails(MongoDatabase mongoDatabase, string RID)
        {
            MongoCollection<SkDetails> skdetails = mongoDatabase.GetCollection<SkDetails>("SkDetails");
            var query = Query.EQ("_id", ObjectId.Parse(RID));
            var update = Update.Set("ResponseDetails", "approved").Set("DataOfResponse", DateTime.Now.ToShortDateString());
            skdetails.Update(query, update);
            return skdetails;
        }

        private MongoCollection<UserRegisteration> ReadUserRegisterationTable(MongoDatabase mongoDatabase)
        {
            MongoCollection<UserRegisteration> UserRegisteration = mongoDatabase.GetCollection<UserRegisteration>("UserRegisteration");
            return UserRegisteration;
        }

        private MongoCollection<SkDetails> ReadSkDetails(MongoDatabase mongoDatabase)
        {
            MongoCollection<SkDetails> skDetails = mongoDatabase.GetCollection<SkDetails>("SkDetails");
            return skDetails;
        }
        private MongoCollection<SkDetails> CreateSkDetails(SkDetails skDetailsRecord, MongoDatabase mongoDatabase)
        {
            MongoCollection<SkDetails> skDetails = mongoDatabase.GetCollection<SkDetails>("SkDetails");
            skDetails.Insert(skDetailsRecord);
            return skDetails;
        }
        private MongoCollection<CloudFiles> CreateCloudFiles(CloudFiles cloudFileRecord, MongoDatabase mongoDatabase)
        {
            MongoCollection<CloudFiles> cloudFiles = mongoDatabase.GetCollection<CloudFiles>("CloudFiles");
            cloudFiles.Insert(cloudFileRecord);
            return cloudFiles;
        }
        private MongoCollection<CloudFiles> ReadCloudFilesTable(MongoDatabase mongoDatabase)
        {
            MongoCollection<CloudFiles> cloudFiles = mongoDatabase.GetCollection<CloudFiles>("CloudFiles");
            return cloudFiles;
        }


    }

    public class SkViewModel
    {
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string FileName { get; set; }
        public string Topic { get; set; }
        public string Status { get; set; }
        public string DoctorId { get; set; }
        public string Id { get; set; }
    }
}
