using System;
using System.Collections.Generic;
using CommitConnector.Core.Configuration;
using CommitConnector.Core.Ranger;
using CommitConnector.Core.Security;

namespace CommitConnector.ConnectionTest
{
    /// <summary>
    /// The RangerMSP wiki's C# API sample ("CSharp API" page), switched from a
    /// local database to the Web API. Run it on the connector machine before
    /// wiring up Claude to prove that the URL, password and CRMLib build work.
    ///
    /// Usage: CommitConnector.ConnectionTest.exe [webApiUrl] [city] [dllFolder]
    /// dllFolder defaults to the RANGERMSP_DLL_FOLDER environment variable.
    /// </summary>
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var options = new RangerMspOptions
            {
                AppName = "Claude Commit Connector - connection test",
                DllFolder = args.Length > 2 ? args[2] : Environment.GetEnvironmentVariable("RANGERMSP_DLL_FOLDER"),
            };
            if (args.Length > 0)
                options.WebApiUrl = args[0];
            string city = args.Length > 1 ? args[1] : "New York";

            try
            {
                options.Validate();

                CRM.Config config = new CRM.Config();
                config.AppName = options.AppName;
                config.WebAPIUrl = options.WebApiUrl;
                config.Password = CredentialResolver.ResolvePassword(options);
                config.UseWebAPI = true;
                config.DllFolder = RangerFolders.RequireDllFolder(options.DllFolder);
                Console.Out.WriteLine("Using DLL folder " + config.DllFolder);

                CRM.Application.Initialize(config);
                Console.Out.WriteLine("Connected to " + options.WebApiUrl);

                CRM.ObjectQuery<CRM.Account> accountSearch = new CRM.ObjectQuery<CRM.Account>();
                accountSearch.AddCriteria(CRM.Account.Fields.City, CRM.OperatorEnum.opEqual, city);
                List<CRM.Account> accounts = accountSearch.FetchObjects();

                Console.Out.WriteLine(accounts.Count + " account(s) in " + city + ":");
                foreach (CRM.Account account in accounts)
                {
                    Console.Out.WriteLine("  " + account.FileName + "  (" + account.AccountREC_ID + ")");
                }
                return 0;
            }
            catch (CRM.Exception exc)
            {
                Console.Error.WriteLine("RangerMSP API error: " + exc.Message);
                return 1;
            }
            catch (Exception exc)
            {
                Console.Error.WriteLine(exc.Message);
                return 1;
            }
            finally
            {
                CRM.Application.Terminate();
            }
        }
    }
}
