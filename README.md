# Ksuid

Handles parsing and generating of Cuvva standard K-sortable unique identifiers, also know as a `ksuid`. This ksuid format was created in house at [Cuvva](https://github.com/cuvva).

The main benefits of a ksuid are; easily sortable by creation time, easily identifiable environment and resource type, and they contain information about which machine they were generated on. (Also, 4,294,967,295 can be generated per environment, per resource type, per process, per second 🎉).

## Installation

Available on [NuGet](https://www.nuget.org/packages/ksuid/)

Visual Studio:

```powershell
PM> Install-Package ksuid
```

.NET Core CLI:

```bash
dotnet add package ksuid
```

## Structure

A ksuid contains an environment, resource, timestamp, instance identifier, and sequence id. The environment and resource are preffixed to the ID. With the environment being optional. (If no environment is specified, then `prod` is assumed.) Below is an example id for a user on production.

`user_000000BPL4RZaImj5irv0RM56z6Ce`

### Ksuid Format

| name | type | length | example | description |
| ---- | ---- | ------ | ------- | ----------- |
| Environment | `ascii` | `any` | `prod` | The environment the ID was generated on. |
| Resource | `ascii` | `any` | `user` | The resource type of the identifier. |
| Timestamp | `uint64` | `0x08` | `1523119808` | The unix timestamp representation of when the id was generated. |
| Instance | `byte[]` | `0x09` | `[ 0x48, 0x8c, 0x85, 0x90, 0x1b, 0x18, 0x9c, 0x40, 0xad ]` | The instance identifer of the machine that generated the id. A explanation of what these can be is below. |
| Sequence Id | `uint32` | `0x04` | `1353` | The incrementing counter that is reset every second. |

### Instance identifiers

The instance identifer a block of 9 bytes, with the first defining the format of the following 8 bytes. The supported schemes are as follows:

- `72 0x48 char('H')` - Machine address and process id
- `68 0x44 char('D')` - Docker instance
- `82 0x52 char('R')` - Random

Random instance identifiers are just a series of 8 cryptographically random bytes, and the docker instance identifer hasn't been decided on yet.

The machine address and process id format had been decided and is explained below.

| name | type | length | example | description |
| ---- | ---- | ------ | ------- | ----------- |
| Machine address | `byte[]` | `0x06` | `[ 0x8c, 0x85, 0x90, 0x1b, 0x18, 0x9c ]` | An operational, non-loopback physical machine address. |
| Process Id | `uint16` | `0x02` | `21741` | The process id of the process generating the id. As process id's are not 16 bit, the following is applied before it is written `pid % 2 ** 16`. |


## Usage

```csharp
using Ksuid;

// Parsing a ksuid
var parsed = Id.Parse("user_000000BPL4RZaImj5irv0RM56z6Ce");

// Generating a ksuid, using the singleton
Node.Singleton.Generate("user");
// - user_000000BPLEn4f9U9TM0eJKi9VNVcu

// Setting the environment of the singleton node
Node.Singleton.Environment = "dev";

// Creating a new, non singleton node for the development environment.
var node = new Node("dev");
```

## Issues & Contributions

If you find a bug or have a feature request, please report them at this repository's issues section. Contributions are highly welcome, however, except for very small changes, kindly file an issue and let's have a discussion before you open a pull request.

## License

This project is licensed under the MIT license. See the [LICENSE](LICENSE) file for more info.
