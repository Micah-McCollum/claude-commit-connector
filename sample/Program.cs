///****************************************************************************************************************
///*
///*    This demo application will demonstrate how to use the RangerMSP C# library 
///*    to connect to a local RangerMSP installation and query for,
///*    insert and/or update different RangerMSP entities.
///*    The demo assumes that RangerMSP is installed in the default location, 
///*    that is: C:\RangerMSP.
///*
///*    In this demo we will:
///*    1.  Configure an instance of the Config class. For the sake of simplicity, fixed values are used.
///*
///*    2.  Initialize the RangerMSP engine; 
///*        - CRM.Application.Initialize must be the first call before invoking any other RangerMSP library method.
///*
///*    3.  Then we'll show how to search for an account by it's field FLDCRDFULLNAME
///*        - for a full list of supported fields, refer to the CRM API Reference Manual
///*        located here: http://www.rangermsp.com/wiki/API_Reference_Manual
///*    
///*    4.  If the account we searched for doesn't exists, we'll insert a new one.
///*
///*    5.  Then we'll update the account's address.
///*
///*    6.  Next, we'll search for a ticket in this account containing specific words in FLDTKTPROBLEM field.
///*
///*    7.  If we don't find such a ticket, we'll create it.
///*    
///*    8.  We'll create a new Charge for this ticket.
///*
///*    9.  Each charge needs an Item associated with it. We'll first search for a common item
///*        and use it if found, or create a new item if not found.
///*
///*    10. Finally, we'll search for charges associated with the ticket and output the details for each one.
///*
///*    If an error occurs while communicating with the RangerMSP engine, an exception of type CRM.Exception
///*    will be thrown, containing Status and Codes reported by the engine.
///*
///****************************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRM_CSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //	For the sake of simplicity of this sample, all the settings are hardcoded in this source file.
                CRM.Config config = new CRM.Config();
                config.AppName = "C# Demo";
                config.DllFolder = "C:\\RangerMSP\\ThirdParty\\UserDev";
                config.DbFolder = "C:\\RangerMSP\\db";

                ////WebAPI
                ////Important: Mode should be set to ON in both CrmWPDbEng.ini and CrmWPDbQry.ini for the WebAPI to work
                //config.WebAPIUrl = "http://localhost:4964/";
                //config.UseWebAPI = true;
                //config.DbEngPassword = "";
                //config.DbQryPassword = "";
                ////End WebAPI

                //  CRM.Application.Initialize must be the first call before invoking any other CRM library method
                //  and it should be done only once in the program's lifetime.
                CRM.Application.Initialize(config);

                //	Search for an account with 'Bart De Hantsetters' in the full name
                CRM.ObjectQuery<CRM.Account> accountSearch = new CRM.ObjectQuery<CRM.Account>();

                accountSearch.AddCriteria(CRM.Account.Fields.FileAs, CRM.OperatorEnum.opLike, "%Bart De Hantsetters%");
                List<CRM.Account> accounts = accountSearch.FetchObjects();
                CRM.Account account = null;

                if (accounts.Count > 0)
                    account = accounts[0];//found it, use this account
                else
                {
                    //	Insert New Account into that Accounts table
                    account = new CRM.Account();

                    //	Fields can be set in two ways
                    //	1.	by using a dedicated property for each field; this is the recommended method of assigning values, 
                    //  use the other one only if there is no property available for the specific field you want to change
                    account.FileAs = "Bart De Hantsetters";
                    account.Dear = "Mr.";
                    account.Contact = "Hantsetters";

                    //	2.	or using the generic method SetFieldValue and passing the field name as the first parameter
                    //  account.SetFieldValue("FLDCRDFULLNAME", "Bart De Hantsetters");
                    //  account.SetFieldValue("FLDCRDDEAR", "Mr.");
                    //  account.SetFieldValue("FLDCRDCONTACT", "Hantsetters");

                    //	Saving the account
                    account.Save();
                }
                
                //	Edit the Address

                //	1.	by using a dedicated property for each field
                //  account.AddressLine1 = "Any street";
                //  account.Zip = "10001";
                //  account.City = "New York";
                //  account.Country = "United States";
                //  account.State = "NY";

                //	2.	or using the generic method SetFieldValue
                account.SetFieldValue("FLDCRDADDRESS1", "Any street");
                account.SetFieldValue("FLDCRDZIP", "10001");
                account.SetFieldValue("FLDCRDCITY", "New York");
                account.SetFieldValue("FLDCRDCOUNTRY", "United States");
                account.SetFieldValue("FLDCRDSTATE", "NY");

                //	Update the account
                account.Save();

                CRM.Ticket ticket = null;

                //	Search for a Ticket in the account that has 'Sample ticket' in the problem description
                CRM.ObjectQuery<CRM.Ticket> ticketSearch = new CRM.ObjectQuery<CRM.Ticket>();

                ticketSearch.AddCriteria(CRM.Ticket.Fields.AccountREC_ID, CRM.OperatorEnum.opEqual, account.AccountREC_ID);
                ticketSearch.AddCriteria(CRM.Ticket.Fields.Description, CRM.OperatorEnum.opLike, "%Sample ticket%");
                List<CRM.Ticket> tickets = ticketSearch.FetchObjects();

                if (tickets.Count > 0)
                    ticket = tickets[0];
                else
                {
                    //	Add a Ticket to this Account
                    ticket = new CRM.Ticket();

                    //	1.	by using a dedicated property for each field
                    ticket.AccountREC_ID = account.AccountREC_ID;
                    ticket.Description = "Sample ticket, put your ticket description here...";

                    //	2.	or using the generic method SetFieldValue
                    //  ticket.SetFieldValue("FLDTKTCARDID", account.GetFieldValue("FLDCRDRECID"));
                    //  ticket.SetFieldValue("FLDTKTPROBLEM", "Sample ticket, put your ticket description here...");

                    ticket.Save();
                }

                //	Add a Charge
                CRM.Charge charge = new CRM.Charge();

                //	1.	by using a dedicated property for each field
                //  charge.AccountREC_ID = account.AccountREC_ID;
                //  charge.TicketREC_ID = ticket.TicketREC_ID;
                //  charge.Description = "Sample charge, put your charge description here ...";

                //	2.	or using the generic method SetFieldValue
                charge.SetFieldValue("FLDSLPCARDID", account.GetFieldValue("FLDCRDRECID"));
                charge.SetFieldValue("FLDSLPTICKETID", ticket.GetFieldValue("FLDTKTRECID"));
                charge.SetFieldValue("FLDSLPDESC", "Sample charge, put your charge description here ...");

                CRM.Item item = null;

                //  Charge needs an item, here we search for a common item
                CRM.ObjectQuery<CRM.Item> itemSearch = new CRM.ObjectQuery<CRM.Item>();
                itemSearch.AddCriteria(CRM.Item.Fields.ItemCode, CRM.OperatorEnum.opEqual, "HP-4100TN-Toner");
                List<CRM.Item> items = itemSearch.FetchObjects();

                if (items.Count > 0)
                    item = items[0];
                else
                {
                    //	Add an Item to the charge
                    item = new CRM.Item();

                    //	1.	by using a dedicated property for each field
                    item.ItemGroup = "P";//Part
                    item.ItemCode = "HP-4100TN-Toner";
                    item.ItemName = "Toner for HP Laserjet 4100 TN";
                    item.PriceSource = "F";//Fixed Price
                    item.Price = 50;

                    //	2.	or using the generic method SetFieldValue
                    //  item.SetFieldValue("FLDITMITEMTYPEGROUP", "P");//Part
                    //  item.SetFieldValue("FLDITMITEMNO", "HP-4100TN-Toner");
                    //  item.SetFieldValue("FLDITMNAME", "Toner for HP Laserjet 4100 TN");
                    //  item.SetFieldValue("FLDITMPRICESOURCE", "F");//Fixed Price
                    //  item.SetFieldValue("FLDITMUNITPRICE", "50");

                    item.Save();
                }
                charge.ChargedItem = item.ItemREC_ID;

                charge.Save();

                //	query for all Charges associated with the ticket and output the details for each one
                CRM.ObjectQuery<CRM.Charge> chargeSearch = new CRM.ObjectQuery<CRM.Charge>();

                chargeSearch.AddCriteria(CRM.Charge.Fields.TicketREC_ID, CRM.OperatorEnum.opEqual, ticket.TicketREC_ID);
                List<CRM.Charge> charges = chargeSearch.FetchObjects();
                foreach (CRM.Charge chg in charges)
                {
                    Console.Out.Write("CHARGE RECORD:\r\n");
                    Console.Out.Write("AccountREC_ID:\t " + chg.AccountREC_ID + "\r\n");
                    Console.Out.Write("AdjustAmount:\t " + chg.AdjustAmount.ToString() + CRM.Charge.Fields.AdjustAmount.DisplaySymbol + "\r\n");
                    Console.Out.Write("AdjustPercent:\t " + chg.AdjustPercent.ToString() + CRM.Charge.Fields.AdjustPercent.DisplaySymbol + "\r\n");
                    Console.Out.Write("Billable:\t " + chg.Billable + "\r\n");
                    Console.Out.Write("Billed:\t\t " + chg.Billed + "\r\n");
                    Console.Out.Write("ChargedItem:\t " + chg.ChargedItem + "\r\n");
                    Console.Out.Write("ChargeREC_ID:\t " + chg.ChargeREC_ID + "\r\n");
                    Console.Out.Write("ContractREC_ID:\t " + chg.ContractREC_ID + "\r\n");
                    Console.Out.Write("CreateUser:\t " + chg.CreateUser + "\r\n");
                    Console.Out.Write("Date:\t\t " + chg.Date.ToString() + "\r\n");
                    Console.Out.Write("Description:\t " + chg.Description + "\r\n");
                    Console.Out.Write("EmployeeREC_ID:\t " + chg.EmployeeREC_ID + "\r\n");
                    Console.Out.Write("Field1:\t " + chg.Field1 + "\r\n");
                    Console.Out.Write("FromTime:\t " + chg.FromTime + "\r\n");
                    Console.Out.Write("Price_Rate:\t " + chg.Price_Rate.ToString() + CRM.Charge.Fields.Price_Rate.DisplaySymbol + "\r\n");
                    Console.Out.Write("TicketREC_ID:\t " + chg.TicketREC_ID + "\r\n");
                    Console.Out.Write("ToTime:\t\t " + chg.ToTime + "\r\n");
                    Console.Out.Write("Units_Hours:\t " + chg.Units_Hours.ToString() + CRM.Charge.Fields.Units_Hours.DisplaySymbol + "\r\n");
                    Console.Out.Write("\r\n");
                }
            }
            catch (CRM.Exception exc)//here we catch RangerMSP specific error
            {
                //  we can inspect the RangerMSP status (exc.Status) 
                //  refer to http://www.rangermsp.com/wiki/API_Reference_Manual#Error_Codes_Description
                //  exc.Codes contains all error codes last call generated
                //  also refer to http://www.rangermsp.com/wiki/API_Reference_Manual#Error_Codes_Description

                //  here we show the error message
                Console.Out.Write("Status: " +exc.Status.ToString() + "\r\n " + exc.Message);
            }
            catch (Exception exc)//here we catch system error
            {
                Console.Out.Write(exc.Message);
            }
            finally
            {
                //  Before exit we should call CRM.Application.Terminate to gracefully release all application resources
                //  and this should be done only once in the program's lifetime.
                CRM.Application.Terminate();
            }
        }
    }
}
