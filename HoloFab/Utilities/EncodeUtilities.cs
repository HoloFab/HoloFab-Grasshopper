using System;

using System.Text;
using Newtonsoft.Json;
// Rhino only includes
using System.Drawing;
using Rhino.Geometry;

using HoloFab.CustomData;

namespace HoloFab {
	// Tools for processing robit data.
	public static partial class EncodeUtilities {
		public static double[] EncodePlane(Plane _plane) {
			Quaternion quaternion = new Quaternion();
			quaternion.SetRotation(Plane.WorldXY, _plane);
            
			double [] transformation = new double[] {
				_plane.OriginX/1000, _plane.OriginZ/1000, _plane.OriginY/1000,
				quaternion.A, quaternion.B, quaternion.C, quaternion.D
			};
			return transformation;
		}
		// Encode a Color.
		public static int[] EncodeColor(Color _color) {
			return new int[] { _color.A, _color.R, _color.G, _color.B };
		}
		// Encode a Location.
		public static float[] EncodeLocation(Point3d _point){
			return new float[] {(float)Math.Round(_point.X/1000.0,3),
					            (float)Math.Round(_point.Z/1000.0,3),
					            (float)Math.Round(_point.Y/1000.0,3)};
		}
	}
	// Part Shared with Unity.
	// Tools for processing robit data.
	public static partial class EncodeUtilities {
		public static string headerSplitter = "|";
		public static string messageSplitter = "~";
		// Encode data into a json readable byte array.
		public static byte[] EncodeData(string header, System.Object item, out string message){
			string output = JsonConvert.SerializeObject(item);
			if (header != string.Empty)
				message = header + EncodeUtilities.headerSplitter + output;
			else
				message = output;
			message += EncodeUtilities.messageSplitter; // End Message Char
			return Encoding.UTF8.GetBytes(message);
		}
		// If message wsn't stripped - remove the message splitter
		public static string StripSplitter(string message){
			if (message.EndsWith(EncodeUtilities.messageSplitter))
				return message.Substring(0, message.Length - 1);
			return message;
		}
		// Decode Data into a string.
		public static string DecodeData(byte[] data) {
			return Encoding.UTF8.GetString(data);
		}
		// Decode Data into a string.
		public static string DecodeData(byte[] data, int index, int count) {
			return Encoding.UTF8.GetString(data, index, count);
		}
	}
}