using BasicMod.Utility;
using HarmonyLib;
using ObjectBased.RecipeMap.Path;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.Extensions;

namespace MoreSalts
{
	public class SaltHelper
	{
		public static Vector2 moveToCardinalsBySalt;
		public static float moveToNearestExperienceBySalt;
		public static float scalePathBySalt;
		public static float rotatePathBySalt;
		public static float changePathToTeleport;

		public static void MoveIndicatorTowardsCardinals(RecipeMapManager RMM)
		{

			var smoothing = 0.1f;

			if (moveToCardinalsBySalt.magnitude < 0.0001f)
			{
				return;
			}
			//nudes the potion along in the desired direction
			Vector2 currentPos = RMM.recipeMapObject.indicatorContainer.localPosition;
			Vector3 newPos = currentPos + (moveToCardinalsBySalt * Time.deltaTime);
			RMM.indicator.MoveIndicatorAndPathToPosition(newPos);

			//Smoothing coefficient keeps a little about of movement for use in the next frame, smoothing things somewhat (Probably needs to incorporate Delta to be frame rate independent)
			moveToCardinalsBySalt *= smoothing;

		}

		public static List<ExperienceBonusMapItem> allExpItems = new List<ExperienceBonusMapItem>();
		public static void MoveIndicatorTowardsExperience(RecipeMapManager RMM)
		{
			if (moveToNearestExperienceBySalt < 0.0001f)
			{
				return;
			}
			Vector2 indicatorLocalPosition = RMM.recipeMapObject.indicatorContainer.localPosition;
			ExperienceBonusMapItem expEffectMapItem = (from exp in allExpItems
													   where exp.isActiveAndEnabled
													   orderby ((Vector2)exp.thisTransform.localPosition - indicatorLocalPosition).sqrMagnitude
													   select exp).FirstOrDefault();
			if (expEffectMapItem == null)
			{
				moveToNearestExperienceBySalt = 0f;
				return;
			}
			Vector2 vector = expEffectMapItem.thisTransform.localPosition;
			float num = expEffectMapItem.thisTransform.eulerAngles.z.RecalculateEulerAngle(FloatExtension.AngleType.ZeroTo2Pi);
			float num2 = (num - RMM.indicatorRotation.Value).RecalculateEulerAngle(FloatExtension.AngleType.ZeroTo2Pi);
			float num3 = moveToNearestExperienceBySalt / RMM.indicatorSettings.philosophersSaltMovementAnimationTime * Time.deltaTime;
			float movedDistance = 0f;
			float movedAngle = 0f;

			//RMM.recipeMapObject.indicatorContainer.GetType().GetTypeInfo().GetDeclaredMethod("MoveIndicatorTowardsObject").Invoke(RMM.recipeMapObject.indicatorContainer, new object[] { vector, num, num3, RMM.indicatorRotationSettings.philosophersSaltIndicatorRotationMaxSpeed, RMM.indicatorRotationSettings.philosophersSaltIndicatorRotationAnimationTime,  movedDistance, movedAngle });

			MoveIndicatorTowardsObject(RMM, vector, num, num3, RMM.indicatorRotationSettings.philosophersSaltIndicatorRotationMaxSpeed, RMM.indicatorRotationSettings.philosophersSaltIndicatorRotationAnimationTime, ref movedDistance, ref movedAngle);

			moveToNearestExperienceBySalt = Mathf.Max(moveToNearestExperienceBySalt - num3, 0f);
			indicatorLocalPosition = RMM.recipeMapObject.indicatorContainer.localPosition;
			num2 = (num - RMM.indicatorRotation.Value).RecalculateEulerAngle(FloatExtension.AngleType.ZeroTo2Pi);
			if ((indicatorLocalPosition - vector).sqrMagnitude.Is(0f) && num2.Is(0f))
			{
				moveToNearestExperienceBySalt = 0f;
			}
		}

		public static void MoveIndicatorTowardsObject(RecipeMapManager RMM, Vector2 objectLocalPosition, float objectEulerAngle, float positionSpeed, float rotationMaxSpeed, float rotationAnimationTime, ref float movedDistance, ref float movedAngle)
		{
			objectEulerAngle = objectEulerAngle.RecalculateEulerAngle(FloatExtension.AngleType.ZeroTo2Pi);
			Vector2 vector = RMM.recipeMapObject.indicatorContainer.localPosition;
			Vector2 vector2 = Vector2.MoveTowards(vector, objectLocalPosition, positionSpeed);
			float magnitude = (objectLocalPosition - vector).magnitude;
			float a = ((magnitude < Mathf.Epsilon) ? 1f : ((vector2 - vector).magnitude / magnitude));
			float num = RMM.indicatorRotation.Value;
			float num2 = (objectEulerAngle - num).RecalculateEulerAngle(FloatExtension.AngleType.ZeroTo2Pi);
			bool flag = num2 > 180f;
			if (flag && num < objectEulerAngle)
			{
				num += 360f;
			}
			else if (!flag && objectEulerAngle < num)
			{
				objectEulerAngle += 360f;
			}
			float num3 = objectEulerAngle - num;
			float num4 = num + num3 * Mathf.Min(a, rotationMaxSpeed * Time.deltaTime / Mathf.Abs(num3));
			RMM.indicator.MoveIndicatorAndPathToPosition(vector2);
			if (!num2.Is(0f))
			{
				RMM.indicatorRotation.RotateTo(num4, flag, rotationAnimationTime);
			}
			movedAngle = num.Distance(num4);
			movedDistance = (vector2 - vector).magnitude;
		}


		public static void TeleportPath(RecipeMapManager RMM)
		{
			var fixedPathHints = Managers.RecipeMap.path.fixedPathHints;

			if (fixedPathHints.Count == 0)
			{
				return;
			}

			// Effect starts from the last teleport hint.
			var teleportHint = (TeleportationFixedHint)fixedPathHints.FindLast(x => x is TeleportationFixedHint);

			// Effect targets the next non teleport hint after the last teleport hint.
			// If no teleport hint, affects the first path.
			NonTeleportationFixedHint nonTeleportHint;
			if (teleportHint == null)
			{
				nonTeleportHint = fixedPathHints[0] as NonTeleportationFixedHint;
			}
			else
			{
				var nextIndex = fixedPathHints.IndexOf(teleportHint) + 1;
				if (nextIndex >= fixedPathHints.Count)
				{
					return;
				}

				nonTeleportHint = fixedPathHints[nextIndex] as NonTeleportationFixedHint;
			}

			if (nonTeleportHint == null)
			{
				Debug.Log("ConvertToTeleport: Bailing because no non teleport hint");
				return;
			}

			if (teleportHint == null)
			{
				Debug.Log("ConvertToTeleport: Creating teleport hint");
				teleportHint = UnityEngine.Object.Instantiate(Managers.RecipeMap.pathSettings.teleportationFixedHint, Managers.RecipeMap.path.fixedPathHintsContainer).GetComponent<TeleportationFixedHint>();
				teleportHint.evenlySpacedPointsDots = new EvenlySpacedPoints(new List<Vector2>());
				teleportHint.evenlySpacedPointsFixedGraphics = new EvenlySpacedPoints(new List<Vector2>());
				fixedPathHints.Insert(0, teleportHint);
				teleportHint.SetPathEndParameters(nonTeleportHint.GetPathStartParameters());
				nonTeleportHint.ShowPathStart(false, 0.0f, false);
				teleportHint.ShowPathStart(true, 0.0f, false);
				nonTeleportHint.UpdatePathEndSpriteGlobalNumber();
				teleportHint.UpdatePathEndSpriteGlobalNumber();
				nonTeleportHint.MakePathVisible();
			}

			var distanceToAbsorb = changePathToTeleport*0.001f;
			changePathToTeleport = changePathToTeleport / (RMM.indicatorSettings.philosophersSaltMovementAnimationTime) * Time.deltaTime;


			while (nonTeleportHint != null && distanceToAbsorb > float.Epsilon)
			{
				var teleGraphicsPoints = teleportHint.evenlySpacedPointsFixedGraphics.points;
				var telePhysicsPoints = teleportHint.evenlySpacedPointsFixedPhysics.points;

				var nonTeleGraphicsPoints = nonTeleportHint.evenlySpacedPointsFixedGraphics.points;
				var nonTelePhysicsPoints = nonTeleportHint.evenlySpacedPointsFixedPhysics.points;

				if (teleGraphicsPoints.Count > 0)
				{
					var previousPoint = teleGraphicsPoints.Last();
					distanceToAbsorb -= (nonTeleGraphicsPoints[0] - previousPoint).magnitude;
				}

				teleGraphicsPoints.Add(nonTeleGraphicsPoints[0]);
				telePhysicsPoints.Add(nonTelePhysicsPoints[0]);

				nonTeleGraphicsPoints.RemoveAt(0);
				nonTelePhysicsPoints.RemoveAt(0);

				// This is not ideal, as curves will confuse this.
				// We could check to see if we hit a distance since last point from distanceToAbsorb, but that will skip out on points if
				// the distance is decently low.
				var dotsPoints = teleportHint.evenlySpacedPointsDots.points;
				var lastGraphicsVec = teleGraphicsPoints.Last();
				if (dotsPoints.Count == 0)
				{
					dotsPoints.Add(lastGraphicsVec);

				}
				else if ((lastGraphicsVec - dotsPoints.Last()).magnitude > 0.4f)
				{
					dotsPoints.Add(lastGraphicsVec);
				}

				if (nonTeleGraphicsPoints.Count == 0 || nonTelePhysicsPoints.Count == 0)
				{
					Debug.Log("ConvertToTeleport: Removing exhausted non tele hint");

					fixedPathHints.Remove(nonTeleportHint);
					nonTeleportHint.DestroyPath();

					// TODO: Probably need to ShowPathStart/ShowPathEnd and set parameters on the next hint?

					nonTeleportHint = (NonTeleportationFixedHint)fixedPathHints.Find(x => x is NonTeleportationFixedHint);
				}
			}

			// Regenerate segment lengths
			// This is really silly, but the Length getter is what generates them.
			// We might be able to avoid this by selectively copying over SegmentsLength in the loop above
			resetSegmentLengths(teleportHint.evenlySpacedPointsFixedGraphics);
			resetSegmentLengths(teleportHint.evenlySpacedPointsFixedPhysics);
			if (nonTeleportHint)
			{
				resetSegmentLengths(nonTeleportHint.evenlySpacedPointsFixedGraphics);
				resetSegmentLengths(nonTeleportHint.evenlySpacedPointsFixedPhysics);
			}

			Managers.RecipeMap.path.fixedPathWasChanged = true;
			Managers.RecipeMap.path.ShowPath(Managers.RecipeMap.path.currentPathHint?.ingredient, Managers.RecipeMap.path.grindStatus, 0.0f, 0.0f);
		}


		static void resetSegmentLengths(EvenlySpacedPoints p)
		{
			Reflection.SetPrivateField<float>(p, "_length", -1);
			var unused = p.Length;
		}


		public static void ScalePath(RecipeMapManager RMM)
		{
			if (scalePathBySalt <= 0.0001f && scalePathBySalt >= -0.0001f)
			{
				return;
			}

			bool positive = scalePathBySalt > 0;

			foreach (var p in RMM.path.fixedPathHints)
			{
				Debug.Log("*" + p.name);
				float multiplier = (1 + (scalePathBySalt * Time.deltaTime));



				p.evenlySpacedPointsFixedGraphics.points = p.evenlySpacedPointsFixedGraphics.points.ConvertAll(v => new Vector2((float)(v.x * multiplier), (float)(v.y * multiplier)));
				p.evenlySpacedPointsFixedPhysics.points = p.evenlySpacedPointsFixedPhysics.points.ConvertAll(v => new Vector2((float)(v.x * multiplier), (float)(v.y * multiplier)));

				p.MakePathVisible();
			}


		}

		public static void RotatePath(RecipeMapManager RMM)
		{
			if (rotatePathBySalt <= 0.0001f && rotatePathBySalt >= -0.0001f)
			{
				return;
			}

			bool positive = rotatePathBySalt > 0;

			foreach (var p in RMM.path.fixedPathHints)
			{
				Debug.Log("*" + p.name);
				float multiplier = rotatePathBySalt * Time.deltaTime;


				p.evenlySpacedPointsFixedGraphics.points = p.evenlySpacedPointsFixedGraphics.points.ConvertAll(v => (Vector2)(Quaternion.Euler(0, 0, multiplier) * v));
				p.evenlySpacedPointsFixedPhysics.points = p.evenlySpacedPointsFixedPhysics.points.ConvertAll(v => (Vector2)(Quaternion.Euler(0, 0, multiplier) * v));


				p.MakePathVisible();
			}

			if (positive)
			{
				rotatePathBySalt = Mathf.Max(rotatePathBySalt - (rotatePathBySalt / Time.deltaTime * 5), 0);
			}
			else
			{
				rotatePathBySalt = Mathf.Min(rotatePathBySalt - (rotatePathBySalt / Time.deltaTime * 5), 0);
			}

		}
	}

	[HarmonyPatch(typeof(RecipeMapManager))]
	[HarmonyPatch("Update")]
	class PotionMapPatch
	{
		static void Prefix(RecipeMapManager __instance)
		{
			//Makes the cardinal directions work
			SaltHelper.MoveIndicatorTowardsCardinals(__instance);
			//Makes Essencia potion work
			SaltHelper.MoveIndicatorTowardsExperience(__instance);
			SaltHelper.TeleportPath(__instance);
			SaltHelper.RotatePath(__instance);
			SaltHelper.ScalePath(__instance);
		}
	}

	[HarmonyPatch(typeof(ExperienceBonusMapItem))]
	[HarmonyPatch("Awake")]
	class ExpItemListPatch
	{
		static void Prefix(ExperienceBonusMapItem __instance)
		{
			SaltHelper.allExpItems.Add(__instance);
		}
	}

}
