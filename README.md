# Enterprise Data Access Layer (DAL) for SQL Server & DB2 ⋆｡°✩☼

## 📍 Overview ────────
This **C# Data Access Layer (DAL)** is a modular, reusable library designed to simplify and standardize database interactions across enterprise .NET applications.  
It provides **robust, transaction-safe CRUD operations** for both **SQL Server** and **DB2**, supporting single queries, multi-query transactions, and parametrized commands.  

The DAL is built with **extensibility and maintainability in mind**, providing interface abstractions, helper utilities, and planned integration points for centralized logging and resource management.

---

## 📍 Core Features ────────
- **Single-query operations:** Efficient `SELECT`, `INSERT`, `UPDATE`, `DELETE`, 'SCALAR' and stored procedure execution.  
- **Multi-query & cross-database transactions:** Execute multiple queries on SQL Server and DB2 within a coordinated transaction, ensuring atomicity with commit and rollback.. 
- **Parametrized queries:** Safe, flexible query execution using `Dictionary<string, object> parameters = null` to prevent SQL injection and support flexible, safe, and optional parameterization.  
- **Configurable initialization:** Supports direct connection overrides, `IConfiguration` (`appsettings.json`), and legacy `App.config`.  
- **Transaction handling:** Private methods automatically manage transactions for internal operations.  
Public methods allow developers to provide their own transactions if needed for advanced scenarios.  
- **Fallback & defaults:** Automatic development-friendly connection string defaults to prevent runtime errors.  
- **Error handling & logging:** Integrated console logging with planned modular, interface-based logging system (`ILogHelper`) for project-wide use.  
- **Helpers & Utilities:** Centralized reusable helpers (`DbUtils`, `ParameterHelper`) for query preparation, parameter handling, and database operations.  
- **Exception management:** Dedicated, interface-driven exception classes (`IDalException`, `DalException`) to ensure consistent error handling across projects.  
- **Planned IDisposable support:** Modular resource management for future scenarios requiring persistent connections or other managed resources.

---

## 📍 Architecture ────────
- **Sealed `DbConnectionLayer`:** Encapsulates all database operations, prevents inheritance, and centralizes connection management.
- **Interface `IDBConnectionLayer`:** Abstracts DAL functionality for mocking, unit testing, and multi-project reuse.  
- **Private helper methods:** Internal low-level implementations (`DoOneSelectSQL`, `DoSQL`, `DoAllDB2_SQL`) provide clean structure, maintain separation of concerns and handle transactions automatically if not externally provided..  
- **Transaction-safe operations:** Automatic `commit`/`rollback` ensures database integrity for multi-query and mixed-database operations.  
- **Configurable connection initialization:** Handles connection sources from multiple environments (`IConfiguration`, App.config, JSON fallback, direct overrides).  
- **Helpers Layer:** Utility classes for common operations and parameter management to ensure DRY principles.  
- **Exception Layer:** Interface-based exceptions for clean, predictable error propagation.

DataAccessLayer/
│
├── ConnectionLayer/
│   └── DbConnectionLayer.cs      # Sealed class principale, with all CRUD e multi-query methods.
│
├── Interfaces/
│   └── IDBConnectionLayer.cs     # Interface for DbConnectionLayer
│   └── IOtherInterfaces.cs       # Placeholder for future interface (logging, helpers, etc.) <--- maybe. Could be better to keep them de-centralized for better mental order. 
├── Log/
│   └── ILogHelper.cs             # logging modular interface
│   └── LogHelper.cs              # base implementation, exstensible. 
│
├── Exceptions/
│   └── IDalException.cs          # Modular logging interface for DAL exceptions.
│   └── DalException.cs           # concrete class to manage exceptions.
│
├── Helpers/
│   └── DbUtils.cs                # generic utility for database (for future implementation)
│   └── ParameterHelper.cs        # Helpers for parameters management and QoL.
│
├── appsettings.json              # connection configuration (optional. Mostly App.config because without i can't code the behaviour for the connection.)
├── App.config
└── DataAccessLayer.csproj

---

## 📍 Tech Stack ────────
- **Language:** C# 7.3  
- **Framework:** .NET Library 2.0 for optimal implementation across all .NET from Framework to Core.   
- **Databases:** SQL Server (`System.Data.SqlClient`), DB2 (`System.Data.OleDb` or `IBM.Data.DB2.Core`)  
- **Configuration Management:** `App.config` for legacy, `appsettings.json` via `Microsoft.Extensions.Configuration`  
- **Transaction Management:** `SqlTransaction` and `OleDbTransaction` with robust rollback support  
- **Planned Extensions:**  
  - Interface-based logging (`ILogHelper`)  
  - Modular `IDisposable` for resource management  

---

## 📍 Development Status ────────

| Feature | Status |
|---------|--------|
| Single-query CRUD for SQL & DB2 | ✅ Complete |
| Parametrized query support | ✅ Complete |
| Multi-query transactions (`DoAllDB2_SQL`) | ✅ Complete |
| Configurable initialization (`IConfiguration`, App.config, JSON) | ✅ Complete |
| Helpers & Utilities (`DbUtils`, `ParameterHelper`) | ✅ Complete |
| Exception Layer (`IDalException`, `DalException`) | ✅ Complete |
| Modular IDisposable support | 🔄 Planned |
| Interface-based logging (`ILogHelper`) | 🔄 Planned |

---

## 📍 License ────────

Apache 2.0 License
