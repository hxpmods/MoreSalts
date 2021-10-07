using BepInEx;
using UnityEngine;
using BasicMod;
using System.Collections.Generic;
using Utils.Extensions;
using System.Linq;
using HarmonyLib;
using static AlchemyMachineProduct;
using static IngredientManager;
using QuestSystem;

namespace MoreSalts
{
	[BepInPlugin(pluginGuid, pluginName, pluginVersion)]
	[BepInDependency("potioncraft.basicmod")]
	public class MoreSalts : BaseUnityPlugin
    {
        public const string pluginGuid = "potioncraft.hxp.moresalts";
        public const string pluginName = "A Pinch of Salt";
        public const string pluginVersion = "0.0.0.8";

		//Recipes are configured in two stages. This list is used to help keep reference of recipes you've made for this mod 
		public static List<ModLegendaryRecipe> modRecipes = new List<ModLegendaryRecipe>();
        public void Awake()
        {
            DoPatching();
           // Debug.Log("Loaded " + pluginName + ", version: " + pluginVersion);
        }

        private void DoPatching()
        {
            var harmony = new HarmonyLib.Harmony("hxp.moresalts");
            harmony.PatchAll();
        }

		// Salt behaviour helpers begin

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

		//Salt behaviour helpers end

		public static void RegisterSalts()
		{

			///Ignis salt start
			ModSalt isalt = SaltFactory.CreateSalt("Ignis Salt");
			isalt.SetGraphicsPaths("Ignis Salt Box Bottom.png", "Ignis Salt Box Top.png", "Ignis Salt Tooltip Icon.png", "Ignis Salt Recipe Mark.png");
			isalt.SetDescription("This fine powder is hot to the touch but will never burn.");

			var ignisBehaviour = new CardinalSaltBehaviour(Vector2.left, 1.0f);
			isalt.SetBehaviour(ignisBehaviour);

			isalt.particleBgColor = Color.red;
			isalt.pileBgColor = Color.red;

			

			//Ignis salt end

			
			//Levitas Salt Start
			ModSalt lsalt = SaltFactory.CreateSalt("Levitas Salt");
			//lsalt.SetGraphicsPaths();
			lsalt.SetDescription("Lightweight, and somewhat volatile. A stiff breeze on some errant crystals has blown many an Alchemists house down.");
			lsalt.SetGraphicsPaths("Levitas Salt Box Bottom.png", "Levitas Salt Box Top.png", _recipeIconPath:  "Levitas Salt Recipe Mark.png");
			var levitasBehaviour = new CardinalSaltBehaviour(Vector2.up, 1.0f);
			lsalt.SetBehaviour(levitasBehaviour);

			lsalt.particleBgColor = Color.grey;
			lsalt.pileBgColor = Color.yellow;

			//Levitas Salt end

			//Water salt start

			ModSalt fsalt = SaltFactory.CreateSalt("Fluix Salt");
			//lsalt.SetGraphicsPaths();
			fsalt.SetDescription("Not sure yet");
			fsalt.SetGraphicsPaths("Fluix Salt Box Bottom.png", "Fluix Salt Box Top.png", _recipeIconPath: "Fluix Salt Recipe Mark.png");
			var fluixBehaviour = new CardinalSaltBehaviour(Vector2.right, 1.0f);
			fsalt.SetBehaviour(fluixBehaviour);

			fsalt.particleBgColor = Color.magenta;
			fsalt.pileBgColor = Color.gray;
			//Water salt end

			//Gaia salt start
			ModSalt gsalt = SaltFactory.CreateSalt("Gaia Salt");
			//lsalt.SetGraphicsPaths();
			gsalt.SetDescription("Not sure yet");
			gsalt.SetGraphicsPaths("Gaia Salt Box Bottom.png", "Gaia Salt Box Top.png", _recipeIconPath: "Gaia Salt Recipe Mark.png");
			var gaiaBehaviour = new CardinalSaltBehaviour(Vector2.down, 1.0f);
			gsalt.SetBehaviour(gaiaBehaviour);

			gsalt.particleBgColor = Color.magenta;
			gsalt.pileBgColor = Color.gray;


			//Gaia salt end

			//Earth salt end

			//Essencia salt start
			ModSalt esalt = SaltFactory.CreateSalt("Essencia Salt");
			esalt.SetGraphicsPaths("E Salt Box Bottom.png", "E Salt Box Top.png", "E Salt Tooltip Icon.png");
			esalt.SetDescription("Exotic aromas and secret scents waft from this salt.Just don't inhale too vigorously.");

			var eBehaviour = new ExperienceSaltBehaviour();
			esalt.SetBehaviour(eBehaviour);

			esalt.particleBgColor = Color.green;
			esalt.pileBgColor = Color.green;
			//Essencia salt end


		}

		public static void RegisterRecipes()
		{
			//Ignis recipe begin
			ModLegendaryRecipe irep = LegendaryRecipeFactory.CreateRecipe("Ignis Recipe");
			LegendarySaltPile iresult = LegendaryRecipeFactory.CreateAlchemyMachineProduct("Ignis Salt Result");

			LocalDict.AddKeyToDictionary("ignis_recipe_title", "Ignis Salt");
			LocalDict.AddKeyToDictionary("ignis_recipe_description", "For those who seek the secrets beyond the flames.");
			LocalDict.AddKeyToDictionary("Ignis Salt", "Ignis Salt");
			irep.SetGraphicsPaths("Ignis Salt Recipe Icon.png", "Ignis Salt Recipe Bookmark.png");
			irep.SetUnlockedByDefault(true).SetResultItem(iresult).SetKnownAtStart(true);
			irep.AddRecipesToUnlock(irep);

			modRecipes.Add(irep);
			//Ignis recipe end

			//Levitas recipe begin
			ModLegendaryRecipe lrep = LegendaryRecipeFactory.CreateRecipe("Levitas Recipe");
			LegendarySaltPile lresult = LegendaryRecipeFactory.CreateAlchemyMachineProduct("Levitas Salt Result");

			LocalDict.AddKeyToDictionary("levitas_recipe_title", "Levitas Salt");
			LocalDict.AddKeyToDictionary("levitas_recipe_description", "Write Me");
			LocalDict.AddKeyToDictionary("Levitas Salt", "Levitas Salt");
			lrep.SetGraphicsPaths("Levitas Salt Recipe Icon.png", "Levitas Salt Recipe Bookmark.png");
			lrep.SetUnlockedByDefault(true).SetResultItem(lresult).SetKnownAtStart(true);
			lrep.AddRecipesToUnlock(lrep);

			modRecipes.Add(lrep);
			//Levitas recipe end

			//Fluix recipe begin
			ModLegendaryRecipe frep = LegendaryRecipeFactory.CreateRecipe("Fluix Recipe");
			LegendarySaltPile fresult = LegendaryRecipeFactory.CreateAlchemyMachineProduct("Fluix Salt Result");

			LocalDict.AddKeyToDictionary("fluix_recipe_title", "Fluix Salt");
			LocalDict.AddKeyToDictionary("Fluix Salt", "Fluix Salt");
			LocalDict.AddKeyToDictionary("fluix_recipe_description", "Write Me");
			frep.SetGraphicsPaths("Fluix Salt Recipe Icon.png", "Fluix Salt Recipe Bookmark.png");
			frep.SetUnlockedByDefault(true).SetResultItem(fresult).SetKnownAtStart(true);
			frep.AddRecipesToUnlock(frep);

			modRecipes.Add(frep);
			//Fluix recipe end

			//Gaia recipe begin
			ModLegendaryRecipe grep = LegendaryRecipeFactory.CreateRecipe("Gaia Recipe");
			LegendarySaltPile gresult = LegendaryRecipeFactory.CreateAlchemyMachineProduct("Gaia Salt Result");

			LocalDict.AddKeyToDictionary("gaia_recipe_title", "Gaia Salt");
			LocalDict.AddKeyToDictionary("Fluix Salt", "Fluix Salt");
			LocalDict.AddKeyToDictionary("gaia_recipe_description", "Write Me");
			grep.SetGraphicsPaths("Gaia Salt Recipe Icon.png", "Gaia Salt Recipe Bookmark.png");
			grep.SetUnlockedByDefault(true).SetResultItem(gresult).SetKnownAtStart(true);
			grep.AddRecipesToUnlock(grep);

			modRecipes.Add(grep);
			//Gaia recipe end

			//Gaia recipe begin
			ModLegendaryRecipe erep = LegendaryRecipeFactory.CreateRecipe("Essencia Recipe");
			LegendarySaltPile eresult = LegendaryRecipeFactory.CreateAlchemyMachineProduct("Essencia Salt Result");

			LocalDict.AddKeyToDictionary("essencia_recipe_title", "Essencia Salt");
			LocalDict.AddKeyToDictionary("Essencia Salt", "Essencia Salt");
			LocalDict.AddKeyToDictionary("essencia_recipe_description", "Write Me");
			erep.SetGraphicsPaths("E Salt Recipe Icon.png", "E Salt Recipe Bookmark.png");
			erep.SetUnlockedByDefault(true).SetResultItem(eresult).SetKnownAtStart(true);
			erep.AddRecipesToUnlock(erep);

			modRecipes.Add(erep);
			//Gaia recipe end

		}

		public static void ConfigureRecipes()
		{

			//Get potion effects
			var fireEffect = PotionEffect.GetByName("Fire");
			var eEffect = PotionEffect.GetByName("Explosion");
			var lEffect = PotionEffect.GetByName("Light");
			var liEffect = PotionEffect.GetByName("Libido");
			var gEffect = PotionEffect.GetByName("Growth");
			var cEffect = PotionEffect.GetByName("Crop");
			var chEffect = PotionEffect.GetByName("Charm");
			var hEffect = PotionEffect.GetByName("Healing");
			var mEffect = PotionEffect.GetByName("Mana");
			var haEffect = PotionEffect.GetByName("Hallucinations");
			var flEffect = PotionEffect.GetByName("Fly");
			var bEffect = PotionEffect.GetByName("Bounce");
			var tEffect = PotionEffect.GetByName("Lightning");
			var nEffect = PotionEffect.GetByName("Necromancy");
			var sEffect = PotionEffect.GetByName("SharpVision");
			var fEffect = PotionEffect.GetByName("Frost");
			var iEffect = PotionEffect.GetByName("Invisibility");
			var pEffect = PotionEffect.GetByName("Poison");
			var aEffect = PotionEffect.GetByName("Acid");
			var sdEffect = PotionEffect.GetByName("SlowDown");
			var spEffect = PotionEffect.GetByName("Sleep");
			var beEffect = PotionEffect.GetByName("Berserker");
			var ssEffect = PotionEffect.GetByName("StoneSkin");


			//Desire potions
			var fireBase = LegendaryRecipeFactory.CreateDesiredPotion(new[] { fireEffect, fireEffect, fireEffect, eEffect, eEffect }); //Fire 3, Explosion 2
			var growthCatalyst = LegendaryRecipeFactory.CreateDesiredPotion(new[] { gEffect, gEffect, cEffect, cEffect, mEffect }); // Growth 2, Crop 2, Mana 1
			var lightBase = LegendaryRecipeFactory.CreateDesiredPotion(new[] { lEffect, lEffect, lEffect, fireEffect, fireEffect });
			var lifeCatalyst = LegendaryRecipeFactory.CreateDesiredPotion(new[] { hEffect, hEffect, hEffect, liEffect, chEffect });
			var deathCatalyst = LegendaryRecipeFactory.CreateDesiredPotion(new[] { nEffect, nEffect, nEffect, liEffect, chEffect });
			var realityDistorter = LegendaryRecipeFactory.CreateDesiredPotion(new[] { mEffect, mEffect, haEffect, haEffect });
			var flightBase = LegendaryRecipeFactory.CreateDesiredPotion(new[] { flEffect, flEffect, flEffect, bEffect, bEffect });
			var spectacleBase = LegendaryRecipeFactory.CreateDesiredPotion(new[] { tEffect,tEffect, tEffect, lEffect, haEffect });
			var truesightBase = LegendaryRecipeFactory.CreateDesiredPotion(new[] { sEffect, sEffect, sEffect, haEffect, mEffect });
			var crystalClear = LegendaryRecipeFactory.CreateDesiredPotion(new[] { iEffect, iEffect, iEffect, fEffect,fEffect });
			var doubleDead = LegendaryRecipeFactory.CreateDesiredPotion(new[] { pEffect, pEffect, pEffect, aEffect, aEffect });
			var industrialGlue = LegendaryRecipeFactory.CreateDesiredPotion(new[] { sdEffect, sdEffect, spEffect, spEffect, spEffect });
			var trueRage = LegendaryRecipeFactory.CreateDesiredPotion(new[] { beEffect, beEffect, beEffect, hEffect, hEffect });
			var decayPotion = LegendaryRecipeFactory.CreateDesiredPotion(new[] { nEffect, nEffect, ssEffect, pEffect, aEffect });
			var wisdomPotion = LegendaryRecipeFactory.CreateDesiredPotion(new[] { sEffect, iEffect, sdEffect, spEffect, mEffect });
			var goodTrip= LegendaryRecipeFactory.CreateDesiredPotion(new[] { haEffect, haEffect,haEffect, liEffect, liEffect });
			var mageFire = LegendaryRecipeFactory.CreateDesiredPotion(new[] { fireEffect, lEffect, eEffect, mEffect, mEffect });
			var toughBinder = LegendaryRecipeFactory.CreateDesiredPotion(new[] { hEffect, mEffect, lEffect, ssEffect, aEffect });

			foreach (ModLegendaryRecipe recipe in modRecipes)
			{


				//Switch statement here ain't pretty but it works
				//Looking for suggestions on how to improve his pattern
				switch (recipe.name)
				{
					case "Ignis Recipe":

						//There are a few more slots named similarly. 
						recipe.doubleVessel = fireBase;
						recipe.triangularVessel = growthCatalyst;
						recipe.rhombusVessel = lightBase;
						recipe.rightDripper = lifeCatalyst;
						recipe.rightRetort = realityDistorter;
						recipe.rightFurnace = DesiredItem.GetByName("Firebell"); //Method for solid items/ingredients

						var iresult = recipe.resultItem as LegendarySaltPile;
						//The game will handle conversion to desire salt automatically
						iresult.convertToSaltOnPickup = Salt.GetByName("Ignis Salt");


						//Colors still throw warnings (It looks like you need a lot of them. getColorsFromMachine works but doesn't satisfy the warning
						var icolList = new ColorsList();
						icolList.colors.Add(Color.red);
						iresult.colors.Add(icolList);
						iresult.furnaceColor = Color.red;
						iresult.getColorsFromMachine = true; //Salt pile will sparkle with color according to the recipe components

						break;

					case "Levitas Recipe":

						//There are a few more slots named similarly. 
						recipe.doubleVessel = flightBase;
						recipe.triangularVessel = growthCatalyst;
						recipe.rhombusVessel = spectacleBase;
						recipe.rightDripper = lifeCatalyst;
						recipe.rightRetort = realityDistorter;
						recipe.rightFurnace = DesiredItem.GetByName("Windbloom"); //Method for solid items/ingredients

						var lresult = recipe.resultItem as LegendarySaltPile;
						//The game will handle conversion to desire salt automatically
						lresult.convertToSaltOnPickup = Salt.GetByName("Levitas Salt");


						//Colors still throw warnings (It looks like you need a lot of them. getColorsFromMachine works but doesn't satisfy the warning
						var lcolList = new ColorsList();
						lcolList.colors.Add(Color.white);
						lresult.colors.Add(lcolList);
						lresult.furnaceColor = Color.white;
						lresult.getColorsFromMachine = true; //Salt pile will sparkle with color according to the recipe components

						break;

					case "Fluix Recipe":

						//There are a few more slots named similarly. 
						recipe.doubleVessel = truesightBase;
						recipe.triangularVessel = growthCatalyst;
						recipe.rhombusVessel = crystalClear;
						recipe.rightDripper = deathCatalyst;
						recipe.rightRetort = realityDistorter;
						recipe.rightFurnace = DesiredItem.GetByName("Waterbloom"); //Method for solid items/ingredients

						var fresult = recipe.resultItem as LegendarySaltPile;
						//The game will handle conversion to desire salt automatically
						fresult.convertToSaltOnPickup = Salt.GetByName("Fluix Salt");


						//Colors still throw warnings (It looks like you need a lot of them. getColorsFromMachine works but doesn't satisfy the warning
						var fcolList = new ColorsList();
						fcolList.colors.Add(Color.white);
						fresult.colors.Add(fcolList);
						fresult.furnaceColor = Color.white;
						fresult.getColorsFromMachine = true; //Salt pile will sparkle with color according to the recipe components

						break;

					case "Gaia Recipe":

						//There are a few more slots named similarly. 
						recipe.doubleVessel = industrialGlue;
						recipe.triangularVessel = growthCatalyst;
						recipe.rhombusVessel = doubleDead;
						recipe.rightDripper = deathCatalyst;
						recipe.rightRetort = realityDistorter;
						recipe.rightFurnace = DesiredItem.GetByName("Leaf"); //Method for solid items/ingredients

						var gresult = recipe.resultItem as LegendarySaltPile;
						//The game will handle conversion to desire salt automatically
						gresult.convertToSaltOnPickup = Salt.GetByName("Gaia Salt");


						//Colors still throw warnings (It looks like you need a lot of them. getColorsFromMachine works but doesn't satisfy the warning
						var gcolList = new ColorsList();
						gcolList.colors.Add(Color.white);
						gresult.colors.Add(gcolList);
						gresult.furnaceColor = Color.white;
						gresult.getColorsFromMachine = true; //Salt pile will sparkle with color according to the recipe components

						break;


					case "Essencia Recipe":

						recipe.doubleVessel = toughBinder;
						recipe.rightRetort = goodTrip;
						recipe.triangularVessel = mageFire;
						recipe.rightDripper = deathCatalyst;
						recipe.rhombusVessel = lifeCatalyst;
						recipe.rightFurnace = DesiredItem.GetByName("Albedo");


						recipe.leftRetort = trueRage;
						recipe.leftFurnace = DesiredItem.GetByName("Goldthorn");
						recipe.spiralVessel = realityDistorter;
						recipe.leftDripper = decayPotion;
						recipe.floorVessel = wisdomPotion;


						var eresult = recipe.resultItem as LegendarySaltPile;
						//The game will handle conversion to desire salt automatically
						eresult.convertToSaltOnPickup = Salt.GetByName("Essencia Salt");



						//Colors still throw warnings (It looks like you need a lot of them. getColorsFromMachine works but doesn't satisfy the warning
						var ecolList = new ColorsList();
						ecolList.colors.Add(Color.green);
						eresult.colors.Add(ecolList);
						eresult.furnaceColor = Color.green;
						eresult.getColorsFromMachine = true; //Salt pile will sparkle with color according to the recipe components
						break;

				}



			}


			//LogAlchemyMachineProducts();
			//Debug.Log(potion);
		}


		public static void LogAlchemyMachineProducts()
		{
			foreach (AlchemyMachineProduct allProduct in allProducts)
			{
				Debug.Log(allProduct.name);
			}
		}

	}

	//Harmony patch begin


	[HarmonyPatch(typeof(InventoryItem))]
	[HarmonyPatch("GetByName")]
	class AddAlchemyProductsToGetByName 
	{
		static bool Prefix(string name, ref InventoryItem __result)
        {
			var byName = AlchemyMachineProduct.GetByName(name, returnFirst: false, warning: false);
			if (byName != null)
			{
				__result = byName;
				return false;
			}
			return true;
		}
	}

	//Add salt start

	[HarmonyPatch(typeof(TradableUpgrade))]
	[HarmonyPatch("Initialize")]
	class SaltPatch
	{
		static void Postfix()
		{
			//Adds our salts to the game
			MoreSalts.RegisterSalts();

		}
	}

	//Add salt end
	//Add recipes start
	[HarmonyPatch(typeof(LegendaryRecipeSubManager))]
	[HarmonyPatch("OnManagerAwake")]
	class AddRecipesToGamePatch
	{
		static void Postfix()
		{
			//Recipes are registered first. This is where you define their name, result and other information.
			Debug.Log("Registering Legendary Recipes to LegendaryRecipeFactory");
			MoreSalts.RegisterRecipes();
		}
	}

	[HarmonyPatch(typeof(PotionManager))]
	[HarmonyPatch("InitiateScriptableObjects")]
	class ConfigureRecipesOnEffectLoadPatch
	{
		static void Postfix()
		{
            //BasicMod.BasicMod.LogEffects();
			//Recipes are then configured after the Potion Manager, to ensure we have full access to all potion effects.
			//This is where you define the recipes themselves, and further configure the result.
			MoreSalts.ConfigureRecipes();
		}
	}

	//Add recipes end

	[HarmonyPatch(typeof(RecipeMapManager))]
	[HarmonyPatch("Update")]
	class PotionMapPatch
	{
		static void Prefix(RecipeMapManager __instance)
		{
			//Makes the cardinal directions work
			MoreSalts.MoveIndicatorTowardsCardinals(__instance);
			//Makes Essencia potion work
			MoreSalts.MoveIndicatorTowardsExperience(__instance);
		}
	}

	[HarmonyPatch(typeof(ExperienceBonusMapItem))]
	[HarmonyPatch("Awake")]
	class ExpItemListPatch
	{
		static void Prefix(ExperienceBonusMapItem __instance)
		{
			MoreSalts.allExpItems.Add(__instance);
		}
	}


	//Harmony patch end
}
