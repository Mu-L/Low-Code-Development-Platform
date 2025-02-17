<%@ WebService Language="C#" Class="RO.Web.AdmSystemsWs" %>
namespace RO.Web
{
    using System;
    using System.Data;
    using System.Web;
    using System.Web.Services;
    using RO.Facade3;
    using RO.Common3;
    using RO.Common3.Data;
    using RO.Rule3;
    using RO.Web;
    using System.Xml;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Web.Script.Services;
    using System.Text.RegularExpressions;
    using System.Collections.Generic;
    using System.Web.SessionState;
    using System.Linq;
    using System.Numerics;

            
    public class AdmSystems87 : DataSet
    {
        public AdmSystems87()
        {

            this.Tables.Add(MakeColumns(new DataTable("AdmSystems")));
            this.Tables.Add(MakeDtlColumns(new DataTable("AdmSystemsDef")));
            this.Tables.Add(MakeDtlColumns(new DataTable("AdmSystemsAdd")));
            this.Tables.Add(MakeDtlColumns(new DataTable("AdmSystemsUpd")));
            this.Tables.Add(MakeDtlColumns(new DataTable("AdmSystemsDel")));
            this.DataSetName = "AdmSystems87";
            this.Namespace = "http://Rintagi.com/DataSet/AdmSystems87";
        }

        private DataTable MakeColumns(DataTable dt)
        {
            DataColumnCollection columns = dt.Columns;
            columns.Add("SystemId45", typeof(string));
            columns.Add("ServerName45", typeof(string));
            columns.Add("SystemName45", typeof(string));
            columns.Add("SystemAbbr45", typeof(string));
            columns.Add("SysProgram45", typeof(string));
            columns.Add("Active45", typeof(string));
            columns.Add("dbAppProvider45", typeof(string));
            columns.Add("dbAppServer45", typeof(string));
            columns.Add("dbAppDatabase45", typeof(string));
            columns.Add("dbDesDatabase45", typeof(string));
            columns.Add("dbAppUserId45", typeof(string));
            columns.Add("dbAppPassword45", typeof(string));
            columns.Add("dbX01Provider45", typeof(string));
            columns.Add("dbX01Server45", typeof(string));
            columns.Add("dbX01Database45", typeof(string));
            columns.Add("dbX01UserId45", typeof(string));
            columns.Add("dbX01Password45", typeof(string));
            columns.Add("dbX01Extra45", typeof(string));
            columns.Add("AdminEmail45", typeof(string));
            columns.Add("AdminPhone45", typeof(string));
            columns.Add("CustServEmail45", typeof(string));
            columns.Add("CustServPhone45", typeof(string));
            columns.Add("CustServFax45", typeof(string));
            columns.Add("WebAddress45", typeof(string));
            columns.Add("FromSystemId", typeof(string));
            columns.Add("UpdBaseSystemId", typeof(string));
            columns.Add("UpdRefSystemId", typeof(string));
            columns.Add("DfltTimezone45", typeof(string));
            return dt;
        }

        private DataTable MakeDtlColumns(DataTable dt)
        {
            DataColumnCollection columns = dt.Columns;
            columns.Add("SystemId45", typeof(string));

            return dt;
        }
    }
            
    [ScriptService()]
    [WebService(Namespace = "http://Rintagi.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public partial class AdmSystemsWs : RO.Web.AsmxBase
    {
        const int screenId = 87;
        const byte systemId = 3;
        const string programName = "AdmSystems87";

        protected override byte GetSystemId() { return systemId; }
        protected override int GetScreenId() { return screenId; }
        protected override string GetProgramName() { return programName; }
        protected override string GetValidateMstIdSPName() { return "GetLisAdmSystems87"; }
        protected override string GetMstTableName(bool underlying = true) { return "Systems"; }
        protected override string GetDtlTableName(bool underlying = true) { return ""; }
        protected override string GetMstKeyColumnName(bool underlying = false) { return underlying ? "SystemId" : "SystemId45"; }
        protected override string GetDtlKeyColumnName(bool underlying = false) { return underlying ? "" : ""; }
            
        Dictionary<string, SerializableDictionary<string, string>> ddlContext = new Dictionary<string, SerializableDictionary<string, string>>() {

        };

        private DataRow MakeTypRow(DataRow dr)
        {
            dr["SystemId45"] = System.Data.OleDb.OleDbType.Numeric.ToString();


            return dr;
        }

        private DataRow MakeDisRow(DataRow dr)
        {
            dr["SystemId45"] = "TextBox";


            return dr;
        }

        private DataRow MakeColRow(DataRow dr, SerializableDictionary<string, string> drv, string keyId, bool bAdd)
        {
            dr["SystemId45"] = keyId;

            DataTable dtAuth = _GetAuthCol(screenId);
            if (dtAuth != null)
            {


            }
            return dr;
        }

        private AdmSystems87 PrepAdmSystemsData(SerializableDictionary<string, string> mst, List<SerializableDictionary<string, string>> dtl, bool bAdd)
        {
            AdmSystems87 ds = new AdmSystems87();
            DataRow dr = ds.Tables["AdmSystems"].NewRow();
            DataRow drType = ds.Tables["AdmSystems"].NewRow();
            DataRow drDisp = ds.Tables["AdmSystems"].NewRow();

            if (bAdd) { dr["SystemId45"] = string.Empty; } else { dr["SystemId45"] = mst["SystemId45"]; }
            drType["SystemId45"] = "Numeric"; drDisp["SystemId45"] = "TextBox";
            try { dr["ServerName45"] = (mst["ServerName45"] ?? "").Trim().Left(50); } catch { }
            drType["ServerName45"] = "VarChar"; drDisp["ServerName45"] = "TextBox";
            try { dr["SystemName45"] = (mst["SystemName45"] ?? "").Trim().Left(50); } catch { }
            drType["SystemName45"] = "VarWChar"; drDisp["SystemName45"] = "TextBox";
            try { dr["SystemAbbr45"] = (mst["SystemAbbr45"] ?? "").Trim().Left(4); } catch { }
            drType["SystemAbbr45"] = "VarChar"; drDisp["SystemAbbr45"] = "TextBox";
            try { dr["SysProgram45"] = (mst["SysProgram45"] ?? "").Trim().Left(1); } catch { }
            drType["SysProgram45"] = "Char"; drDisp["SysProgram45"] = "CheckBox";
            try { dr["Active45"] = (mst["Active45"] ?? "").Trim().Left(1); } catch { }
            drType["Active45"] = "Char"; drDisp["Active45"] = "CheckBox";
            try { dr["dbAppProvider45"] = (mst["dbAppProvider45"] ?? "").Trim().Left(50); } catch { }
            drType["dbAppProvider45"] = "VarChar"; drDisp["dbAppProvider45"] = "TextBox";
            try { dr["dbAppServer45"] = (mst["dbAppServer45"] ?? "").Trim().Left(50); } catch { }
            drType["dbAppServer45"] = "VarChar"; drDisp["dbAppServer45"] = "TextBox";
            try { dr["dbAppDatabase45"] = (mst["dbAppDatabase45"] ?? "").Trim().Left(50); } catch { }
            drType["dbAppDatabase45"] = "VarChar"; drDisp["dbAppDatabase45"] = "TextBox";
            try { dr["dbDesDatabase45"] = (mst["dbDesDatabase45"] ?? "").Trim().Left(50); } catch { }
            drType["dbDesDatabase45"] = "VarChar"; drDisp["dbDesDatabase45"] = "TextBox";
            try { dr["dbAppUserId45"] = (mst["dbAppUserId45"] ?? "").Trim().Left(50); } catch { }
            drType["dbAppUserId45"] = "VarChar"; drDisp["dbAppUserId45"] = "TextBox";
            try { dr["dbAppPassword45"] = (mst["dbAppPassword45"] ?? "").Trim().Left(50); } catch { }
            drType["dbAppPassword45"] = "VarChar"; drDisp["dbAppPassword45"] = "TextBox";
            try { dr["dbX01Provider45"] = (mst["dbX01Provider45"] ?? "").Trim().Left(50); } catch { }
            drType["dbX01Provider45"] = "VarChar"; drDisp["dbX01Provider45"] = "TextBox";
            try { dr["dbX01Server45"] = (mst["dbX01Server45"] ?? "").Trim().Left(50); } catch { }
            drType["dbX01Server45"] = "VarChar"; drDisp["dbX01Server45"] = "TextBox";
            try { dr["dbX01Database45"] = (mst["dbX01Database45"] ?? "").Trim().Left(50); } catch { }
            drType["dbX01Database45"] = "VarChar"; drDisp["dbX01Database45"] = "TextBox";
            try { dr["dbX01UserId45"] = (mst["dbX01UserId45"] ?? "").Trim().Left(50); } catch { }
            drType["dbX01UserId45"] = "VarChar"; drDisp["dbX01UserId45"] = "TextBox";
            try { dr["dbX01Password45"] = (mst["dbX01Password45"] ?? "").Trim().Left(50); } catch { }
            drType["dbX01Password45"] = "VarChar"; drDisp["dbX01Password45"] = "TextBox";
            try { dr["dbX01Extra45"] = (mst["dbX01Extra45"] ?? "").Trim().Left(200); } catch { }
            drType["dbX01Extra45"] = "VarChar"; drDisp["dbX01Extra45"] = "TextBox";
            try { dr["AdminEmail45"] = (mst["AdminEmail45"] ?? "").Trim().Left(50); } catch { }
            drType["AdminEmail45"] = "VarWChar"; drDisp["AdminEmail45"] = "TextBox";
            try { dr["AdminPhone45"] = (mst["AdminPhone45"] ?? "").Trim().Left(20); } catch { }
            drType["AdminPhone45"] = "VarChar"; drDisp["AdminPhone45"] = "TextBox";
            try { dr["CustServEmail45"] = (mst["CustServEmail45"] ?? "").Trim().Left(50); } catch { }
            drType["CustServEmail45"] = "VarWChar"; drDisp["CustServEmail45"] = "TextBox";
            try { dr["CustServPhone45"] = (mst["CustServPhone45"] ?? "").Trim().Left(20); } catch { }
            drType["CustServPhone45"] = "VarChar"; drDisp["CustServPhone45"] = "TextBox";
            try { dr["CustServFax45"] = (mst["CustServFax45"] ?? "").Trim().Left(20); } catch { }
            drType["CustServFax45"] = "VarChar"; drDisp["CustServFax45"] = "TextBox";
            try { dr["WebAddress45"] = (mst["WebAddress45"] ?? "").Trim().Left(50); } catch { }
            drType["WebAddress45"] = "VarChar"; drDisp["WebAddress45"] = "TextBox";
            try { dr["FromSystemId"] = (mst["FromSystemId"] ?? "").Trim().Left(9999999); } catch { }
            drType["FromSystemId"] = string.Empty; drDisp["FromSystemId"] = "TextBox";
            try { dr["UpdBaseSystemId"] = (mst["UpdBaseSystemId"] ?? "").Trim().Left(9999999); } catch { }
            drType["UpdBaseSystemId"] = string.Empty; drDisp["UpdBaseSystemId"] = "CheckBox";
            try { dr["UpdRefSystemId"] = (mst["UpdRefSystemId"] ?? "").Trim().Left(9999999); } catch { }
            drType["UpdRefSystemId"] = string.Empty; drDisp["UpdRefSystemId"] = "CheckBox";
            try { dr["DfltTimezone45"] = (mst["DfltTimezone45"] ?? "").Trim().Left(150); } catch { }
            drType["DfltTimezone45"] = "VarWChar"; drDisp["DfltTimezone45"] = "TextBox";

            if (dtl != null)
            {
                ds.Tables["AdmSystemsDef"].Rows.Add(MakeTypRow(ds.Tables["AdmSystemsDef"].NewRow()));
                ds.Tables["AdmSystemsDef"].Rows.Add(MakeDisRow(ds.Tables["AdmSystemsDef"].NewRow()));
                if (bAdd)
                {
                    foreach (var drv in dtl)
                    {
                        ds.Tables["AdmSystemsAdd"].Rows.Add(MakeColRow(ds.Tables["AdmSystemsAdd"].NewRow(), drv, mst["SystemId45"], true));
                    }
                }
                else
                {
                    var dtlUpd = from r in dtl where !string.IsNullOrEmpty((r[""] ?? "").ToString()) && (r.ContainsKey("_mode") ? r["_mode"] : "") != "delete" select r;
                    foreach (var drv in dtlUpd)
                    {
                        ds.Tables["AdmSystemsUpd"].Rows.Add(MakeColRow(ds.Tables["AdmSystemsUpd"].NewRow(), drv, mst["SystemId45"], false));
                    }
                    var dtlAdd = from r in dtl.AsEnumerable() where string.IsNullOrEmpty(r[""]) select r;
                    foreach (var drv in dtlAdd)
                    {
                        ds.Tables["AdmSystemsAdd"].Rows.Add(MakeColRow(ds.Tables["AdmSystemsAdd"].NewRow(), drv, mst["SystemId45"], true));
                    }
                    var dtlDel = from r in dtl.AsEnumerable() where !string.IsNullOrEmpty((r[""] ?? "").ToString()) && (r.ContainsKey("_mode") ? r["_mode"] : "") == "delete" select r;
                    foreach (var drv in dtlDel)
                    {
                        ds.Tables["AdmSystemsDel"].Rows.Add(MakeColRow(ds.Tables["AdmSystemsDel"].NewRow(), drv, mst["SystemId45"], false));
                    }
                }
            }
            ds.Tables["AdmSystems"].Rows.Add(dr); ds.Tables["AdmSystems"].Rows.Add(drType); ds.Tables["AdmSystems"].Rows.Add(drDisp);

            return ds;
        }

        protected override SerializableDictionary<string, string> InitMaster()
        {
            var mst = new SerializableDictionary<string, string>()
            {
                {"SystemId45",""},
                {"ServerName45",""},
                {"SystemName45",""},
                {"SystemAbbr45",""},
                {"SysProgram45",""},
                {"Active45","Y"},
                {"AddDbs","~/images/custom/adm/SyncToDb.gif"},
                {"dbAppProvider45",""},
                {"dbAppServer45",""},
                {"dbAppDatabase45",""},
                {"dbDesDatabase45",""},
                {"dbAppUserId45",""},
                {"dbAppPassword45",""},
                {"dbX01Provider45",""},
                {"dbX01Server45",""},
                {"dbX01Database45",""},
                {"dbX01UserId45",""},
                {"dbX01Password45",""},
                {"dbX01Extra45",""},
                {"AdminEmail45",""},
                {"AdminPhone45",""},
                {"CustServEmail45",""},
                {"CustServPhone45",""},
                {"CustServFax45",""},
                {"WebAddress45",""},
                {"ResetFromGitRepo",""},
                {"CreateReactBase",""},
                {"RemoveReactBase",""},
                {"PublishReactToSite",""},
                {"FromSystemId",""},
                {"UpdBaseSystemId",""},
                {"UpdRefSystemId",""},
                {"DfltTimezone45",""},

            };
            /* AsmxRule: Init Master Table */
                

            /* AsmxRule End: Init Master Table */

            return mst;
        }

        protected override SerializableDictionary<string, string> InitDtl()
        {
            var mst = new SerializableDictionary<string, string>()
            {


            };
            /* AsmxRule: Init Detail Table */
            

            /* AsmxRule End: Init Detail Table */
            return mst;
        }
            

        [WebMethod(EnableSession = false)]
        public ApiResponse<AutoCompleteResponse, SerializableDictionary<string, AutoCompleteResponse>> GetAdmSystems87List(string searchStr, int topN, string filterId)
        {
            Func<ApiResponse<AutoCompleteResponse, SerializableDictionary<string, AutoCompleteResponse>>> fn = () =>
            {
                SwitchContext(systemId, LCurr.CompanyId, LCurr.ProjectId);
                var effectiveFilterId = GetEffectiveScreenFilterId(filterId, true);                
                System.Web.Script.Serialization.JavaScriptSerializer jss = new System.Web.Script.Serialization.JavaScriptSerializer();
                Dictionary<string, string> context = new Dictionary<string, string>();
                context["method"] = "GetLisAdmSystems87";
                context["mKey"] = "SystemId45";
                context["mVal"] = "SystemId45Text";
                context["mTip"] = "SystemId45Text";
                context["mImg"] = "SystemId45Text";
                context["ssd"] = "";
                context["scr"] = screenId.ToString();
                context["csy"] = systemId.ToString();
                context["filter"] = effectiveFilterId.ToString();
                context["isSys"] = "N";
                context["conn"] = string.Empty;
                AutoCompleteResponse r = LisSuggests(searchStr, jss.Serialize(context), topN, _CurrentScreenCriteria);
                ApiResponse<AutoCompleteResponse, SerializableDictionary<string, AutoCompleteResponse>> mr = new ApiResponse<AutoCompleteResponse, SerializableDictionary<string, AutoCompleteResponse>>();
                mr.errorMsg = "";
                mr.data = r;
                mr.status = "success";
                return mr;
            };
            var ret = ProtectedCall(RestrictedApiCall(fn, systemId, screenId, "R", null));
            return ret;
        }
        [WebMethod(EnableSession = false)]
        public override ApiResponse<AutoCompleteResponse, SerializableDictionary<string, AutoCompleteResponse>> GetSearchList(string searchStr, int topN, string filterId, SerializableDictionary<string, string> desiredScreenCriteria)
        {
            if (desiredScreenCriteria != null)
            {
                _SetEffectiveScrCriteria(desiredScreenCriteria);
            }
            return GetAdmSystems87List(searchStr, topN, filterId);
        }

        [WebMethod(EnableSession = false)]
        public ApiResponse<List<SerializableDictionary<string, string>>, SerializableDictionary<string, AutoCompleteResponse>> GetAdmSystems87ById(string keyId, SerializableDictionary<string, string> options)
        {
            bool refreshUsrImpr = options.ContainsKey("ReAuth") && options["ReAuth"] == "Y" ;


            Func<ApiResponse<List<SerializableDictionary<string, string>>, SerializableDictionary<string, AutoCompleteResponse>>> fn = () =>
            {
                SwitchContext(systemId, LCurr.CompanyId, LCurr.ProjectId, true, true, refreshUsrImpr);
                string mstBlobIconOption = !options.ContainsKey("MstBlob") ? "I" : options["MstBlob"];
                string dtlBlobIconOption = !options.ContainsKey("DtlBlob") ? "I" : options["DtlBlob"];
                var mstBlob = GetBlobOption(mstBlobIconOption);
                var dtlBlob = GetBlobOption(dtlBlobIconOption);
                string jsonCri = options.ContainsKey("CurrentScreenCriteria") ? options["CurrentScreenCriteria"] : null;
                bool includeDtl = !IsGridOnlyScreen() && (!options.ContainsKey("IncludeDtl") || options["IncludeDtl"] == "Y") && !String.IsNullOrEmpty(GetDtlTableName());
                ValidatedMstId("GetLisAdmSystems87", systemId, screenId, "**" + keyId, MatchScreenCriteria(_GetScrCriteria(screenId).DefaultView, jsonCri));
                DataTable dt = _GetMstById(keyId);
                DataTable dtColAuth = _GetAuthCol(GetScreenId());
                DataTable dtColLabel = _GetScreenLabel(GetScreenId());
                Dictionary<string, DataRow> colAuth = _GetAuthColDict(dtColAuth, GetScreenId());
                var utcColumnList = dtColLabel.AsEnumerable().Where(dr => dr["DisplayMode"].ToString().Contains("UTC")).Select(dr => dr["ColumnName"].ToString() + dr["TableId"].ToString()).ToArray();
                HashSet<string> utcColumns = new HashSet<string>(utcColumnList);
                ApiResponse <List<SerializableDictionary<string, string>>, SerializableDictionary<string, AutoCompleteResponse>> mr = new ApiResponse<List<SerializableDictionary<string, string>>, SerializableDictionary<string, AutoCompleteResponse>>();
                SerializableDictionary<string, AutoCompleteResponse> supportingData = new SerializableDictionary<string,AutoCompleteResponse>();
                /* Get Master By Id After start here */


                /* Get Master By Id After end here */
                mr.data = DataTableToListOfObject(dt, mstBlob, colAuth, utcColumns);
                mr.supportingData = includeDtl ? new SerializableDictionary<string, AutoCompleteResponse>() { { "dtl", new AutoCompleteResponse() { data = DataTableToListOfObject(_GetDtlById(keyId, 0), dtlBlob, colAuth, utcColumns) } } } : supportingData;
                mr.status = "success";
                mr.errorMsg = "";
                return mr;
            };
            var ret = ProtectedCall(RestrictedApiCall(fn, systemId, screenId, "R", null));
            return ret;
        }
        [WebMethod(EnableSession = false)]
        public override ApiResponse<List<SerializableDictionary<string, string>>, SerializableDictionary<string, AutoCompleteResponse>> GetMstById(string keyId, SerializableDictionary<string, string> options)
        {
            return GetAdmSystems87ById(keyId, options);
        }
        [WebMethod(EnableSession = false)]
        public ApiResponse<List<SerializableDictionary<string, string>>, SerializableDictionary<string, AutoCompleteResponse>> GetAdmSystems87DtlById(string keyId, SerializableDictionary<string, string> options, int filterId)
        {
            bool refreshUsrImpr = options.ContainsKey("ReAuth") && options["ReAuth"] == "Y" ;
            string filterName = options.ContainsKey("FilterName") ? options["FilterName"] : "";

            Func<ApiResponse<List<SerializableDictionary<string, string>>, SerializableDictionary<string, AutoCompleteResponse>>> fn = () =>
            {
                SwitchContext(systemId, LCurr.CompanyId, LCurr.ProjectId, true, true, refreshUsrImpr);
                string jsonCri = options.ContainsKey("CurrentScreenCriteria") ? options["CurrentScreenCriteria"] : null;            
                
                ApiResponse<List<SerializableDictionary<string, string>>, SerializableDictionary<string, AutoCompleteResponse>> mr = new ApiResponse<List<SerializableDictionary<string, string>>, SerializableDictionary<string, AutoCompleteResponse>>();
                mr.data = new List<SerializableDictionary<string,string>>();
                mr.status = "success";
                mr.errorMsg = "";
                return mr;
            };
            var ret = ProtectedCall(RestrictedApiCall(fn, systemId, screenId, "R", null));
            return ret;
        }
        [WebMethod(EnableSession = false)]
        public override ApiResponse<List<SerializableDictionary<string, string>>, SerializableDictionary<string, AutoCompleteResponse>> GetDtlById(string keyId, SerializableDictionary<string, string> options, int filterId)
        {
            return GetAdmSystems87DtlById(keyId, options, filterId);
        }
        [WebMethod(EnableSession = false)]
        public override ApiResponse<List<SerializableDictionary<string, string>>, SerializableDictionary<string, AutoCompleteResponse>> GetNewMst()
        {
            Func<ApiResponse<List<SerializableDictionary<string, string>>, SerializableDictionary<string, AutoCompleteResponse>>> fn = () =>
            {
                SwitchContext(systemId, LCurr.CompanyId, LCurr.ProjectId);
                var mst = InitMaster();
                ApiResponse<List<SerializableDictionary<string, string>>, SerializableDictionary<string, AutoCompleteResponse>> mr = new ApiResponse<List<SerializableDictionary<string, string>>, SerializableDictionary<string, AutoCompleteResponse>>();
                mr.data = new List<SerializableDictionary<string, string>>() { mst };
                mr.status = "success";
                mr.errorMsg = "";
                return mr;
            };
            var ret = ProtectedCall(RestrictedApiCall(fn, systemId, screenId, "R", null));
            return ret;
        }
        [WebMethod(EnableSession = false)]
        public ApiResponse<List<SerializableDictionary<string, string>>, SerializableDictionary<string, AutoCompleteResponse>> GetNewDtl()
        {
            Func<ApiResponse<List<SerializableDictionary<string, string>>, SerializableDictionary<string, AutoCompleteResponse>>> fn = () =>
            {
                SwitchContext(systemId, LCurr.CompanyId, LCurr.ProjectId);
                var dtl = InitDtl();
                ApiResponse<List<SerializableDictionary<string, string>>, SerializableDictionary<string, AutoCompleteResponse>> mr = new ApiResponse<List<SerializableDictionary<string, string>>, SerializableDictionary<string, AutoCompleteResponse>>();
                mr.data = new List<SerializableDictionary<string, string>>() { dtl };
                mr.status = "success";
                mr.errorMsg = "";
                return mr;
            };
            var ret = ProtectedCall(RestrictedApiCall(fn, systemId, screenId, "R", null));
            return ret;
        }

        protected override DataTable _GetMstById(string mstId)
        {
            return (new RO.Facade3.AdminSystem()).GetMstById("GetAdmSystems87ById", string.IsNullOrEmpty(mstId) ? "-1" : mstId, LcAppConnString, LcAppPw);
        }

        protected override DataTable _GetDtlById(string mstId, int screenFilterId)
        {
            return (new RO.Facade3.AdminSystem()).GetDtlById(screenId, "GetAdmSystems87DtlById", string.IsNullOrEmpty(mstId) ? "-1" : mstId, LcAppConnString, LcAppPw, GetEffectiveScreenFilterId(screenFilterId.ToString(), false), LImpr, LCurr);
        }
        protected override Dictionary<string, SerializableDictionary<string, string>> GetDdlContext()
        {
            return ddlContext;
        }

        [WebMethod(EnableSession = false)]
        public override ApiResponse<SaveDataResponse, SerializableDictionary<string, AutoCompleteResponse>> DelMst(SerializableDictionary<string, string> mst, SerializableDictionary<string, string> options)
        {
            bool refreshUsrImpr = options.ContainsKey("ReAuth") && options["ReAuth"] == "Y" ;
            bool noTrans = Config.NoTrans;
            int commandTimeOut = Config.CommandTimeOut;

            Func<ApiResponse<SaveDataResponse, SerializableDictionary<string, AutoCompleteResponse>>> fn = () =>
            {
                SwitchContext(systemId, LCurr.CompanyId, LCurr.ProjectId, true, true, refreshUsrImpr);
                var pid = mst["SystemId45"];
                var dtl = new List<SerializableDictionary<string, string>>();
                if (IsGridOnlyScreen())
                {
                    mst["_mode"] = "delete";
                    dtl.Add(mst.Clone());                    
                }
                var ds = PrepAdmSystemsData(mst, dtl, IsGridOnlyScreen() ? false : string.IsNullOrEmpty(mst["SystemId45"]));

                if (IsGridOnlyScreen())
                    (new RO.Facade3.AdminSystem()).UpdData(screenId, false, base.LUser, base.LImpr, base.LCurr, ds, LcAppConnString, LcAppPw, base.CPrj, base.CSrc, noTrans, commandTimeOut);                    
                else    
                    (new RO.Facade3.AdminSystem()).DelData(screenId, false, base.LUser, base.LImpr, base.LCurr, ds, LcAppConnString, LcAppPw, base.CPrj, base.CSrc, noTrans, commandTimeOut);

                ApiResponse<SaveDataResponse, SerializableDictionary<string, AutoCompleteResponse>> mr = new ApiResponse<SaveDataResponse, SerializableDictionary<string, AutoCompleteResponse>>();
                SaveDataResponse result = new SaveDataResponse();
                string msg = _GetScreenHlp(screenId).Rows[0]["DelMsg"].ToString();
                result.message = msg;
                mr.status = "success";
                mr.errorMsg = "";
                mr.data = result;
                return mr;
            };
            var ret = ProtectedCall(RestrictedApiCall(fn, systemId, screenId, "D", null));
            return ret;

        }

        [WebMethod(EnableSession = false)]
        public override ApiResponse<SaveDataResponse, SerializableDictionary<string, AutoCompleteResponse>> SaveData(SerializableDictionary<string, string> mst, List<SerializableDictionary<string, string>> dtl, SerializableDictionary<string, string> options)
        {
            bool isAdd = false;
            bool refreshUsrImpr = options.ContainsKey("ReAuth") && options["ReAuth"] == "Y" ;
            string screenButton = options.ContainsKey("ScreenButton") ? options["ScreenButton"] : "";
            string actionButton = options.ContainsKey("OnClickColumeName") ? options["OnClickColumeName"] : "";
            bool noTrans = Config.NoTrans;
            int commandTimeOut = Config.CommandTimeOut;

            Func<ApiResponse<SaveDataResponse, SerializableDictionary<string, AutoCompleteResponse>>> fn = () =>
            {
                SwitchContext(systemId, LCurr.CompanyId, LCurr.ProjectId, true, true, refreshUsrImpr);
                System.Collections.Generic.Dictionary<string, string> context = new System.Collections.Generic.Dictionary<string, string>();
                SerializableDictionary<string, string> skipValidation = new SerializableDictionary<string, string>() { { "SkipAllMst", "SilentColReadOnly" }, { "SkipAllDtl", "SilentColReadOnly" } };
                if (IsGridOnlyScreen() && dtl.Count == 0)
                {
                    dtl.Add(mst.Clone());
                }
                /* AsmxRule: Save Data Before Validation */


                /* AsmxRule End: Save Data Before Validation */

                var pid = mst["SystemId45"];
                isAdd = GetLis("GetLisAdmSystems87", systemId, screenId, "**" + pid, new List<string>(), "0", "", "N", 1, false).Rows.Count == 0;
                if (!isAdd)
                {
                    string jsonCri = options.ContainsKey("CurrentScreenCriteria") ? options["CurrentScreenCriteria"] : null;
                    ValidatedMstId("GetLisAdmSystems87", systemId, screenId, "**" + pid, MatchScreenCriteria(_GetScrCriteria(screenId).DefaultView, jsonCri));
                }
                else
                {
                    ValidateAction(screenId, "A");
                }

                /* current data */
                DataTable dtMst = _GetMstById(pid);
                DataTable dtDtl = null; 
                int maxDtlId = dtDtl == null ? -1 : dtDtl.AsEnumerable().Select(dr => dr[""].ToString()).Where((s) => !string.IsNullOrEmpty(s)).Select(id => int.Parse(id)).DefaultIfEmpty(-1).Max();
                var validationResult = ValidateInput(ref mst, ref dtl, dtMst, dtDtl, "SystemId45", "", skipValidation);
                if (validationResult.Item1.Count > 0 || validationResult.Item2.Count > 0)
                {
                    return new ApiResponse<SaveDataResponse, SerializableDictionary<string, AutoCompleteResponse>>()
                    {
                        status = "failed",
                        errorMsg = "content invalid " + string.Join(" ", (validationResult.Item1.Count > 0 ? validationResult.Item1 : validationResult.Item2[0]).ToArray()),
                        validationErrors = validationResult.Item1.Count > 0 ? validationResult.Item1 : validationResult.Item2[0],
                    };
                }
                /* AsmxRule: Save Data Before */


                /* AsmxRule End: Save Data Before */

                var ds = PrepAdmSystemsData(mst, dtl, string.IsNullOrEmpty(mst["SystemId45"]));
                string msg = string.Empty;

                if (isAdd && !IsGridOnlyScreen())
                {
                    pid = (new RO.Facade3.AdminSystem()).AddData(screenId, false, base.LUser, base.LImpr, base.LCurr, ds, LcAppConnString, LcAppPw, base.CPrj, base.CSrc, noTrans, commandTimeOut);

                    if (!string.IsNullOrEmpty(pid))
                    {
                        msg = _GetScreenHlp(screenId).Rows[0]["AddMsg"].ToString();
                    }
                }
                else
                {
                    bool ok = (new RO.Facade3.AdminSystem()).UpdData(screenId, false, base.LUser, base.LImpr, base.LCurr, ds, LcAppConnString, LcAppPw, base.CPrj, base.CSrc, noTrans, commandTimeOut);

                    if (ok)
                    {
                        msg = _GetScreenHlp(screenId).Rows[0]["UpdMsg"].ToString();
                    }
                }

                /* read updated records */
                dtMst = _GetMstById(pid);

                /* AsmxRule: Save Data After */


                /* AsmxRule End: Save Data After */

                ApiResponse<SaveDataResponse, SerializableDictionary<string, AutoCompleteResponse>> mr = new ApiResponse<SaveDataResponse, SerializableDictionary<string, AutoCompleteResponse>>();
                SaveDataResponse result = new SaveDataResponse();
                DataTable dtColAuth = _GetAuthCol(GetScreenId());
                DataTable dtColLabel = _GetScreenLabel(GetScreenId());
                Dictionary<string, DataRow> colAuth = _GetAuthColDict(dtColAuth, GetScreenId());
                var utcColumnList = dtColLabel.AsEnumerable().Where(dr => dr["DisplayMode"].ToString().Contains("UTC")).Select(dr => dr["ColumnName"].ToString() + dr["TableId"].ToString()).ToArray();
                HashSet<string> utcColumns = new HashSet<string>(utcColumnList);

                result.mst = DataTableToListOfObject(dtMst, IncludeBLOB.None, colAuth, utcColumns)[0];

                    
                result.message = msg;
                mr.status = "success";
                mr.errorMsg = "";
                mr.data = result;
                return mr;
            };
            var ret = ProtectedCall(RestrictedApiCall(fn, systemId, screenId, "S", null));
            return ret;
        }
            


        /* AsmxRule: Custom Function */


        /* AsmxRule End: Custom Function */
           
    }
}
            