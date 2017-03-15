using HospitalCloud;
using HospitalCloud.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace HospitalCould.Controllers
{
    public class LoginController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login(string LoginId, string password) 
        {
            if (LoginId == "admin" && password == "admin@123")
            {
                CreateAuthendicationTicket(new User() { UserName = LoginId, Password = password }, "Admin");
                this.AddToastMessage("", "Login Sucessfully", ToastType.Success);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                if (patientLogin(LoginId, password))
                {
                    this.AddToastMessage("", "Login Sucessfully", ToastType.Success);
                    return RedirectToAction("Index", "Home");
                }
                else
                {

                    this.AddToastMessage("", "Login Failed", ToastType.Error);
                    return RedirectToAction("Index", "Login");
                }
            }
        }

        public bool patientLogin(string LoginId, string password) 
        {

            var patient = ReadPatientRegistrationTable(database()).FindAll().FirstOrDefault(s => s.PatientId == LoginId && s.Password == password);
            if (patient != null)
            {
                CreateAuthendicationTicket(new User() { UserId = patient.Id.ToString(), Password = patient.Password, Role = "Patient", UserName = patient.UserName }, "Patient");
                return true;
            }
            else
            {
                return false;
            }


        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            this.AddToastMessage("", "Logout Sucessfully", ToastType.Success);
            return RedirectToAction("Index", "Login");
        }

        private void CreateAuthendicationTicket(User model, string userRole)
        {

            var serializeModel = new User() { UserId = model.UserId, UserName = model.UserName, Password = model.Password, Role = userRole };
            //,UserImage = model.UserImage};
            var serializer = new JavaScriptSerializer();
            var userData = serializer.Serialize(serializeModel);
            var authTicket = new FormsAuthenticationTicket(1, model.UserName, DateTime.Now, DateTime.Now.AddHours(8), false, userData);
            var encTicket = FormsAuthentication.Encrypt(authTicket);
            var faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            Response.Cookies.Add(faCookie);
        }


        public ActionResult DoctorsLogin()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DoctorsLogin(string DoctorId, string Password, HttpPostedFileBase SecurityImage)
        {
            var selecteduser = ReadUserRegisterationTable(database()).FindAll().FirstOrDefault(s => s.DoctorId == DoctorId && s.Password == Password);
            if (selecteduser != null)
            {
                string imgPath;
                //SecurityImage.SaveAs("E:\\files\\" + SecurityImage.FileName);

                //imgPath = "E:\\files\\" +  SecurityImage.FileName;
                var ms = new MemoryStream();
                SecurityImage.InputStream.CopyTo(ms);


                // byte[] content = imgStream(imgPath);


                byte[] img = selecteduser.SecurityImange;
                if (ms.ToArray().Length == img.Length)
                {
                    CreateAuthendicationTicket(new User() { UserId = Convert.ToString(selecteduser.Id), UserName = selecteduser.UserName, Password = selecteduser.Password }, "Doctor");
                    this.AddToastMessage("", "Login Sucessfully", ToastType.Success);
                    return RedirectToAction("Index", "Home");

                }
                else
                {
                    this.AddToastMessage("", "Login Failed", ToastType.Error);
                    //this.AddToastMessage("error", "username or Password is Incorrect", ToastType.Warning);
                    Response.AddHeader("Login Failed", "Lgoin Failed");
                    //System.IO.File.Delete(imgPath);

                }
            }
            else
            {
                this.AddToastMessage("", "Incorrect User name or password!", ToastType.Error);
                //Label1.Text = "Incorrect User name or password!";
            }
            return View();
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

        private MongoCollection<UserRegisteration> ReadUserRegisterationTable(MongoDatabase mongoDatabase)
        {
            MongoCollection<UserRegisteration> UserRegisteration = mongoDatabase.GetCollection<UserRegisteration>("UserRegisteration");
            return UserRegisteration;
        }

        private MongoCollection<PatientRegistration> ReadPatientRegistrationTable(MongoDatabase mongoDatabase)
        {
            MongoCollection<PatientRegistration> patientRegistration = mongoDatabase.GetCollection<PatientRegistration>("PatientRegistration");
            return patientRegistration;
        }



        private byte[] imgStream(string fileName)
        {
            MemoryStream stream = new MemoryStream();
        tryagain:
            try
            {
                Bitmap bmp = new Bitmap(fileName);
                bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch (Exception ex)
            {
                goto tryagain;
            }

            return stream.ToArray();
        }
    }

}

