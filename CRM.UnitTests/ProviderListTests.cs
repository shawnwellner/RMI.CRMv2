using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CRM.Domain;
using CRM.Models;
using System.Collections.Generic;
using System.Linq;

namespace CRM.UnitTests {
    internal static class CurrentUser {
        public static int UserId { get { return 1; } }
        public static int VerticalId { get { return 1; } }
    }

    [TestClass]
    public class ProviderListTests {
        #region Private Members
        private enum ClientInputTypeRelIds {
            ChronicPain = 2,
            InsuranceQuestion = 11,
            MRIYesNo = 12,
            MRIDate = 13
        }

        private enum InsuranceTypes {
            Aetna = 13,
            Medicare = 306,
            Medicaid = 300
        }

        private enum Client {
            SCA = 16,
            NJSO = 11,
            MISC = 276,
            LISS = 36,
            DISC = 28,
            OLSS = 307
        }

        private static CustomerFormModel GetCustomerModelForm(Client client, InsuranceTypes insurance) {
            switch(client) {
                case Client.NJSO:
                    return new CustomerFormModel() {
                        UserId = null,
                        City = "Princeton",
                        State = "NJ",
                        Zip = "08540",
                        ListOfAnswers = FillVerticalQuestions(client, insurance)
                    };
                case Client.LISS:
                    return new CustomerFormModel() {
                        UserId = null,
                        City = "New York",
                        State = "NY",
                        Zip = "11725",
                        ListOfAnswers = FillVerticalQuestions(client, insurance)
                    };
                case Client.DISC:
                    return new CustomerFormModel() {
                        UserId = null,
                        City = "Gilbert",
                        State = "AZ",
                        Zip = "85297",
                        ListOfAnswers = FillVerticalQuestions(client, insurance)
                    };
                case Client.MISC:
                    return new CustomerFormModel() {
                        UserId = null,
                        City = "Dallas",
                        State = "TX",
                        Zip = "75231",
                        ListOfAnswers = FillVerticalQuestions(client, insurance)
                    };
                case Client.SCA:
                    return new CustomerFormModel() {
                        UserId = null,
                        City = "Atlanta",
                        State = "GA",
                        Zip = "30308",
                        ListOfAnswers = FillVerticalQuestions(client, insurance)
                    };
                case Client.OLSS:
                    return new CustomerFormModel() {
                        UserId = null,
                        City = "Tampa",
                        State = "FL",
                        Zip = "33625",
                        ListOfAnswers = FillVerticalQuestions(client, insurance)
                    };
                default:
                    throw new NotImplementedException();
            }
        }

        private static List<QuestionInput> FillVerticalQuestions(Client client, InsuranceTypes insurance) {
            DateTime now = DateTime.Now;
            List<QuestionInput> listOfAnswers = new List<QuestionInput>();
            List<Question<QuestionInput>> questions = Question.GetVerticalQuestions(CurrentUser.VerticalId, CurrentUser.UserId);

            foreach (Question q in questions) {
                foreach (QuestionInput a in q.ListOfInputs) {
                    switch (a.InputType) {
                        case InputTypes.typeahead:
                        case InputTypes.dropdown:
                            switch (a.QuestionId) {
                                case 1: //Question 1 Year of Birth
                                    a.Answer = "1971";
                                    break;
                                case 4: //Question 4 (Insurance)
                                    a.Answer = Question.InsuranceList[(int)insurance];
                                    //a.Answer = insurance.ToString("D");
                                    break;
                                default:
                                    throw new Exception("Unexpected Dropdown Answer");
                            }
                            break;
                        case InputTypes.yesno:
                            a.Answer = "Yes";
                            break;
                        case InputTypes.checkbox:
                            a.Answer = a.OptionValue;
                            break;
                        case InputTypes.date:
                            a.Answer = now.ToString("MM-yyyy");
                            break;
                        case InputTypes.radio:
                            break;
                    }
                    listOfAnswers.Add(a);
                }
            }
            return listOfAnswers;
        }
        #endregion

        [TestMethod]
        public void NJSpine_Test() {
            CustomerFormModel model = GetCustomerModelForm(Client.NJSO, InsuranceTypes.Aetna);
            ProviderList list = Provider.GetListOfProviders(CurrentUser.VerticalId, model.UserId, model.Zip, model.City, model.State, model.ClientVerticalRelId, model.OfficeLocationId, model.ListOfAnswers);
            //Test that no questions are disqualified
            Assert.AreEqual(0, list.NotQualifiedAnswers.Count);

            //Test that NJSO shows in the list
            bool results = list.Any(l => l.Provider.CompanyName.Equals("NJ Spine & Orthopedic", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(results);
        }

        [TestMethod]
        public void LISS_Test() {
            CustomerFormModel model = GetCustomerModelForm(Client.LISS, InsuranceTypes.Aetna);
            
            ProviderList list = Provider.GetListOfProviders(CurrentUser.VerticalId, model.UserId, model.Zip, model.City, model.State, model.ClientVerticalRelId, model.OfficeLocationId, model.ListOfAnswers);
            //Test that no questions are disqualified
            Assert.AreEqual(0, list.NotQualifiedAnswers.Count);

            //Test that LISS shows in the list
            bool results = list.Any(l => l.ClientId == (int)Client.LISS);
            Assert.IsTrue(results);

            //Test that NJSO shows in the list since they are National
            results = list.Any(l => l.ClientId == (int)Client.NJSO);
            Assert.IsTrue(results);

            //Test the first office in the list and it's distance from the customer form's zip code
            OfficeLocation firstOffice = list.First();
            Assert.IsTrue(firstOffice.OfficeName.Equals("Commack Office", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(firstOffice.Distance < 5);
        }

        [TestMethod]
        public void Medicare_QualifiedTest() {
            CustomerFormModel model = GetCustomerModelForm(Client.LISS, InsuranceTypes.Medicare);
            
            ProviderList list = Provider.GetListOfProviders(CurrentUser.VerticalId, model.UserId, model.Zip, model.City, model.State, model.ClientVerticalRelId, model.OfficeLocationId, model.ListOfAnswers);
            //Test that no questions are disqualified
            Assert.AreEqual(0, list.NotQualifiedAnswers.Count);
            
            //Test that LISS shows in the list
            bool results = list.Any(l => l.ClientId == (int)Client.LISS);
            Assert.IsTrue(results);

            //Test that NJSO does not show in the list since they are National
            results = list.Any(l => l.ClientId == (int)Client.NJSO);
            Assert.IsFalse(results);

            //Test the first office in the list and it's distance from the customer form's zip code
            OfficeLocation firstOffice = list.First();
            Assert.IsTrue(firstOffice.OfficeName.Equals("Commack Office", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(firstOffice.Distance < 5);
        }

        [TestMethod]
        public void Medicaid_NotQualifiedTest() {
            CustomerFormModel model = GetCustomerModelForm(Client.LISS, InsuranceTypes.Medicaid);

            ProviderList list = Provider.GetListOfProviders(CurrentUser.VerticalId, model.UserId, model.Zip, model.City, model.State, model.ClientVerticalRelId, model.OfficeLocationId, model.ListOfAnswers);
            //Test only 1 disqualifing question should show
            Assert.IsTrue(list.NotQualifiedAnswers.First().ClientInputTypeRelId == (int)ClientInputTypeRelIds.InsuranceQuestion);

            //Test that no items are in this list of providers
            Assert.AreEqual(0, list.Count);

            //Test to insure only the insurance question should be disqualified
            Assert.IsTrue(list.NotQualifiedAnswers.First().IsInsurance);
        }

        [TestMethod]
        public void Medicare_NotQualifiedTest() {
            CustomerFormModel model = GetCustomerModelForm(Client.SCA, InsuranceTypes.Medicare);

            ProviderList list = Provider.GetListOfProviders(CurrentUser.VerticalId, model.UserId, model.Zip, model.City, model.State, model.ClientVerticalRelId, model.OfficeLocationId, model.ListOfAnswers);
            QuestionInput notQualified = list.NotQualifiedAnswers.Single();
            
            //Test only 1 disqualifing question should show
            Assert.IsTrue(notQualified.ClientInputTypeRelId == (int)ClientInputTypeRelIds.InsuranceQuestion);

            //Test that no items are in this list of providers
            Assert.AreEqual(0, list.Count);

            //Test to insure only the insurance question should be disqualified
            Assert.IsTrue(notQualified.IsInsurance);
        }

        [TestMethod]
        public void DISC_Test() {
            CustomerFormModel model = GetCustomerModelForm(Client.DISC, InsuranceTypes.Aetna);

            ProviderList list = Provider.GetListOfProviders(CurrentUser.VerticalId, model.UserId, model.Zip, model.City, model.State, model.ClientVerticalRelId, model.OfficeLocationId, model.ListOfAnswers);
            //Test that no questions are disqualified
            Assert.AreEqual(0, list.NotQualifiedAnswers.Count);

            //Test that DISC shows in the list
            bool results = list.Any(l => l.ClientId == (int)Client.DISC);
            Assert.IsTrue(results);

            //Test that NJSO shows in the list also since they are National
            results = list.Any(l => l.ClientId == (int)Client.NJSO);
            Assert.IsTrue(results);

            //Test the first office in the list and it's distance from the customer form's zip code
            OfficeLocation firstOffice = list.First();
            Assert.IsTrue(firstOffice.OfficeName.Equals("East Valley Office", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(firstOffice.Distance < 1.5);
        }

        [TestMethod]
        public void MISC_Test() {
            CustomerFormModel model = GetCustomerModelForm(Client.MISC, InsuranceTypes.Aetna);
        
            ProviderList list = Provider.GetListOfProviders(CurrentUser.VerticalId, model.UserId, model.Zip, model.City, model.State, model.ClientVerticalRelId, model.OfficeLocationId, model.ListOfAnswers);
            //Test that no questions are disqualified
            Assert.AreEqual(0, list.NotQualifiedAnswers.Count);

            //Test that MISC shows in the list
            bool results = list.Any(l => l.ClientId == (int)Client.MISC);
            Assert.IsTrue(results);

            //Test that NJSO shows in the list also since they are National
            results = list.Any(l => l.ClientId == (int)Client.NJSO);
            Assert.IsTrue(results);

            //Test the first office in the list and it's distance from the customer form's zip code
            OfficeLocation firstOffice = list.First();
            Assert.IsTrue(firstOffice.OfficeName.Equals("Central Dallas Office", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(firstOffice.Distance <= 2);
        }

        [TestMethod]
        public void ChronicPain_Qualified() {
            CustomerFormModel model = GetCustomerModelForm(Client.SCA, InsuranceTypes.Aetna);
            model.ListOfAnswers.FindLast(a => a.ClientInputTypeRelId == (int)ClientInputTypeRelIds.ChronicPain).Answer = "No";
            ProviderList list = Provider.GetListOfProviders(CurrentUser.VerticalId, model.UserId, model.Zip, model.City, model.State, model.ClientVerticalRelId, model.OfficeLocationId, model.ListOfAnswers);

            //Test that no questions are disqualified
            Assert.AreEqual(0, list.NotQualifiedAnswers.Count);

            //Test the SCA is not in the list
            bool results = list.Any(l => l.ClientId == (int)Client.SCA);
            Assert.IsFalse(results);

            //Test that NJSO shows in the list also since they are National
            results = list.Any(l => l.ClientId == (int)Client.NJSO);
            Assert.IsTrue(results);
        }

        [TestMethod]
        public void ChronicPain_NotQualified() {
            CustomerFormModel model = GetCustomerModelForm(Client.SCA, InsuranceTypes.Aetna);
            model.ListOfAnswers.FindLast(a => a.ClientInputTypeRelId == (int)ClientInputTypeRelIds.ChronicPain).Answer = "No";
            model.ListOfAnswers.FindLast(a => a.ClientInputTypeRelId == (int)ClientInputTypeRelIds.MRIYesNo).Answer = "No";
            ProviderList list = Provider.GetListOfProviders(CurrentUser.VerticalId, model.UserId, model.Zip, model.City, model.State, model.ClientVerticalRelId, model.OfficeLocationId, model.ListOfAnswers);

            //Test that no questions are disqualified

            bool results = list.NotQualifiedAnswers.Any(q => q.Rules.Any(r => r.ClientId == (int)Client.SCA) && q.ClientInputTypeRelId == (int)ClientInputTypeRelIds.ChronicPain);
            Assert.IsTrue(results);
        }

        [TestMethod]
        public void SCA_Test() {
            CustomerFormModel model = GetCustomerModelForm(Client.SCA, InsuranceTypes.Aetna);
            
            ProviderList list = Provider.GetListOfProviders(CurrentUser.VerticalId, model.UserId, model.Zip, model.City, model.State, model.ClientVerticalRelId, model.OfficeLocationId, model.ListOfAnswers);
            //Test that no questions are disqualified
            Assert.AreEqual(0, list.NotQualifiedAnswers.Count);

            //Test the SCA shows in the list
            bool results = list.Any(l => l.ClientId == (int)Client.SCA);
            Assert.IsTrue(results);

            //Test that NJSO shows in the list also since they are National
            results = list.Any(l => l.ClientId == (int)Client.NJSO);
            Assert.IsTrue(results);

            //Test the first office in the list and it's distance from the customer form's zip code
            OfficeLocation firstOffice = list.First();
            Assert.IsTrue(firstOffice.OfficeName.Equals("Atlanta Office", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(firstOffice.Distance <= 8);
        }

        [TestMethod]
        [ExpectedException(typeof(ZipCodeException))]
        public void InvalidZip_Test() {
            CustomerFormModel model = GetCustomerModelForm(Client.SCA, InsuranceTypes.Aetna);
            model.Zip = "00000";
            
            ProviderList list = Provider.GetListOfProviders(CurrentUser.VerticalId, model.UserId, model.Zip, model.City, model.State, model.ClientVerticalRelId, model.OfficeLocationId, model.ListOfAnswers);
            //Code should never get here...ZipCodeException should occur
            Assert.IsTrue(false);
        }

        [TestMethod]
        [ExpectedException(typeof(ZipCodeException))]
        public void ZipState_NotMatched_Test() {
            CustomerFormModel model = GetCustomerModelForm(Client.SCA, InsuranceTypes.Aetna);
            model.State = "NJ";

            //Code should never get here...ZipCodeException should occur
            ProviderList list = Provider.GetListOfProviders(CurrentUser.VerticalId, model.UserId, model.Zip, model.City, model.State, model.ClientVerticalRelId, model.OfficeLocationId, model.ListOfAnswers);
            Assert.IsTrue(false);
        }

        [TestMethod]
        public void NoMRI_Qualified_Test() {
            CustomerFormModel model = GetCustomerModelForm(Client.SCA, InsuranceTypes.Aetna);
            model.ListOfAnswers.FindLast(a => a.ClientInputTypeRelId == (int)ClientInputTypeRelIds.MRIYesNo).Answer = "No";

            ProviderList list = Provider.GetListOfProviders(CurrentUser.VerticalId, model.UserId, model.Zip, model.City, model.State, model.ClientVerticalRelId, model.OfficeLocationId, model.ListOfAnswers);
            //Test that no questions are disqualified
            Assert.AreEqual(0, list.NotQualifiedAnswers.Count);

            //Test the SCA shows in the list
            bool results = list.Any(l => l.ClientId == (int)Client.SCA && l.OfficeCity.Equals("Atlanta"));
            Assert.IsTrue(results);

            //Test to insure only one office shows in the list
            Assert.AreEqual(1, list.Count);
        }

        [TestMethod]
        public void NoMRI_DateFilter_Qualified_Test() {
            CustomerFormModel model = GetCustomerModelForm(Client.SCA, InsuranceTypes.Aetna);
            model.ListOfAnswers.FindLast(a => a.ClientInputTypeRelId == (int)ClientInputTypeRelIds.MRIDate).Answer = "01-1990";

            ProviderList list = Provider.GetListOfProviders(CurrentUser.VerticalId, model.UserId, model.Zip, model.City, model.State, model.ClientVerticalRelId, model.OfficeLocationId, model.ListOfAnswers);
            //Test that no questions are disqualified
            Assert.AreEqual(0, list.NotQualifiedAnswers.Count);

            //Test the SCA shows in the list
            bool results = list.SingleOrDefault(l => l.ClientId == (int)Client.SCA && l.OfficeCity.Equals("Atlanta")) != null;
            Assert.IsTrue(results);
        }

        [TestMethod]
        public void NoMRI_NotQualifiedTest() {
            CustomerFormModel model = GetCustomerModelForm(Client.NJSO, InsuranceTypes.Aetna);
            model.ListOfAnswers.FindLast(a => a.ClientInputTypeRelId == (int)ClientInputTypeRelIds.MRIYesNo).Answer = "No";

            ProviderList list = Provider.GetListOfProviders(CurrentUser.VerticalId, model.UserId, model.Zip, model.City, model.State, model.ClientVerticalRelId, model.OfficeLocationId, model.ListOfAnswers);
            //Test only 1 disqualifing question should show
            Assert.IsTrue(list.NotQualifiedAnswers.First().ClientInputTypeRelId == (int)ClientInputTypeRelIds.MRIYesNo);
            
            //Test that no providers show
            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        public void MRIDate_Qualified_Test() {
            CustomerFormModel model = GetCustomerModelForm(Client.LISS, InsuranceTypes.Aetna);
            model.ListOfAnswers.FindLast(a => a.ClientInputTypeRelId == (int)ClientInputTypeRelIds.MRIDate).Answer = "01-1990";

            ProviderList list = Provider.GetListOfProviders(CurrentUser.VerticalId, model.UserId, model.Zip, model.City, model.State, model.ClientVerticalRelId, model.OfficeLocationId, model.ListOfAnswers);
            //Test that no questions are disqualified
            Assert.AreEqual(0, list.NotQualifiedAnswers.Count);

            //Test the LISS shows in the list
            bool results = list.Any(l => l.ClientId == (int)Client.LISS);
            Assert.IsTrue(results);

            //Test No Other Client is in the list
            results = list.Any(l => l.ClientId != (int)Client.LISS);
            Assert.IsFalse(results);
        }

        [TestMethod]
        public void MRIDate_NotQualified_Test() {
            CustomerFormModel model = GetCustomerModelForm(Client.NJSO, InsuranceTypes.Aetna);
            model.ListOfAnswers.FindLast(a => a.ClientInputTypeRelId == (int)ClientInputTypeRelIds.MRIDate).Answer = "01-1990";

            ProviderList list = Provider.GetListOfProviders(CurrentUser.VerticalId, model.UserId, model.Zip, model.City, model.State, model.ClientVerticalRelId, model.OfficeLocationId, model.ListOfAnswers);
            //Test only 1 disqualifing question should show
            Assert.IsTrue(list.NotQualifiedAnswers.Any(r => r.ClientInputTypeRelId == (int)ClientInputTypeRelIds.MRIDate));
            
            //Test that no providers show
            Assert.AreEqual(0, list.Count);
        }

        [TestMethod]
        public void OLSS_MRIDate_Qualified_Test() {
            DateTime now = DateTime.Now;

            CustomerFormModel model = GetCustomerModelForm(Client.OLSS, InsuranceTypes.Aetna);
            model.ListOfAnswers.FindLast(a => a.ClientInputTypeRelId == (int)ClientInputTypeRelIds.MRIDate).Answer = now.AddMonths(-11).ToString("MM-yyyy");

            ProviderList list = Provider.GetListOfProviders(CurrentUser.VerticalId, model.UserId, model.Zip, model.City, model.State, model.ClientVerticalRelId, model.OfficeLocationId, model.ListOfAnswers);
            //Test that no questions are disqualified
            Assert.AreEqual(0, list.NotQualifiedAnswers.Count);

            //Test the SCA shows in the list
            bool results = list.Any(l => l.Provider.CompanyName.Equals("Orthopedic and Laser Spine Surgery"));
            Assert.IsTrue(results);
        }

        /* The rule was changed from <= 11 Months to the Default of <= 3 Years [SW] */
        /*[TestMethod]
        public void OLSS_MRIDate_NotQualified_Test() {
            DateTime now = DateTime.Now;

            CustomerFormModel model = GetCustomerModelForm(Client.OLSS, InsuranceTypes.Aetna);
            model.ListOfAnswers.FindLast(a => a.InputTypeRelId == (int)InputTypeRelIds.MRIDate).Answer = now.AddMonths(-12).ToString("MM-yyyy");

            ProviderList list = Provider.GetListOfProviders(CurrentUser.VerticalId, model.UserId, model.Zip, model.City, model.State, model.ClientVerticalRelId, model.OfficeLocationId, model.ListOfAnswers);
            
            //Test the OLSS does not show in the list
            bool results = list.Any(l => l.Provider.CompanyName.Equals("Orthopedic and Laser Spine Surgery"));
            Assert.IsFalse(results);
        }*/

        [TestMethod]
        public void MISC_Radius_Test() {
            CustomerFormModel model = GetCustomerModelForm(Client.MISC, InsuranceTypes.Aetna);
            model.City = "Valliant";
            model.State = "OK";
            model.Zip = "74764";

            ProviderList list = Provider.GetListOfProviders(CurrentUser.VerticalId, model.UserId, model.Zip, model.City, model.State, model.ClientVerticalRelId, model.OfficeLocationId, model.ListOfAnswers);
            //Test that no questions are disqualified
            Assert.AreEqual(0, list.NotQualifiedAnswers.Count);

            //Test that MISC shows in the list
            bool results = list.Any(l => l.Provider.CompanyName.Equals("Minimally Invasive Spine Care", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(results);

            //Test that NJSO shows in the list also since they are National
            results = list.Any(l => l.Provider.CompanyName.Equals("NJ Spine & Orthopedic", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(results);

            //Test the first office in the list and it's distance from the customer form's zip code
            OfficeLocation firstOffice = list.First();
            Assert.IsTrue(firstOffice.OfficeName.Equals("Rockwall Office", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(firstOffice.Distance <= 142);
        }
    }
}
