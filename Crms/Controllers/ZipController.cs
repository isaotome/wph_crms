using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace Crms.Controllers
{
    [DataContract]
    public class Address {
        [DataMember(Name = "ZipCode")]
        public string ZipCode { get; set; }
        [DataMember(Name = "Prefecture")]
        public string Prefecture { get; set; }
        [DataMember(Name = "City")]
        public string City { get; set; }
        [DataMember(Name = "Town")]
        public string Town { get; set; }
    }
    public class ZipController : Controller
    {
        public void Criteria()
        {
            Address address = GetAddress(Request.QueryString["zip"]);
            if (address != null) {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Address));
                MemoryStream ms = new MemoryStream();
                serializer.WriteObject(ms, address);
                var json = Encoding.UTF8.GetString(ms.ToArray());
                Response.Write(json);
            }

        }
        private Address GetAddress(string zipCode) {
            string commandText = "SELECT * FROM Zip WHERE ZipCode7=@ZIP";
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["CrmsDao.Properties.Settings.CRMSConnectionString"].ConnectionString);
            SqlCommand command = new SqlCommand(commandText, con);
            command.Parameters.Add("@ZIP", SqlDbType.NVarChar);
            command.Parameters["@ZIP"].Value = zipCode.Replace("-", "");
            try {
                con.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows) {
                    reader.Read();
                    Address ret = new Address();
                    ret.ZipCode = reader["ZipCode7"].ToString();
                    ret.Prefecture = reader["Prefecture"].ToString();
                    ret.City = reader["City"].ToString();
                    ret.Town = reader["Town"].ToString();
                    return ret;
                }
            } catch {
            }
            return null;
        }
    }
}
