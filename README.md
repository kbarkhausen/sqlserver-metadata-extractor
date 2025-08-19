# SQL Metadata Extraction Tool

This is a .NET 8 console application that extracts detailed metadata from a SQL Server database and exports it into a structured, human-readable **Markdown file**. The extracted metadata includes:

- Table names and descriptions
- Column names, data types, nullability, identity
- Default values
- Extended comments (via `MS_Description`)
- Primary keys
- Foreign keys
- Indexes

This metadata is useful for:
- Documentation
- Onboarding new developers
- Feeding structured data to a GPT model
- Schema review or audit processes

---

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/your-org/SqlMetadataExtraction.git
cd SqlMetadataExtraction
````

### 2. Configure the App

Update `appsettings.json` with your SQL Server connection string and filtering rules:

```json
{
  "ConnectionStrings": {
    "Default": "Server=yourserver;Database=yourdb;User ID=your_user;Password=your_password;"
  },
  "MetadataOptions": {
    "OutputPath": "metadata.md",
    "IncludePattern": "src[_]%",
    "ExcludePatterns": [
      "src_fayear",
      "%[_]bak",
      "%[_]work",
      "%[_]Backup"
    ]
  }
}
```

### 3. Run the App

```bash
dotnet run
```

The Markdown output will be saved to the file specified in `OutputPath`.

---

## ✍️ Adding Comments to SQL Tables and Columns

To enrich the metadata, you can add descriptions directly in your SQL database using `sp_addextendedproperty`. These are pulled automatically by the extractor.

### Add a Table Comment

```sql
EXEC sp_addextendedproperty 
  @name = N'MS_Description', 
  @value = N'Describes different types of student engagement activities.', 
  @level0type = N'SCHEMA', @level0name = 'dbo', 
  @level1type = N'TABLE',  @level1name = 'src_ActivitiesList';
```

### Add a Column Comment

```sql
EXEC sp_addextendedproperty 
  @name = N'MS_Description', 
  @value = N'Indicates whether the activity is active (1) or inactive (0).', 
  @level0type = N'SCHEMA', @level0name = 'dbo', 
  @level1type = N'TABLE',  @level1name = 'src_ActivitiesList', 
  @level2type = N'COLUMN', @level2name = 'Active';
```

Use `sp_updateextendedproperty` to modify, or `sp_dropextendedproperty` to remove.

---

## 📄 Sample Output (Markdown)

Here is an example of the Markdown generated for one table:

```markdown
## Table: src_ActivitiesList
- **Description**: Defines activity types and default configurations for assignments, contact types, and status tracking.
- **Columns:**
  - `Id` (int, NOT NULL, IDENTITY) -- Unique identifier for this activity type record.
  - `Active` (bit, NOT NULL) -- Indicates whether the activity type is active (1) or inactive (0).
  - `ActivityCategoryDescrip` (varchar(40), NOT NULL) -- Description of the category the activity belongs to (e.g., Advising, Career, Incident).
  - `IncludeInETL` (bit, NOT NULL, DEFAULT ((1))) -- Flag indicating whether to include this activity in the ETL process.
- **Primary Keys:**
  - `Id`
- **Foreign Keys:**
  - `FK_AssignToStaff` : `AssignToStaffCode` → `src_Staff.StaffCode`
- **Indexes:**
  - `IX_ActiveCategory` : [NON‑UNIQUE] Active, ActivityCategoryDescrip
```

---

## 🛠️ Technologies Used

* .NET 8.0
* Microsoft.Data.SqlClient
* Microsoft.Extensions.Configuration
* SQL Server extended properties

---

## 👥 Contributors

Built by the Klaus Barkhausen
