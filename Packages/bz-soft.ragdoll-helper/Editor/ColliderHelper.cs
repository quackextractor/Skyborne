﻿using System;
using UnityEditor;
using UnityEngine;

namespace Editor
{
	static class ColliderHelper
	{
		/// <summary>
		/// If you rotate collider, the collider rotates via an additional
		/// node that have the same name + this text.
		/// </summary>
		public const string ColliderRotatorNodeSufix = "_ColliderRotator";

		/// <summary>
		/// Get rotation of collider object
		/// </summary>
		public static Quaternion GetRotatorRotarion(Transform boneTransform)
		{
			var collider = GetCollider(boneTransform);
			return collider.transform.rotation;
		}

		/// <summary>
		/// Get position of collider center
		/// </summary>
		public static Vector3 GetRotatorPosition(Transform boneTransform)
		{
			var collider = GetCollider(boneTransform);
			var cCollider = collider as CapsuleCollider;
			var bCollider = collider as BoxCollider;
			var sCollider = collider as SphereCollider;
			var mCollider = collider as MeshCollider;

			Vector3 colliderCenter;
			if (cCollider != null) colliderCenter = cCollider.center;
			else if (bCollider != null) colliderCenter = bCollider.center;
			else if (sCollider != null) colliderCenter = sCollider.center;
			else if (mCollider != null) colliderCenter = mCollider.sharedMesh.bounds.center;
			else
				colliderCenter = Vector3.zero;

			var colliderTransform = collider.transform;
			return colliderTransform.TransformPoint(colliderCenter);
		}

		/// <summary>
		/// Rotate collider without rotating "transform" object.
		/// </summary>
		public static void RotateCollider(Transform transform, Quaternion rotate)
		{
			var prevPosition = GetColliderPosition(transform);

			Undo.RecordObject(transform, "Rotate collider");
			transform.rotation = rotate;

			SetColliderPosition(transform, prevPosition);
		}

		/// <summary>
		/// Get colliders' center in world space
		/// </summary>
		public static Vector3 GetColliderPosition(Transform transform)
		{
			var collider = GetCollider(transform);
			var cCollider = collider as CapsuleCollider;
			var bCollider = collider as BoxCollider;
			var sCollider = collider as SphereCollider;

			Vector3 center;
			if (cCollider != null) center = cCollider.center;
			else if (bCollider != null) center = bCollider.center;
			else if (sCollider != null) center = sCollider.center;
			else
				throw new InvalidOperationException("Unsupported Collider type: " + collider.GetType().FullName);

			return collider.transform.TransformPoint(center);
		}

		/// <summary>
		/// Set colliders' center in world space
		/// </summary>
		public static void SetColliderPosition(Transform transform, Vector3 position)
		{
			var collider = GetCollider(transform);
			Undo.RecordObject(collider, "Set collider position");

			var cCollider = collider as CapsuleCollider;
			var bCollider = collider as BoxCollider;
			var sCollider = collider as SphereCollider;

			var center = collider.transform.InverseTransformPoint(position);
			if (cCollider != null) cCollider.center = center;
			else if (bCollider != null) bCollider.center = center;
			else if (sCollider != null) sCollider.center = center;
			else
				throw new InvalidOperationException("Unsupported Collider type: " + collider.GetType().FullName);
		}

		/// <summary>
		/// Get object a collider attached to. 
		/// </summary>
		public static Collider GetCollider(Transform transform)
		{
			var collider = transform.GetComponent<Collider>();
			if (collider == null)
			{
				var rotatorName = transform.name + ColliderRotatorNodeSufix;
				var rotatorTransform = transform.Find(rotatorName);
				if (rotatorTransform != null)
					collider = rotatorTransform.GetComponent<Collider>();
			}

			if (collider == null)
				throw new ArgumentException("transform '" + transform.name + "' does not contain collider");

			return collider;
		}
		
		/// <summary>
		/// Gets object a collider attached to.
		/// Collider must have separate GameObject to allow a collider to rotate via it.
		/// So if that GameObject do not exists, creates it.
		/// </summary>
		public static Transform GetRotatorTransform(Transform boneTransform)
		{
			var colliderRotatorName = boneTransform.name + ColliderRotatorNodeSufix;

			// find rotator node
			var rotatorTransform = boneTransform.Find(colliderRotatorName);
			if (rotatorTransform != null)
				return rotatorTransform;

			// if rotator node was not found, create it
			var collider = boneTransform.GetComponent<Collider>();
			if (collider == null)
				throw new ArgumentException("Bone '" + boneTransform.name + "' does not have collider attached to it or ColliderRotatorNode");

			var colliderRotator = new GameObject(colliderRotatorName);
			Undo.RegisterCreatedObjectUndo(colliderRotator, "Create Rotator");
			rotatorTransform = colliderRotator.transform;

			ReattachCollider(boneTransform.gameObject, colliderRotator);
			Undo.SetTransformParent(rotatorTransform, boneTransform, "Set collider parrent");
			rotatorTransform.localPosition = Vector3.zero;
			rotatorTransform.localRotation = Quaternion.identity;
			rotatorTransform.localScale = Vector3.one;

			return colliderRotator.transform;
		}
		
		/// <summary>
		/// Duplicate collidr from "from" to "to" and delete it from "from"
		/// </summary>
		static void ReattachCollider(GameObject from, GameObject to)
		{
			var oldCollider = from.GetComponent<Collider>();
			var cCollider = oldCollider as CapsuleCollider;
			var bCollider = oldCollider as BoxCollider;
			Collider newCollider;
			if (cCollider != null)
			{
				var newCapsuleCollider = Undo.AddComponent<CapsuleCollider>(to);
				newCollider = newCapsuleCollider;
				newCapsuleCollider.direction = cCollider.direction;
				newCapsuleCollider.radius = cCollider.radius;
				newCapsuleCollider.height = cCollider.height;
				newCapsuleCollider.center = cCollider.center;
			}
			else if (bCollider != null)
			{
				var newBoxCollider = Undo.AddComponent<BoxCollider>(to);
				newCollider = newBoxCollider;
				newBoxCollider.size = bCollider.size;
				newBoxCollider.center = bCollider.center;
			}
			else
				throw new NotSupportedException("Collider type '" + oldCollider + "' does not supported to reattach.");

			newCollider.isTrigger = oldCollider.isTrigger;
			Undo.DestroyObjectImmediate(oldCollider);
		}
	}
}