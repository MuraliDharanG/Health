using HospitalCloud;
using HospitalCloud.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HospitalCould.Controllers
{
    public class PatientController : Controller
    {
 

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Reports()
        {
            var db = database();
            var patientId=((CustomPrincipal)HttpContext.User).UserId;
            var PaId = ReadPatientRegisterationTable(db).FindOneById(ObjectId.Parse(patientId));
            var patientreports = ReadCloudFilesTable(db).FindAll().Where(s => s.PatientId == PaId.PatientId);
            return View(patientreports);
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

        private MongoCollection<CloudFiles> ReadCloudFilesTable(MongoDatabase mongoDatabase)
        {
            MongoCollection<CloudFiles> cloudFiles = mongoDatabase.GetCollection<CloudFiles>("CloudFiles");
            return cloudFiles;
        }

    }
}
