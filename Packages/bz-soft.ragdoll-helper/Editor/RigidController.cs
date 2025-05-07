using UnityEditor;
using UnityEngine;

namespace Editor
{
	class RigidController
	{
		public static void DrawControllers(Transform transform)
		{
			var rigid = transform.GetComponent<Rigidbody>();
			if (rigid == null)
				return;

			Quaternion rotatorRotation;

			if (Tools.pivotRotation == PivotRotation.Global)
				rotatorRotation = Quaternion.identity;
			else
				rotatorRotation = transform.rotation;

			var pos = transform.position + transform.TransformDirection(rigid.centerOfMass);

			var newPosition = Handles.PositionHandle(pos, rotatorRotation);

			if (newPosition == pos)
				return;

			Undo.RecordObject(rigid, "Set Rigidbody");
			var centerOfMass = transform.InverseTransformDirection(newPosition - transform.position);
			rigid.centerOfMass = centerOfMass;
		}

		public static Vector3 GetPos(Transform transform)
		{
			var rigid = transform.GetComponent<Rigidbody>();
			if (rigid == null)
				return Vector3.zero;

			var pos = transform.position + transform.TransformDirection(rigid.centerOfMass);
			return pos;
		}
	}
}