using ContactManager.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ContactManager.BusinessLogic
{
    public enum EnLetter { None = 0, Firstletter = 1, Lastletter = 2, Anywhere = 3 };

    public class ClsContactModel
    {
        public int? ContactID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public int? CountryID { get; set; } //*2

        public ClsContactModel() { }
        public ClsContactModel(int? ContactId, string Firstname, string Lastname, string Email, string Phone, string Address, int? CountryId)
        {
            this.ContactID = ContactId;
            this.FirstName = Firstname;
            this.LastName = Lastname;
            this.Email = Email;
            this.Phone = Phone;
            this.Address = Address;
            this.CountryID = CountryId;
        }
    }

    public class ClsContactSearchFilter
    {
        public string? Firstname { get; set; }
        public int? Countryid { get; set; }
        public bool? NullInput { get; set; } = false;
        public EnLetter EnSearchMode { get; set; } = EnLetter.None;
        public int? Contactid { get; set; }
    }

    public class clsFetchResult<T>
    {
        public T Value { get; set; }
        public bool Exists { get; set; }
        public bool IsNull { get; set; }
    }

    public class clsContactBusiness
    {
        private List<ClsContactModel> MapdatatableToList(DataTable dt)
        {
            List<ClsContactModel> list = new List<ClsContactModel>();
            if (dt == null || dt.Rows.Count == 0)
            {
                return list;
            }
            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapDataRowToObject(row));
            }
            return list;
        }

        private ClsContactModel MapDataRowToObject(DataRow row)
        {
            return new ClsContactModel()
            {
                ContactID = row.IsNull("contactid") ? null : (int)row["contactid"],
                FirstName = row.IsNull("firstname") ? null : row["firstname"].ToString(),
                LastName = row.IsNull("lastname") ? null : row["lastname"].ToString(),
                Email = row.IsNull("email") ? null : row["email"].ToString(),
                Phone = row.IsNull("phone") ? null : row["phone"].ToString(),
                Address = row.IsNull("address") ? null : row["address"].ToString(),
                CountryID = row.IsNull("countryid") ? (int?)null : (int)row["countryid"]
            };
        }

        public async Task<List<ClsContactModel>> GetAllContacts(ClsContactSearchFilter Search)
        {
            SqlParameter[] parameters =
            {
                new SqlParameter("@Firstname",(object) Search.Firstname ?? DBNull.Value),
                new SqlParameter("@Countryid",(object) Search.Countryid ?? DBNull.Value),
                new SqlParameter("@Searchonlynull",Search.NullInput),
                new SqlParameter("@Searchbyletter",(int)Search.EnSearchMode),
                new SqlParameter("@Contactid",(object)Search.Contactid??DBNull.Value)
            };
            DataTable dt = await clsContactData.ExecuteStoredProcedure("sp_GetAllContacts", parameters);
            return MapdatatableToList(dt);
        }

        public async Task<clsFetchResult<string>> GetFirstnameById(int? input = null)
        {
            SqlParameter[] Parameter =
            {
               new SqlParameter("@Contactid",input.HasValue?(object)input:DBNull.Value)
            };
            object result = await clsContactData.ExecuteScalar("sp_getfirstname", Parameter);

            if (result == null)
            {
                return new clsFetchResult<string> { Exists = false, IsNull = false, Value = null };
            }
            if (result == DBNull.Value)
            {
                return new clsFetchResult<string> { Exists = true, IsNull = true, Value = null };
            }
            return new clsFetchResult<string> { Exists = true, IsNull = false, Value = result.ToString() };
        }

        public async Task<ClsContactModel> FindContactByID(int? input)
        {
            SqlParameter[] parameter =
            {
                new SqlParameter("@Contactid",input.HasValue?(object)input:DBNull.Value)
            };
            DataTable dt = await clsContactData.ExecuteStoredProcedure("sp_FindContactByID", parameter);

            if (dt != null && dt.Rows.Count > 0)
            {
                return MapDataRowToObject(dt.Rows[0]);
            }
            return null;
        }

        public async Task<List<ClsContactModel>> AddBulkContacts(List<ClsContactModel> ContactList)
        {
            DataTable DtContacts = new DataTable();
            DtContacts.Columns.Add("Firstname", typeof(string));
            DtContacts.Columns.Add("Lastname", typeof(string));
            DtContacts.Columns.Add("Email", typeof(string));
            DtContacts.Columns.Add("Phone", typeof(string));
            DtContacts.Columns.Add("Address", typeof(string));
            DtContacts.Columns.Add("Countryid", typeof(int));

            foreach (ClsContactModel Contact in ContactList)
            {
                DtContacts.Rows.Add(
                    string.IsNullOrEmpty(Contact.FirstName) ? DBNull.Value : (object)Contact.FirstName,
                    string.IsNullOrEmpty(Contact.LastName) ? DBNull.Value : (object)Contact.LastName,
                    string.IsNullOrEmpty(Contact.Email) ? DBNull.Value : (object)Contact.Email,
                    string.IsNullOrEmpty(Contact.Phone) ? DBNull.Value : (object)Contact.Phone,
                    string.IsNullOrEmpty(Contact.Address) ? DBNull.Value : (object)Contact.Address,
                    Contact.CountryID.HasValue ? (object)Contact.CountryID : DBNull.Value
                    );
            }
            SqlParameter[] parameters =
            {
                new SqlParameter("@Contactlist",SqlDbType.Structured)
                {
                    TypeName="dbo.contacttabletype",
                    Value=DtContacts
                }
            };
            DataTable dt = await clsContactData.ExecuteStoredProcedure("sp_insertbulkcontacts", parameters);
            return MapdatatableToList(dt);
        }

        public async Task<bool> CheckCountryID(int CountryID)
        {
            object Result;
            SqlParameter[] parameter = { new SqlParameter("@Countryid", (object)CountryID) };
            Result = await clsContactData.ExecuteScalar("sp_checkcountryexists", parameter);
            return Result != null && Convert.ToInt32(Result) > 0;
        }
    }
}
