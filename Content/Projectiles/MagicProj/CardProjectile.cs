using System;
using ExpansionKele.Content.Customs;
using ExpansionKele.Content.Items.Weapons.Magic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExpansionKele.Content.Projectiles.MagicProj
{
    /// <summary>
    /// 卡牌弹幕 - 扇形发射，可视化手牌
    /// </summary>
    public class CardProjectile : ModProjectile
    {
        public override string LocalizationCategory => "Projectiles.MagicProj";
        
 private const int HAND_TYPE_INDEX = 5;
        private const int MULTIPLIER_INDEX = 6;
        
        private static Asset<Texture2D> _cachedTexture;
        
        private CardData[] _hand;
        private HandType _handType;
        private float _multiplier;
        private float _displayTimer;
        private bool _dataInitialized;
        
        private const float DISPLAY_TIME = 15f;
        private const float ACCELERATE_TIME = 30f;
        
        private const int TOTAL_CARDS = 52;
        private static int _cardWidth;
        private static int _cardHeight;
        
        private Vector2[] _cardPositions;
        private float[] _cardRotations;
        private bool[] _cardActive;
        private int _phase;
        private Vector2 _centerPosition;
        private Vector2 _playerBasePosition;
        private const float CARD_SPREAD_RADIUS = 100f;
        private const float FAN_ANGLE_0 = 45f;
        private const float FAN_ANGLE_1 = 22.5f;
        private const float FAN_ANGLE_2 = 0f;
        private const float FAN_ANGLE_3 = -22.5f;
        private const float FAN_ANGLE_4 = -45f;
        private static readonly float[] FAN_ANGLES = new float[] { FAN_ANGLE_0, FAN_ANGLE_1, FAN_ANGLE_2, FAN_ANGLE_3, FAN_ANGLE_4 };
        private static readonly int[] CARD_ORDER = new int[] { 0, 1, 2, 3, 4 };

        public override void Load()
        {
            _cachedTexture = ModContent.Request<Texture2D>(Texture);
        }

        public override void Unload()
        {
            _cachedTexture = null;
        }
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.netUpdate = true;
        }

        public override void OnSpawn(Terraria.DataStructures.IEntitySource source)
        {
            _hand = new CardData[5];
            _cardPositions = new Vector2[5];
            _cardRotations = new float[5];
            _cardActive = new bool[5];
            
            if (_cachedTexture != null && _cachedTexture.IsLoaded && _cardWidth == 0)
            {
                Texture2D texture = _cachedTexture.Value;
                _cardWidth = texture.Width;
                _cardHeight = texture.Height / TOTAL_CARDS;
            }
            
            if (Main.myPlayer == Projectile.owner)
            {
                bool hasValidData = Projectile.ai[0] != 0 || Projectile.ai[1] != 0;
                
                if (hasValidData)
                {
                    int packed0 = (int)Projectile.ai[0];
                    _hand[0] = CardDeck.DecodeCard(packed0 & 0xFF);
                    _hand[1] = CardDeck.DecodeCard((packed0 >> 8) & 0xFF);
                    _hand[2] = CardDeck.DecodeCard((packed0 >> 16) & 0xFF);
                    
                    int packed1 = (int)Projectile.ai[1];
                    _hand[3] = CardDeck.DecodeCard(packed1 & 0xFF);
                    _hand[4] = CardDeck.DecodeCard((packed1 >> 8) & 0xFF);
                    _handType = (HandType)((packed1 >> 16) & 0x0F);
                    
                    _multiplier = CardHandEvaluator.GetMultiplier(_handType);
                    _dataInitialized = true;
                }
                else
                {
                    // 正常情况下不应该走到这里，因为 CardMaster.Shoot 中已经设置了 ai 数据
                    // 如果走到了这里，说明数据丢失，使用默认抽牌（不应用运气值）
                    CardDeck.GenerateHand(_hand, Main.player[Projectile.owner].luck, 0f);
                    _handType = CardHandEvaluator.Evaluate(_hand, out _multiplier);
                    _dataInitialized = true;
                }
            }
            
            _displayTimer = 0f;
            _phase = 0;
            
            Player player = Main.player[Projectile.owner];
            _playerBasePosition = player.Center + new Vector2(0, -30);
            _centerPosition = _playerBasePosition;
            
            for (int i = 0; i < 5; i++)
            {
                _cardActive[i] = true;
                _cardPositions[i] = _playerBasePosition;
            }
        }
// ... existing code ...

        // ... existing code ...
        public override void AI()
        {
            _displayTimer++;
            
            Player player = Main.player[Projectile.owner];
            Vector2 currentPlayerHeadPos = player.Center + new Vector2(0, -30);
            Vector2 playerMovementDelta = currentPlayerHeadPos - _playerBasePosition;
            
            if (Main.myPlayer != Projectile.owner)
            {
                UpdateVisuals();
                int centerCard = CARD_ORDER[2];
                if (_dataInitialized && _cardActive != null && _cardActive[centerCard])
                {
                    Projectile.Center = _cardPositions[centerCard];
                }
                return;
            }
            
            if (_phase == 0)
            {
                _playerBasePosition = currentPlayerHeadPos;
                _centerPosition = _playerBasePosition;
                
                for (int i = 0; i < 5; i++)
                {
                    int cardIndex = CARD_ORDER[i];
                    float angleRad = MathHelper.ToRadians(FAN_ANGLES[i]);
                    
                    _cardPositions[cardIndex] = _centerPosition + new Vector2(
                        MathF.Sin(angleRad) * CARD_SPREAD_RADIUS,
                        -MathF.Cos(angleRad) * CARD_SPREAD_RADIUS
                    );
                    
                    _cardRotations[cardIndex] = angleRad;
                }
                
                _phase = 1;
                int centerCard = CARD_ORDER[2];
                Projectile.Center = _cardPositions[centerCard];
            }
            else if (_phase == 1)
            {
                if (_displayTimer <= 60)
                {
                    _playerBasePosition = currentPlayerHeadPos;
                    
                    for (int i = 0; i < 5; i++)
                    {
                        if (_cardActive[i])
                        {
                            int cardIndex = CARD_ORDER[i];
                            float angleRad = MathHelper.ToRadians(FAN_ANGLES[cardIndex]);
                            Vector2 targetPos = _playerBasePosition + new Vector2(
                                MathF.Sin(angleRad) * CARD_SPREAD_RADIUS,
                                -MathF.Cos(angleRad) * CARD_SPREAD_RADIUS
                            );
                            
                            _cardPositions[i] = Vector2.Lerp(_cardPositions[i], targetPos, 0.15f);
                            _cardRotations[i] = angleRad;
                            
                            float distanceToTarget = Vector2.Distance(_cardPositions[i], targetPos);
                            if (distanceToTarget < 1f)
                            {
                                _cardPositions[i] = targetPos;
                            }
                        }
                    }
                    
                    bool allReachedMaxDistance = true;
                    for (int i = 0; i < 5; i++)
                    {
                        if (_cardActive[i])
                        {
                            int cardIndex = CARD_ORDER[i];
                            float angleRad = MathHelper.ToRadians(FAN_ANGLES[cardIndex]);
                            Vector2 idealPos = _playerBasePosition + new Vector2(
                                MathF.Sin(angleRad) * CARD_SPREAD_RADIUS,
                                -MathF.Cos(angleRad) * CARD_SPREAD_RADIUS
                            );
                            
                            if (Vector2.Distance(_cardPositions[i], idealPos) > 2f)
                            {
                                allReachedMaxDistance = false;
                                break;
                            }
                        }
                    }
                    
                    if (allReachedMaxDistance)
                    {
                        _phase = 2;
                        _displayTimer = 0;
                    }
                }
                else
                {
                    _phase = 2;
                    _displayTimer = 0;
                }
                
                int centerCardPhase1 = CARD_ORDER[2];
                Projectile.Center = _cardPositions[centerCardPhase1];
            }
            else if (_phase == 2)
            {
                _playerBasePosition = currentPlayerHeadPos;
                _centerPosition = _playerBasePosition;
                
                if (_displayTimer <= 30)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (_cardActive[i])
                        {
                            int cardIndex = CARD_ORDER[i];
                            float angleRad = MathHelper.ToRadians(FAN_ANGLES[cardIndex]);
                            Vector2 idealPos = _playerBasePosition + new Vector2(
                                MathF.Sin(angleRad) * CARD_SPREAD_RADIUS,
                                -MathF.Cos(angleRad) * CARD_SPREAD_RADIUS
                            );
                            
                            _cardPositions[i] = idealPos;
                            _cardRotations[i] = angleRad;
                        }
                    }
                }
                else
                {
                    _phase = 3;
                    _displayTimer = 0;
                    _playerBasePosition = currentPlayerHeadPos;
                    _centerPosition = _playerBasePosition;
                    for (int i = 0; i < 5; i++)
                    {
                        if (_cardActive[i])
                        {
                            int cardIndex = CARD_ORDER[i];
                            float angleRad = MathHelper.ToRadians(FAN_ANGLES[cardIndex]);
                            Vector2 idealPos = _playerBasePosition + new Vector2(
                                MathF.Sin(angleRad) * CARD_SPREAD_RADIUS,
                                -MathF.Cos(angleRad) * CARD_SPREAD_RADIUS
                            );
                            _cardPositions[i] = idealPos;
                            _cardRotations[i] = angleRad;
                        }
                    }
                }
                
                int centerCardPhase2 = CARD_ORDER[2];
                Projectile.Center = _cardPositions[centerCardPhase2];
            }
            else if (_phase == 3)
            {
                int centerCard = CARD_ORDER[2];
                float mergeSpeed = 0.15f;
                bool allMerged = true;
                
                for (int i = 0; i < 5; i++)
                {
                    if (i != centerCard && _cardActive[i])
                    {
                        _cardPositions[i] = Vector2.Lerp(_cardPositions[i], _cardPositions[centerCard], mergeSpeed);
                        
                        if (Vector2.Distance(_cardPositions[i], _cardPositions[centerCard]) > 1f)
                        {
                            allMerged = false;
                        }
                        else
                        {
                            _cardActive[i] = false;
                        }
                    }
                }
                
                if (allMerged)
                {
                    _phase = 4;
                    _displayTimer = 0;
                    Projectile.Center = _cardPositions[centerCard];
                    Projectile.netUpdate = true;
                }
                
                Projectile.Center = _cardPositions[centerCard];
            }
           else if (_phase == 4)
            {
                int centerCard = CARD_ORDER[2];
                
                ProjectileHelper.FindAndMoveTowardsTarget(Projectile, 15f, 800f, 30f, bossPriority: true);
                
                for (int i = 0; i < 5; i++)
                {
                    if (_cardActive[i])
                    {
                        _cardPositions[i] += Projectile.velocity;
                    }
                }
                
                Projectile.Center = _cardPositions[centerCard];
            }
            
            UpdateVisuals();
            
            if (_phase < 4)
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.tileCollide = false;
                int centerCard = CARD_ORDER[2];
                Projectile.Center = _cardPositions[centerCard];
            }
        }
// ... existing code ...
        
        private void UpdateVisuals()
        {
            Color rarityColor = CardDeck.GetRarityColor(_handType);
            float lightIntensity = CardDeck.GetLightIntensityByRarity(_handType);
            
            Lighting.AddLight(Projectile.Center, rarityColor.ToVector3() * lightIntensity);
            
            if (_handType >= HandType.Straight)
            {
                if (Main.rand.NextBool(3))
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                        DustID.RainbowTorch, 0f, 0f, 100, rarityColor, 0.8f);
                    dust.noGravity = true;
                    dust.velocity *= 0.3f;
                }
            }
        }
        
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!_dataInitialized || _hand == null)
                return;
            
            int rankSum = CardHandEvaluator.CalculateRankSum(_hand);
            float damageMultiplier = _multiplier * rankSum / CardMaster.BASE_DAMAGE;
            modifiers.FinalDamage *= damageMultiplier;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.myPlayer == Projectile.owner && _dataInitialized && _hand != null)
            {
                string handName = CardHandEvaluator.GetHandTypeName(_handType);
                Color textColor = CardDeck.GetRarityColor(_handType);
                
                if (_handType == HandType.RoyalFlush)
                {
                    CombatText.NewText(Projectile.getRect(), textColor, $"ROYAL FLUSH!", dramatic: true);
                }
                else if (_handType >= HandType.StraightFlush)
                {
                    CombatText.NewText(Projectile.getRect(), textColor, $"{handName} x{_multiplier}", dramatic: true);
                }
                else
                {
                    CombatText.NewText(Projectile.getRect(), textColor, handName);
                }
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.Plantera_Green, 0f, 0f, 100, Color.White, 1f);
                dust.noGravity = true;
                dust.velocity *= 2f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (_cachedTexture == null || !_cachedTexture.IsLoaded)
            {
                return false;
            }
            
            if (!_dataInitialized || _hand == null || _hand.Length == 0)
            {
                return false;
            }
            
            if (_cardWidth == 0 || _cardHeight == 0)
            {
                Texture2D texture = _cachedTexture.Value;
                _cardWidth = texture.Width;
                _cardHeight = texture.Height / TOTAL_CARDS;
                
                if (_cardWidth == 0 || _cardHeight == 0)
                {
                    return false;
                }
            }
            
            Texture2D textureValue = _cachedTexture.Value;
            Color rarityGlowColor = CardDeck.GetRarityColor(_handType);
            float glowScale = CardDeck.GetGlowScaleByRarity(_handType);
            
            if (_phase >= 4)
            {
                int centerCard = CARD_ORDER[2];
                CardData card = _hand[centerCard];
                int cardIndex = (card.Suit * 13) + (card.Rank - CardDeck.MIN_RANK);
                
                if (cardIndex < 0 || cardIndex >= TOTAL_CARDS)
                {
                    return false;
                }
                
                Rectangle cardRect = new Rectangle(
                    0,
                    cardIndex * _cardHeight,
                    _cardWidth,
                    _cardHeight
                );
                
                Vector2 drawOrigin = new Vector2(_cardWidth * 0.5f, _cardHeight * 0.5f);
                Vector2 drawPosition = Projectile.Center - Main.screenPosition;
                float targetRotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                
                Main.EntitySpriteDraw(textureValue, 
                    drawPosition, 
                    cardRect, 
                    Color.White, 
                    targetRotation, 
                    drawOrigin, 
                    Projectile.scale, 
                    SpriteEffects.None, 
                    0);
                
                if (_handType >= HandType.Straight)
                {
                    Main.EntitySpriteDraw(textureValue, 
                        drawPosition, 
                        cardRect, 
                        rarityGlowColor * 0.6f, 
                        targetRotation, 
                        drawOrigin, 
                        Projectile.scale * glowScale, 
                        SpriteEffects.None, 
                        0);
                }
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    if (!_cardActive[i])
                        continue;
                    
                    CardData card = _hand[i];
                    int cardIndex = (card.Suit * 13) + (card.Rank - CardDeck.MIN_RANK);
                    
                    if (cardIndex < 0 || cardIndex >= TOTAL_CARDS)
                    {
                        continue;
                    }
                    
                    Rectangle cardRect = new Rectangle(
                        0,
                        cardIndex * _cardHeight,
                        _cardWidth,
                        _cardHeight
                    );
                    
                    Vector2 drawOrigin = new Vector2(_cardWidth * 0.5f, _cardHeight * 0.5f);
                    Vector2 drawPosition = _cardPositions[i] - Main.screenPosition;
                    
                    Main.EntitySpriteDraw(textureValue, 
                        drawPosition, 
                        cardRect, 
                        Color.White, 
                        _cardRotations[i], 
                        drawOrigin, 
                        Projectile.scale, 
                        SpriteEffects.None, 
                        0);
                    
                    if (_handType >= HandType.Straight)
                    {
                        Main.EntitySpriteDraw(textureValue, 
                            drawPosition, 
                            cardRect, 
                            rarityGlowColor * 0.6f, 
                            _cardRotations[i], 
                            drawOrigin, 
                            Projectile.scale * glowScale, 
                            SpriteEffects.None, 
                            0);
                    }
                }
            }
            
            return false;
        }

    }
}