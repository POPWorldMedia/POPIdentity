# POP Identity

POP Identity is a lightweight, low-opinion, mini-library for social and third-party logins in ASP.NET Core.

## Project status
v3 has shipped and is available via Nuget. Running against net5.0, it has support for round-tripping to Google, Facebook, Microsoft and any generic OAuth2 service that returns a JWT.

## Who is this for?
This is for people who think that the existing ASP.NET Core external login system is too much magic, or too tightly coupled to Identity and/or EntityFramework. It didn't evolve much from the old OWIN days. It has the following goals:
* Be super light-weight, handing off just enough mundane detail to the library.
* Allow code to change client ID's, secrets and other parameters at request time, as opposed to the built-in framework that sets this all at startup. This makes it appropriate for multi-tenant situations.
* Allow the developer to persist the resulting data (external ID's, name, email, etc.) in whatever manner makes sense. This library has no persistence.
* Defer authorization logic to the developer. It doesn't setup any claims identity... that's up to you.

## Getting started
The sample web app in the source is the easiest way to see how this works. The `HomeController` has enough comments to see how best to use the framework. The goal is to be able to craft a login experience from scratch at request time, or leverage the configuration values.

POP Forums uses this library today. To learn more about that implementation, go here: https://popworldmedia.github.io/POPForums/externalloginconfig.html

## Getting the bits
POP Identity is available as a Nuget package: 

https://www.nuget.org/packages/PopIdentity/ 

## CI build 
https://www.myget.org/F/popidentity/api/v3/index.json 

[![Build status](https://popw.visualstudio.com/POP%20Identity/_apis/build/status/POP%20Identity-ASP.NET%20Core-CI)](https://popw.visualstudio.com/POP%20Identity/_build/latest?definitionId=3)
