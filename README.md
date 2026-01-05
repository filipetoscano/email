`lemail`
==========================================================================

[![CI](https://github.com/filipetoscano/email/workflows/CI/badge.svg)](https://github.com/filipetoscano/email/actions)
[![NuGet](https://img.shields.io/nuget/vpre/lefty.email.svg?label=NuGet)](https://www.nuget.org/packages/Lefty.Email/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)

.NET tool to send emails through SMTP, or Resend.


Installing
--------------------------------------------------------------------------

To install the tool as a repository tool, run the following command in
the root of your repository:

```bash
> dotnet tool install --local Lefty.Email --create-manifest-if-needed
```

If you'd rather have the tool installed globally in your machine, run
the following command:

```bash
> dotnet tool install --global Lefty.Email
```


Senders
--------------------------------------------------------------------------

When using `smtp` sender (default):

| Env             | M/O        | Default | Description
|-----------------|------------|---------|--------------------------------
| `SMTP_HOST`     | Mandatory  | *none*  | Hostname or IP address of SMTP server
| `SMTP_PORT`     | Optional   | `587`   | Port number
| `SMTP_SSL`      | Optional   | `True`  | Whether to use SSL
| `SMTP_USERNAME` | Optional   | *null*  | Username for authentication
| `SMTP_PASSWORD` | Required if username specified | null | Password


When using `resend` sender:

| Env               | M/O        | Default | Description
|-------------------|------------|---------|--------------------------------
| `RESEND_APITOKEN` | Mandatory  | *none*  | API token, as obtained from dashboard

```bash
> set EMAIL_SENDER=resend
> set RESEND_APITOKEN=<api>
```


Example
--------------------------------------------------------------------------

The tool accepts a single argument, which is the name of a Json file.

```bash
> cat email.json
{
   "from": "Master Yoda <yoda@dagobah.sw>",
   "to": "Luke Skywalker <lskywalker19@tatooine.sw>",
   "subject": "Reminder",
   "htmlBody": "<h1>Use the force</h1>"
}
> dotnet lemail email.json
```

It is also possible to pipe JSON to the command:

```bash
> cat email.json | dotnet lemail --stdin
```


Usage patterns
--------------------------------------------------------------------------

The tool supports all of the following usage patterns to construct the
email message to send:

* JSON (file, or piped in)
* Environment variables, only if `--env` is provided)
* CLI options


Json format
--------------------------------------------------------------------------

The format of the file is as follows:

| Property   | Type                      | M/O       | Description
|------------|---------------------------|-----------|--------------------
| `from`     | Email                     | Mandatory | Email sender
| `to`       | Email, or Array of Emails | Mandatory | Email recipients
| `cc`       | Email, or Array of Emails | Optional  | Additional email recipients
| `bcc`      | Email, or Array of Emails | Optional  | Non-disclosed email recipients
| `subject`  | String                    | Mandatory | Subject of email message
| `textBody` | String, or @Filename      | Mandatory | Plain-text body
| `htmlBody` | String, or @Filename      | Mandatory | HTML body

An Email can be expressed as follows:
* `lskywalker19@tatooine.sw` - Just the e-email address
* `Luke Skywalker <lskywalker19@tatooine.sw>` - With display name


Environment variables
--------------------------------------------------------------------------

The following variables will be evaluated, but only if `--env` option
is provided in the command line.

| Env             | Type    | Description
|-----------------|---------|---------------------------------------------
| `EMAIL_FROM`    | Email   | Sender email address
| `EMAIL_TO`      | Emails  | Semi colon seperated list of recipients
| `EMAIL_SUBJECT` | String  | Subject
| `EMAIL_HTML`    | File    | File name of HTML body
| `EMAIL_TEXT`    | File    | File name of text body


CLI options
--------------------------------------------------------------------------

```
> dotnet lemail --help
Email sender

Usage: lemail [options] <InputFile>

Arguments:
  InputFile       Input JSON file

Options:
  --version       Show version information.
  -f|--from       Sender email address
  -t|--to         Recipient email address
  -s|--subject    Subject
  -h|--html-file  HTML file
  -x|--text-file  Text file
  -X|--text       Text content
  --stdin         Read JSON from stdin
  -e|--env        Load sender/recipient from environment variables
  -?|--help       Show help information
```