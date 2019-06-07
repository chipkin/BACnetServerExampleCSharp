using BACnetStackDLLServerCSharpExample;
using System;
using System.Collections.Generic;
using System.Text;

namespace BACnetServerExample
{
    class ExampleDatabase
    {
        public const UInt32 COUNT_ANALOG_INPUT = 3;
        public const UInt32 COUNT_ANALOG_VALUE = 3;
        public const UInt32 COUNT_BINARY_INPUT = 3;
        public const UInt32 COUNT_BINARY_VALUE = 3;

        public class ExampleDatabaseBase
        {
            public String name;
        }
        public class ExampleDatabaseDevice : ExampleDatabaseBase
        {
            public String description;
            public UInt32 instance;
            public String modelName;
            public UInt32 vendorIdentifiier;
            public String vendorName;
        }
        public class ExampleDatabaseAnalog : ExampleDatabaseBase
        {
            public float value;
            public String description;
            public UInt32 units;
            public bool outOfService;
        }
        public class ExampleDatabaseBinary : ExampleDatabaseBase
        {
            public bool value;
        }

        public ExampleDatabaseDevice device;
        public ExampleDatabaseAnalog[] AnalogInput;
        public ExampleDatabaseAnalog[] AnalogValue;
        public ExampleDatabaseBinary[] BinaryInput;
        public ExampleDatabaseBinary[] BinaryValue;


        // This function returns a unique name for each object. 
        // https://en.wikipedia.org/wiki/List_of_colors_(compact)
        private static string[] colors = { "Amber", "Bronze", "Chartreuse", "Diamond", "Emerald", "Fuchsia", "Gold", "Hot Pink", "Indigo",
                                "Kiwi", "Lilac", "Magenta", "Nickel", "Onyx", "Purple", "Quartz", "Red", "Silver", "Turquoise",
                                "Umber", "Vermilion", "White", "Xanadu", "Yellow", "Zebra White" };
        private static int offsetColors = 0;
        public string GetColorName()
        {
            string name = ExampleDatabase.colors[ExampleDatabase.offsetColors % ExampleDatabase.colors.Length];
            if (ExampleDatabase.offsetColors / ExampleDatabase.colors.Length > 1 ) {
                name += " " + Convert.ToUInt32(ExampleDatabase.offsetColors / ExampleDatabase.colors.Length); 
            }

            ExampleDatabase.offsetColors++;
            return name; 
        }

        public void Setup()
        {
            this.device = new ExampleDatabaseDevice();
            this.AnalogInput = new ExampleDatabaseAnalog[COUNT_ANALOG_INPUT];
            this.AnalogValue = new ExampleDatabaseAnalog[COUNT_ANALOG_VALUE];
            this.BinaryInput = new ExampleDatabaseBinary[COUNT_BINARY_INPUT];
            this.BinaryValue = new ExampleDatabaseBinary[COUNT_BINARY_VALUE];

 


            // Default Values 
            this.device.name = "Device name Rainbow";
            this.device.instance = 389001; 
            this.device.description = "This is the example description";
            this.device.vendorIdentifiier = 389; // 389 is Chipkin's vendorIdentifiier
            this.device.vendorName = "Chipkin Automation Systems";
            this.device.modelName = "CAS BACnet Stack";

            

            for (UInt32 offset = 0; offset < this.AnalogInput.Length; offset++)
            {
                this.AnalogInput[offset] = new ExampleDatabaseAnalog();
                this.AnalogInput[offset].value = (float)offset * 100f + offset * 0.1f;
                this.AnalogInput[offset].name = "AnalogInput " + this.GetColorName();
                this.AnalogInput[offset].description = "Example description";
                this.AnalogInput[offset].units = CASBACnetStackAdapter.ENGINEERING_UNITS_NO_UNITS;
                this.AnalogInput[offset].outOfService = false;
            }
            for (UInt32 offset = 0; offset < this.AnalogValue.Length; offset++)
            {
                this.AnalogValue[offset] = new ExampleDatabaseAnalog();
                this.AnalogValue[offset].value = offset * 100f + offset * 0.1f;
                this.AnalogValue[offset].name = "AnalogValue " + this.GetColorName();
                this.AnalogValue[offset].description = "Example description";
                this.AnalogValue[offset].units = CASBACnetStackAdapter.ENGINEERING_UNITS_NO_UNITS;
                this.AnalogValue[offset].outOfService = false;
            }
            for (UInt32 offset = 0; offset < this.BinaryInput.Length; offset++)
            {
                this.BinaryInput[offset] = new ExampleDatabaseBinary();
                this.BinaryInput[offset].value = false;
                this.BinaryInput[offset].name = "BinaryInput " + this.GetColorName();
            }
            for (UInt32 offset = 0; offset < this.BinaryValue.Length; offset++)
            {
                this.BinaryValue[offset] = new ExampleDatabaseBinary();
                this.BinaryValue[offset].value = false;
                this.BinaryValue[offset].name = "BinaryValue " + this.GetColorName();
            }
        }

        public void Loop()
        {

        }
    }
}
