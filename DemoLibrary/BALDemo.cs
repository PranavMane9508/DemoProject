using DemoHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DemoLibrary
{
    public class BALDemo
    {
        MSSQL sql = new MSSQL();
        public async Task SaveUser(Demo d)
        {
            Dictionary<string,string> param = new Dictionary<string,string>();
            param.Add("@flag", "SaveUser");
            param.Add("@name", d.Name);
            param.Add("@email", d.Email);
            param.Add("@address", d.Address);
            param.Add("@gender", d.Gender);
            param.Add("@contact", d.Contact.ToString());
            param.Add("@cityid", d.CityId.ToString());
            param.Add("@password", d.Password);
            await sql.ExecuteStoredProcedure("SPUser", param);
        }

        public async Task<List<Demo>> GetUsers()
        {
            List<Demo> users = new List<Demo>();
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@flag", "GetUsers");
            SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("SPUser", param);
            while (await dr.ReadAsync())
            {
                users.Add(new Demo
                {
                    Id = dr.IsDBNull(dr.GetOrdinal("Id")) ? 0 : dr.GetInt32(dr.GetOrdinal("Id")),
                    Name = dr.IsDBNull(dr.GetOrdinal("Name")) ? string.Empty : dr.GetString(dr.GetOrdinal("Name")),
                    Email = dr.IsDBNull(dr.GetOrdinal("Email")) ? string.Empty :  dr.GetString(dr.GetOrdinal("Email")),
                    Contact = dr.IsDBNull(dr.GetOrdinal("Contact")) ? 0L : dr.GetInt64(dr.GetOrdinal("Contact")),
                    Gender = dr.IsDBNull(dr.GetOrdinal("Gender")) ? string.Empty : dr.GetString(dr.GetOrdinal("Gender")),
                    Address = dr.IsDBNull(dr.GetOrdinal("Address")) ? string.Empty : dr.GetString(dr.GetOrdinal("Address")),
                    CountryName = dr.IsDBNull(dr.GetOrdinal("CountryName")) ? string.Empty : dr.GetString(dr.GetOrdinal("CountryName")),
                    StateName = dr.IsDBNull(dr.GetOrdinal("StateName")) ? string.Empty : dr.GetString(dr.GetOrdinal("StateName")),
                    CityName = dr.IsDBNull(dr.GetOrdinal("CityName")) ? string.Empty : dr.GetString(dr.GetOrdinal("CityName")),
                });
            }
            return users;
        }

        public async Task<List<Demo>> GetCountries()
        {
            List<Demo> lst = new List<Demo>();
            Dictionary<string,string> param = new Dictionary<string, string>();
            param.Add("@flag", "GetCountries");
            SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("SPUser", param);
            while (await dr.ReadAsync())
            {
                lst.Add(new Demo
                {
                    CountryId = dr.IsDBNull(dr.GetOrdinal("CountryId")) ? 0 : dr.GetInt32(dr.GetOrdinal("CountryId")),
                    CountryName = dr.IsDBNull(dr.GetOrdinal("CountryName")) ? string.Empty : dr.GetString(dr.GetOrdinal("CountryName"))
                });
            }
            return lst;
        }

        public async Task<List<Demo>> GetStates(int? id = null)
        {
            List<Demo> lst = new List<Demo>();
            Dictionary<string, string> param = new Dictionary<string, string>();
            if (id != null)
            {
                param.Add("@flag", "GetStates");
                param.Add("@id", id.ToString());
            }
            else
            {
                param.Add("@flag", "GetAllStates");
            }
                SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("SPUser", param);
            while (await dr.ReadAsync())
            {
                lst.Add(new Demo
                {
                    StateId = dr.IsDBNull(dr.GetOrdinal("StateId")) ? 0 : dr.GetInt32(dr.GetOrdinal("StateId")),
                    StateName = dr.IsDBNull(dr.GetOrdinal("StateName")) ? string.Empty : dr.GetString(dr.GetOrdinal("StateName"))
                });
            }
            return lst;
        }

        public async Task<List<Demo>> GetCities(int? id = null)
        {
            List<Demo> lst = new List<Demo>();
            Dictionary<string, string> param = new Dictionary<string, string>();
            if (id != null)
            {
                param.Add("@flag", "GetCities");
                param.Add("@id", id.ToString());
            }
            else
            {
                param.Add("@flag", "GetAllCities");
            }
            SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("SPUser", param);
            while (await dr.ReadAsync())
            {
                lst.Add(new Demo
                {
                    CityId = dr.IsDBNull(dr.GetOrdinal("CityId")) ? 0 : dr.GetInt32(dr.GetOrdinal("CityId")),
                    CityName = dr.IsDBNull(dr.GetOrdinal("CityName")) ? string.Empty : dr.GetString(dr.GetOrdinal("CityName"))
                });
            }
            return lst;
        }

        public async Task UpdateUser(Demo d)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@flag", "UpdateUser");
            param.Add("@id", d.Id.ToString());
            param.Add("@name", d.Name);
            param.Add("@email", d.Email);
            param.Add("@address", d.Address);
            param.Add("@gender", d.Gender);
            param.Add("@contact", d.Contact.ToString());
            param.Add("@cityid", d.CityId.ToString());
            param.Add("@password", d.Password);
            await sql.ExecuteStoredProcedure("SPUser", param);
        }

        public async Task DeleteUser(int id)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@flag", "DeleteUser");
            param.Add("@id", id.ToString());
            await sql.ExecuteStoredProcedure("SPUser", param);
        }

        public async Task<Demo> GetUserById(int id)
        {
            Demo demo = new Demo();
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("@flag", "getSpecificUser");
            param.Add("@id", id.ToString());
            SqlDataReader dr = await sql.ExecuteStoredProcedureReturnDataReader("SPUser", param);
            if (await dr.ReadAsync())
            {
                demo.Id = dr.IsDBNull(dr.GetOrdinal("Id")) ? 0 : dr.GetInt32(dr.GetOrdinal("Id"));
                demo.Name = dr.IsDBNull(dr.GetOrdinal("Name")) ? string.Empty : dr.GetString(dr.GetOrdinal("Name"));
                demo.Email = dr.IsDBNull(dr.GetOrdinal("Email")) ? string.Empty : dr.GetString(dr.GetOrdinal("Email"));
                demo.Contact = dr.IsDBNull(dr.GetOrdinal("Contact")) ? 0L : dr.GetInt64(dr.GetOrdinal("Contact"));
                demo.Gender = dr.IsDBNull(dr.GetOrdinal("Gender")) ? string.Empty : dr.GetString(dr.GetOrdinal("Gender"));
                demo.Address = dr.IsDBNull(dr.GetOrdinal("Address")) ? string.Empty : dr.GetString(dr.GetOrdinal("Address"));
                demo.Password = dr.IsDBNull(dr.GetOrdinal("Password")) ? string.Empty : dr.GetString(dr.GetOrdinal("Password"));
                demo.CountryName = dr.IsDBNull(dr.GetOrdinal("CountryName")) ? string.Empty : dr.GetString(dr.GetOrdinal("CountryName"));
                demo.StateName = dr.IsDBNull(dr.GetOrdinal("StateName")) ? string.Empty : dr.GetString(dr.GetOrdinal("StateName"));
                demo.CityName = dr.IsDBNull(dr.GetOrdinal("CityName")) ? string.Empty : dr.GetString(dr.GetOrdinal("CityName"));
                demo.CountryId = dr.IsDBNull(dr.GetOrdinal("CountryId")) ? 0 : dr.GetInt32(dr.GetOrdinal("CountryId"));
                demo.StateId = dr.IsDBNull(dr.GetOrdinal("StateId")) ? 0 : dr.GetInt32(dr.GetOrdinal("StateId"));
                demo.CityId = dr.IsDBNull(dr.GetOrdinal("CityId")) ? 0 : dr.GetInt32(dr.GetOrdinal("CityId"));
            }
            return demo;
        }
    }
}
