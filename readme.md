## Avaya Moagent Client

Avaya Moagent Client is a fully-managed replacement for the COM-component provided as part of the Avaya SDK.

### Why?

This project was created as a replacement for the somewhat buggy Avaya provided COM-component to interact with Avaya PDS / Avaya Proactive Contact systems.

### What's Included?

* AvayaMoagentClient - This is the client itself.
* AvayaPDSEmulator - A highly compatible simulator for for the Avaya dialer, allows for off-line testing and development.
* AvayaTestClient - A WPF GUI test application for testing the client.

### Compatibility

This has been tested with PDS version 12, and PC 3 & 4. It should be compatible with other versions as well, both SSL and Telnet based systems.

###FAQ

_I'm getting "Unable to load DLL 'libeay32': This application has failed to start because the application configuration is incorrect. Reinstalling the application may fix this problem."_

Install the [Microsoft Visual C++ 2008 SP1 Redistributable](https://www.microsoft.com/en-us/download/details.aspx?displaylang=en&id=5582) package to correct the issue.

### License

This code is licensed under the BSD 2-clause license.
