# 🧹 Digital Janitor: Cross-Platform File Organizer

**Digital Janitor** is a high-performance background service built with **.NET 8** that monitors specific directories (like `Downloads` or `Desktop`) and automatically organizes files into categorized folders based on their extensions and creation dates.

## 🚀 Key Features

* **Real-time Monitoring:** Uses `FileSystemWatcher` for event-driven organization (zero-latency).
* **Smart Categorization:** Automatically sorts files (e.g., `.pdf` to `Documents`, `.jpg` to `Images`).
* **Temporal Sorting:** Creates a hierarchical structure: `Category/Year/Month/file.ext`.
* **Duplicate Handling:** Automatically renames conflicting files (e.g., `resume.pdf` becomes `resume (1).pdf`).
* **Robust I/O:** Implements retry logic to handle files locked by browsers or the OS during download.
* **Cross-Platform:** Native support for **Linux Mint (systemd)** and **Windows Services**.

---

## 🛠️ Architecture & Design Patterns

This project follows **SOLID** principles to ensure maintainability and testability:

* **Dependency Inversion (D):** High-level logic depends on `IFileSystem` abstractions rather than static `System.IO` calls, making the code decoupled from the hardware.
* **Single Responsibility (S):** Separate concerns for configuration (`JanitorSettings`), file operations (`PhysicalFileSystem`), and background orchestration (`Worker`).
* **Options Pattern:** Uses `IOptions<T>` for type-safe configuration via `appsettings.json`.



---

## 🧪 Unit Testing

The project includes a comprehensive test suite using **xUnit** and **Moq**.

* **Mocking:** The file system is mocked to verify organization logic without performing actual disk I/O.
* **Edge Case Testing:** Tests cover date-path generation, category mapping, and "file-ready" verification (handling file locks).

**To run tests:**
```bash
dotnet test