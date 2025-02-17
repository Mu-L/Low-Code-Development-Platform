using System;
using System.IO;
using System.Text;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Globalization;
using System.Threading;
using System.Linq;
using CrystalDecisions.Web;
using CrystalDecisions.Shared;
using CrystalDecisions.CrystalReports.Engine;
using RO.Facade3;
using RO.Common3;
using RO.Common3.Data;

namespace RO.Common3.Data
{
	public class DsAdmRptBizRuleIn : DataSet
	{
		public DsAdmRptBizRuleIn()
		{
			this.Tables.Add(MakeColumns(new DataTable("DtAdmRptBizRuleIn")));
			this.DataSetName = "DsAdmRptBizRuleIn";
			this.Namespace = "http://Rintagi.com/DataSet/DsAdmRptBizRuleIn";
		}

		private DataTable MakeColumns(DataTable dt)
		{
			DataColumnCollection columns = dt.Columns;
			columns.Add("tzInfo", typeof(string));
			columns.Add("tzUtcOffset", typeof(string));
			columns.Add("SystemId", typeof(Byte));
			return dt;
		}
	}
}

namespace RO.Web
{
	public partial class AdmRptBizRuleModule : RO.Web.ModuleBase
	{

		public ReportDocument rp = new ReportDocument();
		private const string KEY_dtReportSct = "Cache:dtReportSct55";
		private const string KEY_dtReportItem = "Cache:dtReportItem55";
		private const string KEY_dtReportHlp = "Cache:dtReportHlp55";
		private const string KEY_dtCri = "Cache:dtCri55";
		private const string KEY_dtClientRule = "Cache:dtClientRule55";
		private const string KEY_dtPrinters = "Cache:dtPrinters55";
		private const string KEY_bClCriVisible = "Cache:bClCriVisible55";
		private const string KEY_bShCriVisible = "Cache:bShCriVisible55";
		private enum exportTo {TXT, RTF, XML, XLS, PDF, DOC, VIEW};
		private string LcSysConnString;
		private string LcAppConnString;
		private string LcAppPw;
		private int CommandTimeOut = Config.CommandTimeOut;

		public AdmRptBizRuleModule()
		{
			this.Init += new System.EventHandler(Page_Init);
		}

		protected void Page_Unload(object sender, System.EventArgs e)
		{
			rp.Dispose();
		}

		protected void Page_Load(object sender, System.EventArgs e)
		{
			CheckAuthentication(true);
			Thread.CurrentThread.CurrentCulture = new CultureInfo(base.LUser.Culture);
			string lang2 = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
			string lang = "en,zh".IndexOf(lang2) < 0 ? lang2 : Thread.CurrentThread.CurrentCulture.Name;
			ScriptManager.RegisterClientScriptInclude(Page, Page.GetType(), "datepicker_i18n", "scripts/i18n/jquery.ui.datepicker-" + lang + ".js");
			rp.Load(Server.MapPath(@"reports/AdmRptBizRuleReport.rpt"));
			if (!IsPostBack)
			{
				DataTable dtMenuAccess = (new MenuSystem()).GetMenu(base.LUser.CultureId,3, base.LImpr, LcSysConnString, LcAppPw, null, 55, null);
				if (dtMenuAccess.Rows.Count == 0)
				{
				    string message = (new AdminSystem()).GetLabel(base.LUser.CultureId, "cSystem", "AccessDeny", null, null, null);
				    throw new Exception(message);
				}
				/* Stop IIS from Caching but allowing export to Excel to work */
				HttpContext.Current.Response.Cache.SetAllowResponseInBrowserHistory(false);
				HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
				HttpContext.Current.Response.Cache.SetNoStore();
				Response.Cache.SetExpires(DateTime.Now.AddSeconds(-60));
				Response.Cache.SetValidUntilExpires(true);
				Session.Remove(KEY_dtReportSct);
				Session.Remove(KEY_dtReportItem);
				Session.Remove(KEY_dtReportHlp);
				Session.Remove(KEY_dtCri);
				Session.Remove(KEY_dtClientRule);
				SetButtonHlp();
				bool bBatchPrint = false;
				if (base.LUser.InternalUsr == "N")
				{
					cPrintButton.Visible = false; cPrinter.Visible = false;
				}
				else
				{
					Session.Remove(KEY_dtPrinters);
					DataTable dtPrinters;
					dtPrinters = (new AdminSystem()).GetPrinterList(base.LImpr.UsrGroups, base.LImpr.Members);
					if (dtPrinters != null && dtPrinters.Rows.Count > 0)
					{
						cPrinter.DataSource = dtPrinters;
						cPrinter.DataBind();
						cPrinter.SelectedIndex = 0;
						Session[KEY_dtPrinters] = dtPrinters;
					}
					int ii = 0;	//Update criteria for Batch reporting.
					while (Request.QueryString["Cri"+ii.ToString()] != null && Request.QueryString["Val"+ii.ToString()] != null)
					{
						(new AdminSystem()).UpdLastCriteria(0,55,base.LUser.UsrId,Int32.Parse(Request.QueryString["Cri"+ii.ToString()]),Request.QueryString["Val"+ii.ToString()],LcSysConnString,LcAppPw);
						bBatchPrint = true; ii = ii +1;
					}
				}
				DataTable dt;
				DataView dv;
				string selectedVal = null;
				dt = (new AdminSystem()).GetLastCriteria(1,0,55,base.LUser.UsrId,LcSysConnString,LcAppPw);
				if ((bool)Session[KEY_bClCriVisible]) {cClearCriButton.Visible = cCriteria.Visible;} else {cClearCriButton.Visible = false;}
				if ((bool)Session[KEY_bShCriVisible]) {cShowCriButton.Visible = !cCriteria.Visible;} else {cShowCriButton.Visible = false;}
				DataTable dtCri = GetReportCriHlp();
				base.SetCriBehavior(cSystemId, cSystemIdP1, cSystemIdLabel, dtCri.Rows[0]);
				cSystemId.AutoPostBack = false;
				try {selectedVal = dt.Rows[0]["LastCriteria"].ToString();} catch { selectedVal = null;};
				dv = new DataView((new AdminSystem()).GetIn(55,"GetIn55SystemId",(new SqlReportSystem()).CountRptCri("18",LcSysConnString,LcAppPw),"Y",base.LImpr,base.LCurr,LcAppConnString,LcAppPw));
				cSystemId.DataSource = dv;
				cSystemId.DataBind();
				try
				{
					cSystemId.Items.FindByValue(dt.Rows[0]["LastCriteria"].ToString()).Selected = true;
				}
				catch
				{
					try {cSystemId.SelectedIndex = 0;} catch {}
				}
				DataTable dtHlp = GetReportHlp();
				try
				{
				    int.TryParse(dtHlp.Rows[0]["CommandTimeOut"].ToString(), out CommandTimeOut);
				}
				catch { }
				cHelpMsg.HelpTitle = dtHlp.Rows[0]["ReportTitle"].ToString(); cHelpMsg.HelpMsg = dtHlp.Rows[0]["DefaultHlpMsg"].ToString();
				cTitleLabel.Text = dtHlp.Rows[0]["ReportTitle"].ToString();
				SetClientRule();
				(new AdminSystem()).LogUsage(base.LUser.UsrId, string.Empty, dtHlp.Rows[0]["ReportTitle"].ToString(), 0, 55, 0, string.Empty, LcSysConnString, LcAppPw);
				if (bBatchPrint)
				{
					getReport(true, exportTo.VIEW);
					Response.Write("<script lang='javascript'>opener=self;window.close();</script>");
				}
			}
		}

		protected void Page_Init(object sender, EventArgs e)
		{
			InitializeComponent();
		}

		#region Web Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			if (LcSysConnString == null) { SetSystem(3); }
			this.cViewer.Search += new CrystalDecisions.Web.SearchEventHandler(this.cViewer_Search);
			this.cViewer.ViewZoom += new CrystalDecisions.Web.ZoomEventHandler(this.cViewer_ViewZoom);
			this.cViewer.Navigate += new CrystalDecisions.Web.NavigateEventHandler(this.cViewer_Navigate);
			this.cViewer.Drill += new CrystalDecisions.Web.DrillEventHandler(this.cViewer_Drill);

		}
		#endregion

		protected void cSystemId_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (!IsPostBack) return;
			ScriptManager.GetCurrent(Parent.Page).SetFocus(((DropDownList)sender).ClientID);
		}

		private void SetSystem(byte SystemId)
		{
			LcSysConnString = base.SysConnectStr(SystemId);
			LcAppConnString = base.AppConnectStr(SystemId);
			LcAppPw = base.AppPwd(SystemId);
			try
			{
				base.CPrj = new CurrPrj(((new RobotSystem()).GetEntityList()).Rows[0]);
				DataRow row = base.SystemsList.Rows.Find(SystemId);
				base.CSrc = new CurrSrc(true, row);
				base.CTar = new CurrTar(true, row);
				if ((Config.DeployType == "DEV" || row["dbAppDatabase"].ToString() == base.CPrj.EntityCode + "View") && !(base.CPrj.EntityCode != "RO" && row["SysProgram"].ToString() == "Y") && (new AdminSystem()).IsRegenNeeded(string.Empty,0,55,0,LcSysConnString,LcAppPw))
				{
					(new GenReportsSystem()).CreateProgram(string.Empty,55, "Server Rules Listing", row["dbAppDatabase"].ToString(), base.CPrj, base.CSrc, base.CTar, LcAppConnString, LcAppPw);
					this.Redirect(Request.RawUrl);
				}
			}
			catch (Exception e) { PreMsgPopup(e.Message); }
		}

		private void SetButtonHlp()
		{
			DataTable dt;
			dt = (new AdminSystem()).GetButtonHlp(0,55,0,base.LUser.CultureId,LcSysConnString,LcAppPw);
			if (dt != null && dt.Rows.Count > 0)
			{
				foreach (DataRow dr in dt.Rows)
				{
					if (dr["ButtonTypeName"].ToString() == "ClearCri") { cClearCriButton.CssClass = "ButtonImg ClearCriButtonImg"; Session[KEY_bClCriVisible] = base.GetBool(dr["ButtonVisible"].ToString()); cClearCriButton.ToolTip = dr["ButtonToolTip"].ToString(); }
					if (dr["ButtonTypeName"].ToString() == "ShowCri") { cShowCriButton.CssClass = "ButtonImg ShowCriButtonImg"; cShowCriButton.Text = dr["ButtonName"].ToString(); Session[KEY_bShCriVisible] = base.GetBool(dr["ButtonVisible"].ToString()); cShowCriButton.ToolTip = dr["ButtonToolTip"].ToString(); }
					if (dr["ButtonTypeName"].ToString() == "ExpTxt") { cExpTxtButton.CssClass = "ButtonImg ExpTxtButtonImg"; cExpTxtButton.Text = dr["ButtonName"].ToString(); cExpTxtButton.Visible = base.GetBool(dr["ButtonVisible"].ToString()); cExpTxtButton.ToolTip = dr["ButtonToolTip"].ToString(); }
					if (dr["ButtonTypeName"].ToString() == "View") { cViewButton.CssClass = "ButtonImg ViewButtonImg"; cViewButton.Visible = base.GetBool(dr["ButtonVisible"].ToString()); cViewButton.Text = dr["ButtonName"].ToString(); cViewButton.ToolTip = dr["ButtonToolTip"].ToString(); }
					if (dr["ButtonTypeName"].ToString() == "ExpPdf") { cExpPdfButton.CssClass = "ButtonImg ExpPdfButtonImg"; cExpPdfButton.Text = dr["ButtonName"].ToString(); cExpPdfButton.Visible = base.GetBool(dr["ButtonVisible"].ToString()); cExpPdfButton.ToolTip = dr["ButtonToolTip"].ToString(); }
					if (dr["ButtonTypeName"].ToString() == "ExpDoc") { cExpDocButton.CssClass = "ButtonImg ExpDocButtonImg"; cExpDocButton.Text = dr["ButtonName"].ToString(); cExpDocButton.Visible = base.GetBool(dr["ButtonVisible"].ToString()); cExpDocButton.ToolTip = dr["ButtonToolTip"].ToString(); }
					if (dr["ButtonTypeName"].ToString() == "ExpXls") { cExpXlsButton.CssClass = "ButtonImg ExpXlsButtonImg"; cExpXlsButton.Text = dr["ButtonName"].ToString(); cExpXlsButton.Visible = base.GetBool(dr["ButtonVisible"].ToString()); cExpXlsButton.ToolTip = dr["ButtonToolTip"].ToString(); }
					if (dr["ButtonTypeName"].ToString() == "Print") { cPrintButton.CssClass = "ButtonImg PrintButtonImg"; cPrintButton.Text = dr["ButtonName"].ToString(); cPrintButton.Visible = base.GetBool(dr["ButtonVisible"].ToString()); cPrintButton.ToolTip = dr["ButtonToolTip"].ToString(); }
				}
			}
		}

		private DataTable GetClientRule()
		{
			DataTable dtRul = (DataTable)Session[KEY_dtClientRule];
			if (dtRul == null)
			{
				CheckAuthentication(false);
				dtRul = (new AdminSystem()).GetClientRule(0,55,base.LUser.CultureId,LcSysConnString,LcAppPw);
				Session[KEY_dtClientRule] = dtRul;
			}
			return dtRul;
		}

		private void SetClientRule()
		{
			DataView dvRul = new DataView(GetClientRule());
			if (dvRul.Count > 0)
			{
				WebControl cc = null;
				string srp = string.Empty;
				string sn = string.Empty;
				string st = string.Empty;
				int ii = 0;
				foreach (DataRowView drv in dvRul)
				{
					srp = drv["ScriptName"].ToString();
					if (drv["ParamName"].ToString() != string.Empty)
					{
						StringBuilder sbName =  new StringBuilder();
						StringBuilder sbType =  new StringBuilder();
						sbName.Append(drv["ParamName"].ToString().Trim());
						sbType.Append(drv["ParamType"].ToString().Trim());
						ii = 0;
						while (sbName.Length > 0)
						{
							sn = Utils.PopFirstWord(sbName,(char)44); st = Utils.PopFirstWord(sbType,(char)44);
							if (st.ToLower() == "combobox") {srp = srp.Replace("@" + ii.ToString() + "@",((RoboCoder.WebControls.ComboBox)this.FindControl(sn)).FocusID);} else {srp = srp.Replace("@" + ii.ToString() + "@",((WebControl)this.FindControl(sn)).ClientID);}
							ii = ii + 1;
						}
					}
					cc = this.FindControl(drv["ColName"].ToString()) as WebControl;
					if (cc != null && (cc.Attributes[drv["ScriptEvent"].ToString()] == null || cc.Attributes[drv["ScriptEvent"].ToString()].IndexOf(srp) < 0)) {cc.Attributes[drv["ScriptEvent"].ToString()] += srp;}
				}
			}
		}

		private DataTable GetReportCriHlp()
		{
			DataTable dtCri = (DataTable)Session[KEY_dtCri];
			if (dtCri == null)
			{
				CheckAuthentication(false);
				dtCri = (new AdminSystem()).GetReportCriHlp(55,base.LUser.CultureId,LcSysConnString,LcAppPw);
				Session[KEY_dtCri] = dtCri;
			}
			return dtCri;
		}

		private DataTable GetReportHlp()
		{
			DataTable dtHlp = (DataTable)Session[KEY_dtReportHlp];
			if (dtHlp == null)
			{
				CheckAuthentication(false);
			    dtHlp = (new AdminSystem()).GetReportHlp(55,base.LUser.CultureId,LcSysConnString,LcAppPw);
				Session[KEY_dtReportHlp] = dtHlp;
			}
			return dtHlp;
		}

		private void CheckAuthentication(bool pageLoad)
		{
			CheckAuthentication(pageLoad, true);
		}

		private DataView GetRptCriteria()
		{
			return ((new SqlReportSystem()).GetReportCriteria(string.Empty,"55",LcSysConnString,LcAppPw)).DefaultView;
		}

		private DsAdmRptBizRuleIn UpdCriteria(bool bUpdate)
		{
			DsAdmRptBizRuleIn ds = new DsAdmRptBizRuleIn();
			DataRow dr = ds.Tables["DtAdmRptBizRuleIn"].NewRow();
			TimeZoneInfo tzinfo = Session["Cache:tzInfo"] as TimeZoneInfo ?? TimeZoneInfo.Local;
			dr["tzInfo"] = tzinfo.StandardName;
			dr["tzUtcOffset"] = string.Format("{0:+00;-00;+00}:{1:00}", tzinfo.BaseUtcOffset.Hours, tzinfo.BaseUtcOffset.Minutes);

			if (cSystemId.SelectedIndex >= 0 && cSystemId.SelectedValue != string.Empty) {dr["SystemId"] = cSystemId.SelectedValue;}
			if (IsPostBack && cSystemId.SelectedValue == string.Empty) { throw new ApplicationException("Criteria column: SystemId should not be empty. Please rectify and try again.");};
			ds.Tables["DtAdmRptBizRuleIn"].Rows.Add(dr);
			if (bUpdate) {(new AdminSystem()).UpdRptDt(55,"UpdAdmRptBizRule",base.LUser.UsrId,ds,GetRptCriteria(),LcAppConnString,LcAppPw);}
			return ds;
		}

		private DataView GetReportSct()
		{
			DataTable dtSct = (DataTable)Session[KEY_dtReportSct];
			if (dtSct == null)
			{
				CheckAuthentication(false);
				dtSct = (new AdminSystem()).GetReportSct();
				Session[KEY_dtReportSct] = dtSct;
			}
			return dtSct.DefaultView;
		}

		private DataView GetReportItem()
		{
			DataTable dtItem = (DataTable)Session[KEY_dtReportItem];
			if (dtItem == null)
			{
				CheckAuthentication(false);
				dtItem = (new AdminSystem()).GetReportItem(55,LcSysConnString,LcAppPw);
				DataView dvSct = GetReportSct();
				foreach (DataRowView drv in dvSct) {dtItem = MapReportSct(dtItem, drv["ReportSctName"].ToString());}
				Session[KEY_dtReportItem] = dtItem;
			}
			return dtItem.DefaultView;
		}

		// In case formula referencecs not in order.
		private DataTable MapReportSct(DataTable dt, string SctName)
		{
			int ii = 0;
			DataView dv = dt.DefaultView;
			dv.RowFilter = "ReportSctName = '" + SctName + "'";
			foreach (DataRowView drv in dv)
			{
				ii = ii + 1;
				drv["InternalField"] = SctName + ii.ToString();
			}
			return dt;
		}

		private void SetReportSct(DataView dv, string SctName)
		{
			DataView dvCopy = new DataView(dv.Table.Copy());
			CrystalDecisions.CrystalReports.Engine.FormulaFieldDefinition ffd;
			CrystalDecisions.CrystalReports.Engine.FieldObject fo;
			dv.RowFilter = "ReportSctName = '" + SctName + "'";
			int iPos1;
			int iPos2;
			string sKey;
			foreach (DataRowView drv in dv)
			{
				ffd = rp.DataDefinition.FormulaFields[drv["InternalField"].ToString()];
				fo = (FieldObject)rp.ReportDefinition.ReportObjects[drv["InternalField"].ToString()];
				if (drv["FormulaReady"].ToString() == "N")
				{
					drv["FormulaReady"] = "Y";
					iPos1 = drv["ItemFormula"].ToString().IndexOf("{@");
					while (iPos1 >= 0)
					{
						iPos2 = drv["ItemFormula"].ToString().IndexOf("}", iPos1 + 1);
						if (iPos2 > iPos1)
						{
							sKey = drv["ItemFormula"].ToString().Substring(iPos1 + 1, iPos2 - iPos1 - 1);
							dvCopy.RowFilter = "ReportItemName = '" + sKey + "'";
							if (dvCopy.Count != 1) { throw new Exception("Referenced Item Issue: Non-unique report item name or referenced item '" + sKey + "' in item formula not found!"); }
							drv["ItemFormula"] = drv["ItemFormula"].ToString().Replace("{" + sKey + "}", "{@" + dvCopy[0]["InternalField"].ToString() + "}");
							iPos1 = drv["ItemFormula"].ToString().IndexOf("{@", iPos2 + 1);
						}
						else {iPos1 = -1;}
					}
				}
				ffd.Text = drv["ItemFormula"].ToString();
				if (drv["FontBold"].ToString() == "Y" && drv["FontItalic"].ToString() == "Y") {fo.ApplyFont(new System.Drawing.Font(drv["FontFamily"].ToString(),float.Parse(drv["FontSize"].ToString()),System.Drawing.FontStyle.Bold|System.Drawing.FontStyle.Italic));}
				else if (drv["FontBold"].ToString() == "Y") {fo.ApplyFont(new System.Drawing.Font(drv["FontFamily"].ToString(),float.Parse(drv["FontSize"].ToString()),System.Drawing.FontStyle.Bold));}
				else if (drv["FontItalic"].ToString() == "Y") {fo.ApplyFont(new System.Drawing.Font(drv["FontFamily"].ToString(),float.Parse(drv["FontSize"].ToString()),System.Drawing.FontStyle.Italic));}
				else {fo.ApplyFont(new System.Drawing.Font(drv["FontFamily"].ToString(),float.Parse(drv["FontSize"].ToString()),System.Drawing.FontStyle.Regular));}
				fo.Left = Int32.Parse(drv["PosLeft"].ToString());
				fo.Top = Int32.Parse(drv["PosTop"].ToString());
				fo.Width = Int32.Parse(drv["PosWidth"].ToString());
				fo.Height = Int32.Parse(drv["PosHeight"].ToString());
				if (drv["Suppress"].ToString() == "Y") {fo.ObjectFormat.EnableSuppress = true;} else {fo.ObjectFormat.EnableSuppress = false;}
				if (drv["Alignment"].ToString() == "C") {fo.ObjectFormat.HorizontalAlignment = CrystalDecisions.Shared.Alignment.HorizontalCenterAlign;}
				else if (drv["Alignment"].ToString() == "L") {fo.ObjectFormat.HorizontalAlignment = CrystalDecisions.Shared.Alignment.LeftAlign;}
				else {fo.ObjectFormat.HorizontalAlignment = CrystalDecisions.Shared.Alignment.RightAlign;}
				if (drv["LineTop"].ToString() == "S") {fo.Border.TopLineStyle = CrystalDecisions.Shared.LineStyle.SingleLine;}
				else if (drv["LineTop"].ToString() == "D") {fo.Border.TopLineStyle = CrystalDecisions.Shared.LineStyle.DoubleLine;}
				if (drv["LineBottom"].ToString() == "S") {fo.Border.TopLineStyle = CrystalDecisions.Shared.LineStyle.SingleLine;}
				else if (drv["LineBottom"].ToString() == "D") {fo.Border.TopLineStyle = CrystalDecisions.Shared.LineStyle.DoubleLine;}
			}
		}

		private void getReport(bool sendToPrinter, exportTo eExport)
		{
			string reportName = "AdmRptBizRule";
			cCriteria.Visible = false; cClearCriButton.Visible = false; cShowCriButton.Visible = (bool)Session[KEY_bShCriVisible];
			DataTable dt = (new AdminSystem()).GetRptDt(55,"GetAdmRptBizRule",base.LImpr,base.LCurr,UpdCriteria(false),GetRptCriteria(),LcAppConnString,LcAppPw,false,false,false,CommandTimeOut);
			CovertRptUTC(dt);
			if (dt.Rows.Count > 0) {if (dt.Columns.Contains("ReportName")) {reportName = dt.Rows[0]["ReportName"].ToString();}}
			else {PreMsgPopup("For your information, no data is currently available as per your reporting criteria.");}
			if (Config.DeployType == "DEV" && Config.AppNameSpace == "RO")
			{
				DataSet ds = new DataSet();
				ds.Tables.Add(dt);
				ds.DataSetName = "DsAdmRptBizRule";
				ds.Tables[0].TableName = "DtAdmRptBizRule";
				string xsdPath = Server.MapPath("~/reports/AdmRptBizRuleReport.xsd");
				using (System.IO.StreamWriter writer = new System.IO.StreamWriter(xsdPath))
				{
					ds.WriteXmlSchema(writer);
					writer.Close();
				}
			}
			reportName = reportName + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
			DataView dvItem = GetReportItem(); DataView dvSct = GetReportSct();
			foreach (DataRowView drv in dvSct) {SetReportSct(dvItem, drv["ReportSctName"].ToString());}
			rp.Refresh();
			if (sendToPrinter)
			{
				rp.PrintOptions.PrinterName = cPrinter.SelectedItem.Value;
				rp.PrintOptions.PaperOrientation = PaperOrientation.Landscape;
			}
			rp.SetDataSource(dt);
			cViewer.ReportSource = rp;
			if (cViewerWidth.Value != string.Empty) { cViewer.Width = Unit.Pixel(int.Parse(cViewerWidth.Value)); }
			cViewer.Visible = true;
#pragma warning disable 0612 // Type or member is obsolete
			cViewer.DisplayGroupTree = false;
#pragma warning restore 0612 // Type or member is obsolete
			if (sendToPrinter) { rp.PrintToPrinter(1,false,0,0); }
			if (eExport == exportTo.TXT)
			{
				StringBuilder sb = new StringBuilder();
				if (dt.Columns.Contains("SystemNameIn")) {sb.Append("System Requested" + (char)9);}
				if (dt.Columns.Contains("SystemName")) {sb.Append("System Name" + (char)9);}
				if (dt.Columns.Contains("ScreenDesc")) {sb.Append("Screen Description" + (char)9);}
				if (dt.Columns.Contains("RuleOrder")) {sb.Append("Execution Order" + (char)9);}
				if (dt.Columns.Contains("RuleType")) {sb.Append("Rule Type" + (char)9);}
				if (dt.Columns.Contains("TableDesc")) {sb.Append("Table Description" + (char)9);}
				if (dt.Columns.Contains("RuleName")) {sb.Append("Rule Name" + (char)9);}
				if (dt.Columns.Contains("RuleDesc")) {sb.Append("Rule Description" + (char)9);}
				if (dt.Columns.Contains("ProcedureName")) {sb.Append("Procedure Name" + (char)9);}
				if (dt.Columns.Contains("OnAdd")) {sb.Append("On Add" + (char)9);}
				if (dt.Columns.Contains("OnUpd")) {sb.Append("On Update" + (char)9);}
				if (dt.Columns.Contains("OnDel")) {sb.Append("On Delete" + (char)9);}
				if (dt.Columns.Contains("BeforeCRUD")) {sb.Append("Before CRUD" + (char)9);}
				sb.Append(Environment.NewLine);
				DataView dv = new DataView(dt);
				foreach (DataRowView drv in dv)
				{
					if (dt.Columns.Contains("SystemNameIn")) {sb.Append(drv["SystemNameIn"].ToString() + (char)9);}
					if (dt.Columns.Contains("SystemName")) {sb.Append(drv["SystemName"].ToString() + (char)9);}
					if (dt.Columns.Contains("ScreenDesc")) {sb.Append(drv["ScreenDesc"].ToString() + (char)9);}
					if (dt.Columns.Contains("RuleOrder")) {sb.Append(drv["RuleOrder"].ToString() + (char)9);}
					if (dt.Columns.Contains("RuleType")) {sb.Append(drv["RuleType"].ToString() + (char)9);}
					if (dt.Columns.Contains("TableDesc")) {sb.Append(drv["TableDesc"].ToString() + (char)9);}
					if (dt.Columns.Contains("RuleName")) {sb.Append(drv["RuleName"].ToString() + (char)9);}
					if (dt.Columns.Contains("RuleDesc")) {sb.Append(drv["RuleDesc"].ToString() + (char)9);}
					if (dt.Columns.Contains("ProcedureName")) {sb.Append(drv["ProcedureName"].ToString() + (char)9);}
					if (dt.Columns.Contains("OnAdd")) {sb.Append(drv["OnAdd"].ToString() + (char)9);}
					if (dt.Columns.Contains("OnUpd")) {sb.Append(drv["OnUpd"].ToString() + (char)9);}
					if (dt.Columns.Contains("OnDel")) {sb.Append(drv["OnDel"].ToString() + (char)9);}
					if (dt.Columns.Contains("BeforeCRUD")) {sb.Append(drv["BeforeCRUD"].ToString() + (char)9);}
					sb.Append(Environment.NewLine);
				}
				ExportToStream(null, reportName + ".csv", sb.Insert(0, (Config.ExportExcelCSV ? "sep=\t\n": "")).Replace("\r\n","\n"), exportTo.TXT);
			}
			else if (eExport == exportTo.XLS)
			{
				ExportToStream(rp, reportName + ".xls", null, exportTo.XLS);
			}
			else if (eExport == exportTo.PDF)
			{
				ExportToStream(rp, reportName + ".pdf", null, exportTo.PDF);
			}
			else if (eExport == exportTo.DOC)
			{
				ExportToStream(rp, reportName + ".doc", null, exportTo.DOC);
			}
		}

		private void ExportToStream(object oReport, string sFileName, StringBuilder sb, exportTo eExport)
		{
			System.IO.Stream oStream =  null;
			StreamWriter sw = null;
			ExportOptions oOptions = new ExportOptions();
			ExportRequestContext oRequest = new ExportRequestContext();
			Response.Buffer = true;
			Response.ClearHeaders();
			Response.ClearContent();
			if (eExport == exportTo.TXT)
			{
				oStream = new MemoryStream();
				sw = new StreamWriter(oStream,System.Text.Encoding.Default);
				sw.WriteLine(sb);
				sw.Flush();
				oStream.Seek(0,SeekOrigin.Begin);
				Response.ContentType = "application/vnd.ms-excel";
			}
			else if (eExport == exportTo.XLS)
			{
				oOptions.ExportFormatType = ExportFormatType.Excel;
				oOptions.FormatOptions = new ExcelFormatOptions();
				oRequest.ExportInfo = oOptions;
				oStream = ((ReportDocument)oReport).ExportToStream(ExportFormatType.Excel);
				Response.ContentType = "application/vnd.ms-excel";
			}
			else if (eExport == exportTo.PDF)
			{
				oOptions.ExportFormatType = ExportFormatType.PortableDocFormat;
				oOptions.FormatOptions = new PdfRtfWordFormatOptions();
				oRequest.ExportInfo = oOptions;
				oStream = ((ReportDocument)oReport).ExportToStream(ExportFormatType.PortableDocFormat);
				Response.ContentType = "application/pdf";
			}
			else if (eExport == exportTo.DOC)
			{
				oOptions.ExportFormatType = ExportFormatType.WordForWindows;
				oOptions.FormatOptions = new PdfRtfWordFormatOptions();
				oRequest.ExportInfo = oOptions;
				oStream = ((ReportDocument)oReport).ExportToStream(ExportFormatType.WordForWindows);
				Response.ContentType = "application/msword";
			}
			Response.AppendHeader("Content-Disposition", "Attachment; Filename=\"" + sFileName.Replace(" ","_") + "\"");
			byte[] streamByte = new byte[oStream.Length];
			oStream.Read(streamByte, 0, (int)oStream.Length);
			Response.BinaryWrite(streamByte);
			Response.End();
			if (oStream != null) {oStream.Close();}
			if (sw != null) {sw.Close();}
		}

		public void cShowCriButton_Click(object sender, System.EventArgs e)
		{
			cCriteria.Visible = true; cShowCriButton.Visible = false;
			cClearCriButton.Visible = (bool)Session[KEY_bClCriVisible];
			cViewer.Visible = false;
		}

		public void cClearCriButton_Click(object sender, System.EventArgs e)
		{
			if (cSystemId.Items.Count > 0) {cSystemId.SelectedIndex = 0;}
		}

		public void cExpTxtButton_Click(object sender, System.EventArgs e)
		{
			UpdCriteria(true);
			getReport(false, exportTo.TXT);
		}

		public void cViewButton_Click(object sender, System.EventArgs e)
		{
			UpdCriteria(true);
			getReport(false, exportTo.VIEW);
		}

		public void cExpXlsButton_Click(object sender, System.EventArgs e)
		{
			UpdCriteria(true);
			getReport(false, exportTo.XLS);
		}

		public void cExpPdfButton_Click(object sender, System.EventArgs e)
		{
			UpdCriteria(true);
			getReport(false, exportTo.PDF);
		}

		public void cExpDocButton_Click(object sender, System.EventArgs e)
		{
			UpdCriteria(true);
			getReport(false, exportTo.DOC);
		}

		public void cPrintImage_Click(object sender, System.Web.UI.ImageClickEventArgs e)
		{
			cPrintButton_Click(sender, new EventArgs());
		}

		public void cPrintButton_Click(object sender, System.EventArgs e)
		{
			UpdCriteria(true);
			getReport(true, exportTo.VIEW);
		}

		private void cViewer_Search(object source, CrystalDecisions.Web.SearchEventArgs e)
		{
			getReport(false, exportTo.VIEW);
		}

		private void cViewer_ViewZoom(object source, CrystalDecisions.Web.ZoomEventArgs e)
		{
			getReport(false, exportTo.VIEW);
		}


		private void cViewer_Drill(object source, CrystalDecisions.Web.DrillEventArgs e)
		{
			getReport(false, exportTo.VIEW);
		}

		private void cViewer_Navigate(object source, CrystalDecisions.Web.NavigateEventArgs e)
		{
			getReport(false, exportTo.VIEW);
		}

		private void PreMsgPopup(string msg)
		{
		    int MsgPos = msg.IndexOf("RO.SystemFramewk.ApplicationAssert");
		    string iconUrl = "images/info.gif";
		    string focusOnCloseId = string.Empty;
		    string msgContent = ReformatErrMsg(msg);
		    if (MsgPos >= 0 && LUser.TechnicalUsr != "Y") { msgContent = ReformatErrMsg(msg.Substring(0, MsgPos - 3)); }
			string script =
			@"<script type='text/javascript' lang='javascript'>
			PopDialog('" + iconUrl + "','" + msgContent.Replace(@"\", @"\\").Replace("'", @"\'") + "','" + focusOnCloseId + @"');
			</script>";
			ScriptManager.RegisterStartupScript(cMsgContent, typeof(Label), "Popup", script, false);
		}
	}
}
