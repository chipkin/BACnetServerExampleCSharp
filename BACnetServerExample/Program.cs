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

                // Set Datatype Callbacks 
                CASBACnetStackAdapter.RegisterCallbackSetPropertyReal(CallbackSetPropertyReal);
                CASBACnetStackAdapter.RegisterCallbackSetPropertyEnumerated(CallbackSetPropertyEnumerated);


                this.database.Setup(); 

                // Add the device. 
                CASBACnetStackAdapter.AddDevice(this.database.device.instance);

                // AnalogInput
                for (UInt32 offset = 0; offset < this.database.AnalogInput.Length; offset++)
                {
                    CASBACnetStackAdapter.AddObject(database.device.instance, CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_INPUT, offset);
                }
                CASBACnetStackAdapter.SetPropertyByObjectTypeEnabled(database.device.instance, CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_INPUT, CASBACnetStackAdapter.PROPERTY_IDENTIFIER_DESCRIPTION, true);

                // AnalogValue
                for (UInt32 offset = 0; offset < this.database.AnalogValue.Length; offset++)
                {
                    CASBACnetStackAdapter.AddObject(database.device.instance, CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_VALUE, offset);
                    CASBACnetStackAdapter.SetPropertyWritable(database.device.instance, CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_VALUE, offset, CASBACnetStackAdapter.PROPERTY_IDENTIFIER_PRESENT_VALUE, true);
                }
                CASBACnetStackAdapter.SetPropertyByObjectTypeEnabled(database.device.instance, CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_VALUE, CASBACnetStackAdapter.PROPERTY_IDENTIFIER_DESCRIPTION, true);

                // BinaryInput
                for (UInt32 offset = 0; offset < this.database.BinaryInput.Length; offset++)
                {
                    CASBACnetStackAdapter.AddObject(database.device.instance, CASBACnetStackAdapter.OBJECT_TYPE_BINARY_INPUT, offset);
                }

                // BinaryValue
                for (UInt32 offset = 0; offset < this.database.BinaryValue.Length; offset++)
                {
                    CASBACnetStackAdapter.AddObject(database.device.instance, CASBACnetStackAdapter.OBJECT_TYPE_BINARY_VALUE, offset);
                    CASBACnetStackAdapter.SetPropertyWritable(database.device.instance, CASBACnetStackAdapter.OBJECT_TYPE_BINARY_VALUE, offset, CASBACnetStackAdapter.PROPERTY_IDENTIFIER_PRESENT_VALUE, true);
                }


                // Enable optional services 
                CASBACnetStackAdapter.SetServiceEnabled(database.device.instance, CASBACnetStackAdapter.SERVICE_READ_PROPERTY_MULTIPLE, true);
                CASBACnetStackAdapter.SetServiceEnabled(database.device.instance, CASBACnetStackAdapter.SERVICE_WRITE_PROPERTY, true);
                CASBACnetStackAdapter.SetServiceEnabled(database.device.instance, CASBACnetStackAdapter.SERVICE_WRITE_PROPERTY_MULTIPLE, true);
                CASBACnetStackAdapter.SetServiceEnabled(database.device.instance, CASBACnetStackAdapter.SERVICE_SUBSCRIBE_COV, true);

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
                                this.database.AnalogInput[0].value += 0.01f;
                                Console.WriteLine("FYI: Incurment Analog input {0} present value to {1:0.00}", 0, this.database.AnalogInput[0].value);
                            }
                            break;
                        case ConsoleKey.DownArrow:
                            if (this.database.AnalogInput.Length > 0)
                            {
                                this.database.AnalogInput[0].value -= 0.01f;
                                Console.WriteLine("FYI: Decrement Analog input {0} present value to {1:0.00}", 0, this.database.AnalogInput[0].value);
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

                switch(objectType)
                {
                    case CASBACnetStackAdapter.OBJECT_TYPE_DEVICE:
                        if (deviceInstance == database.device.instance && objectInstance == database.device.instance && propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OBJECT_NAME)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OBJECT_NAME)
                            {
                                *valueElementCount = UpdateStringAndReturnSize(value, maxElementCount, database.device.name);
                                return true;
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_MODEL_NAME)
                            {
                                *valueElementCount = UpdateStringAndReturnSize(value, maxElementCount, database.device.modelName);
                                return true;
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_VENDOR_NAME)
                            {
                                *valueElementCount = UpdateStringAndReturnSize(value, maxElementCount, database.device.vendorName);
                                return true;
                            }
                        }
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_INPUT:
                        if (objectInstance < database.AnalogInput.Length) {
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
                    case CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_VALUE:
                        if (objectInstance < database.AnalogValue.Length) {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OBJECT_NAME)
                            {
                                *valueElementCount = UpdateStringAndReturnSize(value, maxElementCount, database.AnalogValue[objectInstance].name);
                                return true;
                            }
                            else if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_DESCRIPTION)
                            {
                                *valueElementCount = UpdateStringAndReturnSize(value, maxElementCount, database.AnalogValue[objectInstance].description);
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

                    default:
                        break; 
                }
                
                Console.WriteLine("   FYI: Not implmented. propertyIdentifier={0}", propertyIdentifier);
                return false; // Could not handle this request. 
            }

            public bool CallbackGetPropertyReal(UInt32 deviceInstance, UInt16 objectType, UInt32 objectInstance, UInt32 propertyIdentifier, float* value, bool useArrayIndex, UInt32 propertyArrayIndex)
            {
                Console.WriteLine("FYI: Request for CallbackGetPropertyReal. objectType={0}, objectInstance={1}, propertyIdentifier={2}", objectType, objectInstance, propertyIdentifier);

                switch (objectType)
                {
                    case CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_INPUT:
                        if (objectInstance < database.AnalogInput.Length) {
                            *value = database.AnalogInput[objectInstance].value;
                            Console.WriteLine("FYI: AnalogInput[{0}].value got [{1}]", objectInstance, database.AnalogInput[objectInstance].value);
                            return true;
                        }
                        break; 
                    case CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_VALUE:
                        if (objectInstance < database.AnalogValue.Length) {
                            *value = database.AnalogValue[objectInstance].value;
                            Console.WriteLine("FYI: AnalogValue[{0}].value got [{1}]", objectInstance, database.AnalogValue[objectInstance].value);
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
                                *value = (UInt32)(database.BinaryInput[objectInstance].value ? 1 : 0);
                                Console.WriteLine("FYI: BinaryInput[{0}].value got [{1}]", objectInstance, database.BinaryInput[objectInstance].value);
                                return true;
                            }
                        }                        
                        break;
                    case CASBACnetStackAdapter.OBJECT_TYPE_BINARY_VALUE:
                        if (objectInstance < database.BinaryValue.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_PRESENT_VALUE) {
                                *value = (UInt32)(database.BinaryValue[objectInstance].value ? 1 : 0);
                                Console.WriteLine("FYI: BinaryValue[{0}].value got [{1}]", objectInstance, database.BinaryValue[objectInstance].value);
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
                        if (deviceInstance == database.device.instance && objectInstance == database.device.instance )
                        { 
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OBJECT_NAME)
                            {
                                *value = database.device.vendorIdentifiier;
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

                switch(objectType)
                {
                    case CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_INPUT:
                        if (objectInstance < database.AnalogInput.Length) {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_OUT_OF_SERVICE)
                            {
                                *value = database.AnalogInput[objectInstance].outOfService;
                                Console.WriteLine("FYI: AnalogInput[{0}].outOfService got [{1}]", objectInstance, database.AnalogInput[objectInstance].outOfService);
                                return true;
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
                                database.BinaryValue[objectInstance].value = (value == 1);
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
                    case CASBACnetStackAdapter.OBJECT_TYPE_ANALOG_VALUE:
                        if (objectInstance < database.AnalogValue.Length)
                        {
                            if (propertyIdentifier == CASBACnetStackAdapter.PROPERTY_IDENTIFIER_PRESENT_VALUE) { 
                                database.AnalogValue[objectInstance].value = value;
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
        }
    }
}
