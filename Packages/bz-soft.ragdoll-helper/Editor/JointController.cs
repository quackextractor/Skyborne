﻿using UnityEditor;
using UnityEngine;

namespace Editor
{
	/// <summary>
	/// Joint controller. Draws the controls for 'CharacterJoint' components in scene view.
	/// </summary>
	public static class JointController
	{
		/// <summary>
		/// Draws the controllers. Need to be invoked from 'OnSceneGUI()' method.
		/// </summary>
		/// <param name="joint">Joint.</param>
		public static void DrawControllers(BoneHelper boneHelper, Transform transform)
		{
			var joint = transform.GetComponent<CharacterJoint>();
			if (joint == null)
				return;

			Undo.RecordObject(joint, "Set Joint");
			CharacterJoint symJoint = null;
			Transform symBone;
			if (boneHelper.SymmetricBones != null && boneHelper.SymmetricBones.TryGetValue(transform.name, out symBone))
			{
				symJoint = symBone.GetComponent<CharacterJoint>();
				if (symJoint == null)
					return;

				Undo.RecordObject(symJoint, "Setup symetric joint");
			}

			var backupColor = Handles.color;
			var position = joint.transform.position + joint.anchor;
			var size = HandleUtility.GetHandleSize(position);										// red
			var swingAxisDir = joint.transform.TransformDirection(joint.swingAxis).normalized;	// green
			var axisDir = joint.transform.TransformDirection(joint.axis).normalized;			// yellow
			var direction = GetDirection(joint, swingAxisDir, axisDir);

			DrawTwist(joint, symJoint, position, direction, axisDir, size);
			DrawSwing1(joint, symJoint, position, direction, axisDir, swingAxisDir, size);
			DrawSwing2(joint, symJoint, position, direction, swingAxisDir, size);

			var currRot = Quaternion.LookRotation(swingAxisDir, axisDir);
			var newRotation = Handles.RotationHandle(currRot, position);


			joint.swingAxis = joint.transform.InverseTransformDirection(newRotation * Vector3.forward);	// green
			joint.axis = joint.transform.InverseTransformDirection(newRotation * Vector3.up);           // yellow

			Handles.color = backupColor;
		}

		static void DrawTwist(CharacterJoint joint, CharacterJoint symJoint, Vector3 position, Vector3 direction, Vector3 axisDir, float size)
		{
			Handles.color = new Color(0.7f, 0.7f, 0.0f, 1f);
			Handles.ArrowHandleCap(0, position, Quaternion.LookRotation(axisDir), size * 1.1f, EventType.Repaint);

			Handles.color = new Color(0.7f, 0.7f, 0.0f, 1f);
			var twistNoraml = axisDir;
			var highLimit = joint.highTwistLimit;
			var lowLimit = joint.lowTwistLimit;

			var newHightLimit = highLimit.limit;
			var newLowLimit = lowLimit.limit;

			newHightLimit = -ProcessLimit(position, twistNoraml, direction, size, -newHightLimit);
			newLowLimit = -ProcessLimit(position, twistNoraml, direction, size, -newLowLimit);


			if (highLimit.limit != newHightLimit)
			{
				highLimit.limit = newHightLimit;
				joint.highTwistLimit = highLimit;
				if (symJoint != null)
				{
					symJoint.highTwistLimit = highLimit;
				}
			}

			if (lowLimit.limit != newLowLimit)
			{
				lowLimit.limit = newLowLimit;
				joint.lowTwistLimit = lowLimit;
				if (symJoint != null)
				{
					symJoint.lowTwistLimit = lowLimit;
				}
			}
		}

		static void DrawSwing1(CharacterJoint joint, CharacterJoint symJoint, Vector3 position, Vector3 direction, Vector3 axisDir, Vector3 swingAxisDir, float size)
		{
			Handles.color = new Color(0.0f, 0.7f, 0.0f, 1f);
			Handles.ArrowHandleCap(0, position, Quaternion.LookRotation(swingAxisDir), size * 1.1f, EventType.Repaint);

			Handles.color = new Color(0.0f, 0.7f, 0.0f, 1f);
			var swing1Noraml = Vector3.Cross(axisDir, direction);
			var swing1Limit = joint.swing1Limit;
			var newLimit = swing1Limit.limit;
			newLimit = ProcessLimit(position, swing1Noraml, direction, size, newLimit);
			newLimit = -ProcessLimit(position, swing1Noraml, direction, size, -newLimit);

			if (newLimit < 10f)
				newLimit = 0f;

			if (swing1Limit.limit != newLimit)
			{
				swing1Limit.limit = newLimit;
				joint.swing1Limit = swing1Limit;
				if (symJoint != null)
				{
					symJoint.swing1Limit = swing1Limit;
				}
			}
		}

		static void DrawSwing2(CharacterJoint joint, CharacterJoint symJoint, Vector3 position, Vector3 direction, Vector3 swingAxisDir, float size)
		{
			Handles.color = new Color(1f, 0f, 0f, 1f);
			Handles.ArrowHandleCap(0, position, Quaternion.LookRotation(direction), size * 2f, EventType.Repaint);
			
			Handles.color = new Color(0.0f, 0.0f, 0.7f, 1f);
			var swing2Noraml = direction;
			var swing2Limit = joint.swing2Limit;
			var newLimit = swing2Limit.limit;
			newLimit = ProcessLimit(position, swing2Noraml, swingAxisDir, size, newLimit);
			newLimit = -ProcessLimit(position, swing2Noraml, swingAxisDir, size, -newLimit);

			if (newLimit < 10f)
				newLimit = 0f;

			if (swing2Limit.limit != newLimit)
			{
				swing2Limit.limit = newLimit;
				joint.swing2Limit = swing2Limit;
				if (symJoint != null)
				{
					symJoint.swing2Limit = swing2Limit;
				}
			}
		}

		static Vector3 GetDirection(CharacterJoint joint, Vector3 swingAxisDir, Vector3 axisDir)
		{
			var direction = Vector3.Cross(swingAxisDir, axisDir);
			var direction2 = GetDirection(joint);

			//Handles.color = new Color(1f, 0f, 0f, 1f);
			//Handles.DrawLine(joint.transform.position, joint.transform.position + direction * 100);
			//Handles.color = new Color(0f, 1f, 0f, 1f);
			//Handles.DrawLine(joint.transform.position, joint.transform.position + direction2 * 100);
			var r = Vector3.Dot(direction, direction2);

			return direction *Mathf.Sign(r);
		}

		static Vector3 GetDirection(CharacterJoint joint)
		{
			var transform = joint.transform;
			if (transform.childCount == 0)
			{
				// in now children. Return direction related to parent
				return (joint.transform.position - joint.connectedBody.transform.position).normalized;
			}
			var direction = Vector3.zero;

			for (var ch = 0; ch < transform.childCount; ++ch)
			{
				// take to account colliders that attached to children
				var colliders = transform.GetChild(ch).GetComponents<Collider>();
				for (var i = 0; i < colliders.Length; ++i)
				{
					var collider = colliders[i];
					var cCollider = collider as CapsuleCollider;
					var bCollider = collider as BoxCollider;
					var sCollider = collider as SphereCollider;
					if (cCollider != null)
						direction += collider.transform.TransformDirection(cCollider.center);
					if (bCollider != null)
						direction += collider.transform.TransformDirection(bCollider.center);
					if (sCollider != null)
						direction += collider.transform.TransformDirection(sCollider.center);
				}
			}

			// if colliders was found, return average direction to colliders.
			if (direction != Vector3.zero)
				return direction.normalized;

			// otherwise, take direction to first child
			for (var i = 0; i < transform.childCount; ++i)
				direction += transform.GetChild(i).localPosition;
			return transform.TransformDirection(direction).normalized;
		}

		/// <summary>
		/// Draws arc with controls
		/// </summary>
		/// <returns>New limit.</returns>
		/// <param name="position">Position of center of arc</param>
		/// <param name="planeNormal">Plane normal in which arc are to be drawn</param>
		/// <param name="startDir">Start direction of arc</param>
		/// <param name="size">Radius of arc</param>
		/// <param name="limit">Current limit</param>
		static float ProcessLimit(Vector3 position, Vector3 planeNormal, Vector3 startDir, float size, float limit)
		{
			var cross = Vector3.Cross(planeNormal, startDir);
			startDir = Vector3.Cross(cross, planeNormal);

			var controllerDir = (Quaternion.AngleAxis(limit, planeNormal) * startDir);
			var controllerPos = position + (controllerDir * size * 1.2f);

			var backupColor = Handles.color;
			var newColor = backupColor * 2;
			newColor.a = 1f;
			Handles.color = newColor;
			Handles.DrawLine(position, controllerPos);

			newColor.a = 0.2f;
			Handles.color = newColor;

			Handles.DrawSolidArc(
				position,
				planeNormal,
				startDir,
				limit, size);

			newColor.a = 1f;
			Handles.color = newColor;

#if UNITY_2022_1_OR_NEWER
			var positionChanged = Handles.FreeMoveHandle(controllerPos, size * 0.1f, Vector3.zero, Handles.SphereHandleCap) != controllerPos;
#else
			bool positionChanged = Handles.FreeMoveHandle(controllerPos, Quaternion.identity, size * 0.1f, Vector3.zero, Handles.SphereHandleCap) != controllerPos;
#endif
			if (positionChanged)
			{
				var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
				float rayDistance;

				var plane = new Plane(planeNormal, position);
				if (plane.Raycast(ray, out rayDistance))
					controllerPos = ray.GetPoint(rayDistance);
				controllerPos = position + (controllerPos - position).normalized * size * 1.2f;

				// Get the angle in degrees between 0 and 180
				limit = Vector3.Angle(startDir, controllerPos - position);
				// Determine if the degree value should be negative.  Here, a positive value
				// from the dot product means that our vector is on the right of the reference vector   
				// whereas a negative value means we're on the left.
				var sign = Mathf.Sign(Vector3.Dot(cross, controllerPos - position));
				limit *= sign;

				limit = Mathf.Round(limit / 5f) * 5f;	// i need this to snap rotation
			}

			Handles.color = backupColor;
			return limit;
		}

		public static Vector3 GetPos(Transform transform)
		{
			return transform.position;
		}
	}
}