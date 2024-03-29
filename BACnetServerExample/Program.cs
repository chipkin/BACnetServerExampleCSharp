﻿/*
 * BACnet Server Example C#
 * ----------------------------------------------------------------------------
 * Program.cs
 * 
 * In this CAS BACnet Stack example, we create a BACnet IP server with various
 * objects and properties from an example database.
 *
 * More information https://github.com/chipkin/BACnetServerExampleCSharp
 * 
 * This file contains the 'Main' function. Program execution begins and ends there.
 * 
 * Created by: Steven Smethurst
 * Created on: June 7, 2019 
 * Last updated: May 19, 2021
*/


using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using CASBACnetStack;

namespace BACnetServerExample
{
    class Program
    {
        // Main function
        static void Main(string[] args)
        {
            BACnetServer bacnetServer = new BACnetServer();
            bacnetServer.Run();
        }

        // BACnet Server Object
        unsafe class BACnetServer
        {
            // UDP 
            UdpClient udpServer;
            IPEndPoint RemoteIpEndPoint;

            // A Database to hold the current state of the server
            private ExampleDatabase database = new ExampleDatabase();

            // Version 
            const string APPLICATION_VERSION = "0.0.5";

            // Server setup and main loop
            public void Run()
            {
                Console.WriteLine("Starting BACnetServerExampleCSharp version: {0}.{1}", APPLICATION_VERSION, CIBuildVersion.CIBUILDNUMBER);
                Console.WriteLine("https://github.com/chipkin/BACnetServerExampleCSharp");
                Console.WriteLine("FYI: BACnet Stack version: {0}.{1}.{2}.{3}",
                    CASBACnetStackAdapter.GetAPIMajorVersion(),
                    CASBACnetStackAdapter.GetAPIMinorVersion(),
                    CASBACnetStackAdapter.GetAPIPatchVersion(),
                    CASBACnetStackAdapter.GetAPIBuildVersion());

                // 1. Setup the callbacks
                // ---------------------------------------------------------------------------

                // Send/Receive callbacks
                CASBACnetStackAdapter.RegisterCallbackSendMessage(SendMessage);
                CASBACnetStackAdapter.RegisterCallbackReceiveMessage(RecvMessage);
                CASBACnetStackAdapter.RegisterCallbackGetSystemTime(CallbackGetSystemTime);

                // Get Datatype Callbacks 
                CASBACnetStackAdapter.RegisterCallbackGetPropertyCharacterString(CallbackGetPropertyCharString);
                CASBACnetStackAdapter.RegisterCallbackGetPropertyReal(CallbackGetPropertyReal);
                CASBACnetStackAdapter.RegisterCallbackGetPropertyEnumerated(CallbackGetEnumerated);
                CASBACnetStackAdapter.RegisterCallbackGetPropertyUnsignedInteger(CallbackGetUnsignedInteger);
                CASBACnetStackAdapter.RegisterCallbackGetPropertyBool(CallbackGetPropertyBool);
                CASBACnetStackAdapter.RegisterCallbackGetPropertyDate(CallbackGetPropertyDate);
                CASBACnetStackAdapter.RegisterCallbackGetPropertyDouble(CallbackGetPropertyDouble);
                CASBACnetStackAdapter.RegisterCallbackGetPropertySignedInteger(CallbackGetPropertySignedInteger);
                CASBACnetStackAdapter.RegisterCallbackGetPropertyTime(CallbackGetPropertyTime);
                // NULL
                CASBACnetStackAdapter.RegisterCallbackGetPropertyOctetString(CallbackGetPropertyOctetString);
                // BitString

                // Set Datatype Callbacks 
                CASBACnetStackAdapter.RegisterCallbackSetPropertyCharacterString(CallbackSetPropertyCharacterString);
                CASBACnetStackAdapter.RegisterCallbackSetPropertyReal(CallbackSetPropertyReal);
                CASBACnetStackAdapter.RegisterCallbackSetPropertyEnumerated(CallbackSetPropertyEnumerated);
                CASBACnetStackAdapter.RegisterCallbackSetPropertyUnsignedInteger(CallbackSetPropertyUnsignedInteger);
                CASBACnetStackAdapter.RegisterCallbackSetPropertyBool(CallbackSetPropertyBool);
                CASBACnetStackAdapter.RegisterCallbackSetPropertyDate(CallbackSetPropertyDate);
                CASBACnetStackAdapter.RegisterCallbackSetPropertyDouble(CallbackSetPropertyDouble);
                CASBACnetStackAdapter.RegisterCallbackSetPropertySignedInteger(CallbackSetPropertySignedInteger);
                CASBACnetStackAdapter.RegisterCallbackSetPropertyTime(CallbackSetPropertyTime);
                CASBACnetStackAdapter.RegisterCallbackSetPropertyNull(CallbackSetPropertyNull);
                CASBACnetStackAdapter.RegisterCallbackSetPropertyOctetString(CallbackSetPropertyOctetString);
                CASBACnetStackAdapter.RegisterCallbackSetPropertyBitString(CallbackSetPropertyBitString);

                CASBACnetStackAdapter.RegisterCallbackReinitializeDevice(CallbackReinitializeDevice);


                // 2. Setup the BACnet device
                // ---------------------------------------------------------------------------

                // Initialize database
                this.database.Setup();
                database.NetworkPort.BACnetIPUDPPort = 47808;
                Console.WriteLine(database.NetworkPort.ToString());

                // Add the device
                CASBACnetStackAdapter.AddDevice(this.database.Device.instance);

                // 3. Add Objects
                // ---------------------------------------------------------------------------

                // AnalogInput
                for (UInt32 offset = 0; offset < this.database.AnalogInput.Length; offset++)
                {
                    CASBACnetStackAdapter.AddObject(database.Device.instance, CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_INPUT, offset);
                }
                CASBACnetStackAdapter.SetPropertyByObjectTypeEnabled(database.Device.instance, CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_INPUT, CASBACnetStackAdapter.PROPERTY_IDENTIFIER_DESCRIPTION, true);
                CASBACnetStackAdapter.SetPropertyByObjectTypeSubscribable(database.Device.instance, CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_INPUT, CASBACnetStackAdapter.PROPERTY_IDENTIFIER_PRESENT_VALUE, true);

                // AnalogOutput
                for (UInt32 offset = 0; offset < this.database.AnalogOutput.Length; offset++)
                {
                    CASBACnetStackAdapter.AddObject(database.Device.instance, CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_OUTPUT, offset);
                }

                // AnalogValue
                for (UInt32 offset = 0; offset < this.database.AnalogValue.Length; offset++)
                {
                    CASBACnetStackAdapter.AddObject(database.Device.instance, CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_VALUE, offset);
                    CASBACnetStackAdapter.SetPropertyWritable(database.Device.instance, CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_VALUE, offset, CASBACnetStackAdapter.PROPERTY_IDENTIFIER_PRESENT_VALUE, true);
                }

                // BinaryInput
                for (UInt32 offset = 0; offset < this.database.BinaryInput.Length; offset++)
                {
                    CASBACnetStackAdapter.AddObject(database.Device.instance, CASBACnetStackAdapter.OBJECT_TYPE_BINARY_INPUT, offset);
                }

                // BinaryValue
                for (UInt32 offset = 0; offset < this.database.BinaryValue.Length; offset++)
                {
                    CASBACnetStackAdapter.AddObject(database.Device.instance, CASBACnetStackAdapter.OBJECT_TYPE_BINARY_VALUE, offset);
                    CASBACnetStackAdapter.SetPropertyWritable(database.Device.instance, CASBACnetStackAdapter.OBJECT_TYPE_BINARY_VALUE, offset, CASBACnetStackAdapter.PROPERTY_IDENTIFIER_PRESENT_VALUE, true);
                }

                // MultiStateInput
                for (UInt32 offset = 0; offset < this.database.MultiStateInput.Length; offset++)
                {
                    CASBACnetStackAdapter.AddObject(database.Device.instance, CASBACnetStackAdapter.OBJECT_TYPE_MULTI_STATE_INPUT, offset);
                }
                CASBACnetStackAdapter.SetPropertyByObjectTypeEnabled(database.Device.instance, CASBACnetStackAdapter.OBJECT_TYPE_MULTI_STATE_INPUT, CASBACnetStackAdapter.PROPERTY_IDENTIFIER_STATETEXT, true);

                // MultiStateValue
                for (UInt32 offset = 0; offset < this.database.MultiStateValue.Length; offset++)
                {
                    CASBACnetStackAdapter.AddObject(database.Device.instance, CASBACnetStackAdapter.OBJECT_TYPE_MULTI_STATE_VALUE, offset);
                    CASBACnetStackAdapter.SetPropertyWritable(database.Device.instance, CASBACnetStackAdapter.OBJECT_TYPE_MULTI_STATE_VALUE, offset, CASBACnetStackAdapter.PROPERTY_IDENTIFIER_PRESENT_VALUE, true);
                }
                CASBACnetStackAdapter.SetPropertyByObjectTypeEnabled(database.Device.instance, CASBACnetStackAdapter.OBJECT_TYPE_MULTI_STATE_VALUE, CASBACnetStackAdapter.PROPERTY_IDENTIFIER_STATETEXT, true);

                // CharacterString
                for (UInt32 offset = 0; offset < this.database.CharacterString.Length; offset++)
                {
                    CASBACnetStackAdapter.AddObject(database.Device.instance, CASBACnetStackAdapter.OBJECT_TYPE_CHARACTERSTRING_VALUE, offset);
                }

                // PositiveIntergerValue
                for (UInt32 offset = 0; offset < this.database.PositiveIntergerValue.Length; offset++)
                {
                    CASBACnetStackAdapter.AddObject(database.Device.instance, CASBACnetStackAdapter.OBJECT_TYPE_POSITIVE_INTEGER_VALUE, offset);
                }

                // DateValue
                for (UInt32 offset = 0; offset < this.database.DateValue.Length; offset++)
                {
                    CASBACnetStackAdapter.AddObject(database.Device.instance, CASBACnetStackAdapter.OBJECT_TYPE_DATE_VALUE, offset);
                }

                // TimeValue
                for (UInt32 offset = 0; offset < this.database.TimeValue.Length; offset++)
                {
                    CASBACnetStackAdapter.AddObject(database.Device.instance, CASBACnetStackAdapter.OBJECT_TYPE_TIME_VALUE, offset);
                }

                // Network port object.
                CASBACnetStackAdapter.AddNetworkPortObject(database.Device.instance, 0, CASBACnetStackAdapter.NETWORK_PORT_OBJECT_NETWORK_TYPE_IPV4, CASBACnetStackAdapter.PROTOCOL_LEVEL_BACNET_APPLICATION, CASBACnetStackAdapter.NETWORK_PORT_LOWEST_PROTOCOL_LAYER);
                // CASBACnetStackAdapter.SetPropertyEnabled(database.Device.instance, CASBACnetStackAdapter.OBJECT_TYPE_NETWORK_PORT, 0, CASBACnetStackAdapter.PROPERTY_IDENTIFIER_FDBBMDADDRESS, true);
                // CASBACnetStackAdapter.SetPropertyEnabled(database.Device.instance, CASBACnetStackAdapter.OBJECT_TYPE_NETWORK_PORT, 0, CASBACnetStackAdapter.PROPERTY_IDENTIFIER_FDSUBSCRIPTIONLIFETIME, true);

                // 4. Enable Services
                // ---------------------------------------------------------------------------
                // Enable optional services 
                CASBACnetStackAdapter.SetServiceEnabled(database.Device.instance, CASBACnetStackAdapter.SERVICE_READ_PROPERTY_MULTIPLE, true);
                CASBACnetStackAdapter.SetServiceEnabled(database.Device.instance, CASBACnetStackAdapter.SERVICE_WRITE_PROPERTY, true);
                CASBACnetStackAdapter.SetServiceEnabled(database.Device.instance, CASBACnetStackAdapter.SERVICE_WRITE_PROPERTY_MULTIPLE, true);
                CASBACnetStackAdapter.SetServiceEnabled(database.Device.instance, CASBACnetStackAdapter.SERVICE_SUBSCRIBE_COV, true);

                // Network port
                CASBACnetStackAdapter.SetServiceEnabled(database.Device.instance, CASBACnetStackAdapter.SERVICE_REINITIALIZE_DEVICE, true);

                // All done with the BACnet setup
                Console.WriteLine("FYI: CAS BACnet Stack Setup, successfully");

                // 5. Open the BACnet port to receive messages
                // ---------------------------------------------------------------------------
                this.udpServer = new UdpClient(database.NetworkPort.BACnetIPUDPPort);
                this.RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                // 6. Start the main loop
                // ---------------------------------------------------------------------------
                Console.WriteLine("FYI: Starting main loop");
                PrintHelp(); 

                for (; ; )
                {
                    CASBACnetStackAdapter.Loop();

                    // Update values in the example database
                    database.Loop();

                    // Handle any user input
                    // Note: User input in this example is used for the following:
                    //      H  - Display the help menu
                    //      Up Arrow - Increment the Analog Input 0 property
                    //      Down Arrow - Decrement the Analog Input 0 property
                    //      Q - Quit the program
                    if (!DoUserInput())
                    {
                        // Exit program if Q is hit
                        break;
                    }
                }
            }

            private void PrintHelp()
            {
                Console.WriteLine("\nBACnetServerExampleCSharp version: {0}.{1}", APPLICATION_VERSION, CIBuildVersion.CIBUILDNUMBER);
                Console.WriteLine("BACnet Stack version: {0}.{1}.{2}.{3}",
                    CASBACnetStackAdapter.GetAPIMajorVersion(),
                    CASBACnetStackAdapter.GetAPIMinorVersion(),
                    CASBACnetStackAdapter.GetAPIPatchVersion(),
                    CASBACnetStackAdapter.GetAPIBuildVersion());

                Console.WriteLine("https://github.com/chipkin/BACnetServerExampleCSharp");
                Console.WriteLine("Menu");
                Console.WriteLine("H          - Display the help menu");
                Console.WriteLine("Up Arrow   - Increment the Analog Input 0 property");
                Console.WriteLine("Down Arrow - Decrement the Analog Input 0 property");
                Console.WriteLine("F          - Send foreign device registration");
                Console.WriteLine("Q          - Quit the program");
            }

            private byte* PointerData(byte[] safe)
            {
                fixed (byte* converted = safe)
                {
                    return converted;
                }
            }

            // Handle user input. Return false if quitting program.
            private bool DoUserInput()
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        // Display help menu and version information
                        default:
                        case ConsoleKey.H:
                            PrintHelp();
                            break;

                        // Increment the analog input
                        case ConsoleKey.UpArrow:
                            if (this.database.AnalogInput.Length > 0)
                            {
                                this.database.AnalogInput[0].presentValue += 0.01f;
                                Console.WriteLine("\nFYI: Incurment Analog input {0} present value to {1:0.00}", 0, this.database.AnalogInput[0].presentValue);

                                // Notify the CAS BACnet stack that this value has been updated. 
                                // If there are any subscribers to this value, they will be sent be sent the updated value. 
                                CASBACnetStackAdapter.ValueUpdated(database.Device.instance, CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_INPUT, 0, CASBACnetStackAdapter.PROPERTY_IDENTIFIER_PRESENT_VALUE);
                            }
                            break;

                        // Decrement the analog input 
                        case ConsoleKey.DownArrow:
                            if (this.database.AnalogInput.Length > 0)
                            {
                                this.database.AnalogInput[0].presentValue -= 0.01f;
                                Console.WriteLine("\nFYI: Decrement Analog input {0} present value to {1:0.00}", 0, this.database.AnalogInput[0].presentValue);
                                CASBACnetStackAdapter.ValueUpdated(database.Device.instance, CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_INPUT, 0, CASBACnetStackAdapter.PROPERTY_IDENTIFIER_PRESENT_VALUE);
                            }
                            break;

                        // Decrement the analog input 
                        case ConsoleKey.F:

                            Console.WriteLine("\nFYI: Sending RegisterForeignDevice IP: {0}, Port: {1}", this.database.NetworkPort.FdBbmdAddressHostIp.ToString(), this.database.NetworkPort.FdBbmdAddressPort);

                            byte[] connectionStringAsBytes = new byte[6];
                            Buffer.BlockCopy(this.database.NetworkPort.FdBbmdAddressHostIp.GetAddressBytes(), 0, connectionStringAsBytes, 0, 4);
                            Buffer.BlockCopy(BitConverter.GetBytes(this.database.NetworkPort.FdBbmdAddressPort), 0, connectionStringAsBytes, 4, 2);

                            byte* connectionStringPointer = PointerData(connectionStringAsBytes);
                            if( ! CASBACnetStackAdapter.SendRegisterForeignDevice(this.database.NetworkPort.FdSubscriptionLifetime, connectionStringPointer, (byte) connectionStringAsBytes.Length) )
                            {
                                Console.WriteLine("\nError: Could not send RegisterForeignDevice");  
                            }

                            break;

                        // Quit the program
                        case ConsoleKey.Q:
                            Console.WriteLine("\nQuitting...", 0, this.database.AnalogInput[0].presentValue);
                            return false;
                    }
                }
                return true;
            }

            // Callback used by the BACnet Stack to get the current time
            public ulong CallbackGetSystemTime()
            {
                // https://stackoverflow.com/questions/9453101/how-do-i-get-epoch-time-in-c
                return (ulong)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            }

            // Callback used by the BACnet Stack to send a BACnet message
            public UInt16 SendMessage(System.Byte* message, UInt16 messageLength, System.Byte* connectionString, System.Byte connectionStringLength, System.Byte networkType, Boolean broadcast)
            {
                if (connectionStringLength < 6 || messageLength <= 0)
                {
                    return 0;
                }
                // Extract the connection string into a IP address and port. 
                IPAddress ipAddress = new IPAddress(new byte[] { connectionString[0], connectionString[1], connectionString[2], connectionString[3] });
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, (connectionString[4] + connectionString[5] * 256));

                // Debug 
                Console.WriteLine("FYI: Sending {0} bytes to {1}", messageLength, ipEndPoint.ToString());

                // Copy from the unsafe pointer to a Byte array. 
                byte[] sendBytes = new byte[messageLength];
                Marshal.Copy((IntPtr)message, sendBytes, 0, messageLength);

                try
                {
                    this.udpServer.Send(sendBytes, sendBytes.Length, ipEndPoint);
                    return (UInt16)sendBytes.Length;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }

                return 0;
            }

            // Callback used by the BACnet Stack to check if there is a message to process
            public UInt16 RecvMessage(System.Byte* message, UInt16 maxMessageLength, System.Byte* receivedConnectionString, System.Byte maxConnectionStringLength, System.Byte* receivedConnectionStringLength, System.Byte* networkType)
            {
                try
                {
                    if (this.udpServer.Available > 0)
                    {
                        // Data buffer for incoming data.  
                        byte[] receiveBytes = this.udpServer.Receive(ref this.RemoteIpEndPoint);
                        byte[] ipAddress = RemoteIpEndPoint.Address.GetAddressBytes();
                        byte[] port = BitConverter.GetBytes(UInt16.Parse(RemoteIpEndPoint.Port.ToString()));

                        // Copy from the unsafe pointer to a Byte array. 
                        Marshal.Copy(receiveBytes, 0, (IntPtr)message, receiveBytes.Length);

                        // Copy the Connection string 
                        Marshal.Copy(ipAddress, 0, (IntPtr)receivedConnectionString, 4);
                        Marshal.Copy(port, 0, (IntPtr)receivedConnectionString + 4, 2);
                        *receivedConnectionStringLength = 6;

                        // Debug 
                        Console.WriteLine("FYI: Recving {0} bytes from {1}", receiveBytes.Length, RemoteIpEndPoint.ToString());

                        // Return length. 
                        return (ushort)receiveBytes.Length;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                return 0;
            }

            // Callback used by the BACnet Stack to set Charstring property values to the user
            public bool CallbackGetPropertyCharString(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, System.Byte* value, UInt32* valueElementCount, UInt32 maxElementCount, System.Byte encodingType, bool useArrayIndex, UInt32 propertyArrayIndex)
            {
                Console.WriteLine("FYI: Request for CallbackGetPropertyCharString. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);

                switch (objectType)
                {
                    case CASBACnetStackAdapter.OBJECT_TYPE_DEVICE:
                        if (deviceInstance == database.Device.instance && objectInstance == database.Device.instance)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OBJECT_NAME)
                            {
                                *valueElementCount = CASBACnetStackAdapter.UpdateStringAndReturnSize(value, maxElementCount, database.Device.name);
                                return true;
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_MODEL_NAME)
                            {
                                *valueElementCount = CASBACnetStackAdapter.UpdateStringAndReturnSize(value, maxElementCount, database.Device.modelName);
                                return true;
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_VENDOR_NAME)
                            {
                                *valueElementCount = CASBACnetStackAdapter.UpdateStringAndReturnSize(value, maxElementCount, database.Device.vendorName);
                                return true;
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_APPLICATIONSOFTWAREVERSION)
                            {
                                string version = APPLICATION_VERSION + "." + CIBuildVersion.CIBUILDNUMBER;
                                *valueElementCount = CASBACnetStackAdapter.UpdateStringAndReturnSize(value, maxElementCount, version);
                                return true;
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_INPUT:
                        if (objectInstance < database.AnalogInput.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OBJECT_NAME)
                            {
                                *valueElementCount = CASBACnetStackAdapter.UpdateStringAndReturnSize(value, maxElementCount, database.AnalogInput[objectInstance].name);
                                return true;
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_DESCRIPTION)
                            {
                                *valueElementCount = CASBACnetStackAdapter.UpdateStringAndReturnSize(value, maxElementCount, database.AnalogInput[objectInstance].description);
                                return true;
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_OUTPUT:
                        if (objectInstance < database.AnalogOutput.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OBJECT_NAME)
                            {
                                *valueElementCount = CASBACnetStackAdapter.UpdateStringAndReturnSize(value, maxElementCount, database.AnalogOutput[objectInstance].name);
                                return true;
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_VALUE:
                        if (objectInstance < database.AnalogValue.Length) {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OBJECT_NAME)
                            {
                                *valueElementCount = CASBACnetStackAdapter.UpdateStringAndReturnSize(value, maxElementCount, database.AnalogValue[objectInstance].name);
                                return true;
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_BINARY_INPUT:
                        if (objectInstance < database.BinaryInput.Length) {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OBJECT_NAME)
                            {
                                *valueElementCount = CASBACnetStackAdapter.UpdateStringAndReturnSize(value, maxElementCount, database.BinaryInput[objectInstance].name);
                                return true;
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_BINARY_VALUE:
                        if (objectInstance < database.BinaryValue.Length) {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OBJECT_NAME)
                            {
                                *valueElementCount = CASBACnetStackAdapter.UpdateStringAndReturnSize(value, maxElementCount, database.BinaryValue[objectInstance].name);
                                return true;
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_MULTI_STATE_INPUT:
                        if (objectInstance < database.MultiStateInput.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OBJECT_NAME)
                            {
                                *valueElementCount = CASBACnetStackAdapter.UpdateStringAndReturnSize(value, maxElementCount, database.MultiStateInput[objectInstance].name);
                                return true;
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_STATETEXT && useArrayIndex)
                            {
                                if (propertyArrayIndex <= database.MultiStateInput[objectInstance].stateText.Length)
                                {
                                    *valueElementCount = CASBACnetStackAdapter.UpdateStringAndReturnSize(value, maxElementCount, database.MultiStateInput[objectInstance].stateText[propertyArrayIndex - 1]);
                                    return true;
                                }
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_MULTI_STATE_VALUE:
                        if (objectInstance < database.MultiStateValue.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OBJECT_NAME)
                            {
                                *valueElementCount = CASBACnetStackAdapter.UpdateStringAndReturnSize(value, maxElementCount, database.MultiStateValue[objectInstance].name);
                                return true;
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_STATETEXT && useArrayIndex)
                            {
                                if (propertyArrayIndex <= database.MultiStateValue[objectInstance].stateText.Length)
                                {
                                    *valueElementCount = CASBACnetStackAdapter.UpdateStringAndReturnSize(value, maxElementCount, database.MultiStateValue[objectInstance].stateText[propertyArrayIndex - 1]);
                                    return true;
                                }
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_CHARACTERSTRING_VALUE:
                        if (objectInstance < database.CharacterString.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OBJECT_NAME)
                            {
                                *valueElementCount = CASBACnetStackAdapter.UpdateStringAndReturnSize(value, maxElementCount, database.CharacterString[objectInstance].name);
                                return true;
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_PRESENT_VALUE)
                            {
                                *valueElementCount = CASBACnetStackAdapter.UpdateStringAndReturnSize(value, maxElementCount, database.CharacterString[objectInstance].presentValue);
                                return true;
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_POSITIVE_INTEGER_VALUE:
                        if (objectInstance < database.CharacterString.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OBJECT_NAME)
                            {
                                *valueElementCount = CASBACnetStackAdapter.UpdateStringAndReturnSize(value, maxElementCount, database.PositiveIntergerValue[objectInstance].name);
                                return true;
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_DATE_VALUE:
                        if (objectInstance < database.DateValue.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OBJECT_NAME)
                            {
                                *valueElementCount = CASBACnetStackAdapter.UpdateStringAndReturnSize(value, maxElementCount, database.DateValue[objectInstance].name);
                                return true;
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_TIME_VALUE:
                        if (objectInstance < database.DateValue.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OBJECT_NAME)
                            {
                                *valueElementCount = CASBACnetStackAdapter.UpdateStringAndReturnSize(value, maxElementCount, database.TimeValue[objectInstance].name);
                                return true;
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_NETWORK_PORT:
                        if (objectInstance == 0)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OBJECT_NAME)
                            {
                                *valueElementCount = CASBACnetStackAdapter.UpdateStringAndReturnSize(value, maxElementCount, database.NetworkPort.name);
                                return true;
                            }
                        }
                        break;

                    default:
                        break;
                }

                Console.WriteLine(" FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false; // Could not handle this request. 
            }

            // Callback used by the BACnet Stack to get Real property values from the user
            public bool CallbackGetPropertyReal(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, float* value, bool useArrayIndex, UInt32 propertyArrayIndex)
            {
                Console.WriteLine("FYI: Request for CallbackGetPropertyReal. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);

                switch (objectType)
                {
                    case CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_INPUT:
                        if (objectInstance < database.AnalogInput.Length)
                        {
                            *value = database.AnalogInput[objectInstance].presentValue;
                            Console.WriteLine("FYI: AnalogInput[{0}].value got [{1}]", objectInstance, database.AnalogInput[objectInstance].presentValue);
                            return true;
                        }
                        break;

                    case CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_OUTPUT:
                        if (objectInstance < database.AnalogInput.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_PRIORITY_ARRAY && useArrayIndex)
                            {
                                if (propertyArrayIndex <= database.AnalogOutput[objectInstance].priorityArrayNulls.Length)
                                {
                                    *value = database.AnalogOutput[objectInstance].priorityArrayValues[propertyArrayIndex - 1];
                                    return true;
                                }
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_RELINQUISHDEFAULT)
                            {
                                *value = database.AnalogOutput[objectInstance].relinquishDefault;
                                return true;
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_VALUE:
                        if (objectInstance < database.AnalogValue.Length) {
                            *value = database.AnalogValue[objectInstance].presentValue;
                            Console.WriteLine("FYI: AnalogValue[{0}].value got [{1}]", objectInstance, database.AnalogValue[objectInstance].presentValue);
                            return true;
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_NETWORK_PORT:
                        if (objectInstance == 0)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_LINKSPEED )
                            {
                                Console.WriteLine("Link speed          : {0}", NetworkInterface.GetAllNetworkInterfaces()[0].Speed.ToString());
                                *value = (float) NetworkInterface.GetAllNetworkInterfaces()[0].Speed;
                                return true;
                            }
                        }
                        break;

                        
                    default:
                        break;
                }

                Console.WriteLine(" FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }

            // Callback used by the BACnet Stack to get Enumerated property values from the user
            public bool CallbackGetEnumerated(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, UInt32* value, bool useArrayIndex, UInt32 propertyArrayIndex)
            {
                Console.WriteLine("FYI: Request for CallbackGetEnumerated. objectType={0}, objectInstance={1}, propertyIdentifier={2}", objectType, objectInstance, propertyIdentifier);

                switch (objectType)
                {
                    case CASBACnetStackAdapter.OBJECT_TYPE_BINARY_INPUT:
                        if (objectInstance < database.BinaryInput.Length) {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_PRESENT_VALUE) {
                                *value = (UInt32)(database.BinaryInput[objectInstance].presentValue ? 1 : 0);
                                Console.WriteLine("FYI: BinaryInput[{0}].value got [{1}]", objectInstance, database.BinaryInput[objectInstance].presentValue);
                                return true;
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_BINARY_VALUE:
                        if (objectInstance < database.BinaryValue.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_PRESENT_VALUE) {
                                *value = (UInt32)(database.BinaryValue[objectInstance].presentValue ? 1 : 0);
                                Console.WriteLine("FYI: BinaryValue[{0}].value got [{1}]", objectInstance, database.BinaryValue[objectInstance].presentValue);
                                return true;
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_INPUT:
                        if (objectInstance < database.AnalogInput.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_UNITS)
                            {
                                *value = (UInt32)(database.AnalogInput[objectInstance].units);
                                Console.WriteLine("FYI: AnalogInput[{0}].units got [{1}]", objectInstance, database.AnalogInput[objectInstance].units);
                                return true;
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_VALUE:
                        if (objectInstance < database.AnalogValue.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_UNITS)
                            {
                                *value = (UInt32)(database.AnalogValue[objectInstance].units);
                                Console.WriteLine("FYI: AnalogValue[{0}].units got [{1}]", objectInstance, database.AnalogValue[objectInstance].units);
                                return true;
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_NETWORK_PORT:
                        if (objectInstance == 0)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_FDBBMDADDRESS)
                            {
                                *value = database.NetworkPort.FdBbmdAddressHostType;
                                return true;
                            }
                        }
                        break;
                    default:
                        break;
                }

                Console.WriteLine(" FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }

            // Callback used by the BACnet Stack to get Unsigned Integer property values from the user
            public bool CallbackGetUnsignedInteger(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, UInt32* value, bool useArrayIndex, UInt32 propertyArrayIndex)
            {
                Console.WriteLine("FYI: Request for CallbackGetUnsignedInteger. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);
                switch (objectType)
                {
                    case CASBACnetStackAdapter.OBJECT_TYPE_DEVICE:
                        if (deviceInstance == database.Device.instance && objectInstance == database.Device.instance)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_PROTOCOLREVISION)
                            {
                                *value = database.Device.protocolRevision;
                                return true;
                            }
                        }
                        break;

                    case CASBACnetStackAdapter.OBJECT_TYPE_MULTI_STATE_INPUT:
                        if (objectInstance < database.MultiStateInput.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_PRESENT_VALUE)
                            {
                                *value = database.MultiStateInput[objectInstance].presentValue;
                                return true;
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_STATETEXT && useArrayIndex && propertyArrayIndex == 0)
                            {
                                *value = Convert.ToUInt32(database.MultiStateInput[objectInstance].stateText.Length);
                                return true;
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_NUMBEROFSTATES)
                            {
                                *value = Convert.ToUInt32(database.MultiStateInput[objectInstance].stateText.Length);
                                return true;
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_MULTI_STATE_VALUE:
                        if (objectInstance < database.MultiStateValue.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_PRESENT_VALUE)
                            {
                                *value = database.MultiStateValue[objectInstance].presentValue;
                                return true;
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_STATETEXT && useArrayIndex && propertyArrayIndex == 0)
                            {
                                *value = Convert.ToUInt32(database.MultiStateValue[objectInstance].stateText.Length);
                                return true;
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_NUMBEROFSTATES)
                            {
                                *value = Convert.ToUInt32(database.MultiStateValue[objectInstance].stateText.Length);
                                return true;
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_POSITIVE_INTEGER_VALUE:
                        if (objectInstance < database.PositiveIntergerValue.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_PRESENT_VALUE)
                            {
                                *value = database.PositiveIntergerValue[objectInstance].presentValue;
                                return true;
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_NETWORK_PORT:
                        if (objectInstance == 0)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_FDBBMDADDRESS)
                            {
                                const Byte FD_BBMD_ADDRESS_PORT = 2;
                                if (useArrayIndex && propertyArrayIndex == FD_BBMD_ADDRESS_PORT)
                                {
                                    *value = database.NetworkPort.FdBbmdAddressPort;
                                    return true;
                                }
                            } 
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_FDSUBSCRIPTIONLIFETIME)
                            {
                                *value = database.NetworkPort.FdSubscriptionLifetime;
                                return true;
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_IPDNSSERVER && useArrayIndex && propertyArrayIndex == 0)
                            {
                                var dnsAddress = NetworkInterface.GetAllNetworkInterfaces()
                                   .Where(e => e.OperationalStatus == OperationalStatus.Up)
                                   .Where(e => e.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                                   .SelectMany(e => e.GetIPProperties().DnsAddresses)
                                   .Where(adr => adr.AddressFamily == AddressFamily.InterNetwork)
                                   .ToArray();
                                if (dnsAddress == null)
                                {
                                    Console.WriteLine("Error: Could not find a the DNS address");
                                    return false;
                                }

                                *value = (UInt32) dnsAddress.Length;
                                return true; 
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_BACNETIPUDPPORT)
                            {
                                *value = database.NetworkPort.BACnetIPUDPPort;
                                return true;
                            }
                        }
                        break;
                    default:
                        break;
                }
                Console.WriteLine(" FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }

            // Callback used by the BACnet Stack to get Boolean property values from the user
            public bool CallbackGetPropertyBool(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, bool* value, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex)
            {
                Console.WriteLine("FYI: Request for CallbackGetPropertyBool. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);

                switch (objectType)
                {
                    case CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_INPUT:
                        if (objectInstance < database.AnalogInput.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OUT_OF_SERVICE)
                            {
                                *value = database.AnalogInput[objectInstance].outOfService;
                                Console.WriteLine("FYI: AnalogInput[{0}].outOfService got [{1}]", objectInstance, database.AnalogInput[objectInstance].outOfService);
                                return true;
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_OUTPUT:
                        if (objectInstance < database.AnalogOutput.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OUT_OF_SERVICE)
                            {
                                *value = database.AnalogOutput[objectInstance].outOfService;
                                Console.WriteLine("FYI: AnalogOutput[{0}].outOfService got [{1}]", objectInstance, database.AnalogOutput[objectInstance].outOfService);
                                return true;
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_PRIORITY_ARRAY && useArrayIndex)
                            {
                                if (propertyArrayIndex <= database.AnalogOutput[objectInstance].priorityArrayNulls.Length)
                                {
                                    *value = database.AnalogOutput[objectInstance].priorityArrayNulls[propertyArrayIndex - 1];
                                    return true;
                                }
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_VALUE:
                        if (objectInstance < database.AnalogValue.Length)
                        {
                            *value = database.AnalogValue[objectInstance].outOfService;
                            Console.WriteLine("FYI: AnalogValue[{0}].outOfService got [{1}]", objectInstance, database.AnalogValue[objectInstance].outOfService);
                            return true;
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_NETWORK_PORT:
                        if (objectInstance == 0)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_CHANGESPENDING) {                            
                                *value = database.NetworkPort.ChangesPending;
                                return true;
                            }
                        }                        
                        break;
                    default:
                        break;
                }

                Console.WriteLine(" FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }

            // Callback used by the BACnet Stack to get Date property values from the user
            bool CallbackGetPropertyDate(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, Byte* year, Byte* month, Byte* day, Byte* weekday, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex)
            {
                Console.WriteLine("FYI: Request for CallbackGetPropertyDate. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);

                switch( objectType)
                {
                    case CASBACnetStackAdapter.OBJECT_TYPE_DATE_VALUE:
                        if (objectInstance < database.DateValue.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_PRESENT_VALUE)
                            {
                                *year = database.DateValue[objectInstance].presentValueYear;
                                *month = database.DateValue[objectInstance].presentValueMonth;
                                *day = database.DateValue[objectInstance].presentValueDay;
                                *weekday = database.DateValue[objectInstance].presentValueWeekday;
                                Console.WriteLine("FYI: DateValue[{0}] got year=[{1}] month=[{2}] day=[{3}] weekday=[{4}]", objectInstance, database.DateValue[objectInstance].presentValueYear,
                                    database.DateValue[objectInstance].presentValueMonth, database.DateValue[objectInstance].presentValueDay, database.DateValue[objectInstance].presentValueWeekday);
                                return true;
                            }
                        }
                        break;
                    default:
                        break; 
                }


                Console.WriteLine(" FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }

            // Callback used by the BACnet Stack to get Dboule property values from the user
            bool CallbackGetPropertyDouble(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, Double* value, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex)
            {
                Console.WriteLine("FYI: Request for CallbackGetPropertyDouble. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);
                Console.WriteLine(" FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }

            // Callback used by the BACnet Stack to get Integer property values from the user
            bool CallbackGetPropertySignedInteger(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, Int32* value, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex)
            {
                Console.WriteLine("FYI: Request for CallbackGetPropertySignedInteger. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);
                Console.WriteLine(" FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }
            bool CallbackGetPropertyTime(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, Byte* hour, Byte* minute, Byte* second, Byte* hundrethSeconds, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex)
            {
                Console.WriteLine("FYI: Request for CallbackGetPropertyTime. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);

                switch( objectType)
                {
                    case CASBACnetStackAdapter.OBJECT_TYPE_TIME_VALUE:
                        if (objectInstance < database.TimeValue.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_PRESENT_VALUE)
                            {
                                *hour = database.TimeValue[objectInstance].presentValueHour;
                                *minute = database.TimeValue[objectInstance].presentValueMinute;
                                *second = database.TimeValue[objectInstance].presentValueSecond;
                                *hundrethSeconds = database.TimeValue[objectInstance].presentValueHundrethSecond;
                                Console.WriteLine("FYI: TimeValue[{0}] got hour=[{1}] minute=[{2}] second=[{3}] hundrethSeconds=[{4}]", objectInstance, 
                                    database.TimeValue[objectInstance].presentValueHour, database.TimeValue[objectInstance].presentValueMinute, 
                                    database.TimeValue[objectInstance].presentValueSecond, database.TimeValue[objectInstance].presentValueHundrethSecond);
                                return true;
                            }
                        }
                        break;
                    default:
                        break;
                }

                Console.WriteLine(" FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }

            // Callback used by the BACnet Stack to set OctetString property values to the user
            bool CallbackGetPropertyOctetString(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, Byte* value, UInt32* valueElementCount, UInt32 maxElementCount, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex)
            {
                Console.WriteLine("FYI: Request for CallbackGetPropertyOctetString. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);

                switch (objectType)
                {
                    case CASBACnetStackAdapter.OBJECT_TYPE_NETWORK_PORT:
                        if (objectInstance == 0)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_FDBBMDADDRESS)
                            {
                                // Network Port FdBbmdAddressOffset
                                const Byte FD_BBMD_ADDRESS_HOST = 1;

                                if (useArrayIndex && propertyArrayIndex == FD_BBMD_ADDRESS_HOST)
                                {
                                    Console.WriteLine("BBMD_ADDRESS_HOST   : {0}", database.NetworkPort.FdBbmdAddressHostIp.ToString());
                                    *valueElementCount = CASBACnetStackAdapter.UpdateOctetStringAndReturnSize(value, maxElementCount, database.NetworkPort.FdBbmdAddressHostIp.GetAddressBytes());
                                    return true; 
                                }
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_IPADDRESS)
                            {
                                // Query the Network interface for the information that we need to setup the Network Port. 
                                var networkInterface = NetworkInterface.GetAllNetworkInterfaces()
                                    .Where(e => e.OperationalStatus == OperationalStatus.Up)
                                    .SelectMany(e => e.GetIPProperties().UnicastAddresses)
                                    .Where(adr => adr.Address.AddressFamily == AddressFamily.InterNetwork && adr.IsDnsEligible)
                                    .FirstOrDefault();
                                if (networkInterface == null)
                                {
                                    Console.WriteLine("Error: Could not find a suitable network interface");
                                    return false;
                                }

                                Console.WriteLine("IP Address          : {0}", networkInterface.Address.ToString());
                                *valueElementCount = CASBACnetStackAdapter.UpdateOctetStringAndReturnSize(value, maxElementCount, networkInterface.Address.GetAddressBytes() );
                                return true; 
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_IPDEFAULTGATEWAY)
                            {
                                var gatewayAddress = NetworkInterface.GetAllNetworkInterfaces()
                                    .Where(e => e.OperationalStatus == OperationalStatus.Up)
                                    .Where(e => e.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                                    .SelectMany(e => e.GetIPProperties().GatewayAddresses)
                                    .FirstOrDefault();
                                if (gatewayAddress == null)
                                {
                                    Console.WriteLine("Error: Could not find a the gateway address");
                                    return false;
                                }
                                Console.WriteLine("Default Gateway     : {0}", gatewayAddress.Address.ToString());
                                *valueElementCount = CASBACnetStackAdapter.UpdateOctetStringAndReturnSize(value, maxElementCount, gatewayAddress.Address.GetAddressBytes());
                                return true; 
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_IPDNSSERVER && useArrayIndex)
                            {
                                var dnsAddress = NetworkInterface.GetAllNetworkInterfaces()
                                   .Where(e => e.OperationalStatus == OperationalStatus.Up)
                                   .Where(e => e.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                                   .SelectMany(e => e.GetIPProperties().DnsAddresses)
                                   .Where(adr => adr.AddressFamily == AddressFamily.InterNetwork)
                                   .ToArray();
                                if (dnsAddress == null) {
                                    Console.WriteLine("Error: Could not find a the DNS address");
                                    return false;
                                }

                                if(dnsAddress.Length >= propertyArrayIndex-1)
                                {
                                    Console.WriteLine("DNS: " + dnsAddress[propertyArrayIndex - 1].ToString() );
                                    *valueElementCount = CASBACnetStackAdapter.UpdateOctetStringAndReturnSize(value, maxElementCount, dnsAddress[propertyArrayIndex - 1].GetAddressBytes());
                                    return true;                                     
                                }
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_IPSUBNETMASK )
                            {
                                // Query the Network interface for the information that we need to setup the Network Port. 
                                var networkInterface = NetworkInterface.GetAllNetworkInterfaces()
                                    .Where(e => e.OperationalStatus == OperationalStatus.Up)
                                    .SelectMany(e => e.GetIPProperties().UnicastAddresses)
                                    .Where(adr => adr.Address.AddressFamily == AddressFamily.InterNetwork && adr.IsDnsEligible)
                                    .FirstOrDefault();
                                if (networkInterface == null)
                                {
                                    Console.WriteLine("Error: Could not find a suitable network interface");
                                    return false;
                                }

                                Console.WriteLine("Subnet Mask         : {0}", networkInterface.IPv4Mask.ToString());
                                *valueElementCount = CASBACnetStackAdapter.UpdateOctetStringAndReturnSize(value, maxElementCount, networkInterface.IPv4Mask.GetAddressBytes());
                                return true; 
                            }                            
                        }
                        break;
                    default:
                        break; 
                }

                Console.WriteLine(" FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }

            // Callback used by the BACnet Stack to set Enumerated property values to the user
            public bool CallbackSetPropertyEnumerated(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, UInt32 value, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex, System.Byte priority, UInt32* errorCode)
            {
                Console.WriteLine("FYI: Request for CallbackSetPropertyEnumerated. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3} value={4}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex, value);
                switch (objectType)
                {
                    case CASBACnetStackAdapter.OBJECT_TYPE_BINARY_VALUE:
                        if (objectInstance < database.BinaryValue.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_PRESENT_VALUE)
                            {
                                database.BinaryValue[objectInstance].presentValue = (value == 1);
                                Console.WriteLine("FYI: BinaryValue[{0}] set to [{1}]", objectInstance, (value == 1));
                                return true;
                            }
                        }
                        break;
                    default:
                        break;
                }

                Console.WriteLine(" FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }

            // Callback used by the BACnet Stack to set Real property values to the user
            public bool CallbackSetPropertyReal(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, float value, bool useArrayIndex, UInt32 propertyArrayIndex, System.Byte priority, UInt32* errorCode)
            {
                Console.WriteLine("FYI: Request for CallbackSetPropertyReal. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3} value={4}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex, value);

                switch (objectType)
                {
                    case CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_OUTPUT:
                        if (objectInstance < database.AnalogOutput.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_PRESENT_VALUE)
                            {
                                if(priority <= database.AnalogOutput[objectInstance].priorityArrayNulls.Length &&
                                   priority <= database.AnalogOutput[objectInstance].priorityArrayValues.Length)
                                {
                                    database.AnalogOutput[objectInstance].priorityArrayNulls[priority - 1] = false;
                                    database.AnalogOutput[objectInstance].priorityArrayValues[priority - 1] = value;
                                    Console.WriteLine("FYI: AnalogOutput[{0}] set to [{1}] at priorty=[{2}]", objectInstance, value, priority);
                                    return true;
                                }
                            }
                        }
                        break; 
                    case CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_VALUE:
                        if (objectInstance < database.AnalogValue.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_PRESENT_VALUE) {
                                database.AnalogValue[objectInstance].presentValue = value;
                                Console.WriteLine("FYI: AnalogValue[{0}] set to [{1}]", objectInstance, value);
                                return true;
                            }
                        }
                        break;
                    default:
                        break;
                }

                Console.WriteLine(" FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }

            // Callback used by the BACnet Stack to get Unsigned Integer property values from the user
            bool CallbackSetPropertyUnsignedInteger(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, UInt32 value, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex, System.Byte priority, UInt32* errorCode)
            {
                Console.WriteLine("FYI: Request for CallbackSetPropertyUnsignedInteger. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3} value={4}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex, value);

                switch (objectType)
                {
                    case CASBACnetStackAdapter.OBJECT_TYPE_MULTI_STATE_VALUE:
                        if (objectInstance < database.MultiStateValue.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_PRESENT_VALUE)
                            {
                                database.MultiStateValue[objectInstance].presentValue = value;
                                Console.WriteLine("FYI: MultiStateValue[{0}] set to [{1}]", objectInstance, value);
                                return true;
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_NETWORK_PORT:
                        if (objectInstance == 0)
                        {
                            const Byte FD_BBMD_ADDRESS_PORT = 2;
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_FDBBMDADDRESS && useArrayIndex && propertyArrayIndex == FD_BBMD_ADDRESS_PORT )
                            {
                                database.NetworkPort.FdBbmdAddressPort = (UInt16) value;
                                database.NetworkPort.ChangesPending = true;
                                return true;
                            } else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_FDSUBSCRIPTIONLIFETIME )
                            {
                                database.NetworkPort.FdSubscriptionLifetime = (UInt16) value;
                                database.NetworkPort.ChangesPending = true;
                                return true;
                            }
                        }
                        break;
                    default:
                        break; 
                }

                Console.WriteLine(" FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false; 
            }

            // Callback used by the BACnet Stack to set NULL property values to the user
            // 
            // This is commonly used when a BACnet client 'reliqunishes' a value in a object that has a priority array. The client sends a 
            // WriteProperty message with a value of "NULL" to the present value with a priority. When the CAS BACnet Stack receives this 
            // message, it will call the CallbackSetPropertyNull callback function with the write priorty.
            bool CallbackSetPropertyNull(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex, System.Byte priority, UInt32* errorCode)
            {
                Console.WriteLine("FYI: Request for CallbackSetPropertyNull. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);

                switch (objectType)
                {
                    case CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_OUTPUT:
                        if (objectInstance < database.AnalogOutput.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_PRESENT_VALUE)
                            {
                                if(priority <= database.AnalogOutput[objectInstance].priorityArrayNulls.Length )
                                {
                                    database.AnalogOutput[objectInstance].priorityArrayNulls[priority-1] = true;
                                    Console.WriteLine("FYI: AnalogOutput[{0}] set to [NULL] for priorty=[{1}]", objectInstance, priority);
                                    return true;
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }

                Console.WriteLine(" FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false; 
            }

            // Callback used by the BACnet Stack to set Integer property values to the user
            bool CallbackSetPropertySignedInteger(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, Int32 value, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex, System.Byte priority, UInt32* errorCode)
            {
                Console.WriteLine("FYI: Request for CallbackSetPropertySignedInteger. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);
                Console.WriteLine(" FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }

            // Callback used by the BACnet Stack to set Double property values to the user
            bool CallbackSetPropertyDouble(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, Double value, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex, System.Byte priority, UInt32* errorCode)
            {
                Console.WriteLine("FYI: Request for CallbackSetPropertyDouble. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);
                Console.WriteLine(" FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false; 
            }

            // Callback used by the BACnet Stack to set Boolean property values to the user
            bool CallbackSetPropertyBool(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, bool value, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex, System.Byte priority, UInt32* errorCode)
            {
                Console.WriteLine("FYI: Request for CallbackSetPropertyBool. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);
                Console.WriteLine(" FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
            return false; 
            }

            // Callback used by the BACnet Stack to set Time property values to the user
            bool CallbackSetPropertyTime(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, Byte hour, Byte minute, Byte second, Byte hundrethSeconds, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex, System.Byte priority, UInt32* errorCode)
            {
                Console.WriteLine("FYI: Request for CallbackSetPropertyTime. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);
                Console.WriteLine(" FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false; 
            }

            // Callback used by the BACnet Stack to set Date property values to the user
            bool CallbackSetPropertyDate(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, Byte year, Byte month, Byte day, Byte weekday, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex, System.Byte priority, UInt32* errorCode)
            {
                Console.WriteLine("FYI: Request for CallbackSetPropertyDate. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);
                Console.WriteLine(" FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false; 
            }

            // Callback used by the BACnet Stack to set Charstring property values to the user
            bool CallbackSetPropertyCharacterString(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, System.Byte* value, UInt32 length, Byte encodingType, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex, System.Byte priority, UInt32* errorCode)
            {
                Console.WriteLine("FYI: Request for CallbackSetPropertyCharacterString. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);
                Console.WriteLine(" FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }

            // Callback used by the BACnet Stack to set OctetString property values to the user
            bool CallbackSetPropertyOctetString(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, Byte* value, UInt32 length, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex, System.Byte priority, UInt32* errorCode)
            {
                Console.WriteLine("FYI: Request for CallbackSetPropertyOctetString. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);

                switch (objectType)
                {
                    case CASBACnetStackAdapter.OBJECT_TYPE_NETWORK_PORT:
                        if (objectInstance == 0 && propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_FDBBMDADDRESS)
                        {
                            const Byte FD_BBMD_ADDRESS_HOST = 1;
                            if (useArrayIndex && propertyArrayIndex == FD_BBMD_ADDRESS_HOST)
                            {
                                if(length != 4 )
                                {
                                    *errorCode = CASBACnetStackAdapter.ERROR_VALUE_OUT_OF_RANGE;
                                    return false; 
                                }

                                // ToDo: Test this 
                                database.NetworkPort.FdBbmdAddressHostIp = new IPAddress(CASBACnetStackAdapter.IntPtrToByteArray(value, length));
                                database.NetworkPort.ChangesPending = true; 
                                return true; 
                            }
                        }

                        break;
                    default:
                        break; 
                }

                Console.WriteLine(" FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }

            // Callback used by the BACnet Stack to set Bitstring property values to the user
            bool CallbackSetPropertyBitString(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, bool* value, UInt32 length, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex, System.Byte priority, UInt32* errorCode)
            {
                Console.WriteLine("FYI: Request for CallbackSetPropertyBitString. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);
                Console.WriteLine(" FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }

            bool CallbackReinitializeDevice(UInt32 deviceInstance, UInt32 reinitializedStateOfDevice, System.Byte* password, UInt32 passwordLength, UInt32* errorCode)
            {
                Console.WriteLine("FYI: Request for CallbackReinitializeDevice. deviceInstance={0}, reinitializedStateOfDevice={1}", deviceInstance, reinitializedStateOfDevice);

                // This callback is called when this BACnet Server device receives a ReinitializeDevice message
                // In this callback, you will handle the reinitializedState.
                // If reinitializedState = ACTIVATE_CHANGES (7) then you will apply any network port changes and store the values in non -volatile memory
                // If reinitializedState = WARM_START(1) then you will apply any network port changes, store the values in non -volatile memory, and restart the device.
                // Before handling the reinitializedState, first check the password.
                // If your device does not require a password, then ignore any password passed in.
                // Otherwise, validate the password.
                //     If password invalid: set errorCode to PasswordInvalid (26)
                //     If password is required, but no password was provided: set errorCode to MissingRequiredParameter(16)
                // In this example, a password of 12345 is required.

                if(passwordLength == 0 )
                {
                    Console.WriteLine("Error: Password required. ERROR_MISSING_REQUIRED_PARAMETER");
                    *errorCode = CASBACnetStackAdapter.ERROR_MISSING_REQUIRED_PARAMETER;
                    return false;
                }
                if( CASBACnetStackAdapter.ASCIIBufferAsString(password, passwordLength) != "1234" )
                {
                    Console.WriteLine("Error: Password does not match. ERROR_PASSWORD_FAILURE");
                    *errorCode = CASBACnetStackAdapter.ERROR_PASSWORD_FAILURE; 
                    return false; 
                }

                // In this example, only the NetworkPort Object FdBbmdAddress and FdSubscriptionLifetime properties are writable and need to be
                // stored in non-volatile memory. For the purpose of this example, we will not storing these values in non - volatile memory.
                // 1. Store values that must be stored in non-volatile memory (i.e. must survive a reboot).
                // 2. Apply any Network Port values that have been written to. 
                // If any validation on the Network Port values failes, set errorCode to INVALID_CONFIGURATION_DATA(46)
                // 3. Set Network Port ChangesPending property to false
                // 4. Handle ReinitializedState. If ACTIVATE_CHANGES, no other action, return true.
                // If WARM_START, prepare device for reboot, return true.and reboot.
                // NOTE: Must return true first before rebooting so the stack sends the SimpleAck.

                if (reinitializedStateOfDevice == CASBACnetStackAdapter.REINITIALIZED_STATE_ACTIVATE_CHANGES)
                {
                    database.NetworkPort.ChangesPending = false; 
                    return true; 
                } 
                else if (reinitializedStateOfDevice == CASBACnetStackAdapter.REINITIALIZED_STATE_WARM_START)
                {
                    // Flag for reboot and handle reboot after stack responds with SimpleAck.
                    database.NetworkPort.ChangesPending = false;
                    return true;
                } 

                // All other states are not supported in this example.
                *errorCode = CASBACnetStackAdapter.ERROR_OPTIONAL_FUNCTIONALITY_NOT_SUPPORTED;
                return false; 
            }
        }
    }
}
