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
		
		[Desc("Can a killed soldier be hyrdolated?")]
		public readonly bool KilledOnGround = true;

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

			var hydra1 = self.World.CreateActor(false, self.Info.Name,
				new TypeDictionary { new OwnerInit(self.Owner), new LocationInit(self.Location) });

			var hydra2 = self.World.CreateActor(false, self.Info.Name,
				new TypeDictionary { new OwnerInit(self.Owner), new LocationInit(self.Location) });

			if (Info.AllowUnsuitableCell || IsSuitableCell(self, hydra1))
			{
				self.World.AddFrameEndTask(w => w.Add(hydra1));
				var hydraMobile = hydra1.TraitOrDefault<Mobile>();
				if (hydraMobile != null)
					hydraMobile.Nudge(hydra1, hydra1, true);
			}
			else
				hydra1.Dispose();

			if (Info.AllowUnsuitableCell || IsSuitableCell(self, hydra2))
			{
				self.World.AddFrameEndTask(w => w.Add(hydra2));
				var hydraMobile = hydra1.TraitOrDefault<Mobile>();
				if (hydraMobile != null)
					hydraMobile.Nudge(hydra2, hydra2, true);
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