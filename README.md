# BACnet Server Example C#

A basic BACnet IP server example written in C# using the [CAS BACnet Stack](https://store.chipkin.com/services/stacks/bacnet-stack). Includes various sample BACnet objects and services.

## Releases

Build versions of this example can be downloaded from the [Releases](https://github.com/chipkin/BACnetServerExampleCSharp/releases) page. 

## Installation

Download the latest release zip file on the [Releases](https://github.com/chipkin/BACnetServerExampleCSharp/releases) page.
Copy CASBACnetStack_x64_Release.dll from the CAS BACnet Stack into the release folder. Please contact Chipkin Automation Systems for access to the CAS BACnet Stack. Launch server by using the following command:
```
dotnet BACnetServerExample.dll
```
Requires [.NET Core 3.0+](https://dotnet.microsoft.com/download)

## Usage

Run the executable included in the zip file. Pre-configured with the following example BACnet device and objects:

- Device: 389001  (Device name Rainbow)
  - analog_input: 0  (AnalogInput Amber)
  - analog_input: 1  (AnalogInput Bronze)
  - analog_input: 2  (AnalogInput Chartreuse)
  - analog_output: 0  (AnalogOutput Diamond)
  - analog_output: 1  (AnalogOutput Emerald)
  - analog_output: 2  (AnalogOutput Fuchsia)
  - analog_value: 0  (AnalogValue Gold)
  - analog_value: 1  (AnalogValue Hot Pink)
  - analog_value: 2  (AnalogValue Indigo)
  - binary_input: 0  (BinaryInput Kiwi)
  - binary_input: 1  (BinaryInput Lilac)
  - binary_input: 2  (BinaryInput Magenta)
  - binary_value: 0  (BinaryValue Nickel)
  - binary_value: 1  (BinaryValue Onyx)
  - binary_value: 2  (BinaryValue Purple)
  - multi_state_input: 0  (MultiStateInput Quartz)
  - multi_state_input: 1  (MultiStateInput Red)
  - multi_state_input: 2  (MultiStateInput Silver)
  - multi_state_value: 0  (MultiStateValue Turquoise)
  - multi_state_value: 1  (MultiStateValue Umber)
  - multi_state_value: 2  (MultiStateValue Vermilion)

The following keyboard commands can be issued in the server window:
* **H**: Display help menu
* **Up Arrow**: Increment Analog Input 0
* **Down Arrow**: Decrement Analog Input 0
* **Q**: Quit the program

## Build

A [Visual Studio 2019](https://visualstudio.microsoft.com/downloads/) project is included with this project. This project also auto built using [Gitlab CI](https://docs.gitlab.com/ee/ci/) on every commit.

1. Copy *CASBACnetStack_x64_Debug.dll*, *CASBACnetStack_x64_Debug.lib*, *CASBACnetStack_x64_Release.dll*, and *CASBACnetStack_x64_Release.lib* from the [CAS BACnet Stack](https://store.chipkin.com/services/stacks/bacnet-stack) project into the /bin/netcoreapp2.1/ folder.
2. Use [Visual Studio 2019](https://visualstudio.microsoft.com/vs/) to build the project. The solution can be found in the */BACnetServerExample/* folder.

## Example Output
```
Starting BACnetServerExampleCSharp version: 0.0.1.0
https://github.com/chipkin/BACnetServerExampleCSharp
FYI: BACnet Stack version: 3.16.0.1107
FYI: CAS BACnet Stack Setup, successfully
FYI: Starting main loop
FYI: Recving 12 bytes from 192.168.1.8:56014
FYI: Request for CallbackGetUnsignedInteger. objectType=8, objectInstance=389001, propertyIdentifier=62, propertyArrayIndex=0
FYI: Sending 13 bytes to 192.168.1.8:56014
FYI: Recving 17 bytes from 192.168.1.8:56014
FYI: Sending 188 bytes to 192.168.1.8:56014

BACnetServerExampleCSharp version: 0.0.1.0
BACnet Stack version: 3.16.0.1107
https://github.com/chipkin/BACnetServerExampleCSharp
H  - Display the help menu
Up Arrow - Increment the Analog Input 0 property
Down Arrow - Decrement the Analog Input 0 property
Q - Quit the program
```