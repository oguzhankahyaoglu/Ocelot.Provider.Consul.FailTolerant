[<img src="http://threemammals.com/images/ocelot_logo.png">](http://threemammals.com/ocelot)

[![Build status](https://ci.appveyor.com/api/projects/status/jmkqqg6i24dx1crc?svg=true)](https://ci.appveyor.com/project/TomPallister/ocelot-provider-consul)
Windows (AppVeyor)
[![Build Status](https://travis-ci.org/ThreeMammals/Ocelot.Provider.Consul.svg?branch=develop)](https://travis-ci.org/ThreeMammals/Ocelot.Provider.Consul) Linux & OSX (Travis)

[![Coverage Status](https://coveralls.io/repos/github/ThreeMammals/Ocelot.Provider.Consul/badge.svg)](https://coveralls.io/github/ThreeMammals/Ocelot.Provider.Consul)

# Ocelot.Provider.Consul

This package adds [Consul](https://www.consul.io/) support to Ocelot via the package [Consul.NET](https://github.com/PlayFab/consuldotnet).

## What's Different from the Original "Ocelot.Provider.Consul" Package?
- **The application crashes at startup time if cannot access to the specified Consul server!** In this package, this is fixed. The Ocelot gateway just startus up normally, uses the local appsettings/ocelot.json configuration file and starts trying to access Consul server using the specified PollingInterval (ms) value. Once it is accessed successfully, it now starts to use routes from Consul.
- **PollingInterval** in local config file was not being used. The value (if specified) in Consul server was being used instead. This behaviour has been changed to use local configuration file usage. Thus, **PollingInterval** value is being used from the local config file.
- After initial startup and successfully fetch of config from Consul, even after the Consul server is down, the Ocelot gateway is happy with the in-memory routes/configuration from latest fetch from Consul. Once the Consul server is up again, the config fill be tried to updated (since it might have been changed or not).
- "/gateway-routes" path will serve information about the current routes collection of the Gateway, both showing last fetched and file stored route configuration.

## How to install

Ocelot is designed to work with ASP.NET Core only and it targets `netstandard2.0`. 
This means it can be used anywhere `.NET Standard 2.0` is supported, 
including `.NET Core 2.1` and `.NET Framework 4.7.2` and up. 
[This](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) documentation may prove 
helpful when working out if Ocelot would be suitable for you.

Install Ocelot and it's dependencies using NuGet. 

`Install-Package Ocelot.Provider.Consul.FaultTolerant`

Or via the .NET Core CLI:

`dotnet add package Ocelot.Provider.Consul.FaultTolerant`

All versions can be found [here](https://www.nuget.org/packages/Ocelot.Provider.Consul/)

## Documentation

Please click [here](http://ocelot.readthedocs.io/en/latest/features/servicediscovery.html) for the Ocleot serviec discovery documentation and [here](http://ocelot.readthedocs.io/en/latest/features/configuration.html#store-configuration-in-consul) for storing configuration in Consul. This includes lots of information and will be helpful if you want to understand the features Ocelot currently offers.

## Contributing

We love to receive contributions from the community so please keep them coming :) 

Pull requests, issues and commentary welcome!

Please complete the relevant template for issues and PRs. Sometimes it's worth getting in touch with us to discuss changes 
before doing any work incase this is something we are already doing or it might not make sense. We can also give
advice on the easiest way to do things :)

Finally we mark all existing issues as help wanted, small, medium and large effort. If you want to contribute for the first time I suggest looking at a help wanted & small effort issue :)

## Donate

If you think this project is worth supporting financially please make a contribution using the button below!

[![Support via PayPal](https://cdn.rawgit.com/twolfson/paypal-github-button/1.0.0/dist/button.svg)](https://www.paypal.me/ThreeMammals/)


