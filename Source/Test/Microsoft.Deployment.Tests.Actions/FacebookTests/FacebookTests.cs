﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Deployment.Common.Helpers;
using Microsoft.Deployment.Tests.Actions.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Deployment.Common.ActionModel;
using Newtonsoft.Json.Linq;

namespace Microsoft.Deployment.Tests.Actions.Facebook
{
    [TestClass]
    public class FacebookTests
    {
        [TestMethod]
        public void ValidateCorrectFacebookCredentials()
        {
            var dataStore = new DataStore();
            dataStore.AddToDataStore("FacebookClientId", "422676881457852");
            dataStore.AddToDataStore("FacebookClientSecret", "bf5fca097936ece936290031623b577b");
            var response = TestManager.ExecuteAction("Microsoft-ValidateFacebookDeveloperAccount", dataStore);
            Assert.IsTrue(response.IsSuccess);
        }

        [TestMethod]
        public void ValidateIncorrectFacebookCredentials()
        {
            var dataStore = new DataStore();
            dataStore.AddToDataStore("FacebookClientId", "422676881457851");
            dataStore.AddToDataStore("FacebookClientSecret", "bf5fca097936ece936290031623b577a");
            var response = TestManager.ExecuteAction("Microsoft-ValidateFacebookDeveloperAccount", dataStore);
            Assert.IsTrue(!response.IsSuccess);

            dataStore.AddToDataStore("FacebookClientId", "");
            dataStore.AddToDataStore("FacebookClientSecret", "");
            response = TestManager.ExecuteAction("Microsoft-ValidateFacebookDeveloperAccount", dataStore);
            Assert.IsTrue(!response.IsSuccess);
        }

        [TestMethod]
        public void ValidateFacebookPage()
        {
            var dataStore = new DataStore();
            dataStore.AddToDataStore("FacebookClientId", "422676881457852");
            dataStore.AddToDataStore("FacebookClientSecret", "bf5fca097936ece936290031623b577b");
            dataStore.AddToDataStore("FacebookPages", "walmart");
            var response = TestManager.ExecuteAction("Microsoft-ValidateFacebookPage", dataStore);
            Assert.IsTrue(response.IsSuccess);
        }

        [TestMethod]
        public void ValidateIncorrectFacebookPage()
        {
            var dataStore = new DataStore();
            dataStore.AddToDataStore("FacebookClientId", "422676881457852");
            dataStore.AddToDataStore("FacebookClientSecret", "bf5fca097936ece936290031623b577b");
            dataStore.AddToDataStore("FacebookPages", "walmartsfakepagethatdoesnotexist");
            var response = TestManager.ExecuteAction("Microsoft-ValidateFacebookPage", dataStore);
            Assert.IsTrue(!response.IsSuccess);
        }

        [TestMethod]
        public void SearchFacebookPage()
        {
            var dataStore = new DataStore();
            dataStore.AddToDataStore("FacebookClientId", "422676881457852");
            dataStore.AddToDataStore("FacebookClientSecret", "bf5fca097936ece936290031623b577b");
            dataStore.AddToDataStore("FacebookPage", "walmart");
            var response = TestManager.ExecuteAction("Microsoft-SearchFacebookPage", dataStore);
            Assert.IsTrue(!response.IsSuccess);
        }

        [TestMethod]
        public async Task DeployFacebookTemplate()
        {
            var dataStore =  await TestManager.GetDataStore(true);
            dataStore.AddToDataStore("FacebookClientId", "422676881457852");
            dataStore.AddToDataStore("FacebookClientSecret", "bf5fca097936ece936290031623b577b");
            dataStore.AddToDataStore("SqlConnectionString", "Server=tcp:modb1.database.windows.net,1433;Initial Catalog=fb;Persist Security Info=False;User ID=pbiadmin;Password=Corp123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
            dataStore.AddToDataStore("Schema", "fb");
          

            dataStore.AddToDataStore("SqlServerIndex", "0");
            dataStore.AddToDataStore("SqlScriptsFolder", "Database");
   

            dataStore.AddToDataStore("SqlGroup", "SolutionTemplate");
            dataStore.AddToDataStore("SqlSubGroup", "ETL");
            dataStore.AddToDataStore("SqlEntryName", "PagesToFollow");
            dataStore.AddToDataStore("SqlEntryValue", "walmart,target");
            dataStore.AddToDataStore("SqlConfigTable", "fb.configuration");

            ActionResponse response = null;

            dataStore.AddToDataStore("DeploymentName", "FunctionDeploymentTest1");
            dataStore.AddToDataStore("CognitiveServiceName", "TextCognitiveService");
            dataStore.AddToDataStore("CognitiveServiceType", "TextAnalytics");
            dataStore.AddToDataStore("CognitiveSkuName", "S1");
            response = TestManager.ExecuteAction("Microsoft-DeployCognitiveService", dataStore);
            Assert.IsTrue(response.IsSuccess);

            response = TestManager.ExecuteAction("Microsoft-GetCognitiveKey", dataStore);
            Assert.IsTrue(response.IsSuccess);

            response = await TestManager.ExecuteActionAsync("Microsoft-WaitForCognitiveService", dataStore, "Microsoft-FacebookTemplate");
            Assert.IsTrue(response.IsSuccess);

            response = await TestManager.ExecuteActionAsync("Microsoft-DeploySQLScripts", dataStore, "Microsoft-FacebookTemplate");
            Assert.IsTrue(response.IsSuccess);

            response = await TestManager.ExecuteActionAsync("Microsoft-SetConfigValueInSql", dataStore, "Microsoft-FacebookTemplate");
            Assert.IsTrue(response.IsSuccess);

            // Testing to see if the tear down works
            response = await TestManager.ExecuteActionAsync("Microsoft-DeploySQLScripts", dataStore, "Microsoft-FacebookTemplate");
            Assert.IsTrue(response.IsSuccess);

            response = await TestManager.ExecuteActionAsync("Microsoft-SetConfigValueInSql", dataStore, "Microsoft-FacebookTemplate");
            Assert.IsTrue(response.IsSuccess);


            dataStore.AddToDataStore("DeploymentName", "FunctionDeploymentTest1");
            dataStore.AddToDataStore("FunctionName", "unittestfunction1154");
            dataStore.AddToDataStore("RepoUrl", "https://github.com/MohaaliMicrosoft/FacebookExtraction");
            dataStore.AddToDataStore("sku", "Standard");

            response = TestManager.ExecuteAction("Microsoft-DeployAzureFunction", dataStore);
            Assert.IsTrue(response.IsSuccess);


            dataStore.AddToDataStore("DeploymentName", "FunctionDeploymentTest2");
            dataStore.AddToDataStore("StorageAccountName", "testmostorage123456");
            dataStore.AddToDataStore("StorageAccountType", "Standard_LRS");
            dataStore.AddToDataStore("StorageAccountEncryptionEnabled", "true");

            response = TestManager.ExecuteAction("Microsoft-CreateAzureStorageAccount", dataStore);
            Assert.IsTrue(response.IsSuccess);

            dataStore.AddToDataStore("DeploymentName", "FunctionDeploymentTest1");
            response = TestManager.ExecuteAction("Microsoft-WaitForArmDeploymentStatus", dataStore);
            Assert.IsTrue(response.IsSuccess);

            dataStore.AddToDataStore("DeploymentName", "FunctionDeploymentTest2");
            response = TestManager.ExecuteAction("Microsoft-WaitForArmDeploymentStatus", dataStore);
            Assert.IsTrue(response.IsSuccess);

            response = TestManager.ExecuteAction("Microsoft-GetStorageAccountKey", dataStore);
            Assert.IsTrue(response.IsSuccess);

            dataStore.AddToDataStore("ConnectorName", "azurequeues");
            JObject connector = new JObject();
            connector.Add("sharedkey", dataStore.GetValue("StorageAccountKey"));
            connector.Add("storageaccount", dataStore.GetValue("StorageAccountName"));
            dataStore.AddToDataStore("ConnectorPayload", connector);
            response = await TestManager.ExecuteActionAsync("Microsoft-UpdateBlobStorageConnector", dataStore, "Microsoft-FacebookTemplate");
            Assert.IsTrue(response.IsSuccess);

            dataStore.AddToDataStore("DeploymentName", "FunctionDeploymentTest");
            JObject val = new JObject();
            val.Add("queue", dataStore.GetValue("StorageAccountConnectionString"));
            dataStore.AddToDataStore("AppSettingKeys", val);

            response = TestManager.ExecuteAction("Microsoft-DeployAzureFunctionAppSettings", dataStore);
            Assert.IsTrue(response.IsSuccess);

            JObject val2 = new JObject();
            val2.Add("SqlConnectionString", dataStore.GetValue("SqlConnectionString"));
            val2.Add("CognitiveKey", dataStore.GetValue("CognitiveServiceKey"));
            val2.Add("Schema", dataStore.GetValue("Schema"));
            val2.Add("FacebookClientId", dataStore.GetValue("FacebookClientId"));
            val2.Add("FacebookClientSecret", dataStore.GetValue("FacebookClientSecret"));
            dataStore.AddToDataStore("AppSettingKeys", val2);

            response = TestManager.ExecuteAction("Microsoft-DeployAzureFunctionConnectionStrings", dataStore);
            Assert.IsTrue(response.IsSuccess);


            dataStore.AddToDataStore("AzureArmFile", "Service/Arm/logicapps.json");
            JObject logicapps = new JObject();
            logicapps.Add("functionname", dataStore.GetValue("FunctionName"));
            logicapps.Add("storageName", dataStore.GetValue("StorageAccountName"));
            logicapps.Add("subscription", dataStore.GetJson("SelectedSubscription", "SubscriptionId"));
            logicapps.Add("resourceGroup", dataStore.GetValue("SelectedResourceGroup"));
            logicapps.Add("days", "3000");
            dataStore.AddToDataStore("AzureArmParameters", logicapps);
            response = await TestManager.ExecuteActionAsync("Microsoft-DeployAzureArmTemplate", dataStore, "Microsoft-FacebookTemplate");
            Assert.IsTrue(response.IsSuccess);

        }
    }
}
