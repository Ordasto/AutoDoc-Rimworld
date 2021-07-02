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
        // Ingame time:
        // tick = 1/60 sec
        // tickRare = 4.16 sec
        // tickLong = 33.33 sec

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
                foreach (IngredientCount j in surgeryNeeded[i].recipe.ingredients)
                {
                    Log.Message(j.ToString());
                    
                }
                try
                {
                    surgeryNeeded[i].Notify_IterationCompleted(null, null); // Works i guess but throws errors 
                }
                catch { }
            }
        }
    }
}
