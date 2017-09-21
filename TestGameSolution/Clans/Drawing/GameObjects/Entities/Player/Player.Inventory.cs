using Clans.Data;
using Juixel.Drawing.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Clans.Drawing.GameObjects
{
    public partial class Player
    {
        #region Equipment Properties

        private ItemData _EquippedWeapon;
        public ItemData EquippedWeapon
        {
            get => _EquippedWeapon;
            set
            {
                _EquippedWeapon = value;
                SetAnimationOffset(WeaponAnimations, EquippedWeapon.FileIndex, false);
            }
        }

        private ItemData _EquippedHelm;
        public ItemData EquippedHelm
        {
            get => _EquippedHelm;
            set
            {
                _EquippedHelm = value;
                SetAnimationOffset(HelmAnimations, _EquippedHelm.FileIndex, false);
            }
        }

        private ItemData _EquippedArmor;
        public ItemData EquippedArmor
        {
            get => _EquippedArmor;
            set
            {
                _EquippedArmor = value;
                SetAnimationOffset(Animations, _EquippedArmor.FileIndex, false);
            }
        }

        public ItemData EquippedRing;

        #endregion
    }
}
