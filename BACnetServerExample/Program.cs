﻿/**
 * Windows BACnet Server Example CSharp
 * ----------------------------------------------------------------------------
 * In this CAS BACnet Stack example, we create a BACnet IP server with the basic object types. 
 *
 * More information https://github.com/chipkin/Windows-BACnetServerExampleCSharp
 * 
 * Created by: Steven Smethurst 
 * Created on: June 7, 2019 
 * Last updated: June 7, 2019 
 */


using BACnetStackDLLServerCSharpExample;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace BACnetServerExample
{
    class Program
    {
        static void Main(string[] args)
        {
            BACnetServer bacnetServer = new BACnetServer();
            bacnetServer.Run();
        }

        unsafe class BACnetServer
        {
            // UDP 
            UdpClient udpServer;
            IPEndPoint RemoteIpEndPoint;

            // Settings 
            const UInt16 SETTING_BACNET_PORT = 47808;

            // A Database to hold the current state of the 
            private ExampleDatabase database = new ExampleDatabase();

            // Version 
            const string APPLICATION_VERSION = "0.0.1";

            public void Run()
            {
                Console.WriteLine("Starting Windows-BACnetServerExampleCSharp version{0}.{1}", APPLICATION_VERSION, CIBuildVersion.CIBUILDNUMBER);
                Console.WriteLine("https://github.com/chipkin/Windows-BACnetServerExampleCSharp");
                Console.WriteLine("FYI: BACnet Stack version: {0}.{1}.{2}.{3}",
                    CASBACnetStackAdapter.GetAPIMajorVersion(),
                    CASBACnetStackAdapter.GetAPIMinorVersion(),
                    CASBACnetStackAdapter.GetAPIPatchVersion(),
                    CASBACnetStackAdapter.GetAPIBuildVersion());

                // Send/Recv callbacks. 
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


                this.database.Setup();

                // Add the device. 
                CASBACnetStackAdapter.AddDevice(this.database.Device.instance);

                // AnalogInput
                for (UInt32 offset = 0; offset < this.database.AnalogInput.Length; offset++)
                {
                    CASBACnetStackAdapter.AddObject(database.Device.instance, CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_INPUT, offset);
                }
                CASBACnetStackAdapter.SetPropertyByObjectTypeEnabled(database.Device.instance, CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_INPUT, CASBACnetStackAdapter.PROPERTY_IDENTIFIER_DESCRIPTION, true);

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




                // Enable optional services 
                CASBACnetStackAdapter.SetServiceEnabled(database.Device.instance, CASBACnetStackAdapter.SERVICE_READ_PROPERTY_MULTIPLE, true);
                CASBACnetStackAdapter.SetServiceEnabled(database.Device.instance, CASBACnetStackAdapter.SERVICE_WRITE_PROPERTY, true);
                CASBACnetStackAdapter.SetServiceEnabled(database.Device.instance, CASBACnetStackAdapter.SERVICE_WRITE_PROPERTY_MULTIPLE, true);
                CASBACnetStackAdapter.SetServiceEnabled(database.Device.instance, CASBACnetStackAdapter.SERVICE_SUBSCRIBE_COV, true);

                // All done with the BACnet setup. 
                Console.WriteLine("FYI: CAS BACnet Stack Setup, successfuly");

                // Open the BACnet port to recive messages. 
                this.udpServer = new UdpClient(SETTING_BACNET_PORT);
                this.RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

                // Main loop.
                Console.WriteLine("FYI: Starting main loop");
                for (; ; )
                {
                    CASBACnetStackAdapter.Loop();
                    database.Loop();
                    DoUserInput();
                }
            }

            private void DoUserInput()
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.F1:
                            Console.WriteLine("FYI: BACnet Stack version: {0}.{1}.{2}.{3}",
                                CASBACnetStackAdapter.GetAPIMajorVersion(),
                                CASBACnetStackAdapter.GetAPIMinorVersion(),
                                CASBACnetStackAdapter.GetAPIPatchVersion(),
                                CASBACnetStackAdapter.GetAPIBuildVersion());
                            break;
                        case ConsoleKey.UpArrow:
                            if (this.database.AnalogInput.Length > 0)
                            {
                                this.database.AnalogInput[0].presentValue += 0.01f;
                                Console.WriteLine("FYI: Incurment Analog input {0} present value to {1:0.00}", 0, this.database.AnalogInput[0].presentValue);
                            }
                            break;
                        case ConsoleKey.DownArrow:
                            if (this.database.AnalogInput.Length > 0)
                            {
                                this.database.AnalogInput[0].presentValue -= 0.01f;
                                Console.WriteLine("FYI: Decrement Analog input {0} present value to {1:0.00}", 0, this.database.AnalogInput[0].presentValue);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }


            public ulong CallbackGetSystemTime()
            {
                // https://stackoverflow.com/questions/9453101/how-do-i-get-epoch-time-in-c
                return (ulong)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            }
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

            private UInt32 UpdateStringAndReturnSize(System.Char* value, UInt32 maxElementCount, string stringAsVallue)
            {
                byte[] nameAsBuffer = ASCIIEncoding.ASCII.GetBytes(stringAsVallue);
                UInt32 valueElementCount = maxElementCount;
                if (nameAsBuffer.Length < valueElementCount)
                {
                    valueElementCount = Convert.ToUInt32(nameAsBuffer.Length);
                }
                Marshal.Copy(nameAsBuffer, 0, (IntPtr)value, Convert.ToInt32(valueElementCount));
                return valueElementCount;
            }

            public bool CallbackGetPropertyCharString(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, System.Char* value, UInt32* valueElementCount, UInt32 maxElementCount, System.Byte encodingType, bool useArrayIndex, UInt32 propertyArrayIndex)
            {
                Console.WriteLine("FYI: Request for CallbackGetPropertyCharString. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);

                switch (objectType)
                {
                    case CASBACnetStackAdapter.OBJECT_TYPE_DEVICE:
                        if (deviceInstance == database.Device.instance && objectInstance == database.Device.instance)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OBJECT_NAME)
                            {
                                *valueElementCount = UpdateStringAndReturnSize(value, maxElementCount, database.Device.name);
                                return true;
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_MODEL_NAME)
                            {
                                *valueElementCount = UpdateStringAndReturnSize(value, maxElementCount, database.Device.modelName);
                                return true;
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_VENDOR_NAME)
                            {
                                *valueElementCount = UpdateStringAndReturnSize(value, maxElementCount, database.Device.vendorName);
                                return true;
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_APPLICATIONSOFTWAREVERSION)
                            {
                                string version = APPLICATION_VERSION + "." + CIBuildVersion.CIBUILDNUMBER; 
                                *valueElementCount = UpdateStringAndReturnSize(value, maxElementCount, version);
                                return true;
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_INPUT:
                        if (objectInstance < database.AnalogInput.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OBJECT_NAME)
                            {
                                *valueElementCount = UpdateStringAndReturnSize(value, maxElementCount, database.AnalogInput[objectInstance].name);
                                return true;
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_DESCRIPTION)
                            {
                                *valueElementCount = UpdateStringAndReturnSize(value, maxElementCount, database.AnalogInput[objectInstance].description);
                                return true;
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_OUTPUT:
                        if (objectInstance < database.AnalogOutput.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OBJECT_NAME)
                            {
                                *valueElementCount = UpdateStringAndReturnSize(value, maxElementCount, database.AnalogOutput[objectInstance].name);
                                return true;
                            }                            
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_VALUE:
                        if (objectInstance < database.AnalogValue.Length) {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OBJECT_NAME)
                            {
                                *valueElementCount = UpdateStringAndReturnSize(value, maxElementCount, database.AnalogValue[objectInstance].name);
                                return true;
                            }                           
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_BINARY_INPUT:
                        if (objectInstance < database.BinaryInput.Length) {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OBJECT_NAME)
                            {
                                *valueElementCount = UpdateStringAndReturnSize(value, maxElementCount, database.BinaryInput[objectInstance].name);
                                return true;
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_BINARY_VALUE:
                        if (objectInstance < database.BinaryValue.Length) {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OBJECT_NAME)
                            {
                                *valueElementCount = UpdateStringAndReturnSize(value, maxElementCount, database.BinaryValue[objectInstance].name);
                                return true;
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_MULTI_STATE_INPUT:
                        if (objectInstance < database.MultiStateInput.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OBJECT_NAME)
                            {
                                *valueElementCount = UpdateStringAndReturnSize(value, maxElementCount, database.MultiStateInput[objectInstance].name);
                                return true;
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_STATETEXT && useArrayIndex)
                            {
                                if (propertyArrayIndex <= database.MultiStateInput[objectInstance].stateText.Length)
                                {
                                    *valueElementCount = UpdateStringAndReturnSize(value, maxElementCount, database.MultiStateInput[objectInstance].stateText[propertyArrayIndex - 1]);
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
                                *valueElementCount = UpdateStringAndReturnSize(value, maxElementCount, database.MultiStateValue[objectInstance].name);
                                return true;
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_STATETEXT && useArrayIndex)
                            {
                                if (propertyArrayIndex <= database.MultiStateValue[objectInstance].stateText.Length)
                                {
                                    *valueElementCount = UpdateStringAndReturnSize(value, maxElementCount, database.MultiStateValue[objectInstance].stateText[propertyArrayIndex - 1]);
                                    return true;
                                }
                            }
                        }
                        break;

                    default:
                        break;
                }

                Console.WriteLine("   FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false; // Could not handle this request. 
            }

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
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_RELINQUISHDEFAULT )
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
                    default:
                        break;
                }

                Console.WriteLine("   FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }
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
                    default:
                        break;
                }

                Console.WriteLine("   FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }

            public bool CallbackGetUnsignedInteger(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, UInt32* value, bool useArrayIndex, UInt32 propertyArrayIndex)
            {
                Console.WriteLine("FYI: Request for CallbackGetUnsignedInteger. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);
                switch (objectType)
                {
                    case CASBACnetStackAdapter.OBJECT_TYPE_DEVICE:
                        if (deviceInstance == database.Device.instance && objectInstance == database.Device.instance)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OBJECT_NAME)
                            {
                                *value = database.Device.vendorIdentifiier;
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
                    default:
                        break;
                }
                Console.WriteLine("   FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }

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
                                if(propertyArrayIndex <= database.AnalogOutput[objectInstance].priorityArrayNulls.Length)
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
                    default:
                        break;
                }

                Console.WriteLine("   FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }


            bool CallbackGetPropertyDate(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, Byte* year, Byte* month, Byte* day, Byte* weekday, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex)
            {
                Console.WriteLine("FYI: Request for CallbackGetPropertyDate. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);
                Console.WriteLine("   FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }
            bool CallbackGetPropertyDouble(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, Double* value, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex)
            {
                Console.WriteLine("FYI: Request for CallbackGetPropertyDouble. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);
                Console.WriteLine("   FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }
            bool CallbackGetPropertySignedInteger(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, Int32* value, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex)
            {
                Console.WriteLine("FYI: Request for CallbackGetPropertySignedInteger. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);
                Console.WriteLine("   FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }
            bool CallbackGetPropertyTime(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, Byte* hour, Byte* minute, Byte* second, Byte* hundrethSeconds, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex)
            {
                Console.WriteLine("FYI: Request for CallbackGetPropertyTime. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);
                Console.WriteLine("   FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }
            bool CallbackGetPropertyOctetString(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, Byte* value, UInt32* valueElementCount, UInt32 maxElementCount, System.Byte encodingType, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex)
            {
                Console.WriteLine("FYI: Request for CallbackGetPropertyOctetString. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);
                Console.WriteLine("   FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }


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

                Console.WriteLine("   FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }

            public bool CallbackSetPropertyReal(bool deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, float value, bool useArrayIndex, UInt32 propertyArrayIndex, System.Byte priority, UInt32* errorCode)
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

                Console.WriteLine("   FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }

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
                    default:
                        break; 
                }

                Console.WriteLine("   FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false; 
            }

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

                Console.WriteLine("   FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false; 
            }

            bool CallbackSetPropertySignedInteger(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, Int32 value, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex, System.Byte priority, UInt32* errorCode)
            {
                Console.WriteLine("FYI: Request for CallbackSetPropertySignedInteger. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);
                Console.WriteLine("   FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }
            bool CallbackSetPropertyDouble(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, Double value, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex, System.Byte priority, UInt32* errorCode)
            {
                Console.WriteLine("FYI: Request for CallbackSetPropertyDouble. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);
                Console.WriteLine("   FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false; 
            }
            bool CallbackSetPropertyBool(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, bool value, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex, System.Byte priority, UInt32* errorCode)
            {
                Console.WriteLine("FYI: Request for CallbackSetPropertyBool. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);
                Console.WriteLine("   FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
            return false; 
            }
            bool CallbackSetPropertyTime(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, Byte hour, Byte minute, Byte second, Byte hundrethSeconds, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex, System.Byte priority, UInt32* errorCode)
            {
                Console.WriteLine("FYI: Request for CallbackSetPropertyTime. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);
                Console.WriteLine("   FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false; 
            }
            bool CallbackSetPropertyDate(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, Byte year, Byte month, Byte day, Byte weekday, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex, System.Byte priority, UInt32* errorCode)
            {
                Console.WriteLine("FYI: Request for CallbackSetPropertyDate. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);
                Console.WriteLine("   FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false; 
            }
            bool CallbackSetPropertyCharacterString(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, char* value, UInt32 length, Byte encodingType, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex, System.Byte priority, UInt32* errorCode)
            {
                Console.WriteLine("FYI: Request for CallbackSetPropertyCharacterString. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);
                Console.WriteLine("   FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }
            bool CallbackSetPropertyOctetString(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, Byte* value, UInt32 length, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex, System.Byte priority, UInt32* errorCode)
            {
                Console.WriteLine("FYI: Request for CallbackSetPropertyOctetString. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);
                Console.WriteLine("   FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }
            bool CallbackSetPropertyBitString(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, bool* value, UInt32 length, [In, MarshalAs(UnmanagedType.I1)] bool useArrayIndex, UInt32 propertyArrayIndex, System.Byte priority, UInt32* errorCode)
            {
                Console.WriteLine("FYI: Request for CallbackSetPropertyBitString. objectType={0}, objectInstance={1}, propertyIdentifier={2}, propertyArrayIndex={3}", objectType, objectInstance, propertyIdentifier, propertyArrayIndex);
                Console.WriteLine("   FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false;
            }
        }
    }
}
