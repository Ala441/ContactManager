using ContactManager.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

        public void PrintAllContacts(List<ClsContactModel> Contactlist)
        {
            if (Contactlist == null || Contactlist.Count == 0)
            {
                Console.WriteLine("not found");
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
                Console.WriteLine("not found");
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
            Console.WriteLine("please eneter firstname");
            string firstName = Console.ReadLine();

            Console.WriteLine("please eneter lastname");
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
            bool isCountryValid = false;

            while (count > 0)
            {
                if (int.TryParse(Console.ReadLine(), out input))
                {
                    try
                    {
                        if (await _bll.CheckCountryID(input))
                        {
                            isCountryValid = true;
                            break;
                        }
                    }
                    catch (SqlException ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Database Error: {ex.Message}");
                        Console.ResetColor();
                    }
                }
                if (!isCountryValid)
                {
                    count--;
                    if (count == 0)
                    {
                        Console.Beep();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("maximum retries exceeded, opration canceled");
                        Console.ResetColor();
                        return null;
                    }
                    Console.Beep();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"wrong input please try again with numaric number between 1 t 5 ? you have {count}  more tries");
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
        private int? AddAnotherContact()
        {
            Console.WriteLine("do you want to add another one yes[1] or no[2] or press any number to cancel");

            if (byte.TryParse(Console.ReadLine(), out byte Answer))
            {
                if (Answer == 1 || Answer == 2)
                {
                    return Answer;
                }
            }
            return null;
        }
        public async Task AddData()
        {
            List<ClsContactModel> list = new List<ClsContactModel>();
            bool IsInputValid = true;

            while (true)
            {
                ClsContactModel contact = await ReadSingleContactInput();

                if (contact == null)
                {
                    IsInputValid = false;
                    break;
                }

                list.Add(contact);

                int? UserChoic = AddAnotherContact();

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

                if (SuccessfullyAddedList != null && SuccessfullyAddedList.Count > 0)
                {
                    Console.WriteLine($"added successfully \n");
                    PrintAllContacts(SuccessfullyAddedList);
                }

                else
                {
                    Console.WriteLine("failed to add the contact");
                }
            }

            else
            {
                Console.WriteLine("operation caceled due to invalid input");
            }
        }

        public int? GetUserInput()
        {
            Console.WriteLine("Please Enter The Contact ID ");
            int input;
            byte count = 2;

            while (!int.TryParse(Console.ReadLine(), out input))
            {
                if (count == 0)
                {
                    Console.Beep();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("maximum retries exceeded, opration canceled");
                    Console.ResetColor();
                    return null;
                }
                Console.Beep();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"wrong input please try again with numaric number you have {count}  more tries");
                Console.ResetColor();
                count--;
            }
            return input;
        }

        public async Task FindContact()
        {
            int? input = GetUserInput();

            ClsContactModel contact = await _bll.FindContactByID(input);
            PrintSingleContact(contact);
        }

        public async Task DisplayFirstName()
        {
            int? input = GetUserInput();
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

                Console.WriteLine("GetAllContactsByContactId [1]\n");
                list = await contact.GetAllContacts(new ClsContactSearchFilter { Contactid = 1 });
                ConsolUI.PrintAllContacts(list);

                Console.WriteLine("GetAllContactsByFirstName [jane]\n");
                list = await contact.GetAllContacts(new ClsContactSearchFilter { Firstname = "jane" });
                ConsolUI.PrintAllContacts(list);

                Console.WriteLine("GetAllContactsByCoutryId [5]\n");
                list = await contact.GetAllContacts(new ClsContactSearchFilter { Countryid = 5 });
                ConsolUI.PrintAllContacts(list);

                Console.WriteLine("GetAllContactsStarstWith [e]");
                list = await contact.GetAllContacts
                    (new ClsContactSearchFilter { Firstname = "e", EnSearchMode = EnLetter.Firstletter });
                ConsolUI.PrintAllContacts(list);

                Console.WriteLine("GetAllContactsEndsWith [x]\n");
                list = await contact.GetAllContacts
                    (new ClsContactSearchFilter { Firstname = "x", EnSearchMode = EnLetter.Lastletter });
                ConsolUI.PrintAllContacts(list);

                Console.WriteLine("GetAllContactsContainsWith[z]\n");
                list = await contact.GetAllContacts
                    (new ClsContactSearchFilter { Firstname = "z", EnSearchMode = EnLetter.Anywhere });
                ConsolUI.PrintAllContacts(list);

                Console.WriteLine("GetFirstnameById\n");
                await ConsolUI.DisplayFirstName();

                Console.WriteLine("FindContactByID\n");

                await ConsolUI.FindContact();

                Console.WriteLine("AddBulkContacts\n");
                await ConsolUI.AddData();
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
    }
}
