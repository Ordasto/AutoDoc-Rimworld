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
    class AutoDocBuilding : Building_CryptosleepCasket
    {
        private CompPowerTrader powerComp;
        private CompBreakdownable breakdownable;
        public bool AutoDocActive => (powerComp?.PowerOn ?? true) && !(breakdownable?.BrokenDown ?? false);

        public CompAutoDoc autoDoc;

        public const int tickRate = 60;

        public Pawn PawnContained => innerContainer.FirstOrDefault() as Pawn;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
            breakdownable = GetComp<CompBreakdownable>();
            autoDoc = GetComp<CompAutoDoc>();
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            if (PawnContained != null) yield break;
            else if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly, false, false, TraverseMode.ByPawn))
            {
                var failer = new FloatMenuOption("CannotUseNopath".Translate(), null);
                yield return failer;
            }
            else if (PawnContained == null) // checks if a pawn is already inside
            {
                JobDef jobDef = JobDefOf.EnterCryptosleepCasket;
                void MakeJob()
                {
                    Job job = JobMaker.MakeJob(jobDef, this);
                    myPawn.jobs.TryTakeOrderedJob(job);
                }
                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Enter Auto Doc", MakeJob), myPawn, this);
            }
            yield break;
        }

<<<<<<< Updated upstream

        // Draw Rect around autodoc to search for medical materials
        // probably a better solution to this but meh
        //public void DrawRect(Map map)
        //{
        //    IntVec3 loc = this.Position; 
        //    CellRect testingRect = new CellRect
        //    {
        //        minX = loc.x-1,
        //        minZ = loc.z-2,
        //        Width = 3,
        //        Height = 4
                
        //    };
        //    Log.Message(loc.x.ToString());
        //    Log.Message(testingRect.CenterCell.ToString());
        //    foreach(var i in testingRect)
        //    {
        //        if (i.GetFirstItem(map) != null)
        //        {
        //            //Log.Message(i.GetFirstItem(map).ToString());

        //            List<Thing> thingList = i.GetThingList(map);
        //            foreach(Thing j in thingList)
        //            {
        //                Log.Message(j.stackCount.ToString());

        //            }
        //        }   
        //    }
        //}
=======
>>>>>>> Stashed changes

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            yield return new Command_Action() // Todo: Add to check if surgry is in progress and if somebody is inside
            {
                defaultLabel = "Exit Auto Doc",
                action = EjectContents 
            };
        }
    }
}
