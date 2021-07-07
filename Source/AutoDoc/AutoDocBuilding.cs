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
        public bool SurgeryInProgress { get; set; }

        public CompAutoDoc autoDoc;

        public const int tickRate = 60;

        public Pawn PawnContained => innerContainer.FirstOrDefault() as Pawn;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            SurgeryInProgress = false;
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
        }
        

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            yield return ExitAutoDoc();
        }

        // [INFO]
        // Command_Action are the buttons you see at the bottom of the screen when a building is selected
        // Like Deconstruct and uninstall
        private Gizmo ExitAutoDoc() 
        {
            Command_Action exit = new Command_Action()
            {
                defaultLabel = "Exit Auto Doc",
                action = EjectContents,
                defaultDesc = "Ejects the pawn inside.",
                disabled = false
            };
            if (SurgeryInProgress) exit.Disable("Busy");
            else if (PawnContained == null) exit.Disable("Empty");
            return exit;
        }
        public void SetSurgeryInProgress(bool setting)
        {
            SurgeryInProgress = setting;
        }
        public override void EjectContents()
        {
            base.EjectContents();
            autoDoc.surgeryBill = null;
        }
    }
}
