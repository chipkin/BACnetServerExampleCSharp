# Windows BACnet Server Example CSharp

A basic BACnet IP server example written in CSharp using the [CAS BACnet Stack](https://www.bacnetstack.com/).

- device: 389001  (Device name Rainbow)
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

## Building

A [Visual studio 2019](https://visualstudio.microsoft.com/downloads/) project is included with this project.

This project also auto built using [Gitlab CI](https://docs.gitlab.com/ee/ci/) on every commit.
