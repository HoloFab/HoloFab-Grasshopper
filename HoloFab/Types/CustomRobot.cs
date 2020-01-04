using System;
using System.Collections.Generic;

namespace HoloFab {
	// Structure to hold Custom data types holding data to be sent.
	namespace CustomData {
		// Custom Robot Data.
		[Serializable]
		public class RobotData {
			public int robotID;
			public string robotName;
			public EndeffectorData endEffector;
			public double[] robotPlane;
			// //public double[] TCPplane;
			// //public int markerId;
            
			public RobotData() {
				this.robotID = 0;
				this.robotName = "Robot";
				this.endEffector = null;
				this.robotPlane = new double[] { 0, 0, 0, 1, 0, 0, 0 };
			}
			public RobotData(int _robotID, string _robotName, EndeffectorData _endEffector) {
				this.robotID = _robotID;
				this.robotName = _robotName;
				this.endEffector = _endEffector;
                this.robotPlane = new double[] { 0, 0, 0, 1, 0, 0, 0 };
            }
			public RobotData(int _robotID, string _robotName, EndeffectorData _endEffector, double[] _robotPlane) : this(_robotID, _robotName, _endEffector) {
                this.robotPlane = _robotPlane;
			}
		}
		// Custom Endeffector Mesh Data.
		[Serializable]
		public class EndeffectorData : MeshData {
			public EndeffectorData(MeshData item) : base() {
				if (item != null) {
					this.vertices = item.vertices;
					this.faces = item.faces;
				}
			}
		}
		// Custom Robot Controller.
		[Serializable]
		public struct RobotControllerData {
			public int robotID;
			public List<double> robotAxisAngles;
            
			public RobotControllerData(int _robotID, List<double> _robotAxisAngles) {
				this.robotID = _robotID;
				this.robotAxisAngles = _robotAxisAngles;
			}
		}
	}
}