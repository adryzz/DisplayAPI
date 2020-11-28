using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DisplayAPI
{
    /// <summary>
    /// Allows for easy access to displays and their settings.
    /// </summary>
    public class Display
    {
        DISPLAY_DEVICE Device;

        public DEVMODE Mode;

        public Orientations Orientation { get; protected set; }

        public uint DisplayNumber { get; protected set; }

        public string Name { get; protected set; } = "";

        public string DeviceString { get; protected set; } = "";

        public string DeviceID { get; protected set; } = "";

        public string DeviceKey { get; protected set; } = "";

        public DisplayDeviceStateFlags DeviceState { get; protected set; }

        public DevMode DevMode { get; protected set; }

        /// <summary>
        /// Initializes a new display object.
        /// </summary>
        /// <param name="displayNumber"></param>
        public Display(uint displayNumber)
        {
            Device = new DISPLAY_DEVICE();
            Mode = new DEVMODE();
            Device.cb = Marshal.SizeOf(Device);

            if (!NativeMethods.EnumDisplayDevices(null, displayNumber, ref Device, 0))
                throw new ArgumentOutOfRangeException("DisplayNumber", displayNumber, "Number is greater than connected displays.");

            DisplayNumber = displayNumber;

            NativeMethods.EnumDisplaySettings(Device.DeviceName, NativeMethods.ENUM_CURRENT_SETTINGS, ref Mode);

            Name = Device.DeviceName;
            DeviceString = Device.DeviceString;
            DeviceID = Device.DeviceID;
            DeviceKey = Device.DeviceKey;
            DeviceState = Device.StateFlags;
            DevMode = Mode.dmFields;
        }

        public void UpdateValues()
        {
            NativeMethods.EnumDisplaySettings(Name, NativeMethods.ENUM_CURRENT_SETTINGS, ref Mode);
        }

        /// <summary>
        /// Rotates a display
        /// </summary>
        /// <param name="NewOrientation">The orientation of the display</param>
        /// <returns>Returns true if it succeeds</returns>
        public bool Rotate(Orientations NewOrientation)
        {
            bool result = false;

            if (0 != NativeMethods.EnumDisplaySettings(
                Device.DeviceName, NativeMethods.ENUM_CURRENT_SETTINGS, ref Mode))
            {
                if ((Mode.dmDisplayOrientation + (int)Orientation) % 2 == 1) // Need to swap height and width?
                {
                    int temp = Mode.dmPelsHeight;
                    Mode.dmPelsHeight = Mode.dmPelsWidth;
                    Mode.dmPelsWidth = temp;
                }

                switch (Orientation)
                {
                    case Orientations.DEGREES_CW_90:
                        Mode.dmDisplayOrientation = NativeMethods.DMDO_270;
                        break;
                    case Orientations.DEGREES_CW_180:
                        Mode.dmDisplayOrientation = NativeMethods.DMDO_180;
                        break;
                    case Orientations.DEGREES_CW_270:
                        Mode.dmDisplayOrientation = NativeMethods.DMDO_90;
                        break;
                    case Orientations.DEGREES_CW_0:
                        Mode.dmDisplayOrientation = NativeMethods.DMDO_DEFAULT;
                        break;
                    default:
                        break;
                }

                DISP_CHANGE ret = NativeMethods.ChangeDisplaySettingsEx(
                    Device.DeviceName, ref Mode, IntPtr.Zero,
                    DisplaySettingsFlags.CDS_UPDATEREGISTRY, IntPtr.Zero);

                result = ret == 0;
            }

            return result;
        }

        public static IEnumerable<Display> EnumerateConnectedDisplays()
        {
            return null;
        }


        /// <summary>
        /// The orientation of a display
        /// </summary>
        public enum Orientations
        {
            DEGREES_CW_0 = 0,
            DEGREES_CW_90 = 3,
            DEGREES_CW_180 = 2,
            DEGREES_CW_270 = 1
        }
    }
}
