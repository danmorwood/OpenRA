using OpenRA.Mods.Common.Activities;
using OpenRA.Primitives;
using OpenRA.Traits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenRA.Mods.Common.Traits
{

	[Desc("Eject a ground soldier or a paratrooper while in the air.")]
	public class HydraInfo : ConditionalTraitInfo
	{
		[ActorReference]
		[Desc("Name of the unit to eject. This actor type needs to have the Parachutable trait defined.")]
		public readonly string PilotActor = "E1";

		[Desc("Probability that the aircraft's pilot gets ejected once the aircraft is destroyed.")]
		public readonly int SuccessRate = 50;

		[Desc("Sound to play when ejecting the pilot from the aircraft.")]
		public readonly string ChuteSound = null;

		[Desc("Can a destroyed aircraft eject its pilot while it has not yet fallen to ground level?")]
		public readonly bool EjectInAir = false;

		[Desc("Can a destroyed aircraft eject its pilot when it falls to ground level?")]
		public readonly bool EjectOnGround = true;

		[Desc("Risks stuck units when they don't have the Paratrooper trait.")]
		public readonly bool AllowUnsuitableCell = false;

		public override object Create(ActorInitializer init) { return new Hydra(init.Self, this); }
	}

	public class Hydra : ConditionalTrait<HydraInfo>, INotifyKilled
	{
		public Hydra(Actor self, HydraInfo info)
			: base(info) { }

		void INotifyKilled.Killed(Actor self, AttackInfo e)
		{
			if (IsTraitDisabled || self.Owner.WinState == WinState.Lost || !self.World.Map.Contains(self.Location))
				return;

			var cp = self.CenterPosition;
			var inAir = !self.IsAtGroundLevel();
			if ((inAir && !Info.EjectInAir) || (!inAir && !Info.EjectOnGround))
				return;

			var hydra1 = self.World.CreateActor(false, Info.PilotActor.ToLowerInvariant(),
				new TypeDictionary { new OwnerInit(self.Owner), new LocationInit(self.Location) });

			var hydra2 = self.World.CreateActor(false, Info.PilotActor.ToLowerInvariant(),
				new TypeDictionary { new OwnerInit(self.Owner), new LocationInit(self.Location) });

			if (Info.AllowUnsuitableCell || IsSuitableCell(self, hydra1))
			{
				self.World.AddFrameEndTask(w => w.Add(hydra1));
				var pilotMobile = hydra1.TraitOrDefault<Mobile>();
				if (pilotMobile != null)
					pilotMobile.Nudge(hydra1, hydra1, true);
			}
			else
				hydra1.Dispose();

			if (Info.AllowUnsuitableCell || IsSuitableCell(self, hydra2))
			{
				self.World.AddFrameEndTask(w => w.Add(hydra2));
				var pilotMobile = hydra1.TraitOrDefault<Mobile>();
				if (pilotMobile != null)
					pilotMobile.Nudge(hydra2, hydra2, true);
			}
			else
				hydra2.Dispose();
		}

		static bool IsSuitableCell(Actor self, Actor actorToDrop)
		{
			return actorToDrop.Trait<IPositionable>().CanEnterCell(self.Location, self, true);
		}
	}

}