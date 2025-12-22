# 039 - Packaging and Installation Guide

Hello! Today we are moving from "Developer Mode" to "Distributor Mode." We will learn how to turn your code into a professional installation file that you can send to your clients.

## Table of Contents
1. [Setting the Application Icon](#setting-the-application-icon)
2. [Publishing for Production](#publishing-for-production)
3. [Creating the Installer (The Wizard)](#creating-the-installer-the-wizard)
4. [Distributing your App](#distributing-your-app)

---

## 1. Setting the Application Icon
Before we create the installer, we need to make sure your app looks professional on the Desktop and Taskbar.

### In your .csproj file:
We added this line to your project configuration:
```xml
<ApplicationIcon>Images\Health_Solutions-Icon -2.ico</ApplicationIcon>
```
* **Why?**: This ensures that when you build the project, the resulting `.exe` file itself carries the custom icon you provided.

---

## 2. Publishing for Production
Standard "Builds" (Debug) are for developers. For clients, we use **Publishing**. We have configured your project to produce a **Self-Contained Single File**.

### The Steps:
1. Open your terminal in the project folder.
2. Run this command:
   ```powershell
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
   ```
* **What this does**:
  * `-c Release`: Optimizes the code for speed.
  * `-r win-x64`: Targets 64-bit Windows.
  * `--self-contained true`: Includes the .NET runtime. This means the client **does not** need to install .NET to run your app!
  * `-p:PublishSingleFile=true`: Merges everything into one single `.exe` file.

---

## 3. Creating the Installer (The Wizard)
A single `.exe` is good, but a "Setup Wizard" is better. We will use **Inno Setup**, a industry-standard free tool.

### Using the Script:
### Crucial: The "runtimes" Folder
WPF apps that use databases like SQLite depend on "native" files. These are stored in a folder called `runtimes`. 
- **If you don't include this folder**, the app will crash silently on startup because it can't find the database engine.
- We have updated the script to automatically include this and the `LatoFont` folder using:
  `Source: "...\\runtimes\\*"; DestDir: "{app}\\runtimes"; Flags: recursesubdirs`

1. Download and install [Inno Setup](https://jrsoftware.org/isdl.php).
2. Open the file we created: `Help Documents\Inno-Setup-Script.iss`.
3. Click the **Compile** button (the Play icon) in Inno Setup.
4. It will combine your published `.exe`, icons, and **runtimes** into a single `Output\setup.exe`.

* **What the script handles**:
  * Registry entries for "Add/Remove Programs."
  * Creating Desktop and Start Menu shortcuts.
  * Ensuring the database and dependencies are in the right folder.

---

## 4. Distributing your App
Once the setup is finished, you only need to send the `setup.exe` to your client. They can run it, click "Next, Next, Finish," and they will have your prototype ready to test on their own computer!

Congratulations on completing your first full production-ready deployment!
