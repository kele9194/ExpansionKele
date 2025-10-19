using System;
using Terraria;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Items
{
    public abstract class MentalOmegaProjectile : ModProjectile
    {
        public enum OmegaLevel
        {
            Level0 = 0,
            Level1 = 1,
            Level2 = 2,
            Level3 = 3
        }

        // 必须填写的属性
        private OmegaLevel? _antiInfantry;
        private OmegaLevel? _antiArmor;
        private OmegaLevel? _antiBuilding;
        private OmegaLevel? _antiAirForce;

        public OmegaLevel AntiInfantry 
        { 
            get
            {
                if (!_antiInfantry.HasValue)
                    throw new InvalidOperationException("AntiInfantry 属性未设置。必须在 SetOmegaDefaults 方法中设置所有 Omega 属性。");
                return _antiInfantry.Value;
            }
            private set
            {
                _antiInfantry = value;
            }
        }
        
        public OmegaLevel AntiArmor 
        { 
            get
            {
                if (!_antiArmor.HasValue)
                    throw new InvalidOperationException("AntiArmor 属性未设置。必须在 SetOmegaDefaults 方法中设置所有 Omega 属性。");
                return _antiArmor.Value;
            }
            private set
            {
                _antiArmor = value;
            }
        }
        
        public OmegaLevel AntiBuilding 
        { 
            get
            {
                if (!_antiBuilding.HasValue)
                    throw new InvalidOperationException("AntiBuilding 属性未设置。必须在 SetOmegaDefaults 方法中设置所有 Omega 属性。");
                return _antiBuilding.Value;
            }
            private set
            {
                _antiBuilding = value;
            }
        }
        
        public OmegaLevel AntiAirForce 
        { 
            get
            {
                if (!_antiAirForce.HasValue)
                    throw new InvalidOperationException("AntiAirForce 属性未设置。必须在 SetOmegaDefaults 方法中设置所有 Omega 属性。");
                return _antiAirForce.Value;
            }
            private set
            {
                _antiAirForce = value;
            }
        }

        public bool isOnlyAntiBuilding => 
            AntiInfantry == OmegaLevel.Level0 && 
            AntiArmor == OmegaLevel.Level0 && 
            AntiBuilding != OmegaLevel.Level0;

        public bool isOnlyAntiAirForce => 
            AntiInfantry == OmegaLevel.Level0 && 
            AntiArmor == OmegaLevel.Level0 && 
            AntiBuilding == OmegaLevel.Level0 && 
            AntiAirForce != OmegaLevel.Level0;

        

        // 用于设置属性的方法
        protected void SetAntiInfantry(OmegaLevel level) => AntiInfantry = level;
        protected void SetAntiArmor(OmegaLevel level) => AntiArmor = level;
        protected void SetAntiBuilding(OmegaLevel level) => AntiBuilding = level;
        protected void SetAntiAirForce(OmegaLevel level) => AntiAirForce = level;

        // 原来的 protected override void SetOmegaDefaults()
        protected virtual void SetOmegaDefaults()
        {
            // 可由子类重写
        }
        public override void SetDefaults()
            {
                SetOmegaDefaults(); // 调用虚方法
                base.SetDefaults();
            }

        
    }
}