using BACnetStackDLLServerCSharpExample;
using System;
using System.Collections.Generic;
using System.Text;

namespace BACnetServerExample
{
    class ExampleDatabase
    {
        public const UInt32 COUNT_ANALOG_INPUT = 3;
        public const UInt32 COUNT_ANALOG_OUTPUT = 3;
        public const UInt32 COUNT_ANALOG_VALUE = 3;
        public const UInt32 COUNT_BINARY_INPUT = 3;
        public const UInt32 COUNT_BINARY_VALUE = 3;
        public const UInt32 COUNT_MULTI_STATE_INPUT = 3;
        public const UInt32 COUNT_MULTI_STATE_VALUE = 3;

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
            public float presentValue;
            public String description;
            public UInt32 units;
            public bool outOfService;
        }
        public class ExampleDatabaseAnalogOutput : ExampleDatabaseBase
        {
            public bool[] priorityArrayNulls = new bool[16];
            public float[] priorityArrayValues = new float[16];
            public UInt32 units;
            public bool outOfService;
            public float relinquishDefault; 
        }

        public class ExampleDatabaseAnalogValue : ExampleDatabaseBase
        {
            public float presentValue;
            public UInt32 units;
            public bool outOfService;
        }

        public class ExampleDatabaseBinary : ExampleDatabaseBase
        {
            public bool presentValue;
        }

        public class ExampleDatabaseMultiState : ExampleDatabaseBase
        {
            public UInt32 presentValue;
            public string[] stateText;

            // https://en.wikipedia.org/wiki/Points_of_the_compass
            public static string[] _windCompassText = {
                "North (N)", "North by east (NbE)", "Northeast by north (NEbN)", "Northeast by east (NEbE)",
                "East (E)", "East by north (EbN)", "Southeast by east (SEbE)", "Southeast by south (SEbS)",
                "South (S)", "South by east (SbE)", "Southwest by south (SWbS)", "Southwest by west (SWbW)",
                "West (W)", "West by south (WbS)", "Northwest by west (NWbW)", "Northwest by north (NWbN)",
                "North by west (NbW)" }; 
        }

        public ExampleDatabaseDevice Device;
        public ExampleDatabaseAnalog[] AnalogInput;
        public ExampleDatabaseAnalogOutput[] AnalogOutput; 
        public ExampleDatabaseAnalogValue[] AnalogValue;
        public ExampleDatabaseBinary[] BinaryInput;
        public ExampleDatabaseBinary[] BinaryValue;
        public ExampleDatabaseMultiState[] MultiStateInput;
        public ExampleDatabaseMultiState[] MultiStateValue;


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
            this.Device = new ExampleDatabaseDevice();
            this.AnalogInput = new ExampleDatabaseAnalog[COUNT_ANALOG_INPUT];
            this.AnalogOutput = new ExampleDatabaseAnalogOutput[COUNT_ANALOG_OUTPUT];
            this.AnalogValue = new ExampleDatabaseAnalogValue[COUNT_ANALOG_VALUE];
            this.BinaryInput = new ExampleDatabaseBinary[COUNT_BINARY_INPUT];
            this.BinaryValue = new ExampleDatabaseBinary[COUNT_BINARY_VALUE];
            this.MultiStateInput = new ExampleDatabaseMultiState[COUNT_MULTI_STATE_INPUT] ;
            this.MultiStateValue = new ExampleDatabaseMultiState[COUNT_MULTI_STATE_VALUE];
            
            // Default Values 
            this.Device.name = "Device name Rainbow";
            this.Device.instance = 389001; 
            this.Device.description = "This is the example description";
            this.Device.vendorIdentifiier = 389; // 389 is Chipkin's vendorIdentifiier
            this.Device.vendorName = "Chipkin Automation Systems";
            this.Device.modelName = "Windows-BACnetServerExampleCSharp";            

            for (UInt32 offset = 0; offset < this.AnalogInput.Length; offset++)
            {
                this.AnalogInput[offset] = new ExampleDatabaseAnalog();
                this.AnalogInput[offset].presentValue = (float)offset * 100f + offset * 0.1f;
                this.AnalogInput[offset].name = "AnalogInput " + this.GetColorName();
                this.AnalogInput[offset].description = "Example description";
                this.AnalogInput[offset].units = CASBACnetStackAdapter.ENGINEERING_UNITS_NO_UNITS;
                this.AnalogInput[offset].outOfService = false;
            }
            for (UInt32 offset = 0; offset < this.AnalogOutput.Length; offset++)
            {
                this.AnalogOutput[offset] = new ExampleDatabaseAnalogOutput();
                this.AnalogOutput[offset].name = "AnalogOutput " + this.GetColorName();
                this.AnalogOutput[offset].units = CASBACnetStackAdapter.ENGINEERING_UNITS_NO_UNITS;
                this.AnalogOutput[offset].outOfService = false;
                this.AnalogOutput[offset].relinquishDefault =  (float)offset * 1.1f;
                for (int index = 0 ; index < this.AnalogOutput[offset].priorityArrayNulls.Length; index++)
                {
                    this.AnalogOutput[offset].priorityArrayNulls[index] = true; 
                }
            }
            for (UInt32 offset = 0; offset < this.AnalogValue.Length; offset++)
            {
                this.AnalogValue[offset] = new ExampleDatabaseAnalogValue();
                this.AnalogValue[offset].presentValue = offset * 100f + offset * 0.1f;
                this.AnalogValue[offset].name = "AnalogValue " + this.GetColorName();
                this.AnalogValue[offset].units = CASBACnetStackAdapter.ENGINEERING_UNITS_NO_UNITS;
                this.AnalogValue[offset].outOfService = false;
            }
            for (UInt32 offset = 0; offset < this.BinaryInput.Length; offset++)
            {
                this.BinaryInput[offset] = new ExampleDatabaseBinary();
                this.BinaryInput[offset].presentValue = false;
                this.BinaryInput[offset].name = "BinaryInput " + this.GetColorName();
            }
            for (UInt32 offset = 0; offset < this.BinaryValue.Length; offset++)
            {
                this.BinaryValue[offset] = new ExampleDatabaseBinary();
                this.BinaryValue[offset].presentValue = false;
                this.BinaryValue[offset].name = "BinaryValue " + this.GetColorName();
            }
            for (UInt32 offset = 0; offset < this.MultiStateInput.Length; offset++)
            {
                this.MultiStateInput[offset] = new ExampleDatabaseMultiState();
                this.MultiStateInput[offset].name = "MultiStateInput " + this.GetColorName();
                this.MultiStateInput[offset].stateText = ExampleDatabaseMultiState._windCompassText;
                this.MultiStateInput[offset].presentValue = Convert.ToUInt32(offset % this.MultiStateInput[offset].stateText.Length);
            }
            for (UInt32 offset = 0; offset < this.MultiStateValue.Length; offset++)
            {
                this.MultiStateValue[offset] = new ExampleDatabaseMultiState();
                this.MultiStateValue[offset].name = "MultiStateValue " + this.GetColorName();
                this.MultiStateValue[offset].stateText = ExampleDatabaseMultiState._windCompassText;
                this.MultiStateValue[offset].presentValue = Convert.ToUInt32(offset % this.MultiStateValue[offset].stateText.Length);
            }
        }

        public void Loop()
        {

        }
    }
}
