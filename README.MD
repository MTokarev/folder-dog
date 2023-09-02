# Overview
![super dog](src/FolderDog/static/superdog.png)

Do you need to listen for file creation events and send files as attachments via Email? Folder Dog is ready to help.

If you like this app, please give it a star ⭐️

## How it works
It binds to OS file events and subscribes specifically to file creation events.

## How the app notifies
It uses an SMTP server. You need to open a relay from the IP where this app is running. You can provide the message subject, send from address, and recipient information such as `SendTos`, `SendCcs`, and `SendBccs`.

## How do I know that the app is working
Once you run the app, it will log messages to both the console and to the file log. By default, it creates a `log` directory in the current folder and every day creates a log file. The log files are rotated for 31 days by default.

## Handle edge cases
You can control how to handle edge cases such as:
- A file is still open by another process (Some apps might write content by batches).
- We had already processed the file before, but now received the same event. You can control what to do in this case.

Also, you can provide one or many file extensions to listen to. For example, only excel or csv files.

## Multi threading in the app
Each file creation event executes in a dedicated thread from the `ThreadPool`. If all thread pools are busy, then the event will be queued and processed later.

## Configuration
Configuration file `appsettings.json` must be present in the same app with folder.
Example:
```
{
  "Binding": {
    "FolderPath": "./",
    "FileExtensions": [
      "csv",
      "txt"
    ]
  },

  "FileService": {
    "RepeatAccessAttempts": 3,
    "WaitUntilNextRetryInMilliseconds": 1000,
    "SkipProcessedFiles": true
  },

  "Email": {
    "SendFrom": "no-reply@mail.com",
    "SendTos": [
      "user@mail.com"
    ],
    "SmtpServerIpv4Address": "<PutYourIpv4SmtpServerAddressHere>",
    "SmtpServerPortNumber": 25,
    "MessageSubject": "File has been created"
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "./logs/log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 31
        }
      },
      {
        "Name": "Console"
      }
    ]
  }
}
```
- Bindings - provide folder path where to listen as well as file extensions.
- FileService - configuration for the file service to control how to handle edge cases.
- Email - Mail configuration
- Serilog - Logging configuration

## Prerequisites
- .NET 7 runtime if you don't use the self-contained version
- The SMTP server must relay emails from the IP where the app runs

## Feedback
Please leave [feedback](https://github.com/MTokarev/folder-dog/discussions) to make this small app better ❤️.
