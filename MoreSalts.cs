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
using BasicMod.Factories;

namespace MoreSalts
{
	[BepInPlugin(pluginGuid, pluginName, pluginVersion)]
	[BepInDependency("potioncraft.basicmod")]
	public class MoreSalts : BaseUnityPlugin
    {
        public const string pluginGuid = "potioncraft.hxp.moresalts";
        public const string pluginName = "A Pinch of Salt";
        public const string pluginVersion = "0.0.1.0";

        public void Awake()
        {
            DoPatching();

		    SaltFactory.onPreRegisterSaltEvent += (_,e) => {
			    RegisterSalts();
            };

		}

        private void DoPatching()
        {
            var harmony = new HarmonyLib.Harmony("hxp.moresalts");
            harmony.PatchAll();
        }

		// Salt behaviour helpers begin



		//Salt behaviour helpers end

		public static void RegisterSalts()
		{




			///Ignis salt start
			ModSalt dawnsalt = SaltFactory.CreateSalt("Dawn Salt");
			//msalt.SetGraphicsPaths("Ignis Salt Box Bottom.png", "Ignis Salt Box Top.png", "Ignis Salt Tooltip Icon.png", "Ignis Salt Recipe Mark.png");
			//msalt.SetDescription("This fine powder is hot to the touch but will never burn.");

			var dawnBehaviour = new RotateSaltBehaviour(10.0f);
			dawnsalt.SetBehaviour(dawnBehaviour);

			dawnsalt.particleBgColor = Color.grey;
			dawnsalt.pileBgColor = Color.grey;

			///Ignis salt start
			ModSalt crystalsalt = SaltFactory.CreateSalt("Crystal Salt");
			//msalt.SetGraphicsPaths("Ignis Salt Box Bottom.png", "Ignis Salt Box Top.png", "Ignis Salt Tooltip Icon.png", "Ignis Salt Recipe Mark.png");
			//msalt.SetDescription("This fine powder is hot to the touch but will never burn.");

			var crystalBehaviour = new TeleportSaltBehaviour(0.0001f);
			crystalsalt.SetBehaviour(crystalBehaviour);

			crystalsalt.particleBgColor = Color.grey;
			crystalsalt.pileBgColor = Color.grey;



			///Ignis salt start
			ModSalt msalt = SaltFactory.CreateSalt("Maximus Salt");
			//msalt.SetGraphicsPaths("Ignis Salt Box Bottom.png", "Ignis Salt Box Top.png", "Ignis Salt Tooltip Icon.png", "Ignis Salt Recipe Mark.png");
			//msalt.SetDescription("This fine powder is hot to the touch but will never burn.");

			var maximusBehaviour = new ScaleSaltBehaviour(0.05f);
			msalt.SetBehaviour(maximusBehaviour);

			msalt.particleBgColor = Color.grey;
			msalt.pileBgColor = Color.grey;

			///Ignis salt start
			ModSalt misalt = SaltFactory.CreateSalt("Minimus Salt");
			//msalt.SetGraphicsPaths("Ignis Salt Box Bottom.png", "Ignis Salt Box Top.png", "Ignis Salt Tooltip Icon.png", "Ignis Salt Recipe Mark.png");
			//msalt.SetDescription("This fine powder is hot to the touch but will never burn.");

			var minimusBehaviour = new ScaleSaltBehaviour(-0.05f);
			misalt.SetBehaviour(minimusBehaviour);

			msalt.particleBgColor = Color.grey;
			msalt.pileBgColor = Color.grey;

			/*
			//Essencia salt start
			ModSalt esalt = SaltFactory.CreateSalt("Essencia Salt");
			esalt.SetGraphicsPaths("E Salt Box Bottom.png", "E Salt Box Top.png", "E Salt Tooltip Icon.png");
			esalt.SetDescription("Exotic aromas and secret scents waft from this salt. Just don't inhale too vigorously.");

			var eBehaviour = new ExperienceSaltBehaviour();
			esalt.SetBehaviour(eBehaviour);

			esalt.particleBgColor = Color.green;
			esalt.pileBgColor = Color.green;
			//Essencia salt end
			*/

		}

		public static void LogAlchemyMachineProducts()
		{
			foreach (AlchemyMachineProduct allProduct in allProducts)
			{
				Debug.Log(allProduct.name);
			}
		}

	}

}
