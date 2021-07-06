using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace AutoDoc
{
    class CompAutoDoc : ThingComp
    {

        public CompPropertiesAutoDocBuilding Properties => props as CompPropertiesAutoDocBuilding;
        private AutoDocBuilding AutoDoc => parent as AutoDocBuilding;
        public Pawn PawnContained => AutoDoc.PawnContained;
        private CellRect MaterialSearch;

        private Map ParentMap { set; get; }
        // [INFO]
        // Ingame time:
        // tick = 1/60 sec
        // tickRare = 4.16 sec
        // tickLong = 33.33 sec
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            ParentMap = parent.Map;
            DrawRect();
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            if (!AutoDoc.AutoDocActive || PawnContained == null)
            {
                return;
            }
            if (PawnContained.health.HasHediffsNeedingTend())
            {
                TendHediffs();
            }
            if(PawnContained.health.surgeryBills.Bills.Count>0)
            {
                DoSurgery();
            }

        }
        
        private void TendHediffs()
        {
            List<Hediff> hediffSet = PawnContained.health.hediffSet.hediffs;
            for (int i = 0; i < hediffSet.Count; i++)
            {
                if (hediffSet[i].TendableNow())
                {
                    Log.Message(hediffSet[i].Label);
                    hediffSet[i].Tended_NewTemp(1,1);
                    break;
                }
            }
        }

        private void DoSurgery()
        {
            List<Bill> surgeryNeeded = PawnContained.health.surgeryBills.Bills;
            for(int i = 0; i<surgeryNeeded.Count; i++)
            {
                //List<IngredientCount> requiredIngredients = surgeryNeeded[i].recipe.ingredients;  // Makes list of required ingredients

                //foreach (IngredientCount j in surgeryNeeded[i].recipe.ingredients) { Log.Message(j.GetType().ToString()); } // Just Displays required recipe

                List<Thing> MaterialsAround = CheckMat();
                if (MaterialsAround == null) return;
                HashSet<Thing> MaterialsRequired = new HashSet<Thing>();
                IntVec3 loc = parent.Position;
                foreach (Thing j in MaterialsAround)
                {
                    Log.Message("");
                    Log.Message($"{surgeryNeeded[i].IsFixedOrAllowedIngredient(j)} : {j} "); // [IMPORTANT] Displays whether item is required in recipe, still need to remove the desired amount
                    Log.Message($"{surgeryNeeded[i].recipe.IsIngredient(j.def)} : {j.def} ");                      // Then remove from materials around/required
                    if (surgeryNeeded[i].recipe.IsIngredient(j.def))
                    {
                        MaterialsRequired.Add(j);
                    }
                    Log.Message("");
                }
                if (MaterialsRequired.Count == surgeryNeeded[i].recipe.ingredients.Count)
                {
                    Bill temp = surgeryNeeded[i];
                    surgeryNeeded[i].Notify_IterationCompleted(null, null);
                    //PawnContained.health.AddHediff(surgeryNeeded[i].recipe.addsHediff);
                    //surgeryNeeded[i].deleted = true;
                    //PawnContained.health.surgeryBills.Bills.Remove(surgeryNeeded[i]);
                    if (!PawnContained.health.surgeryBills.Bills.Contains(temp))
                    {
                        foreach (Thing j in MaterialsRequired) j.stackCount--;
                    }
                    
                    
                }
            }
        }

        // [Info]
        // Draw 3x4 Rect around autodoc to search for medical materials
        // probably a better solution to this but meh
        // parent.rotation south = 0, 1 = west, 2 = north, 3 = east
        private void DrawRect()
        {

            int width = 3;
            int height = 4;
            int xm = 1;
            int zm = 2;
            IntVec3 loc = parent.Position;

            //[SEMI-IMPORTANT]
            // Two Choices: 1:Figure out good way to rotate cell with Building
            //              2:Make AutDoc more square or bigger

            MaterialSearch = new CellRect
            {
                minX = loc.x - xm,
                minZ = loc.z - zm,
                Width = width,
                Height = height
            };
            MaterialSearch.DebugDraw();
        }

        private List<Thing> CheckMat() // I would prefer a better solution than this ( just removes requied items in the area )
        {
            List<Thing> output = new List<Thing>();
            foreach (var i in MaterialSearch) // Searches the Cell Rectangle MaterialSearch for required items
            {
                if (i.GetFirstItem(ParentMap) != null)
                {
                    output.AddRange(i.GetThingList(ParentMap));
                }
            }
            return output;
        }
    }
}
