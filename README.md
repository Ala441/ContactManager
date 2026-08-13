# Contact Manager System 

A robust C# Console Application built with a **3-Tier Architecture** pattern and integrated with **SQL Server** database via **ADO.NET** (using `async/await` asynchronous operations).

---

##  Architecture & Project Structure

The project follows a clean, decoupled 3-Tier Architecture:

* **`ContactManager.UI`**: Handles user input and console interface presentation.
* **`ContactManager.BusinessLogic` (BLL)**: Contains business rules and filters.
* **`ContactManager.DataAccess` (DAL)**: Handles database communication and Stored Procedures execution.
* **`Database`**: Contains SQL script files (`.sql`) for creating tables and stored procedures.

---

##  Features Implemented
- [x] **Add Contact**: Insert new contacts into SQL Server database.
- [x] **Retrieve Contact**: Fetch contacts asynchronously using `async/await`.
- [x] **Advanced Search/Filtering**: Search contacts by:
  - First name starting with specific letter(s).
  - First name ending with specific letter(s).
  - First name containing specific letter(s).
- [x] **Bulk Insert**: Add multiple contacts using Table-Valued Parameters (TVP) or lists.
- [x] **Secure Configuration**: Connection strings sanitized for repository sharing.

---

##  Tech Stack & Concepts
* **Language:** C#
* **Database:** SQL Server (T-SQL, Stored Procedures)
* **Data Access:** ADO.NET (`SqlConnection`, `SqlCommand`, `SqlDataReader`)
* **Asynchronous Programming:** `Task`, `async`, `await`

---

##  Database Setup
1. Navigate to the `/Database` folder in this repository.
2. Open the `ContactQueries.sql` file.
3. Run the script on your local SQL Server instance to create the required database schema and stored procedures.
4. Update the `ConnectionString` in `clsContactData.cs` with your local server credentials.

---

##  Future Roadmap (Under Development)
- [ ] Implement **Update** contact functionality.
- [ ] Implement **Delete** contact functionality.
- [ ] Add data validation rules.
