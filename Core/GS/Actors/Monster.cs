﻿/*
 * Copyright (C) 2011 - 2018 NullD project
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

using System.Collections.Generic;
using System.Linq;
using NullD.Common.Helpers.Math;
using NullD.Core.GS.AI.Brains;
using NullD.Core.GS.Map;
using NullD.Core.GS.Objects;
using NullD.Core.GS.Players;
using NullD.Net.GS.Message;
using NullD.Net.GS.Message.Definitions.World;
using NullD.Net.GS.Message.Fields;
using NullD.Net.GS.Message.Definitions.Animation;
using NullD.Net.GS.Message.Definitions.Effect;
using NullD.Net.GS.Message.Definitions.Misc;
using NullD.Core.GS.Common.Types.TagMap;
using MonsterFF = NullD.Common.MPQ.FileFormats.Monster;
using GameBalance = NullD.Common.MPQ.FileFormats.GameBalance;
using NullD.Core.GS.Common.Types.SNO;
using NullD.Net.GS;

namespace NullD.Core.GS.Actors
{
    public class Monster : Living, IUpdateable
    {
        public override ActorType ActorType { get { return ActorType.Monster; } }

        public override int Quality
        {
            get
            {
                if (ActorSNO.Id == 85900 || // Мира жена кузнеца
                    ActorSNO.Id == 5350 || // Леорик
                    ActorSNO.Id == 176889 || // Мать её..)
                    ActorSNO.Id == 156801 || //Капитан Далтин
                    ActorSNO.Id == 156353 || //Советник Имон
                    ActorSNO.Id == 86624 || //Йондар
                    ActorSNO.Id == 131131 || //Пророк Урик
                    ActorSNO.Id == 139757 || //Пророк Эзек
                    ActorSNO.Id == 139713 || //Брат в пещере 1
                    ActorSNO.Id == 139715 || //Брат в пещере 2
                    ActorSNO.Id == 139756 || //Брат в пещере 3
                    ActorSNO.Id == 178619 || //Урцель Мордрег
                    ActorSNO.Id == 51341 || //Королева Арана
                    ActorSNO.Id == 98879 || //Главный тюремщик
                    ActorSNO.Id == 3526 || //Мясник
                    ActorSNO.Id == 174013 || //Разбойник - Нигель
                    ActorSNO.Id == 6031 || //Магда
                    ActorSNO.Id == 196102 || //Искату
                    ActorSNO.Id == 4630 || //Диспаир
                    ActorSNO.Id == 148449 || //Изуал
                    ActorSNO.Id == 114917 || //ДЪЯБЛО!!!
                    ActorSNO.Id == 133562 || //Тень Дъябло
                    ActorSNO.Id == 0)
                {

                    return (int)NullD.Common.MPQ.FileFormats.SpawnType.Boss;
                }
                else if (//ActorSNO.Id == 6652 ||
                         ActorSNO.Id == 0
                        )
                {
                    return (int)NullD.Common.MPQ.FileFormats.SpawnType.Rare;
                }
                else if (ActorSNO.Id == 219995 ||  //Моррис Джекйобс
                         ActorSNO.Id == 129439 ||  //Арсект ядовитый
                         ActorSNO.Id == 115403 ||  //Головоруб
                         ActorSNO.Id == 81533 || // Чупа-Хаза
                         ActorSNO.Id == 0)
                {
                    return (int)NullD.Common.MPQ.FileFormats.SpawnType.Unique;
                }
                else if (ActorSNO.Id == 219725 ||  //Проклятая мать
                         ActorSNO.Id == 90367 ||  //Темный пробудитель
                         ActorSNO.Id == 178300 ||  //Берсерки в Вортеме 
                         ActorSNO.Id == 375 ||  //Шаманы клана луны
                         ActorSNO.Id == -2)
                {
                    return (int)NullD.Common.MPQ.FileFormats.SpawnType.Champion;
                }
                else if (ActorSNO.Id == 6644 || // Тонкий зомбарь
                         ActorSNO.Id == 6652 ||// Толстый зомбарь
                         ActorSNO.Id == 0)
                {
                    int Percent = RandomHelper.Next(0, 100);
                    if (Percent > 98)
                        return (int)NullD.Common.MPQ.FileFormats.SpawnType.Champion;
                    else
                        return (int)NullD.Common.MPQ.FileFormats.SpawnType.Normal;
                }
                else
                    return (int)NullD.Common.MPQ.FileFormats.SpawnType.Normal;

            }
            set
            {
                // TODO MonsterQuality setter not implemented. Throwing a NotImplementedError is catched as message not beeing implemented and nothing works anymore...
            }
        }

        public int LoreSNOId
        {
            get
            {
                return Monster.IsValid ? (Monster.Target as MonsterFF).SNOLore : -1;
            }
        }

        /// <summary>
        /// Gets the Actors summoning fields from the mpq's and returns them in format for Monsters.
        /// Useful for the Monsters spawning/summoning skills.
        /// </summary>
        public int[] SNOSummons
        {
            get
            {
                return (Monster.Target as MonsterFF).SNOSummonActor;
            }
        }

        public Monster(World world, int snoId, TagMap tags)
            : base(world, snoId, tags)
        {
            this.Field2 = 0x8;
            this.GBHandle.Type = (int)GBHandleType.Monster; this.GBHandle.GBID = 1;
            this.Attributes[GameAttribute.Experience_Granted] = 125 * Config.Instance.ExpRate;

            if (this.Quality == (int)NullD.Common.MPQ.FileFormats.SpawnType.Boss)
            {
                //Включаем иконку босса.
                this.Attributes[GameAttribute.MinimapActive] = true;
            }
            // lookup GameBalance MonsterLevels.gam asset
            var monsterLevels = (GameBalance)NullD.Common.MPQ.MPQStorage.Data.Assets[SNOGroup.GameBalance][19760].Data;
            var monsterData = (Monster.Target as MonsterFF);

            // always use normal difficulty levels for now
            if (monsterData.Level.Normal >= 0 && monsterData.Level.Normal < monsterLevels.MonsterLevel.Count)
            {
                this.Brain = new MonsterBrain(this);
                this.Attributes[GameAttribute.Level] = monsterData.Level.Normal;
                this.Attributes[GameAttribute.Hitpoints_Max] = monsterLevels.MonsterLevel[monsterData.Level.Normal].F0 * Config.Instance.MonsterHPRate;
                this.Attributes[GameAttribute.Hitpoints_Cur] = this.Attributes[GameAttribute.Hitpoints_Max_Total];
                this.Attributes[GameAttribute.Attacks_Per_Second] = 1.2f;
                this.Attributes[GameAttribute.Damage_Weapon_Min, 0] = 5f;
                this.Attributes[GameAttribute.Damage_Weapon_Delta, 0] = 5f;
                this.WalkSpeed = monsterData.Floats[129];  // TODO: this is probably multiplied by something
            }
        }

        public override void OnTargeted(Player player, TargetMessage message)
        {
        }

        public void Update(int tickCounter)
        {
            if (this.Brain == null)
                return;

            this.Brain.Update(tickCounter);
        }

        /// <summary>
        /// Plays lore for first death of this monster's death.
        /// </summary>
        public void PlayLore()
        {
            if (LoreSNOId != -1)
            {
                var players = this.GetPlayersInRange();
                if (players != null)
                {
                    foreach (var player in players.Where(player => !player.HasLore(LoreSNOId)))
                    {
                        player.PlayLore(LoreSNOId, false);
                    }
                }
            }
        }
    }
}
