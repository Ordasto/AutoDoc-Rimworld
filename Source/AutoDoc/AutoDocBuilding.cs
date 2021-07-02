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
            if (!myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
            {
                FloatMenuOption failer = new FloatMenuOption("CannotUseNoPath".Translate(), null);
                yield return failer;
            }
            else
            {
                JobDef jobDef = JobDefOf.EnterCryptosleepCasket;
                void MakeJob()
                {
                    Job job = JobMaker.MakeJob(jobDef, this);
                    myPawn.jobs.TryTakeOrderedJob(job);
                }
                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Enter Auto Doc", MakeJob), myPawn, this);
            }

        }
    }
}
