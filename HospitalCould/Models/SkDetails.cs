using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalCloud.Models
{
    public class SkDetails
    {

        public ObjectId Id { get; set; } 
        public string FileId { get; set; }
        public string ReqDocId { get; set; }
        public string ResDocId { get; set; }
       
        public string DateOfRequest { get; set; }
        public string DataOfResponse { get; set; }
        public string ResponseDetails { get; set; } 
        public string DownloadDetails { get; set; }
    }
}