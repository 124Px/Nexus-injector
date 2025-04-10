# ![Nexus Injecteur](https://raw.githubusercontent.com/124Px/Nexus-injector/refs/heads/main/Resources/nexus%20banner.png)

# Nexus Injecteur - An advanced Dll Injector

## Overview
Nexus Injecteur is a powerful and regularly updated DLL injection tool, designed to support multiple injection techniques. This tool allows developers and security researchers to study process injection methods and explore various techniques used in modern software manipulation.

> **Disclaimer** ⚠️
> This tool is intended for educational and research purposes only. The misuse of this software for malicious purposes is strictly prohibited.

## Features 🚀
- Multiple injection methods ✅
- Modern UI using **Bunifu** framework 🎨
- Coded in **C#** for performance and flexibility 💻
- Open-source and accepting contributions 🤝

---

## Injection Methods 🧩
Nexus Injecteur supports multiple injection techniques, each with its own approach and use case:

### 1️⃣ LoadLibrary Injection
This is the most common injection method. It works by:
- Opening the target process.
- Allocating memory for the DLL path.
- Writing the DLL path into the allocated memory.
- Creating a remote thread to call `LoadLibrary` with the DLL path.

> **Pros:** Simple, reliable, and works on most applications.
> **Cons:** Easily detected by anti-cheats and security solutions.

---

### 2️⃣ WriteProcessMemory + CreateRemoteThread
This technique involves manually writing the DLL into the process and executing it:
- Opens the target process.
- Allocates memory in the process.
- Writes the DLL payload using `WriteProcessMemory`.
- Executes the payload with `CreateRemoteThread`.

> **Pros:** Straightforward and efficient.
> **Cons:** Often flagged by security software.

---

### 3️⃣ SetWindowsHookEx Injection
This method leverages Windows hooks to inject a DLL into a process:
- A global hook is installed using `SetWindowsHookEx`.
- The DLL is loaded when the hook callback is triggered.

> **Pros:** Useful for GUI-based applications.
> **Cons:** Requires user interaction to trigger the hook.

---

### 4️⃣ Manual Mapping
Manual mapping loads a DLL without registering it in the PEB:
- The DLL is manually loaded into memory.
- Dependencies are resolved manually.
- Execution is performed without using `LoadLibrary`.

> **Pros:** Harder to detect, avoids API hooking.
> **Cons:** Requires deeper understanding of PE structures.

---

### 5️⃣ Process Hollowing
A stealthier technique used to replace the memory of a legitimate process:
- A new process is created in a suspended state.
- The legitimate executable is replaced with the malicious payload.
- Execution is resumed with the injected payload.

> **Pros:** Highly stealthy, used by advanced malware.
> **Cons:** More complex to implement.

---

## Technical Details ⚙️
- **Language:** C#
- **UI Framework:** Bunifu
- **Target:** Windows OS
- **Security Bypass:** Focus on avoiding traditional detection methods

## Contribution ✨
Contributions are always welcome! Feel free to submit pull requests or report any issues to improve the project.

## Changelog 📌
### v1.0 (Initial Release)
- Added LoadLibrary injection
- Implemented WriteProcessMemory + CreateRemoteThread
- Integrated SetWindowsHookEx method
- Developed Manual Mapping technique
- Added Process Hollowing support

## License 📜
This project is licensed under the **MIT License**. Use responsibly!

---

![Nexus Injecteur Logo](https://cdn.discordapp.com/attachments/1308922381754040340/1339996458782556222/Picsart_25-02-14_17-27-24-052.png?ex=67b2bab3&is=67b16933&hm=f6ffe7634da774df8df1948324343325f49221066ff94b73c5c290fa8adc5423&)

