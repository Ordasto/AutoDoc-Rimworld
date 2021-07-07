using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace AutoDoc
{
    class CompAutoDoc : ThingComp
    {
        public CompPropertiesAutoDocBuilding Properties => props as CompPropertiesAutoDocBuilding;
        private AutoDocBuilding AutoDoc => parent as AutoDocBuilding;
        public Pawn PawnContained => AutoDoc.PawnContained;
        private CellRect MaterialSearch;
        private Map ParentMap { set; get; }
        private float timer = -1;
        public Bill surgeryBill;
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

        public override void CompTick() // might move to rare if it impacts performence heavily
        {
            base.CompTick();
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
            if (timer > 0)
            {
                timer -= 1;
            }
            if (timer<=0)
            {
                AutoDoc.SetSurgeryInProgress(false);
                timer = -1;
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


        // This is a pretty horrible way to do this but im too dumb to figure out a better way
        // Just looking at it makes me want to vomit
        private void DoSurgery() // <--- most disgusting thing i've ever written, there has to be a better way 
        {
            List<Bill> surgeryNeeded = PawnContained.health.surgeryBills.Bills;
            for (int i = 0; i < surgeryNeeded.Count; i++)
            {
                surgeryBill = surgeryNeeded[i];
                List<Thing> MaterialsAround = CheckMat();
                if (MaterialsAround == null) return;
                HashSet<Thing> MaterialsRequired = new HashSet<Thing>();
                HashSet<ThingDef> MaterialsRequiredDef = new HashSet<ThingDef>();
                foreach (Thing j in MaterialsAround)
                {
                    if (surgeryBill.recipe.IsIngredient(j.def) && !MaterialsRequiredDef.Contains(j.def))
                    {
                        MaterialsRequired.Add(j);
                        MaterialsRequiredDef.Add(j.def);
                    }
                }
                if (MaterialsRequired.Count >= surgeryBill.recipe.ingredients.Count)
                {
                    Bill temp = surgeryBill;
                    try { surgeryBill.Notify_IterationCompleted(null, null); }
                    catch { };
                    if (!PawnContained.health.surgeryBills.Bills.Contains(temp)) 
                    {
                        AutoDoc.SetSurgeryInProgress(true);
                        timer = surgeryBill.recipe.workAmount;
                        foreach (Thing j in MaterialsRequired)
                        {
                            if (j.stackCount > 1) j.stackCount--;
                            else j.Destroy();
                        }
                    }      
                }
            }
        }

        // [Info]
        // Draw 3x4 Rect around autodoc to search for medical materials
        // Theres probably a better solution to this
        // parent.rotation south = 0, 1 = west, 2 = north, 3 = east
        private void DrawRect()
        {

            int width = 3;
            int height = 4;
            int xm = 1;
            int zm = 2;
            IntVec3 loc = parent.Position;

            // [SEMI-IMPORTANT]
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

        // [INFO]
        // Makes a list of the materials in the MaterialSearch CellRect then returns it
        // Probably should move this to another file and add params for cellrects
        // Also add more functionality
        private List<Thing> CheckMat()
        {
            List<Thing> output = new List<Thing>();
            foreach (var i in MaterialSearch)
            {
                if (i.GetFirstItem(ParentMap) != null)
                {
                    output.AddRange(i.GetThingList(ParentMap));
                }
            }
            return output;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            //Scribe_Values.Look(ref surgeryBill, "Current Task:");
        }

        public override string CompInspectStringExtra()
        {
            if (surgeryBill == null) return "No Task";
            StringBuilder output = new StringBuilder();
            output.Append($"Current Bill: {surgeryBill.Label}\n");
            if (timer > 0) output.Append($"Time Left: { (int)timer/10 }");
            output.Append("\nRequires: ");
            foreach (IngredientCount i in surgeryBill.recipe.ingredients) output.Append( i.ToString());
            return $"{output}";

        }
    }
}
