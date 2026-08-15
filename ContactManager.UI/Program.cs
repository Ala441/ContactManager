using ContactManager.BusinessLogic;
//using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ContactManager.UI
{
    public class ClsContactConsolUI
    {
        private readonly clsContactBusiness _bll;

        public ClsContactConsolUI(clsContactBusiness bll)
        {
            _bll = bll;
        }

        public void PrintAllContacts(IEnumerable<ClsContactModel> Contactlist)
        {
            if (Contactlist == null || !Contactlist.Any())
            {
                Console.WriteLine("No contacts found");
                Console.WriteLine("============================");
                return;
            }

            foreach (ClsContactModel co in Contactlist)
            {
                PrintSingleContact(co);
            }
        }

        public void PrintSingleContact(ClsContactModel co)
        {
            if (co == null)
            {
                Console.WriteLine("No contact found");
                Console.WriteLine("====================================");
                return;
            }
            Console.WriteLine($"contactid  {co.ContactID?.ToString() ?? "null"}");
            Console.WriteLine($"name {co.FirstName ?? "null"} {co.LastName ?? "null"}");
            Console.WriteLine($"email {co.Email ?? "null"}");
            Console.WriteLine($"phone {co.Phone ?? "null"}");
            Console.WriteLine($"address {co.Address ?? "null"}");
            Console.WriteLine($"countryid {co.CountryID?.ToString() ?? "null"}");
            Console.WriteLine("====================================");
        }

        private async Task<ClsContactModel> ReadSingleContactInput()
        {
            Console.WriteLine("please enter firstname");
            string firstName = Console.ReadLine();

            Console.WriteLine("please enter lastname");
            string lastName = Console.ReadLine();

            Console.WriteLine("please enter email");
            string email = Console.ReadLine();

            Console.WriteLine("please enter phone");
            string phone = Console.ReadLine();

            Console.WriteLine("please enter address");
            string address = Console.ReadLine();

            Console.WriteLine("please enter countryid from 1 to 5");
            int input = 0;
            byte count = 3;

            while (count > 0)
            {
                if (int.TryParse(Console.ReadLine(), out input))
                {
                    if (await _bll.CheckCountryID(input))
                    {
                        break;
                    }
                    else
                    {
                        count--;
                        if (count == 0)
                        {
                            Console.Beep();
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("maximum retries exceeded!");
                            Console.ResetColor();
                            return null;
                        }
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"CountryID Is Not Found :-( Please try again with numeric number between 1 to 5 ? you have {count} more tries");
                        Console.ResetColor();
                        continue;
                    }
                }
                else
                {
                    count--;
                    if (count == 0)
                    {
                        Console.Beep();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("maximum retries exceeded");
                        Console.ResetColor();
                        return null;
                    }
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"wrong input please try again with numeric number between 1 to 5 ? you have {count}  more tries");
                    Console.ResetColor();
                }
            }
            return new ClsContactModel()
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = phone,
                Address = address,
                CountryID = input
            };
        }
        private int? AnotherContact(string Message)
        {
            byte count = 3;
            byte Answer = 0;

            Console.WriteLine($"do you want to {Message} another one yes[1] or no[2] or any number to cancel");

            while (!byte.TryParse(Console.ReadLine(), out Answer))
            {
                count--;
                if (count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Maximum Retries Exceeded");
                    Console.ResetColor();
                    return null;
                }
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"invalid input please try again with numeric number, you have {count}  more tries !");
                Console.ResetColor();
            }
            if (Answer == 1 || Answer == 2)
            {
                return Answer;
            }
            return null;
        }
        public async Task AddData()
        {
            List<ClsContactModel> list = new List<ClsContactModel>();
            bool IsInputValid = true;

            RecordScreen("Add");
            while (true)
            {
                ClsContactModel contact = await ReadSingleContactInput();

                if (contact == null)
                {
                    IsInputValid = false;
                    break;
                }

                list.Add(contact);

                int? UserChoic = AnotherContact("Add");

                if (UserChoic == 1)
                {
                    continue;
                }
                else if (UserChoic == 2)
                {
                    break;
                }
                else
                {
                    IsInputValid = false;
                    break;
                }
            }

            if (IsInputValid && list.Count > 0)
            {
                List<ClsContactModel> SuccessfullyAddedList = await _bll.AddBulkContacts(list);

                if (SuccessfullyAddedList.Any())
                {
                    Console.WriteLine($"Added successfully \n");
                    PrintAllContacts(SuccessfullyAddedList);
                }

                else
                {
                    Console.WriteLine("Failed to add the contact");
                }
            }

            else
            {
                Console.WriteLine("Operation Canceled");
            }
        }

        public async Task<int?> GetUserInput()
        {
            //Console.WriteLine("============================");
            Console.WriteLine("Please Enter The Contact ID ");
            int input = 0;
            byte count = 3;
            while (count > 0)
            {
                if (int.TryParse(Console.ReadLine(), out input))
                {
                    if (await _bll.CheckContactID(input))
                    {
                        break;
                    }
                    else
                    {
                        count--;
                        if (count == 0)
                        {
                            Console.Beep();
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Maximum Retries Exceeded,");
                            Console.ResetColor();
                            return null;
                        }
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"ContactID Is Not Found :-( please try again with numeric number  you have {count} more tries");
                        Console.ResetColor();
                        continue;
                    }
                }
                else
                {
                    count--;
                    if (count == 0)
                    {
                        Console.Beep();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Maximum Retries Exceeded,");
                        Console.ResetColor();
                        return null;
                    }
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Wrong input please try again with numeric number you have {count}  more tries");
                    Console.ResetColor();
                }
            }
            return input;
        }

        public async Task<ClsContactModel> FindContact()
        {
            int? input = await GetUserInput();
            if (input == null) { Console.WriteLine("Operation Canceled"); return null; }

            ClsContactModel contact = await _bll.FindContactByID(input);
            PrintSingleContact(contact);
            return contact;
        }

        public async Task DisplayFirstName()
        {
            int? input = await GetUserInput();
            if (input == null) { Console.WriteLine("Operation Canceled"); return; }

            var Result = await _bll.GetFirstnameById(input);

            if (Result.Exists == false)
            {
                Console.WriteLine("No Record Found In database");
                Console.WriteLine("====================================");
            }
            else if (Result.IsNull == true)
            {
                Console.WriteLine("the record is null in the database");
                Console.WriteLine("====================================");
            }
            else
            {
                Console.WriteLine(Result.Value);
                Console.WriteLine("====================================");
            }
        }
        private void RecordScreen(string message)
        {
            Console.WriteLine($"{message} Record Screen");
            Console.WriteLine("========================");
        }

        public async Task UpdateRecord()
        {
            ClsContactModel contact, contactupdate;
            List<ClsContactModel> contactlists = new List<ClsContactModel>();
            bool IsInputValid = false;

            RecordScreen("Update");
            while (true)
            {
                contact = await FindContact();
                if (contact == null) { return; }

                contactupdate = await ReadSingleContactInput();
                if (contactupdate == null) { break; }

                contactupdate.ContactID = contact.ContactID;
                contactlists.Add(contactupdate);

                int? UserChoice = AnotherContact("Update");
                if (UserChoice == 1) { continue; }
                else if (UserChoice == 2) { IsInputValid = true; break; }
                else { break; }
            }

            if (IsInputValid && contactlists.Count > 0)
            {
                List<ClsContactModel> SuccessfulllyUpdated = await _bll.UpdateBulkContacts(contactlists);

                if (SuccessfulllyUpdated.Any())
                {
                    Console.WriteLine("Record Updated Successfully");
                    Console.WriteLine("========================");
                    PrintAllContacts(SuccessfulllyUpdated);
                }
                else
                {
                    Console.WriteLine("Failed To Update the Contact");
                }
            }
            else
            {
                Console.WriteLine("Operation canceled ");
            }
        }

        public async Task DeleteContacts()
        {
            List<int> contactlist = new List<int>();
            ClsContactModel contact;
            bool IsInputValid = false;

            RecordScreen("Delet");
            while (true)
            {
                contact = await FindContact();
                if (contact == null) { return; }

                contactlist.Add(contact.ContactID.Value);

                int? CheckInput = AnotherContact("delete");
                if (CheckInput == 1) { continue; }

                else if (CheckInput == 2) { IsInputValid = true; break; }

                else { break; }
            }

            if (IsInputValid && contactlist.Count > 0)
            {
                List<ClsContactModel> SuccessfullyDeleted = await _bll.DeleteBulkContacts(contactlist);

                if (SuccessfullyDeleted.Any())
                {
                    Console.WriteLine("\nRecord Deleted Successfully");
                    Console.WriteLine("===========================");
                    PrintAllContacts(SuccessfullyDeleted);
                }
                else
                {
                    Console.WriteLine("Failed To Delete The Contacts");
                }
            }
            else
            {
                Console.Beep();
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Operation Canceld");
                Console.ResetColor();
            }
        }

        public static async Task LogErrorSafely(Exception ex, string UserMessage)
        {
            try
            {
                await clsContactBusiness.Errors(ex);
            }
            catch (Exception logex)
            {
                string LogFilePath = "Emergency_Error_log.txt";

                File.AppendAllText(LogFilePath, $"\n{DateTime.Now}: {ex.Message} | LogError{logex.Message}");

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(LogFilePath)
                {
                    UseShellExecute = true
                });
            }
            Console.WriteLine(UserMessage);
        }
    }

    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                clsContactBusiness contact = new clsContactBusiness();
                ClsContactConsolUI ConsolUI = new ClsContactConsolUI(contact);
                List<ClsContactModel> list = new List<ClsContactModel>();

                Console.WriteLine("GetAllContacts\n");
                list = await contact.GetAllContacts(new ClsContactSearchFilter { });
                ConsolUI.PrintAllContacts(list);

                Console.WriteLine("\nGetAllContactsByContactId [1]\n");
                list = await contact.GetAllContacts(new ClsContactSearchFilter { Contactid = 1 });
                ConsolUI.PrintAllContacts(list);

                Console.WriteLine("\nGetAllContactsByFirstName [jane]\n");
                list = await contact.GetAllContacts(new ClsContactSearchFilter { Firstname = "jane" });
                ConsolUI.PrintAllContacts(list);

                Console.WriteLine("\nGetAllContactsByCoutnryId [5]\n");
                list = await contact.GetAllContacts(new ClsContactSearchFilter { Countryid = 5 });
                ConsolUI.PrintAllContacts(list);

                Console.WriteLine("\nGetAllContactsStartstWith [a]");
                list = await contact.GetAllContacts
                    (new ClsContactSearchFilter { Firstname = "a", EnSearchMode = EnLetter.Firstletter });
                ConsolUI.PrintAllContacts(list);

                Console.WriteLine("\nGetAllContactsEndsWith [d]\n");
                list = await contact.GetAllContacts
                    (new ClsContactSearchFilter { Firstname = "d", EnSearchMode = EnLetter.Lastletter });
                ConsolUI.PrintAllContacts(list);

                Console.WriteLine("\nGetAllContactsContains[o]\n");
                list = await contact.GetAllContacts
                    (new ClsContactSearchFilter { Firstname = "o", EnSearchMode = EnLetter.Anywhere });
                ConsolUI.PrintAllContacts(list);

                Console.WriteLine("\nGetFirstnameById\n");
                await ConsolUI.DisplayFirstName();

                Console.WriteLine("\nFindContactByID\n");
                await ConsolUI.FindContact();

                Console.WriteLine("\nAdd Bulk Contacts\n");
                await ConsolUI.AddData();

                Console.WriteLine("\nUpdate Bulk Contacts\n");
                await ConsolUI.UpdateRecord();

                Console.WriteLine("\nDelete Bulk Contacts\n");
                await ConsolUI.DeleteContacts();

            }
            catch (SqlException sqex)
            {
                await ClsContactConsolUI.LogErrorSafely(sqex, "Sorry, An Unexpected DataBase Error Occurred Please Try Again Later");
            }
            catch (Exception ex)
            {
                await ClsContactConsolUI.LogErrorSafely(ex, "Sorry, An Unexpected Application Error Occurred. Please Try Again Later.");
            }
        }
    }
}
