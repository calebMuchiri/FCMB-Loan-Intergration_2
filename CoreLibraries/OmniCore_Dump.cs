using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


    class OmniCore_Dump
{

    /*

            #region Methods for Statements
            public static bool GetStatementIB(UserInfo usrInfo, string OurBranchID, string AccountID, string FromDate, string ToDate, string ChargeID, int TopCountTrx, out BRDataSet ds_Ministatement)
            {

                IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
                ds_Ministatement = new BRDataSet();
                IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(6);
                arParms[0] = intfDBHelper.CreateNewDBParam("FromBranchID", SqlDbType.NVarChar, 6);
                arParms[0].Value = OurBranchID;
                arParms[1] = intfDBHelper.CreateNewDBParam("AccountID", SqlDbType.NVarChar, 20);
                arParms[1].Value = AccountID;
                arParms[2] = intfDBHelper.CreateNewDBParam("FromDate", SqlDbType.NVarChar, 20);
                arParms[2].Value = FromDate;
                arParms[3] = intfDBHelper.CreateNewDBParam("ToDate", SqlDbType.NVarChar, 20);
                arParms[3].Value = ToDate;
                arParms[4] = intfDBHelper.CreateNewDBParam("ChargeID", SqlDbType.NVarChar, 20);
                arParms[4].Value = ChargeID;
                arParms[4] = intfDBHelper.CreateNewDBParam("TopCountTrx", SqlDbType.Int);
                arParms[4].Value = TopCountTrx;

                try
                {
                    using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                    {
                        intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetStatementOmni", ds_Ministatement, new string[] { "status", "AccoutStatement" }, arParms);
                    }

                    if (ds_Ministatement.Tables[0].Rows.Count == 0)
                        return false;
                    return true;
                }
                catch (Exception ex)
                {
                    throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                }
                finally
                {
                    ds_Ministatement.Dispose();
                }

            }
            #endregion Methods for Statements

            #region Methods for Maintenance Workflow Loan
            static public string AddWFLoanApplication(UserInfo usrInfo, DS_WFLoanApplications dsWFLoanApplications, out DataSet dsLoanApplication)
            {
                AuditInfo auditInfo = new AuditInfo();
                auditInfo.EventID = BROperation.Add;
                auditInfo.ModuleID = BRModule.WFLoanApplications;
                auditInfo.Status = BRStatus.Successful;
                string strApplicationID = string.Empty;
                IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
                dsLoanApplication = new BRDataSet();
                SqlDbType dataType = SqlDbType.NVarChar;
                int dataSize = 0;

                DataTable dt = dsWFLoanApplications.Tables["WFLoanApplications"];
                dt.Rows[0]["CreatedBy"] = usrInfo.strUser;
                dt.Rows[0]["ModifiedBy"] = usrInfo.strUser;
                string ColumnName = string.Empty, ColumnData = string.Empty;

                int count = dt.Columns.Count;
                IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(count);
                int x = 0;

                if (count > 0)
                {

                    foreach (DataRow row in dt.Rows)
                    {
                        foreach (DataColumn column in dt.Columns)
                        {

                            ColumnName = column.ColumnName.ToString();
                            ColumnData = row[column].ToString();

                            if (ColumnData != "")
                            {
                                if (dt.Columns[x].DataType.Name == "String")
                                {
                                    dataType = SqlDbType.NVarChar; 
                                    dataSize = 50;
                                }
                                if (dt.Columns[x].DataType.Name == "Boolean")
                                {
                                    dataType = SqlDbType.Bit;
                                    dataSize = 1;
                                }
                                if (dt.Columns[x].DataType.Name.Contains("int"))
                                {
                                    dataType = SqlDbType.BigInt;                             
                                }
                                if (dt.Columns[x].DataType.Name == "DateTime")
                                {
                                    dataType = SqlDbType.DateTime;                              
                                }

                                arParms[x] = intfDBHelper.CreateNewDBParam(ColumnName, dataType, dataSize);
                                arParms[x].Value = ColumnData;
                            }

                            x++;
                        }
                    }

                }

                try
                {

                    using (IDbConnection connection = BR.DBClient.DBClient.GetConnection(usrInfo))
                    {
                        intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_AddEditWFLoanApplicationsApi", dsLoanApplication, new string[] { "status", "AccountOpening", }, arParms);
                    }

                    //DR_WFLoanApplications drWFLoanApplications = dsWFLoanApplications.t_WFLoanApplications[0];
                    //using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                    //{
                    //    using (IDbTransaction trans = connection.BeginTransaction())
                    //    {
                    //        try
                    //        {
                    //            strApplicationID = (string)intfDBHelper.ExecuteScalarTypedParams(trans, "p_AddEditWFLoanApplicationsApi", drWFLoanApplications);
                    //            PassAuditInformation(trans, usrInfo, BRUtilities.GetDataSetXML(dsWFLoanApplications), string.Empty, auditInfo);
                    //            trans.Commit();
                    //        }
                    //        catch (Exception ex)
                    //        {
                    //            trans.Rollback();
                    //            auditInfo.Status = BRStatus.Failed;
                    //            auditInfo.Message = ex.Message;
                    //            PassAuditInformation(null, usrInfo, BRUtilities.GetDataSetXML(dsWFLoanApplications), string.Empty, auditInfo);
                    //            throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                    //        }
                    //    }
                    //}
                }
                catch (Exception ex)
                {
                    throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                }
                return strApplicationID;
            }
            static public bool EditWFLoanApplication(UserInfo usrInfo, DS_WFLoanApplications dsWFLoanApplications, string strPrevInfo)
            {
                AuditInfo auditInfo = new AuditInfo();
                auditInfo.EventID = BROperation.Edit;
                auditInfo.ModuleID = BRModule.WFLoanApplications;
                auditInfo.Status = BRStatus.Successful;
                try
                {
                    DR_WFLoanApplications drWFLoanApplications = dsWFLoanApplications.t_WFLoanApplications[0];
                    IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
                    using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                    {
                        using (IDbTransaction trans = connection.BeginTransaction())
                        {
                            try
                            {
                                intfDBHelper.ExecuteScalarTypedParams(trans, "p_AddEditWFLoanApplications", drWFLoanApplications);
                                PassAuditInformation(trans, usrInfo, BRUtilities.GetDataSetXML(dsWFLoanApplications), strPrevInfo, auditInfo);
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.Rollback();
                                auditInfo.Status = BRStatus.Failed;
                                auditInfo.Message = ex.Message;
                                PassAuditInformation(null, usrInfo, BRUtilities.GetDataSetXML(dsWFLoanApplications), strPrevInfo, auditInfo);
                                throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //auditInfo.Status = BRStatus.Failed;
                    //auditInfo.Message = ex.Message;
                    //PassAuditInformation(null, usrInfo, BRUtilities.GetDataSetXML(dsWFLoanApplications), strPrevInfo, auditInfo);
                    throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                }
                return true;
            }
            static public int DeleteWFLoanApplication(UserInfo usrInfo, DS_WFLoanApplications dsWFLoanApplications)
            {
                AuditInfo auditInfo = new AuditInfo();
                auditInfo.EventID = BROperation.Delete;
                auditInfo.ModuleID = BRModule.WFLoanApplications;
                auditInfo.Status = BRStatus.Successful;
                object o = new object();
                try
                {
                    IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
                    IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(4);
                    arParms[0] = intfDBHelper.CreateNewDBParam("OurBranchID", SqlDbType.VarChar, 6);
                    arParms[0].Value = dsWFLoanApplications.t_WFLoanApplications[0].OurBranchID;
                    arParms[1] = intfDBHelper.CreateNewDBParam("ApplicationId", SqlDbType.VarChar, 10);
                    arParms[1].Value = dsWFLoanApplications.t_WFLoanApplications[0].ApplicationID;
                    arParms[2] = intfDBHelper.CreateNewDBParam("OperatorID", SqlDbType.NVarChar, 25);
                    arParms[2].Value = usrInfo.strUser;
                    arParms[3] = intfDBHelper.CreateNewDBParam("UpdateCount", SqlDbType.TinyInt, 3);
                    arParms[3].Value = dsWFLoanApplications.t_WFLoanApplications[0].UpdateCount;
                    using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                    {
                        using (IDbTransaction trans = connection.BeginTransaction())
                        {
                            try
                            {
                                o = intfDBHelper.ExecuteScalar(trans, CommandType.StoredProcedure, "p_DeleteWFLoanApplications", arParms);
                                PassAuditInformation(trans, usrInfo, string.Empty, BRUtilities.GetDataSetXML(dsWFLoanApplications), auditInfo);
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.Rollback();
                                auditInfo.Status = BRStatus.Failed;
                                auditInfo.Message = ex.Message;
                                PassAuditInformation(null, usrInfo, string.Empty, BRUtilities.GetDataSetXML(dsWFLoanApplications), auditInfo);
                                throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                            }
                        }
                    }
                    return 1;
                }
                catch (Exception ex)
                {
                    throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                }
            }
            public static OperationResult EditWFLoanAppraisals(UserInfo usrInfo, DS_WFLoanAppraisals dsWFLoanAppraisals, string strPrevInfo)
            {
                AuditInfo auditInfo = new AuditInfo();
                auditInfo.EventID = BROperation.Edit;
                auditInfo.ModuleID = BRModule.WFLoanAppraisals;
                auditInfo.Status = BRStatus.Successful;
                try
                {
                    IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
                    DR_WFLoanAppraisals drWFLoanAppraisals = dsWFLoanAppraisals.t_WFLoanAppraisals[0];
                    using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                    {
                        using (IDbTransaction trans = connection.BeginTransaction())
                        {
                            try
                            {
                                intfDBHelper.ExecuteScalarTypedParams(trans, "p_EditWFLoanAppraisals", drWFLoanAppraisals);
                                PassAuditInformation(trans, usrInfo, BRUtilities.GetDataSetXML(dsWFLoanAppraisals), strPrevInfo, auditInfo);
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.Rollback();
                                auditInfo.Status = BRStatus.Failed;
                                auditInfo.Message = ex.Message;
                                PassAuditInformation(null, usrInfo, BRUtilities.GetDataSetXML(dsWFLoanAppraisals), strPrevInfo, auditInfo);
                                throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                            }
                        }
                    }
                    return OperationResult.Success;
                }
                catch (Exception ex)
                {
                    throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                }
            }
            static public string AddWFLoanSanction(UserInfo usrInfo, DS_WFLoanAppraisal dsWFLoanApp)
            {
                return AddWFLoanSanction(usrInfo, dsWFLoanApp, false);
            }
            static public string AddWFLoanSanction(UserInfo usrInfo, DS_WFLoanAppraisal dsWFLoanApp, bool SkipAudit)
            {
                AuditInfo auditInfo = new AuditInfo();
                auditInfo.EventID = BROperation.Edit;
                auditInfo.ModuleID = BRModule.WFLoanSanction;
                auditInfo.Status = BRStatus.Successful;
                string strStage;
                try
                {
                    DR_WFLoanAppraisal drLoanAppraisal = dsWFLoanApp.t_WFLoanAppraisal[0];

                    IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
                    dsWFLoanApp.t_WFLoanAppraisal.Columns.Add("NewRecord", typeof(short));
                    drLoanAppraisal["NewRecord"] = 1;

                    using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                    {
                        using (IDbTransaction trans = connection.BeginTransaction())
                        {
                            try
                            {
                                strStage = (string)intfDBHelper.ExecuteScalarTypedParams(trans, "p_AddWFLoanSanction", drLoanAppraisal);
                                if (!SkipAudit)
                                    PassAuditInformation(trans, usrInfo, BRUtilities.GetDataSetXML(dsWFLoanApp), string.Empty, auditInfo);
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.Rollback();
                                auditInfo.Status = BRStatus.Failed;
                                auditInfo.Message = ex.Message;
                                PassAuditInformation(null, usrInfo, BRUtilities.GetDataSetXML(dsWFLoanApp), string.Empty, auditInfo);
                                throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                }
                return strStage;// true;
            }
            static public string AddLoanDisbursement(UserInfo usrInfo, DS_GroupLoanDisbursement dsLoanDisp, DS_CurrencyDenominations dsDenomination)
            {
                AuditInfo auditInfo = new AuditInfo();
                auditInfo.EventID = BROperation.Add;
                auditInfo.ModuleID = BRModule.LoanDisbursement;
                auditInfo.Status = BRStatus.Successful;
                string strSerialID = string.Empty;
                string strBatchID = string.Empty;

                DR_GroupLoanScheme drLoanDisp = dsLoanDisp.t_GroupLoanScheme[0];
                try
                {
                    IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);

                    using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                    {
                        using (IDbTransaction trans = connection.BeginTransaction())
                        {
                            try
                            {
                                // Here Execute scalar with IDataparam array used because Execute scalare with DatRow doesn't support 
                                //out Parameters
                                IDataParameter[] arAddParms = intfDBHelper.GetSPParameters(connection, "p_AddLoanDisbursement");
                                for (int i = 0; i < arAddParms.Length; i++)
                                    if ((dsLoanDisp.t_GroupLoanScheme.Columns.IndexOf(arAddParms[i].ParameterName.Substring(1)) != -1))
                                        arAddParms[i].Value = drLoanDisp[arAddParms[i].ParameterName.Substring(1)];
                                intfDBHelper.ExecuteScalar(trans, CommandType.StoredProcedure, "p_AddLoanDisbursement", arAddParms);

                                // Serial No is Out Parameter, returning the value
                                strBatchID = arAddParms[18].Value.ToString();
                                strSerialID = arAddParms[19].Value.ToString();
                                if (dsDenomination != null)
                                {
                                    AddCurDenominations(intfDBHelper, trans, dsDenomination, strBatchID, strSerialID);
                                }
                                PassAuditInformation(trans, usrInfo, BRUtilities.GetDataSetXML(dsLoanDisp), string.Empty, auditInfo);
                                trans.Commit();
                            }
                            catch (Exception ex)
                            {
                                trans.Rollback();
                                auditInfo.Status = BRStatus.Failed;
                                auditInfo.Message = ex.Message;
                                PassAuditInformation(null, usrInfo, BRUtilities.GetDataSetXML(dsLoanDisp), string.Empty, auditInfo);
                                throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                }
                return strBatchID + '~' + strSerialID;
            }
            static public bool GetWFLoanApplication(UserInfo usrInfo, string OurBranchID, string ApplicationID, out DS_WFLoanApplications dsWFLoanApplications)
            {

                AuditInfo auditInfo = new AuditInfo();
                auditInfo.ModuleID = BRModule.WFLoanApplications;
                auditInfo.EventID = BROperation.View;
                auditInfo.Status = BRStatus.Successful;
                dsWFLoanApplications = new DS_WFLoanApplications();
                IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
                IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(3);
                arParms[0] = intfDBHelper.CreateNewDBParam("OurBranchID", SqlDbType.NVarChar, 6);
                arParms[0].Value = OurBranchID;
                arParms[1] = intfDBHelper.CreateNewDBParam("ApplicationID", SqlDbType.NVarChar, 16);
                arParms[1].Value = ApplicationID;
                arParms[2] = intfDBHelper.CreateNewDBParam("OperatorID", SqlDbType.NVarChar, 25);
                arParms[2].Value = usrInfo.strUser;
                try
                {
                    using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                    {
                        intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetWFLoanApplications", dsWFLoanApplications, new string[] { "SlabDetails", "ClientDetails", "WFLoanApplications" }, arParms);

                    }
                }
                catch (Exception ex)
                {
                    auditInfo.Status = BRStatus.Failed;
                    auditInfo.Message = ex.Message;
                    PassAuditInformation(null, usrInfo, ApplicationID, string.Empty, auditInfo);
                    throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                }
                if (dsWFLoanApplications.t_WFLoanApplications.Rows.Count == 0)
                    return false;
                return true;
            }
            public static OperationResult GetWFLoanAppraisals(UserInfo usrInfo, string strOurBranchID, string strApplicationID, short SerielID, short direction, out DS_WFLoanAppraisals dsWFLoanAppraisal)
            {
                AuditInfo auditInfo = new AuditInfo();
                auditInfo.ModuleID = BRModule.WFLoanAppraisals;
                auditInfo.EventID = BROperation.View;
                auditInfo.Status = BRStatus.Successful;
                dsWFLoanAppraisal = new DS_WFLoanAppraisals();
                IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
                IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(6);

                arParms[0] = intfDBHelper.CreateNewDBParam("OurBranchID", SqlDbType.NVarChar, 6);
                arParms[0].Value = strOurBranchID;
                arParms[1] = intfDBHelper.CreateNewDBParam("ApplicationID", SqlDbType.NVarChar, 16);
                arParms[1].Value = strApplicationID;
                arParms[2] = intfDBHelper.CreateNewDBParam("RefNo", SqlDbType.NVarChar);
                arParms[2].Value = SerielID;
                arParms[3] = intfDBHelper.CreateNewDBParam("OperatorID", SqlDbType.NVarChar, 25);
                arParms[3].Value = usrInfo.strUser;
                arParms[4] = intfDBHelper.CreateNewDBParam("Direction", SqlDbType.SmallInt);
                arParms[4].Value = direction;
                arParms[5] = intfDBHelper.CreateNewDBParam("LogInBranchID", SqlDbType.NVarChar, 6);
                arParms[5].Value = usrInfo.strBranch;
                try
                {
                    using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                    {
                        intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetWFLoanAppraisals", dsWFLoanAppraisal, new string[] { "SlabDetails", "ClientDetails", "WFLoanAppraisals" }, arParms);
                    }
                    return OperationResult.Success;
                }
                catch (Exception ex)
                {
                    auditInfo.Status = BRStatus.Failed;
                    auditInfo.Message = ex.Message;
                    PassAuditInformation(null, usrInfo, strApplicationID, string.Empty, auditInfo);
                    throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                }
            }
            static public bool GetWFLoanSanction(UserInfo usrInfo, string OurBranchID, string strApplicationID, string Direction, out DS_WFLoanAppraisal dsWFLoanSanction)
            {
                AuditInfo auditInfo = new AuditInfo();
                auditInfo.EventID = BROperation.View;
                auditInfo.Status = BRStatus.Successful;
                dsWFLoanSanction = new DS_WFLoanAppraisal();
                IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
                auditInfo.ModuleID = BRModule.WFLoanSanction;
                IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(5);
                arParms[0] = intfDBHelper.CreateNewDBParam("OurBranchID", SqlDbType.NVarChar, 6);
                arParms[0].Value = OurBranchID;
                arParms[1] = intfDBHelper.CreateNewDBParam("ApplicationiD", SqlDbType.VarChar, 16);
                arParms[1].Value = strApplicationID;
                arParms[2] = intfDBHelper.CreateNewDBParam("OperatorID", SqlDbType.NVarChar, 25);
                arParms[2].Value = usrInfo.strUser;
                arParms[3] = intfDBHelper.CreateNewDBParam("Direction", SqlDbType.SmallInt);
                arParms[3].Value = Direction;
                arParms[4] = intfDBHelper.CreateNewDBParam("LogInBranchID", SqlDbType.NVarChar, 6);
                arParms[4].Value = usrInfo.strBranch;
                try
                {
                    using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                    {
                        intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetWFLoanSanction", dsWFLoanSanction, new string[] { dsWFLoanSanction.t_WFLoanAppraisal.TableName, dsWFLoanSanction.t_WFLnsanctionBscene.TableName, dsWFLoanSanction.t_SlabDetails.TableName, dsWFLoanSanction.t_ClientDetails.TableName }, arParms);
                    }

                }
                catch (Exception ex)
                {
                    auditInfo.Status = BRStatus.Failed;
                    auditInfo.Message = ex.Message;
                    PassAuditInformation(null, usrInfo, strApplicationID, string.Empty, auditInfo);
                    throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                }
                return true;
            }
            static public bool GetLoanDisbSchedulesOptional(UserInfo usrInfo, string OurBranchID, string strApplicationID, string ModuleID, out DS_LoanDisbSchedules dsOptionalinfo)
            {
                AuditInfo auditInfo = new AuditInfo();
                auditInfo.EventID = BROperation.View;
                auditInfo.Status = BRStatus.Successful;
                dsOptionalinfo = new DS_LoanDisbSchedules();
                IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
                if (ModuleID == "7070")
                    auditInfo.ModuleID = BRModule.WFLoanDisbursementSchedule;
                else
                    auditInfo.ModuleID = BRModule.PendingApplnWFLoanDisbursementSchedule;
                IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(4);
                arParms[0] = intfDBHelper.CreateNewDBParam("OurBranchID", SqlDbType.NVarChar, 6);
                arParms[0].Value = OurBranchID;
                arParms[1] = intfDBHelper.CreateNewDBParam("ApplicationID", SqlDbType.VarChar, 20);
                arParms[1].Value = strApplicationID;
                arParms[2] = intfDBHelper.CreateNewDBParam("OperatorID", SqlDbType.NVarChar, 25);
                arParms[2].Value = usrInfo.strUser;
                arParms[3] = intfDBHelper.CreateNewDBParam("ModuleID", SqlDbType.Int);
                arParms[3].Value = ModuleID == "7070" ? 7075 : 7416;
                try
                {
                    using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                    {
                        intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetWFLoanDisbSchedules", dsOptionalinfo, new string[] { dsOptionalinfo.t_LoanDisbSchedules.TableName }, arParms);
                    }

                }
                catch (Exception ex)
                {
                    auditInfo.Status = BRStatus.Failed;
                    auditInfo.Message = ex.Message;
                    PassAuditInformation(null, usrInfo, strApplicationID, string.Empty, auditInfo);
                    throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                }
                return true;
            }
            static public bool GetLoanDisb(UserInfo usrInfo, string strBranchID, string strAccountID, out DS_Loans dsLoanDisb)
            {
                AuditInfo auditInfo = new AuditInfo();
                auditInfo.EventID = BROperation.View;
                auditInfo.Status = BRStatus.Successful;
                dsLoanDisb = new DS_Loans();
                IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
                auditInfo.ModuleID = BRModule.LoanDisbursement;


                IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(3);
                arParms[0] = intfDBHelper.CreateNewDBParam("OurBranchID", SqlDbType.VarChar, 6);
                arParms[0].Value = strBranchID;
                arParms[1] = intfDBHelper.CreateNewDBParam("AccountID", SqlDbType.NVarChar, 20);
                arParms[1].Value = strAccountID;
                arParms[2] = intfDBHelper.CreateNewDBParam("OperatorID", SqlDbType.NVarChar, 25);
                arParms[2].Value = usrInfo.strUser;
                try
                {
                    using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                    {
                        intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetLoanDisbursement", dsLoanDisb, new string[] { "Loans", "BHSLoans" }, arParms);
                    }
                }
                catch (Exception ex)
                {
                    auditInfo.Status = BRStatus.Failed;
                    auditInfo.Message = ex.Message;
                    PassAuditInformation(null, usrInfo, strAccountID, string.Empty, auditInfo);
                    throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                }
                return true;
            }
            static public bool GetLoanInitiationDetail(UserInfo usrInfo, string strBranchID, string strAccountID, string LoanSeries, string ModuleID, string EventID, out DS_LoanInitiationDetail dsLoanInitiationDetail)
            {
                AuditInfo auditInfo = new AuditInfo();
                auditInfo.EventID = BROperation.View;
                string strsp = null;
                if (ModuleID == "4400")
                {
                    auditInfo.ModuleID = BRModule.LoanRescheduleInitiation;
                    strsp = "p_GetLoanRescheduleComp";
                }
                else
                {
                    auditInfo.ModuleID = BRModule.LoanRefinanceInitiation;
                    strsp = "p_GetLoanRefinanceComp";
                }
                auditInfo.Status = BRStatus.Successful;

                IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
                dsLoanInitiationDetail = new DS_LoanInitiationDetail();
                IDataParameter[] arParms;
                if (EventID == "NONE")
                {
                    arParms = intfDBHelper.CreateDBParamsArray(3);
                    arParms[0] = intfDBHelper.CreateNewDBParam("OurBranchID", SqlDbType.VarChar, 6);
                    arParms[0].Value = strBranchID;
                    arParms[1] = intfDBHelper.CreateNewDBParam("AccountID", SqlDbType.VarChar, 20);
                    arParms[1].Value = strAccountID;
                    arParms[2] = intfDBHelper.CreateNewDBParam("OperatorID", SqlDbType.VarChar, 25);
                    arParms[2].Value = usrInfo.strUser;
                }
                else
                {
                    arParms = intfDBHelper.CreateDBParamsArray(4);
                    arParms[0] = intfDBHelper.CreateNewDBParam("OurBranchID", SqlDbType.VarChar, 6);
                    arParms[0].Value = strBranchID;
                    arParms[1] = intfDBHelper.CreateNewDBParam("AccountID", SqlDbType.VarChar, 20);
                    arParms[1].Value = strAccountID;
                    arParms[2] = intfDBHelper.CreateNewDBParam("LoanSeries", SqlDbType.Int);
                    arParms[2].Value = LoanSeries == "" ? System.Convert.DBNull : Convert.ToInt16(LoanSeries);
                    arParms[3] = intfDBHelper.CreateNewDBParam("OperatorID", SqlDbType.VarChar, 25);
                    arParms[3].Value = usrInfo.strUser;
                }
                try
                {
                    using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                    {
                        if (EventID == "NONE")
                        {
                            if (ModuleID == "4400")
                                intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetLoanRescheduleDetails", dsLoanInitiationDetail, new string[] { "LoanInitiationGridDetails", "LoanInitiationDetail", "LoanInitiationBHS", "SlabDetails", "LoanInitiationValidation" }, arParms);
                            else
                                intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetLoanRefinanceDetails", dsLoanInitiationDetail, new string[] { "LoanAccounts", "LoanInitiationGridDetails", "LoanInitiationDetail", "LoanInitiationBHS", "SlabDetails", "LoanInitiationValidation" }, arParms);
                        }
                        else
                            intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, strsp, dsLoanInitiationDetail, new string[] { "LoanInitiationGridDetails" }, arParms);
                    }
                }
                catch (Exception ex)
                {
                    auditInfo.Status = BRStatus.Failed;
                    auditInfo.Message = ex.Message;
                    PassAuditInformation(null, usrInfo, strAccountID, string.Empty, auditInfo);
                    throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);

                }
                if (dsLoanInitiationDetail.t_LoanInitiationDetail.Rows.Count == 0)
                    return false;
                return true;

            }
            static public string AddLoanInitiationDetail(UserInfo usrInfo, DS_LoanInitiationDetail dsLoanInitiationDetail)
            {
                AuditInfo auditInfo = new AuditInfo();
                auditInfo.EventID = BROperation.Add;
                string strsp = string.Empty;
                string strApplicationID = string.Empty;
                if (dsLoanInitiationDetail.t_LoanInitiationDetail[0].ModuleID == "4400")
                {
                    auditInfo.ModuleID = BRModule.LoanRescheduleInitiation;
                    strsp = "p_AddEditLoanReschedule";
                }
                else
                {
                    auditInfo.ModuleID = BRModule.LoanRefinanceInitiation;
                    strsp = "p_AddEditLoanRefinance";
                }
                auditInfo.Status = BRStatus.Successful;

                DS_LoanInitiationDetail dsTemp = new DS_LoanInitiationDetail();
                DR_LoanInitiationDetail drLoanInitiationDetail = dsLoanInitiationDetail.t_LoanInitiationDetail[0];
                dsTemp = (DS_LoanInitiationDetail)dsLoanInitiationDetail.Copy();
                dsTemp.Relations.Clear();
                dsTemp.Tables[dsTemp.t_LoanInitiationDetail.TableName].Constraints.Clear();
                dsTemp.Tables.Remove(dsTemp.t_LoanInitiationDetail.TableName);

                BRDALUtilities.RemoveColumns(dsTemp, dsTemp.t_LoanInitiationGridDetails.TableName, "Component");
                if (!dsLoanInitiationDetail.t_LoanInitiationDetail.Columns.Contains("DetailRecords"))
                {
                    dsLoanInitiationDetail.t_LoanInitiationDetail.Columns.Add("DetailRecords", typeof(string));
                    drLoanInitiationDetail["DetailRecords"] = BRDALUtilities.GetXmlTable(dsTemp);
                }
                else
                    drLoanInitiationDetail["DetailRecords"] = BRDALUtilities.GetXmlTable(dsTemp);

                IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
                try
                {
                    using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                    {
                        using (IDbTransaction trans = connection.BeginTransaction())
                        {
                            strApplicationID = (string)intfDBHelper.ExecuteScalarTypedParams(trans, strsp, drLoanInitiationDetail);
                            PassAuditInformation(trans, usrInfo, BRUtilities.GetDataSetXML(dsLoanInitiationDetail), string.Empty, auditInfo);
                            trans.Commit();
                        }
                    }
                }
                catch (Exception ex)
                {
                    auditInfo.Status = BRStatus.Failed;
                    auditInfo.Message = ex.Message;
                    PassAuditInformation(null, usrInfo, BRUtilities.GetDataSetXML(dsLoanInitiationDetail), string.Empty, auditInfo);
                    throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                }
                return strApplicationID;
            }
            static public string AddLoanRescheduleTrxPosting(UserInfo usrInfo, DS_LoanPreClosureDetail dsLoanRescheduleTrxPost, DS_Transactions dsTransactions, DS_CurrencyDenominations dsCurrDenomination)
            {
                AuditInfo auditInfo = new AuditInfo();
                auditInfo.EventID = BROperation.Add;
                auditInfo.ModuleID = BRModule.RescheduleTrxPosting;
                auditInfo.Status = BRStatus.Successful;
                string strBatchID = string.Empty;
                string strSerialID = string.Empty;
                if (!dsLoanRescheduleTrxPost.t_BHSLoanPreClosureDetail.Columns.Contains("TrxSerialID"))
                    dsLoanRescheduleTrxPost.t_BHSLoanPreClosureDetail.Columns.Add("TrxSerialID", typeof(System.Int64));
                DR_BHSLoanPreClosureDetail drBHSLoanPreClosure = dsLoanRescheduleTrxPost.t_BHSLoanPreClosureDetail[0];
                if (dsTransactions.t_Transactions != null)
                    BRDALUtilities.RemoveColumns(dsTransactions, dsTransactions.t_Transactions.TableName, "TrxRowID", "TrxBatchID", "SerialID", "ModuleID", "InstrumentTypeID", "ChequeID", "ChequeDate", "TrxPrinted", "TrxFlagID", "ImageID", "UnsupervisedAmount", "IsSystemTrx", "Remarks", "AppID", "ForwardToUser", "ForwardToGroup", "CreatedBy", "CreatedOn", "SupervisedBy", "SupervisedOn", "SupervisedBy2", "SupervisedOn2", "DeletedBy", "DeletedOn",
                                    "DeletedReason", "DeletionSupervisedBy", "DeletionSupervisedOn", "CurrencyName", "BankID", "BankName", "BranchID", "BranchName", "DrawerOrPayeeAccountID", "DrawerOrPayee", "AEDOperation", "TrxFlag", "SlNo", "ChequeDigit", "VoucherCode", "ReturnCodeID", "Commission", "TheirCommission", "AccountCurrencyID", "Balance", "SubGroupID", "IsForfeit", "Description");
                if (!dsLoanRescheduleTrxPost.t_BHSLoanPreClosureDetail.Columns.Contains("DetailRecords"))
                {
                    dsLoanRescheduleTrxPost.t_BHSLoanPreClosureDetail.Columns.Add("DetailRecords", typeof(string));
                    drBHSLoanPreClosure["DetailRecords"] = BRDALUtilities.GetXmlTable(dsTransactions);
                }
                else
                    drBHSLoanPreClosure["DetailRecords"] = BRDALUtilities.GetXmlTable(dsTransactions);
                if (!dsLoanRescheduleTrxPost.t_BHSLoanPreClosureDetail.Columns.Contains("IsCloseAccount"))
                {
                    dsLoanRescheduleTrxPost.t_BHSLoanPreClosureDetail.Columns.Add("IsCloseAccount", typeof(bool));
                    drBHSLoanPreClosure["IsCloseAccount"] = 0;
                }
                else
                    drBHSLoanPreClosure["IsCloseAccount"] = 0;

                IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
                try
                {
                    using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                    {
                        using (IDbTransaction trans = connection.BeginTransaction())
                        {
                            // Here Execute scalar with IDataparam array used because Execute scalare with DatRow doesn't support 
                            //out Parameters
                            IDataParameter[] arAddParms = intfDBHelper.GetSPParameters(connection, "p_AddLoanReschPostTrx");
                            for (int i = 0; i < arAddParms.Length; i++)
                                if ((dsLoanRescheduleTrxPost.t_BHSLoanPreClosureDetail.Columns.IndexOf(arAddParms[i].ParameterName.Substring(1)) != -1))
                                    arAddParms[i].Value = drBHSLoanPreClosure[arAddParms[i].ParameterName.Substring(1)];
                            intfDBHelper.ExecuteScalar(trans, CommandType.StoredProcedure, "p_AddLoanReschPostTrx", arAddParms);

                            // Serial No is Out Parameter, returning the value
                            strBatchID = arAddParms[10].Value.ToString();
                            strSerialID = arAddParms[11].Value.ToString();
                            if (dsCurrDenomination != null)
                            {
                                AddCurDenominations(intfDBHelper, trans, dsCurrDenomination, strBatchID, strSerialID);
                            }
                            PassAuditInformation(trans, usrInfo, BRUtilities.GetDataSetXML(dsLoanRescheduleTrxPost), string.Empty, auditInfo);
                            trans.Commit();
                        }
                    }
                }
                catch (Exception ex)
                {
                    auditInfo.Status = BRStatus.Failed;
                    auditInfo.Message = ex.Message;
                    PassAuditInformation(null, usrInfo, BRUtilities.GetDataSetXML(dsLoanRescheduleTrxPost), string.Empty, auditInfo);
                    throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                }
                return strSerialID;
            }
            static public string AddLoanPreClosurePost(UserInfo usrInfo, DS_Transactions dsTransactions, DS_LoanPreClosureDetail dsLoanPreClosureDetail)
            {
                AuditInfo auditInfo = new AuditInfo();
                auditInfo.EventID = BROperation.Add;
                auditInfo.ModuleID = BRModule.LoanPreClosePosting;
                auditInfo.Status = BRStatus.Successful;
                auditInfo.Message = "";
                string strBatchID = string.Empty;
                string strSerialID = string.Empty;

                DR_BHSLoanPreClosureDetail drLoanPreClosureDetail = dsLoanPreClosureDetail.t_BHSLoanPreClosureDetail[0];

                DS_Transactions dsTemp = new DS_Transactions();
                dsTemp = (DS_Transactions)dsTransactions.Copy();
                dsTemp.Relations.Clear();

                if (!dsLoanPreClosureDetail.t_BHSLoanPreClosureDetail.Columns.Contains("DetailRecords"))
                {
                    dsLoanPreClosureDetail.t_BHSLoanPreClosureDetail.Columns.Add("DetailRecords", typeof(string));
                    drLoanPreClosureDetail["DetailRecords"] = BRDALUtilities.GetXmlTable(dsTemp);
                }
                else
                {
                    drLoanPreClosureDetail["DetailRecords"] = BRDALUtilities.GetXmlTable(dsTemp);
                }
                IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
                try
                {
                    using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                    {
                        using (IDbTransaction trans = connection.BeginTransaction())
                        {
                            IDataParameter[] arAddParms = intfDBHelper.GetSPParameters(connection, "p_AddLoanPCPostTrx");
                            for (int i = 0; i < arAddParms.Length; i++)
                                if ((dsLoanPreClosureDetail.t_BHSLoanPreClosureDetail.Columns.IndexOf(arAddParms[i].ParameterName.Substring(1)) != -1))
                                    arAddParms[i].Value = drLoanPreClosureDetail[arAddParms[i].ParameterName.Substring(1)];
                            intfDBHelper.ExecuteScalar(trans, CommandType.StoredProcedure, "p_AddLoanPCPostTrx", arAddParms);

                            // Serial No is Out Parameter, returning the value
                            strBatchID = arAddParms[10].Value.ToString();
                            strSerialID = arAddParms[11].Value.ToString();
                            PassAuditInformation(trans, usrInfo, BRUtilities.GetDataSetXML(dsLoanPreClosureDetail), string.Empty, auditInfo);
                            trans.Commit();
                        }
                    }
                }
                catch (Exception ex)
                {
                    auditInfo.Status = BRStatus.Failed;
                    auditInfo.Message = ex.Message;
                    PassAuditInformation(null, usrInfo, BRUtilities.GetDataSetXML(dsLoanPreClosureDetail), string.Empty, auditInfo);
                    throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                }
                return strSerialID;
            }
            static public bool GetLoanPreClosureDetail(UserInfo usrInfo, string strBranchID, string strAccountID, out DS_LoanPreClosureDetail dsLoanPreClosureDetail)
            {
                AuditInfo auditInfo = new AuditInfo();
                auditInfo.EventID = BROperation.View;
                auditInfo.ModuleID = BRModule.LoanPreCloseInitiation;
                auditInfo.Status = BRStatus.Successful;

                IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
                dsLoanPreClosureDetail = new DS_LoanPreClosureDetail();

                IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(3);
                arParms[0] = intfDBHelper.CreateNewDBParam("OurBranchID", SqlDbType.VarChar, 6);
                arParms[0].Value = strBranchID;
                arParms[1] = intfDBHelper.CreateNewDBParam("AccountID", SqlDbType.VarChar, 20);
                arParms[1].Value = strAccountID;
                arParms[2] = intfDBHelper.CreateNewDBParam("OperatorID", SqlDbType.VarChar, 25);
                arParms[2].Value = usrInfo.strUser;

                try
                {
                    using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                    {
                        intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetLoanPreClosureDetail", dsLoanPreClosureDetail, new string[] { "LoanPreClosureDetail", "BHSLoanPreClosureDetail" }, arParms);

                    }
                }
                catch (Exception ex)
                {
                    auditInfo.Status = BRStatus.Failed;
                    auditInfo.Message = ex.Message;
                    PassAuditInformation(null, usrInfo, strAccountID, string.Empty, auditInfo);
                    throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);

                }
                if (dsLoanPreClosureDetail.t_BHSLoanPreClosureDetail.Rows.Count == 0)
                    return false;
                return true;
            }
            static public bool GetPreClosureTrxDetails(UserInfo usrInfo, string strBranchID, string strAccountID, short LoanSeries, string TrxDate, DS_Transactions dsTransactions, DS_CurrencyDenominations dsDenomination, double NetSettlementAmount, out DS_Transactions dsPreClosureTrxDetails)
            {
                AuditInfo auditInfo = new AuditInfo();
                auditInfo.EventID = BROperation.View;
                auditInfo.ModuleID = BRModule.LoanPreClosePosting;
                auditInfo.Status = BRStatus.Successful;
                dsTransactions.Relations.Clear();
                if (dsTransactions.t_Transactions != null)
                    dsTransactions.t_Transactions.Columns.Add("DetailRecords", typeof(string));

                IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
                dsPreClosureTrxDetails = new DS_Transactions();

                IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(6);
                arParms[0] = intfDBHelper.CreateNewDBParam("OurBranchID", SqlDbType.VarChar, 6);
                arParms[0].Value = strBranchID;
                arParms[1] = intfDBHelper.CreateNewDBParam("AccountID", SqlDbType.VarChar, 20);
                arParms[1].Value = strAccountID;
                arParms[2] = intfDBHelper.CreateNewDBParam("LoanSeries", SqlDbType.Int);
                arParms[2].Value = LoanSeries;
                arParms[3] = intfDBHelper.CreateNewDBParam("TrxDate", SqlDbType.SmallDateTime);
                arParms[3].Value = Convert.ToDateTime(TrxDate);
                arParms[4] = intfDBHelper.CreateNewDBParam("DetailRecords", SqlDbType.Xml);
                arParms[4].Value = BRDALUtilities.GetXmlTable(dsTransactions);
                arParms[5] = intfDBHelper.CreateNewDBParam("OperatorID", SqlDbType.VarChar, 25);
                arParms[5].Value = usrInfo.strUser;
                try
                {
                    using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                    {
                        intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetLoanPCPostTrx", dsPreClosureTrxDetails, new string[] { "Transactions" }, arParms);
                    }
                }
                catch (Exception ex)
                {
                    auditInfo.Status = BRStatus.Failed;
                    auditInfo.Message = ex.Message;
                    PassAuditInformation(null, usrInfo, strAccountID, string.Empty, auditInfo);
                    throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);

                }
                if (dsPreClosureTrxDetails.t_Transactions.Rows.Count == 0)
                    return false;
                return true;
            }

        public static bool GetCustomerLoansQuery(UserInfo usrInfo, string strOurBranchID, string strClientID, out BRDataSet ds_CustomerQuery)
            {
                IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
                ds_CustomerQuery = new BRDataSet();
                IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(3);
                arParms[0] = intfDBHelper.CreateNewDBParam("OurBranchID", SqlDbType.NVarChar, 6);
                arParms[0].Value = strOurBranchID;
                arParms[1] = intfDBHelper.CreateNewDBParam("ClientID", SqlDbType.NVarChar, 20);
                arParms[1].Value = strClientID;
                arParms[2] = intfDBHelper.CreateNewDBParam("OperatorID", SqlDbType.NVarChar, 20);
                arParms[2].Value = usrInfo.strUser;
                try
                {
                    using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                    {
                        intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetCustomerLoansQueryOmni", ds_CustomerQuery, new string[] { "status", "CustomerLoansQuery" }, arParms);
                    }

                    if (ds_CustomerQuery.Tables[1].Rows.Count == 0)
                        return false;
                    return true;
                }
                catch (Exception ex)
                {
                    throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                }
                finally
                {
                    ds_CustomerQuery.Dispose();
                }
            }
            #endregion Methods for Maintenance Workflow Loan

        static public void PassAuditInformation(IDbTransaction trans, UserInfo usrInfo, string newInfo, string prevInfo, AuditInfo auditInfo)
            {
                if (auditInfo.Message == string.Empty && auditInfo.MsgID != 0)
                {
                    auditInfo.Message = BR.Resources.GetLocalisedText(auditInfo.MsgID.ToString(), usrInfo.strBank);
                }

                IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);

                IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(11);
                arParms[0] = intfDBHelper.CreateNewDBParam("ApplicationID", SqlDbType.TinyInt);
                arParms[1] = intfDBHelper.CreateNewDBParam("OurBranchID", SqlDbType.NVarChar, 6);
                arParms[2] = intfDBHelper.CreateNewDBParam("OperatorID", SqlDbType.NVarChar, 25);
                arParms[3] = intfDBHelper.CreateNewDBParam("EventID", SqlDbType.TinyInt);
                arParms[4] = intfDBHelper.CreateNewDBParam("Status", SqlDbType.TinyInt);
                arParms[5] = intfDBHelper.CreateNewDBParam("ModuleID", SqlDbType.SmallInt);
                arParms[6] = intfDBHelper.CreateNewDBParam("IPAddress", SqlDbType.NVarChar, 20);
                arParms[7] = intfDBHelper.CreateNewDBParam("NewData", SqlDbType.NVarChar);
                arParms[8] = intfDBHelper.CreateNewDBParam("OldData", SqlDbType.NVarChar);
                arParms[9] = intfDBHelper.CreateNewDBParam("MessageID", SqlDbType.Int);
                arParms[10] = intfDBHelper.CreateNewDBParam("Message", SqlDbType.NVarChar, 255);

                arParms[0].Value = auditInfo.AppID;
                arParms[1].Value = usrInfo.strBranch;
                arParms[2].Value = usrInfo.strUser;
                arParms[3].Value = auditInfo.EventID;
                arParms[4].Value = auditInfo.Status;
                if (auditInfo.ModuleID.ToString().ToUpper() == "NONE")
                    arParms[5].Value = auditInfo.RptModuleID;
                else
                    arParms[5].Value = auditInfo.ModuleID;
                arParms[6].Value = usrInfo.MachineIP;
                arParms[7].Value = newInfo;
                arParms[8].Value = prevInfo;
                arParms[9].Value = auditInfo.MsgID;
                arParms[10].Value = auditInfo.Message;
                try
                {
                    if (Convert.ToBoolean(BRConfigurationManager.BRAppSettings["AuditRequired"].ToString()))
                    {
                        string AuditType = BRConfigurationManager.BRAppSettings["AuditType"].ToString();
                        string AuditData = string.Empty;
                        string strFileName = BRConfigurationManager.BRAppSettings["AuditLogPath"].ToString() + usrInfo.strSystem + "\\";
                        System.IO.Directory.CreateDirectory(strFileName);
                        if (AuditType.ToUpper() == "FILE")
                        {
                            if (auditInfo.ModuleID.ToString().ToUpper() == "NONE")
                            {
                                if (newInfo == "" && prevInfo == "")
                                    AuditData = "BranchID" + ":" + usrInfo.strBranch + Environment.NewLine + "EventID" + ":" + auditInfo.EventID + Environment.NewLine + "Status" + ":" + auditInfo.Status + Environment.NewLine + "TimeStamp" + ":" + DateTime.Now + Environment.NewLine + "Module" + ":" + "Reports/" + auditInfo.RptModuleID + Environment.NewLine + "IP Address" + ":" + usrInfo.MachineIP + Environment.NewLine + "-------------------------------" + Environment.NewLine;
                                else if (newInfo == "")
                                    AuditData = "BranchID" + ":" + usrInfo.strBranch + Environment.NewLine + "EventID" + ":" + auditInfo.EventID + Environment.NewLine + "Status" + ":" + auditInfo.Status + Environment.NewLine + "TimeStamp" + ":" + DateTime.Now + Environment.NewLine + "Module" + ":" + "Reports/" + auditInfo.RptModuleID + Environment.NewLine + "IP Address" + ":" + usrInfo.MachineIP + Environment.NewLine + "Old Data" + ":" + prevInfo + Environment.NewLine + "-------------------------------" + Environment.NewLine;
                                else if (prevInfo == "")
                                    AuditData = "BranchID" + ":" + usrInfo.strBranch + Environment.NewLine + "EventID" + ":" + auditInfo.EventID + Environment.NewLine + "Status" + ":" + auditInfo.Status + Environment.NewLine + "TimeStamp" + ":" + DateTime.Now + Environment.NewLine + "Module" + ":" + "Reports/" + auditInfo.RptModuleID + Environment.NewLine + "IP Address" + ":" + usrInfo.MachineIP + Environment.NewLine + "New Data" + ":" + newInfo + Environment.NewLine + "-------------------------------" + Environment.NewLine;
                                else
                                    AuditData = "BranchID" + ":" + usrInfo.strBranch + Environment.NewLine + "EventID" + ":" + auditInfo.EventID + Environment.NewLine + "Status" + ":" + auditInfo.Status + Environment.NewLine + "TimeStamp" + ":" + DateTime.Now + Environment.NewLine + "Module" + ":" + "Reports/" + auditInfo.RptModuleID + Environment.NewLine + "IP Address" + ":" + usrInfo.MachineIP + Environment.NewLine + "New Data" + ":" + newInfo + Environment.NewLine + "-------------------------------" + Environment.NewLine + "Old Data" + ":" + prevInfo + Environment.NewLine + "-------------------------------" + Environment.NewLine;
                            }
                            else
                            {
                                if (newInfo == "" && prevInfo == "")
                                    AuditData = "BranchID" + ":" + usrInfo.strBranch + Environment.NewLine + "EventID" + ":" + auditInfo.EventID + Environment.NewLine + "Status" + ":" + auditInfo.Status + Environment.NewLine + "TimeStamp" + ":" + DateTime.Now + Environment.NewLine + "Module" + ":" + auditInfo.ModuleID + Environment.NewLine + "IP Address" + ":" + usrInfo.MachineIP + Environment.NewLine + "-------------------------------" + Environment.NewLine;
                                else if (newInfo == "")
                                    AuditData = "BranchID" + ":" + usrInfo.strBranch + Environment.NewLine + "EventID" + ":" + auditInfo.EventID + Environment.NewLine + "Status" + ":" + auditInfo.Status + Environment.NewLine + "TimeStamp" + ":" + DateTime.Now + Environment.NewLine + "Module" + ":" + auditInfo.ModuleID + Environment.NewLine + "IP Address" + ":" + usrInfo.MachineIP + Environment.NewLine + "Old Data" + ":" + prevInfo + Environment.NewLine + "-------------------------------" + Environment.NewLine;
                                else if (prevInfo == "")
                                    AuditData = "BranchID" + ":" + usrInfo.strBranch + Environment.NewLine + "EventID" + ":" + auditInfo.EventID + Environment.NewLine + "Status" + ":" + auditInfo.Status + Environment.NewLine + "TimeStamp" + ":" + DateTime.Now + Environment.NewLine + "Module" + ":" + auditInfo.ModuleID + Environment.NewLine + "IP Address" + ":" + usrInfo.MachineIP + Environment.NewLine + "New Data" + ":" + newInfo + Environment.NewLine + "-------------------------------" + Environment.NewLine;
                                else
                                    AuditData = "BranchID" + ":" + usrInfo.strBranch + Environment.NewLine + "EventID" + ":" + auditInfo.EventID + Environment.NewLine + "Status" + ":" + auditInfo.Status + Environment.NewLine + "TimeStamp" + ":" + DateTime.Now + Environment.NewLine + "Module" + ":" + auditInfo.ModuleID + Environment.NewLine + "IP Address" + ":" + usrInfo.MachineIP + Environment.NewLine + "New Data" + ":" + newInfo + Environment.NewLine + "-------------------------------" + Environment.NewLine + "Old Data" + ":" + prevInfo + Environment.NewLine + "-------------------------------" + Environment.NewLine;
                            }
                            File.AppendAllText(strFileName + usrInfo.strUser + DateTime.Now.ToString("yyyy-MM-dd").Replace("-", "") + ".txt", Environment.NewLine);
                            File.AppendAllText(strFileName + usrInfo.strUser + DateTime.Now.ToString("yyyy-MM-dd").Replace("-", "") + ".txt", AuditData);
                        }
                        else
                        {

                            if (trans == null)
                            {
                                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                                {
                                    using (IDbTransaction transact = connection.BeginTransaction())
                                    {
                                        intfDBHelper.ExecuteScalar(transact, CommandType.StoredProcedure, "p_AddAuditLogCode", arParms);
                                        transact.Commit();
                                    }
                                }
                            }
                            else
                            {
                                intfDBHelper.ExecuteScalar(trans, CommandType.StoredProcedure, "p_AddAuditLogCode", arParms);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                }
            } 

        #region Currency Stuff       
            private static void AddCurDenominations(IDBHelper intfDBHelper, IDbTransaction trans, DS_CurrencyDenominations dsCurrency, string strTrxBatchID, string strSerialNo)
            {
                DS_CurrencyDenominations dsTemp = new DS_CurrencyDenominations();
                BREntities.CurrencyDenominations.DR_CurrencyTransactions drCurrency = dsCurrency.t_CurrencyTransactions[0];
                dsTemp = (DS_CurrencyDenominations)dsCurrency.Copy();
                BRDALUtilities.RemoveColumns(dsTemp, dsTemp.t_CurrencyTransactions.TableName, "CurrencyTrxID", "OurBranchID", "TrxDate", "SerialNo", "TrxBatchID", "TillID", "IsAccepted", "ModuleID");//
                if (!dsCurrency.t_CurrencyTransactions.Columns.Contains("SerialNo"))
                {
                    dsCurrency.t_CurrencyTransactions.Columns.Add("SerialNo", typeof(int));
                    drCurrency["SerialNo"] = Convert.ToInt16(strSerialNo);
                }
                else
                    drCurrency["SerialNo"] = Convert.ToInt16(strSerialNo);
                if (!dsCurrency.t_CurrencyTransactions.Columns.Contains("TrxBatchID"))
                {
                    dsCurrency.t_CurrencyTransactions.Columns.Add("TrxBatchID", typeof(string));
                    drCurrency["TrxBatchID"] = strTrxBatchID;
                }
                else
                    drCurrency["TrxBatchID"] = strTrxBatchID;
                if (!dsCurrency.t_CurrencyTransactions.Columns.Contains("DetailRecords"))
                {
                    dsCurrency.t_CurrencyTransactions.Columns.Add("DetailRecords", typeof(string));
                    drCurrency["DetailRecords"] = BRDALUtilities.GetXmlTable(dsTemp);
                }
                else
                    drCurrency["DetailRecords"] = BRDALUtilities.GetXmlTable(dsTemp);
                intfDBHelper.ExecuteScalarTypedParams(trans, "p_AddCurDenominations", drCurrency);
            }
            public static bool GetDailyExchangeRates(UserInfo usrInfo, out BRDataSet ds_ExhangeRates)
            {

                IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
                ds_ExhangeRates = new BRDataSet();
                IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(3);
                try
                {
                    using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                    {
                        intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetDailyExchangeRates", ds_ExhangeRates, new string[] { "GetDailyExchangeRates" });
                    }

                    if (ds_ExhangeRates.Tables[0].Rows.Count == 0)
                        return false;
                    return true;
                }
                catch (Exception ex)
                {
                    throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                }
                finally
                {
                    ds_ExhangeRates.Dispose();
                }
            }

            #region Loan Application Process

        public static string LoanApplicationProcess(UserInfo usrInfo, string strStage, DataSet MyApplication, out DataSet dsLoanApp)
        {
            string Response = string.Empty;

            string ApplicationID = string.Empty;
            string OurBranchID = string.Empty;
            string columnName = string.Empty;
            string tableName = string.Empty;
            DataSet dsLoanApplication = new DataSet();
            dsLoanApp = new DataSet();


            WFStageID wfStageID = (CoreOmniApiBase.WFStageID)Enum.Parse(typeof(CoreOmniApiBase.WFStageID), strStage, true);


            if (wfStageID != WFStageID.APPL)
                MyApplication = GetLoanApplicationDetails(usrInfo, wfStageID, MyApplication);

            switch (wfStageID)
            {
                case WFStageID.APPL:

                    DS_WFLoanApplications dsSaveWFLoanApplications = new DS_WFLoanApplications();
                    dsSaveWFLoanApplications.Merge(MyApplication, true, MissingSchemaAction.Ignore);
                    if (!dsSaveWFLoanApplications.t_WFLoanApplications.Columns.Contains("UpdateCount"))
                        dsSaveWFLoanApplications.t_WFLoanApplications.Columns.Add("UpdateCount", typeof(Int16));
                    dsSaveWFLoanApplications.t_WFLoanApplications[0]["UpdateCount"] = 1;
                    dsSaveWFLoanApplications.t_WFLoanApplications[0]["CreatedBy"] = usrInfo.strUser;
                    ApplicationID = OmniCore.AddWFLoanApplication(usrInfo, dsSaveWFLoanApplications, out dsLoanApplication);
                    if (ApplicationID.IndexOf(":") > -1)
                    {
                        ApplicationID = ApplicationID.Split(new char[] { ':' })[0];
                    }
                    dsLoanApp = dsLoanApplication;

                    break;

                case WFStageID.APPR:

                    DS_WFLoanAppraisals dsWFLoanAppraisals = new DS_WFLoanAppraisals();
                    dsWFLoanAppraisals.Merge(MyApplication, true, MissingSchemaAction.Ignore);
                    if (!dsWFLoanAppraisals.t_WFLoanAppraisals.Columns.Contains("AppraisedBy"))
                        dsWFLoanAppraisals.t_WFLoanAppraisals.Columns.Add("AppraisedBy", typeof(string));
                    dsWFLoanAppraisals.t_WFLoanAppraisals[0]["AppraisedBy"] = usrInfo.strUser;
                    string strPrevInfo = string.Empty;
                    OmniCore.EditWFLoanAppraisals(usrInfo, dsWFLoanAppraisals, strPrevInfo);
                    Response = "<BRDataSet><dt_WebServiceStatus><WebServiceStatus>00</WebServiceStatus></dt_WebServiceStatus></BRDataSet>";
                    break;

                case WFStageID.SANC:

                    DS_WFLoanAppraisal dsWFLoanApp = new DS_WFLoanAppraisal();
                    dsWFLoanApp.Merge(MyApplication, true, MissingSchemaAction.Ignore);
                    tableName = "dt_WFLoanAppraisal";
                    foreach (DataColumn col in MyApplication.Tables[tableName].Columns)
                    {
                        columnName = col.ColumnName;
                        if (_hasValue(MyApplication.Tables[tableName].Rows[0][columnName])
                            )
                        {
                            if (!dsWFLoanApp.Tables[tableName].Columns.Contains(columnName))
                            {
                                columnName = (columnName.Equals("InstallmentStartDate") ? "ScheduleStartDate" : columnName);
                                dsWFLoanApp.Tables[tableName].Columns.Add(columnName, col.DataType);
                            }
                            if (!_hasValue(dsWFLoanApp.Tables[tableName].Rows[0][columnName]))
                                dsWFLoanApp.Tables[tableName].Rows[0][columnName] =
                            MyApplication.Tables[tableName].Rows[0][col.ColumnName];
                        }

                    }
                    if (!dsWFLoanApp.t_WFLoanAppraisal.Columns.Contains("CreatedBy"))
                        dsWFLoanApp.t_WFLoanAppraisal.Columns.Add("CreatedBy", typeof(string));

                    dsWFLoanApp.t_WFLoanAppraisal[0]["CreatedBy"] = usrInfo.strUser;

                    OmniCore.AddWFLoanSanction(usrInfo, dsWFLoanApp);
                    Response = "<BRDataSet><dt_WebServiceStatus><WebServiceStatus>00</WebServiceStatus></dt_WebServiceStatus></BRDataSet>";
                    break;

                case WFStageID.DISB:

                    DS_GroupLoanDisbursement dsGroupLoanDisbursement = new DS_GroupLoanDisbursement();
                    dsGroupLoanDisbursement.Merge(MyApplication, true, MissingSchemaAction.Ignore);
                    string TrxBatchID = OmniCore.AddLoanDisbursement(usrInfo, dsGroupLoanDisbursement, null);
                    Response = "<BRDataSet><dt_WebServiceStatus><WebServiceStatus>00</WebServiceStatus></dt_WebServiceStatus><dt_LoanDisbursement><TrxBatchID>" +
                        TrxBatchID +
                        "</TrxBatchID></dt_LoanDisbursement></BRDataSet>";
                    break;

            }


            return Response;

        }







            public static DataSet GetLoanApplicationDetails(UserInfo usrInfo, WFStageID wfStageID, DataSet dsData)
        {
            string OurBranchID = string.Empty;
            string AccountID = string.Empty;
            string ApplicationID = string.Empty;
            string tableName = string.Empty;
            DataSet dsResult = new DataSet();


            switch (wfStageID)
            {
                case WFStageID.APPL:
                    tableName = "dt_WFLoanApplications";
                    break;
                case WFStageID.APPR:
                    tableName = "dt_WFLoanAppraisals";
                    break;
                case WFStageID.SANC:
                    tableName = "dt_WFLoanAppraisal";
                    break;
                case WFStageID.DISB:
                    tableName = "dt_GroupLoanScheme";
                    break;
            }
            if (dsData.Tables[tableName].Columns.Contains("OurBranchID"))
                OurBranchID = dsData.Tables[tableName].Rows[0]["OurBranchID"].ToString();
            if (dsData.Tables[tableName].Columns.Contains("ApplicationID"))
                ApplicationID = dsData.Tables[tableName].Rows[0]["ApplicationID"].ToString();


            switch (wfStageID)
            {
                case WFStageID.APPL:
                    DS_WFLoanApplications dsWFLoanApplications = new DS_WFLoanApplications();
                    OmniCore.GetWFLoanApplication(usrInfo, OurBranchID, ApplicationID, out dsWFLoanApplications);
                    dsResult.Merge(dsWFLoanApplications, true, MissingSchemaAction.Add);
                    break;
                case WFStageID.APPR:
                    DS_WFLoanAppraisals dsWFLoanAppraisals = new DS_WFLoanAppraisals();
                    OmniCore.GetWFLoanAppraisals(usrInfo, OurBranchID, ApplicationID, 0, 0, out dsWFLoanAppraisals);
                    dsResult.Merge(dsWFLoanAppraisals, true, MissingSchemaAction.Add);
                    break;
                case WFStageID.SANC:
                    DS_WFLoanAppraisal dsWFLoanApp = new DS_WFLoanAppraisal();
                    OmniCore.GetWFLoanSanction(usrInfo, OurBranchID, ApplicationID, "0", out dsWFLoanApp);
                    dsWFLoanApp.Tables.Remove(tableName);
                    dsWFLoanApp.Tables["dt_WFLnsanctionBscene"].TableName = tableName;
                    dsResult.Merge(dsWFLoanApp, true, MissingSchemaAction.Add);
                    //stream = new StringReader(dsWFLoanApp.GetXml());
                    break;
                case WFStageID.DISB:
                    DS_Loans dsLoanDisb = null;
                    DataSet _dsData = dsData.Copy();
                    _dsData.Tables[tableName].TableName = "dt_WFLoanApplications";
                    DataSet dsLoanApplications = GetLoanApplicationDetails(usrInfo, WFStageID.APPL, _dsData);
                    if (dsLoanApplications.Tables["dt_WFLoanApplications"].Columns.Contains("AccountID"))
                        AccountID = dsLoanApplications.Tables["dt_WFLoanApplications"].Rows[0]["AccountID"].ToString();
                    if (_hasValue(AccountID))
                    {
                        OmniCore.GetLoanDisb(usrInfo, OurBranchID, AccountID, out dsLoanDisb);
                        DS_GroupLoanDisbursement dsGroupLoanDisbursement = new DS_GroupLoanDisbursement();
                        dsGroupLoanDisbursement.Merge(dsData, true, MissingSchemaAction.Ignore);
                        DR_GroupLoanScheme drGroupLoanDisb = dsGroupLoanDisbursement.t_GroupLoanScheme[0];
                        DR_Loans drLoans = (DR_Loans)dsLoanDisb.t_Loans[0];
                        DR_BHSLoans drBHSLoans = (DR_BHSLoans)dsLoanDisb.t_BHSLoans[0];
                        dsGroupLoanDisbursement.t_GroupLoanScheme[0].OurBranchID = drLoans.OurBranchID;
                        dsGroupLoanDisbursement.t_GroupLoanScheme[0].ApplicationID = ApplicationID;
                        dsGroupLoanDisbursement.t_GroupLoanScheme[0].LoanSeries = drLoans.LoanSeries;
                        dsGroupLoanDisbursement.t_GroupLoanScheme[0].AccountID = AccountID;
                        dsGroupLoanDisbursement.t_GroupLoanScheme[0].ProductID = drLoans.ProductID;

                        double NetDisbAmount = System.Convert.ToDouble(drBHSLoans["DisbursementAmount"].ToString()) -
                           System.Convert.ToDouble(drBHSLoans["Deduction"].ToString());

                        dsGroupLoanDisbursement.t_GroupLoanScheme[0].NetDisbAmount = NetDisbAmount;
                        dsGroupLoanDisbursement.t_GroupLoanScheme[0].LocalAmount = dsGroupLoanDisbursement.t_GroupLoanScheme[0].NetDisbAmount;

                        if (!_hasValue(dsGroupLoanDisbursement.t_GroupLoanScheme[0].CurrencyID))
                            dsGroupLoanDisbursement.t_GroupLoanScheme[0].CurrencyID = drLoans.CurrencyID;
                        if (!_hasValue(dsGroupLoanDisbursement.t_GroupLoanScheme[0].ExchangeRate))
                            dsGroupLoanDisbursement.t_GroupLoanScheme[0].ExchangeRate = 1.0;
                        if (!_hasValue(dsGroupLoanDisbursement.t_GroupLoanScheme[0].TillID))
                            dsGroupLoanDisbursement.t_GroupLoanScheme[0].TillID = "0";
                        if (!_hasValue(dsGroupLoanDisbursement.t_GroupLoanScheme[0].ContraAccountID))
                            dsGroupLoanDisbursement.t_GroupLoanScheme[0].ContraAccountID = drLoans.RepaymentAccountID;
                        if (!_hasValue(dsGroupLoanDisbursement.t_GroupLoanScheme[0].AccountTypeID))
                            dsGroupLoanDisbursement.t_GroupLoanScheme[0].AccountTypeID = "C";
                        if (!_hasValue(dsGroupLoanDisbursement.t_GroupLoanScheme[0].ContraBranchID))
                            dsGroupLoanDisbursement.t_GroupLoanScheme[0].ContraBranchID = OurBranchID;
                        if (!_hasValue(dsGroupLoanDisbursement.t_GroupLoanScheme[0].ChequeID))
                            dsGroupLoanDisbursement.t_GroupLoanScheme[0].ChequeID = "0";
                        if (!_hasValue(dsGroupLoanDisbursement.t_GroupLoanScheme[0].ReferenceNo))
                            dsGroupLoanDisbursement.t_GroupLoanScheme[0].ReferenceNo = "";
                        if (!_hasValue(dsGroupLoanDisbursement.t_GroupLoanScheme[0].BeneficiaryName))
                            dsGroupLoanDisbursement.t_GroupLoanScheme[0].BeneficiaryName = "";
                        if (!_hasValue(dsGroupLoanDisbursement.t_GroupLoanScheme[0].Narration))
                            dsGroupLoanDisbursement.t_GroupLoanScheme[0].Narration = "";
                        if (!_hasValue(dsGroupLoanDisbursement.t_GroupLoanScheme[0].DisbursementModeID))
                        {
                            if (dsGroupLoanDisbursement.t_GroupLoanScheme[0].TillID != string.Empty && dsGroupLoanDisbursement.t_GroupLoanScheme[0].TillID != "0")
                                dsGroupLoanDisbursement.t_GroupLoanScheme[0].DisbursementModeID = "CH";
                            else if (dsGroupLoanDisbursement.t_GroupLoanScheme[0].ContraBranchID != dsGroupLoanDisbursement.t_GroupLoanScheme[0].ContraBranchID)
                                dsGroupLoanDisbursement.t_GroupLoanScheme[0].DisbursementModeID = "IB";
                            else
                                dsGroupLoanDisbursement.t_GroupLoanScheme[0].DisbursementModeID = "TR";
                        }
                        dsGroupLoanDisbursement.t_GroupLoanScheme[0].OperatedBy = usrInfo.strUser;
                        dsResult.Merge(dsGroupLoanDisbursement, true, MissingSchemaAction.Add);

                    }
                    break;

            }

            if (wfStageID != WFStageID.DISB)
            {
                foreach (DataColumn col in dsData.Tables[tableName].Columns)
                {
                    if (_hasValue(dsData.Tables[tableName].Rows[0][col.ColumnName]))
                    {
                        if (!dsResult.Tables[tableName].Columns.Contains(col.ColumnName))
                            dsResult.Tables[tableName].Columns.Add(col.ColumnName, col.DataType);
                        dsResult.Tables[tableName].Rows[0][col.ColumnName] = dsData.Tables[tableName].Rows[0][col.ColumnName];
                    }

                }
            }

            return dsResult;


        }



        /*
        #region Loan On Phone
        public static LoanApplicationResponse LoanOnPhoneRequest(UserInfo usrInfo, LoanApplicationModel loanApplicationRequest)
        {
            string Response = string.Empty;


            LoanApplicationResponse responseObject = null;
            double dblLoanAmount = 0;
            int intLoanTerm = 0;
            BRDataSet dsLoanOnPhone = new BRDataSet();
            DataRow r_LoanOnPhoneRequest = null;
            string strResponse = string.Empty;


            try
            {

                int.TryParse(loanApplicationRequest.LoanTerm, out intLoanTerm);
                double.TryParse(loanApplicationRequest.LoanAmount, out dblLoanAmount);

                if (intLoanTerm <= 0)
                {
                    responseObject = new LoanApplicationResponse()
                    {
                        ResponseCode = "99",
                        ResponseMessage = "Invalid Loan Term"
                    };
                    return responseObject;
                }

                if (dblLoanAmount <= 0)
                {
                    responseObject = new LoanApplicationResponse()
                    {
                        ResponseCode = "99",
                        ResponseMessage = "Invalid Loan Amount"
                    };
                    return responseObject;
                }


                DataTable dtLoanOnPhoneRequest = new DataTable("LoanOnPhoneRequest");
                dtLoanOnPhoneRequest.Columns.Add("OurBranchID", typeof(string));
                dtLoanOnPhoneRequest.Columns.Add("MobileNumber", typeof(string));
                dtLoanOnPhoneRequest.Columns.Add("RepaymentAccountID", typeof(string));
                dtLoanOnPhoneRequest.Columns.Add("LoanProductID", typeof(string));
                dtLoanOnPhoneRequest.Columns.Add("LoanAmount", typeof(double));
                dtLoanOnPhoneRequest.Columns.Add("LoanTerm", typeof(int));
                dtLoanOnPhoneRequest.Columns.Add("OperatorID", typeof(string));
                dtLoanOnPhoneRequest.Columns.Add("UniqueRefID", typeof(string));

                dtLoanOnPhoneRequest.Rows.Add(loanApplicationRequest.OurBranchID.Trim(),
                                                loanApplicationRequest.MobileNumber.Trim(),
                                                loanApplicationRequest.RepaymentAccountID.Trim(),
                                                loanApplicationRequest.LoanProductID.Trim(),
                                                dblLoanAmount,
                                                intLoanTerm,
                                                usrInfo.strUser.ToString(),
                                                loanApplicationRequest.UniqueReferenceID.Trim());

                r_LoanOnPhoneRequest = dtLoanOnPhoneRequest.Rows[0];
                //dsLoanOnPhone.Tables.Add(dtLoanOnPhoneRequest);
                strResponse = OmniCore.LoanOnPhoneApplicationDetails(usrInfo, loanApplicationRequest.RepaymentAccountID.Trim(), loanApplicationRequest.MobileNumber.Trim(), r_LoanOnPhoneRequest);
                if (strResponse != string.Empty)
                {

                    string[] messageCode = strResponse.Split('|');

                    if (messageCode[0] == "00")
                    {

                        responseObject = new LoanApplicationResponse()
                        {

                            ResponseCode = messageCode[0],
                            LoanAccountID = messageCode[1],
                            LoanNumber = messageCode[2],
                            LoanBalance = messageCode[3],
                            MaturityDate = messageCode[4],
                            ResponseMessage = "Loan Application Successful"
                        };
                    }
                    else {
                        responseObject = new LoanApplicationResponse()
                        {

                            ResponseCode = messageCode[0],
                            ResponseMessage = messageCode[1]

                        };

                    }


                }
                           else
                {
                    responseObject = new LoanApplicationResponse()
                    {
                        ResponseCode = "99",
                        ResponseMessage = ResponseErroMessage
                    };
                }

                return responseObject;
            }
            catch (Exception ex)
            {

                responseObject = new LoanApplicationResponse()
                {
                    ResponseCode = "99",
                    ResponseMessage = ex.Message.ToString()
                };

                return responseObject;
            }

        }

        public static LoanBalanceResponse LoanBalanceRequest(UserInfo usrInfo, LoanBalanceRequest loanBalanceRequest)
        {
            string Response = string.Empty;
            BRDataSet dsLoanOnPhone = new BRDataSet();

            LoanBalanceResponse responseObject = null;
            string strResponse = string.Empty;


            try
            {


                //dsLoanOnPhone.Tables.Add(dtLoanOnPhoneRequest);
                dsLoanOnPhone = OmniCore.LoanBalanceDetails(usrInfo, loanBalanceRequest.RepaymentAccountID.Trim(), loanBalanceRequest.MobileNumber.Trim(), loanBalanceRequest.UniqueReferenceID.Trim());
                if (dsLoanOnPhone != null &&
                              dsLoanOnPhone.Tables.Count > 0 &&
                              dsLoanOnPhone.Tables.Contains("dt_LoanBalanceDetail") &&
                              dsLoanOnPhone.Tables["dt_LoanBalanceDetail"].Rows.Count > 0)
                {

                    DataRow r_LoanOnPhone = dsLoanOnPhone.Tables["dt_LoanBalanceDetail"].Rows[0];



                    responseObject = new LoanBalanceResponse()
                    {
                        ResponseCode = "00",

                        MaturityDate = r_LoanOnPhone["MaturityDate"].ToString().Trim(),
                        LoanBalance = r_LoanOnPhone["LoanBalance"].ToString().Trim(),
                        OutstandingPrincipal = r_LoanOnPhone["OutstandingPrincipal"].ToString().Trim(),
                        OutstandingInterest = r_LoanOnPhone["OutstandingInterest"].ToString().Trim(),
                        OutstandingPenalty = r_LoanOnPhone["OutstandingPenalty"].ToString().Trim(),
                        ResponseMessage = "Loan Balance Successful"
                    };
                }
                else
                {
                    responseObject = new LoanBalanceResponse()
                    {
                        ResponseCode = "99",
                        ResponseMessage = ResponseErroMessage
                    };
                }

                return responseObject;
            }
            catch (Exception ex)
            {

                responseObject = new LoanBalanceResponse()
                {
                    ResponseCode = "99",
                    ResponseMessage = ex.Message.ToString()
                };

                return responseObject;
            }

        }


        public static LoanPaymentResponse LoanPaymentDetails(UserInfo usrInfo, LoanPayment loanPayment)
        {
            string Response = string.Empty;
            BRDataSet dsLoanOnPhone = new BRDataSet();

            LoanPaymentResponse responseObject = null;
            string strResponse = string.Empty;
            double dblLoanAmount = 0;
            DataRow r_LoanOPayment = null;

            try
            {



                double.TryParse(loanPayment.Amount, out dblLoanAmount);



                if (dblLoanAmount <= 0)
                {
                    responseObject = new  LoanPaymentResponse()
                    {
                        ResponseCode = "99",
                        ResponseMessage = "Invalid Loan Amount"
                    };
                    return responseObject;
                }


                DataTable dtLoanOnPhonePayment = new DataTable("LoanOnPhonePayment");
                dtLoanOnPhonePayment.Columns.Add("OurBranchID", typeof(string));
                dtLoanOnPhonePayment.Columns.Add("MobileNumber", typeof(string));
                dtLoanOnPhonePayment.Columns.Add("RepaymentAccountID", typeof(string));
                dtLoanOnPhonePayment.Columns.Add("TrxTypeID", typeof(string));
                dtLoanOnPhonePayment.Columns.Add("Amount", typeof(double));
                dtLoanOnPhonePayment.Columns.Add("Remarks", typeof(string));
                dtLoanOnPhonePayment.Columns.Add("LoanProductID", typeof(string));
                dtLoanOnPhonePayment.Columns.Add("OperatorID", typeof(string));
                dtLoanOnPhonePayment.Columns.Add("UniqueRefID", typeof(string));


                dtLoanOnPhonePayment.Rows.Add(loanPayment.OurBranchID.Trim(),
                                                loanPayment.MobileNumber.Trim(),
                                                loanPayment.RepaymentAccountID.Trim(),
                                                "CC",
                                                dblLoanAmount,
                                                "Loan on Phone Payment",
                                                loanPayment.LoanProductID,
                                                usrInfo.strUser.ToString(),
                                                loanPayment.UniqueReferenceID.Trim());

                r_LoanOPayment = dtLoanOnPhonePayment.Rows[0];
                //dsLoanOnPhone.Tables.Add(dtLoanOnPhoneRequest);
                strResponse = OmniCore.LoanOnPhonePaymentDetails(usrInfo, loanPayment.RepaymentAccountID.Trim(), loanPayment.MobileNumber.Trim(), r_LoanOPayment);
                if (strResponse != string.Empty)
                {

                    string[] messageCode = strResponse.Split('|');


                    responseObject = new LoanPaymentResponse()
                    {
                        ResponseCode = messageCode[0],
                        ResponseMessage = messageCode[1],


                    };
                }
                else
                {
                    responseObject = new LoanPaymentResponse()
                    {
                        ResponseCode = "99",
                        ResponseMessage = ResponseErroMessage
                    };
                }


                //dsLoanOnPhone.Tables.Add(dtLoanOnPhoneRequest);


                return responseObject;
            }
            catch (Exception ex)
            {

                responseObject = new LoanPaymentResponse()
                {
                    ResponseCode = "99",
                    ResponseMessage = ex.Message.ToString()
                };

                return responseObject;
            }

        }
        #endregion Loan On Phone

    #endregion Loan Application Process

            #region Loan Schedule
        public static DS_LoanCalulator LoanCalculator(UserInfo usrInfo, string ProductID, double LoanAmount, string RepaymentFrequencyID, int RepaymentTerm, int GracePeriod, string SpecialIntRate)
        {

            DS_ProductLoan dsProductLoan = new DS_ProductLoan();
            BRDataSet dsSuperVision = new BRDataSet();
            DS_LoanCalulator dsLoanCalculator = new DS_LoanCalulator();
            //Get Product Details
            OmniCore.GetProductLoan(usrInfo, ProductID, 0, out dsProductLoan, out dsSuperVision);

            if (dsProductLoan != null &&
                dsProductLoan.t_ProductLoan != null &&
                dsProductLoan.t_ProductLoan.Rows.Count > 0)
            {
                BRDataSet dsLoanRepayment = new BRDataSet();
                DR_ProductLoan drProductLoan = (DR_ProductLoan)dsProductLoan.t_ProductLoan.Rows[0];
                DS_LoanIntCalcRule dsLoanIntCalcRule = new DS_LoanIntCalcRule();
                //Get Loan Product Calc Rule Details
                OmniCore.GetLoanIntCalcRule(usrInfo, drProductLoan.DbIntCalcRuleID, 0, out dsLoanIntCalcRule, out dsSuperVision);
                bool IsAmortizeRule78 = false;
                string CalculationMethodID = string.Empty;
                if (dsLoanIntCalcRule != null &&
                       dsLoanIntCalcRule.t_LoanIntCalcRule != null &&
                       dsLoanIntCalcRule.t_LoanIntCalcRule.Rows.Count > 0)
                {
                    DR_LoanIntCalcRule drLoanIntCalcRule = (DR_LoanIntCalcRule)dsLoanIntCalcRule.t_LoanIntCalcRule[0];
                    IsAmortizeRule78 = drLoanIntCalcRule.IsAmortizeRule78;
                    CalculationMethodID = drLoanIntCalcRule.CalculationMethodID;
                }
                if (SpecialIntRate == null || SpecialIntRate == string.Empty)
                {
                    //Get Loan Product interest slab Details
                    DS_WFLoanApplications dsWFLoanApplications = null;
                    OmniCore.GetProductDetails(usrInfo, usrInfo.strBranch, drProductLoan.WorkflowID, ProductID, string.Empty, out dsWFLoanApplications);
                    if (dsWFLoanApplications != null &&
                        dsWFLoanApplications.t_SlabDetails != null &&
                        dsWFLoanApplications.t_SlabDetails.Rows.Count > 0)
                    {
                        BREntities.WFLoanApplications.DR_SlabDetails drSlabDetails = (BREntities.WFLoanApplications.DR_SlabDetails)dsWFLoanApplications.t_SlabDetails[0];
                        SpecialIntRate = drSlabDetails.EffectiveRate.ToString();
                    }
                }

                string InterestPeriod = string.Empty;
                object[] objParamArray = new object[15];

                if (GracePeriod == null)
                    GracePeriod = 0;
                if (RepaymentTerm == null)
                    RepaymentTerm = 0;
                int LoanTerm = 0;
                RepaymentFrequencyID = RepaymentFrequencyID.ToUpper();
                double multiplier = 1.0;
                switch (drProductLoan.PeriodTypeID)
                {
                    case "W":
                        switch (RepaymentFrequencyID)
                        {
                            case "W": multiplier = 1.0; break;
                            case "F": multiplier = 1.0; break;
                            case "D": multiplier = 1.0 / 7.0; break;
                        }
                        break;
                    case "M":
                        switch (RepaymentFrequencyID)
                        {
                            case "M": multiplier = 1.0; break;
                            case "Q": multiplier = 3.0; break;
                            case "T": multiplier = 4.0; break;
                            case "H": multiplier = 6.0; break;
                            case "B": multiplier = 1.00 / 2.00; break;

                        }
                        break;
                }
                LoanTerm = (int)Math.Ceiling(RepaymentTerm * multiplier);
                objParamArray[0] = LoanAmount;
                objParamArray[1] = SpecialIntRate;
                objParamArray[2] = CalculationMethodID;
                objParamArray[3] = RepaymentFrequencyID;
                objParamArray[4] = LoanTerm;
                objParamArray[5] = RepaymentTerm;
                objParamArray[6] = drProductLoan.DbRoundingID;
                objParamArray[7] = IsAmortizeRule78;
                objParamArray[8] = InterestPeriod;
                objParamArray[9] = drProductLoan.PeriodTypeID;
                objParamArray[10] = drProductLoan.DbRoundingID;
                objParamArray[11] = GracePeriod;
                objParamArray[12] = drProductLoan.IsTermExcludesGracePeriod;
                objParamArray[13] = drProductLoan.IsIntCollectedInGracePeriod;
                objParamArray[14] = DateTime.Today;

                OmniCore.GetLoanCalcIbank(usrInfo, out dsLoanCalculator, objParamArray);
            }
            return dsLoanCalculator;
        }
        #endregion Loan Schedule

        #region Loan ReSchedule
        public static string LoanReschedule(UserInfo usrInfo, string OurBranchID, string AccountID,
               string RescheduleStartDate, string ProposedInterestRate,
               string TermExtendReduceID, string ExtRedTerm)
        {

            DS_LoanInitiationDetail dsLoanInitiationDetail = new DS_LoanInitiationDetail();
            DS_LoanPreClosureDetail dsLoanRescheduleTrxPost = new DS_LoanPreClosureDetail();
            DS_Transactions dsTransactions = new DS_Transactions();
            DS_CurrencyDenominations dsCurrDenomination = null;
            string LoanSeries = string.Empty;
            string ModuleID = "4400";
            string EventID = "NONE";
            string Status = string.Empty;

            OmniCore.GetLoanInitiationDetail(usrInfo, OurBranchID, AccountID, LoanSeries, ModuleID, EventID, out dsLoanInitiationDetail);
            if (dsLoanInitiationDetail != null &&
                dsLoanInitiationDetail.t_LoanInitiationDetail != null &&
                dsLoanInitiationDetail.t_LoanInitiationDetail.Rows.Count > 0)
            {

                dsLoanInitiationDetail.t_LoanInitiationDetail[0].ModuleID = ModuleID;
                dsLoanInitiationDetail.t_LoanInitiationDetail[0].RescheduleStatusID = "AR";
                dsLoanInitiationDetail.t_LoanInitiationDetail[0].CreatedBy = usrInfo.strUser;
                if (RescheduleStartDate != null && RescheduleStartDate != string.Empty)
                    dsLoanInitiationDetail.t_LoanInitiationDetail[0].RescheduleStartDate = System.Convert.ToDateTime(RescheduleStartDate);
                if (ProposedInterestRate != null)
                    dsLoanInitiationDetail.t_LoanInitiationDetail[0].ProposedInterestRate = System.Convert.ToDouble(ProposedInterestRate);
                if (CoreOmniApiBase._hasValue(TermExtendReduceID) &&
                    (TermExtendReduceID.ToUpper().Trim().Equals("EXT") || TermExtendReduceID.ToUpper().Trim().Equals("RED")) &&
                    ExtRedTerm != null)
                {
                    dsLoanInitiationDetail.t_LoanInitiationDetail[0].TermExtendReduceID = TermExtendReduceID;
                    dsLoanInitiationDetail.t_LoanInitiationDetail[0].ExtRedTerm = System.Convert.ToInt16(ExtRedTerm);
                }

                OmniCore.AddLoanInitiationDetail(usrInfo, dsLoanInitiationDetail);
                DR_LoanInitiationDetail drLoanInitiationDetail = (DR_LoanInitiationDetail)dsLoanInitiationDetail.t_LoanInitiationDetail.Rows[0];

                DR_BHSLoanPreClosureDetail drBHSLoanPreClosureDetail = dsLoanRescheduleTrxPost.t_BHSLoanPreClosureDetail.NewLoansRow();
                foreach (DataColumn col in dsLoanInitiationDetail.t_LoanInitiationDetail.Columns)
                {
                    if (dsLoanRescheduleTrxPost.t_BHSLoanPreClosureDetail.Columns.Contains(col.ColumnName))
                    {
                        drBHSLoanPreClosureDetail[col.ColumnName] = drLoanInitiationDetail[col.ColumnName];

                    }
                }
                dsLoanRescheduleTrxPost.t_BHSLoanPreClosureDetail.Rows.Add(drBHSLoanPreClosureDetail);
                OmniCore.AddLoanRescheduleTrxPosting(usrInfo, dsLoanRescheduleTrxPost, dsTransactions, dsCurrDenomination);
                Status = "00";
            }
            StringDictionary myOutPuts = new StringDictionary();
            myOutPuts.Add("Status", (Status == string.Empty ? "99" : Status));
            return FormatWebMethodResponse(myOutPuts);
        }
        #endregion Loan ReSchedule

        #region LoanTopUp
        public static string LoanTopUp(UserInfo usrInfo, string OurBranchID, string LoanAccountID, string AdditionalAmount,
               string InterestRate, string ProposedTerm, string InstallmentStartDate)
        {
            string strModuleID = "4390";
            string EventID = "NONE";
            string LoanSeries = string.Empty;
            DS_LoanInitiationDetail dsLoanInitiationDetail = null;
            DS_LoanInitiationDetail _dsLoanInitiationDetail = null;
            OmniCore.GetLoanInitiationDetail(usrInfo, OurBranchID, LoanAccountID, LoanSeries, strModuleID, EventID, out  dsLoanInitiationDetail);
            if ((dsLoanInitiationDetail != null &&
                  dsLoanInitiationDetail.Tables.Count > 0 &&
                  dsLoanInitiationDetail.t_LoanInitiationDetail != null))
            {

                EventID = "ADD";
                OmniCore.GetLoanInitiationDetail(usrInfo, OurBranchID, LoanAccountID, LoanSeries, strModuleID, EventID, out  _dsLoanInitiationDetail);
                if ((_dsLoanInitiationDetail != null &&
                 _dsLoanInitiationDetail.Tables.Count > 0 &&
                 _dsLoanInitiationDetail.t_LoanInitiationGridDetails != null))
                {

                    if (dsLoanInitiationDetail.t_LoanInitiationGridDetails != null)
                        dsLoanInitiationDetail.t_LoanInitiationGridDetails.Clear();
                    dsLoanInitiationDetail.Merge(_dsLoanInitiationDetail, true, MissingSchemaAction.Ignore);
                    Double TotalActualAmount = 0;
                    DR_LoanInitiationGridDetails drLoanInitiationGridDetails = null;
                    for (var k = 0; k < dsLoanInitiationDetail.t_LoanInitiationGridDetails.Rows.Count; k++)
                    {
                        drLoanInitiationGridDetails = (DR_LoanInitiationGridDetails)dsLoanInitiationDetail.t_LoanInitiationGridDetails.Rows[k];

                        if (drLoanInitiationGridDetails.ActualAmount != 0 && drLoanInitiationGridDetails.ComponentID != "ADVINST_RCD")
                            TotalActualAmount = TotalActualAmount + System.Convert.ToDouble(drLoanInitiationGridDetails.ActualAmount);
                        else
                            TotalActualAmount = TotalActualAmount - System.Convert.ToDouble(drLoanInitiationGridDetails.ActualAmount);

                    }
                    Double _AdditionalAmount = 0;
                    TotalActualAmount = TotalActualAmount + (CoreOmniApiBase._hasValue(AdditionalAmount) && Double.TryParse(AdditionalAmount, out _AdditionalAmount) ? _AdditionalAmount : 0);
                    dsLoanInitiationDetail.t_LoanInitiationDetail[0].UpdateCount = 1;
                    dsLoanInitiationDetail.t_LoanInitiationDetail[0].RefinanceStatusID = "FP";
                    dsLoanInitiationDetail.t_LoanInitiationDetail[0].AdditionalLoanAmount = _AdditionalAmount;
                    dsLoanInitiationDetail.t_LoanInitiationDetail[0].NewLoanAmount = TotalActualAmount;
                    DS_SystemBranchStatus dsSystemBranchStatus = BRDALSystemModules.GetSystemBranchStatus(usrInfo, OurBranchID);
                    dsLoanInitiationDetail.t_LoanInitiationDetail[0].DisburementDate = dsSystemBranchStatus.t_SystemBranchStatus[0].SODDate;
                    dsLoanInitiationDetail.t_LoanInitiationDetail[0].CreatedBy = usrInfo.strUser;
                    if (!CoreOmniApiBase._hasValue(dsLoanInitiationDetail.t_LoanInitiationDetail[0].LoanPurposeID))
                        dsLoanInitiationDetail.t_LoanInitiationDetail[0].LoanPurposeID = "O";
                    DR_LoanInitiationBHS drLoanInitiationBHS = (DR_LoanInitiationBHS)dsLoanInitiationDetail.t_LoanInitiationBHS[0];

                    DateTime _InstallmentStartDate;
                    string[] dateFormats = new string[] { "d/M/yyyy", "dd/MM/yyyy" };
                    if (DateTime.TryParseExact(InstallmentStartDate, dateFormats, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out _InstallmentStartDate))
                        dsLoanInitiationDetail.t_LoanInitiationDetail[0].InstallmentStartDate = _InstallmentStartDate;

                    double ProposedInterestRate = 0;
                    if (!Double.TryParse(InterestRate, out ProposedInterestRate))
                        ProposedInterestRate = drLoanInitiationBHS.InterestRate;
                    dsLoanInitiationDetail.t_LoanInitiationDetail[0].ProposedInterestRate = ProposedInterestRate;

                    Int16 _ProposedTerm = 0;
                    if (!Int16.TryParse(ProposedTerm, out _ProposedTerm))
                        _ProposedTerm = drLoanInitiationBHS.BalanceTerm;
                    dsLoanInitiationDetail.t_LoanInitiationDetail[0].ProposedTerm = _ProposedTerm;
                    string ApplicationID = OmniCore.AddLoanInitiationDetail(usrInfo, dsLoanInitiationDetail);
                    if (ApplicationID != string.Empty)
                    {

                        //Loan Sanction
                        DS_WFLoanAppraisal dsWFLoanSanction = null;
                        OmniCore.GetWFLoanSanction(usrInfo, OurBranchID, ApplicationID, "0", out dsWFLoanSanction);
                        foreach (DataColumn col in dsWFLoanSanction.t_WFLnsanctionBscene.Columns)
                        {
                            if (dsWFLoanSanction.t_WFLoanAppraisal.Columns.Contains(col.ColumnName))
                                dsWFLoanSanction.t_WFLoanAppraisal[0][col.ColumnName] = dsWFLoanSanction.t_WFLnsanctionBscene[0][col.ColumnName];

                        }
                        dsWFLoanSanction.t_WFLoanAppraisal[0].ApprovedBy = usrInfo.strUser;
                        if (!dsWFLoanSanction.t_WFLoanAppraisal.Columns.Contains("CreatedBy"))
                            dsWFLoanSanction.t_WFLoanAppraisal.Columns.Add("CreatedBy", typeof(string));
                        dsWFLoanSanction.t_WFLoanAppraisal[0]["CreatedBy"] = usrInfo.strUser;

                        if (!dsWFLoanSanction.t_WFLoanAppraisal.Columns.Contains("ScheduleStartDate"))
                            dsWFLoanSanction.t_WFLoanAppraisal.Columns.Add("ScheduleStartDate", typeof(DateTime));
                        dsWFLoanSanction.t_WFLoanAppraisal[0]["ScheduleStartDate"] = _InstallmentStartDate;

                        if (!dsWFLoanSanction.t_WFLoanAppraisal.Columns.Contains("NoOfDisbursements"))
                            dsWFLoanSanction.t_WFLoanAppraisal.Columns.Add("NoOfDisbursements", typeof(int));
                        dsWFLoanSanction.t_WFLoanAppraisal[0]["NoOfDisbursements"] = 1;
                        dsWFLoanSanction.t_WFLoanAppraisal[0].ApprovedDate = dsSystemBranchStatus.t_SystemBranchStatus[0].SODDate;
                        OmniCore.AddWFLoanSanction(usrInfo, dsWFLoanSanction);
                        //LoanDisbursement
                        DS_Loans dsLoanDisb = null;
                        OmniCore.GetLoanDisb(usrInfo, OurBranchID, LoanAccountID, out dsLoanDisb);
                        DS_GroupLoanDisbursement dsGroupLoanDisbursement = new DS_GroupLoanDisbursement();
                        DR_GroupLoanScheme drGroupLoanDisb = dsGroupLoanDisbursement.t_GroupLoanScheme.NewGroupLoanSchemeRow();
                        DR_Loans drLoans = (DR_Loans)dsLoanDisb.t_Loans[0];
                        DR_BHSLoans drBHSLoans = (DR_BHSLoans)dsLoanDisb.t_BHSLoans[0];
                        drGroupLoanDisb.OurBranchID = drLoans.OurBranchID;
                        double NetDisbAmount = System.Convert.ToDouble(drBHSLoans["DisbursementAmount"].ToString()) -
                           System.Convert.ToDouble(drBHSLoans["Deduction"].ToString());
                        drGroupLoanDisb.NetDisbAmount = NetDisbAmount;
                        drGroupLoanDisb.CurrencyID = drLoans.CurrencyID;
                        drGroupLoanDisb.ExchangeRate = 1.0;
                        drGroupLoanDisb.LocalAmount = drGroupLoanDisb.NetDisbAmount;
                        drGroupLoanDisb.ApplicationID = ApplicationID;
                        drGroupLoanDisb.LoanSeries = drLoans.LoanSeries;
                        drGroupLoanDisb.TillID = "0";
                        drGroupLoanDisb.ContraAccountID = drLoans.RepaymentAccountID;
                        drGroupLoanDisb.AccountTypeID = "C";
                        drGroupLoanDisb.AccountID = LoanAccountID;
                        drGroupLoanDisb.ContraBranchID = OurBranchID;
                        drGroupLoanDisb.ProductID = drLoans.ProductID;
                        drGroupLoanDisb.ChequeID = "0";
                        drGroupLoanDisb.ReferenceNo = "";
                        drGroupLoanDisb.BeneficiaryName = "";
                        drGroupLoanDisb.Narration = "";
                        drGroupLoanDisb.DisbursementModeID = "TR";
                        drGroupLoanDisb.OperatedBy = usrInfo.strUser;
                        dsGroupLoanDisbursement.t_GroupLoanScheme.Rows.Add(drGroupLoanDisb);
                        OmniCore.AddLoanDisbursement(usrInfo, dsGroupLoanDisbursement, null);

                    }

                }

            }
            return dsLoanInitiationDetail.GetXml().ToString().Replace(" xmlns=\"BRBase.BRDataSet\"", "");
        }

        #endregion LoanTopUp

        #region Loan Payoff
        public static string LoanPayoff(UserInfo usrInfo, string OurBranchID, string LoanAccountID, string DebitAccountID)
        {
            string strSerialID = string.Empty;
            decimal PayOffAmount = 0;
            string TrxDate = string.Empty;
            DS_Transactions dsTransactions = new DS_Transactions();
            DS_CurrencyDenominations dsDenomination = new DS_CurrencyDenominations();
            DS_LoanPreClosureDetail dsLoanPayoff = new DS_LoanPreClosureDetail();
            OmniCore.GetLoanPreClosureDetail(usrInfo, OurBranchID, LoanAccountID, out dsLoanPayoff);

            DS_SystemBranchStatus dsSystemBranchStatus = BRDALSystemModules.GetSystemBranchStatus(usrInfo, OurBranchID);
            TrxDate = BRBaseConvert.ConvertToShortDate(System.Convert.ToDateTime(dsSystemBranchStatus.t_SystemBranchStatus[0].SODDate));
            DR_BHSLoanPreClosureDetail drBHSLoanPreClosureDetail = (DR_BHSLoanPreClosureDetail)dsLoanPayoff.t_BHSLoanPreClosureDetail.Rows[0];

            foreach (DR_LoanPreClosureDetail drLoanPreClosureDetail in dsLoanPayoff.t_LoanPreClosureDetail.Rows)
            {
                if (!drLoanPreClosureDetail.ComponentID.ToUpper().Equals("INT_REBATE"))
                    PayOffAmount += drLoanPreClosureDetail.ActualAmount;
            }

            OmniCore.GetPreClosureTrxDetails(usrInfo, OurBranchID, LoanAccountID, drBHSLoanPreClosureDetail.LoanSeries,
                TrxDate, dsTransactions, dsDenomination, System.Convert.ToDouble(PayOffAmount), out dsTransactions);

            Dictionary<string, Dictionary<string, string>> lst = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, string> td = new Dictionary<string, string>();
            td.Add("TrxTypeID", "TC");
            td.Add("AccountID", LoanAccountID);
            td.Add("TrxDescriptionID", "007");
            lst.Add("TC", td);

            td = new Dictionary<string, string>();
            td.Add("TrxTypeID", "TD");
            td.Add("AccountID", DebitAccountID);
            td.Add("TrxDescriptionID", "008");
            lst.Add("TD", td);

            string TrxTypeID = string.Empty;
            string AccountID = string.Empty;
            string TrxDescriptionID = string.Empty;
            foreach (KeyValuePair<string, Dictionary<string, string>> kv in lst)
            {
                TrxTypeID = string.Empty;
                AccountID = string.Empty;
                TrxDescriptionID = string.Empty;
                DR_Transactions drTransactions = dsTransactions.t_Transactions.NewTransactionsRow();
                drTransactions.TrxBranchID = OurBranchID;
                drTransactions.OurBranchID = OurBranchID;
                kv.Value.TryGetValue("TrxTypeID", out  TrxTypeID);
                drTransactions.TrxTypeID = TrxTypeID;
                drTransactions.AccountTypeID = "C";
                kv.Value.TryGetValue("AccountID", out  AccountID);
                drTransactions.AccountID = AccountID;
                kv.Value.TryGetValue("TrxDescriptionID", out  TrxDescriptionID);
                drTransactions.TrxDescriptionID = TrxDescriptionID;
                drTransactions.Amount = PayOffAmount;
                drTransactions.LocalAmount = PayOffAmount;
                drTransactions.TrxAmount = PayOffAmount;
                drTransactions.TrxCurrencyID = drBHSLoanPreClosureDetail.CurrencyID;
                drTransactions.ExchangeRate = 1;
                drTransactions.MeanRate = 1.0;
                drTransactions.Profit = 0;
                dsTransactions.t_Transactions.AddTransactionsRow(drTransactions);
            }
            dsLoanPayoff.t_BHSLoanPreClosureDetail[0].CreatedBy = usrInfo.strUser;
            strSerialID = OmniCore.AddLoanPreClosurePost(usrInfo, dsTransactions, dsLoanPayoff);
            StringDictionary myOutPuts = new StringDictionary();
            myOutPuts.Add("SerialID", strSerialID);
            return FormatWebMethodResponse(myOutPuts);
        }
        #endregion Loan Payoff


    public static bool InternalTransferiBanking(UserInfo usrInfo, string OurBranchID, string FromAccountID, string ToBranchID, string ToAccountID, string TrxAmount, string crTransactionDescription, string drTransactionDescription, string TrxDescriptionID, string TrxTraceNo, out BRDataSet ds_InternalTransfer)//, string ChargeID
        {

            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
            ds_InternalTransfer = new BRDataSet();
            string sp = "p_ChannelInternalTransferIB";

            try
            {

                using (IDbConnection connection = BR.DBClient.DBClient.GetConnection(usrInfo))
                {
                    using (IDbTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            str2 = (string)dBHelper.ExecuteScalarTypedParams(transaction, "p_AddEditAccountCustomersMobile", dataRow);
                            info.Message = str2;
                            transaction.Commit();
                        }
                        catch (Exception exception1)
                        {
                            exception = exception1;
                            transaction.Rollback();
                            str2 = "Database Error";
                            info.Status = BRStatus.Failed;
                            info.Message = exception.Message;
                            throw DBClientUtils.GetDBErrorMessages(exception, usrInfo.strUser, usrInfo.strSystem);
                        }
                    }
                }
                str3 = str2;
                /*
                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    IDataParameter[] arParms = intfDBHelper.GetSPParameters(connection, sp);
                    object paramValue = null;
                    foreach (IDataParameter param in arParms)
                    {
                        paramValue = null;
                        switch (param.ParameterName.Substring(1).ToUpper())
                        {


                            case "OPERATORID":
                                paramValue = usrInfo.strUser.ToString();
                                break;
                            case "OURBRANCHID":
                                paramValue = OurBranchID;
                                break;
                            case "FROMACCOUNTID":
                                paramValue = FromAccountID;
                                break;
                            case "TOBRANCHID":
                                paramValue = ToBranchID;
                                break;
                            case "TOACCOUNTID":
                                paramValue = ToAccountID;
                                break;
                            case "TRXAMOUNT":
                                paramValue = TrxAmount;
                                break;
                            case "CRTRANSACTIONDESCRIPTION":
                                paramValue = crTransactionDescription;
                                break;
                            case "DRTRANSACTIONDESCRIPTION":
                                paramValue = drTransactionDescription;
                                break;
                            case "TRXDESCRIPTIONID":
                                paramValue = TrxDescriptionID;
                                break;
                            case "TRXTRACENO":
                                paramValue = TrxTraceNo;
                                break;
                        }
                        param.Value = paramValue;


                    }

                    intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, sp, ds_InternalTransfer, new string[] { "status", "InternalTransfer" }, arParms);
                }
 
                if (ds_InternalTransfer.Tables[0].Rows.Count == 0)
                    return false;
                return true;
            }
            catch (Exception ex)
            {
                throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
            }
            finally
            {
                ds_InternalTransfer.Dispose();
            }

        }

       static public string AddStandingInstructionIbank(UserInfo usrInfo, DS_StandingInstructions dsStandingInstruction)
        {
            return AddStandingInstructionIbank(usrInfo, dsStandingInstruction, false);
        }
        static public string AddStandingInstructionIbank(UserInfo usrInfo, DS_StandingInstructions dsStandingInstruction, bool SkipAudit)
        {
            AuditInfo auditInfo = new AuditInfo();
            auditInfo.EventID = BROperation.Add;
            auditInfo.ModuleID = BRModule.SITransfer;
            auditInfo.Status = BRStatus.Successful;

            string strStandingInstructionId = String.Empty;
            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
            dsStandingInstruction.t_StandingInstructions.Columns.Add("NewRecord", typeof(short));
            DR_StandingInstructions drStandingInstructions = dsStandingInstruction.t_StandingInstructions[0];
            drStandingInstructions["NewRecord"] = 1;
            try
            {
                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    using (IDbTransaction trans = connection.BeginTransaction())
                    {
                        strStandingInstructionId = (string)intfDBHelper.ExecuteScalarTypedParams(trans, "p_AddEditSITransferIbank", drStandingInstructions);
                        if (!SkipAudit)
                            PassAuditInformation(trans, usrInfo, BRUtilities.GetDataSetXML(dsStandingInstruction), string.Empty, auditInfo);
                        trans.Commit();
                    }
                }
            }
            catch (Exception ex)
            {

                auditInfo.Status = BRStatus.Failed;
                auditInfo.Message = ex.Message;
                PassAuditInformation(null, usrInfo, BRUtilities.GetDataSetXML(dsStandingInstruction), string.Empty, auditInfo);
                throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
            }
            return strStandingInstructionId;
        }
        static public string StopResumeSITranfer(UserInfo usrInfo, DS_StandingInstructions dsStandingInstruction, short ModuleID)
        {
            return StopResumeSITranfer(usrInfo, dsStandingInstruction, ModuleID, false);
        }
        static public string StopResumeSITranfer(UserInfo usrInfo, DS_StandingInstructions dsStandingInstruction, short ModuleID, bool SkipAudit)
        {

            AuditInfo auditInfo = new AuditInfo();
            auditInfo.EventID = BROperation.Add;
            auditInfo.ModuleID = BRModule.SITransfer;
            auditInfo.Status = BRStatus.Successful;
            string strRefNo = string.Empty;
            try
            {
                IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
                DR_StandingInstructions drStandingInstructions = dsStandingInstruction.t_StandingInstructions[0];
                if (!dsStandingInstruction.t_StandingInstructions.Columns.Contains("ModuleID"))
                {
                    dsStandingInstruction.t_StandingInstructions.Columns.Add("ModuleID", typeof(short));
                    drStandingInstructions["ModuleID"] = ModuleID;
                }
                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    using (IDbTransaction trans = connection.BeginTransaction())
                    {
                        strRefNo = intfDBHelper.ExecuteScalarTypedParams(trans, "p_StopResumeSITransfer", drStandingInstructions).ToString();
                        if (!SkipAudit)
                            PassAuditInformation(trans, usrInfo, BRUtilities.GetDataSetXML(dsStandingInstruction), string.Empty, auditInfo);
                        trans.Commit();
                    }
                }
            }
            catch (Exception ex)
            {

                auditInfo.Status = BRStatus.Failed;
                auditInfo.Message = ex.Message;
                PassAuditInformation(null, usrInfo, BRUtilities.GetDataSetXML(dsStandingInstruction), string.Empty, auditInfo);
                throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
            }
            return strRefNo;
        }
        #endregion Transactions methods

    private static byte[] CompressData(object obj)
        {
            byte[] inb = SerializeXmlBinary(obj);
            byte[] outb;
            using (MemoryStream ostream = new MemoryStream())
            {
                using (DeflateStream df = new DeflateStream(ostream, System.IO.Compression.CompressionMode.Compress, true))
                {
                    df.Write(inb, 0, inb.Length);
                }
                outb = ostream.ToArray();
            }
            return outb;
        }

        public static byte[] SerializeXmlBinary(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlDictionaryWriter wtr = XmlDictionaryWriter.CreateBinaryWriter(ms))
                {
                    NetDataContractSerializer serializer; serializer = new NetDataContractSerializer();
                    serializer.WriteObject(wtr, obj);
                    ms.Flush();
                }
                return ms.ToArray();
            }
        }

        public static object DeSerializeXmlBinary(byte[] bytes)
        {
            using (XmlDictionaryReader rdr = XmlDictionaryReader.CreateBinaryReader(bytes, XmlDictionaryReaderQuotas.Max))
            {
                NetDataContractSerializer serializer; serializer = new NetDataContractSerializer();
                serializer.AssemblyFormat = FormatterAssemblyStyle.Simple;
                return serializer.ReadObject(rdr);
            }
        }

    
        #region charges methods
        static public bool GetLoanIntCalcRule(UserInfo usrInfo, string strRuleID, short sDirection, out DS_LoanIntCalcRule dsLoanIntCalcRule, out BRDataSet dsSupervision)
        {

            AuditInfo auditInfo = new AuditInfo();
            auditInfo.ModuleID = BRModule.LoanCalculationRule;
            auditInfo.EventID = BROperation.View;
            auditInfo.Status = BRStatus.Successful;
            dsLoanIntCalcRule = new DS_LoanIntCalcRule();
            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
            IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(5);
            arParms[0] = intfDBHelper.CreateNewDBParam("BankID", SqlDbType.NVarChar, 6);
            arParms[0].Value = usrInfo.strBank;
            arParms[1] = intfDBHelper.CreateNewDBParam("OurBranchID", SqlDbType.NVarChar, 6);
            arParms[1].Value = usrInfo.strBranch;
            arParms[2] = intfDBHelper.CreateNewDBParam("IntCalcRuleID", SqlDbType.NVarChar, 10);
            arParms[2].Value = strRuleID;
            arParms[3] = intfDBHelper.CreateNewDBParam("OperatorID", SqlDbType.NVarChar, 25);
            arParms[3].Value = usrInfo.strUser;
            arParms[4] = intfDBHelper.CreateNewDBParam("Direction", SqlDbType.SmallInt);
            arParms[4].Value = sDirection;
            try
            {
                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetLoanIntCalcRule", dsLoanIntCalcRule, new string[] { BRUtilities.strSupTableName, dsLoanIntCalcRule.t_LoanIntCalcRule.TableName }, arParms);

                }
                dsSupervision = BRUtilities.GetSupervisionRecord(dsLoanIntCalcRule);
            }
            catch (Exception ex)
            {
                auditInfo.Status = BRStatus.Failed;
                auditInfo.Message = ex.Message;
                PassAuditInformation(null, usrInfo, strRuleID, string.Empty, auditInfo);
                throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
            }
            if (dsLoanIntCalcRule.t_LoanIntCalcRule.Rows.Count == 0 && dsSupervision.Tables[BRUtilities.strSupTableName].Rows.Count == 0)
                return false;
            return true;
        }
        #endregion charges methods

    
        #region utilities
        static public bool GetLoanCalcIbank(UserInfo usrInfo, out DS_LoanCalulator dsLoanCalculator, params object[] objParamArray)
        {
            dsLoanCalculator = new DS_LoanCalulator();
            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
            try
            {
                IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(15);
                arParms[0] = intfDBHelper.CreateNewDBParam("Amount", SqlDbType.VarChar, 15);
                arParms[0].Value = objParamArray[0];
                arParms[1] = intfDBHelper.CreateNewDBParam("InterestRate", SqlDbType.NVarChar, 20);
                arParms[1].Value = objParamArray[1];
                arParms[2] = intfDBHelper.CreateNewDBParam("CalculationMethodID", SqlDbType.NVarChar, 25);
                arParms[2].Value = objParamArray[2];
                arParms[3] = intfDBHelper.CreateNewDBParam("PeriodTypeID", SqlDbType.NVarChar, 25);
                arParms[3].Value = objParamArray[9];
                arParms[4] = intfDBHelper.CreateNewDBParam("FrequencyID", SqlDbType.NVarChar, 25);
                arParms[4].Value = objParamArray[3];
                arParms[5] = intfDBHelper.CreateNewDBParam("Term", SqlDbType.SmallInt);
                arParms[5].Value = objParamArray[4];
                arParms[6] = intfDBHelper.CreateNewDBParam("NoOfInstallments", SqlDbType.SmallInt);
                arParms[6].Value = objParamArray[5];
                arParms[7] = intfDBHelper.CreateNewDBParam("CalculationPeriod", SqlDbType.NVarChar, 5);
                arParms[7].Value = objParamArray[8];
                arParms[8] = intfDBHelper.CreateNewDBParam("RoundingID", SqlDbType.NVarChar, 25);
                arParms[8].Value = objParamArray[6];
                arParms[9] = intfDBHelper.CreateNewDBParam("IntRoundingID", SqlDbType.NVarChar, 25);
                arParms[9].Value = objParamArray[10];
                arParms[10] = intfDBHelper.CreateNewDBParam("LoanDisbDate", SqlDbType.DateTime);
                arParms[10].Value = objParamArray[14];
                arParms[11] = intfDBHelper.CreateNewDBParam("Rule78", SqlDbType.Bit);
                arParms[11].Value = objParamArray[7];
                arParms[12] = intfDBHelper.CreateNewDBParam("GracePeriod", SqlDbType.SmallInt);
                arParms[12].Value = objParamArray[11];
                arParms[13] = intfDBHelper.CreateNewDBParam("TermExcludesGracePeriod", SqlDbType.Bit);
                arParms[13].Value = objParamArray[12];
                arParms[14] = intfDBHelper.CreateNewDBParam("CollectInterestDuringGracePeriod", SqlDbType.Bit);
                arParms[14].Value = objParamArray[13];

                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "P_GetLoanInstalmentIbank", dsLoanCalculator, new string[] { "LoanCalulator", "LoanCalulatorGrid", "status" }, arParms);
                }
            }
            catch (Exception ex)
            {
                throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
            }

            if (dsLoanCalculator.t_LoanCalulator.Rows.Count == 0 && dsLoanCalculator.t_LoanCalulatorGrid.Rows.Count == 0)
                return false;
            return true;
        }
        #endregion utilities

        #region product methods
        static public bool GetProductLoan(UserInfo usrInfo, string ProductID, short Direction, out DS_ProductLoan dsProductLoan, out BRDataSet dsSuperVision)
        {
            AuditInfo auditInfo = new AuditInfo();
            auditInfo.EventID = BROperation.View;
            auditInfo.ModuleID = BRModule.ProductLoan;
            auditInfo.Status = BRStatus.Successful;

            dsProductLoan = new DS_ProductLoan();
            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
            IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(5);

            arParms[0] = intfDBHelper.CreateNewDBParam("BankID", SqlDbType.NVarChar, 6);
            arParms[0].Value = usrInfo.strBank;
            arParms[1] = intfDBHelper.CreateNewDBParam("OurBranchID", SqlDbType.NVarChar, 6);
            arParms[1].Value = usrInfo.strBranch;
            arParms[2] = intfDBHelper.CreateNewDBParam("ProductID", SqlDbType.NVarChar, 6);
            arParms[2].Value = ProductID;
            arParms[3] = intfDBHelper.CreateNewDBParam("OperatorID", SqlDbType.NVarChar, 25);
            arParms[3].Value = usrInfo.strUser;
            arParms[4] = intfDBHelper.CreateNewDBParam("Direction", SqlDbType.SmallInt);
            arParms[4].Value = Direction;
            try
            {
                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetProductLoan", dsProductLoan, new string[] { BRUtilities.strSupTableName, "ProductLoan", "ProductCustRestriction" }, arParms);
                    dsSuperVision = BRUtilities.GetSupervisionRecord(dsProductLoan);
                }
            }
            catch (Exception ex)
            {
                auditInfo.Status = BRStatus.Failed;
                auditInfo.Message = ex.Message;
                PassAuditInformation(null, usrInfo, ProductID, string.Empty, auditInfo);
                throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
            }
            if (dsProductLoan.t_ProductLoan.Rows.Count == 0 && dsSuperVision.Tables[BRUtilities.strSupTableName].Rows.Count == 0)
                return false;
            return true;

        }
        static public bool GetProductDetails(UserInfo usrInfo, string OurBranchID, string WFAdvTypeID, string ProductID, string SpecialIntRate, out DS_WFLoanApplications dsWFLoanApplications)
        {

            AuditInfo auditInfo = new AuditInfo();
            auditInfo.ModuleID = BRModule.WFLoanApplications;
            auditInfo.Status = BRStatus.Successful;
            dsWFLoanApplications = new DS_WFLoanApplications();
            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);

            string sp = "p_GetWFProductDetails";

            try
            {
                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    IDataParameter[] arParms = intfDBHelper.GetSPParameters(connection, sp);
                    object paramValue = null;
                    foreach (IDataParameter param in arParms)
                    {
                        paramValue = null;
                        switch (param.ParameterName.Substring(1).ToUpper())
                        {
                            case "OURBRANCHID":
                                paramValue = OurBranchID;
                                break;
                            case "WFADVTYPEID":
                                paramValue = WFAdvTypeID;
                                break;
                            case "PRODUCTID":
                                paramValue = ProductID;
                                break;
                            case "OPERATORID":
                                paramValue = usrInfo.strUser;
                                break;
                            case "SPECIALINTRATE":
                                paramValue = SpecialIntRate;
                                break;

                        }
                        param.Value = paramValue;


                    }

                    intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, sp, dsWFLoanApplications, new string[] { "WFLoanApplications", "SlabDetails" }, arParms);
                }
            }
            catch (Exception ex)
            {
                auditInfo.Status = BRStatus.Failed;
                auditInfo.Message = ex.Message;
                PassAuditInformation(null, usrInfo, ProductID, string.Empty, auditInfo);
                throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
            }
            if (dsWFLoanApplications.t_WFLoanApplications.Rows.Count == 0)
                return false;
            return true;
        }
        #endregion product methods


        #region product methods
        public static bool GetProductList(UserInfo usrInfo, out BRDataSet ds_ProductList)
        {

            AuditInfo auditInfo = new AuditInfo();
            auditInfo.EventID = BROperation.View;
            auditInfo.ModuleID = BRModule.Products;
            auditInfo.Status = BRStatus.Successful;

            string strRefNo = string.Empty;

            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
            ds_ProductList = new BRDataSet();
            IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(0);
            //arParms[0] = intfDBHelper.CreateNewDBParam("ProductID", SqlDbType.NVarChar, 20);
            //arParms[0].Value = ProductID;
            //arParms[1] = intfDBHelper.CreateNewDBParam("CurrencyID", SqlDbType.NVarChar, 6);
            //arParms[1].Value = CurrencyID;
            //arParms[2] = intfDBHelper.CreateNewDBParam("ProductTypeID", SqlDbType.NVarChar, 20);
            //arParms[2].Value = ProductTypeID;


            try
            {
                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetMobileProductList", ds_ProductList, new string[] { "dt_WebServiceStatus", "GetProductList" }, arParms);
                }

                auditInfo.Message = CoreOmniApiBase.DataTableToJSONWithJSONNet(ds_ProductList.Tables[0]);

                if (ds_ProductList.Tables[0].Rows.Count == 0)
                    return false;

                
                return true;
            }
            catch (Exception ex)
            {
                //throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
                auditInfo.Message = ex.Message.ToString();
                return false;
            }
            finally
            {
                ds_ProductList.Dispose();
                Task.Run(() => CoreOmniApiBase.AddEditAPIAuditLog(usrInfo, "GetProductList", "RESPONSE", JsonConvert.SerializeObject(auditInfo.Message).ToString(), ""));
            }
        }
        #endregion product methods


    #region utilities
        static public bool GetLoanCalcIbank(UserInfo usrInfo, out DS_LoanCalulator dsLoanCalculator, params object[] objParamArray)
        {
            dsLoanCalculator = new DS_LoanCalulator();
            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
            try
            {
                IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(15);
                arParms[0] = intfDBHelper.CreateNewDBParam("Amount", SqlDbType.VarChar, 15);
                arParms[0].Value = objParamArray[0];
                arParms[1] = intfDBHelper.CreateNewDBParam("InterestRate", SqlDbType.NVarChar, 20);
                arParms[1].Value = objParamArray[1];
                arParms[2] = intfDBHelper.CreateNewDBParam("CalculationMethodID", SqlDbType.NVarChar, 25);
                arParms[2].Value = objParamArray[2];
                arParms[3] = intfDBHelper.CreateNewDBParam("PeriodTypeID", SqlDbType.NVarChar, 25);
                arParms[3].Value = objParamArray[9];
                arParms[4] = intfDBHelper.CreateNewDBParam("FrequencyID", SqlDbType.NVarChar, 25);
                arParms[4].Value = objParamArray[3];
                arParms[5] = intfDBHelper.CreateNewDBParam("Term", SqlDbType.SmallInt);
                arParms[5].Value = objParamArray[4];
                arParms[6] = intfDBHelper.CreateNewDBParam("NoOfInstallments", SqlDbType.SmallInt);
                arParms[6].Value = objParamArray[5];
                arParms[7] = intfDBHelper.CreateNewDBParam("CalculationPeriod", SqlDbType.NVarChar, 5);
                arParms[7].Value = objParamArray[8];
                arParms[8] = intfDBHelper.CreateNewDBParam("RoundingID", SqlDbType.NVarChar, 25);
                arParms[8].Value = objParamArray[6];
                arParms[9] = intfDBHelper.CreateNewDBParam("IntRoundingID", SqlDbType.NVarChar, 25);
                arParms[9].Value = objParamArray[10];
                arParms[10] = intfDBHelper.CreateNewDBParam("LoanDisbDate", SqlDbType.DateTime);
                arParms[10].Value = objParamArray[14];
                arParms[11] = intfDBHelper.CreateNewDBParam("Rule78", SqlDbType.Bit);
                arParms[11].Value = objParamArray[7];
                arParms[12] = intfDBHelper.CreateNewDBParam("GracePeriod", SqlDbType.SmallInt);
                arParms[12].Value = objParamArray[11];
                arParms[13] = intfDBHelper.CreateNewDBParam("TermExcludesGracePeriod", SqlDbType.Bit);
                arParms[13].Value = objParamArray[12];
                arParms[14] = intfDBHelper.CreateNewDBParam("CollectInterestDuringGracePeriod", SqlDbType.Bit);
                arParms[14].Value = objParamArray[13];

                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "P_GetLoanInstalmentIbank", dsLoanCalculator, new string[] { "LoanCalulator", "LoanCalulatorGrid", "status" }, arParms);
                }
            }
            catch (Exception ex)
            {
                throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
            }

            if (dsLoanCalculator.t_LoanCalulator.Rows.Count == 0 && dsLoanCalculator.t_LoanCalulatorGrid.Rows.Count == 0)
                return false;
            return true;
        }
        #endregion utilities

        #region product methods

    
        
        static public bool GetProductLoan(UserInfo usrInfo, string ProductID, short Direction, out DS_ProductLoan dsProductLoan, out BRDataSet dsSuperVision)
        {
            AuditInfo auditInfo = new AuditInfo();
            auditInfo.EventID = BROperation.View;
            auditInfo.ModuleID = BRModule.ProductLoan;
            auditInfo.Status = BRStatus.Successful;

            dsProductLoan = new DS_ProductLoan();
            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
            IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(5);

            arParms[0] = intfDBHelper.CreateNewDBParam("BankID", SqlDbType.NVarChar, 6);
            arParms[0].Value = usrInfo.strBank;
            arParms[1] = intfDBHelper.CreateNewDBParam("OurBranchID", SqlDbType.NVarChar, 6);
            arParms[1].Value = usrInfo.strBranch;
            arParms[2] = intfDBHelper.CreateNewDBParam("ProductID", SqlDbType.NVarChar, 6);
            arParms[2].Value = ProductID;
            arParms[3] = intfDBHelper.CreateNewDBParam("OperatorID", SqlDbType.NVarChar, 25);
            arParms[3].Value = usrInfo.strUser;
            arParms[4] = intfDBHelper.CreateNewDBParam("Direction", SqlDbType.SmallInt);
            arParms[4].Value = Direction;
            try
            {
                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetProductLoan", dsProductLoan, new string[] { BRUtilities.strSupTableName, "ProductLoan", "ProductCustRestriction" }, arParms);
                    dsSuperVision = BRUtilities.GetSupervisionRecord(dsProductLoan);
                }
            }
            catch (Exception ex)
            {
                auditInfo.Status = BRStatus.Failed;
                auditInfo.Message = ex.Message;
                PassAuditInformation(null, usrInfo, ProductID, string.Empty, auditInfo);
                throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
            }
            if (dsProductLoan.t_ProductLoan.Rows.Count == 0 && dsSuperVision.Tables[BRUtilities.strSupTableName].Rows.Count == 0)
                return false;
            return true;

        }
        static public bool GetProductDetails(UserInfo usrInfo, string OurBranchID, string WFAdvTypeID, string ProductID, string SpecialIntRate, out DS_WFLoanApplications dsWFLoanApplications)
        {

            AuditInfo auditInfo = new AuditInfo();
            auditInfo.ModuleID = BRModule.WFLoanApplications;
            auditInfo.Status = BRStatus.Successful;
            dsWFLoanApplications = new DS_WFLoanApplications();
            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);

            string sp = "p_GetWFProductDetails";

            try
            {
                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    IDataParameter[] arParms = intfDBHelper.GetSPParameters(connection, sp);
                    object paramValue = null;
                    foreach (IDataParameter param in arParms)
                    {
                        paramValue = null;
                        switch (param.ParameterName.Substring(1).ToUpper())
                        {
                            case "OURBRANCHID":
                                paramValue = OurBranchID;
                                break;
                            case "WFADVTYPEID":
                                paramValue = WFAdvTypeID;
                                break;
                            case "PRODUCTID":
                                paramValue = ProductID;
                                break;
                            case "OPERATORID":
                                paramValue = usrInfo.strUser;
                                break;
                            case "SPECIALINTRATE":
                                paramValue = SpecialIntRate;
                                break;

                        }
                        param.Value = paramValue;


                    }

                    intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, sp, dsWFLoanApplications, new string[] { "WFLoanApplications", "SlabDetails" }, arParms);
                }
            }
            catch (Exception ex)
            {
                auditInfo.Status = BRStatus.Failed;
                auditInfo.Message = ex.Message;
                PassAuditInformation(null, usrInfo, ProductID, string.Empty, auditInfo);
                throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
            }
            if (dsWFLoanApplications.t_WFLoanApplications.Rows.Count == 0)
                return false;
            return true;
        }
        
    #region charges methods
        static public bool GetLoanIntCalcRule(UserInfo usrInfo, string strRuleID, short sDirection, out DS_LoanIntCalcRule dsLoanIntCalcRule, out BRDataSet dsSupervision)
        {

            AuditInfo auditInfo = new AuditInfo();
            auditInfo.ModuleID = BRModule.LoanCalculationRule;
            auditInfo.EventID = BROperation.View;
            auditInfo.Status = BRStatus.Successful;
            dsLoanIntCalcRule = new DS_LoanIntCalcRule();
            IDBHelper intfDBHelper = DBClient.GetDBHelper(usrInfo);
            IDataParameter[] arParms = intfDBHelper.CreateDBParamsArray(5);
            arParms[0] = intfDBHelper.CreateNewDBParam("BankID", SqlDbType.NVarChar, 6);
            arParms[0].Value = usrInfo.strBank;
            arParms[1] = intfDBHelper.CreateNewDBParam("OurBranchID", SqlDbType.NVarChar, 6);
            arParms[1].Value = usrInfo.strBranch;
            arParms[2] = intfDBHelper.CreateNewDBParam("IntCalcRuleID", SqlDbType.NVarChar, 10);
            arParms[2].Value = strRuleID;
            arParms[3] = intfDBHelper.CreateNewDBParam("OperatorID", SqlDbType.NVarChar, 25);
            arParms[3].Value = usrInfo.strUser;
            arParms[4] = intfDBHelper.CreateNewDBParam("Direction", SqlDbType.SmallInt);
            arParms[4].Value = sDirection;
            try
            {
                using (IDbConnection connection = DBClient.GetConnection(usrInfo))
                {
                    intfDBHelper.FillDataset(connection, CommandType.StoredProcedure, "p_GetLoanIntCalcRule", dsLoanIntCalcRule, new string[] { BRUtilities.strSupTableName, dsLoanIntCalcRule.t_LoanIntCalcRule.TableName }, arParms);

                }
                dsSupervision = BRUtilities.GetSupervisionRecord(dsLoanIntCalcRule);
            }
            catch (Exception ex)
            {
                auditInfo.Status = BRStatus.Failed;
                auditInfo.Message = ex.Message;
                PassAuditInformation(null, usrInfo, strRuleID, string.Empty, auditInfo);
                throw DBClientUtils.GetDBErrorMessages(ex, usrInfo.strUser, usrInfo.strSystem);
            }
            if (dsLoanIntCalcRule.t_LoanIntCalcRule.Rows.Count == 0 && dsSupervision.Tables[BRUtilities.strSupTableName].Rows.Count == 0)
                return false;
            return true;
        }
        #endregion charges methods


    

/*
         /// <summary>
         /// Initiates a workflow for loan application the different stages are (APPL,APPR,SANC,DISB)
         /// </summary>
         /// <param name="myData"></param>
         /// <returns></returns>
         
        [HttpPost]
        [SwaggerResponse(300057, "Invalid Request Parameters")]
        [SwaggerResponse(500, "Internal Server Error")]
        public IHttpActionResult WFLoanApplication(Loans myData)
        {           
            bool isValid = false;          
            //string strSessionID = string.Empty;           
            string response = string.Empty;
            string wfStageID = string.Empty;
            DataSet dsLoanApp = new DataSet();
            string tableName = string.Empty;
            string WFStageID = string.Empty;

            if (myData == null)
            {
                DataSet resp = CoreOmniApiBase.GenResponse("99", "300057", "Invalid Request Parameters");
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, resp, resp);
            }

            try
            {
                DataSet MyApplication = new DataSet();
                //strSessionID = myData.SessionID;
                wfStageID = myData.Stage;
                UserInfo usrInfo = CoreOmniApiBase.GetDbParameter();
                //usrInfo.strBRUserName = CoreOmniApiBase.gOperatorID;
                //CoreOmniApiBase.ValidateRequest(strSessionID, out isValid);
                WFStageID = myData.Stage;

                switch (wfStageID)
                {
                    case "APPL":
                        tableName = "dt_WFLoanApplications";
                        break;
                    case "APPR":
                        tableName = "dt_WFLoanAppraisals";
                        break;
                    case "SANC":
                        tableName = "dt_WFLoanAppraisal";
                        break;
                    case "DISB":
                        tableName = "dt_GroupLoanScheme";
                        break;
                }

                DataTable dtLoan = CoreOmniApiBase.dtFromObject(myData, tableName);
                dtLoan.Columns.Remove("SessionID");
                //if (isValid == true)
                //{
                    MyApplication.Tables.Add(dtLoan);
                    response = CoreOmniApiBase.LoanApplicationProcess(usrInfo, wfStageID, MyApplication, out dsLoanApp);
                    return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, dsLoanApp, dsLoanApp);
                //}
                //return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetInvalidSessionIDMessage(), CoreOmniApiBase.GetInvalidSessionIDMessage());

            }
            catch (Exception ex)
            {
                return CreatedAtRoute(CoreOmniApiBase.OmniApiRoute, CoreOmniApiBase.GetBRErrorMessage(ex), CoreOmniApiBase.GetBRErrorMessage(ex));
            }
        }
        */

         

}
