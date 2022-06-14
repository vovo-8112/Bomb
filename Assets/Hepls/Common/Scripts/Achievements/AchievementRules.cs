using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;

namespace MoreMountains.TopDownEngine
{
	public class AchievementRules : MMAchievementRules, 
									MMEventListener<MMGameEvent>, 
									MMEventListener<MMCharacterEvent>, 
									MMEventListener<TopDownEngineEvent>,
									MMEventListener<MMStateChangeEvent<CharacterStates.MovementStates>>,
									MMEventListener<MMStateChangeEvent<CharacterStates.CharacterConditions>>,
                                    MMEventListener<PickableItemEvent>,
                                    MMEventListener<CheckPointEvent>,
                                    MMEventListener<MMInventoryEvent>
    {
		public override void OnMMEvent(MMGameEvent gameEvent)
		{
			base.OnMMEvent (gameEvent);
		}
		public virtual void OnMMEvent(MMCharacterEvent characterEvent)
		{
			if (characterEvent.TargetCharacter.CharacterType == Character.CharacterTypes.Player)
			{
				switch (characterEvent.EventType)
				{
					case MMCharacterEventTypes.Jump:
						MMAchievementManager.AddProgress ("JumpAround", 1);
						break;
				}	
			}
		}
		public virtual void OnMMEvent(TopDownEngineEvent topDownEngineEvent)
		{
			switch (topDownEngineEvent.EventType)
			{
				case TopDownEngineEventTypes.PlayerDeath:
					MMAchievementManager.UnlockAchievement ("DeathIsOnlyTheBeginning");
					break;
			}
		}
		public virtual void OnMMEvent(PickableItemEvent pickableItemEvent)
		{
		}
		public virtual void OnMMEvent(MMStateChangeEvent<CharacterStates.MovementStates> movementEvent)
		{
        }
        public virtual void OnMMEvent(MMStateChangeEvent<CharacterStates.CharacterConditions> conditionEvent)
        {
        }
        public virtual void OnMMEvent(CheckPointEvent checkPointEvent)
        {
            if (checkPointEvent.Order > 0)
            {
                MMAchievementManager.UnlockAchievement("SteppingStone");
            }
        }
        public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
        {
            if (inventoryEvent.InventoryEventType == MMInventoryEventType.Pick)
            {
                if (inventoryEvent.EventItem.ItemID == "KoalaCoin")
                {
                    MMAchievementManager.AddProgress("MoneyMoneyMoney", 1);
                }
                if (inventoryEvent.EventItem.ItemID == "KoalaHealth")
                {
                    MMAchievementManager.UnlockAchievement("Medic");
                }
            }
        }
        protected override void OnEnable()
		{
			base.OnEnable ();
			this.MMEventStartListening<MMCharacterEvent>();
			this.MMEventStartListening<TopDownEngineEvent>();
			this.MMEventStartListening<MMStateChangeEvent<CharacterStates.MovementStates>>();
			this.MMEventStartListening<MMStateChangeEvent<CharacterStates.CharacterConditions>>();
            this.MMEventStartListening<PickableItemEvent>();
            this.MMEventStartListening<CheckPointEvent>();
            this.MMEventStartListening<MMInventoryEvent>();
        }
		protected override void OnDisable()
		{
			base.OnDisable ();
			this.MMEventStopListening<MMCharacterEvent>();
			this.MMEventStopListening<TopDownEngineEvent>();
			this.MMEventStopListening<MMStateChangeEvent<CharacterStates.MovementStates>>();
			this.MMEventStopListening<MMStateChangeEvent<CharacterStates.CharacterConditions>>();
            this.MMEventStopListening<PickableItemEvent>();
            this.MMEventStopListening<CheckPointEvent>();
            this.MMEventStopListening<MMInventoryEvent>();
        }
	}
}