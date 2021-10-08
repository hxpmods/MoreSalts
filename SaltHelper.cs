using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Utils.Extensions;

namespace MoreSalts
{
    public class SaltHelper
    {
		public static Vector2 moveToCardinalsBySalt;
		public static float moveToNearestExperienceBySalt;

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

			MoveIndicatorTowardsObject(RMM, vector, num, num3, RMM.indicatorRotationSettings.philosophersSaltIndicatorRotationMaxSpeed, RMM.indicatorRotationSettings.philosophersSaltIndicatorRotationAnimationTime,ref movedDistance,ref movedAngle);

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
