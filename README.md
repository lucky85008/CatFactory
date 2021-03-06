# CatFactory ==^^==

## What Is CatFactory?

CatFactory it's a scaffolding engine for .NET Core built in C#.

## How it Works?

The key thing in CatFactory it's import an existing database from a SQL Server instance, then scaffold code for specific technology.

In some cases we can replace the database from SQL Server instance with a in memory database.

In this section, we'll use sample for Entity Framework Core.

```csharp
// Create database factory
var databaseFactory = new SqlServerDatabaseFactory
{
	DatabaseImportSettings = new DatabaseImportSettings
	{
		ConnectionString = "server=(local);database=Store;integrated security=yes;",
		Exclusions =
		{
			"dbo.sysdiagrams"
		}
	}
};

// Import database
var database = databaseFactory.Import();

// Create instance of Entity Framework Core project
var project = new EntityFrameworkCoreProject
{
	Name = "Store",
	Database = database,
	OutputDirectory = "C:\\Projects\\Store"
};

// Apply settings for Entity Framework Core project
project.GlobalSelection(settings =>
{
	settings.ForceOverwrite = true;
	settings.AuditEntity = new AuditEntity("CreationUser", "CreationDateTime", "LastUpdateUser", "LastUpdateDateTime");
	settings.ConcurrencyToken = "Timestamp";
});

project.Select("Sales.Order", settings => settings.EntitiesWithDataContracts = true);

// Build features for project, group all entities by schema into a feature
project.BuildFeatures();

// Add event handlers to before and after of scaffold

project.ScaffoldingDefinition += (source, args) =>
{
	// Add code to perform operations with code builder instance before to create code file
};

project.ScaffoldedDefinition += (source, args) =>
{
	// Add code to perform operations after of create code file
};

// Scaffolding =^^=
project
	.ScaffoldEntityLayer()
	.ScaffoldDataLayer();
```

To understand the scope for CatFactory, in few words CatFactory is the core, to have more packages we can create them with this naming convention: CatFactory.PackageName.

In the sample code, the basic flow for existing database is:

* Create Database Factory
* Import Database
* Create instance of Project (Entity Framework Core, Dapper, etc)
* Build Features (One feature per schema)
* Scaffold objects, these methods read all objects from database and create instances for code builders

## Concepts behind CatFactory

### Database Type Map

One of things I don't like to get equivalent between SQL data type for CLR is use magic strings, after of review the more "fancy" way to resolve a type equivalence is to have a class that allows to know the equivalence between SQL data type and CLR type.

Using this table as reference, now CatFactory has a class with name DatabaseTypeMap. Database class contains a property with all mappings with name Mappings, so this property is filled by Import feature for SQL Server package.

```csharp
public class DatabaseTypeMap
{
        public string DatabaseType { get; set; }
        
        public bool AllowsLengthInDeclaration { get; set; }
        
        public bool AllowsPrecInDeclaration { get; set; }
        
        public bool AllowsScaleInDeclaration { get; set; }
        
        public string ClrFullNameType { get; set; }
        
        public bool HasClrFullNameType { get; }
        
        public string ClrAliasType { get; set; }
        
        public bool HasClrAliasType { get; }
        
        public bool AllowClrNullable { get; set; }
        
        public DbType DbTypeEnum { get; set; }
        
        public bool IsUserDefined { get; set; }
        
        public string ParentDatabaseType { get; set; }
        
        public string Collation { get; set; }
}
```

DatabaseTypeMap is the class to represent database type definition, for database instance we need to create a collection of DatabaseTypeMap class to have a matrix to resolve data types.

This concept was created from this link: [`SQL Server Data Type Mappings`](https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-data-type-mappings)

Suppose there is a class with name DatabaseTypeMapList, this class has a property to get data types. Once we have imported an existing database we can resolve data types:

Resolve without extension methods:

```csharp
// Get mappings
var dataTypes = database.DatabaseTypeMaps;

// Resolve CLR type
var mapsForString = dataTypes.Where(item => item.ClrType == typeof(string)).ToList();

// Resolve SQL Server type
var mapForVarchar = dataTypes.FirstOrDefault(item => item.DatabaseType == "varchar");
```

Resolve with extension methods:

```csharp
// Get database type
var varcharDataType = database.ResolveType("varchar");

// Resolve CLR
var mapForVarchar = varcharDataType.GetClrType();
```

SQL Server allows to define data types, suppose the database instance has a data type defined by user with name Flag, Flag data type is a bit, bool in C#. Import method retrieve user data types, so in DatabaseTypeMaps collection we can search the parent data type for Flag:

```csharp
// Get Flag data type
var flagDataType = database.DatabaseTypeMaps.FirstOrDefault(item => item.DatabaseType == "Flag");

// Get parent data type for Flag
var flagParentDataType.GetParentType(database.DatabaseTypeMaps);
```

### Project Selection

A project selection is a limit to apply settings for objects that match with pattern.

GlobalSelection is the default selection for project, contains a default instance of settings.

Patterns:

|Pattern|Scope|
|-------|-----|
|Sales.Order|Applies for specific object with name Sales.Order|
|Sales.\*|Applies for all objects inside of Sales schema|
|\*.Order|Applies for all objects with name Order with no matter schema|
|\*.\*|Applies for all objects, this is the global selection|

Sample:

```csharp
// Apply settings for Project
project.GlobalSelection(settings =>
{
    settings.ForceOverwrite = true;
    settings.AuditEntity = new AuditEntity("CreationUser", "CreationDateTime", "LastUpdateUser", "LastUpdateDateTime");
    settings.ConcurrencyToken = "Timestamp";
});

// Apply settings for specific object
project.Select("Sales.Order", settings =>
{
    settings.ForceOverwrite = true;
    settings.AuditEntity = new AuditEntity("CreationUser", "CreationDateTime", "LastUpdateUser", "LastUpdateDateTime");
    settings.ConcurrencyToken = "Timestamp";
    settings.EntitiesWithDataContracts = true;
});
```

### Event Handlers to Scaffold

In order to provide a more flexible way to scaffold, there are two delegates in CatFactory, one to perform an action before of scaffolding and another one to handle and action after of scaffolding.

```csharp
// Add event handlers to before and after of scaffold

project.ScaffoldingDefinition += (source, args) =>
{
    // Add code to perform operations with code builder instance before to create code file
};

project.ScaffoldedDefinition += (source, args) =>
{
    // Add code to perform operations after of create code file
};
```

## Packages

### CatFactory

This package provides all definitions for CatFactory engine, this is the core for child packages.

#### Namespaces

CodeFactory: Contains objects to perform code generation.

Diagnostics: Contains objects for diagnostics.

Mapping: Contains objects for ORM.

Markup: Contains objects for markup languages.

OOP: Contains objects to modeling definitions.

### CatFactory.SqlServer

This packages contains logic to import existing databases from SQL Server instances.

|Object|Supported|
|------|---------|
|Tables|Yes|
|Views|Yes|
|Scalar Functions|Yes|
|Table Functions|Yes|
|Stored Procedures|Yes|
|Sequences|Not yet|
|Extended Properties|Yes|
|Data types|Yes|

### CatFactory.NetCore

This package contains code builders and definitions for .NET Core (C#).

|Object|Feature|Supported|
|------|-------|---------|
|Interface|Inheritance|Yes|
|Interface|Events|Yes|
|Interface|Properties|Yes|
|Interface|Methods|Yes|
|Class|Inheritance|Yes|
|Class|Events|Yes|
|Class|Fields|Yes|
|Class|Constructors|Yes|
|Class|Properties|Yes|
|Class|Methods|Yes|
|Enum|Options|Yes|
|Struct|All|Not yet|

### CatFactory.EntityFrameworkCore

This package provides scaffolding for Entity Framework Core.

|Category|Compatibility Chart|Supported|
|--------|-------------------|---------|
|Modeling|Table splitting|Not yet|
|Modeling|Owned types|Not yet|
|Modeling|Model-level query filters|Not yet|
|Modeling|Database scalar function mapping|Not yet|
|Modeling|Self-contained type configuration for code first|Not yet|
|High Performance|DbContext pooling|Not yet|
|High Performance|Explicitly compiled queries|Not yet|

[`New features in EF Core 2.0`](https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-2.0)

### CatFactory.AspNetCore

This package provides scaffolding for Asp .NET Core.

|Feature|Supported|
|-------|---------|
|Controllers|Yes|
|Requests|Yes|
|Responses|Yes|
|Scaffold Client|Not yet|
|Unit Tests|Not yet|
|Integration Tests|Not yet|

### CatFactory.Dapper

This package provides scaffolding for Dapper.

|Object|Supported|
|------|---------|
|Tables|Yes|
|Views|Yes|
|Scalar Functions|Yes|
|Table Functions|Yes|
|Stored Procedures|Yes|
|Sequences|Yes|

### CatFactory.TypeScript

This package provides scaffolding for Type Script.

|Object|Feature|Supported|
|------|-------|---------|
|Interface|Inheritance|Yes|
|Interface|Fields|Yes|
|Interface|Properties|Yes|
|Interface|Methods|Yes|
|Class|Inheritance|Yes|
|Class|Fields|Yes|
|Class|Constructor|Yes|
|Class|Properties|Yes|
|Class|Methods|Yes|
|Module|Methods|Yes|

## History

In 2005 year, I was on my college days and I worked on my final project that included a lot of tables, for those days C# didn't have automatic properties also I worked on store procedures that included a lot of columns, I thought if there was a way to generate all that code because it was repetitive and I wasted time in wrote a lot of code.

In 2006 beggining I've worked for a company and I worked in a prototype to generate code but I didn't have experience and I was a junior developer, so I developed a version in WebForms that didn't allow to save the structure ha,ha,ha that project it was my first project in C# because I came from VB world but I bought a book about Web Services in DotNet and that book used C# code, that was new for me but it got me a very important idea, learn C# and I wrote all first code generation form in C#.

Later, there was a prototype of Entity for SQL, the grandfather of entity framework and I develop a simple ORM because I had table class and other classes such as Column, so after of reviewed Entity for SQL I decided to add the logic to read database and provide a simple way to read the database also of code generation.

In 2008 I built the first ORM based on my code generation engine, in that time it was called F4N1, I worked on an ORM must endure different databases engines such as SQL Server, Sybase and Oracle; so I generated a lot of classes with that engine, for that time the automated unit tests did not exist, I had a webform page that generated that code ha,ha,ha I know it was ugly and crappy but in that time that was my knowledge allowed me.

In 2011 I worked on a demo for a person that worked in his company and that person used another tool for code generation, so my code generation engine wasn't use for his work.

In 2012 I worked for a company needed to rebuilt all system with new technologies (ASP.NET MVC and Entity Framework) so I invested time about MVC and EF learning but as usual, there isn't time for that ha,ha,ha and again my code generation it wasn't considered for that upgrade =(

In 2014, I thought to make a nuget package to my code generation but in those days I didn't have the focus to accomplish that feature and always I used my code generation as a private tool, in some cases I shared my tool with some coworkers to generate code and reduce the time for code writing.

In 2016, I decided to create a nuget package and integrates with EF Core, using all experience from 10 years ago :D Please remember that from the beginning I was continuing improve the way of code generation, my first code was a crap but with the timeline I've improved the design and naming for objects.

Why I named CatFactory? It was I had a cat, her name was Mindy and that cat had manny kittens (sons), so the basic idea it was the code generation engine generates the code as fast Mindy provided kittens ha,ha,ha

## Trivia

* The name for this framework it was F4N1 before than CatFactory
* Framework's name is related to kitties
* Import logic uses sp_help stored procedure to retrieve the database object's definition, I learned that in my database course at college
* Load mapping for entities with MEF, it's inspired in "OdeToCode" (Scott Allen) article for Entity Framework 6.x
* Expose all settings in one class inside of project's definition is inspired on DevExpress settings for Web controls (Web Forms)
* There are three alpha versions for CatFactory as reference for Street Fighter Alpha fighting game.
* There will be two beta versions for CatFactory, the first with name Sun and second one with name Moon as reference for characters from The King of Fighters fighting game: Kusanagi Kyo and Yagami Iori.

## Quick starts

[`Scaffolding Entity Framework Core with CatFactory`](https://www.codeproject.com/Articles/1160615/Scaffolding-Entity-Framework-Core-with-CatFactory)

[`Scaffolding Dapper with CatFactory`](https://www.codeproject.com/Articles/1213355/Scaffolding-Dapper-with-CatFactory)

[`Scaffolding View Models with CatFactory`](https://www.codeproject.com/Tips/1164636/Scaffolding-View-Models-with-CatFactory)
