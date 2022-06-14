using UnityEngine;
using System.Collections;
using System.Text;
using MoreMountains.Tools;
using UnityEngine.UI;

namespace MoreMountains.TopDownEngine
{
    [AddComponentMenu("TopDown Engine/GUI/AmmoDisplay")]
    public class AmmoDisplay : MMProgressBar 
	{
		[Tooltip("the ID of the AmmoDisplay ")]
		public int AmmoDisplayID = 0;
		[Tooltip("the Text object used to display the current ammo numbers")]
		public Text TextDisplay;

		protected int _totalAmmoLastTime, _maxAmmoLastTime, _ammoInMagazineLastTime, _magazineSizeLastTime;
		protected StringBuilder _stringBuilder;
		public override void Initialization()
		{
			base.Initialization();
			_stringBuilder = new StringBuilder();
		}
		public virtual void UpdateTextDisplay(string newText)
		{
			if (TextDisplay != null)
			{
				TextDisplay.text = newText;
			}
		}
		public virtual void UpdateAmmoDisplays(bool magazineBased, int totalAmmo, int maxAmmo, int ammoInMagazine, int magazineSize, bool displayTotal)
		{
			if ((_totalAmmoLastTime == totalAmmo)
			    && (_maxAmmoLastTime == maxAmmo)
			    && (_ammoInMagazineLastTime == ammoInMagazine)
			    && (_magazineSizeLastTime == magazineSize))
			{
				return;
			}

			_stringBuilder.Clear();
			
			if (magazineBased)
			{
				this.UpdateBar(ammoInMagazine,0,magazineSize);	
				if (displayTotal)
				{
					_stringBuilder.Append(ammoInMagazine.ToString());
					_stringBuilder.Append("/");
					_stringBuilder.Append(magazineSize.ToString());
					_stringBuilder.Append(" - ");
					_stringBuilder.Append((totalAmmo - ammoInMagazine).ToString());
					this.UpdateTextDisplay (_stringBuilder.ToString());					
				}
				else
				{
					_stringBuilder.Append(ammoInMagazine.ToString());
					_stringBuilder.Append("/");
					_stringBuilder.Append(magazineSize.ToString());
					this.UpdateTextDisplay (_stringBuilder.ToString());
				}
			}
			else
			{
				_stringBuilder.Append(totalAmmo.ToString());
				_stringBuilder.Append("/");
				_stringBuilder.Append(maxAmmo.ToString());
				this.UpdateBar(totalAmmo,0,maxAmmo);	
				this.UpdateTextDisplay (_stringBuilder.ToString());
			}

			_totalAmmoLastTime = totalAmmo;
			_maxAmmoLastTime = maxAmmo;
			_ammoInMagazineLastTime = ammoInMagazine;
			_magazineSizeLastTime = magazineSize;
		}
	}
}
