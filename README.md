Loupe Agent Web Module for WebApi
===================

This module enables you to not only utilise a [Loupe JavaScript agent](https://github.com/GibraltarSoftware/Gibraltar.Agent.Web.JavaScript)
but it also provide session tracking of users.

It adds to an existing Loupe system so all your existing code continues to work as it does now
and it extends the [Loupe Agent for Mvc/WebApi](https://github.com/GibraltarSoftware/Gibraltar.Agent.Web.Mvc) enabling you
to still use any viewer for Loupe to review the agent's information

This module is designed specifically to be used with WebApi and isn't reliant on you utilizing Forms Authentication to enable the correct 
logging of the user which initiated a request.


*NOTE*
This is currently a prototype with a customized JavaScript agent which has additional configuration required to work within WebApi and
with token based authentication mechanisims

Using the Module
---------------

Currently you need to build and add it to an existing system manually then you need to alter the web cofig and add the following to the ```<system.webServer><modules>``` section: ```<add name="Loupe.JSLogging" type="Loupe.Agent.Web.Module.Logging" />```


Building the Agent
------------------

This project is designed for use with Visual Studio 2013 with NuGet package restore enabled.
When you build it the first time it will retrieve dependencies from NuGet.

Contributing
------------

Feel free to branch this project and contribute a pull request to the development branch. 
If your changes are incorporated into the master version they'll be published out to NuGet for
everyone to use!
